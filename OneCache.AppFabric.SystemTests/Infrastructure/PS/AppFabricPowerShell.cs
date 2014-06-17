using System.Management.Automation.Runspaces;

namespace OneCache.AppFabric.SystemTests.Infrastructure.PS
{
	public static class AppFabricPowerShell
	{
		public static string RunAppFabricCommands(string script)
		{
			using (var shell = new PowerShell())
			{
				shell.Open();

				var importAdminCommand = new Command(@"Import-Module DistributedCacheAdministration;Use-CacheCluster", true);

				var scriptCommand = new Command(script, true);
				
				return shell.Execute(new[] { importAdminCommand, scriptCommand });
			}
		}
	}
}
