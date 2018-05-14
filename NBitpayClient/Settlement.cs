using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBitpayClient
{
    public class Settlement
    {
        public Settlement() { }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "payoutInfo")]
        public PayoutInfo PayoutInfo { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(PropertyName = "dateExecuted")]
        public DateTime DateExecuted { get; set; }

        [JsonProperty(PropertyName = "dateCompleted")]
        public DateTime DateCompleted { get; set; }

        [JsonProperty(PropertyName = "openingDate")]
        public DateTime OpeningDate { get; set; }

        [JsonProperty(PropertyName = "closingDate")]
        public DateTime ClosingDate { get; set; }

        [JsonProperty(PropertyName = "openingBalance")]
        public decimal OpeningBalance { get; set; }

        [JsonProperty(PropertyName = "ledgerEntriesSum")]
        public decimal LedgerEntriesSum { get; set; }

        [JsonProperty(PropertyName = "withholdings")]
        public List<Withholdings> Withholdings { get; set; }

        [JsonProperty(PropertyName = "withholdingsSum")]
        public decimal WithHoldingsSum { get; set; }

        [JsonProperty(PropertyName = "totalAmount")]
        public decimal TotalAmount { get; set; }
    }
}
