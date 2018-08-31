using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBitpayClient
{
    public interface IBitpay
    {
        /// <summary>
        /// Authorize (pair) this client with the server using the specified pairing code.
        /// </summary>
        /// <param name="pairingCode">A code obtained from the server; typically from bitpay.com/api-tokens.</param>
        Task AuthorizeClient(PairingCode pairingCode);

        /// <summary>
        /// Request authorization (a token) for this client in the specified facade.
        /// </summary>
        /// <param name="label">The label of this token.</param>
        /// <param name="facade">The facade for which authorization is requested.</param>
        /// <returns>A pairing code for this client.  This code must be used to authorize this client at BitPay.com/api-tokens.</returns>
        PairingCode RequestClientAuthorization(string label, Facade facade);

        /// <summary>
        /// Request authorization (a token) for this client in the specified facade.
        /// </summary>
        /// <param name="label">The label of this token.</param>
        /// <param name="facade">The facade for which authorization is requested.</param>
        /// <returns>A pairing code for this client.  This code must be used to authorize this client at BitPay.com/api-tokens.</returns>
        Task<PairingCode> RequestClientAuthorizationAsync(string label, Facade facade);

        /// <summary>
        /// Create an invoice using the specified facade.
        /// </summary>
        /// <param name="invoice">An invoice request object.</param>
        /// <param name="facade">The facade used (default POS).</param>
        /// <returns>A new invoice object returned from the server.</returns>
        Invoice CreateInvoice(Invoice invoice, Facade facade = null);

        /// <summary>
        /// Create an invoice using the specified facade.
        /// </summary>
        /// <param name="invoice">An invoice request object.</param>
        /// <param name="facade">The facade used (default POS).</param>
        /// <returns>A new invoice object returned from the server.</returns>
        Task<Invoice> CreateInvoiceAsync(Invoice invoice, Facade facade = null);

        /// <summary>
        /// Retrieve an invoice by id and token.
        /// </summary>
        /// <param name="invoiceId">The id of the requested invoice.</param>
        /// <param name="facade">The facade used (default POS).</param>
        /// <returns>The invoice object retrieved from the server.</returns>
        Invoice GetInvoice(String invoiceId, Facade facade = null);

        /// <summary>
        /// Retrieve an invoice by id and token.
        /// </summary>
        /// <param name="invoiceId">The id of the requested invoice.</param>
        /// <param name="facade">The facade used (default POS).</param>
        /// <returns>The invoice object retrieved from the server.</returns>
        Task<Invoice> GetInvoiceAsync(String invoiceId, Facade facade = null);

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
        SettlementSummary[] GetSettlements(DateTime? startDate = null, DateTime? endDate = null, string currency = null, string status = null, int limit = 50, int offset = 0);

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
        Task<SettlementSummary[]> GetSettlementsAsync(DateTime? startDate = null, DateTime? endDate = null, string currency = null, string status = null, int limit = 50, int offset = 0);

        /// <summary>
        /// Retrieves a summary of the specified settlement.
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <returns>Settlement object</returns>
        SettlementSummary GetSettlementSummary(string settlementId);

        /// <summary>
        /// Retrieves a summary of the specified settlement.
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <returns>Settlement object</returns>
        Task<SettlementSummary> GetSettlementSummaryAsync(string settlementId);

        /// <summary>
        /// Gets a detailed reconciliation report of the activity within the settlement period
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <param name="settlementSummaryToken">Resource token from /settlements/{settlementId}</param>
        /// <returns>SettlementReconciliationReport object</returns>
        SettlementReconciliationReport GetSettlementReconciliationReport(string settlementId, string settlementSummaryToken);

        /// <summary>
        /// Gets a detailed reconciliation report of the activity within the settlement period
        /// </summary>
        /// <param name="settlementId">Id of the settlement to fetch</param>
        /// <param name="settlementSummaryToken">Resource token from /settlements/{settlementId}</param>
        /// <returns>SettlementReconciliationReport object</returns>
        Task<SettlementReconciliationReport> GetSettlementReconciliationReportAsync(string settlementId, string settlementSummaryToken);

        /// <summary>
        /// Retrieve a list of invoices by date range using the merchant facade.
        /// </summary>
        /// <param name="dateStart">The start date for the query.</param>
        /// <param name="dateEnd">The end date for the query.</param>
        /// <returns>A list of invoice objects retrieved from the server.</returns>
        Invoice[] GetInvoices(DateTime? dateStart = null, DateTime? dateEnd = null);

        /// <summary>
        /// Retrieve a list of invoices by date range using the merchant facade.
        /// </summary>
        /// <param name="dateStart">The start date for the query.</param>
        /// <param name="dateEnd">The end date for the query.</param>
        /// <returns>A list of invoice objects retrieved from the server.</returns>
        Task<Invoice[]> GetInvoicesAsync(DateTime? dateStart = null, DateTime? dateEnd = null);

        /// <summary>
        /// Retrieve the exchange rate table using the public facade.
        /// </summary>
        /// <param name="baseCurrencyCode">What currency to base the result rates on</param>
        /// <returns>The rate table as an object retrieved from the server.</returns>
        Rates GetRates(string baseCurrencyCode = null);

        /// <summary>
        /// Retrieve the exchange rate table using the public facade.
        /// </summary>
        /// <param name="baseCurrencyCode">What currency to base the result rates on</param>
        /// <returns>The rate table as an object retrieved from the server.</returns>
        Task<Rates> GetRatesAsync(string baseCurrencyCode = null);

        /// <summary>
        /// Retrieves the exchange rate for the given currency using the public facade.
        /// </summary>
        /// <param name="baseCurrencyCode">The currency to base the result rate on</param>
        /// <param name="currencyCode">The target currency to get the rate for</param>
        /// <returns>The rate as an object retrieved from the server.</returns>
        Rate GetRate(string baseCurrencyCode, string currencyCode);

        /// <summary>
        /// Retrieves the exchange rate for the given currency using the public facade.
        /// </summary>
        /// <param name="baseCurrencyCode">The currency to base the result rate on</param>
        /// <param name="currencyCode">The target currency to get the rate for</param>
        /// <returns>The rate as an object retrieved from the server.</returns>
        Task<Rate> GetRateAsync(string baseCurrencyCode, string currencyCode);

        /// <summary>
        /// Retrieves the caller's ledgers for each currency with summary.
        /// </summary>
        /// <returns>A list of ledger objects retrieved from the server.</returns>
        List<Ledger> GetLedgers();

        /// <summary>
        /// Retrieves the caller's ledgers for each currency with summary.
        /// </summary>
        /// <returns>A list of ledger objects retrieved from the server.</returns>
        Task<List<Ledger>> GetLedgersAsync();

        /// <summary>
        /// Retrieve a list of ledger entries by date range using the merchant facade.
        /// </summary>
        /// <param name="currency">The three digit currency string for the ledger to retrieve.</param>
        /// <param name="dateStart">The start date for the query.</param>
        /// <param name="dateEnd">The end date for the query.</param>
        /// <returns>A list of invoice objects retrieved from the server.</returns>
        Ledger GetLedger(String currency, DateTime? dateStart = null, DateTime? dateEnd = null);

        /// <summary>
        /// Retrieve a list of ledger entries by date range using the merchant facade.
        /// </summary>
        /// <param name="currency">The three digit currency string for the ledger to retrieve.</param>
        /// <param name="dateStart">The start date for the query.</param>
        /// <param name="dateEnd">The end date for the query.</param>
        /// <returns>A list of invoice objects retrieved from the server.</returns>
        Task<Ledger> GetLedgerAsync(String currency, DateTime? dateStart = null, DateTime? dateEnd = null);

        Task<Bitpay.AccessToken[]> GetAccessTokensAsync();

        /// <summary>
        /// Test access to a facade
        /// </summary>
        /// <param name="facade">The facade</param>
        /// <returns>True authorized, else false</returns>
        bool TestAccess(Facade facade);

        /// <summary>
        /// Test access to a facade
        /// </summary>
        /// <param name="facade">The facade</param>
        /// <returns>True if authorized, else false</returns>
        Task<bool> TestAccessAsync(Facade facade);

        Task<Bitpay.AccessToken> GetAccessTokenAsync(Facade facade);
    }
}
