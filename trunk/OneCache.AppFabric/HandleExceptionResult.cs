namespace OneCache.AppFabric
{
	internal class HandleExceptionResult
	{
		public HandleExceptionResult(bool needRetry, bool rethrow)
		{
			Rethrow = rethrow;
			MustRetry = needRetry;
		}

		public bool MustRetry { get; private set; }
		public bool Rethrow { get; private set; }
	}
}