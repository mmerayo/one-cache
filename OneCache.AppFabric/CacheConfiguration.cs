using System;
using Microsoft.ApplicationServer.Caching;

namespace OneCache.AppFabric
{
	internal sealed class CacheConfiguration: ICacheConfiguration<DataCacheFactoryConfiguration>
	{
		public CacheConfiguration(DataCacheFactoryConfiguration configuration):this(configuration,false){}
		internal CacheConfiguration(DataCacheFactoryConfiguration configuration, bool connectOnStartUp)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");
			Object = configuration;
			ConnectOnStartUp = connectOnStartUp;
		}

		public DataCacheFactoryConfiguration Object { get; private set; }
		public bool ConnectOnStartUp { get; private set; }
	}
}