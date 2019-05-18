using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Smee.IO.Client
{
  public class SmeeData
    {
        [JsonProperty("content-type")] public string ContentType { get; internal set; }
        [JsonProperty("cache-control")] public string CacheControl { get; internal set; }
        [JsonProperty("user-agent")] public string UserAgent { get; internal set; }
        [JsonProperty("accept")] public string Accept { get; internal set; }
        [JsonProperty("host")] public string Host { get; internal set; }
        [JsonProperty("accept-encoding")] public string AcceptEncoding { get; internal set; }
        [JsonProperty("content-length")] public string ContentLength { get; internal set; }
        [JsonProperty("connection")] public string Connection { get; internal set; }
        [JsonProperty("body")] public object Body { get; internal set; }
        [JsonProperty("query")] public Dictionary<string, object> Query { get; internal set; }
        [JsonProperty("timestamp")] public long Timestamp { get; internal set; }

        private Dictionary<string, string> _queryString;
        public Dictionary<string, string> QueryString
        {
            get
            {
                return _queryString ?? (_queryString = Query.ToDictionary(
                           f => f.Key,
                           g => JsonConvert.SerializeObject(g.Value)
                       ));
            }
        }
    }
}
