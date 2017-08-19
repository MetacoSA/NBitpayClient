using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBitpayClient.Tests
{
	public class Logs
	{
		static Logs()
		{
			Configure(new FuncLoggerFactory(n => NullLogger.Instance));
		}
		public static void Configure(ILoggerFactory factory)
		{
			Tests = factory.CreateLogger("Tests");
		}
		public static ILogger Tests
		{
			get; set;
		}
		public const int ColumnLength = 16;
	}

	public class FuncLoggerFactory : ILoggerFactory
	{
		private Func<string, ILogger> createLogger;
		public FuncLoggerFactory(Func<string, ILogger> createLogger)
		{
			this.createLogger = createLogger;
		}
		public void AddProvider(ILoggerProvider provider)
		{

		}

		public ILogger CreateLogger(string categoryName)
		{
			return createLogger(categoryName);
		}

		public void Dispose()
		{

		}
	}
}
