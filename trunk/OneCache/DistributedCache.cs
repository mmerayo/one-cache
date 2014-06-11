using System;
using System.Threading;
using log4net;

namespace OneCache
{
	internal sealed class DistributedCache : IDistributedCache
	{
		private readonly ILog _log;
		private readonly string _cacheName;
		private IDistributedCacheFactory _factory;
		private volatile IDistributedCache _cacheInstance = null;
		private static readonly TimeSpan TimeSpanZero = new TimeSpan(0);
		
		private readonly object _syncLock = new object();

		public DistributedCache(string cacheName, string productInstanceName, IDistributedCacheFactory factory)
		{
			if (factory == null) throw new ArgumentNullException("factory");
			if (string.IsNullOrEmpty(productInstanceName)) throw new ArgumentNullException("productInstanceName");
			if (string.IsNullOrEmpty(cacheName)) throw new ArgumentNullException("cacheName");

			_log = LogManager.GetLogger(string.Format("{0}.{1}.{2}", typeof (DistributedCache), productInstanceName, cacheName));

			_cacheName = cacheName;
			_factory = factory;
		}

		private IDistributedCache GetCache()
		{
			if (_cacheInstance == null)
				lock (_syncLock)
					if (_cacheInstance == null)
						try
						{
							_cacheInstance = _factory.GetCache(_cacheName);
						}
						catch (InvalidOperationException)
						{
							int tries = 5;
							while (_cacheInstance==null && tries-- > 0)
								try
								{
									_cacheInstance = _factory.GetCache(_cacheName);
								}
								catch
								{
									Thread.Sleep(TimeSpan.FromSeconds(2));
								}
						}
						catch
						{
							_cacheInstance = null;
						}
			return _cacheInstance;
		}

		public void Add(string key, ICacheRegion region, object value)
		{
			Add(key, region, value, TimeSpan.MinValue);
		}

		public void Add(string key, ICacheRegion region, object value, TimeSpan expirationTime)
		{
			_log.DebugFormat("Add - key={0}, value={1}, region={2}", key, value, region);

			ThrowIfDisposed();

			if (value != null)
			{
				var distributedCache = GetCache();
				if (distributedCache == null)
					return;
				try
				{
					if (expirationTime < TimeSpanZero)
						distributedCache.Add(key, region, value);
					else
						distributedCache.Add(key, region, value, expirationTime);
				}
				catch (Exception e)
				{
					_log.ErrorFormat("Exception during distributedCache.Add for key={0}, region={1} Exception={2}",
					                 key, region, e);
				}
			}
			else
				Remove(key, region);
		}

		public T Get<T>(string key, ICacheRegion region) where T : class
		{
			T value;

			TryGet(key, region, out value);

			return value;
		}

		public bool TryGet<T>(string key, ICacheRegion region, out T value) where T : class
		{
			_log.DebugFormat("TryGet - key={0}, region={1}", key, region);

			ThrowIfDisposed();

			value = default(T);

			var distributedCache = GetCache();
			if (distributedCache == null)
				return false;
			
			try
			{
				value = distributedCache.Get<T>(key, region);

				return value != default(T);
			}
			catch (Exception e)
			{
				_log.ErrorFormat("Exception during distributedCache.Get for key={0}, region={1} Exception={2}",
				                 key,
				                 region,
				                 e);
				return false;
			}
		}

		public bool Remove(string key, ICacheRegion region = null)
		{
			_log.DebugFormat("Remove - key={0}, region={1}", key, region==null ? "null"  :region.ToString());

			ThrowIfDisposed();
			var distributedCache = GetCache();
			if (distributedCache == null)
				return false;
			try
			{
				return distributedCache.Remove(key, region);
			}
			catch (Exception e)
			{
				_log.ErrorFormat("Exception during distributedCache.Remove for key={0}, region={1} Exception={2}",
				                 key,
				                 region,
				                 e);
				return false;
			}

		}

		public bool RemoveRegion(ICacheRegion region)
		{
			_log.DebugFormat("RemoveRegion - region={0}", region);

			ThrowIfDisposed();
			var distributedCache = GetCache();
			if (distributedCache == null)
				return false;
			try
			{
				return distributedCache.RemoveRegion(region);
			}
			catch (Exception e)
			{
				_log.ErrorFormat("Exception during distributedCache.RemoveRegion for region={0}, Message={1}",
				                 region,
				                 e.Message);
				return false;
			}
		}
		
		private void ThrowIfDisposed()
		{
			if (_factory == null)
				throw new ObjectDisposedException(GetType().FullName);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~DistributedCache()
		{
            try{
                Dispose(false);
            }
            catch (Exception ex)
            {
                try
                {
                    _log.Error("Finalizer", ex);
                }
                catch { }
            }
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
				if (_factory != null)
					lock (_syncLock)
						if (_factory != null)
						{
							_factory.Dispose();
							_factory = null;
						}
		}
	}
}
