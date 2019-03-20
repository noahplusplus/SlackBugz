using System;
using System.Linq;
using System.Runtime.Serialization;

namespace SlackBugz.Api.Models.Slack
{
	// Long-term TODO: Use blocks API instead of this
	// https://api.slack.com/messaging/composing/layouts
	[DataContract]
	public class SlashCommandResponse
	{
		[DataMember(Name = "response_type")]
		public readonly string ResponseType;

		[DataMember(Name = "text")]
		public readonly string ResponseText;

		[DataMember(Name = "attachments")]
		public readonly SlashCommandReponseAttachment[] Attachments;

		private SlashCommandResponse(SlackCommandResponseType responseType, string responseText,
			params SlashCommandReponseAttachment[] attachments)
		{
			ResponseType = responseType.ToApiString();
			ResponseText = responseText;
			Attachments = attachments;
		}

		/// <summary>
		/// Acknowledging the slash command with this method causes the command
		/// to be displayed in channel. Without an immediate acknowledgment like this,
		/// the slash command may have the appearance of silently failing.
		/// </summary>
		public static SlashCommandResponse ForCommandAcknowledgement()
		{
			return new SlashCommandResponse(SlackCommandResponseType.InChannel, null, null);
		}

		/// <summary>
		/// Acknowledging the slash command with this method sends an error or help text
		/// to the issuing user. Without an immediate acknowledgment like this,
		/// the slash command may have the appearance of silently failing.
		/// </summary>
		public static SlashCommandResponse ForBadCommand(string errorOrHelpText,
			params SlashCommandReponseAttachment[] errorOrHelpAttachments)
		{
			return new SlashCommandResponse(SlackCommandResponseType.Ephemeral,
				errorOrHelpText, errorOrHelpAttachments);
		}

		public static SlashCommandResponse ForFogBugzApiCallResult(string text,
			params SlashCommandReponseAttachment[] attachments)
		{
			return new SlashCommandResponse(SlackCommandResponseType.InChannel,
				text, attachments);
		}
	}

	// https://api.slack.com/docs/message-attachments
	[DataContract]
	public class SlashCommandReponseAttachment
	{
		[DataMember(Name = "fallback")]
		public readonly string PlaintextSummary;
		[DataMember(Name = "color")]
		public readonly string Color;
		[DataMember(Name = "pretext")]
		public string Pretext { get; set; }
		[DataMember(Name = "author_name")]
		public string AuthorName { get; set; }
		[DataMember(Name = "author_link")]
		public string AuthorUrl { get; set; }
		[DataMember(Name = "author_icon")]
		public string AuthorIconUrl { get; set; }
		[DataMember(Name = "title")]
		public string Title { get; set; }
		[DataMember(Name = "title_link")]
		public string TitleUrl { get; set; }
		[DataMember(Name = "text")]
		public string Text { get; set; }
		[DataMember(Name = "fields")]
		public SlashCommandReponseAttachmentField[] Fields { get; set; }
		[DataMember(Name = "image_url")]
		public string ImageUrl { get; set; }
		[DataMember(Name = "thumb_url")]
		public string ThumbnailUrl { get; set; }
		[DataMember(Name = "footer")]
		public string FooterText { get; set; }
		[DataMember(Name = "footer_icon")]
		public string FooterIconUrl { get; set; }
		[DataMember(Name = "ts")]
		public long DisplayTime { get; set; }

		public SlashCommandReponseAttachment(string plaintextSummary)
		{
			PlaintextSummary = plaintextSummary;
			Color = null;
		}

		public SlashCommandReponseAttachment(string plaintextSummary, SlackAttachmentColorType colorType)
		{
			switch (colorType)
			{
				case SlackAttachmentColorType.Danger:
					Color = "danger";
					break;
				case SlackAttachmentColorType.Good:
					Color = "good";
					break;
				case SlackAttachmentColorType.Warning:
					Color = "warning";
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(colorType), actualValue: colorType, message: string.Empty);
			}
			PlaintextSummary = plaintextSummary;
		}

		public SlashCommandReponseAttachment(string plaintextSummary, string colorHex)
		{
			Color = ParseColorString(colorHex);
			PlaintextSummary = plaintextSummary;
		}

		internal static string ParseColorString(string colorHex)
		{
			const string hexChars = "01234567890aAbBcCdDeEfF";
			const string argError = "Argument must be 1-6 hex characters, optionally preceded by '#'.";

			if (colorHex == null)
			{
				goto BadArgError;
			}

			int len = colorHex.Length;
			if (len < 1
				|| len > 7 
				|| (len == 7 && colorHex[0] != '#')
				|| (len == 1 && !hexChars.Contains(colorHex[0])))
			{
				goto BadArgError;
			}

			char[] parsedColor = Enumerable.Repeat('#', 7).ToArray();
			int outIdx = 6;
			int inIdx = len - 1;
			while (inIdx >= 0)
			{
				char inputCh = colorHex[inIdx];
				if (outIdx == 0 && inputCh != '#')
				{
					goto BadArgError;
				}
				else if (inputCh == '#')
				{
					if (inIdx != 0)
					{
						goto BadArgError;
					}
					else
					{
						break;
					}
				}
				else if (!hexChars.Contains(inputCh))
				{
					goto BadArgError;
				}
				else
				{
					parsedColor[outIdx] = inputCh;
				}
				--outIdx;
				--inIdx;
			}

			while (outIdx > 0)
			{
				parsedColor[outIdx] = '0';
				--outIdx;
			}
			return new string(parsedColor);

		BadArgError:
			throw new ArgumentOutOfRangeException(nameof(colorHex), actualValue: colorHex, message: argError);
		}
	}

	[DataContract]
	public class SlashCommandReponseAttachmentField
	{
		[DataMember(Name = "title")]
		public readonly string Title;
		[DataMember(Name = "value")]
		public readonly string Content;
		[DataMember(Name = "short")]
		public readonly bool? IsShort;

		public SlashCommandReponseAttachmentField(string title, string content, bool? isShort = null)
		{
			Title = title;
			Content = content;
			IsShort = isShort;
		}
	}

	public enum SlackAttachmentColorType
	{
		Good, Warning, Danger,
	}

	public enum SlackCommandResponseType
	{
		InChannel,
		Ephemeral,
	}

	public static class SlackCommandResponseExtensions
	{
		public static string ToApiString(this SlackCommandResponseType enumVal)
		{
			switch (enumVal)
			{
				case SlackCommandResponseType.InChannel:
					return "in_channel";
				case SlackCommandResponseType.Ephemeral:
					return "ephemeral";
				default:
					throw new ArgumentOutOfRangeException(nameof(enumVal), enumVal.ToString());
			}
		}
	}
}
