using System;
using System.IO;
using System.Linq;

namespace Git.Files
{
    public class Commit
    {
        public Commit(string url, string commit_id)
        {
            Url = url;
            CommitId = commit_id;
        }

        public string Url { get; }
        public string CommitId { get; }

        private static string RootPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    + Path.DirectorySeparatorChar + "Git.Files";
            }
        }

        private string CommitPath { get { return RootPath + Path.DirectorySeparatorChar + CommitId; } }

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

        public Stream? GetStream(string name)
        {
            if (!Directory.Exists(CommitPath))
            {
                var dir_info = new DirectoryInfo(CommitPath);
                if (!dir_info.Parent!.Exists)
                {
                    dir_info.Parent.Create();
                }
                var clone = "git clone " + Url + " " + CommitId;
                if (Execute(clone, dir_info.Parent.ToString()) == 0)
                {
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
            return CommitPath + Path.DirectorySeparatorChar + name;
        }

        private static int Execute(string command, string directory)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var parts = command.Split(" ");
            var arguments = parts.Skip(1);

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
                    ClearReadOnly(cdi);
                    //cdi.ClearReadOnly();
                    cdi.Delete(true);
                }
                System.IO.Directory.Delete(directory.FullName, true);
            }
        }
    }
}