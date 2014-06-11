using System.Management.Automation.Runspaces;

namespace OneCache.SystemTests.Infrastructure.PS
{
	public static class AppFabricPowerShell
	{
		public static string RunAppFabricCommands(string script)
		{
			using (var shell = new PowerShell())
			{
				shell.Open();

				var importCommand = new Command(@"Import-Module DistributedCacheAdministration;Use-CacheCluster", true);
				var scriptCommand = new Command(script, true);
				
				return shell.Execute(new[] { importCommand, scriptCommand });
			}
		}
	}
}
