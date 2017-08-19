using NBitcoin;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using NBitcoin.RPC;
using NBitcoin.Payment;

namespace NBitpayClient.Tests
{
	public class Program
	{


		//Bitpay parameters
		Uri BitPayUri = new Uri("https://test.bitpay.com/");
		static Network Network = Network.TestNet;
		//////////////

		//RPC Testnet settings
		static string RPCAuth = "cookiefile=G:\\Bitcoin\\testnet3\\.cookie"; // or user:pwd or null for default
		/// </summary>

		static CustomServer Server;
		static RPCClient RPC;
		static int serverPort = 38633;
		static string CallbackUri;

		Bitpay Bitpay = null;

		public static void Main(string[] args)
		{
			Logs.Configure(new FuncLoggerFactory(i => new CustomerConsoleLogger(i, (a, b) => true, false)));
			CustomServer server = null;
			try
			{
				RPC = new RPCClient(RPCAuth, "http://localhost:" + Network.RPCPort, Network);
				RPC.GetBalance();
				Logs.Tests.LogInformation("Can connect to RPC");
				var cookie = RandomUtils.GetUInt64();
				CallbackUri = "http://" + GetExternalIp() + ":" + serverPort + "/" + cookie;
				Logs.Tests.LogInformation("Callback url used is " + CallbackUri);
				Server = new CustomServer("http://0.0.0.0:" + serverPort + "/", cookie);
				new Program().Run();
				Logs.Tests.LogInformation("Tests ran successfully");
			}
			catch(AssertException ex)
			{
				Logs.Tests.LogError(ex.Message);
			}
			catch(Exception ex)
			{
				Logs.Tests.LogError(ex.ToString());
			}
			finally
			{
				if(server != null)
					Server.Dispose();
			}
			Console.ReadLine();
		}

		private static IPAddress GetExternalIp()
		{
			using(var http = new HttpClient())
			{
				var ip = http.GetAsync("http://icanhazip.com").Result.Content.ReadAsStringAsync().Result;
				return IPAddress.Parse(ip.Replace("\n", ""));
			}
		}

		private void Run()
		{
			EnsureRegisteredKey();
			CanMakeInvoice();
			CanGetRate();
		}

		private void CanMakeInvoice()
		{
			var invoice = Bitpay.CreateInvoice(new Invoice()
			{
				Price = 5.0,
				Currency = "USD",
				PosData = "posData",
				OrderId = "orderId",
				//RedirectURL = redirect + "redirect",
				NotificationURL = CallbackUri + "notification",
				ItemDesc = "Some description Monethic(s)"
			});
			Logs.Tests.LogInformation("Invoice created");
			BitcoinUrlBuilder url = new BitcoinUrlBuilder(invoice.PaymentUrls.BIP21);
			RPC.SendToAddress(url.Address, url.Amount);
			Logs.Tests.LogInformation("Invoice paid");
			Server.ProcessNextRequest((ctx) =>
			{
			});
		}

		private void CanGetRate()
		{
			var rates = Bitpay.GetRates();
			Assert.NotNull(rates);
			Assert.True(rates.AllRates.Count > 0);
		}

		private void EnsureRegisteredKey()
		{
			if(!Directory.Exists(Network.Name))
				Directory.CreateDirectory(Network.Name);

			BitcoinSecret k = null;
			var keyFile = Path.Combine(Network.Name, "key.env");
			try
			{
				k = new BitcoinSecret(File.ReadAllText(keyFile), Network);
			}
			catch { }

			if(k != null)
			{
				try
				{

					Bitpay = new Bitpay(k.PrivateKey, BitPayUri);
					if(Bitpay.TestAccess(Facade.PointOfSale))
						return;
				}
				catch { }
			}

			k = k ?? new BitcoinSecret(new Key(), Network);
			File.WriteAllText(keyFile, k.ToString());

			Bitpay = new Bitpay(k.PrivateKey, BitPayUri);
			var pairing = Bitpay.RequestClientAuthorization(Facade.PointOfSale);


			throw new AssertException("You need to approve the test key to access bitpay by going to this link " + pairing.CreateLink(Bitpay.BaseUrl).AbsoluteUri);
		}
	}
}
