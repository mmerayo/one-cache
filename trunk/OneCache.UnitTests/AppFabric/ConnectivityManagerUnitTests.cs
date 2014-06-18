using System;
using System.Threading;
using Microsoft.ApplicationServer.Caching;
using NUnit.Framework;
using OneCache.AppFabric;
using Ploeh.AutoFixture;
using Rhino.Mocks;

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
		public void CanExecuteDefaultKeepAlive()
		{
			_testContext.WithKeepAlive(false);
			var target = _testContext.Sut; 
			target.NotifyUnavailability();
			Assert.DoesNotThrow(()=>target.CheckIsAvailable());

		}

		[Test]
		public void Can_CheckIsAvailable_WhenIsNot()
		{
			var target = _testContext.Sut;
			target.NotifyUnavailability();

			Assert.IsTrue(target.CheckIsAvailable());
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

		[Test]
		public void When_PerformsKeepAlive_Fails_ItBecomesUnavailable()
		{
			_testContext.WithKeepAlive(true, () => { throw new Exception(); });
			var target = _testContext.Sut;
			target.NotifyUnavailability();
			Assert.IsFalse(target.CheckIsAvailable());
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
				_cacheWrapper = MockRepository.GenerateStub<DataCacheWrapper>();
				//_cacheWrapper.Expect(x => x.Add(Arg<string>.Is.Anything, Arg<object>.Is.Anything));
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

			public TestContext WithKeepAlive(bool custom, Action keepAliveAction = null)
			{
				_fixture.Register(() => custom
					? new ConnectivityManager(_cacheWrapper, keepAliveAction ?? DoKeepAlive)
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