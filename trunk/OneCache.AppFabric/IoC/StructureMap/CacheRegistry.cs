using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using log4net;
using Microsoft.ApplicationServer.Caching;
using OneCache.Infrastructure.IoC;

namespace OneCache.AppFabric.IoC.StructureMap
{
	public sealed class ClientCacheConfiguration
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cacheName">The name of the cache</param>
		/// <param name="clientId">The client Id</param>
		public ClientCacheConfiguration(string cacheName, string clientId)
		{
			ClientId = clientId;
			CacheName = cacheName;
			EndPoints=new List<EndPointConfiguration>();
		}

		/// <summary>
		/// The name of the cache
		/// </summary>
		public string CacheName { get; private set; }

		/// <summary>
		/// The client Id
		/// </summary>
		/// <example>MyProduct_1.0.0</example>
		public string ClientId { get; private set; }

		/// <summary>
		/// AppFabric endpoints.
		/// </summary>
		/// <remarks>Only used if the config section dataCacheClient is not present</remarks>
		public List<EndPointConfiguration> EndPoints { get; private set; } 

	}

	public sealed class EndPointConfiguration
	{
		public EndPointConfiguration(string hostName, int port)
		{
			Port = port;
			HostName = hostName;
		}

		public string HostName { get; private set; }
		public int Port { get; private set; }
	}

	public sealed class CacheRegistry : StructureMapBaseRegistry
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (CacheRegistry));

		public CacheRegistry(ClientCacheConfiguration configuration)
		{
			try
			{
				

				For<IDistributedCache>()
					.Singleton()
					.Use<OneCache.DistributedCache>()
					.Ctor<string>("cacheName")
					.Is(configuration.CacheName)
					.Ctor<string>("productInstanceName")
					.Is(configuration.ClientId);

				For<DataCacheFactoryConfiguration>()
					.ConditionallyUse(
						x =>
						{
							x.TheDefault
								.Is.ConstructedBy(
									c =>
									{
										Log.Info("No App Fabric hosts configured in config file. Using endpoints provided in registry configuration.");
										if(configuration.EndPoints.Count==0) throw new InvalidOperationException("Configure at least on default endpoint");
										return new DataCacheFactoryConfiguration
										{
											Servers = configuration.EndPoints.Select(ep => new DataCacheServerEndpoint(ep.HostName, ep.Port)).ToList(),
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
				Log.Fatal("cctor - Could not define the injections", ex);
				throw;
			}
		}
	}
}