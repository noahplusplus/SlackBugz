using System;
using System.Collections.Generic;
using SlackBugz.Api.Config;
using Xunit;

namespace SlackBugz.Api.Tests.Config
{
	public class SlackSignedSecretPolicy_Tests
	{
		private static DateTime TheHistoricDate = new DateTime(1985, 10, 21, 22, 04, 00);
		private static DateTime RightAboutNow = TheHistoricDate.AddMinutes(-1);
		private static DateTime RightAboutThen = TheHistoricDate.AddMinutes(1);
		private static DateTime BackInThePast = TheHistoricDate.AddMinutes(-5);
		private static DateTime BackToTheFuture = TheHistoricDate.AddMinutes(5);
		public static IEnumerable<object[]> TimestampTestCases = new[]
		{
			new object[] { false, TheHistoricDate, BackInThePast },
			new object[] { false, TheHistoricDate, BackToTheFuture },
			new object[] { true, TheHistoricDate, TheHistoricDate },
			new object[] { true, TheHistoricDate, RightAboutNow },
			new object[] { true, TheHistoricDate, RightAboutThen },
		};

		[Theory]
		[MemberData(nameof(TimestampTestCases))]
		public void IsTimestampAboutNow_Test(bool expected, DateTime now, DateTime timeStamp)
		{
			Assert.Equal(expected, SlackSignatureUtilities.IsTimestampAboutNow(timeStamp.Ticks, now));
		}

		[Theory]
		[InlineData(ExampleBaseString, ExampleTimestamp, ExampleBody)]
		public void GetBaseString_Test(string expected, string timestamp, string body)
		{
			string actual = SlackSignatureUtilities.GetBaseString(timestamp, body);
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(ExampleSignature, ExampleTimestamp, ExampleBody, ExampleSecret)]
		public void ComputeSignature_Test(string expected, string timestamp, string body, string secret)
		{
			string actual = SlackSignatureUtilities.ComputeSignature(timestamp, body, secret);
			Assert.Equal(expected.ToLower(), actual.ToLower());
		}

		// Example from https://api.slack.com/docs/verifying-requests-from-slack
		private const string ExampleSignature = "v0=a2114d57b48eac39b9ad189dd8316235a7b4a8d21a10bd27519666489c69b503";
		private const string ExampleTimestamp = "1531420618";
		private const string ExampleSecret = "8f742231b10e8888abcd99yyyzzz85a5";
		private const string ExampleBody = "token=xyzz0WbapA4vBCDEFasx0q6G&team_id=T1DC2JH3J&team_domain=testteamnow&channel_id=G8PSS9T3V&channel_name=foobar&user_id=U2CERLKJA&user_name=roadrunner&command=%2Fwebhook-collect&text=&response_url=https%3A%2F%2Fhooks.slack.com%2Fcommands%2FT1DC2JH3J%2F397700885554%2F96rGlfmibIGlgcZRskXaIFfN&trigger_id=398738663015.47445629121.803a0bc887a14d10d2c447fce8b6703c";
		private const string ExampleBaseString = "v0:1531420618:token=xyzz0WbapA4vBCDEFasx0q6G&team_id=T1DC2JH3J&team_domain=testteamnow&channel_id=G8PSS9T3V&channel_name=foobar&user_id=U2CERLKJA&user_name=roadrunner&command=%2Fwebhook-collect&text=&response_url=https%3A%2F%2Fhooks.slack.com%2Fcommands%2FT1DC2JH3J%2F397700885554%2F96rGlfmibIGlgcZRskXaIFfN&trigger_id=398738663015.47445629121.803a0bc887a14d10d2c447fce8b6703c";
	}
}
