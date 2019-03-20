using System;
using SlackBugz.Api.Models.Slack;
using Xunit;

namespace SlackBugz.Api.Tests.Models.Slack
{
	public class SlackCommandResponse_Tests
	{
		[Theory]
		[InlineData(null, null)]
		[InlineData("", null)]
		[InlineData("#", null)]
		[InlineData("#g6", null)]
		[InlineData("#g12345", null)]
		[InlineData("12345h", null)]
		[InlineData("#012#5", null)]
		[InlineData("#1234567", null)]
		[InlineData("1234567", null)]
		[InlineData("#a", "#00000a")]
		[InlineData("#A", "#00000A")]
		[InlineData("1", "#000001")]
		[InlineData("034d", "#00034d")]
		[InlineData("#f6", "#0000f6")]
		[InlineData("#0123ef", "#0123ef")]
		[InlineData("FE3210", "#FE3210")]
		public void ParseColorString_Test(string input, string expected)
		{
			Func<string> test = () =>
				SlashCommandReponseAttachment.ParseColorString(input);
			if (expected == null)
			{
				Assert.Throws<ArgumentOutOfRangeException>(test);
			}
			else
			{
				string result = test.Invoke();
				Assert.Equal(expected.ToLower(), result.ToLower());
			}
		}
	}
}
