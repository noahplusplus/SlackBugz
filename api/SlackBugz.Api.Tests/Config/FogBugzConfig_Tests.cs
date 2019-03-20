using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static SlackBugz.Api.Config.FogBugzConfig;

namespace SlackBugz.Api.Tests.Config
{
	public class FogBugzConfig_Tests
	{
		// 8 + 167 + 1 = 176.
		private static readonly string BaseUrl176Chars = "https://" + string.Concat(Enumerable.Repeat('x', 167)) + "/";

		// 176 + 8 + 5 + 1 = 190.
		private static readonly string CaseNumber = "12345";
		private static readonly string ExpectedUrlWithCaseNumber = BaseUrl176Chars + "f/cases/" + CaseNumber + "/";

		private static readonly SlackLink TheLink = new SlackLink(
			config: new SlackLinkConfig(
				displayName: null,
				baseUrl: BaseUrl176Chars,
				color: null),
			orgName: null);

		// Given a URL length limit of 200, our tests shall expect
		//  a case description of over 200-190=10 char to always be truncated.
		public static IEnumerable<object[]> TitlePartTestCases = new[]
		{
			new object[] { string.Empty, ExpectedUrlWithCaseNumber },
			new object[] { "@-}--", ExpectedUrlWithCaseNumber },
			new object[] { "a1234567890", ExpectedUrlWithCaseNumber },
			new object[] { "0123456789ab", ExpectedUrlWithCaseNumber },
			new object[] { "x123456789", ExpectedUrlWithCaseNumber + "x123456789" },
			new object[] { "x123456789,", ExpectedUrlWithCaseNumber + "x123456789" },
			new object[] { "x,123456789", ExpectedUrlWithCaseNumber + "x" },
			new object[] { "x,,123 456789", ExpectedUrlWithCaseNumber + "x-123" },
			new object[] { "Fo ba Ba", ExpectedUrlWithCaseNumber + "Fo-ba-Ba" },
		};

		[Theory]
		[MemberData(nameof(TitlePartTestCases))]
		public void SlackLink_TitlePart_Test(string caseTitle, string expected)
		{
			Uri result = TheLink.GetCaseUrl(CaseNumber, caseTitle);
			Assert.Equal(expected, result.ToString());
		}
	}
}
