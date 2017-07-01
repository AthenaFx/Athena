using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Athena.Diagnostics;

namespace Athena.Web.Diagnostics.Endpoints.Home
{
    public class Type
    {
        public async Task<TypeGetResult> Get(TypeGetInput input, AthenaContext context)
        {
            var steps = await context
                .GetSetting<DiagnosticsConfiguration>()
                .DataManager
                .GetStepsFor(input.Slug, input.Id)
                .ConfigureAwait(false);
            
            return new TypeGetResult(input.Slug, input.Id, steps);
        }
    }

    public class TypeGetInput
    {
        public string Slug { get; set; }
        public string Id { get; set; }
    }

    public class TypeGetResult
    {
        public TypeGetResult(string application, string type, IEnumerable<string> steps)
        {
            Application = application;
            Type = type;
            Steps = steps;
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
                                                <a href=""/{WebDiagnostics.BaseUrl}/{Application}/data/{Type}/{step}"">
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