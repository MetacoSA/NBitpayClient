using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NBitpayClient.Extensions;


namespace NBitpayClient
{

	public class Facade
	{
		public static Facade Merchant = new Facade("merchant");
		public static Facade PointOfSale = new Facade("pos");
		public static Facade User = new Facade("user");
		public static Facade Payroll = new Facade("payroll");
		readonly string _Value;

		public Facade(string value)
		{
			if(value == null)
				throw new ArgumentNullException(nameof(value));
			_Value = value;
		}


		public override bool Equals(object obj)
		{
			Facade item = obj as Facade;
			if(item == null)
				return false;
			return _Value.Equals(item._Value);
		}
		public static bool operator ==(Facade a, Facade b)
		{
			if(System.Object.ReferenceEquals(a, b))
				return true;
			if(((object)a == null) || ((object)b == null))
				return false;
			return a._Value == b._Value;
		}

		public static bool operator !=(Facade a, Facade b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return _Value.GetHashCode();
		}

		public override string ToString()
		{
			return _Value;
		}
	}

	public class PairingCode
	{
		public PairingCode(string value)
		{
			if(value == null)
				throw new ArgumentNullException(nameof(value));
			_Value = value;
		}

		readonly string _Value;
		public override string ToString()
		{
			return _Value;
		}

		public Uri CreateLink(Uri baseUrl)
		{
			var link = baseUrl.AbsoluteUri;
			if(!link.EndsWith("/"))
				link = link + "/";
			link = link + "api-access-request?pairingCode=" + _Value;
			return new Uri(link);
		}
	}

	public class Bitpay
	{
		public class AccessToken
		{
			public string Key
			{
				get; set;
			}
			public string Value
			{
				get; set;
			}
		}
		class AuthInformation
		{
			public AuthInformation(Key key)
			{
				if(key == null)
					throw new ArgumentNullException(nameof(key));
				SIN = key.PubKey.GetBitIDSIN();
				Key = key;
			}

			public Key Key
			{
				get;
				private set;
			}

			public string SIN
			{
				get;
				private set;
			}

			internal void Sign(HttpRequestMessage message)
			{
				var uri = message.RequestUri.AbsoluteUri;
				var body = message.Content?.ReadAsStringAsync()?.GetAwaiter().GetResult(); //no await is ok, no network access
				
				message.Headers.Add("x-signature", Key.GetBitIDSignature(uri, body));
				message.Headers.Add("x-identity", Key.PubKey.ToHex());

			}

			ConcurrentDictionary<string, AccessToken> _Tokens = new ConcurrentDictionary<string, AccessToken>();
			public AccessToken GetAccessToken(Facade requirement)
			{
				var token = _Tokens.TryGet(requirement.ToString());
				if(requirement == Facade.User)
				{
					token = _Tokens.TryGet(Facade.PointOfSale.ToString()) ?? _Tokens.TryGet(Facade.Merchant.ToString());
				}
				if(requirement == Facade.PointOfSale)
				{
					token = _Tokens.TryGet(Facade.Merchant.ToString());
				}
				return token;
			}

			public void SaveTokens(AccessToken[] tokens)
			{
				foreach(var token in tokens)
					_Tokens.TryAdd(token.Key, token);
			}

			public void SaveToken(string key, string value)
			{
				_Tokens.TryAdd(key, new AccessToken() { Key = key, Value = value });
			}
		}




		private const String BITPAY_API_VERSION = "2.0.0";

		private Uri _baseUrl = null;
		AuthInformation _Auth;
		static HttpClient _Client = new HttpClient();

		public string SIN
		{
			get
			{
				return _Auth.SIN;
			}
		}

		public Uri BaseUrl
		{
			get
			{
				return _baseUrl;
			}
		}

		/// <summary>
		/// Constructor for use if the keys and SIN were derived external to this library.
		/// </summary>
		/// <param name="ecKey">An elliptical curve key.</param>
		/// <param name="clientName">The label for this client.</param>
		/// <param name="envUrl">The target server URL.</param>
		public Bitpay(Key ecKey, Uri envUrl)
		{
			if(ecKey == null)
				throw new ArgumentNullException(nameof(ecKey));
			if(envUrl == null)
				throw new ArgumentNullException(nameof(envUrl));
			_Auth = new AuthInformation(ecKey);
			_baseUrl = envUrl;
		}

		/// <summary>
		/// Authorize (pair) this client with the server using the specified pairing code.
		/// </summary>
		/// <param name="pairingCode">A code obtained from the server; typically from bitpay.com/api-tokens.</param>
		public async Task AuthorizeClient(PairingCode pairingCode)
		{
			Token token = new Token();
			token.Id = _Auth.SIN;
			token.Guid = Guid.NewGuid().ToString();
			token.PairingCode = pairingCode.ToString();
			token.Label = "DEFAULT";
			String json = JsonConvert.SerializeObject(token);
			HttpResponseMessage response = await this.PostAsync("tokens", json).ConfigureAwait(false);
			var tokens = await this.ParseResponse<List<Token>>(response).ConfigureAwait(false);
			foreach(Token t in tokens)
			{
				_Auth.SaveToken(t.Facade, t.Value);
			}
		}

		/// <summary>
		/// Request authorization (a token) for this client in the specified facade.
		/// </summary>
		/// <param name="label">The label of this token.</param>
		/// <param name="facade">The facade for which authorization is requested.</param>
		/// <returns>A pairing code for this client.  This code must be used to authorize this client at BitPay.com/api-tokens.</returns>
		public PairingCode RequestClientAuthorization(string label, Facade facade)
		{
			return RequestClientAuthorizationAsync(label, facade).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Request authorization (a token) for this client in the specified facade.
		/// </summary>
		/// <param name="label">The label of this token.</param>
		/// <param name="facade">The facade for which authorization is requested.</param>
		/// <returns>A pairing code for this client.  This code must be used to authorize this client at BitPay.com/api-tokens.</returns>
		public async Task<PairingCode> RequestClientAuthorizationAsync(string label, Facade facade)
		{
			Token token = new Token();
			token.Id = _Auth.SIN;
			token.Guid = Guid.NewGuid().ToString();
			token.Facade = facade.ToString();
			token.Count = 1;
			token.Label = label ?? "DEFAULT";
			String json = JsonConvert.SerializeObject(token);
			HttpResponseMessage response = await this.PostAsync("tokens", json).ConfigureAwait(false);
			var tokens = await this.ParseResponse<List<Token>>(response).ConfigureAwait(false);
			// Expecting a single token resource.
			if(tokens.Count != 1)
			{
				throw new BitPayException("Error - failed to get token resource; expected 1 token, got " + tokens.Count);
			}
			_Auth.SaveToken(tokens[0].Facade, tokens[0].Value);
			return new PairingCode(tokens[0].PairingCode);
		}

		/// <summary>
		/// Create an invoice using the specified facade.
		/// </summary>
		/// <param name="invoice">An invoice request object.</param>
		/// <param name="facade">The facade used (default POS).</param>
		/// <returns>A new invoice object returned from the server.</returns>
		public Invoice CreateInvoice(Invoice invoice, Facade facade = null)
		{
			return CreateInvoiceAsync(invoice, facade).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Create an invoice using the specified facade.
		/// </summary>
		/// <param name="invoice">An invoice request object.</param>
		/// <param name="facade">The facade used (default POS).</param>
		/// <returns>A new invoice object returned from the server.</returns>
		public async Task<Invoice> CreateInvoiceAsync(Invoice invoice, Facade facade = null)
		{
			facade = facade ?? Facade.PointOfSale;
			invoice.Token = (await this.GetAccessTokenAsync(facade).ConfigureAwait(false)).Value;
			invoice.Guid = Guid.NewGuid().ToString();
			String json = JsonConvert.SerializeObject(invoice);
			HttpResponseMessage response = await this.PostAsync("invoices", json, true).ConfigureAwait(false);
			invoice = await this.ParseResponse<Invoice>(response).ConfigureAwait(false);
			// Track the token for this invoice
			//_Auth.SaveToken(invoice.Id, invoice.Token);
			return invoice;
		}

		/// <summary>
		/// Retrieve an invoice by id and token.
		/// </summary>
		/// <param name="invoiceId">The id of the requested invoice.</param>
		/// <param name="facade">The facade used (default POS).</param>
		/// <returns>The invoice object retrieved from the server.</returns>
		public Invoice GetInvoice(String invoiceId, Facade facade = null)
		{
			return GetInvoiceAsync(invoiceId, facade).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Retrieve an invoice by id and token.
		/// </summary>
		/// <param name="invoiceId">The id of the requested invoice.</param>
		/// <param name="facade">The facade used (default POS).</param>
		/// <returns>The invoice object retrieved from the server.</returns>
		public async Task<Invoice> GetInvoiceAsync(String invoiceId, Facade facade = null)
		{
			facade = facade ?? Facade.Merchant;
			// Provide the merchant token whenthe merchant facade is being used.
			// GET/invoices expects the merchant token and not the merchant/invoice token.

			HttpResponseMessage response = null;
			if(facade == Facade.Merchant)
			{
				var token = (await GetAccessTokenAsync(facade).ConfigureAwait(false)).Value;
				response = await this.GetAsync($"invoices/{invoiceId}?token={token}", true).ConfigureAwait(false);
			}
			else
			{
				response = await this.GetAsync($"invoices/" + invoiceId, true).ConfigureAwait(false);
			}
			return await this.ParseResponse<Invoice>(response).ConfigureAwait(false);
		}
		
		/// <summary>
        /// Retrieves settlement reports for the calling merchant filtered by query. The `limit` and `offset` parameters specify pages for large query sets.
        /// </summary>
        /// <param name="startDate">Date filter start (optional)</param>
        /// <param name="endDate">Date filter end (optional)</param>
        /// <param name="currency">Currency filter (optional)</param>
        /// <param name="status">Status filter (optional)</param>
        /// <param name="limit">Pagination entries ceiling (default:50)</param>
        /// <param name="offset">Pagination quantity offset (default:0)</param>
        /// <returns>Array of Settlements</returns>
        public Settlement[] GetSettlements(DateTime? startDate = null, DateTime? endDate = null, string currency = null, string status = null, int limit = 50, int offset = 0)
        {
            return GetSettlementsAsync(startDate, endDate, currency, status, limit, offset).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieves settlement reports for the calling merchant filtered by query. The `limit` and `offset` parameters specify pages for large query sets.
        /// </summary>
        /// <param name="startDate">Date filter start (optional)</param>
        /// <param name="endDate">Date filter end (optional)</param>
        /// <param name="currency">Currency filter (optional)</param>
        /// <param name="status">Status filter (optional)</param>
        /// <param name="limit">Pagination entries ceiling (default:50)</param>
        /// <param name="offset">Pagination quantity offset (default:0)</param>
        /// <returns>Array of Settlements</returns>
        public async Task<Settlement[]> GetSettlementsAsync(DateTime? startDate = null, DateTime? endDate = null, string currency = null, string status = null, int limit = 50, int offset = 0)
        {
            var token = await this.GetAccessTokenAsync(Facade.Merchant).ConfigureAwait(false);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("token", token.Value);
            if (startDate != null)
                parameters.Add("startDate", startDate.Value.ToString("d", CultureInfo.InvariantCulture));
            if (endDate != null)
                parameters.Add("endDate", endDate.Value.ToString("d", CultureInfo.InvariantCulture));
            if (currency != null)
                parameters.Add("currency", currency);
            if (status != null)
                parameters.Add("status", status);
            parameters.Add("limit", $"{limit}");
            parameters.Add("offset", $"{offset}");

            HttpResponseMessage response = await this.GetAsync($"settlements" + BuildQuery(parameters), true).ConfigureAwait(false);
            return await this.ParseResponse<Settlement[]>(response).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a summary of the specified settlement.
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <returns>Settlement object</returns>
	    public Settlement GetSettlement(string settlementId)
	    {
	        return GetSettlementAsync(settlementId).GetAwaiter().GetResult();
	    }

	    /// <summary>
	    /// Retrieves a summary of the specified settlement.
	    /// </summary>
	    /// <param name="settlementId">Id of the settlement to fetch</param>
	    /// <returns>Settlement object</returns>
	    public async Task<Settlement> GetSettlementAsync(string settlementId)
	    {
	        var token = (await GetAccessTokenAsync(Facade.Merchant).ConfigureAwait(false)).Value;
	        HttpResponseMessage response = await this.GetAsync($"settlements/{settlementId}?token={token}", true).ConfigureAwait(false);
            return await this.ParseResponse<Settlement>(response).ConfigureAwait(false);
	    }

        /// <summary>
        /// Gets a detailed reconciliation report of the activity within the settlement period
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <returns>SettlementReconciliationReport object</returns>
        public SettlementReconciliationReport GetSettlementReconciliationReport(string settlementId)
	    {
	        return GetSettlementReconciliationReportAsync(settlementId).GetAwaiter().GetResult();
	    }

        /// <summary>
        /// Gets a detailed reconciliation report of the activity within the settlement period
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <returns>SettlementReconciliationReport object</returns>
        public async Task<SettlementReconciliationReport> GetSettlementReconciliationReportAsync(string settlementId)
	    {
	        var token = (await GetAccessTokenAsync(Facade.Merchant).ConfigureAwait(false)).Value;
	        HttpResponseMessage response = await this.GetAsync($"settlements/{settlementId}/reconciliationReport?token={token}", true).ConfigureAwait(false);
	        return await this.ParseResponse<SettlementReconciliationReport>(response).ConfigureAwait(false);
	    }

        /// <summary>
        /// Retrieve a list of invoices by date range using the merchant facade.
        /// </summary>
        /// <param name="dateStart">The start date for the query.</param>
        /// <param name="dateEnd">The end date for the query.</param>
        /// <returns>A list of invoice objects retrieved from the server.</returns>
        public Invoice[] GetInvoices(DateTime? dateStart = null, DateTime? dateEnd = null)
		{
			return GetInvoicesAsync(dateStart, dateEnd).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Retrieve a list of invoices by date range using the merchant facade.
		/// </summary>
		/// <param name="dateStart">The start date for the query.</param>
		/// <param name="dateEnd">The end date for the query.</param>
		/// <returns>A list of invoice objects retrieved from the server.</returns>
		public async Task<Invoice[]> GetInvoicesAsync(DateTime? dateStart = null, DateTime? dateEnd = null)
		{
			var token = await this.GetAccessTokenAsync(Facade.Merchant).ConfigureAwait(false);

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("token", token.Value);
			if(dateStart != null)
				parameters.Add("dateStart", dateStart.Value.ToString("d", CultureInfo.InvariantCulture));
			if(dateEnd != null)
				parameters.Add("dateEnd", dateEnd.Value.ToString("d", CultureInfo.InvariantCulture));
			HttpResponseMessage response = await this.GetAsync($"invoices" + BuildQuery(parameters), true).ConfigureAwait(false);
			return await this.ParseResponse<Invoice[]>(response).ConfigureAwait(false);
		}

		private string BuildQuery(Dictionary<string, string> parameters)
		{
			var result = String.Join("&", parameters
				.Select(p => $"{p.Key}={p.Value}")
				.ToArray());
			if(string.IsNullOrEmpty(result))
				return string.Empty;
			return "?" + result;
		}

		/// <summary>
		/// Retrieve the exchange rate table using the public facade.
		/// </summary>
		/// <returns>The rate table as an object retrieved from the server.</returns>
		public Rates GetRates()
		{
			return GetRatesAsync().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Retrieve the exchange rate table using the public facade.
		/// </summary>
		/// <returns>The rate table as an object retrieved from the server.</returns>
		public async Task<Rates> GetRatesAsync()
		{
			HttpResponseMessage response = await this.GetAsync("rates", false).ConfigureAwait(false);
			var rates = await this.ParseResponse<List<Rate>>(response).ConfigureAwait(false);
			return new Rates(rates);
		}

		/// <summary>
		/// Retrieves the caller's ledgers for each currency with summary.
		/// </summary>
		/// <returns>A list of ledger objects retrieved from the server.</returns>
		public List<Ledger> GetLedgers()
		{
			return GetLedgersAsync().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Retrieves the caller's ledgers for each currency with summary.
		/// </summary>
		/// <returns>A list of ledger objects retrieved from the server.</returns>
		public async Task<List<Ledger>> GetLedgersAsync()
		{
			var token = await this.GetAccessTokenAsync(Facade.Merchant).ConfigureAwait(false);

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("token", token.Value);

			HttpResponseMessage response = await this.GetAsync($"ledgers/" + BuildQuery(parameters), true).ConfigureAwait(false);
			var ledgers = await this.ParseResponse<List<Ledger>>(response).ConfigureAwait(false);
			return ledgers;
		}

		/// <summary>
		/// Retrieve a list of ledger entries by date range using the merchant facade.
		/// </summary>
		/// <param name="currency">The three digit currency string for the ledger to retrieve.</param>
		/// <param name="dateStart">The start date for the query.</param>
		/// <param name="dateEnd">The end date for the query.</param>
		/// <returns>A list of invoice objects retrieved from the server.</returns>
		public Ledger GetLedger(String currency, DateTime? dateStart = null, DateTime? dateEnd = null)
		{
			return GetLedgerAsync(currency, dateStart, dateEnd).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Retrieve a list of ledger entries by date range using the merchant facade.
		/// </summary>
		/// <param name="currency">The three digit currency string for the ledger to retrieve.</param>
		/// <param name="dateStart">The start date for the query.</param>
		/// <param name="dateEnd">The end date for the query.</param>
		/// <returns>A list of invoice objects retrieved from the server.</returns>
		public async Task<Ledger> GetLedgerAsync(String currency, DateTime? dateStart = null, DateTime? dateEnd = null)
		{
			var token = await this.GetAccessTokenAsync(Facade.Merchant).ConfigureAwait(false);

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("token", token.Value);
			if(dateStart != null)
				parameters.Add("startDate", dateStart.Value.ToString("d", CultureInfo.InvariantCulture));
			if(dateEnd != null)
				parameters.Add("endDate", dateEnd.Value.ToString("d", CultureInfo.InvariantCulture));

			HttpResponseMessage response = await this.GetAsync($"ledgers/{currency}" + BuildQuery(parameters), true).ConfigureAwait(false);
			var entries = await this.ParseResponse<List<LedgerEntry>>(response).ConfigureAwait(false);
			return new Ledger(null, 0, entries);
		}

		private AccessToken[] ParseTokens(string response)
		{
			List<AccessToken> tokens = new List<AccessToken>();
			try
			{
				JArray obj = (JArray)JObject.Parse(response).First.First;
				foreach(var jobj in obj.OfType<JObject>())
				{
					foreach(var prop in jobj.Properties())
					{
						tokens.Add(new AccessToken() { Key = prop.Name, Value = prop.Value.Value<string>() });
					}
				}
				return tokens.ToArray();
			}
			catch(Exception ex)
			{
				throw new BitPayException("Error: response to GET /tokens could not be parsed - " + ex.ToString());
			}
		}

		public async Task<AccessToken[]> GetAccessTokensAsync()
		{
			HttpResponseMessage response = await this.GetAsync("tokens", true).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			return ParseTokens(result);
		}

		/// <summary>
		/// Test access to a facade
		/// </summary>
		/// <param name="facade">The facade</param>
		/// <returns>True authorized, else false</returns>
		public bool TestAccess(Facade facade)
		{
			return TestAccessAsync(facade).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Test access to a facade
		/// </summary>
		/// <param name="facade">The facade</param>
		/// <returns>True if authorized, else false</returns>
		public async Task<bool> TestAccessAsync(Facade facade)
		{
			if(facade == null)
				throw new ArgumentNullException(nameof(facade));

			var auth = _Auth;
			var token = auth.GetAccessToken(facade);
			if(token != null)
				return true;
			var tokens = await GetAccessTokensAsync().ConfigureAwait(false);
			auth.SaveTokens(tokens);
			token = auth.GetAccessToken(facade);
			return token != null;
		}

		public async Task<AccessToken> GetAccessTokenAsync(Facade facade)
		{
			var auth = _Auth;
			var token = auth.GetAccessToken(facade);
			if(token != null)
				return token;

			var tokens = await GetAccessTokensAsync().ConfigureAwait(false);
			auth.SaveTokens(tokens);
			token = auth.GetAccessToken(facade);
			if(token == null)
				throw new BitPayException("Error: You do not have access to facade: " + facade);
			return token;
		}

		private async Task<HttpResponseMessage> GetAsync(String path, bool sign)
		{
			try
			{
				var message = new HttpRequestMessage(HttpMethod.Get, GetFullUri(path));
				message.Headers.Add("x-accept-version", BITPAY_API_VERSION);
				if(sign)
				{
					_Auth.Sign(message);
				}
				var result = await _Client.SendAsync(message).ConfigureAwait(false);
				return result;
			}
			catch(Exception ex)
			{
				throw new BitPayException("Error: " + ex.ToString());
			}
		}

		private async Task<HttpResponseMessage> PostAsync(String path, String json, bool signatureRequired = false)
		{
			try
			{
				var message = new HttpRequestMessage(HttpMethod.Post, GetFullUri(path));
				message.Headers.Add("x-accept-version", BITPAY_API_VERSION);
				message.Content = new StringContent(json, Encoding.UTF8, "application/json");
				if(signatureRequired)
				{
					_Auth.Sign(message);
				}
				var result = await _Client.SendAsync(message).ConfigureAwait(false);
				return result;
			}
			catch(Exception ex)
			{
				throw new BitPayException("Error: " + ex.ToString());
			}
		}

		private string GetFullUri(string relativePath)
		{
			var uri = _baseUrl.AbsoluteUri;
			if(!uri.EndsWith("/", StringComparison.Ordinal))
				uri += "/";
			uri += relativePath;
			return uri;
		}

		private async Task<T> ParseResponse<T>(HttpResponseMessage response)
		{
			if(response == null)
			{
				throw new BitPayException("Error: HTTP response is null");
			}

			// Get the response as a dynamic object for detecting possible error(s) or data object.
			// An error(s) object raises an exception.
			// A data object has its content extracted (throw away the data wrapper object).
			String responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			
			if(responseString[0] == '[') {
				// some endpoints return an array at the root (like /Ledgers/{currency}).
				// without short circuiting here, the JObject.Parse will throw
				return JsonConvert.DeserializeObject<T>(responseString);
			}
			
			var obj = JObject.Parse(responseString);

			// Check for error response.
			if(obj.Property("error") != null)
			{
				var ex = new BitPayException();
				ex.AddError(obj.Property("error").Value.ToString());
				throw ex;
			}
			if(obj.Property("errors") != null)
			{
				var ex = new BitPayException();
				foreach(var errorItem in ((JArray)obj.Property("errors").Value).OfType<JObject>())
				{
					ex.AddError(errorItem.Property("error").Value.ToString() + " " + errorItem.Property("param").Value.ToString());
				}
				throw ex;
			}

			T data = default(T);
			// Check for and exclude a "data" object from the response.
			if(obj.Property("data") != null)
			{
				responseString = JObject.Parse(responseString).SelectToken("data").ToString();
				data = JsonConvert.DeserializeObject<T>(responseString);
			}
			return data;
		}
	}
}
