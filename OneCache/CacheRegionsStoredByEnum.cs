using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using log4net;

namespace OneCache
{
	internal static class CacheRegionsStoredByEnum<TEnum> 
		where TEnum : struct
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CacheRegionsStoredByEnum<TEnum>));

		private static readonly Regex regionRegex = new Regex("[^a-zA-Z0-9 -]", RegexOptions.Compiled);

		private static readonly ConcurrentDictionary<TEnum, ICacheRegion> Regions = new ConcurrentDictionary<TEnum, ICacheRegion>();

		private static ICacheRegion CreateRegion(TEnum enumValue)
		{
			var productInstanceName = string.Empty;
			try
			{
				//TODO: this is a prefix  
				productInstanceName = "TODO";//ProductNaming.GetProductInstanceName();
			}
			catch (TypeInitializationException e)
			{
				Logger.WarnFormat(
					"Type cctor - Swallowing exception: {0}{1}This should only happen in non production AppDomains like those related to automated testing",
					e, Environment.NewLine);
			}
			var enumT = typeof(TEnum);
			var regionName = string.Format("{0}-{1}-{2}-{3}", 
				productInstanceName, 
				enumT.Assembly.GetName().Version, //TODO: the version to be extracted from the entry assembly(not calling)
				enumT.Name, 
				enumValue);
			var converted = ConvertToSupportedRegionFormat(regionName);
			return new SimpleCacheRegion(converted);
		}

		public static ICacheRegion GetOrCreateRegion(TEnum cacheRegion)
		{
			return Regions.GetOrAdd(cacheRegion, CreateRegion);
		}

		private static string ConvertToSupportedRegionFormat(string src)
		{
			return src == null ? null : regionRegex.Replace(src, string.Empty);
		}
	}
}