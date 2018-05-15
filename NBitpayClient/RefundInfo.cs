using Newtonsoft.Json;

namespace NBitpayClient
{
    public class RefundInfo
    {
        [JsonProperty(PropertyName = "supportRequest")]
        public string SupportRequest { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amounts")]
        public RefundAmount Amounts { get; set; }
    }
}
