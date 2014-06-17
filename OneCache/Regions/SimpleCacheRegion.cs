namespace OneCache.Regions
{
	internal class SimpleCacheRegion : ICacheRegion
	{
		private readonly string _key;

		public SimpleCacheRegion(string key)
		{
			_key = key;
		}

		public string RegionKey()
		{
			return _key;
		}

		public override string ToString()
		{
			return _key;
		}

		public static implicit operator string(SimpleCacheRegion src)
		{
			return src == null ? null : src.RegionKey();
		}
	}
}