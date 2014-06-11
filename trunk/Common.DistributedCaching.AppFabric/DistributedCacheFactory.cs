using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common.DistributedCaching.Infrastructure;
using Microsoft.ApplicationServer.Caching;
using log4net;

namespace Common.DistributedCaching.AppFabric
{
	[DebuggerStepThrough]
	internal sealed class DistributedCacheFactory : IDistributedCacheFactory
	{
		private static readonly TimeSpan TimeBetweenChecks = TimeSpan.FromSeconds(30);
		private readonly ICacheConfiguration<DataCacheFactoryConfiguration> _configuration;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (DistributedCacheFactory));
		private DateTime _lastCheck = DateTime.MinValue;
		private volatile DataCacheFactory _factory=null;
		private readonly object _syncLock=new object();
		

		public DistributedCacheFactory(ICacheConfiguration<DataCacheFactoryConfiguration> configuration)
		{
			_configuration = configuration;
			if (configuration == null) throw new ArgumentNullException("configuration");
			var realConfiguration = configuration.Object;

			if (realConfiguration == null)
				throw new InvalidOperationException(string.Format("argument must be of type {0}",
				                                                  typeof (DataCacheFactoryConfiguration).FullName));

			Logger.InfoFormat("Connecting to App Fabric on {0}...", string.Join(", ", realConfiguration.Servers.Select(s => s.HostName + ":" + s.CachePort)));

			if(configuration.ConnectOnStartUp)
				DoGetFactory();
			
		}

		public DistributedCacheFactory(bool connectOnStartUp=false)
			: this(new CacheConfiguration(new DataCacheFactoryConfiguration(),connectOnStartUp))
		{
		}

		private DataCacheFactory GetFactory()
		{
			if (_factory != null)
				return _factory;

			if (DateTime.UtcNow.Subtract(_lastCheck) > TimeBetweenChecks)
				lock (_syncLock)
					if (DateTime.UtcNow.Subtract(_lastCheck) > TimeBetweenChecks)
					{
						try
						{
							Task.Factory.StartNew(DoGetFactory).LogTaskException(Logger);
						}
						catch (Exception ex)
						{
							Logger.Error("GetFactory - Could not get AppFabric factory", ex);
						}
						_lastCheck = DateTime.UtcNow;
					}

			return null;
		}


		private void DoGetFactory()
		{
			try
			{
				if (_factory == null)
					lock(_syncLock)
						if(_factory==null)
							_factory = new DataCacheFactory(_configuration.Object);
			}
			catch (Exception ex)
			{
				Logger.Warn("Could not establish a connection to AppFabric. Please verify the service is UP", ex);
				throw;
			}
		}

		

		private readonly Dictionary<string,DistributedCache> _caches=new Dictionary<string, DistributedCache>();

		public IDistributedCache GetCache(string cacheName)
		{
			ThrowIfDisposed();

			var factory = GetFactory();
			if (factory == null)
				return null;

			if (!_caches.ContainsKey(cacheName))
				lock (_syncLock)
					if (!_caches.ContainsKey(cacheName))
					{
						DataCache realCache;

						try
						{
							realCache = _factory.GetCache(cacheName);
						}
						catch (DataCacheException dcx)
						{
							string error = string.Format("Could not get cache called [{0}] (does it exist?) ", cacheName);
							Logger.Error(error);
							throw new InvalidOperationException(error, dcx);
						}

						var dataCache = new DataCacheWrapper(realCache);
						var connectivityManager = new ConnectivityManager(dataCache);
						var appFabricDistributedCache = new DistributedCache(dataCache, connectivityManager);
						_caches.Add(cacheName, appFabricDistributedCache);
					}
			return _caches[cacheName];
		}

		private bool _disposed = false;

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
		}


		public void Dispose()
		{
			ThrowIfDisposed();

			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~DistributedCacheFactory()
		{
		    try
		    {
		        Dispose(false);
		    }
		    catch (Exception ex)
		    {
		        try
		        {
		            Logger.Error("Finalizer", ex);
		        }catch{}
		    }
		}

		private void Dispose(bool disposing)
		{
			if (disposing)

				if (_factory != null)
				{
					_factory.Dispose();
					_factory = null;
				}
			_disposed = true;
		}
	}
}