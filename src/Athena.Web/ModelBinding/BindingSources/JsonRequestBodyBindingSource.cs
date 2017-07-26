using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Logging;
using Newtonsoft.Json.Linq;

namespace Athena.Web.ModelBinding.BindingSources
{
    public class JsonRequestBodyBindingSource : BindingSource
    {
        public async Task<IReadOnlyDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            try
            {
                var json = await envinronment.GetRequest().ReadBodyAsString().ConfigureAwait(false);
                
                var outer = JObject.Parse(json);

                var result = new Dictionary<string, object>();

                SetEdgeValues(outer, "", result);

                return result.ToDictionary(x => x.Key.ToLower(), x => x.Value);
            }
            catch (Exception ex)
            {
                Logger.Write(LogLevel.Debug, "Unable to parse body as json.", ex);
                
                return new Dictionary<string, object>();
            }
        }

        private static void SetEdgeValues(JObject outer, string prefix, IDictionary<string, object> data)
        {
            foreach (var inner in outer.Properties())
            {
                switch (inner.Value.Type)
                {
                    case JTokenType.Object:
                        SetEdgeValues((JObject)inner.Value, $"{prefix}{inner.Name}_", data);
                        break;
                    case JTokenType.Array:
                        HandleArray((JArray)inner.Value, $"{prefix}{inner.Name}", data);
                        break;
                    default:
                        data[$"{prefix}{inner.Name}"] = inner.Value.ToObject<object>();
                        break;
                }
            }
        }

        private static void HandleArray(JArray array, string prefix, IDictionary<string, object> data)
        {
            var index = 0;
            foreach (var child in array)
            {
                switch (child.Type)
                {
                    case JTokenType.Object:
                        SetEdgeValues((JObject)child, $"{prefix}[{index}]_", data);
                        break;
                    case JTokenType.Array:
                        HandleArray((JArray)child, $"{prefix}[{index}]", data);
                        break;
                    case JTokenType.Property:
                        data[$"{prefix}[{index}]_{((JProperty) child).Name}"] = child.ToObject<object>();
                        break;
                    default:
                        data[$"{prefix}[{index}]_"] = child.ToObject<object>();
                        break;
                }

                index++;
            }
        }
    }
}