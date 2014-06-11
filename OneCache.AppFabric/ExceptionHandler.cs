using System.Threading;
using Microsoft.ApplicationServer.Caching;
using log4net;

namespace OneCache.AppFabric
{
	internal sealed class ExceptionHandler : IExceptionHandler
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ExceptionHandler));

		private readonly OperationExecutionContext _context;
		
		private int _retries = 2;
		const int MillisecondsRetry = 100;


		public ExceptionHandler(OperationExecutionContext context)
		{
			_context = context;
			
		}

		public HandleExceptionResult Handle(DataCacheExceptionWrapper exception)
		{
			if (_retries-- == 0)
				return new HandleExceptionResult(false, true);

			switch (exception.ErrorCode)
			{
				case DataCacheErrorCode.KeyDoesNotExist:
					return new HandleExceptionResult(false, false);

				case DataCacheErrorCode.RegionDoesNotExist:
					CreateRegion();
					return new HandleExceptionResult(true, false);

				case DataCacheErrorCode.RegionAlreadyExists:
					break;

				case DataCacheErrorCode.Timeout://AppFabric raises a timeout in some cases like when using a non alphanumeric region
				case DataCacheErrorCode.ConnectionTerminated:
					Thread.Sleep(MillisecondsRetry);
					return new HandleExceptionResult(true, false);
				
				case DataCacheErrorCode.RetryLater:
					_context.ConnectivityManager.NotifyUnavailability();
					return new HandleExceptionResult(false, true);
				default: //those that are not handled will need to have a custom compensation mechanism, see CreateRegion() as is the only one implemented here
					Logger.ErrorFormat("Unhandled AppFabric Exception. This could suggest to implement a new concrete handler: {0}", exception);
					break;
			}
			return new HandleExceptionResult(false, true);
		}

		private bool CreateRegion()
		{
			try
			{
				_context.Cache.CreateRegion(_context.RegionName);
				return true;
			}
			catch (DataCacheExceptionWrapper e)
			{
				Handle(e);
				return false;
			}
		}

	}
}