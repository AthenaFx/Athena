using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Athena.Web;

namespace Athena.Diagnostics.Web.Endpoints.Home
{
    public class Type
    {
        public async Task<TypeGetResult> Get(TypeGetInput input, AthenaContext context,
            IDictionary<string, object> environment)
        {
            var steps = await context
                .GetSetting<DiagnosticsConfiguration>()
                .GetDiagnosticsDataManager()
                .GetStepsFor(input.Slug, input.Id)
                .ConfigureAwait(false);
            
            var settings = environment.GetCurrentWebApplicationSettings();
            
            return new TypeGetResult(input.Slug, input.Id, steps, settings.BaseUrl);
        }
    }

    public class TypeGetInput
    {
        public string Slug { get; set; }
        public string Id { get; set; }
    }

    public class TypeGetResult
    {
        private readonly string _baseUrl;
        
        public TypeGetResult(string application, string type, IEnumerable<string> steps, string baseUrl)
        {
            Application = application;
            Type = type;
            Steps = steps;
            _baseUrl = baseUrl;
        }

        public string Application { get; }
        public string Type { get; }
        public IEnumerable<string> Steps { get; }
        
        public override string ToString()
        {
            var stepsContentBuilder = new StringBuilder();

            foreach (var step in Steps)
            {
                stepsContentBuilder.Append($@"<li>
                                                <a href=""/{_baseUrl}/{Application}/data/{Type}/{step}"">
                                                    {step}
                                                </a>
                                            </li>");
            }
            
            return $@"<!DOCTYPE html>
                    <html>
                        <head>
                            <title>{Application} - {Type}</title>
                        </head>
                        <body>
                            <h1>{Application} - {Type}</h1>
                            <ul>
                                {stepsContentBuilder}
                            </ul>
                        </body>
                    </html>";
        }
    }
}