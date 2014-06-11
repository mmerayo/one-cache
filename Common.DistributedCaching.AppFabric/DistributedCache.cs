using System;
using log4net;
using Microsoft.ApplicationServer.Caching;

namespace Common.DistributedCaching.AppFabric
{
	internal sealed class DistributedCache : IDistributedCache
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(DistributedCache));

		private readonly DataCacheWrapper _cacheWrapper;
		private readonly IConnectivityManager _connectivityManager;

		private static string RegionKey(ICacheRegion region)
		{
			return region == null ? null : region.RegionKey();
		}

		public DistributedCache(DataCacheWrapper cacheWrapper, IConnectivityManager connectivityManager)
		{
			if (cacheWrapper == null) throw new ArgumentNullException("cacheWrapper");
			if (connectivityManager == null) throw new ArgumentNullException("connectivityManager");
			_cacheWrapper = cacheWrapper;
			_connectivityManager = connectivityManager;
		}

		public void Add(string key, ICacheRegion region, object value)
		{
			if (region == null) throw new ArgumentNullException("region");
			Log.DebugFormat("Add: key={0}, value={1}, region={2}", key, value, region);

			if (!_connectivityManager.CheckIsAvailable())
			{
				Log.Warn("AppFabric is not available");
				return;
			}

			var context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper, RegionKey(region));
			var executor = new OperationExecutor<DataCacheItemVersion>(context);
			executor.Execute(() => _cacheWrapper.Put(key, value, RegionKey(region)));
		}


		public void Add(string key, ICacheRegion region, object value, TimeSpan expirationTime)
		{
			if (region == null) throw new ArgumentNullException("region");
			Log.DebugFormat("Add: key={0}, value={1}, timeout={2}, region={3}", key, value, expirationTime, region);

			if (!_connectivityManager.CheckIsAvailable())
			{
				Log.Warn("AppFabric is not available");
				return;
			}

			var context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper, RegionKey(region));
			var executor = new OperationExecutor<DataCacheItemVersion>(context);
			executor.Execute(() => _cacheWrapper.Put(key, value, expirationTime, RegionKey(region)));
		}

		public T Get<T>(string key, ICacheRegion region) where T : class
		{
			if (region == null) throw new ArgumentNullException("region");
			Log.DebugFormat("Get: key={0}, region={1}", key, region);

			T value;

			TryGet(key, region, out value);

			return value;
		}

		public bool TryGet<T>(string key, ICacheRegion region, out T value) where T : class
		{
			if (region == null) throw new ArgumentNullException("region");
			Log.DebugFormat("TryGet: key={0}, region={1}", key, region);
			object storedValue = null;

			if (_connectivityManager.CheckIsAvailable())
			{
				var context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper, RegionKey(region));
				var executor = new OperationExecutor<object>(context);
				storedValue = executor.Execute(() => _cacheWrapper.Get(key, RegionKey(region)));
			}
			else
			{
				Log.Warn("AppFabric not available");
			}
			if (storedValue == null)
			{
				value = default(T);
				return false;
			}

			value = (T)storedValue;
			return true;
		}

		public bool Remove(string key, ICacheRegion region = null)
		{
			Log.InfoFormat("Remove: key={0}, region={1}", key, region);
			if (!_connectivityManager.CheckIsAvailable())
			{
				Log.Warn("AppFabric is not available");
				return false;
			}

			var context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper, RegionKey(region));
			var executor = new OperationExecutor<bool>(context);
			return executor.Execute(() => region != null ? _cacheWrapper.Remove(key, RegionKey(region)) : _cacheWrapper.Remove(key));
		}

		public bool RemoveRegion(ICacheRegion region)
		{
			if (region == null) throw new ArgumentNullException("region");
			Log.InfoFormat("RemoveRegion: region={0}", region);

			if (!_connectivityManager.CheckIsAvailable())
			{
				Log.Warn("AppFabric is not available");
				return false;
			}

			var context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper, RegionKey(region));
			var executor = new OperationExecutor<bool>(context);
			return executor.Execute(() => _cacheWrapper.RemoveRegion(RegionKey(region)));

		}
		public void Dispose() { }
	}

}
