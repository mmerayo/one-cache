using System;

namespace OneCache
{
	internal interface IDistributedCacheFactory : IDisposable
	{
		IDistributedCache GetCache(string cacheName);
	}
}