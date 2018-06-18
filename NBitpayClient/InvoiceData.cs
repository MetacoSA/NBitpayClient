using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBitpayClient
{
    public class InvoiceData
    {
        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "overpaidAmount")]
        public decimal OverPaidAmount { get; set; }

        [JsonProperty(PropertyName = "payoutPercentage")]
        public Dictionary<string, double> PayoutPercentage { get; set; }

        [JsonProperty(PropertyName = "btcPrice")]
        public decimal BtcPrice { get; set; }

        [JsonProperty(PropertyName = "refundInfo")]
        public RefundInfo RefundInfo { get; set; }
    }
}
