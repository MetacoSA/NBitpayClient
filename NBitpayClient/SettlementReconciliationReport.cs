using Newtonsoft.Json;
using System.Collections.Generic;

namespace NBitpayClient
{
    public class SettlementReconciliationReport : Settlement
    {
        [JsonProperty(PropertyName = "ledgerEntries")]
        public List<SettlementLedgerEntry> LedgerEntries { get; set; }
    }
}
