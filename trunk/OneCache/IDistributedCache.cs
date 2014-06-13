using System;
using System.Collections.Generic;

namespace OneCache
{
	public interface IDistributedCache : IDisposable
	{
		void Add(string key, ICacheRegion region, object value);
		void Add(string key, ICacheRegion region, object value, TimeSpan expirationTime);
		T Get<T>(string key, ICacheRegion region) where T : class;
		bool TryGet<T>(string key, ICacheRegion region, out T value) where T : class;
		bool Remove(string key, ICacheRegion region = null);
		bool RemoveRegion(ICacheRegion region);
		bool ClearRegion(ICacheRegion region);

		bool TryBulkGet(IEnumerable<string> keys, ICacheRegion region,
			out IEnumerable<KeyValuePair<string, object>> result);

		IEnumerable<KeyValuePair<string, object>> BulkGet(IEnumerable<string> keys, ICacheRegion region);
		IEnumerable<KeyValuePair<string, T>> BulkGet<T>(IEnumerable<string> keys, ICacheRegion region);
	}
}