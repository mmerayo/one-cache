using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Common.DistributedCaching.SystemTests.Infrastructure.PS
{
	public class PowerShell : IDisposable
	{
		private Runspace _runspace;

		/// <summary>
		/// Opens the shell.
		/// </summary>
		public void Open()
		{
			_runspace = RunspaceFactory.CreateRunspace();
			_runspace.Open();
		}

		/// <summary>
		/// Executes a list of commands.
		/// </summary>
		/// <param name="commands">The list of commands to execute.</param>
		/// <returns>The script (NOT file path) to execute.</returns>
		public string Execute(IEnumerable<Command> commands)
		{
			if (_runspace == null)
				throw new InvalidOperationException("PowerShell has not been opened.");

			Pipeline pipeline = _runspace.CreatePipeline();

			foreach (var command in commands)
			{
				pipeline.Commands.Add(command);
			}

			//create a collection to hold output of script

			// execute the script
			Collection<PSObject> results = pipeline.Invoke();

			var stringBuilder = new StringBuilder();

			if (pipeline.Error.Count > 0)
			{
				throw new InvalidOperationException(pipeline.Error.ReadToEnd().ToString());
			}

			//loop thru all the objects returned (each object will contain output text)
			foreach (PSObject obj in results)
			{
				stringBuilder.AppendLine(obj.ToString());
			}

			return stringBuilder.ToString();
		}

		/// <summary>
		/// Executes a snippet of PowerShell script.
		/// </summary>
		/// <param name="script">Script snippet.</param>
		/// <returns>Execution output text.</returns>
		public static string ExecuteScript(string script)
		{
			using (var shell = new PowerShell())
			{
				shell.Open();

				var scriptCommand = new Command(script, true);

				return shell.Execute(new[] { scriptCommand });
			}
		}

		/// <summary>
		/// Disposes the shell.
		/// </summary>
		public void Dispose()
		{
			if (_runspace != null)
			{
				_runspace.Close();
				_runspace = null;
			}
		}
	}
}