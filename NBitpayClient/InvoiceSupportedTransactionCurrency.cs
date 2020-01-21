using Newtonsoft.Json;

namespace NBitpayClient
{
    public class InvoiceSupportedTransactionCurrency
    {
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; set; }
    }
}