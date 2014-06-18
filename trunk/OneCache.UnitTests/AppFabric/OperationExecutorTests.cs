using System;
using NUnit.Framework;
using OneCache.AppFabric;
using Rhino.Mocks;

namespace OneCache.UnitTests.AppFabric
{
	[TestFixture]
	internal class OperationExecutorTests
	{
		[Test]
		public void CanExecuteSucessfulAction()
		{
			const string expectedResult = "To Anonymous";
			var testContext = new TestContext<string>()
				.WithSucessfulResponse(expectedResult);

			var target = testContext.Sut;

			string actual = null;
			Assert.DoesNotThrow(() => actual = target.Execute(testContext.OperationToExecute));

			testContext.AssertNoErrorHandlingWasNeeded();

			Assert.IsNotNull(actual);
			Assert.AreEqual(expectedResult, actual);
		}

		[Test]
		public void CannotReuseInstance()
		{
			const string expectedResult = "To Anonymous";
			var testContext = new TestContext<string>()
				.WithSucessfulResponse(expectedResult);

			var target = testContext.Sut;

			Assert.DoesNotThrow(() => target.Execute(testContext.OperationToExecute));
			Assert.Throws<NotSupportedException>(() => target.Execute(testContext.OperationToExecute));
			testContext.AssertNoErrorHandlingWasNeeded();
		}

		[Test]
		public void HandlesAppFabricExceptions()
		{
			var testContext = new TestContext<string>()
				.WithAppFabricException(false, false);

			var target = testContext.Sut;

			string actual = null;
			Assert.DoesNotThrow(() => actual = target.Execute(testContext.OperationToExecute));

			testContext.AssertErrorHandlingWasNeeded();

			Assert.IsNull(actual);
		}

		[Test]
		public void RethrowsNonAppFabricExceptions()
		{
			var testContext = new TestContext<string>()
				.WithNonAppFabricException();

			var target = testContext.Sut;

			Assert.Throws<TestException>(() => target.Execute(testContext.OperationToExecute));

			testContext.AssertNoErrorHandlingWasNeeded();
		}

		[Test]
		public void RetriesOperationWhenMustRetry()
		{
			const int times = 2;
			var testContext = new TestContext<int>()
				.WithAppFabricException(true, false, times);

			var target = testContext.Sut;

			Assert.Throws<NullReferenceException>(() => target.Execute(testContext.OperationToExecute));

			//The time +1  is when is not expected and rethrows
			testContext.AssertErrorHandlingWasNeeded(times + 1);
		}

		[Test]
		public void ReturnsDefaultWhenNotNeededToRethrow()
		{
			var testContext = new TestContext<int>()
				.WithAppFabricException(false, false);

			var target = testContext.Sut;

			var actual = 0;
			Assert.DoesNotThrow(() => actual = target.Execute(testContext.OperationToExecute));

			testContext.AssertErrorHandlingWasNeeded();

			Assert.AreEqual(default(int), actual);
		}


		private class TestContext<TResult>
		{
			private readonly IExceptionHandler _exceptionHandler;
			private readonly DataCacheExceptionWrapper _theException;
			private Func<TResult> _operation;

			public TestContext()
			{
				_exceptionHandler = MockRepository.GenerateMock<IExceptionHandler>();
				_theException = MockRepository.GenerateMock<DataCacheExceptionWrapper>();
			}

			public OperationExecutor<TResult> Sut
			{
				get
				{
					var result = new OperationExecutor<TResult>(OperationExecutionContext.Create(null, null, null), _exceptionHandler);
					return result;
				}
			}

			public TestContext<TResult> WithSucessfulResponse(TResult result)
			{
				_operation = () => result;
				return this;
			}

			public TestContext<TResult> WithAppFabricException(bool needRetry, bool rethrow, int times = 1)
			{
				_exceptionHandler.Expect(x => x.Handle(Arg<DataCacheExceptionWrapper>.Is.Anything))
					.Return(new HandleExceptionResult(needRetry, rethrow)).Repeat.Times(times);


				_operation = () => { throw _theException; };
				return this;
			}

			public TestContext<TResult> WithNonAppFabricException()
			{
				_operation = () => { throw new TestException(); };
				return this;
			}

			public TResult OperationToExecute()
			{
				return _operation();
			}

			public void AssertNoErrorHandlingWasNeeded()
			{
				_exceptionHandler.AssertWasNotCalled(x => x.Handle(Arg<DataCacheExceptionWrapper>.Is.Anything));
			}

			public void AssertErrorHandlingWasNeeded(int times = 1)
			{
				_exceptionHandler.AssertWasCalled(x => x.Handle(_theException),
					options => options.Repeat.Times(times));
			}
		}

		private class TestException : Exception
		{
		}
	}
}