using System;
using System.Collections.Generic;
using System.Text;

namespace NBitpayClient.Tests
{
	public class AssertException : Exception
	{
		public AssertException(string message) : base(message)
		{

		}
	}
	public class Assert
	{
		internal static void NotNull<T>(T obj) where T : class
		{
			if(obj == null)
				throw new AssertException("Should not be null");
		}

		internal static void True(bool v)
		{
			if(!v)
				throw new AssertException("Should be true");
		}
	}
}
