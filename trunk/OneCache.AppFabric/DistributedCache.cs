using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.ApplicationServer.Caching;

namespace OneCache.AppFabric
{
	internal sealed class DistributedCache : IDistributedCache
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (DistributedCache));

		private readonly DataCacheWrapper _cacheWrapper;
		private readonly IConnectivityManager _connectivityManager;

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

			OperationExecutionContext context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper,
				RegionKey(region));
			var executor = new OperationExecutor<DataCacheItemVersion>(context);
			executor.Execute(() => _cacheWrapper.Add(key, value, RegionKey(region)));
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

			OperationExecutionContext context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper,
				RegionKey(region));
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

		public IEnumerable<KeyValuePair<string,object>> BulkGet(IEnumerable<string> keys, ICacheRegion region) 
		{
			if (region == null) throw new ArgumentNullException("region");
			Log.DebugFormat("BulkGet: region={0}", region);

			IEnumerable<KeyValuePair<string,object>> value;

			TryBulkGet(keys, region, out value);

			return value;
		}

		public IEnumerable<KeyValuePair<string, T>> BulkGet<T>(IEnumerable<string> keys, ICacheRegion region)
		{
			Log.DebugFormat("BulkGet<{1}>: region={0}", region,typeof(T));
			var candidates=BulkGet(keys, region);

			return
				candidates.Where(x => x.Value is T).Select(x => new KeyValuePair<string, T>(x.Key, (T) x.Value));
		}


		public bool TryGet<T>(string key, ICacheRegion region, out T value) where T : class
		{
			if (region == null) throw new ArgumentNullException("region");
			Log.DebugFormat("TryGet: key={0}, region={1}", key, region);
			object storedValue = null;

			if (_connectivityManager.CheckIsAvailable())
			{
				OperationExecutionContext context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper,
					RegionKey(region));
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

			value = (T) storedValue;
			return true;
		}

		public bool TryBulkGet(IEnumerable<string> keys, ICacheRegion region,
			out IEnumerable<KeyValuePair<string, object>> result)
		{
			if (region == null) throw new ArgumentNullException("region");
			Log.DebugFormat("TryBulkGet: region={0}", region);
			result = null;

			if (!_connectivityManager.CheckIsAvailable())
			{
				Log.Warn("AppFabric is not available");
				return false;
			}
			OperationExecutionContext context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper,
				RegionKey(region));
			var executor = new OperationExecutor<IEnumerable<KeyValuePair<string, object>>>(context);
			result = executor.Execute(() => _cacheWrapper.BulkGet(keys, RegionKey(region)));
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

			OperationExecutionContext context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper,
				RegionKey(region));
			var executor = new OperationExecutor<bool>(context);
			return
				executor.Execute(() => region != null ? _cacheWrapper.Remove(key, RegionKey(region)) : _cacheWrapper.Remove(key));
		}

		public bool ClearRegion(ICacheRegion region)
		{
			if (region == null) throw new ArgumentNullException("region");
			Log.InfoFormat("ClearRegion: region={0}", region);

			if (!_connectivityManager.CheckIsAvailable())
			{
				Log.Warn("AppFabric is not available");
				return false;
			}

			OperationExecutionContext context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper,
				RegionKey(region));
			var executor = new OperationExecutor<bool>(context);
			return executor.Execute(() => _cacheWrapper.ClearRegion(RegionKey(region)));
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

			OperationExecutionContext context = OperationExecutionContext.Create(_connectivityManager, _cacheWrapper,
				RegionKey(region));
			var executor = new OperationExecutor<bool>(context);
			return executor.Execute(() => _cacheWrapper.RemoveRegion(RegionKey(region)));
		}


		public void Dispose()
		{
		}

		private static string RegionKey(ICacheRegion region)
		{
			return region == null ? null : region.RegionKey();
		}
	}
}