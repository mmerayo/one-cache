using System;
using OneCache.AppFabric;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;

namespace OneCache.UnitTests.AppFabric
{
	[TestFixture]
	public class DistributedCacheUnitTests
	{
		[Test,Theory]
		public void CanAdd( bool connectivityAvailable)
		{
			var testContext = new TestContext().WithConnectivityAvailable(connectivityAvailable);

			var target = testContext.Sut;

			target.Add(testContext.Key, testContext.Region, testContext.Value);

			testContext.AssertAddWasCalled(connectivityAvailable);
		}


		[Test, Theory]
		public void CanAddWithExpiration(bool connectivityAvailable)
		{
			var testContext = new TestContext().WithConnectivityAvailable(connectivityAvailable);

			var target = testContext.Sut;

			target.Add(testContext.Key, testContext.Region, testContext.Value,testContext.ExpirationTime);

			testContext.AssertAddWithExpirationWasCalled(connectivityAvailable);
		}

		[Test, Theory]
		public void CanTryGet(bool connectivityAvailable)
		{
			var testContext = new TestContext().WithConnectivityAvailable(connectivityAvailable);

			var target = testContext.Sut;

			object actualValue;
			target.TryGet(testContext.Key, testContext.Region,out actualValue);
			
			if(connectivityAvailable)
				Assert.AreSame(testContext.Value,actualValue);

			testContext.AssertTryGetWasCalled(connectivityAvailable);
		}

		[Test, Theory]
		public void CanRemove(bool connectivityAvailable)
		{
			var testContext = new TestContext().WithConnectivityAvailable(connectivityAvailable);

			var target = testContext.Sut;

			target.Remove(testContext.Key, testContext.Region);

			testContext.AssertRemoveWasCalled(connectivityAvailable);
		}

		[Test, Theory]
		public void CanRemoveRegion(bool connectivityAvailable)
		{
			var testContext = new TestContext().WithConnectivityAvailable(connectivityAvailable);

			var target = testContext.Sut;

			target.RemoveRegion(testContext.Region);

			testContext.AssertRemoveRegionWasCalled(connectivityAvailable);
		}

		private class TestContext
		{
			private readonly DataCacheWrapper _cacheWrapper;
			private readonly IConnectivityManager _connectivityManager;
			private readonly Fixture _fixture = new Fixture();

			public TestContext()
			{
				_cacheWrapper = MockRepository.GenerateStub<DataCacheWrapper>();
				_connectivityManager = MockRepository.GenerateMock<IConnectivityManager>();
				Key = _fixture.CreateAnonymous<string>();
				Region = MockRepository.GenerateMock<ICacheRegion>();
				Value = _fixture.CreateAnonymous<object>();

				_cacheWrapper.Expect(x => x.Get(Key, Region.RegionKey())).Return(Value);
			}

			public OneCache.AppFabric.DistributedCache Sut
			{
				get { return new OneCache.AppFabric.DistributedCache(_cacheWrapper, _connectivityManager); }
			}

			public string Key { get; private set; }

			public ICacheRegion Region { get; private set; }

			public object Value { get; private set; }

			public TimeSpan ExpirationTime { get; private set; }

			public TestContext WithConnectivityAvailable(bool toReturn = true)
			{
				_connectivityManager.Expect(x => x.CheckIsAvailable()).Return(toReturn);
				return this;
			}

			public void AssertAddWasCalled(bool called)
			{
				if (called)
					_cacheWrapper.AssertWasCalled(x => x.Put(Key, Value, Region.RegionKey()));
				else
					_cacheWrapper.AssertWasNotCalled(x => x.Put(Key, Value, Region.RegionKey()));
			}

			public void AssertAddWithExpirationWasCalled(bool called)
			{
				if (called)
					_cacheWrapper.AssertWasCalled(x => x.Put(Key, Value, ExpirationTime, Region.RegionKey()));
				else
					_cacheWrapper.AssertWasNotCalled(x => x.Put(Key, Value, ExpirationTime, Region.RegionKey()));
			}

			public void AssertTryGetWasCalled(bool called)
			{
				if (called)
					_cacheWrapper.AssertWasCalled(x => x.Get(Key, Region.RegionKey()));
				else
					_cacheWrapper.AssertWasNotCalled(x => x.Get(Key, Region.RegionKey()));
			}

			public void AssertRemoveWasCalled(bool called)
			{
				if (called)
					_cacheWrapper.AssertWasCalled(x => x.Remove(Key, Region.RegionKey()));
				else
					_cacheWrapper.AssertWasNotCalled(x => x.Remove(Key, Region.RegionKey()));
			}

			public void AssertRemoveRegionWasCalled(bool called)
			{
				if (called)
					_cacheWrapper.AssertWasCalled(x => x.RemoveRegion(Region.RegionKey()));
				else
					_cacheWrapper.AssertWasNotCalled(x => x.RemoveRegion(Region.RegionKey()));
			}
		}
	}
}