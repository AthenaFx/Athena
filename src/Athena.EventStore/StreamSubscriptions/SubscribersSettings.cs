using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Athena.EventStore.StreamSubscriptions
{
    public class SubscribersSettings
    {
        private readonly ICollection<Tuple<string, int, bool>> _streams = new List<Tuple<string, int, bool>>();

        public SubscribersSettings SubscribeToStream(string stream, int workers = 1, bool liveOnly = false)
        {
            _streams.Add(new Tuple<string, int, bool>(stream, workers, liveOnly));

            return this;
        }

        public IReadOnlyCollection<Tuple<string, int, bool>> GetSubscribedStreams()
        {
            return new ReadOnlyCollection<Tuple<string, int, bool>>(_streams.ToList());
        }
    }
}