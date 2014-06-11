﻿using System;
using Common.DistributedCaching.AppFabric;
using Microsoft.ApplicationServer.Caching;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;

namespace Common.DistributedCaching.UnitTests.AppFabric
{
	[TestFixture]
	public class AppFabricExceptionHandlerUnitTests
	{
		[Test]
		public void WhenMaxRetriesReached_NeedRethrow()
		{
			var testContext = new TestContext();

			var target = testContext.Sut;

			HandleExceptionResult result=null;
			for (int i = 0; i < 5;i++ )
				result = target.Handle(testContext.GetException(DataCacheErrorCode.RegionAlreadyExists));


			Assert.AreEqual(false, result.MustRetry);
			Assert.AreEqual(true, result.Rethrow);
		}

		[TestCase(DataCacheErrorCode.RetryLater)]
		[TestCase(DataCacheErrorCode.RegionAlreadyExists)]
		[TestCase(DataCacheErrorCode.RegionDoesNotExist)]
		[TestCase(DataCacheErrorCode.Timeout)]
		[TestCase(DataCacheErrorCode.ConnectionTerminated)]
		[TestCase(DataCacheErrorCode.KeyDoesNotExist)]
		[Test]
		public void ResultIsExpected_ErroCodeCases(int errorCode)
		{
			var testContext = new TestContext();

			var target = testContext.Sut;
			var result = target.Handle(testContext.GetException(errorCode));

			testContext.AssertResultIsTheExpected(errorCode, result);
		}


		[Test]
		public void CreatesRegion_WhenRegionDoesNotExists()
		{
			var testContext = new TestContext();

			var target = testContext.Sut;
			var result = target.Handle(testContext.GetException(DataCacheErrorCode.RegionDoesNotExist));

			testContext.AssertCreatesRegion();
		}

		[Test]
		public void NotifiesUnavailability_WhenAppFabricIsDown()
		{
			var testContext = new TestContext();

			var target = testContext.Sut;
			var result = target.Handle(testContext.GetException(DataCacheErrorCode.RetryLater));

			testContext.AssertNotifiedUnavaliability();
		}

		private class TestContext
		{
			private readonly IConnectivityManager _connectivityManager;
			private readonly DataCacheWrapper _cacheWrapper;
			private readonly Fixture _fixture;
			private readonly string _regionName;

			public TestContext()
			{
				_fixture = new Fixture();
				_connectivityManager = MockRepository.GenerateMock<IConnectivityManager>();
				_cacheWrapper = MockRepository.GenerateMock<DataCacheWrapper>();
				_regionName = _fixture.CreateAnonymous<string>();
			}

			public ExceptionHandler Sut
			{
				get
				{
					return
						new ExceptionHandler(OperationExecutionContext.Create(_connectivityManager, _cacheWrapper,
						                                                                        _regionName));
				}
			}

			public DataCacheExceptionWrapper GetException(int errorCode)
			{
				var result = MockRepository.GenerateMock<DataCacheExceptionWrapper>();
				result.Expect(x => x.ErrorCode).Return(errorCode);
				return result;
			}

			public void AssertNotifiedUnavaliability()
			{
				_connectivityManager.AssertWasCalled(x => x.NotifyUnavailability(), opt => opt.Repeat.Times(1));
			}

			public void AssertCreatesRegion()
			{
				_cacheWrapper.AssertWasCalled(x => x.CreateRegion(_regionName), o => o.Repeat.Times(1));
			}

			public void AssertResultIsTheExpected(int errorCode, HandleExceptionResult actual)
			{
				bool retry, rethrow;
				switch (errorCode)
				{
					case DataCacheErrorCode.KeyDoesNotExist:
						retry = false;
						rethrow = false;
						break;

					case DataCacheErrorCode.Timeout:
					case DataCacheErrorCode.ConnectionTerminated:
					case DataCacheErrorCode.RegionDoesNotExist:
						retry = true;
						rethrow = false;
						break;

					case DataCacheErrorCode.RetryLater:
					case DataCacheErrorCode.RegionAlreadyExists:
						retry = false;
						rethrow = true;
						break;
					default:
						throw new NotImplementedException("Case not implemented");


				}
				Assert.AreEqual(retry, actual.MustRetry);
				Assert.AreEqual(rethrow, actual.Rethrow);

			}
		}
	}
}