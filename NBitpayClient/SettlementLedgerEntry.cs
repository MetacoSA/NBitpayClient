using Newtonsoft.Json;
using System;

namespace NBitpayClient
{
    public class SettlementLedgerEntry
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "invoiceId")]
        public string InvoiceId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "invoiceData")]
        public InvoiceData InvoiceData { get; set; }
    }
}
