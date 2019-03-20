namespace SlackBugz.Api.Models.Slack
{
	public class FogBugzSlashCommandInfo
	{
		public readonly string Command;
		public readonly int CaseNumber;
		public readonly string ChannelId;
		public readonly string TriggerId;
		public readonly string ResponseUrl;

		public FogBugzSlashCommandInfo(string command, int caseNumber, string channelId, string triggerId, string responseUrl)
		{
			Command = command;
			CaseNumber = caseNumber;
			ChannelId = channelId;
			TriggerId = triggerId;
			ResponseUrl = responseUrl;
		}
	}
}
