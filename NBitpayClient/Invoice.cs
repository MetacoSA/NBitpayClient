using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using NBitcoin;
using NBitpayClient.JsonConverters;
using Newtonsoft.Json.Linq;

namespace NBitpayClient
{
	public class MinerFeeInfo
	{
		[JsonProperty("satoshisPerByte")]
		public decimal SatoshiPerBytes
		{
			get; set;
		}
		[JsonProperty("totalFee")]
		public long TotalFee
		{
			get; set;
		}
	}

	public class InvoiceCryptoInfo
	{
		[JsonProperty("cryptoCode")]
		public string CryptoCode
		{
			get; set;
		}

		[JsonProperty("paymentType")]
		public string PaymentType
		{
			get; set;
		}

		[JsonProperty("rate")]
		public decimal Rate
		{
			get; set;
		}

		//"exRates":{"USD":4320.02}
		[JsonProperty("exRates")]
		public Dictionary<string, decimal> ExRates
		{
			get; set;
		}

		//"btcPaid":"0.000000"
		[JsonProperty("paid")]
		public string Paid
		{
			get; set;
		}

		//"btcPrice":"0.001157"
		[JsonProperty("price")]
		public string Price
		{
			get; set;
		}

		//"btcDue":"0.001160"
		/// <summary>
		/// Amount of crypto remaining to pay this invoice
		/// </summary>
		[JsonProperty("due")]
		public string Due
		{
			get; set;
		}

		[JsonProperty("paymentUrls")]
		public NBitpayClient.InvoicePaymentUrls PaymentUrls
		{
			get; set;
		}

		[JsonProperty("address")]
		public string Address
		{
			get; set;
		}
		[JsonProperty("url")]
		public string Url
		{
			get; set;
		}

		/// <summary>
		/// Total amount of this invoice
		/// </summary>
		[JsonProperty("totalDue")]
		public string TotalDue
		{
			get; set;
		}

		/// <summary>
		/// Total amount of network fee to pay to the invoice
		/// </summary>
		[JsonProperty("networkFee")]
		public string NetworkFee
		{
			get; set;
		}

		/// <summary>
		/// Number of transactions required to pay
		/// </summary>
		[JsonProperty("txCount")]
		public int TxCount
		{
			get; set;
		}

		/// <summary>
		/// Total amount of the invoice paid in this crypto
		/// </summary>
		[JsonProperty("cryptoPaid")]
		public string CryptoPaid
		{
			get; set;
		}
	}

	public class Invoice
	{
		public const String STATUS_NEW = "new";
		public const String STATUS_PAID = "paid";
		public const String STATUS_CONFIRMED = "confirmed";
		public const String STATUS_COMPLETE = "complete";
		public const String STATUS_INVALID = "invalid";
		public const String EXSTATUS_FALSE = "false";
		public const String EXSTATUS_PAID_OVER = "paidOver";
		public const String EXSTATUS_PAID_PARTIAL = "paidPartial";

		/// <summary>
		/// Creates an uninitialized invoice request object.
		/// </summary>
		public Invoice() { }

		// Creates a minimal inovice request object.
		public Invoice(decimal price, String currency)
		{
			Price = price;
			Currency = currency;
		}

		// API fields
		//

		[JsonProperty(PropertyName = "buyer")]
		public Buyer Buyer { get; set; }

		[JsonProperty(PropertyName = "guid")]
		public string Guid { get; set; }

		[JsonProperty(PropertyName = "nonce")]
		public long Nonce { get; set; }
		public virtual bool ShouldSerializeNonce() { return Nonce != 0; }

		[JsonProperty(PropertyName = "token")]
		public string Token { get; set; }

		// Required fields
		//

		[JsonProperty(PropertyName = "price")]
		public decimal Price { get; set; }

        /// <summary>
        /// How much tax are included in the price. Only supported by BTCPay Server.
        /// </summary>
        [JsonProperty(PropertyName = "taxIncluded", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal TaxIncluded { get; set; }

        String _currency = "";
		[JsonProperty(PropertyName = "currency")]
		public string Currency
		{
			get { return _currency; }
			set
			{
				_currency = value;
			}
		}

		// Optional fields
		//

		[JsonProperty(PropertyName = "orderId")]
		public string OrderId { get; set; }
		public virtual bool ShouldSerializeOrderId() { return !String.IsNullOrEmpty(OrderId); }

		[JsonProperty(PropertyName = "itemDesc")]
		public string ItemDesc { get; set; }
		public virtual bool ShouldSerializeItemDesc() { return !String.IsNullOrEmpty(ItemDesc); }

		[JsonProperty(PropertyName = "itemCode")]
		public string ItemCode { get; set; }
		public virtual bool ShouldSerializeItemCode() { return !String.IsNullOrEmpty(ItemCode); }

		[JsonProperty(PropertyName = "posData")]
		public string PosData { get; set; }
		public virtual bool ShouldSerializePosData() { return !String.IsNullOrEmpty(PosData); }

		[JsonProperty(PropertyName = "notificationURL")]
		public string NotificationURL { get; set; }
		public virtual bool ShouldSerializeNotificationURL() { return !String.IsNullOrEmpty(NotificationURL); }

		[JsonProperty(PropertyName = "transactionSpeed")]
		public string TransactionSpeed { get; set; }
		public virtual bool ShouldSerializeTransactionSpeed() { return !String.IsNullOrEmpty(TransactionSpeed); }

		[JsonProperty(PropertyName = "fullNotifications")]
		public bool FullNotifications { get; set; }
		public virtual bool ShouldSerializeFullNotifications() { return FullNotifications; }

		[JsonProperty(PropertyName = "extendedNotifications")]
		public bool ExtendedNotifications
		{
			get; set;
		}
		public virtual bool ShouldSerializextendedNotifications()
		{
			return ExtendedNotifications;
		}

		[JsonProperty(PropertyName = "notificationEmail")]
		public string NotificationEmail { get; set; }
		public virtual bool ShouldSerializeNotificationEmail() { return !String.IsNullOrEmpty(NotificationEmail); }

		[JsonProperty(PropertyName = "redirectURL")]
		public string RedirectURL { get; set; }
		public virtual bool ShouldSerializeRedirectURL() { return !String.IsNullOrEmpty(RedirectURL); }

		[JsonProperty(PropertyName = "physical")]
		public bool Physical { get; set; }
		public virtual bool ShouldSerializePhysical() { return Physical; }

		[JsonProperty(PropertyName = "buyerName")]
		public string BuyerName { get; set; }
		public virtual bool ShouldSerializeBuyerName() { return !String.IsNullOrEmpty(BuyerName); }

		[JsonProperty(PropertyName = "buyerAddress1")]
		public string BuyerAddress1 { get; set; }
		public virtual bool ShouldSerializeBuyerAddress1() { return !String.IsNullOrEmpty(BuyerAddress1); }

		[JsonProperty(PropertyName = "buyerAddress2")]
		public string BuyerAddress2 { get; set; }
		public virtual bool ShouldSerializeBuyerAddress2() { return !String.IsNullOrEmpty(BuyerAddress2); }

		[JsonProperty(PropertyName = "buyerCity")]
		public string BuyerCity { get; set; }
		public virtual bool ShouldSerializeBuyerCity() { return !String.IsNullOrEmpty(BuyerCity); }

		[JsonProperty(PropertyName = "buyerState")]
		public string BuyerState { get; set; }
		public virtual bool ShouldSerializeBuyerState() { return !String.IsNullOrEmpty(BuyerState); }

		[JsonProperty(PropertyName = "buyerZip")]
		public string BuyerZip { get; set; }
		public virtual bool ShouldSerializeBuyerZip() { return !String.IsNullOrEmpty(BuyerZip); }

		[JsonProperty(PropertyName = "buyerCountry")]
		public string BuyerCountry { get; set; }
		public virtual bool ShouldSerializeBuyerCountry() { return !String.IsNullOrEmpty(BuyerCountry); }

		[JsonProperty(PropertyName = "buyerEmail")]
		public string BuyerEmail { get; set; }
		public virtual bool ShouldSerializeBuyerEmail() { return !String.IsNullOrEmpty(BuyerEmail); }

		[JsonProperty(PropertyName = "buyerPhone")]
		public string BuyerPhone { get; set; }
		public virtual bool ShouldSerializeBuyerPhone() { return !String.IsNullOrEmpty(BuyerPhone); }

		// Response fields
		//

		public string Id { get; set; }
		public virtual bool ShouldSerializeId() { return false; }

		public string Url { get; set; }
		public virtual bool ShouldSerializeUrl() { return false; }

		public string Status { get; set; }
		public bool IsPaid()
		{
			var paid = new[] { "paid", "confirmed", "complete" };
			return Status != null && paid.Contains(Status);
		}
		public virtual bool ShouldSerializeStatus() { return false; }

		[JsonConverter(typeof(MoneyJsonConverter))]
		public Money BtcPrice { get; set; }
		public virtual bool ShouldSerializeBtcPrice() { return false; }

		[JsonConverter(typeof(DateTimeJsonConverter))]
		public DateTimeOffset InvoiceTime { get; set; }
		public virtual bool ShouldSerializeInvoiceTime() { return false; }

		[JsonConverter(typeof(DateTimeJsonConverter))]
		public DateTimeOffset ExpirationTime { get; set; }
		public virtual bool ShouldSerializeExpirationTime() { return false; }

		[JsonConverter(typeof(DateTimeJsonConverter))]
		public DateTimeOffset CurrentTime { get; set; }
		public virtual bool ShouldSerializeCurrentTime() { return false; }

		[JsonConverter(typeof(MoneyJsonConverter))]
		public Money BtcPaid { get; set; }
		public virtual bool ShouldSerializeBtcPaid() { return false; }

		[JsonConverter(typeof(MoneyJsonConverter))]
		public Money BtcDue { get; set; }
		public virtual bool ShouldSerializeBtcDue() { return false; }

		[JsonProperty("cryptoInfo")]
		public InvoiceCryptoInfo[] CryptoInfo
		{
			get; set;
		}
		public virtual bool ShouldSerializeCryptoInfo()
		{
			return CryptoInfo != null;
		}

		public List<InvoiceTransaction> Transactions { get; set; }
		public virtual bool ShouldSerializeTransactions() { return false; }

		public decimal Rate { get; set; }
		public virtual bool ShouldSerializeRate() { return false; }

		public Dictionary<string, string> ExRates { get; set; }
		public virtual bool ShouldSerializeExRates() { return false; }

		public JToken ExceptionStatus { get; set; }
		public virtual bool ShouldSerializeExceptionStatus() { return false; }

		public InvoicePaymentUrls PaymentUrls { get; set; }
		public virtual bool ShouldSerializePaymentUrls() { return false; }

		public string BitcoinAddress
		{
			get; set;
		}
		public bool ShouldBitcoinAddress()
		{
			return false;
		}

		public bool Refundable
		{
			get { return this.Flags != null && this.Flags.Refundable; }
		}
		public virtual bool ShouldSerializeRefundable() { return false; }

		[Newtonsoft.Json.JsonProperty]
		private Flags Flags { get; set; }
		public virtual bool ShouldSerializeFlags() { return false; }

		public Dictionary<string, long> PaymentSubtotals { get; set; }

		public Dictionary<string, long> PaymentTotals { get; set; }
		public long AmountPaid { get; set; }

		public Dictionary<string, Dictionary<string, decimal>> ExchangeRates { get; set; }

		public Dictionary<string, InvoiceSupportedTransactionCurrency> SupportedTransactionCurrencies { get; set; }

		public Dictionary<string, MinerFeeInfo> MinerFees { get; set; }

		public Dictionary<string, string> Addresses { get; set; }
		public virtual bool ShouldSerializeAddresses() { return false; }

		public Dictionary<string, InvoicePaymentUrls> PaymentCodes
		{
			get; set;
		}
		public virtual bool ShouldSerializePaymentCodes() { return false; }

        public string TransactionCurrency { get; set; }
        public bool ShouldSerializeTransactionCurrency() { return false; }

        public List<RefundInfo> RefundInfo { get; set; }
        public bool ShouldSerializeRefundInfo() { return false; }

        public bool RefundAddressRequestPending { get; set; }
        public bool ShouldSerializeRefundAddressRequestPending() { return false; }

        public List<Dictionary<string, RefundAddress>> RefundAddresses { get; set; }
        public bool ShouldSerializeRefundAddresses() { return false; }

    }

    class Flags
	{
		public bool Refundable { get; set; }
	}
}
