using System;
using System.Configuration;
using Microsoft.ApplicationServer.Caching;
using StructureMap.Configuration.DSL;
using log4net;


namespace Common.DistributedCaching.AppFabric
{
	public class CachingRegistry : Registry
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(CachingRegistry));

		public CachingRegistry()
		{
			try
			{
				//TODO:
				var productName = "TODO_INSTANCE_UNIQUE_PREFIX";
				var productInstanceName = "TODO_ONLY_PRODUCT_NAME_IS_NEEDED";

				For<IDistributedCache>()
					.Singleton()
					.Use<DistributedCaching.DistributedCache>()
					.Ctor<string>("cacheName")
					.Is(productName)
					.Ctor<string>("productInstanceName")
					.Is(productInstanceName);
				
				For<DataCacheFactoryConfiguration>()
					.ConditionallyUse(
						x =>
							{
								x.TheDefault
								 .Is.ConstructedBy(
									 c =>
										 {
											 _log.Info("No App Fabric hosts configured in config file. Defaulting to localhost.");

											 return new DataCacheFactoryConfiguration
												 {
													 Servers = new[] {new DataCacheServerEndpoint("localhost", 22233)},
													 TransportProperties =
														 {
															 ChannelInitializationTimeout = new TimeSpan(0, 0, 0, 3)
														 }
												 };
										 });

								x.If(c => ConfigurationManager.GetSection("dataCacheClient") != null)
								 .ThenIt.IsThis(
									 new DataCacheFactoryConfiguration() /*Let the API read the config from the config file section.*/
									);
							}
					);
				For<ICacheConfiguration<DataCacheFactoryConfiguration>>()
					.Use<CacheConfiguration>();

				For<IDistributedCacheFactory>()
					.Singleton()
					.Use<DistributedCacheFactory>();
			}
			catch (Exception ex)
			{
				_log.Fatal("cctor - Could not define the injections",ex);
				throw;
			}
		}
	}
}