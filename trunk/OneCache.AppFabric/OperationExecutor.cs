using System;
using System.Diagnostics;
using log4net;

namespace OneCache.AppFabric
{
	internal sealed class OperationExecutor<TResult>
	{
		private readonly OperationExecutionContext _context;
		
		private static readonly ILog Logger = LogManager.GetLogger("Otu.Services.Caching.AppFabric.AppFabricOperationExecutor");
		private readonly IExceptionHandler _exceptionHandler;
		private bool _canExecuteOperation = true;
		

		public OperationExecutor(OperationExecutionContext context,
			IExceptionHandler exceptionHandler=null)
		{
			if (context == null) throw new ArgumentNullException("context");
			_context = context;
			_exceptionHandler = exceptionHandler ?? new ExceptionHandler(_context);
		}

		public TResult Execute(Func<TResult> operation )
		{
			Logger.DebugFormat("Execute - Action:{0}", operation);
			lock (this)
			{
				if (!_canExecuteOperation)
					throw new NotSupportedException(
						string.Format("Each instance of {0} can only execute one operation.", GetType().FullName));
				_canExecuteOperation = false;
			}


			while (true)
			{
				try
				{
					return operation();
				}
				catch (DataCacheExceptionWrapper ex)
				{
					var res = _exceptionHandler.Handle(ex);
					if (res.MustRetry)
					{
						Logger.DebugFormat("Execute - This is expected. Retrying. Exception:{0}", ex);
						continue;
					}
					if (!res.Rethrow)
						return default(TResult);

					Logger.ErrorFormat("Execute - Rethrowing Action:{0} Exception:{1}", GetCallerFrame(),ex);
					throw;
				}
				catch (Exception ex)
				{
					Logger.ErrorFormat("Execute -  Rethrowing unhandleable exception. Exception: {0}, Action: {1}", ex,GetCallerFrame());
					throw;
				}
			}
		}

		private static string GetCallerFrame()
		{
			var stackFrames = new StackTrace().GetFrames();
			var callerFrame = stackFrames != null && stackFrames.Length >= 3 && stackFrames[2]!=null ? stackFrames[2].ToString() : "null";
			return callerFrame;
		}
	}
}