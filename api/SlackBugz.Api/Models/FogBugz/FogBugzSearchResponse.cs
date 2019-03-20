#pragma warning disable 0649
using System.Runtime.Serialization;

namespace SlackBugz.Api.Models.FogBugz
{
	// Partial implementation, drawn from https://help.manuscript.com/10853/using-json-with-the-fogbugz-api
	[DataContract]
	public class FogBugzSearchResponse
	{
		[DataMember(Name = "data")]
		private FogBugzResponseData _ResponseData;
		public FogBugzResponseData ResponseData => _ResponseData;

		[DataMember(Name = "errors")]
		private FogBugzResponseError[] _Errors;
		public FogBugzResponseError[] Errors => _Errors;
	}

	[DataContract]
	public class FogBugzResponseData
	{
		[DataMember(Name = "cases")]
		private FogBugzCaseData[] _Cases;
		public FogBugzCaseData[] Cases => _Cases;
	}

	[DataContract]
	public class FogBugzCaseData
	{
		[DataMember(Name = "ixBug")]
		private int _CaseNumber;
		public int CaseNumber => _CaseNumber;

		[DataMember(Name = FogBugzCaseColumns.Title)]
		private string _Title;
		public string Title => _Title;

		[DataMember(Name = FogBugzCaseColumns.EstimatedHours)]
		private decimal _EstimatedHours;
		public decimal EstimatedHours => _EstimatedHours;

		[DataMember(Name = FogBugzCaseColumns.ElapsedHours)]
		private decimal _ElapsedHours;
		public decimal ElapsedHours => _ElapsedHours;
	}

	[DataContract]
	public class FogBugzResponseError
	{
		[DataMember(Name = "message")]
		private string _Message;
		public string Message => _Message;

		[DataMember(Name = "detail")]
		private string _Detail;
		public string Detail => _Detail;

		[DataMember(Name = "code")]
		private string _Code;
		public string Code => _Code;
	}
}
