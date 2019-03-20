using System;

namespace SlackBugz.Api.Models.Slack
{
	public class SlashCommandException : Exception
	{
		public SlashCommandException(string message, Exception innerException)
			: base(message, innerException) { }

		public SlashCommandException(string message)
			: base(message) { }
	}
}
