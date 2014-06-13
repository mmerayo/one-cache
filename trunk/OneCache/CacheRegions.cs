namespace OneCache
{
	//TODO: INJECTED non static
	//TODO: document all public classes
	//TODO: ADD GENDARME RULES
	public static class CacheRegions
	{
		public static ICacheRegion ByEnum<TEnum>(TEnum enumValue)
			where TEnum : struct
		{
			return CacheRegionsStoredByEnum<TEnum>.GetOrCreateRegion(enumValue);
		}
	}
}