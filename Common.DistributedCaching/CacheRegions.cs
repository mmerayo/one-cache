namespace Common.DistributedCaching
{
	public static class CacheRegions
	{
		public static ICacheRegion ByEnum<TEnum>(TEnum enumValue)
			where TEnum : struct
		{
			return CacheRegionsStoredByEnum<TEnum>.GetOrCreateRegion(enumValue);
		}
	}
}