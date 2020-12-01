using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Git.Files
{
    public static class Streams
    {
        public static Stream? GetStream(string name)
        {
            if(Streams.GetCommit(name) is Commit commit)
            {
                var url = name.Split("@")[0];
                var commit_id = name.Split("@")[1].Split("/")[0];
                var rel_path = name.Replace(url + "@" + commit_id, "").Substring(1);
                return commit.GetStream(rel_path);
            }
            return null;
        }

        public static string GetFileName(string name)
        {
            if (Streams.GetCommit(name) is Commit commit)
            {
                var url = name.Split("@")[0];
                var commit_id = name.Split("@")[1].Split("/")[0];
                var rel_path = name.Replace(url + "@" + commit_id, "").Substring(1);
                return commit.GetFileName(rel_path);
            }
            return string.Empty;
        }

        public static Commit? GetCommit(string name)
        {
            if (name.Contains("@"))
            {
                var url = name.Split("@")[0];
                var commit_id = name.Split("@")[1].Split("/")[0];
                var rel_path = name.Replace(url + "@" + commit_id, "").Substring(1);
                return new Commit(url, commit_id);
            }
            
            return null;
        }

    }
}
