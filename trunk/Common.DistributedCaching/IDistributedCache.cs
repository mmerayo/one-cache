using System;

namespace Common.DistributedCaching
{
	public interface IDistributedCache : IDisposable
	{
		void Add(string key, ICacheRegion region, object value);
		void Add(string key, ICacheRegion region, object value, TimeSpan expirationTime);
		T Get<T>(string key, ICacheRegion region) where T : class;
		bool TryGet<T>(string key, ICacheRegion region, out T value) where T : class;
		bool Remove(string key, ICacheRegion region = null);
		bool RemoveRegion(ICacheRegion region);
	}
}