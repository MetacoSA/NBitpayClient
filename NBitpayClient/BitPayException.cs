using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NBitpayClient
{
	/// <summary>
	/// Provides an API specific exception handler.
	/// </summary>
	public class BitPayException : Exception
	{
		private string _message = "Exception information not provided";
		private Exception _inner = null;

		/// <summary>
		/// Constructor.  Creates an empty exception.
		/// </summary>
		public BitPayException()
		{
		}

		/// <summary>
		/// Constructor.  Creates an exception with a message only.
		/// </summary>
		/// <param name="message">The message text for the exception.</param>
		public BitPayException(string message) : base(message)
		{
			_message = message;
		}

		/// <summary>
		/// Constructor.  Creates an exception with a message and root cause exception.
		/// </summary>
		/// <param name="message">The message text for the exception.</param>
		/// <param name="inner">The root cause exception.</param>
		public BitPayException(string message, Exception inner) : base(message, inner)
		{
			_message = message;
			_inner = inner;
		}

		public override string Message
		{
			get
			{
				return String.Join(Environment.NewLine, new[] { base.Message }.Concat(_Errors).ToArray());
			}
		}

		public ICollection<string> Errors
		{
			get
			{
				return _Errors;
			}
		}

		List<string> _Errors = new List<string>();
		internal void AddError(string errorMessage)
		{
			_Errors.Add(errorMessage);
		}
	}
}
