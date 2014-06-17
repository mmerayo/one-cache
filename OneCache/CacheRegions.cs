namespace OneCache
{
	//TODO: INJECTED non static
	//TODO: document all public classes
	//TODO: ADD GENDARME RULES
	//	TODO: THIS SHOULD BE A PROVIDER
	public static class CacheRegions
	{
		public static ICacheRegion ByEnum<TEnum>(TEnum enumValue)
			where TEnum : struct
		{
			//THIS SMELLS BADLY
			return CacheRegionsStoredByEnum<TEnum>.GetOrCreateRegion(enumValue);
		}
	}
}