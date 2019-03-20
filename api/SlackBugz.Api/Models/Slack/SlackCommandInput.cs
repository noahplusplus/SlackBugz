using System;
using System.Runtime.Serialization;

namespace SlackBugz.Api.Models.Slack
{
	[DataContract]
	public class SlackCommandInput
	{
		[DataMember(Name = "team_id")]
		public string TeamId { get; private set; }
		[DataMember(Name = "team_domain")]
		public string TeamDomain { get; private set; }
		[DataMember(Name = "enterprise_id")]
		public string EnterpriseId { get; private set; }
		[DataMember(Name = "enterprise_name")]
		public string EnterpriseName { get; private set; }
		[DataMember(Name = "channel_id")]
		public string ChannelId { get; private set; }
		[DataMember(Name = "channel_name")]
		public string ChannelName { get; private set; }
		[DataMember(Name = "user_id")]
		public string UserId { get; private set; }
		[DataMember(Name = "user_name")]
		public string Username { get; private set; }
		[DataMember(Name = "command")]
		public string Command { get; private set; }
		[DataMember(Name = "text")]
		public string Text { get; private set; }
		[DataMember(Name = "response_url")]
		public Uri ResponseUrl { get; private set; }
		[DataMember(Name = "triger_id")]
		public string TriggerId { get; private set; }
	}
}
