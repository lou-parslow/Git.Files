using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Git.Files
{
    public class Commit
    {
        public Commit(Uri url, string commitId)
        {
            Url = url;
            CommitId = commitId;
        }

        public Commit(string url,string commitId)
		{
            Url = new Uri(url);
            CommitId = commitId;
		}

        public Uri Url { get; }
        public string CommitId { get; }

        private static string RootPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                        + Path.DirectorySeparatorChar + "Git.Files";
                /*
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                        + Path.DirectorySeparatorChar + "Git.Files";
                    return path;
                }
                catch(Exception e)
                {
                    throw new Exception("Unable to setup RootPath for Git.Files", e);
                }*/
            }
        }

        private string CommitPath { get 
            {
                //try
                //{
                    string pathUrl = Url.ToString().Replace("://", ".");
                    return RootPath + Path.DirectorySeparatorChar + pathUrl + Path.DirectorySeparatorChar + CommitId;
                //}
                //catch(Exception e)
                //{
                //    throw new Exception("Unable to setup CommitPath, RootPath: " + RootPath + "Url: " + Url + " CommitId:" + CommitId, e);
                //}
            } 
        }

        public static void Clobber()
        {
            if (Directory.Exists(RootPath))
            {
                var dir_info = new DirectoryInfo(RootPath);
                DeleteAll(dir_info);
            }
        }

        public void Clean()
        {
            if (Directory.Exists(CommitPath))
            {
                var dir_info = new DirectoryInfo(CommitPath);
                DeleteAll(dir_info);
            }
        }

        private void SetupCommitPath()
        {
            if (!Directory.Exists(CommitPath))
            {
                try
                {
                    var dir_info = new DirectoryInfo(CommitPath);
                    if (!dir_info.Parent!.Exists)
                    {
                        System.IO.Directory.CreateDirectory(dir_info.Parent.FullName);
                        dir_info.Parent.Create();
                    }
                    var clone = "git clone " + Url + " " + CommitId;
                    if (Execute(clone, dir_info.Parent.FullName) == 0)
                    {
                        if (!Directory.Exists(CommitPath))
                        {
                            throw new Exception("Commit Path " + CommitPath + " does not exist after " + clone + " in " + dir_info.Parent.FullName);
                        }
                        var checkout = "git checkout " + CommitId;
                        if (Execute(checkout, CommitPath) == 0)
                        {
                            
                            SetReadOnly(dir_info);
                        }
                        else
                        {
                            throw new Exception("command '" + checkout + "' failed");
                        }
                    }
                    else
                    {
                        Clean();
                        throw new Exception("command '" + clone + "' failed");
                    }
                }
                catch(Exception e)
                {
                    var dir_info = new DirectoryInfo(CommitPath);
                    DeleteAll(dir_info);
                    if (dir_info.Parent!.Exists && dir_info.Parent.GetDirectories().Length == 0)
                    {
                        dir_info.Parent.Delete();
                    }
                    throw new Exception("Error setting up " + Url + "@" + CommitId ,e);
                }
            }
        }
        public Stream? GetStream(string name)
        {
            SetupCommitPath();
            var filename = GetFileName(name);
            if (File.Exists(filename))
            {
                using var fs = new FileStream(GetFileName(name), FileMode.Open, FileAccess.Read);
                var memory = new MemoryStream();
                fs.CopyTo(memory);
                fs.Close();
                memory.Seek(0, SeekOrigin.Begin);
                return memory;
            }
            return null;
        }

        public string GetFileName(string name)
        {
            SetupCommitPath();
            string filename = CommitPath + Path.DirectorySeparatorChar + name;
            if(!File.Exists(filename))
			{
                throw new Exception($"file {filename} does not exist.");
			}
            return filename;
        }

        private static int Execute(string command, string directory)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var parts = command.Split(' ');
            var arguments = parts.Skip(1);

            var exe_name = GetExecutableFilename(parts[0]);
            if(!File.Exists(exe_name))
            {
                throw new Exception("Unable to determine executable filename for " + parts[0]);
            }
            if (!Directory.Exists(directory))
            {
                throw new Exception("Directory " + directory + " does not exist");
            }
            System.Diagnostics.Debug.WriteLine("command " + parts[0]);
            System.Diagnostics.Debug.WriteLine("arguments " + String.Join(" ", arguments));
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = GetExecutableFilename(parts[0]),
                    Arguments = String.Join(" ", arguments)
                }
            };
            process.StartInfo.WorkingDirectory = directory;
            process.Start();
            using StreamReader outputReader = process.StandardOutput;
            var output = outputReader.ReadToEnd();
            using StreamReader errorReader = process.StandardError;
            var error = errorReader.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode;
        }

        private static string GetExecutableFilename(string name)
        {
            var delimiter = IsWindows ? ";" : ":";
            string? pathstr = Environment.GetEnvironmentVariable("PATH");
            if (pathstr != null)
            {
                foreach (var path in pathstr.Split(delimiter.ToCharArray()))
                {
                    var exe = $"{path}{Path.DirectorySeparatorChar}{name}.exe";
                    if (File.Exists(exe)) return exe;
                    exe = $"{path}{Path.DirectorySeparatorChar}{name}.bat";
                    if (File.Exists(exe)) return exe;
                    exe = $"{path}{Path.DirectorySeparatorChar}{name}.cmd";
                    if (File.Exists(exe)) return exe;
                    exe = $"{path}{Path.DirectorySeparatorChar}{name}";
                    if (File.Exists(exe)) return exe;
                }
            }
            return string.Empty;
        }

        private static bool IsWindows
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT;
            }
        }

        private static void ClearReadOnly(DirectoryInfo directory)
        {
            if (directory!.Exists)
            {
                directory.Attributes = FileAttributes.Normal;
                foreach (FileInfo fi in directory.GetFiles())
                {
                    fi.Attributes = FileAttributes.Normal;
                }
                foreach (DirectoryInfo cdi in directory.GetDirectories())
                {
                    ClearReadOnly(cdi);
                }
            }
        }

        private static void SetReadOnly(DirectoryInfo directory)
        {
            if (directory!.Exists)
            {
                directory.Attributes = FileAttributes.Normal;
                foreach (FileInfo fi in directory.GetFiles())
                {
                    fi.Attributes = FileAttributes.ReadOnly;
                }
                foreach (DirectoryInfo cdi in directory.GetDirectories())
                {
                    SetReadOnly(cdi);
                }
            }
        }

        private static void DeleteAll(DirectoryInfo directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (directory.Exists)
            {
                ClearReadOnly(directory);// directory.ClearReadOnly();
                directory.Attributes = FileAttributes.Normal;
                foreach (FileInfo fi in directory.GetFiles())
                {
                    fi.Attributes = FileAttributes.Normal;
                    fi.Delete();
                }
                foreach (DirectoryInfo cdi in directory.GetDirectories())
                {
                    if (cdi.Exists)
                    {
                        ClearReadOnly(cdi);
                        //cdi.ClearReadOnly();
                        try
                        {
                            cdi.Delete(true);
                        }
                        catch { }
                    }
                }
                try
                {
                    System.IO.Directory.Delete(directory.FullName, true);
                }
                catch { }
            }
        }
    }
}