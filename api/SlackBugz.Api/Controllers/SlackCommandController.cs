using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlackBugz.Api.Config;
using SlackBugz.Api.Models.FogBugz;
using SlackBugz.Api.Models.Slack;

namespace SlackBugz.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[RequireHttps]
	public class SlackCommandController : ControllerBase
	{
		private readonly HttpClient WebClient;
		private readonly SlackConfig SlackConfig;
		private readonly FogBugzConfig FogBugzConfig;

		public SlackCommandController(HttpClient webClient, SlackConfig slackConfig, FogBugzConfig fogBugzConfig)
		{
			WebClient = webClient;
			SlackConfig = slackConfig;
			FogBugzConfig = fogBugzConfig;
		}

		[HttpPost]
		[Authorize(Policy = Constants.SlackSignedSecretPolicyName)]
		public ActionResult<SlashCommandResponse> DoSlashCommand([FromForm] SlackCommandInput input)
		{
			if (!input.TeamDomain.Equals(SlackConfig.Workspace, StringComparison.CurrentCultureIgnoreCase))
			{
				return Forbid();
			}
			string command = input.Command.ToLower();
			if (!SlackConfig.IsKnownCommand(command))
			{
				return BadRequest();
			}

			string caseCmd = input.Text.Trim();
			bool intParseSuccess = int.TryParse(caseCmd, out int caseNumber);
			if (!intParseSuccess)
			{
				return SlashCommandResponse.ForBadCommand(
					errorOrHelpText: "Please enter a single, numeric value.");
			}

			var jobInfo = new FogBugzSlashCommandInfo(
				command: command,
				caseNumber: caseNumber,
				channelId: input.ChannelId,
				triggerId: input.TriggerId,
				responseUrl: input.ResponseUrl.ToString());
			BackgroundJob.Enqueue(() => RespondToSlashCommand(jobInfo));

			return SlashCommandResponse.ForCommandAcknowledgement();
		}

		[NonAction]
		[AutomaticRetry(Attempts = 0)]
		public async Task RespondToSlashCommand(FogBugzSlashCommandInfo info)
		{			
			var request = new FogBugzSearchRequest(
				apiToken: FogBugzConfig.GetApiToken(),
				caseNumber: info.CaseNumber);

			FogBugzSearchResponse responseFromFogBugz;
			SlashCommandResponse responseToSlack;
			string stepName;

			/* 📜 𝕾𝖙𝖊𝖕 𝕿𝖍𝖊 𝖅𝖊𝖗𝖔𝖙𝖍 📜 */
			{
				responseToSlack = null;
				stepName = "calling the FogBugz API";
			}
			try
			{				
				using (var httpResponse = await WebClient.PostAsJsonAsync(FogBugzConfig.ApiUri, request))
				using (var message = await httpResponse.Content.ReadAsHttpResponseMessageAsync())
				{
					if (!message.IsSuccessStatusCode)
					{
						string errorText = GetFriendlyHttpErrorMsg(httpResponse);
						responseToSlack = SlashCommandResponse.ForFogBugzApiCallResult(
							text: $"There was an error calling the FogBugz API for Case {info.CaseNumber}.",
							attachments: new SlashCommandReponseAttachment(errorText, SlackAttachmentColorType.Danger));
						throw new SlashCommandException(errorText);
					}

					{
						responseToSlack = null;
						stepName = "reading the FogBugz API response";
					}
					using (var contentStream = await message.Content.ReadAsStreamAsync())
					{
						responseFromFogBugz = (FogBugzSearchResponse)new DataContractJsonSerializer(typeof(FogBugzSearchResponse))
							.ReadObject(contentStream);
					}
				}

				/* 📜 𝕾𝖙𝖊𝖕 𝕿𝖍𝖊 𝕱𝖎𝖗𝖘𝖙 📜 */
				{
					responseToSlack = null;
					stepName = "reading the FogBugz API message";
				}
				var theCase = responseFromFogBugz?.ResponseData?.Cases?
					.FirstOrDefault(c => c.CaseNumber == info.CaseNumber);
				if (theCase == null)
				{
					if (responseFromFogBugz.Errors?.Any() == true)
					{
						var error = responseFromFogBugz.Errors.First();
						responseToSlack = SlashCommandResponse.ForFogBugzApiCallResult(
							text: $"There was an error reading the FogBugz API message for Case {info.CaseNumber}.",
							attachments: new SlashCommandReponseAttachment($"{ error.Code }: { error.Message }", SlackAttachmentColorType.Danger));
						throw new SlashCommandException($"{ error.Code }: { error.Message }");
					}
					else
					{
						responseToSlack = SlashCommandResponse.ForFogBugzApiCallResult(
							text: $"Case {info.CaseNumber} does not seem to exist.");
					}
				}
				else
				{
					/* 📜 𝕾𝖙𝖊𝖕 𝕿𝖍𝖊 𝕾𝖊𝖈𝖔𝖓𝖉 📜 */
					{
						responseToSlack = null;
						stepName = "generating the command response";
					}
					(decimal Elapsed, decimal Total)? estimate = null;
					if (theCase.EstimatedHours > 0)
					{
						estimate = (Elapsed: theCase.ElapsedHours, Total: theCase.EstimatedHours);
					}
					responseToSlack = GetSuccessfulResponseText(
						caseNumber: info.CaseNumber.ToString(),
						title: theCase.Title,
						estimate: estimate);
				}
			}
			catch (Exception ex)
			{
				if (responseToSlack == null)
				{
					responseToSlack = SlashCommandResponse.ForFogBugzApiCallResult(
						text: $"There was an error {stepName} for Case {info.CaseNumber}.",
						attachments: new SlashCommandReponseAttachment(ex.Message, SlackAttachmentColorType.Danger));
				}				
				throw ex;
			}
			finally
			{
				await RespondToSlack(info.ResponseUrl, responseToSlack);
			}
		}

		private async Task RespondToSlack(string responseUrl, SlashCommandResponse response)
		{
			await WebClient.PostAsJsonAsync(responseUrl, response);
		}

		// Long-term TODO: Different, short and long format responses
		private SlashCommandResponse GetSuccessfulResponseText(string caseNumber, string title, (decimal Elapsed, decimal Total)? estimate)
		{
			var text = new StringBuilder($"{caseNumber}: {title}");
			if (estimate.HasValue)
			{
				text.Append($" ({estimate.Value.Elapsed}/{estimate.Value.Total} hours)");
			}
			string finalText = text.ToString();

			var attachments = FogBugzConfig.LinkInfos.Select(li =>
				new SlashCommandReponseAttachment(plaintextSummary: finalText, colorHex: li.PillColor)
				{
					Title = li.DisplayName,
					TitleUrl = li.GetCaseUrl(caseNumber, title).ToString(),
				});

			return SlashCommandResponse.ForFogBugzApiCallResult(
				text: finalText,
				attachments: attachments.ToArray());
		}

		private string GetFriendlyHttpErrorMsg(HttpResponseMessage response)
		{
			string msg = $"Error {(int)response.StatusCode}";
			if (!string.IsNullOrWhiteSpace(response.ReasonPhrase))
			{
				return msg + ": " + response.ReasonPhrase;
			}
			return msg + ".";
		}
	}
}
