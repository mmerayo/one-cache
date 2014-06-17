using System;
using System.Threading.Tasks;
using log4net;

namespace OneCache.Infrastructure
{
	public static class TaskExceptionsLogger
	{
		public static Task LogTaskException(this Task theTask, ILog theLogger, string logPrefix = "")
		{
			return theTask.LogTaskException(theLogger.Error);
		}


		public static Task LogTaskException(this Task theTask, Action<string> logExecutor, string logPrefix = "")
		{
			if (logExecutor == null) throw new ArgumentNullException("logExecutor");

			theTask.ContinueWith(task =>
			{
				var aggException = task.Exception.Flatten();
				foreach (var exception in aggException.InnerExceptions)
					logExecutor(string.Format("{0} - There was an exception while executing the task. Details: {1}", logPrefix,
						exception));
			}, TaskContinuationOptions.OnlyOnFaulted);
			return theTask;
		}
	}
}