using System;
using System.Linq;
using System.Text;

namespace SlackBugz.Api.Config
{
	public class FogBugzConfig
	{
		public readonly string OrgName;
		public readonly Func<string> GetApiToken;
		public readonly Uri ApiUri;
		public readonly SlackLink[] LinkInfos;

		public FogBugzConfig(string orgName, Func<string> getApiToken, string apiUrl, SlackLinkConfig[] linkInfos)
		{
			OrgName = orgName;
			GetApiToken = getApiToken;
			ApiUri = new Uri(string.Format(apiUrl, orgName));
			LinkInfos = linkInfos.Select(l => new SlackLink(l, orgName)).ToArray();
		}

		public class SlackLinkConfig
		{
			internal readonly string DisplayName;
			internal readonly string BaseUrl;
			internal readonly string Color;

			public SlackLinkConfig(string displayName, string baseUrl, string color)
			{
				DisplayName = displayName;
				BaseUrl = baseUrl;
				Color = color;
			}
		}

		public class SlackLink
		{
			public readonly string DisplayName;
			public readonly string PillColor;

			private readonly Uri BaseUrl;
			private readonly int BaseUrlWithoutSlashLength;
			// so sayeth Stack Overflow:
			private const int MaxUrlLength = 200;
			private const string BasePath = "/f/cases/";

			internal SlackLink(SlackLinkConfig config, string orgName)
			{
				DisplayName = config.DisplayName;
				PillColor = config.Color;

				BaseUrl = new Uri(string.Format(config.BaseUrl, orgName));
				BaseUrlWithoutSlashLength = config.BaseUrl.Length - (config.BaseUrl.Last() == '/' ? 1 : 0);
			}

			public Uri GetCaseUrl(string caseNumber, string caseTitle)
			{
				int caseTitleCharBudget = MaxUrlLength - BaseUrlWithoutSlashLength - BasePath.Length - caseNumber.Length - 1; // (trailing slash)

				string caseTitlePart = GetCaseTitlePart(caseTitle, caseTitleCharBudget);
				return new Uri(BaseUrl, BasePath + caseNumber + "/" + caseTitlePart);
			}

			internal static string GetCaseTitlePart(string caseTitle, int charBudget)
			{
				var titlePart = new StringBuilder(charBudget);

				var thisWord = new StringBuilder();
				foreach (var ch in caseTitle)
				{
					if (!char.IsLetterOrDigit(ch))
					{
						OnPossibleWordBreak();
						continue;
					}
					thisWord.Append(ch);
					if (thisWord.Length > charBudget - titlePart.Length)
					{
						return titlePart.ToString();
					}
				}

				OnPossibleWordBreak();
				return titlePart.ToString();


				void OnPossibleWordBreak()
				{
					if (thisWord.Length > 0 && thisWord[thisWord.Length - 1] != '-')
					{
						titlePart.Append(thisWord);
						thisWord = new StringBuilder("-");
					}
				}
			}
		}
	}
}
