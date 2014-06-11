using System;
using Microsoft.ApplicationServer.Caching;

namespace OneCache.AppFabric
{
	internal class DataCacheExceptionWrapper:Exception
	{

		private readonly DataCacheException _innerException;
		internal DataCacheExceptionWrapper(){}
		public DataCacheExceptionWrapper(DataCacheException innerException):base(innerException.Message,innerException)
		{
			_innerException = innerException;
		}

		public virtual int ErrorCode
		{
			get { return _innerException.ErrorCode; }
		}

	}
}
