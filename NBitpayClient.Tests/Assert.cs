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

		internal static void Equal<T>(T a, T b) where T : class
		{
			if(a != b)
				throw new AssertException("Should be equals");
		}

		internal static void True(bool v)
		{
			if(!v)
				throw new AssertException("Should be true");
		}

		internal static void False(bool v)
		{
			if(v)
				throw new AssertException("Should be false");
		}
	}
}
