using NBitcoin;
using NBitpayClient.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NBitpayClient
{

	//{"id":"NzzNUB5DEMLP5q95szL1VS","url":"https://test.bitpay.com/invoice?id=NzzNUB5DEMLP5q95szL1VS",
	//"posData":"posData","status":"paid","btcPrice":"0.001246","price":5,"currency":"USD",
	//"invoiceTime":1503140597709,"expirationTime":1503141497709,"currentTime":1503140607752,
	//"btcPaid":"0.001246","btcDue":"0.000000","rate":4012.12,"exceptionStatus":false,"buyerFields":{}}
	public class InvoicePaymentNotification
	{
		[JsonProperty(PropertyName = "id")]
		public string Id
		{
			get; set;
		}

		[JsonProperty(PropertyName = "url")]
		public string Url
		{
			get; set;
		}

		[JsonProperty(PropertyName = "posData")]
		public string PosData
		{
			get; set;
		}

		[JsonProperty(PropertyName = "status")]
		public string Status
		{
			get; set;
		}


		[JsonProperty(PropertyName = "btcPrice")]
		[JsonConverter(typeof(MoneyJsonConverter))]
		public Money BTCPrice
		{
			get; set;
		}

		[JsonProperty(PropertyName = "price")]
		public decimal Price
		{
			get; set;
		}

		[JsonProperty(PropertyName = "currency")]
		public string Currency
		{
			get; set;
		}

		[JsonConverter(typeof(DateTimeJsonConverter))]
		[JsonProperty(PropertyName = "invoiceTime")]
		public DateTimeOffset InvoiceTime
		{
			get; set;
		}

		[JsonConverter(typeof(DateTimeJsonConverter))]
		[JsonProperty(PropertyName = "expirationTime")]
		public DateTimeOffset ExpirationTime
		{
			get; set;
		}


		[JsonConverter(typeof(DateTimeJsonConverter))]
		[JsonProperty(PropertyName = "currentTime")]
		public DateTimeOffset CurrentTime
		{
			get; set;
		}

		[JsonProperty(PropertyName = "btcPaid")]
		[JsonConverter(typeof(MoneyJsonConverter))]
		public Money BTCPaid
		{
			get; set;
		}

		[JsonProperty(PropertyName = "btcDue")]
		[JsonConverter(typeof(MoneyJsonConverter))]
		public Money BTCDue
		{
			get; set;
		}

		[JsonProperty(PropertyName = "rate")]
		public decimal Rate
		{
			get; set;
		}

		[JsonProperty(PropertyName = "exceptionStatus")]
		public JToken ExceptionStatus
		{
			get; set;
		}

		[JsonProperty(PropertyName = "buyerFields")]
		public JObject BuyerFields
		{
			get; set;
		}

	    [JsonProperty(PropertyName = "transactionCurrency")]
	    public string TransactionCurrency
        {
	        get; set;
	    }
	    [JsonProperty(PropertyName = "paymentSubtotals")]
	    public Dictionary<string, decimal> PaymentSubtotals
        {
	        get; set;
	    }
	    [JsonProperty(PropertyName = "paymentTotals")]
	    public Dictionary<string, decimal> PaymentTotals
        {
	        get; set;
	    }

	    [JsonProperty(PropertyName = "amountPaid")]
	    [JsonConverter(typeof(MoneyJsonConverter))]
	    public Money AmountPaid
	    {
	        get; set;
	    }

	    [JsonProperty(PropertyName = "exchangeRates")]
	    public Dictionary<string, Dictionary<string, decimal>> ExchangeRates
        {
	        get; set;
	    }

	    [JsonProperty(PropertyName = "orderId")]
	    public string OrderId
        {
	        get; set;
	    }
    }
}
