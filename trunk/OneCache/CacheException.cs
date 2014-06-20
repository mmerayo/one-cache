using System;
using System.Runtime.Serialization;

namespace OneCache
{
	[Serializable]
	public abstract class CacheException : Exception
	{
		protected CacheException(string message, Exception innerException) : base(message, innerException)
		{
		}
		
		protected CacheException(SerializationInfo info, StreamingContext context):base(info,context)
		{
		}

		protected CacheException(){}
	}
}