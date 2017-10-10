using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using System;
using System.Collections.Generic;
using System.Text;

namespace NBitpayClient.Extensions
{
	public static class BitIdExtensions
	{
		public static string GetBitIDSIN(this PubKey key)
		{
			return Encoders.Base58Check.EncodeData(Encoders.Hex.DecodeData("0f02" + key.Hash.ToString()));
		}

		public static bool ValidateSIN(this string sin)
		{
			try
			{
				var decoded = Encoders.Base58Check.DecodeData(sin);
				return decoded.Length == 2 + 20 && decoded[0] == 0x0f && decoded[1] == 0x02;
			}
			catch { return false; }
		}

		public static string GetBitIDSignature(this Key key, string uri, string body)
		{
			body = body ?? string.Empty;
			if(key == null)
				throw new ArgumentNullException(nameof(key));
			if(uri == null)
				throw new ArgumentNullException(nameof(uri));
			var hash = new uint256(Hashes.SHA256(Encoding.UTF8.GetBytes(uri + body)));
			return Encoders.Hex.EncodeData(key.Sign(hash).ToDER());
		}

		public static bool CheckBitIDSignature(this PubKey key, string sig, string uri, string body)
		{
			body = body ?? string.Empty;
			if(key == null)
				throw new ArgumentNullException(nameof(key));
			if(sig == null)
				throw new ArgumentNullException(nameof(sig));
			if(uri == null)
				throw new ArgumentNullException(nameof(uri));
			try
			{
				if(!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
					return false;
				var hash = new uint256(Hashes.SHA256(Encoding.UTF8.GetBytes(uri + body)));
				var result = key.Verify(hash, new ECDSASignature(Encoders.Hex.DecodeData(sig)));
				return result;
			}
			catch { return false; }
		}
	}
}
