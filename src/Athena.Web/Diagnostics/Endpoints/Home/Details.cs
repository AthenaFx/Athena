using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Athena.Diagnostics;

namespace Athena.Web.Diagnostics.Endpoints.Home
{
    public class Details
    {
        public async Task<DetailsGetResult> Get(DetailsGetInput input)
        {
            var types = await ApplicationDiagnostics.DataManager.GetTypesFor(input.Slug).ConfigureAwait(false);

            return new DetailsGetResult(input.Slug, types);
        }
    }

    public class DetailsGetInput
    {
        public string Slug { get; set; }
    }

    public class DetailsGetResult
    {
        public DetailsGetResult(string application, IEnumerable<string> types)
        {
            Application = application;
            Types = types;
        }

        public string Application { get; }
        public IEnumerable<string> Types { get; }

        public override string ToString()
        {
            var typesContentBuilder = new StringBuilder();

            foreach (var type in Types)
            {
                typesContentBuilder.Append($@"<li>
                                                <a href=""/{WebDiagnostics.BaseUrl}/{Application}/type/{type}"">
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