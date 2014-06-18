using System;

namespace OneCache
{
	public abstract class CacheException : Exception
	{
		protected CacheException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected CacheException()
		{
		}
	}
}