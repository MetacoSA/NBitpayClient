using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBitpayClient
{
    /// <summary>
    /// Invoice payment URLs identified by BIP format.
    /// </summary>
    public class InvoicePaymentUrls {

        public InvoicePaymentUrls() {}

		[JsonProperty("BIP21")]
        public string BIP21 { get; set; }
		[JsonProperty("BIP72")]
		public string BIP72 { get; set; }
		[JsonProperty("BIP72b")]
		public string BIP72b { get; set; }
		[JsonProperty("BIP73")]
		public string BIP73 { get; set; }
	}
}
