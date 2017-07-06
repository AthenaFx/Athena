using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.EventStore.ProcessManagers;
using Athena.EventStore.Serialization;
using Athena.Logging;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;

namespace Athena.EventStore
{
    public static class EventStoreConnectionExtensions
    {
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;
        
        public static async Task<T> Load<T>(this IEventStoreConnection connection, string id, 
            EventSerializer serializer) 
            where T : EventSourcedEntity, new()
        {
            var entity = new T
            {
                Id = id
            };

            var streamName = entity.GetStreamName();

            var events = await LoadEventsFromStream(connection, streamName, 0, int.MaxValue).ConfigureAwait(false);

            entity.BuildFromHistory(events
                .Select(x => new Event(x.Event.EventId, serializer.DeSerialize(x).Data))
                .ToList());
            
            return entity;
        }
        
        public static async Task Save(this IEventStoreConnection connection, EventSourcedEntity entity, 
            IDictionary<string, object> metaData, EventSerializer serializer)
        {
            var streamName = entity.GetStreamName();
            var eventStream = entity.GetUncommittedChanges();
            var originalVersion = entity.Version - eventStream.Count;

            var versionToExpect = originalVersion == 0 ? ExpectedVersion.Any : originalVersion - 1;

            while (true)
            {
                try
                {
                    var eventsToSave = eventStream.Select(e => serializer.Serialize(e.Id, e.Instance, metaData))
                        .ToList();

                    if (!eventsToSave.Any())
                        return;

                    if (eventsToSave.Count < WritePageSize)
                    {
                        await connection.AppendToStreamAsync(streamName, versionToExpect, eventsToSave
                            .Select(x => new EventData(x.EventId, x.Type, true, x.Data, x.Metadata)))
                            .ConfigureAwait(false);

                        Logger.Write(LogLevel.Debug, $"Saved {eventsToSave.Count} events to stream {streamName}.");
                    }
                    else
                    {
                        var transaction = await connection.StartTransactionAsync(streamName, versionToExpect)
                            .ConfigureAwait(false);

                        var position = 0;
                        while (position < eventsToSave.Count)
                        {
                            var pageEvents = eventsToSave.Skip(position).Take(WritePageSize)
                                .Select(x => new EventData(x.EventId, x.Type, true, x.Data, x.Metadata)).ToList();
                            await transaction.WriteAsync(pageEvents).ConfigureAwait(false);
                            Logger.Write(LogLevel.Debug, $"Saved {pageEvents.Count} events to stream {streamName}.");
                            
                            position += WritePageSize;
                        }

                        await transaction.CommitAsync().ConfigureAwait(false);
                    }

                    break;
                }
                catch (WrongExpectedVersionException ex)
                {
                    Logger.Write(LogLevel.Warn,
                        $"Events where added to aggregate with id: {entity.Id} since last load. Checking for conflicts and trying again...",
                        ex);
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is WrongExpectedVersionException))
                        throw;

                    Logger.Write(LogLevel.Warn,
                        $"Events where added to aggregate with id: {entity.Id} since last load. Checking for conflicts and trying again...",
                        ae.InnerException);
                }

                var storedEvents =
                (await LoadEventsFromStream(connection, streamName, versionToExpect < 0 ? 0 : versionToExpect,
                    long.MaxValue).ConfigureAwait(false)).ToList();

                var currentVersion = storedEvents.Select(x => x.OriginalEventNumber).OrderByDescending(x => x).FirstOrDefault();

                versionToExpect = currentVersion;
            }

            entity.ClearUncommittedChanges();
        }

        
        public static async Task<IEnumerable<ResolvedEvent>> LoadEventsFromStream(this IEventStoreConnection connection, 
            string streamName, long from, long to)
        {
            var sliceStart = from;
            StreamEventsSlice currentSlice;
            var result = new List<ResolvedEvent>();

            do
            {
                var sliceCount = sliceStart + ReadPageSize <= to
                    ? ReadPageSize
                    : to - sliceStart;

                if (sliceCount == 0)
                    break;

                currentSlice = await connection
                    .ReadStreamEventsForwardAsync(streamName, sliceStart, (int)sliceCount, false)
                    .ConfigureAwait(false);

                if (currentSlice.Status == SliceReadStatus.StreamDeleted)
                    throw new StreamDeletedException(streamName);

                sliceStart = currentSlice.NextEventNumber;

                result.AddRange(currentSlice.Events);
            } while (to >= currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            return result;
        }
    }
}