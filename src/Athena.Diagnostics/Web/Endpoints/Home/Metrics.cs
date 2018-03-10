using System.Threading.Tasks;

namespace Athena.Diagnostics.Web.Endpoints.Home
{
    public class Metrics
    {
        public async Task<MetricsGetResult> Get(MetricsGetInput input, AthenaContext context)
        {
            var diagnosticsSettings = context.GetSetting<DiagnosticsConfiguration>();

            var average = await diagnosticsSettings
                .GetMetricsDataManager()
                .GetAverageFor(input.Slug, input.Id)
                .ConfigureAwait(false);
            
            return new MetricsGetResult(input.Slug, input.Id, average);
        }
    }

    public class MetricsGetInput
    {
        public string Slug { get; set; }
        public string Id { get; set; }
    }

    public class MetricsGetResult
    {
        public MetricsGetResult(string application, string key, double averageValue)
        {
            Application = application;
            Key = key;
            AverageValue = averageValue;
        }

        public string Application { get; }
        public string Key { get; }
        public double AverageValue { get; }

        public override string ToString()
        {
            return $@"<!DOCTYPE html>
                    <html>
                        <head>
                            <title>{Application} - {Key}</title>
                        </head>
                        <body>
                            <h1>{Application} - {Key}</h1>
                            {AverageValue}
                        </body>
                    </html>";
        }
    }
}