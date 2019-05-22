using System;
using System.Collections.Generic;

namespace Smee.IO.Client.Dto
{
    public class SmeeData
    {
        public SmeeData()
        {
            // Can be used for tests.
        }

        internal SmeeData(Models.SmeeData smeeData)
        {
            Headers = smeeData.Headers;
            QueryString = smeeData.QueryString;
            Body = smeeData.Body;
            Timestamp = smeeData.Timestamp;
        }

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        public Dictionary<string, string> QueryString = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        public object Body { get; internal set; }
        public long Timestamp { get; internal set; }
    }
}
