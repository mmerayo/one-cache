namespace OneCache.AppFabric
{
	internal sealed class OperationExecutionContext
	{
		private OperationExecutionContext(IConnectivityManager connectivityManager, DataCacheWrapper cache,
			string regionName = null)
		{
			ConnectivityManager = connectivityManager;
			RegionName = regionName;
			Cache = cache;
		}

		public IConnectivityManager ConnectivityManager { get; private set; }
		public string RegionName { get; private set; }
		public DataCacheWrapper Cache { get; private set; }

		public static OperationExecutionContext Create(IConnectivityManager connectivityManager, DataCacheWrapper cache,
			string regionName)
		{
			return new OperationExecutionContext(connectivityManager, cache, regionName);
		}
	}
}