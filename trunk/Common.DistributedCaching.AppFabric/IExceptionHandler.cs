namespace Common.DistributedCaching.AppFabric
{
	internal interface IExceptionHandler
	{
		HandleExceptionResult Handle(DataCacheExceptionWrapper exception);
	}
}