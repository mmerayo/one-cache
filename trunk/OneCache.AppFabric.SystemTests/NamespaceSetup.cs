using System;
using System.Threading;
using NUnit.Framework;
using OneCache.AppFabric.SystemTests.Infrastructure.PS;

namespace OneCache.AppFabric.SystemTests
{
	[SetUpFixture]
	public class NamespaceSetup
	{
		public static string CacheName = "SysTest" + Guid.NewGuid().ToString("N").Substring(0, 10);
		public static string ProductInstancePrefix = "ProductInstancePrefixSampleName";


		[SetUp]
		public static void SetUp()
		{
			Console.WriteLine(AppFabricPowerShell.RunAppFabricCommands("new-cache " + CacheName));

			for (int retries = 0;; retries++)
				try
				{
					GetCache_Should_Retrieve_Cache_Client(new DistributedCacheFactory(true));
					break;
				}
				catch (Exception)
				{
					if (retries >= 9)
						throw;

					Thread.Sleep(1000);
				}
		}

		[TearDown]
		public static void Cleanup()
		{
			AppFabricPowerShell.RunAppFabricCommands("remove-cache " + CacheName);
		}

		private static void GetCache_Should_Retrieve_Cache_Client(DistributedCacheFactory factory)
		{
#pragma warning disable 642
			using (new OneCache.DistributedCache(CacheName, ProductInstancePrefix, factory)) ;
#pragma warning restore 642
		}
	}
}