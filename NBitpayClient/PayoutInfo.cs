using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBitpayClient
{
    public class PayoutInfo
    {
        public PayoutInfo() { }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        
        [JsonProperty(PropertyName = "account")]
        public string Account { get; set; }

        [JsonProperty(PropertyName = "routing")]
        public string Routing { get; set; }

        [JsonProperty(PropertyName = "merchantEIN")]
        public string MerchantEIN { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "bankCountry")]
        public string BankCountry { get; set; }
    }
}
