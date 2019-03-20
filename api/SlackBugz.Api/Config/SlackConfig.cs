using System;
using System.Linq;

namespace SlackBugz.Api.Config
{
	public class SlackConfig
	{
		public readonly string Workspace;
		public readonly string[] ShortSlashCommands;
		public readonly string[] LongSlashCommands;

		public SlackConfig(string workspace, string[] shortSlashCommands, string[] longSlashCommands)
		{
			Workspace = workspace;
			ShortSlashCommands = shortSlashCommands.Select(c => c.ToLower()).ToArray();
			LongSlashCommands = longSlashCommands.Select(c => c.ToLower()).ToArray();
		}

		public bool IsKnownCommand(string command)
		{
			return ShortSlashCommands.Concat(LongSlashCommands)
				.Any(c => c.Equals(command, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}
