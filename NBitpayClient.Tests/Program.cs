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
using Newtonsoft.Json;
using NBitpayClient.Extensions;

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
				new Program().TestContextFree();
				RPC = new RPCClient(RPCAuth, "http://localhost:" + Network.RPCPort, Network);
				RPC.GetBalance();
				Logs.Tests.LogInformation("Can connect to RPC");
				var cookie = RandomUtils.GetUInt64();
				CallbackUri = "http://" + GetExternalIp() + ":" + serverPort + "/" + cookie;
				Logs.Tests.LogInformation("Callback url used is " + CallbackUri);
				Server = new CustomServer("http://0.0.0.0:" + serverPort + "/", cookie);
				new Program().Run();
				//Console.ReadLine();
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

		private void TestContextFree()
		{
			CanSignAndCheckSig();
			CanSerializeDeserialize();
		}

		private void CanSignAndCheckSig()
		{
			var key = new Key();
			string uri = "http://toto:9393/";
			string content = "blah";
			var sig = key.GetBitIDSignature(uri, content);
			Assert.NotNull(sig);
			Assert.True(key.PubKey.CheckBitIDSignature(sig, uri, content));
			Assert.False(key.PubKey.CheckBitIDSignature(sig, uri + "1", content));
			Assert.False(key.PubKey.CheckBitIDSignature(sig, uri, content + "1"));

			content = null;
			sig = key.GetBitIDSignature(uri, content);
			Assert.True(key.PubKey.CheckBitIDSignature(sig, uri, content));
			Assert.True(key.PubKey.CheckBitIDSignature(sig, uri, string.Empty));
			Assert.False(key.PubKey.CheckBitIDSignature(sig, uri, "1"));
		}

		private void CanSerializeDeserialize()
		{
			var str = "{\"id\":\"NzzNUB5DEMLP5q95szL1VS\",\"url\":\"https://test.bitpay.com/invoice?id=NzzNUB5DEMLP5q95szL1VS\",\"posData\":\"posData\",\"status\":\"paid\",\"btcPrice\":\"0.001246\",\"price\":5,\"currency\":\"USD\",\"invoiceTime\":1503140597709,\"expirationTime\":1503141497709,\"currentTime\":1503140607752,\"btcPaid\":\"0.001246\",\"btcDue\":\"0.000000\",\"rate\":4012.12,\"exceptionStatus\":false,\"buyerFields\":{}}";
			var notif = JsonConvert.DeserializeObject<InvoicePaymentNotification>(str);
			var serialized = JsonConvert.SerializeObject(notif);
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
				NotificationURL = CallbackUri + "/notification",
				ItemDesc = "Some description",
				FullNotifications = true
			});
			Logs.Tests.LogInformation("Invoice created");
			BitcoinUrlBuilder url = new BitcoinUrlBuilder(invoice.PaymentUrls.BIP21);
			RPC.SendToAddress(url.Address, url.Amount);
			Logs.Tests.LogInformation("Invoice paid");
			var t = Bitpay.GetAccessTokensAsync().Result;
			//Server.ProcessNextRequest((ctx) =>
			//{
			//	var ipn = new StreamReader(ctx.Request.Body).ReadToEnd();
			//	JsonConvert.DeserializeObject<InvoicePaymentNotification>(ipn); //can deserialize
			//});
			var invoice2 = Bitpay.GetInvoice(invoice.Id, Facade.PointOfSale);
			Assert.NotNull(invoice2);
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
					if(Bitpay.TestAccess(Facade.Merchant))
						return;
				}
				catch { }
			}

			k = k ?? new BitcoinSecret(new Key(), Network);
			File.WriteAllText(keyFile, k.ToString());

			Bitpay = new Bitpay(k.PrivateKey, BitPayUri);
			var pairing = Bitpay.RequestClientAuthorization(Facade.Merchant);


			throw new AssertException("You need to approve the test key to access bitpay by going to this link " + pairing.CreateLink(Bitpay.BaseUrl).AbsoluteUri);
		}
	}
}
