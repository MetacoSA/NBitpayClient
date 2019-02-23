using System;
using Newtonsoft.Json;

namespace NBitpayClient
{
    public class RefundAddress
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTimeOffset Date { get; set; }
    }
}
