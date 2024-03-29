﻿using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using OneCache.Infrastructure;

namespace OneCache.AppFabric
{
	internal sealed class ConnectivityManager : IConnectivityManager
	{
		private static readonly object SyncLock = new object();
		private static readonly TimeSpan TimeBetweenChecks = TimeSpan.FromSeconds(30);

		private static readonly ILog Logger = LogManager.GetLogger(typeof (ConnectivityManager));
		private readonly DataCacheWrapper _cacheWrapper;
		private readonly Action _keepAliveAction;
		private volatile bool _isAvailable = true;

		private bool _isFirstRequest = true;
		private DateTime _lastCheck = DateTime.MinValue;

		public ConnectivityManager(DataCacheWrapper cacheWrapper) : this(cacheWrapper, null)
		{
		}

		public ConnectivityManager(DataCacheWrapper cacheWrapper, Action onDoKeepAlive)
		{
			if (cacheWrapper == null) throw new ArgumentNullException("cacheWrapper");
			_cacheWrapper = cacheWrapper;
			_keepAliveAction = onDoKeepAlive ?? DoKeepAlive;
		}

		public void NotifyUnavailability()
		{
			_isAvailable = false;
		}

		public bool CheckIsAvailable()
		{
			if (!_isAvailable)
				if (DateTime.UtcNow.Subtract(_lastCheck) > TimeBetweenChecks)
					lock (SyncLock)
						if (DateTime.UtcNow.Subtract(_lastCheck) > TimeBetweenChecks)
						{
							try
							{
								Task.Factory.StartNew(PerformKeepAlive).LogTaskException(Logger);
							}
							catch (Exception ex)
							{
								Logger.Error("CheckIsAvailable - Could not perform AppFabric availability", ex);
							}

							//The component tries always to process the first request since it was loaded in the application domain
							if (_isFirstRequest)
							{
								int numRetriesFirstConnection = 10;
								while (!_isAvailable && numRetriesFirstConnection-- > 0)
									Thread.Sleep(TimeSpan.FromMilliseconds(250));

								_isFirstRequest = false;
							}
							_lastCheck = DateTime.UtcNow;
						}

			return _isAvailable;
		}

		private void PerformKeepAlive()
		{
			try
			{
				Logger.Debug("PerformKeepAlive");
				_keepAliveAction();
				_isAvailable = true;
			}
			catch (Exception ex)
			{
				NotifyUnavailability();
				Logger.Warn("PerformKeepAlive - AppFabric is not available", ex);
			}
		}

		private void DoKeepAlive()
		{
			var cacheKey = "PerformKeepAlive" + DateTime.UtcNow.Ticks;
			_cacheWrapper.Add(cacheKey, new object());
			_cacheWrapper.Remove(cacheKey);
		}
	}
}