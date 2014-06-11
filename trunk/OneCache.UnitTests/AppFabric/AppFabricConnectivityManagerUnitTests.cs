using System;
using System.Threading;
using OneCache.AppFabric;
using NUnit.Framework;
using Rhino.Mocks;

namespace OneCache.UnitTests.AppFabric
{
	[TestFixture]
	public class AppFabricConnectivityManagerUnitTests
	{
		[Test]
		public void StartsAvailable()
		{
			var testContext = new TestContext();
			var target=testContext.Sut;

			Assert.IsTrue(target.CheckIsAvailable());
		}

		[Test]
		public void CheckIsAvailable_PerformsKeepAlive_WhenNotAvailable()
		{
			var testContext = new TestContext();
			var target = testContext.Sut;
			target.NotifyUnavailability();

			target.CheckIsAvailable();
			
			testContext.OnKeepAliveCalled.WaitOne(TimeSpan.FromSeconds(10));

			testContext.AssertKeepAliveWasCalled();
		}

		private class TestContext
		{
			private readonly DataCacheWrapper _cacheWrapper;
			public TestContext()
			{
				_cacheWrapper = MockRepository.GenerateMock<DataCacheWrapper>();
				OnKeepAliveCalled=new ManualResetEvent(false);
			}

			public ConnectivityManager Sut
			{
				get { return new ConnectivityManager(_cacheWrapper,DoKeepAlive); }
			}

			private bool KeepAliveCalled { get; set; }
			public ManualResetEvent OnKeepAliveCalled { get; private set; }
			private void DoKeepAlive()
			{
				KeepAliveCalled = true;
				OnKeepAliveCalled.Set();

			}

			public void AssertKeepAliveWasCalled()
			{
				Assert.IsTrue(KeepAliveCalled);
			}
		}
	}
}
