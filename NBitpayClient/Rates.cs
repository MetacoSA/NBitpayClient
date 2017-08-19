using System;
using System.Collections.Generic;
using System.Net.Http;

namespace NBitpayClient
{
    /// <summary>
    /// Provides an interface to the BitPay server to obtain exchange rate information.
    /// </summary>
    public class Rates
    {
        private List<Rate> _rates;

        public Rates(List<Rate> rates)
        {
            _rates = rates;
        }
	
		public List<Rate> AllRates
		{
			get
			{
				return _rates;
			}
		}

        public decimal GetRate(string currencyCode)
        {
		    decimal val = 0;
		    foreach (Rate rateObj in _rates)
            {
			    if (rateObj.Code.Equals(currencyCode))
                {
                    val = rateObj.Value;
                    break;
			    }
		    }
    		return val;
	    }
    }
}
