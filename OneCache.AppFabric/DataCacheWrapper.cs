using System;
using Microsoft.ApplicationServer.Caching;

namespace OneCache.AppFabric
{
cover all the calls
	internal class DataCacheWrapper
	{
		private readonly DataCache _realCache;
		public DataCacheWrapper(){}
		public DataCacheWrapper(DataCache realCache)
		{
			if (realCache == null) throw new ArgumentNullException("realCache");
			_realCache = realCache;
		}

		public virtual DataCacheItemVersion Put(string key, object value, string region)
		{
			try
			{
				var result = _realCache.Put(key, value, region);
				return result;
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}

		public virtual DataCacheItemVersion Put(string key, object value, TimeSpan expirationTime, string region)
		{
			try
			{
				var result = _realCache.Put(key, value, expirationTime, region);
				return result;
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}

		public virtual object Get(string key, string region)
		{
			try
			{
				var result = _realCache.Get(key, region);
				return result;
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}

		public virtual bool Remove(string key)
		{
			try
			{
				
				return _realCache.Remove(key);
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}

		public virtual bool Remove(string key, string region)
		{
			try
			{
				return _realCache.Remove(key, region);
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}

		public virtual bool RemoveRegion(string region)
		{
			try
			{
				return _realCache.RemoveRegion(region);
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}

		public virtual void Add(string key, object value)
		{
			try
			{
				_realCache.Add(key, value);
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}

		public virtual bool CreateRegion(string regionName)
		{
			try
			{
				return _realCache.CreateRegion(regionName);
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}
	}
}