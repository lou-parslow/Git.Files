using NUnit.Framework;
using System;
using System.IO;

namespace Git.Files.Test
{
    public class Tests
    {
        [Test]
        public void SampleFiles()
        {
            Commit.Clobber();

            var commit = new Commit(new Uri("https://github.com/lou-parslow/Sample.Files.git"), "3e4b242");
            commit.Clean();
            Assert.True(File.Exists(commit.GetFileName("Sample.Files/Resources/Text/Lorum.Ipsum.txt")));
            var lorum_stream = commit.GetStream("Sample.Files/Resources/Text/Lorum.Ipsum.txt");
            using (var lorum = new StreamReader(lorum_stream))
            {
                var text = lorum.ReadToEnd();
                Assert.True(text.Contains("Lorem ipsum dolor sit amet"));
            }

            var lorum_filename = commit.GetFileName("Sample.Files/Resources/Text/Lorum.Ipsum.txt");
            Assert.True(File.Exists(lorum_filename));
            using (var lorum = new StreamReader(lorum_filename))
            {
                var text = lorum.ReadToEnd();
                Assert.True(text.Contains("Lorem ipsum dolor sit amet"));
            }
            commit.Clean();
            Commit.Clobber();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    + Path.DirectorySeparatorChar + "Git.Files";
            Assert.False(Directory.Exists(path));
        }

        //[Test]
        public void InvalidUrl()
        {
            var commit = new Commit(new Uri("https://github.com/lou-parslow/Invalid.Sample.Files.git"), "3e4b242");
            Assert.Throws<Exception>(() => commit.GetFileName("Sample.Files/Resources/Text/Lorum.Ipsum.txt"));
        }
        //[Test]
        public void InvalidCommit()
        {
            var commit = new Commit("https://github.com/lou-parslow/Sample.Files.git", "xe4b242");
            Assert.Throws<Exception>(() => commit.GetFileName("Sample.Files/Resources/Text/Lorum.Ipsum.txt"));
        }
    }
}