using Newtonsoft.Json;

namespace NBitpayClient
{
    public class Withholdings
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "notes")]
        public string Notes { get; set; }
    }
}
