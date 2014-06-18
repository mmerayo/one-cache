using System;
using NUnit.Framework;
using OneCache.Regions;
using Ploeh.AutoFixture;
using Rhino.Mocks;

namespace OneCache.UnitTests
{
	[TestFixture]
	public class CacheProviderTests
	{
		[Test]
		public void AddNullItem_DoesNotThrow()
		{
			object value;
			string key;
			ICacheRegion region;
			var testContext = new TestContext()
				.With(out key, out value, out region);

			value = null;

			var sut = testContext.Sut;

			Assert.DoesNotThrow(() => sut.Add(key, region, value));
		}

		[Test]
		public void AddNullItem_RemovesItem_IfExists()
		{
			object value;
			string key;
			ICacheRegion region;
			var testContext = new TestContext()
				.With(out key, out value, out region);

			var sut = testContext.Sut;

			sut.Add(key, region, null);

			testContext.AssertRemoveItemWasCalled(key, region);
		}

		[Test]
		public void CanAddItem()
		{
			object value;
			string key;
			ICacheRegion region;
			var testContext = new TestContext()
				.With(out key, out value, out region);

			var sut = testContext.Sut;

			sut.Add(key, region, value);

			testContext.AssertAddWasCalled(key, value, region);
		}

		[Test]
		public void CanAddItem_WithExpiration()
		{
			object value;
			string key;
			ICacheRegion region;
			TimeSpan expirationTime;
			var testContext = new TestContext()
				.With(out key, out value, out region, out expirationTime);

			var sut = testContext.Sut;

			sut.Add(key, region, value, expirationTime);

			testContext.AssertAddWasCalled(key, value, expirationTime, region);
		}

		[Test]
		public void CanGetItem()
		{
			object value;
			string key;
			ICacheRegion region;
			var testContext = new TestContext()
				.WithItem(out key, out value, out region);

			var sut = testContext.Sut;

			var actual = sut.Get<object>(key, region);

			testContext.AssertGetWasCalled<object>(key, region);

			Assert.AreEqual(value, actual);
		}

		[Test]
		public void CanGetItem_ByUnexistingKey_ReturnsNull()
		{
			object value;
			string key;
			ICacheRegion region;

			var testContext = new TestContext()
				.WithItem(out key, out value, out region);

			var sut = testContext.Sut;

			region = testContext.CacheRegionProvider.GetByEnum(RegionName.Currencies);
			var actual = sut.Get<object>(key, region);

			testContext.AssertGetWasCalled<object>(key, region);

			Assert.IsNull(actual);
		}

		[Test]
		public void CanRemoveItem()
		{
			object value;
			string key;
			ICacheRegion region;
			var testContext = new TestContext()
				.WithItem(out key, out value, out region);

			var sut = testContext.Sut;

			sut.Remove(key, region);

			testContext.AssertRemoveItemWasCalled(key, region);
		}

		[Test]
		public void PublicMethods_ThrowIfDisposed()
		{
			object value;
			string key;
			ICacheRegion region;
			TimeSpan expiration;

			var testContext = new TestContext()
				.With(out key, out value, out region, out expiration);
			var sut = testContext.Sut;
			sut.Dispose();

			Assert.Throws<ObjectDisposedException>(() => sut.Add(key, region, value));
			Assert.Throws<ObjectDisposedException>(() => sut.Add(key, region, value, expiration));
			Assert.Throws<ObjectDisposedException>(() => sut.Get<object>(key, region));
			Assert.Throws<ObjectDisposedException>(() => sut.Remove(key, region));
			Assert.Throws<ObjectDisposedException>(() => sut.RemoveRegion(region));
		}

		private enum RegionName
		{
			Product,
			Currencies
		}

		private class TestContext
		{
			private readonly IFixture _fixture;

			public TestContext()
			{
				_fixture = new Fixture();

				CacheName = _fixture.CreateAnonymous<string>();
				ProductInstanceName = _fixture.CreateAnonymous<string>();
				Factory = MockRepository.GenerateMock<IDistributedCacheFactory>();
				DistributedCache = MockRepository.GenerateMock<IDistributedCache>();
				Factory.Stub(x => x.GetCache(CacheName)).Return(DistributedCache);
				CacheRegionProvider = new CacheRegionProvider();
			}

			public DistributedCache Sut
			{
				get { return new DistributedCache(CacheName, ProductInstanceName, Factory); }
			}

			private string ProductInstanceName { get; set; }

			private string CacheName { get; set; }

			private IDistributedCacheFactory Factory { get; set; }

			private IDistributedCache DistributedCache { get; set; }

			public ICacheRegionProvider CacheRegionProvider { get; private set; }


			public TestContext With<TValue>(out string key, out TValue value, out ICacheRegion region)
			{
				TimeSpan expirationTime;
				return With(out key, out value, out region, out expirationTime);
			}

			public TestContext With<TValue>(out string key, out TValue value, out ICacheRegion region,
				out TimeSpan expirationTime)
			{
				var localKey = _fixture.CreateAnonymous<string>();
				var localValue = _fixture.CreateAnonymous<TValue>();
				var localRegion = CacheRegionProvider.GetByEnum(_fixture.CreateAnonymous<RegionName>());
				var localExpiration = _fixture.CreateAnonymous<TimeSpan>();

				key = localKey;
				value = localValue;
				region = localRegion;
				expirationTime = localExpiration;

				return this;
			}

			public TestContext WithItem<TValue>(out string key, out TValue value, out ICacheRegion region)
			{
				With(out key, out value, out region);
				var reg = region;
				string s = key;
				DistributedCache.Stub(x => x.Get<object>(s, reg)).Return(value);

				return this;
			}

			public void AssertAddWasCalled<TValue>(string key, TValue value, ICacheRegion region)
			{
				DistributedCache.AssertWasCalled(x => x.Add(key, region, value),
					options => options.Repeat.AtLeastOnce());
			}

			public void AssertAddWasCalled<TValue>(string key, TValue value, TimeSpan expirationTime, ICacheRegion region)
			{
				DistributedCache.AssertWasCalled(x => x.Add(key, region, value, expirationTime),
					options => options.Repeat.AtLeastOnce());
			}

			public void AssertRemoveItemWasCalled(string key, ICacheRegion region = null)
			{
				DistributedCache.AssertWasCalled(x => x.Remove(key, region),
					options => options.Repeat.AtLeastOnce());
			}

			public void AssertGetWasCalled<TValue>(string key, ICacheRegion region) where TValue : class
			{
				DistributedCache.AssertWasCalled(x => x.Get<TValue>(key, region),
					options => options.Repeat.AtLeastOnce());
			}
		}
	}
}