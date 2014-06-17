using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneCache.Regions;
using StructureMap.Configuration.DSL;

namespace OneCache.Infrastructure.IoC
{
	public abstract class StructureMapBaseRegistry:Registry
	{
		protected StructureMapBaseRegistry()
		{
			For<ICacheRegionProvider>().Singleton().Use<CacheRegionProvider>();
		}
	}
}
