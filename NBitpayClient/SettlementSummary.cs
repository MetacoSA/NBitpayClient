using Newtonsoft.Json;

namespace NBitpayClient
{
    public class SettlementSummary : Settlement
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}
