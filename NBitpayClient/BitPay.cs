using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
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
			if(ReferenceEquals(a, b))
				return true;
			if((object)a == null || (object)b == null)
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
            _Value = value ?? throw new ArgumentNullException(nameof(value));
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
        AuthInformation _Auth;

        public static HttpClient HttpClient { get; set; } = new HttpClient();

        public string SIN
		{
			get
			{
				return _Auth.SIN;
			}
		}

        public Uri BaseUrl { get; } = null;

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
			BaseUrl = envUrl;
		}

		/// <summary>
		/// Authorize (pair) this client with the server using the specified pairing code.
		/// </summary>
		/// <param name="pairingCode">A code obtained from the server; typically from bitpay.com/api-tokens.</param>
		public virtual async Task AuthorizeClient(PairingCode pairingCode)
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
		public virtual PairingCode RequestClientAuthorization(string label, Facade facade)
		{
			return RequestClientAuthorizationAsync(label, facade).GetAwaiter().GetResult();
		}

        /// <summary>
        /// Request authorization (a token) for this client in the specified facade.
        /// </summary>
        /// <param name="label">The label of this token.</param>
        /// <param name="facade">The facade for which authorization is requested.</param>
        /// <returns>A pairing code for this client.  This code must be used to authorize this client at BitPay.com/api-tokens.</returns>
        public virtual async Task<PairingCode> RequestClientAuthorizationAsync(string label, Facade facade)
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
        public virtual Invoice CreateInvoice(Invoice invoice, Facade facade = null)
		{
			return CreateInvoiceAsync(invoice, facade).GetAwaiter().GetResult();
		}

        /// <summary>
        /// Create an invoice using the specified facade.
        /// </summary>
        /// <param name="invoice">An invoice request object.</param>
        /// <param name="facade">The facade used (default POS).</param>
        /// <returns>A new invoice object returned from the server.</returns>
        public virtual async Task<Invoice> CreateInvoiceAsync(Invoice invoice, Facade facade = null)
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
        public virtual Invoice GetInvoice(String invoiceId, Facade facade = null)
		{
			return GetInvoiceAsync(invoiceId, facade).GetAwaiter().GetResult();
		}

        /// <summary>
        /// Retrieve an invoice by id and token.
        /// </summary>
        /// <param name="invoiceId">The id of the requested invoice.</param>
        /// <param name="facade">The facade used (default POS).</param>
        /// <returns>The invoice object retrieved from the server.</returns>
        public virtual async Task<Invoice> GetInvoiceAsync(String invoiceId, Facade facade = null)
        {
            return await GetInvoiceAsync<Invoice>(invoiceId, facade);
        }

        /// <summary>
        /// Retrieve an invoice by id and token.
        /// </summary>
        /// <param name="invoiceId">The id of the requested invoice.</param>
        /// <param name="facade">The facade used (default POS).</param>
        /// <returns>The invoice object retrieved from the server.</returns>
        public virtual async Task<T> GetInvoiceAsync<T>(String invoiceId, Facade facade = null) where T : Invoice
        {
            facade = facade ?? Facade.Merchant;
            // Provide the merchant token whenthe merchant facade is being used.
            // GET/invoices expects the merchant token and not the merchant/invoice token.

            HttpResponseMessage response = null;
            if (facade == Facade.Merchant)
            {
                var token = (await GetAccessTokenAsync(facade).ConfigureAwait(false)).Value;
                response = await this.GetAsync($"invoices/{invoiceId}?token={token}", true).ConfigureAwait(false);
            }
            else
            {
                response = await GetAsync($"invoices/" + invoiceId, true).ConfigureAwait(false);
            }

            return await ParseResponse<T>(response).ConfigureAwait(false);
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
        public virtual SettlementSummary[] GetSettlements(DateTime? startDate = null, DateTime? endDate = null, string currency = null, string status = null, int limit = 50, int offset = 0)
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
        public virtual async Task<SettlementSummary[]> GetSettlementsAsync(DateTime? startDate = null, DateTime? endDate = null, string currency = null, string status = null, int limit = 50, int offset = 0)
        {
            var token = await this.GetAccessTokenAsync(Facade.Merchant).ConfigureAwait(false);

            var parameters = new Dictionary<string, string>();
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

            var response = await GetAsync("settlements" + BuildQuery(parameters), true).ConfigureAwait(false);
            return await ParseResponse<SettlementSummary[]>(response).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a summary of the specified settlement.
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <returns>Settlement object</returns>
	    public virtual SettlementSummary GetSettlementSummary(string settlementId)
	    {
	        return GetSettlementSummaryAsync(settlementId).GetAwaiter().GetResult();
	    }

        /// <summary>
        /// Retrieves a summary of the specified settlement.
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <returns>Settlement object</returns>
        public virtual async Task<SettlementSummary> GetSettlementSummaryAsync(string settlementId)
	    {
	        var token = (await GetAccessTokenAsync(Facade.Merchant).ConfigureAwait(false)).Value;
	        var response = await GetAsync($"settlements/{settlementId}?token={token}", true).ConfigureAwait(false);
            return await ParseResponse<SettlementSummary>(response).ConfigureAwait(false);
	    }

        /// <summary>
        /// Gets a detailed reconciliation report of the activity within the settlement period
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <param name="settlementSummaryToken">Resource token from /settlements/{settlementId}</param>
        /// <returns>SettlementReconciliationReport object</returns>
        public virtual SettlementReconciliationReport GetSettlementReconciliationReport(string settlementId, string settlementSummaryToken)
	    {
	        return GetSettlementReconciliationReportAsync(settlementId, settlementSummaryToken).GetAwaiter().GetResult();
	    }

        /// <summary>
        /// Gets a detailed reconciliation report of the activity within the settlement period
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <param name="settlementSummaryToken">Resource token from /settlements/{settlementId}</param>
        /// <returns>SettlementReconciliationReport object</returns>
        public virtual async Task<SettlementReconciliationReport> GetSettlementReconciliationReportAsync(string settlementId, string settlementSummaryToken)
	    {
	        var response = await GetAsync($"settlements/{settlementId}/reconciliationReport?token={settlementSummaryToken}", true).ConfigureAwait(false);
	        return await ParseResponse<SettlementReconciliationReport>(response).ConfigureAwait(false);
	    }

        /// <summary>
        /// Retrieve a list of invoices by date range using the merchant facade.
        /// </summary>
        /// <param name="dateStart">The start date for the query.</param>
        /// <param name="dateEnd">The end date for the query.</param>
        /// <returns>A list of invoice objects retrieved from the server.</returns>
        public virtual Invoice[] GetInvoices(DateTime? dateStart = null, DateTime? dateEnd = null)
		{
			return GetInvoicesAsync(dateStart, dateEnd).GetAwaiter().GetResult();
		}

        /// <summary>
        /// Retrieve a list of invoices by date range using the merchant facade.
        /// </summary>
        /// <param name="dateStart">The start date for the query.</param>
        /// <param name="dateEnd">The end date for the query.</param>
        /// <returns>A list of invoice objects retrieved from the server.</returns>
        public virtual async Task<Invoice[]> GetInvoicesAsync(DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            return await GetInvoicesAsync<Invoice>(dateStart, dateEnd);
        }
        
        /// <summary>
        /// Retrieve a list of invoices by date range using the merchant facade.
        /// </summary>
        /// <param name="dateStart">The start date for the query.</param>
        /// <param name="dateEnd">The end date for the query.</param>
        /// <returns>A list of invoice objects retrieved from the server.</returns>
        public virtual async Task<T[]> GetInvoicesAsync<T>(DateTime? dateStart = null, DateTime? dateEnd = null) where T:Invoice
        {
            var token = await this.GetAccessTokenAsync(Facade.Merchant).ConfigureAwait(false);

            var parameters = new Dictionary<string, string>();
            parameters.Add("token", token.Value);
            if(dateStart != null)
                parameters.Add("dateStart", dateStart.Value.ToString("d", CultureInfo.InvariantCulture));
            if(dateEnd != null)
                parameters.Add("dateEnd", dateEnd.Value.ToString("d", CultureInfo.InvariantCulture));
            var response = await GetAsync($"invoices" + BuildQuery(parameters), true).ConfigureAwait(false);
            return await ParseResponse<T[]>(response).ConfigureAwait(false);
        }

		private string BuildQuery(Dictionary<string, string> parameters)
		{
			var result = string.Join("&", parameters
				.Select(p => $"{p.Key}={p.Value}")
				.ToArray());
			if(string.IsNullOrEmpty(result))
				return string.Empty;
			return "?" + result;
		}

        /// <summary>
        /// Retrieve the exchange rate table using the public facade.
        /// </summary>
        /// <param name="baseCurrencyCode">What currency to base the result rates on</param>
        /// <returns>The rate table as an object retrieved from the server.</returns>
        public virtual Rates GetRates(string baseCurrencyCode = null)
		{
			return GetRatesAsync(baseCurrencyCode).GetAwaiter().GetResult();
		}

        /// <summary>
        /// Retrieve the exchange rate table using the public facade.
        /// </summary>
        /// <param name="baseCurrencyCode">What currency to base the result rates on</param>
        /// <returns>The rate table as an object retrieved from the server.</returns>
        public virtual async Task<Rates> GetRatesAsync(string baseCurrencyCode = null)
		{
            var url = $"rates{(string.IsNullOrEmpty(baseCurrencyCode) ? $"/{baseCurrencyCode}" : String.Empty)}";

            HttpResponseMessage response = await this.GetAsync(url, true).ConfigureAwait(false);
			var rates = await this.ParseResponse<List<Rate>>(response).ConfigureAwait(false);
			return new Rates(rates);
		}

        /// <summary>
        /// Retrieves the exchange rate for the given currency using the public facade.
        /// </summary>
        /// <param name="baseCurrencyCode">The currency to base the result rate on</param>
        /// <param name="currencyCode">The target currency to get the rate for</param>
        /// <returns>The rate as an object retrieved from the server.</returns>
        public Rate GetRate(string baseCurrencyCode, string currencyCode)
	    {
	        return GetRateAsync(baseCurrencyCode, currencyCode).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieves the exchange rate for the given currency using the public facade.
        /// </summary>
        /// <param name="baseCurrencyCode">The currency to base the result rate on</param>
        /// <param name="currencyCode">The target currency to get the rate for</param>
        /// <returns>The rate as an object retrieved from the server.</returns>
        public virtual async Task<Rate> GetRateAsync(string baseCurrencyCode, string currencyCode)
        {
	        var url = $"rates/{baseCurrencyCode}/{currencyCode}";
	        HttpResponseMessage response = await this.GetAsync(url, true).ConfigureAwait(false);
	        var rate = await this.ParseResponse<Rate>(response).ConfigureAwait(false);
	        return rate;
	    }

        /// <summary>
        /// Retrieves the caller's ledgers for each currency with summary.
        /// </summary>
        /// <returns>A list of ledger objects retrieved from the server.</returns>
        public virtual List<Ledger> GetLedgers()
		{
			return GetLedgersAsync().GetAwaiter().GetResult();
		}

        /// <summary>
        /// Retrieves the caller's ledgers for each currency with summary.
        /// </summary>
        /// <returns>A list of ledger objects retrieved from the server.</returns>
        public virtual async Task<List<Ledger>> GetLedgersAsync()
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
        public virtual Ledger GetLedger(String currency, DateTime? dateStart = null, DateTime? dateEnd = null)
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
        public virtual async Task<Ledger> GetLedgerAsync(String currency, DateTime? dateStart = null, DateTime? dateEnd = null)
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

        public virtual async Task<AccessToken[]> GetAccessTokensAsync()
		{
			HttpResponseMessage response = await this.GetAsync("tokens", true).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new AccessToken[0];
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			return ParseTokens(result);
		}

        /// <summary>
        /// Test access to a facade
        /// </summary>
        /// <param name="facade">The facade</param>
        /// <returns>True authorized, else false</returns>
        public virtual bool TestAccess(Facade facade)
		{
			return TestAccessAsync(facade).GetAwaiter().GetResult();
		}

        /// <summary>
        /// Test access to a facade
        /// </summary>
        /// <param name="facade">The facade</param>
        /// <returns>True if authorized, else false</returns>
        public virtual async Task<bool> TestAccessAsync(Facade facade)
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

        public virtual async Task<AccessToken> GetAccessTokenAsync(Facade facade)
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
				var result = await HttpClient.SendAsync(message).ConfigureAwait(false);
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
				var result = await HttpClient.SendAsync(message).ConfigureAwait(false);
				return result;
			}
			catch(Exception ex)
			{
				throw new BitPayException("Error: " + ex.ToString());
			}
		}

		private string GetFullUri(string relativePath)
		{
			var uri = BaseUrl.AbsoluteUri;
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
			
			if(responseString.Length > 0 && responseString[0] == '[') {
				// some endpoints return an array at the root (like /Ledgers/{currency}).
				// without short circuiting here, the JObject.Parse will throw
				return JsonConvert.DeserializeObject<T>(responseString);
			}

            JObject obj = null;
            try
            {
                obj = JObject.Parse(responseString);
            }
            catch
            {
                // Probably a http error, send this instead of parsing error.
                response.EnsureSuccessStatusCode();
                throw;
            }

            // Check for error response.
            if (obj.Property("error") != null)
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
