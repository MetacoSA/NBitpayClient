using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using System.Threading;

namespace NBitpayClient.Tests
{
	public class CustomServer
	{
		TaskCompletionSource<bool> _Evt = null;
		IWebHost _Host = null;
		CancellationTokenSource _Closed = new CancellationTokenSource();
		ulong _Cookie;
		public CustomServer(string bindUrl, ulong cookie)
		{
			_Cookie = cookie;
			_Host = new WebHostBuilder()
				.Configure(app =>
				{
					app.Run(async req =>
					{
						if(req.Request.Path.Value.Contains(cookie.ToString()))
						{
							while(_Act != null)
							{
								Thread.Sleep(10);
								_Closed.Token.ThrowIfCancellationRequested();
							}
							_Act(req);
							_Act = null;
							_Evt.TrySetResult(true);
						}
						req.Response.StatusCode = 200;
					});
				})
				.UseKestrel()
				.UseUrls(bindUrl)
				.Build();
			_Host.Start();
		}


		
		Action<HttpContext> _Act;
		public void ProcessNextRequest(Action<HttpContext> act)
		{
			var source = new TaskCompletionSource<bool>();
			CancellationTokenSource cancellation = new CancellationTokenSource(20000);
			cancellation.Token.Register(() => source.TrySetCanceled());
			source = new TaskCompletionSource<bool>();
			_Evt = source;
			_Act = act;
			try
			{
				_Evt.Task.GetAwaiter().GetResult();
			}
			catch(TaskCanceledException)
			{
				throw new AssertException("Callback to the webserver was expected, check if the callback url is accessible from internet");
			}
		}

		internal void Dispose()
		{
			_Closed.Cancel();
			_Host.Dispose();
		}
	}
}
