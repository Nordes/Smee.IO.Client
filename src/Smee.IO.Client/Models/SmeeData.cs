using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Smee.IO.Client.Models
{
    [JsonConverter(typeof(SmeeDataConverter))]
    internal class SmeeData
    {
        [JsonIgnore] public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        [JsonProperty("query")] internal Dictionary<string, object> Query { get; set; }

        private Dictionary<string, string> _queryString;
        public Dictionary<string, string> QueryString
        {
            get
            {
                return _queryString ?? (_queryString = Query.ToDictionary(
                           f => f.Key,
                           g => g.Value.ToString()
                       ));
            }
        }
        [JsonProperty("body")] public object Body { get; internal set; }
        [JsonProperty("timestamp")] public long Timestamp { get; internal set; }
    }

    internal class SmeeDataConverter : JsonConverter<SmeeData>
    {
        private static readonly List<string> SmeeDataFixedAttributes = new List<string>
            { "body", "query", "timestamp" };

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, SmeeData value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override SmeeData ReadJson(
            JsonReader reader,
            Type objectType,
            SmeeData existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            if (existingValue == null)
            {
                existingValue = new SmeeData();
            }

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), existingValue);

            if (jObject.HasValues && jObject.Count > 0)
            {
                foreach (var token in jObject.AsJEnumerable())
                {
                    if (!SmeeDataFixedAttributes.Contains(token.Path, StringComparer.OrdinalIgnoreCase))
                    {
                        existingValue.Headers.Add(token.Path, token.First.ToString());
                    }
                }
            }

            return existingValue;
        }
    }
}
