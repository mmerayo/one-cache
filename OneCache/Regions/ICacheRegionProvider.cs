using System.Linq;

namespace OneCache.Regions
{

	public interface ICacheRegionProvider
	{
		ICacheRegion GetByEnum<TEnum>(TEnum enumValue) where TEnum : struct;
	}
}