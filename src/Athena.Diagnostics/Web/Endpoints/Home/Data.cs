using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Diagnostics.Web.Endpoints.Home
{
    public class Data
    {
        public async Task<DataGetResult> Get(DataGetInput input, AthenaContext context)
        {
            var data = await context.GetSetting<DiagnosticsConfiguration>()
                .GetDiagnosticsDataManager()
                .GetDataFor(input.Slug, input.Id, input.Step)
                .ConfigureAwait(false);
            
            return new DataGetResult(input.Slug, input.Id, input.Step, 
                data.ToDictionary(x => x.Key, x => x.Value.Select(y => 
                    new KeyValuePair<string, string>(y.Key, y.Value))));
        }
    }

    public class DataGetInput
    {
        public string Slug { get; set; }
        public string Id { get; set; }
        public string Step { get; set; }
    }

    public class DataGetResult
    {
        public DataGetResult(string application, string type, string step, 
            IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>> data)
        {
            Application = application;
            Type = type;
            Step = step;
            Data = data;
        }

        public string Application { get; }
        public string Type { get; }
        public string Step { get; }
        public IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>> Data { get; }
        
        public override string ToString()
        {
            var dataContentBuilder = new StringBuilder();

            foreach (var item in Data)
            {
                var itemContentBuilder = new StringBuilder();

                foreach (var data in item.Value)
                {
                    itemContentBuilder.Append($@"<li>
                                                    <strong>{data.Key}</strong>: {data.Value}
                                                </li>");
                }
                
                dataContentBuilder.Append($@"<div>
                                                <h2>{item.Key}</h2>
                                                <ul>
                                                    {itemContentBuilder}
                                                </ul>
                                            </div>");
            }
            
            return $@"<!DOCTYPE html>
                    <html>
                        <head>
                            <title>{Application} - {Type} - {Step}</title>
                        </head>
                        <body>
                            <h1>{Application} - {Type} - {Step}</h1>
                            {dataContentBuilder}
                        </body>
                    </html>";
        }
    }
}