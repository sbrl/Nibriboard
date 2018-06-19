using System;
using System.Threading.Tasks;

namespace Nibriboard.CommandConsole
{
	public interface ICommandModule
	{
		ModuleDescription Description { get; }

		/// <summary>
		/// Sets up the command module.
		/// </summary>
		/// <param name="server">The parent nibriboard server.</param>
		void Setup(NibriboardServer server);

		/// <summary>
		/// Handles the specified arguments.
		/// </summary>
		/// <param name="request">The request to handle.</param>
		Task Handle(CommandRequest request);
	}
}
