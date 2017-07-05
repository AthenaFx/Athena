using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Athena.Diagnostics;

namespace Athena.Web.Diagnostics.Endpoints.Home
{
    public class Details
    {
        public async Task<DetailsGetResult> Get(DetailsGetInput input, AthenaContext context, 
            IDictionary<string, object> environment)
        {
            var diagnosticsSettings = context.GetSetting<DiagnosticsConfiguration>();
            
            var types = await diagnosticsSettings
                .DataManager
                .GetTypesFor(input.Slug)
                .ConfigureAwait(false);

            var metricKeys = await diagnosticsSettings
                .MetricsManager
                .GetKeys(input.Slug)
                .ConfigureAwait(false);

            var settings = environment.GetCurrentWebApplicationSettings();

            return new DetailsGetResult(input.Slug, types, metricKeys, settings.BaseUrl);
        }
    }

    public class DetailsGetInput
    {
        public string Slug { get; set; }
    }

    public class DetailsGetResult
    {
        private readonly string _baseUrl;
        
        public DetailsGetResult(string application, IEnumerable<string> types, IEnumerable<string> metricKeys, string baseUrl)
        {
            Application = application;
            Types = types;
            _baseUrl = baseUrl;
            MetricKeys = metricKeys;
        }

        public string Application { get; }
        public IEnumerable<string> Types { get; }
        public IEnumerable<string> MetricKeys { get; }

        public override string ToString()
        {
            var typesContentBuilder = new StringBuilder();

            foreach (var type in Types)
            {
                typesContentBuilder.Append($@"<li>
                                                <a href=""/{_baseUrl}/{Application}/type/{type}"">
                                                    {type}
                                                </a>
                                            </li>");
            }
            
            var metricKeysBuilder = new StringBuilder();

            foreach (var metricKey in MetricKeys)
            {
                metricKeysBuilder.Append($@"<li>
                                                <a href=""/{_baseUrl}/{Application}/metrics/{metricKey}"">
                                                    {metricKey}
                                                </a>
                                            </li>");
            }
            
            return $@"<!DOCTYPE html>
                    <html>
                        <head>
                            <title>{Application}</title>
                        </head>
                        <body>
                            <h1>{Application}</h1>
                            <h2>Types</h2>
                            <ul>
                                {typesContentBuilder}
                            </ul>
                            <h2>Metrics</h2>
                            <ul>
                                {metricKeysBuilder}
                            </ul>
                        </body>
                    </html>";
        }
    }
}