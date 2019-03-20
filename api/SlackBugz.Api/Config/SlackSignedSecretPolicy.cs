using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SlackBugz.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace SlackBugz.Api.Config
{
	public sealed class SlackSignedSecretRequirement : IAuthorizationRequirement
	{ }

	public class SlackSignedSecretConfig
	{
		public readonly Func<string> GetConfiguredSecret;
		public readonly Func<DateTime> GetUtcNow;

		public SlackSignedSecretConfig(Func<string> getConfiguredSecret, Func<DateTime> getUtcNow)
		{
			GetConfiguredSecret = getConfiguredSecret;
			GetUtcNow = getUtcNow;
		}
	}

	public sealed class SlackSignedSecretHandler : AuthorizationHandler<SlackSignedSecretRequirement>
	{
		private readonly IHttpContextAccessor ContextAccessor;
		private readonly Func<string> GetConfiguredSecret;
		private readonly Func<DateTime> GetUtcNow;

		public SlackSignedSecretHandler(IHttpContextAccessor contextAccessor, SlackSignedSecretConfig config)
		{
			ContextAccessor = contextAccessor;
			GetConfiguredSecret = config.GetConfiguredSecret;
			GetUtcNow = config.GetUtcNow;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SlackSignedSecretRequirement requirement)
		{
			string configuredSecret = GetConfiguredSecret();
			if (string.IsNullOrWhiteSpace(configuredSecret))
			{
				throw new ArgumentException("The configured secret could not be retrieved. Authorization cannot occur.");
			}

			var request = ContextAccessor.HttpContext.Request;
			string requestSignature = request.Headers["X-Slack-Signature"];				
			string requestTimestamp = request.Headers["X-Slack-Request-Timestamp"];

/*
			 🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥
			⚙️                                                             ⚙️
			 🌻   ☞  B  A  S  I  C      V  A  L  I  D  A  T  I  O  N  ☜    🌻
			⚙️                                                             ⚙️
			  ✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️      */
			{
				var theTwoThingsWeNeed = new[] { requestSignature, requestTimestamp };
				if (theTwoThingsWeNeed.Any(thing => string.IsNullOrWhiteSpace(thing)))
				{
					return Task.CompletedTask;
				}
				if (!long.TryParse(requestTimestamp, out long ticks))
				{
					return Task.CompletedTask;
				}
				if (!SlackSignatureUtilities.IsTimestampAboutNow(ticks, GetUtcNow()))
				{
					return Task.CompletedTask;
				}
			}
/*
			 🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥🚦🚥
			⚙️                                                             ⚙️
			 🌻   ☞  S  I  G       V  E  R  I  F  I  C  A  T  I  O  N  ☜   🌻
			⚙️                                                             ⚙️
			  ✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️❌✔️     */
			{
				string requestBody = null;
				using (var reader = new BeKindPleaseRewindStreamReader(request.Body))
				{
					requestBody = reader.ReadToEnd();
				}

				string computedSignature = SlackSignatureUtilities.ComputeSignature(
					timestamp: requestTimestamp,
					body: requestBody,
					secret: configuredSecret);
				if (string.Compare(computedSignature, requestSignature, ignoreCase: true) != 0)
				{
					return Task.CompletedTask;
				}
			}
			context.Succeed(requirement);
			return Task.CompletedTask;
		}
	}

	internal static class SlackSignatureUtilities
	{
		public static bool IsTimestampAboutNow(long timestampTicks, DateTime utcNow)
		{
			long aShortTimeAgo = utcNow.AddMinutes(-2).Ticks;
			long aShortTimeIntoTheFuture = utcNow.AddMinutes(2).Ticks;


			return timestampTicks >= aShortTimeAgo && timestampTicks <= aShortTimeIntoTheFuture;
		}

		public static string ComputeSignature(string timestamp, string body, string secret)
		{
			string basestring = GetBaseString(timestamp, body);
			var encoding = Encoding.UTF8;
			var msgBytes = encoding.GetBytes(basestring);
			var keyBytes = encoding.GetBytes(secret);

			byte[] hashBytes;
			using (var hash = new HMACSHA256(keyBytes))
			{
				hashBytes = hash.ComputeHash(msgBytes);
			}

			return "v0=" + BitConverter.ToString(hashBytes).Replace("-", string.Empty);
		}

		internal static string GetBaseString(string timestamp, string body)
		{
			return string.Join(':', "v0", timestamp, body);
		}
	}
}
