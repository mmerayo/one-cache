using System;
using System.Diagnostics;
using System.Threading;
using OneCache.AppFabric;
using Microsoft.ApplicationServer.Caching;
using NUnit.Framework;

namespace OneCache.SystemTests
{
	/// <summary>
	/// 	System tests the distributedCache component.
	/// </summary>
	/// <remarks>
	/// 	To make this run on a X64 box: 1. Copy the AppFabric PowerShell modules from X64 in X68: C:\Windows\System32\WindowsPowerShell\v1.0\Modules\DistributedCache* --> C:\Windows\SysWOW64\WindowsPowerShell\v1.0\Modules 2. Export your registry hive HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\AppFabric into a .reg file. 3. Run C:\Windows\SysWOW64\regedt32.exe and import that file into the 32-Bit hive: "File" --> "Import
	/// </remarks>
	[TestFixture]
	public class CacheProviderTests
	{
		readonly ICacheRegion _testRegion = CacheRegions.ByEnum(RegionName.RegionName1);

		[Test]
		public void Add_Null_Should_Swallow_Exception()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();

				target.Add(key, _testRegion, null);
			}
		}

		[Test]
		public void Add_Null_Should_Swallow_Exception_And_Remove_Existing_Value()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();

				target.Add(key, _testRegion, "Hello World!");
				target.Add(key, _testRegion, null);
				var result = target.Get<string>(key, _testRegion);

				Assert.IsNull(result);
			}
		}

		[Test]
		public void Add_With_TimeSpan_Should_Cause_Value_To_Evict()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				string value = GetSomething();

				target.Add(key, _testRegion, value, TimeSpan.FromSeconds(3));

				Assert.AreEqual(value, target.Get<string>(key, _testRegion));

				Thread.Sleep(4000);

				Assert.IsNull(target.Get<string>(key, _testRegion));
			}
		}


		[Test]
		public void Add_With_TimeSpan_By_Region_Should_Cause_Value_To_Evict()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				string value = GetSomething();

				target.Add(key, _testRegion, value, TimeSpan.FromSeconds(3));

				Assert.AreEqual(value, target.Get<string>(key, _testRegion));

				Thread.Sleep(4000);

				Assert.IsNull(target.Get<string>(key, _testRegion));
			}
		}

		[Test]
		public void Add_Null_By_Key_And_Region_Should_Swallow_Exception()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();

				target.Add(key, _testRegion, null);
			}
		}

		[Test]
		public void Get_By_Key_Should_Return_Previously_Added_Value()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				var value = GetSomething();

				target.Add(key, _testRegion, value);

				Assert.AreEqual(value, target.Get<string>(key, _testRegion));
			}
		}
		[Test]
		public void Get_BeforeRegionIsCreated_ReturnsNull()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();

				Assert.IsNull(target.Get<string>(key, CacheRegions.ByEnum(RegionName.RegionName2)));
			}
		}
		[Test]
		public void Get_By_Unknown_Key_Should_Return_Null()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();

				Assert.IsNull(target.Get<string>(key, _testRegion));
			}
		}

		[Test]
		public void Get_By_Unknown_Key_And_Region_Should_Return_Null()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				

				Assert.IsNull(target.Get<string>(key, _testRegion));
			}
		}

		[Test]
		public void Get_By_Key_And_Region_Should_Return_Previously_Added_Value()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				var value = GetSomething();

				target.Add(key, _testRegion, value);
				Assert.AreEqual(value, target.Get<string>(key, _testRegion));
			}
		}

		[Test]
		public void TryGet_By_Key_Should_Return_Previously_Added_Value()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				var value = GetSomething();

				target.Add(key, _testRegion, value);

				string resultValue;
				Assert.IsTrue(target.TryGet(key, _testRegion, out resultValue));

				Assert.AreEqual(value, resultValue);
			}
		}

		[Test]
		public void TryGet_By_Unknown_Key_Should_Return_False()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();

				string resultValue;
				Assert.IsFalse(target.TryGet(key, _testRegion, out resultValue));
			}
		}

		[Test]
		public void TryGet_By_Unknown_Key_And_Region_Should_Return_Null()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();

				string resultValue;
				Assert.IsFalse(target.TryGet(key, _testRegion, out resultValue));
			}
		}

		[Test]
		public void TryGet_Bey_Key_And_Region_Should_Return_Previously_Added_Value()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				var value = GetSomething();
				

				target.Add(key, _testRegion, value);

				string resultValue;
				Assert.IsTrue(target.TryGet(key, _testRegion, out resultValue));
				Assert.AreEqual(value, resultValue);
			}
		}

		[Test]
		public void Remove_By_Key_And_Region_Should_Cause_Get_To_Return_Null()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				var value = GetSomething();
				

				target.Add(key, _testRegion, value);

				Assert.AreEqual(value, target.Get<string>(key, _testRegion));

				target.Remove(key, _testRegion);

				Assert.IsNull(target.Get<string>(key, _testRegion));
			}
		}

		[Test]
		public void Remove_By_Region_Should_Cause_Get_To_Return_Null()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				var value = GetSomething();
				

				target.Add(key, _testRegion, value);

				Assert.AreEqual(value, target.Get<string>(key, _testRegion));

				target.RemoveRegion(_testRegion);

				Assert.IsNull(target.Get<string>(key, _testRegion));
			}
		}

		[Test]
		public void Remove_By_Key_Should_Cause_Get_To_Return_Null()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				var value = GetSomething();

				target.Add(key, _testRegion, value);

				Assert.AreEqual(value, target.Get<string>(key, _testRegion));

				target.Remove(key, _testRegion);

				Assert.IsNull(target.Get<string>(key, _testRegion));
			}
		}

		[Test]
		public void Remove_By_UnexistingKey_Does_Not_Throw()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();

				Assert.DoesNotThrow(()=>target.Remove(key, _testRegion));
			}
		}

		[Test]
		public void Get_By_Key_And_Region_How_Fast_Can_We_Get_1000()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				var value = GetSomething();
				

				target.Add(key, _testRegion, value);

				var stopwatch = new Stopwatch();
				stopwatch.Start();

				for (int i = 0; i < 1000; i++)
					Assert.AreEqual(value, target.Get<string>(key, _testRegion));

				stopwatch.Stop();

				Console.WriteLine(stopwatch.Elapsed);
			}
		}

		[Test]
		public void Get_By_Key_How_Fast_Can_We_Get_1000()
		{
			using (var target = GetCacheProvider())
			{
				var key = GetSomething();
				var value = GetSomething();

				target.Add(key, _testRegion, value);

				var stopwatch = new Stopwatch();
				stopwatch.Start();

				for (int i = 0; i < 1000; i++)
					Assert.AreEqual(value, target.Get<string>(key, _testRegion));

				stopwatch.Stop();

				Console.WriteLine(stopwatch.Elapsed);
			}
		}

      
		private static string GetSomething()
		{
			return Guid.NewGuid().ToString();
		}


		private static DistributedCache GetCacheProvider()
		{
				var appFabricCacheConfiguration = new CacheConfiguration(new DataCacheFactoryConfiguration(), true);
				var appFabricDistributedCacheFactory = new DistributedCacheFactory(appFabricCacheConfiguration);
				return new DistributedCache(NamespaceSetup.CacheName, NamespaceSetup.ProductInstancePrefix,
				                            appFabricDistributedCacheFactory);
		}
	}
}