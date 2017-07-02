using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Athena.Diagnostics;

namespace Athena.Web.Diagnostics.Endpoints.Home
{
    public class Index
    {
        public async Task<IndexGetResult> Get(AthenaContext context)
        {
            var applications = await context
                .GetSetting<DiagnosticsConfiguration>()
                .DataManager
                .GetApplications()
                .ConfigureAwait(false);
            
            var settings = context.GetSetting<DiagnosticsWebApplicationSettings>();
            
            return new IndexGetResult(applications, settings.BaseUrl);
        }
    }

    public class IndexGetResult
    {
        private readonly string _baseUrl;
        
        public IndexGetResult(IEnumerable<string> applications, string baseUrl)
        {
            Applications = applications;
            _baseUrl = baseUrl;
        }

        public IEnumerable<string> Applications { get; }

        public override string ToString()
        {
            var applicationsContentBuilder = new StringBuilder();

            foreach (var application in Applications)
            {
                applicationsContentBuilder.Append($@"<li>
                                                        <a href=""/{_baseUrl}/{application}"">
                                                            {application}
                                                        </a>
                                                    </li>");
            }
            
            return $@"<!DOCTYPE html>
                    <html>
                        <head>
                            <title>Diagnostics</title>
                        </head>
                        <body>
                            <h1>Diagnostics</h1>
                            <ul>
                                {applicationsContentBuilder}
                            </ul>
                        </body>
                    </html>";
        }
    }
}