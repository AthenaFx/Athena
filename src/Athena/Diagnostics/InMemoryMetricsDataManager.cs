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

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, long>> _frustratedRequests =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, long>>();

        public InMemoryMetricsDataManager(TimeSpan saveDataFor)
        {
            _saveDataFor = saveDataFor;
        }

        public Task ReportMetricsTotalValue(string application, string key, double value, DateTime at)
        {
            return ReportMetricsValue(application, key, value, at, (totalValue, items, startAt) => totalValue / items);
        }

        public Task ReportMetricsPerSecondValue(string application, string key, double value, DateTime at)
        {
            return ReportMetricsValue(application, key, value, at, (totalValue, items, startAt) =>
            {
                var seconds = (DateTime.UtcNow - startAt).Seconds;

                return totalValue / seconds;
            });
        }

        public Task ReportMetricsApdexValue(string application, string key, double value, DateTime at, double tolerable)
        {
            var wasTolerable = value <= tolerable;

            var wasFrustrating = tolerable * 4 <= value;

            if (wasFrustrating)
            {
                _frustratedRequests
                    .GetOrAdd(application, x => new ConcurrentDictionary<string, long>())
                    .AddOrUpdate(key, x => 1, (_, currentValue) => currentValue + 1);
                
                return Task.CompletedTask;
            }
            
            return ReportMetricsValue(application, key, wasTolerable ? 1 : 0, at, (totalValue, items, startAt) =>
            {
                var frustratedRequests = _frustratedRequests
                    .GetOrAdd(application, x => new ConcurrentDictionary<string, long>())
                    .GetOrAdd(key, x => 0);

                return (totalValue + (items - totalValue) / 2) / (items + frustratedRequests);
            });
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
        
        private Task ReportMetricsValue(string application, string key, double value, DateTime at,
            Func<double, long, DateTime, double> calculateAverage)
        {
            var loweredApplication = (application ?? "").ToLower();
            var loweredKey = (key ?? "").ToLower();
            
            var applicationData = _data.GetOrAdd(loweredApplication,
                x => new ConcurrentDictionary<string, MetricsAverage>());

            var removeBefore = DateTime.UtcNow - _saveDataFor;

            applicationData.AddOrUpdate(loweredKey, _ => new MetricsAverage(value, at, calculateAverage),
                (_, item) => item.Add(value, at, removeBefore));

            return Task.CompletedTask;
        }

        private class MetricsAverage
        {
            private readonly Func<double, long, DateTime, double> _calculateAverage;
            private double _totalValue;
            private long _items;
            private DateTime _startAt;

            private readonly ConcurrentDictionary<string, AveragePerMinuteValue> _averagePerMinute =
                new ConcurrentDictionary<string, AveragePerMinuteValue>();

            public MetricsAverage()
            {
                _calculateAverage = (_, __, ___) => 0;
            }

            public MetricsAverage(double value, DateTime at, Func<double, long, DateTime, double> calculateAverage)
            {
                _startAt = at;
                _calculateAverage = calculateAverage;
                Add(value, at);
            }

            public MetricsAverage Add(double value, DateTime at, DateTime? removeBefore = null)
            {
                var itemToAdd = new AveragePerMinuteValue(value, at);

                _averagePerMinute.AddOrUpdate(itemToAdd.GetKey(), _ => itemToAdd, (_, item) => item.Merge(itemToAdd));

                _totalValue += value;
                _items += 1;

                _startAt = at < _startAt ? at : _startAt;

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
                return _calculateAverage(_totalValue, _items, _startAt);
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