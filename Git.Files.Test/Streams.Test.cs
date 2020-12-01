using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Git.Files.Test
{
    [TestFixture]
    public static class StreamsTest
    {
        //[Test]
        public static void SampleFiles()
        {
            Commit.Clobber();

            var license_stream = Streams.GetStream("https://github.com/lou-parslow/SampleFiles.git@3e4b242/LICENSE");
            using (var licenseSR = new StreamReader(license_stream))
            {
                var license = licenseSR.ReadToEnd();
                Assert.True(license.Contains("MIT License"));
            }
        }
    }
}
