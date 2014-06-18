using System;
using System.Threading;
using Microsoft.ApplicationServer.Caching;
using NUnit.Framework;
using OneCache.AppFabric.SystemTests.Infrastructure.PS;
using OneCache.Regions;

namespace OneCache.AppFabric.SystemTests
{
	[TestFixture]
	public class CacheUnavailableTests
	{
		[TearDown]
		public void OnTearDown()
		{
			try
			{
				AppFabricPowerShell.RunAppFabricCommands(StartCacheClusterCommand);
				Thread.Sleep(3000);
			}
			catch
			{
				Console.WriteLine("CacheUnavailableTests - OnTearDown. Could not {0}", StartCacheClusterCommand);
			}
		}

		private static OneCache.DistributedCache GetCacheProvider()
		{
			return new OneCache.DistributedCache(NamespaceSetup.CacheName, NamespaceSetup.ProductInstancePrefix,
				new DistributedCacheFactory(new CacheConfiguration(new DataCacheFactoryConfiguration(), false)));
		}

		private static string GetSomething()
		{
			return Guid.NewGuid().ToString();
		}

		[Test]
		public void WhenCacheIsDownCanInvoke_Operation()
		{
			using (var target = GetCacheProvider())
			{
				AppFabricPowerShell.RunAppFabricCommands(StopCacheClusterCommand);
				var key = GetSomething();

				Assert.DoesNotThrow(() => target.Add(key, _testRegion, new object()));
			}
		}

		[Explicit]
		[Test]
		public void WhenCacheIsDownCanInvoke_Operation_OnceIsUp()
		{
			using (var target = GetCacheProvider())
			{
				AppFabricPowerShell.RunAppFabricCommands(StopCacheClusterCommand);
				var key = GetSomething();

				Assert.DoesNotThrow(() => target.Add(key, _testRegion, GetSomething()));

				AppFabricPowerShell.RunAppFabricCommands(StartCacheClusterCommand);
				Thread.Sleep(3000);
				//NamespaceSetup.SetUp();
				//first time wakes up the manager
				target.Add(key, _testRegion, GetSomething());
				Assert.IsNull(target.Get<object>(key, _testRegion));
				Thread.Sleep(TimeSpan.FromSeconds(31)); //Timespan between checks
				target.Get<object>(key, _testRegion); //this checks availability
				Thread.Sleep(3000);

				//second time should be up
				var expected = GetSomething();
				target.Add(key, _testRegion, expected);
				var actual = target.Get<object>(key, _testRegion);
				Assert.IsNotNull(actual);
				Assert.AreEqual(expected, actual);
			}
		}

		private const string StartCacheClusterCommand = "Start-CacheCluster";
		private const string StopCacheClusterCommand = "Stop-CacheCluster";
		private readonly ICacheRegion _testRegion = new CacheRegionProvider().GetByEnum(RegionName.RegionName1);
	}
}