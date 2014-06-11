namespace Common.DistributedCaching.AppFabric
{
	internal interface IConnectivityManager
	{
		bool CheckIsAvailable();
		void NotifyUnavailability();
	}
}