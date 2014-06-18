using Microsoft.ApplicationServer.Caching;
using NUnit.Framework;
using OneCache.AppFabric;
using Rhino.Mocks;

namespace OneCache.UnitTests.AppFabric
{
	[TestFixture]
	internal class DataCacheExceptionWrapperTests
	{
		[Test]
		public void CanCreateWrapper()
		{
			var src=MockRepository.GenerateMock<DataCacheException>();

			var actual = new DataCacheExceptionWrapper(src);
			Assert.AreEqual(src.ErrorCode,actual.ErrorCode);
			Assert.AreEqual(src,actual.InnerException);
		}
	}
}