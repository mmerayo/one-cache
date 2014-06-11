using System;

namespace Common.DistributedCaching
{
	internal interface IDistributedCacheFactory : IDisposable
	{
		IDistributedCache GetCache(string cacheName);
	}
}