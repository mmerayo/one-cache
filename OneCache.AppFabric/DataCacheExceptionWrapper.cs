﻿using System;
using System.Runtime.Serialization;
using Microsoft.ApplicationServer.Caching;

namespace OneCache.AppFabric
{
	internal class DataCacheExceptionWrapper : CacheException
	{
		private readonly DataCacheException _innerException;

		internal DataCacheExceptionWrapper() 
		{
		}

		public DataCacheExceptionWrapper(SerializationInfo info,StreamingContext context):base(info,context){}

		public DataCacheExceptionWrapper(DataCacheException innerException) :
			base(innerException.Message, innerException)
		{
			_innerException = innerException;
		}

		public virtual int ErrorCode
		{
			get { return _innerException.ErrorCode; }
		}
	}
}