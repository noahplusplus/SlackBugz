using System.Runtime.Serialization;

namespace SlackBugz.Api.Models.FogBugz
{
	[DataContract]
	public class FogBugzSearchRequest
	{
		[DataMember(Name = "cmd")]
		public const string Command = "search";
		[DataMember(Name = "cols")]
		public readonly string[] Columns = new[] { FogBugzCaseColumns.Title, FogBugzCaseColumns.EstimatedHours, FogBugzCaseColumns.ElapsedHours };
		[DataMember(Name = "token")]
		public readonly string ApiToken;
		[DataMember(Name = "q")]
		public readonly string CaseNumber;

		public FogBugzSearchRequest(string apiToken, int caseNumber)
		{
			ApiToken = apiToken;
			CaseNumber = caseNumber.ToString();
		}
	}
}
