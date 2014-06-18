using System;
using System.Threading;
using NUnit.Framework;
using OneCache.AppFabric;
using Ploeh.AutoFixture;

namespace OneCache.UnitTests.AppFabric
{
	[TestFixture]
	public class ConnectivityManagerUnitTests
	{
		[SetUp]
		public void OnSetUp()
		{
			_testContext = new TestContext();
		}

		[Test]
		public void CanBeCreated_WithDefaultKeepAlive()
		{
			_testContext.WithKeepAlive(false);
			Assert.DoesNotThrow(() => { var a = _testContext.Sut; });
		}


		[Test]
		public void CheckIsAvailable_PerformsKeepAlive_WhenNotAvailable()
		{
			var target = _testContext.Sut;
			target.NotifyUnavailability();

			target.CheckIsAvailable();

			_testContext.OnKeepAliveCalled.WaitOne(TimeSpan.FromSeconds(10));

			_testContext.AssertKeepAliveWasCalled();
		}

		[Test]
		public void StartsAvailable()
		{
			var target = _testContext.Sut;

			Assert.IsTrue(target.CheckIsAvailable());
		}

		private TestContext _testContext;

		private class TestContext
		{
			private readonly DataCacheWrapper _cacheWrapper;
			private readonly IFixture _fixture = new Fixture();
			private bool _keepAliveConfigured;
			private ConnectivityManager _sut;

			public TestContext()
			{
				_cacheWrapper = _fixture.Freeze<DataCacheWrapper>(); //MockRepository.GenerateMock<DataCacheWrapper>();
				OnKeepAliveCalled = new ManualResetEvent(false);
			}

			public ConnectivityManager Sut
			{
				get
				{
					if (!_keepAliveConfigured)
						WithKeepAlive(true);
					return _sut ??
					       (_sut = _fixture.Create<ConnectivityManager>());
				}
			}

			private bool KeepAliveCalled { get; set; }
			public ManualResetEvent OnKeepAliveCalled { get; private set; }

			private void DoKeepAlive()
			{
				KeepAliveCalled = true;
				OnKeepAliveCalled.Set();
			}

			public TestContext WithKeepAlive(bool custom)
			{
				_fixture.Register(() => custom
					? new ConnectivityManager(_cacheWrapper, DoKeepAlive)
					: new ConnectivityManager(_cacheWrapper));
				_keepAliveConfigured = true;
				return this;
			}

			public void AssertKeepAliveWasCalled()
			{
				Assert.IsTrue(KeepAliveCalled);
			}
		}
	}
}