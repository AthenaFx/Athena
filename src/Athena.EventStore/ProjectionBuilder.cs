using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Athena.EventStore
{
    public static class ProjectionBuilder
    {
        private const string FromStreamsTemplate = @"fromStreams([[[FromStreams]]]) 
    .when({          
        $any: function (s, e){
            linkTo('[[ToStream]]', e);
        }
    })";

        public static string BuildStreamProjection(IEnumerable<string> fromStreams, string toStream)
        {
            return FromStreamsTemplate
                .Replace("[[FromStreams]]", string.Join(", ", fromStreams.Select(x => $"'{x}'")))
                .Replace("[[ToStream]]", toStream);
        }

        public static string BuildStreamProjection(IEnumerable<string> fromStreams, string toStream, 
            IEnumerable<EventMap> eventMaps)
        {
            var projectionBuilder = new StringBuilder();

            projectionBuilder.AppendLine(
                $"fromStreams([{string.Join(", ", fromStreams.Select(x => $"'{x}'"))}])");
            projectionBuilder.AppendLine(".when({");

            foreach (var eventMap in eventMaps)
            {
                projectionBuilder.AppendLine($"{eventMap.EventName}: function(s, e){{");

                projectionBuilder.AppendLine(string.IsNullOrEmpty(eventMap.PartitionKeyLocation)
                    ? $"linkTo('{toStream}', e);"
                    : string.Format("linkTo('{0}', e, {2}'PartitionKey': 'e.data.{1}'{3});", toStream, eventMap.PartitionKeyLocation, "{", "}"));

                projectionBuilder.AppendLine("},");
            }

            projectionBuilder.AppendLine("})");

            return projectionBuilder.ToString();
        }

        public class EventMap
        {
            public EventMap(string eventName, string partitionKeyLocation = null)
            {
                EventName = eventName;
                PartitionKeyLocation = partitionKeyLocation;
            }

            public string EventName { get; }
            public string PartitionKeyLocation { get; }
        }
    }
}