using System;
using System.IO;

namespace SlackBugz.Api.Utilities
{
	public class BeKindPleaseRewindStreamReader : StreamReader, IDisposable
	{
		private readonly Stream TheStream;
		private readonly Action<Stream> OnDispose;

		public BeKindPleaseRewindStreamReader(Stream stream, Action<Stream> onDispose = null)
			: base(stream)
		{
			TheStream = stream;
			OnDispose = onDispose;
		}

		void IDisposable.Dispose()
		{
			TheStream.Seek(0, SeekOrigin.Begin);
			if (OnDispose != null)
			{
				OnDispose.Invoke(TheStream);
			}
			base.Dispose();
		}
	}
}
