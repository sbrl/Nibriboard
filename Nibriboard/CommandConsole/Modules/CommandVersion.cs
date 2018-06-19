using System;
using System.Threading.Tasks;

namespace Nibriboard.CommandConsole.Modules
{
	public class CommandVersion : ICommandModule
	{
		public ModuleDescription Description { get; } = new ModuleDescription(
			"version",
			"Show the version of nibriboard that is currently running"
		);
		
		public CommandVersion()
		{
		}

		public void Setup(NibriboardServer inConsole)
		{
			// noop - we access only static variables here
		}

		public async Task Handle(CommandRequest request)
		{
			await request.WriteLine(
				"Nibriboard Server {0}, built on {1}",
				NibriboardServer.Version,
				NibriboardServer.BuildDate.ToString("R")
			);
			await request.WriteLine("By Starbeamrainbowlabs, licensed under MPL-2.0");
		}

	}
}
