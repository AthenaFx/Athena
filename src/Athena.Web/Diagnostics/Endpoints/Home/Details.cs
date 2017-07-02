using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Athena.Diagnostics;

namespace Athena.Web.Diagnostics.Endpoints.Home
{
    public class Details
    {
        public async Task<DetailsGetResult> Get(DetailsGetInput input, AthenaContext context)
        {
            var types = await context
                .GetSetting<DiagnosticsConfiguration>()
                .DataManager
                .GetTypesFor(input.Slug)
                .ConfigureAwait(false);

            var settings = context.GetSetting<DiagnosticsWebApplicationSettings>();

            return new DetailsGetResult(input.Slug, types, settings.BaseUrl);
        }
    }

    public class DetailsGetInput
    {
        public string Slug { get; set; }
    }

    public class DetailsGetResult
    {
        private readonly string _baseUrl;
        
        public DetailsGetResult(string application, IEnumerable<string> types, string baseUrl)
        {
            Application = application;
            Types = types;
            _baseUrl = baseUrl;
        }

        public string Application { get; }
        public IEnumerable<string> Types { get; }

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
            
            return $@"<!DOCTYPE html>
                    <html>
                        <head>
                            <title>{Application}</title>
                        </head>
                        <body>
                            <h1>{Application}</h1>
                            <ul>
                                {typesContentBuilder}
                            </ul>
                        </body>
                    </html>";
        }
    }
}