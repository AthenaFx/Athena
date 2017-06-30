using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Athena.Diagnostics;

namespace Athena.Web.Diagnostics.Endpoints.Home
{
    public class Index
    {
        public async Task<IndexGetResult> Get()
        {
            var applications = await ApplicationDiagnostics
                .DataManager
                .GetApplications()
                .ConfigureAwait(false);
            
            return new IndexGetResult(applications);
        }
    }

    public class IndexGetResult
    {
        public IndexGetResult(IEnumerable<string> applications)
        {
            Applications = applications;
        }

        public IEnumerable<string> Applications { get; }

        public override string ToString()
        {
            var applicationsContentBuilder = new StringBuilder();

            foreach (var application in Applications)
            {
                applicationsContentBuilder.Append($@"<li>
                                                        <a href=""/{WebDiagnostics.BaseUrl}/{application}"">
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