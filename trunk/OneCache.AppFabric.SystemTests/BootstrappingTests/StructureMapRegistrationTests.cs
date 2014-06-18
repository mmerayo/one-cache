using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using OneCache.AppFabric.Configuration;
using OneCache.AppFabric.Configuration.StructureMap;
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
			InitializeContainer();

			ObjectFactory.AssertConfigurationIsValid();
		}

		private static void InitializeContainer()
		{
			ObjectFactory.Initialize(c =>
			{
				string substring = Guid.NewGuid().ToString("N").Substring(0, 10);
				var clientCacheConfiguration = new ClientCacheConfiguration("SysTest" + substring, "SampleClientId" + substring);
				clientCacheConfiguration.EndPoints.Add(new EndPointConfiguration("localhost",22233));
				c.AddRegistry(new CacheRegistry(clientCacheConfiguration));

			});
		}

		[Test]
		public void CanResolve_PublicCoreServices()
		{
			var exclusionList = new[]
			{
				"ICacheRegion"
			};

			InitializeContainer();

			var publicInterfaces = Assembly.GetAssembly(typeof (CacheRegionProvider)).GetTypes()
				.Where(x => x.IsPublic && x.IsInterface && !exclusionList.Contains(x.Name));

			foreach (var source in publicInterfaces)
			{
				Assert.IsNotNull(ObjectFactory.GetInstance(source));
			}
		}
	}
}