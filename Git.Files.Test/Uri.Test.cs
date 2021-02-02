using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Git.Files
{
	[TestFixture]
	internal class UriTest
	{
		[Test]
		public void Usage()
		{
			string logFilename = new FileInfo(typeof(UriTest).Assembly.Location).Directory.Parent.Parent.Parent.Parent.FullName
				+ Path.DirectorySeparatorChar + "Uri.md";

			List<string> urls = new List<string>
				{
					"https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName",
					logFilename
				};

			using (var log = new StreamWriter(logFilename))
			{
				foreach (string url in urls)
				{
					Uri uri = new Uri(url);
					log.WriteLine($"AbsolutePath: {uri.AbsolutePath}");
					log.WriteLine($"AbsoluteUri: {uri.AbsoluteUri}");
					log.WriteLine($"DnsSafeHost: {uri.DnsSafeHost}");
					log.WriteLine($"Fragment: {uri.Fragment}");
					log.WriteLine($"Host: {uri.Host}");
					log.WriteLine($"HostNameType: {uri.HostNameType}");
					log.WriteLine($"IdnHost: {uri.IdnHost}");
					log.WriteLine($"IsAbsoluteUri: {uri.IsAbsoluteUri}");
					log.WriteLine($"IsDefaultPort: {uri.IsDefaultPort}");
					log.WriteLine($"IsFile: {uri.IsFile}");
					log.WriteLine($"IsLoopback: {uri.IsLoopback}");
					log.WriteLine($"IsUnc: {uri.IsUnc}");
					log.WriteLine($"LocalPath: {uri.LocalPath}");
					log.WriteLine($"OriginalString: {uri.OriginalString}");
					log.WriteLine($"PathAndQuery: {uri.PathAndQuery}");
					log.WriteLine($"Port: {uri.Port}");
					log.WriteLine($"Query: {uri.Query}");
					log.WriteLine($"Scheme: {uri.Scheme}");
					log.WriteLine($"Segments: {string.Join(", ", uri.Segments)}");
					log.WriteLine($"UserEscaped: {uri.UserEscaped}");
					log.WriteLine($"UserInfo: {uri.UserInfo}");
				}
			}
		}
	}
}