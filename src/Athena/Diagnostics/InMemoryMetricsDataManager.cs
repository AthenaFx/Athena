using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    public class InMemoryMetricsDataManager : MetricsDataManager
    {
        private readonly TimeSpan _saveDataFor;

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, MetricsAverage>> _data =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, MetricsAverage>>();

        public InMemoryMetricsDataManager(TimeSpan saveDataFor)
        {
            _saveDataFor = saveDataFor;
        }

        public Task ReportMetricsValue(string application, string key, double value, DateTime at)
        {
            var loweredApplication = (application ?? "").ToLower();
            var loweredKey = (key ?? "").ToLower();
            
            var applicationData = _data.GetOrAdd(loweredApplication,
                    x => new ConcurrentDictionary<string, MetricsAverage>());

            var removeBefore = DateTime.UtcNow - _saveDataFor;

            applicationData.AddOrUpdate(loweredKey, _ => new MetricsAverage(value, at),
                (_, item) => item.Add(value, at, removeBefore));

            return Task.CompletedTask;
        }

        public Task<double> GetAverageFor(string application, string key)
        {
            var loweredApplication = (application ?? "").ToLower();
            var loweredKey = (key ?? "").ToLower();
            
            var applicationData = _data.GetOrAdd(loweredApplication,
                x => new ConcurrentDictionary<string, MetricsAverage>());

            var data = applicationData.GetOrAdd(loweredKey, _ => new MetricsAverage());

            return Task.FromResult(data.GetAverage());
        }

        public Task<IReadOnlyCollection<string>> GetKeys(string application)
        {
            var loweredApplication = (application ?? "").ToLower();
            
            var applicationData = _data.GetOrAdd(loweredApplication,
                x => new ConcurrentDictionary<string, MetricsAverage>());

            return Task.FromResult<IReadOnlyCollection<string>>(applicationData.Keys.ToList());
        }

        private class MetricsAverage
        {
            private double _totalValue;
            private long _items;

            private readonly ConcurrentDictionary<string, AveragePerMinuteValue> _averagePerMinute =
                new ConcurrentDictionary<string, AveragePerMinuteValue>();

            public MetricsAverage()
            {
                
            }

            public MetricsAverage(double value, DateTime at)
            {
                Add(value, at);
            }

            public MetricsAverage Add(double value, DateTime at, DateTime? removeBefore = null)
            {
                var itemToAdd = new AveragePerMinuteValue(value, at);

                _averagePerMinute.AddOrUpdate(itemToAdd.GetKey(), _ => itemToAdd, (_, item) => item.Merge(itemToAdd));

                _totalValue += value;
                _items += 1;

                if (!removeBefore.HasValue) 
                    return this;
                
                var oldData = _averagePerMinute
                    .Where(x => x.Value.Start < removeBefore.Value);

                foreach (var item in oldData)
                {
                    AveragePerMinuteValue removedItem;
                    _averagePerMinute.TryRemove(item.Key, out removedItem);
                }

                return this;
            }

            public double GetAverage()
            {
                return _totalValue / _items;
            }
        }

        private class AveragePerMinuteValue
        {
            private readonly double _totalValue;
            private readonly long _items;

            public AveragePerMinuteValue(double value, DateTime at)
            {
                _totalValue = value;
                _items = 1;

                Start = new DateTime(at.Year, at.Month, at.Day, at.Hour, at.Minute, 0);
            }

            private AveragePerMinuteValue(DateTime start, double totalValue, long items)
            {
                Start = start;
                _totalValue = totalValue;
                _items = items;
            }

            public DateTime Start { get; }
            public TimeSpan Length = TimeSpan.FromMinutes(1);

            public double GetAverage()
            {
                return _totalValue / _items;
            }

            public AveragePerMinuteValue Merge(AveragePerMinuteValue with)
            {
                return new AveragePerMinuteValue(Start, _totalValue + with._totalValue, _items + with._items);
            }

            public string GetKey()
            {
                return Start.ToString($"{Start.Year}-{Start.Month}-{Start.Day}-{Start.Hour}-{Start.Minute}");
            }
        }
    }
}