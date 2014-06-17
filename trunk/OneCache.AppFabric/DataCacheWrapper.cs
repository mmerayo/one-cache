using System;
using System.Collections.Generic;
using Microsoft.ApplicationServer.Caching;

namespace OneCache.AppFabric
{
	internal class DataCacheWrapper
	{
		private readonly DataCache _realCache;

		public DataCacheWrapper()
		{
		}

		public DataCacheWrapper(DataCache realCache)
		{
			if (realCache == null) throw new ArgumentNullException("realCache");
			_realCache = realCache;
		}

		public virtual object Get(string key)
		{
			return Get(key, null);
		}

		public virtual IEnumerable<KeyValuePair<string, object>> GetObjectsInRegion(string region)
		{
			try
			{
				var result = _realCache.GetObjectsInRegion(region);
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
				object result = region != null ? _realCache.Get(key, region) : _realCache.Get(key);
				return result;
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}


		public virtual IEnumerable<KeyValuePair<string, object>> BulkGet(IEnumerable<string> keys, string region)
		{
			try
			{
				return _realCache.BulkGet(keys, region);
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}

		public virtual DataCacheItemVersion Put(string key, object value, string region)
		{
			try
			{
				DataCacheItemVersion result = _realCache.Put(key, value, region);
				return result;
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}

		public DataCacheItemVersion Put(string key, object value)
		{
			try
			{
				DataCacheItemVersion result = _realCache.Put(key, value);
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
				DataCacheItemVersion result = _realCache.Put(key, value, expirationTime, region);
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

		public virtual DataCacheItemVersion Add(string key, object value)
		{
			try
			{
				return _realCache.Add(key, value);
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
		}

		public virtual DataCacheItemVersion Add(string key, object value, string region)
		{
			try
			{
				return _realCache.Add(key, value, region);
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

		public virtual bool ClearRegion(string regionName)
		{
			try
			{
				_realCache.ClearRegion(regionName);
			}
			catch (DataCacheException e)
			{
				throw new DataCacheExceptionWrapper(e);
			}
			return true;
		}
	}
}