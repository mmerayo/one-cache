using System.Collections.Generic;

namespace OneCache.AppFabric.Configuration
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
}