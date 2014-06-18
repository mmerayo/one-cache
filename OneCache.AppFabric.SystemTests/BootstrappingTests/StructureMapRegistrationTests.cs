using System.Linq;
using System.Reflection;
using NUnit.Framework;
using OneCache.AppFabric.IoC.StructureMap;
using OneCache.Regions;
using StructureMap;

namespace OneCache.AppFabric.SystemTests.BootstrappingTests
{
	[TestFixture]
	internal class StructureMapRegistrationTests
	{
		[SetUp]
		public void OnTearDown()
		{
			ObjectFactory.Initialize(_ => { });
		}

		[Test]
		public void CanRegisterRegistry()
		{
			ObjectFactory.Initialize(c => c.AddRegistry(new CacheRegistry()));

			ObjectFactory.AssertConfigurationIsValid();
		}

		[Test]
		public void CanResolve_PublicCoreServices()
		{
			var exclusionList = new[]
			{
				"ICacheRegion"
			};

			ObjectFactory.Initialize(c => c.AddRegistry(new CacheRegistry()));

			var publicInterfaces = Assembly.GetAssembly(typeof (CacheRegionProvider)).GetTypes()
				.Where(x => x.IsPublic && x.IsInterface && !exclusionList.Contains(x.Name));

			foreach (var source in publicInterfaces)
			{
				Assert.IsNotNull(ObjectFactory.GetInstance(source));
			}
		}
	}
}