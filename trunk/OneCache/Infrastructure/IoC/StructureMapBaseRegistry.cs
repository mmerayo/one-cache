using OneCache.Regions;
using StructureMap.Configuration.DSL;

namespace OneCache.Infrastructure.IoC
{
	public abstract class StructureMapBaseRegistry : Registry
	{
		protected StructureMapBaseRegistry()
		{
			For<ICacheRegionProvider>().Singleton().Use<CacheRegionProvider>();
		}
	}
}