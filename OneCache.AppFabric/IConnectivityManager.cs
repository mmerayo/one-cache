namespace OneCache.AppFabric
{
	internal interface IConnectivityManager
	{
		bool CheckIsAvailable();
		void NotifyUnavailability();
	}
}