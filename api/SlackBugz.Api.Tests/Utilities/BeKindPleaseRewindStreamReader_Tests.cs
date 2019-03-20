using System.IO;
using System.Text;
using SlackBugz.Api.Utilities;
using Xunit;

namespace SlackBugz.Api.Tests.Utilities
{
	public class BeKindPleaseRewindStreamReader_Tests
	{
		[Fact]
		public void RewindsOnDispose()
		{
			long streamPos = -1;
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("Lipbalm Oram")))
			using (var streamReader = new BeKindPleaseRewindStreamReader(
					stream: stream,
					onDispose: s => streamPos = s.Position))
			{
				streamReader.ReadToEnd();
			}
			Assert.Equal(0, streamPos);
		}
	}
}
