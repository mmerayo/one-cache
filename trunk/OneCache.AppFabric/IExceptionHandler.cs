namespace OneCache.AppFabric
{
	internal interface IExceptionHandler
	{
		HandleExceptionResult Handle(DataCacheExceptionWrapper exception);
	}
}