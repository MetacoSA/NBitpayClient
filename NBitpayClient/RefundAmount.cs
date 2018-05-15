using Newtonsoft.Json;

namespace NBitpayClient
{
    public class RefundAmount
    {
        [JsonProperty(PropertyName = "btc")]
        public decimal Btc { get; set; }

        [JsonProperty(PropertyName = "usd")]
        public decimal Usd { get; set; }
    }
}
