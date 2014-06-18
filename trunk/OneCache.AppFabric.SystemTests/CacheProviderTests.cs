using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.ApplicationServer.Caching;
using NUnit.Framework;
using OneCache.Regions;

namespace OneCache.AppFabric.SystemTests
{
	/// <summary>
	///     System tests the distributedCache component.
	/// </summary>
	/// <remarks>
	///     To make this run on a X64 box: 1. Copy the AppFabric PowerShell modules from X64 in X68:
	///     C:\Windows\System32\WindowsPowerShell\v1.0\Modules\DistributedCache* -->
	///     C:\Windows\SysWOW64\WindowsPowerShell\v1.0\Modules 2. Export your registry hive
	///     HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\AppFabric into a .reg file. 3. Run C:\Windows\SysWOW64\regedt32.exe and
	///     import that file into the 32-Bit hive: "File" --> "Import
	/// </remarks>
	[TestFixture]
	public class CacheProviderTests
	{
		[SetUp]
		public void OnSetup()
		{
			_context = new TestContext();
		}

		[TearDown]
		public void OnTearDown()
		{
			_context.Dispose();
		}

		private static string GetSomething()
		{
			return Guid.NewGuid().ToString();
		}

		[Test]
		public void Add_Null_By_Key_And_Region_Should_Swallow_Exception()
		{
			var target = _context.Sut;

			var key = GetSomething();

			target.Add(key, _testRegion, null);
		}


		[Test]
		public void Add_Null_Should_Swallow_Exception()
		{
			var target = _context.Sut;

			var key = GetSomething();

			target.Add(key, _testRegion, null);
		}

		[Test]
		public void Add_Null_Should_Swallow_Exception_And_Remove_Existing_Value()
		{
			var target = _context.Sut;
			var key = GetSomething();

			target.Add(key, _testRegion, "Hello World!");
			target.Add(key, _testRegion, null);
			var result = target.Get<string>(key, _testRegion);

			Assert.IsNull(result);
		}


		[Test]
		public void Add_With_TimeSpan_By_Region_Should_Cause_Value_To_Evict()
		{
			var target = _context.Sut;

			var key = GetSomething();
			string value = GetSomething();

			target.Add(key, _testRegion, value, TimeSpan.FromSeconds(3));

			Assert.AreEqual(value, target.Get<string>(key, _testRegion));

			Thread.Sleep(4000);

			Assert.IsNull(target.Get<string>(key, _testRegion));
		}

		[Test]
		public void Add_With_TimeSpan_Should_Cause_Value_To_Evict()
		{
			var target = _context.Sut;
			var key = GetSomething();
			string value = GetSomething();

			target.Add(key, _testRegion, value, TimeSpan.FromSeconds(3));

			Assert.AreEqual(value, target.Get<string>(key, _testRegion));

			Thread.Sleep(4000);

			Assert.IsNull(target.Get<string>(key, _testRegion));
		}

		[Test]
		public void Can_BulkGet()
		{
			var target = _context.Sut;

			int capacity = 1000;
			var dictionary = new Dictionary<string, string>(capacity);
			for (int i = 0; i < capacity; i++)
			{
				string key = GetSomething();
				string value = GetSomething();
				dictionary.Add(key, value);
				target.Add(key, _testRegion, value);
			}


			IEnumerable<KeyValuePair<string, string>> keyValuePairs = target.BulkGet<string>(dictionary.Keys.Select(x => x),
				_testRegion);
			Assert.IsTrue(keyValuePairs.Count() == capacity);
			foreach (var keyValuePair in keyValuePairs)
			{
				Assert.AreEqual(dictionary[keyValuePair.Key], keyValuePair.Value);
			}
		}

		[Test]
		public void Can_ClearRegion()
		{
			var target = _context.Sut;

			var key = GetSomething();
			var value = GetSomething();

			target.Add(key, _testRegion, value);
			Assert.AreEqual(value, target.Get<string>(key, _testRegion));

			target.ClearRegion(_testRegion);
			Assert.IsNull(target.Get<string>(key, _testRegion));
		}

		[Test]
		public void Can_GetObjectsInRegion()
		{
			var target = _context.Sut;

			var key = GetSomething();
			string something = GetSomething();
			target.Add(key, _testRegion, something);

			var actual = target.GetObjectsInRegion(_testRegion);

			CollectionAssert.Contains(actual, something);
		}

		[Test]
		public void Get_BeforeRegionIsCreated_ReturnsNull()
		{
			var target = _context.Sut;

			var key = GetSomething();

			Assert.IsNull(target.Get<string>(key, new CacheRegionProvider().GetByEnum(RegionName.RegionName2)));
		}

		[Test]
		public void Get_By_Key_And_Region_How_Fast_Can_We_Get_1000()
		{
			var target = _context.Sut;

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

		[Test]
		public void Get_By_Key_And_Region_Should_Return_Previously_Added_Value()
		{
			var target = _context.Sut;

			var key = GetSomething();
			var value = GetSomething();

			target.Add(key, _testRegion, value);
			Assert.AreEqual(value, target.Get<string>(key, _testRegion));
		}

		[Test]
		public void Get_By_Key_How_Fast_Can_We_Get_1000()
		{
			var target = _context.Sut;
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

		[Test]
		public void Get_By_Key_Should_Return_Previously_Added_Value()
		{
			var target = _context.Sut;

			var key = GetSomething();
			var value = GetSomething();

			target.Add(key, _testRegion, value);

			Assert.AreEqual(value, target.Get<string>(key, _testRegion));
		}

		[Test]
		public void Get_By_Unknown_Key_And_Region_Should_Return_Null()
		{
			var target = _context.Sut;

			var key = GetSomething();


			Assert.IsNull(target.Get<string>(key, _testRegion));
		}

		[Test]
		public void Get_By_Unknown_Key_Should_Return_Null()
		{
			var target = _context.Sut;

			var key = GetSomething();

			Assert.IsNull(target.Get<string>(key, _testRegion));
		}

		[Test]
		public void Remove_By_Key_And_Region_Should_Cause_Get_To_Return_Null()
		{
			var target = _context.Sut;

			var key = GetSomething();
			var value = GetSomething();


			target.Add(key, _testRegion, value);

			Assert.AreEqual(value, target.Get<string>(key, _testRegion));

			target.Remove(key, _testRegion);

			Assert.IsNull(target.Get<string>(key, _testRegion));
		}

		[Test]
		public void Remove_By_Key_Should_Cause_Get_To_Return_Null()
		{
			var target = _context.Sut;

			var key = GetSomething();
			var value = GetSomething();

			target.Add(key, value);

			Assert.AreEqual(value, target.Get<string>(key));

			target.Remove(key);

			Assert.IsNull(target.Get<string>(key));
		}

		[Test]
		public void Remove_By_Region_Should_Cause_Get_To_Return_Null()
		{
			var target = _context.Sut;

			var key = GetSomething();
			var value = GetSomething();


			target.Add(key, _testRegion, value);

			Assert.AreEqual(value, target.Get<string>(key, _testRegion));

			target.RemoveRegion(_testRegion);

			Assert.IsNull(target.Get<string>(key, _testRegion));
		}

		[Test]
		public void Remove_By_UnexistingKey_Does_Not_Throw()
		{
			var target = _context.Sut;

			var key = GetSomething();

			Assert.DoesNotThrow(() => target.Remove(key, _testRegion));
		}

		[Test]
		public void TryGet_Bey_Key_And_Region_Should_Return_Previously_Added_Value()
		{
			var target = _context.Sut;

			var key = GetSomething();
			var value = GetSomething();


			target.Add(key, _testRegion, value);

			string resultValue;
			Assert.IsTrue(target.TryGet(key, _testRegion, out resultValue));
			Assert.AreEqual(value, resultValue);
		}

		[Test]
		public void TryGet_By_Key_Should_Return_Previously_Added_Value()
		{
			var target = _context.Sut;

			var key = GetSomething();
			var value = GetSomething();

			target.Add(key, _testRegion, value);

			string resultValue;
			Assert.IsTrue(target.TryGet(key, _testRegion, out resultValue));

			Assert.AreEqual(value, resultValue);
		}

		[Test]
		public void TryGet_By_Unknown_Key_And_Region_Should_Return_Null()
		{
			var target = _context.Sut;

			var key = GetSomething();

			string resultValue;
			Assert.IsFalse(target.TryGet(key, _testRegion, out resultValue));
		}

		[Test]
		public void TryGet_By_Unknown_Key_Should_Return_False()
		{
			var target = _context.Sut;

			var key = GetSomething();

			string resultValue;
			Assert.IsFalse(target.TryGet(key, _testRegion, out resultValue));
		}

		[Test]
		public void WhenNotConnectedFromTheStartUp_Uses_FirstRequest_ToConnect()
		{
			_context.WithStartupConnectivityMode(false);
			var target = _context.Sut;

			var key = GetSomething();
			var value = GetSomething();

			target.Add(key, _testRegion, value);
			Assert.IsNull(target.Get<string>(key, _testRegion));

			Thread.Sleep(1500);

			target.Add(key, _testRegion, value);
			Assert.AreEqual(value, target.Get<string>(key, _testRegion));
		}

		private readonly ICacheRegion _testRegion = new CacheRegionProvider().GetByEnum(RegionName.RegionName1);
		private TestContext _context;

		private class TestContext : IDisposable
		{
			private bool _connectOnStartup = true;
			private OneCache.DistributedCache _sut;

			public OneCache.DistributedCache Sut
			{
				get { return _sut ?? GetSut(); }
			}

			public void Dispose()
			{
				if (_sut != null)
				{
					_sut.Dispose();
					_sut = null;
				}
			}

			public TestContext WithStartupConnectivityMode(bool connectOnStartup = true)
			{
				_connectOnStartup = connectOnStartup;
				return this;
			}

			private OneCache.DistributedCache GetSut()
			{
				var appFabricCacheConfiguration = new CacheConfiguration(new DataCacheFactoryConfiguration(), _connectOnStartup);
				var appFabricDistributedCacheFactory = new DistributedCacheFactory(appFabricCacheConfiguration);
				return new OneCache.DistributedCache(NamespaceSetup.CacheName, NamespaceSetup.ProductInstancePrefix,
					appFabricDistributedCacheFactory);
			}
		}
	}
}