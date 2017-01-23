using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace Pelco.Helpers
{
    public class IOHelper
    {
        /// <summary>
        /// Tests to see if a directory contains a particular file.
        /// </summary>
        /// <param name="dir">A DirectoryInfo object describing the directory.</param>
        /// <param name="file">FileInfo object describing the file.</param>
        /// <returns>True if the directory contains the file.</returns>
        /// <remarks>This method does not actually test for the existence of either the directory or the file; it merely compares the paths.</remarks>
        public static bool DirectoryContainsFile(DirectoryInfo dir, FileInfo file)
        {
            // Resolve any symbolic links in the two paths
            string resolvedDirName = ResolvePathName(dir);
            string resolvedFileDirName = ResolvePathName(file.Directory);

            return String.Equals(resolvedDirName, resolvedFileDirName, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Tests to see if a directory contains a particular file.
        /// </summary>
        /// <param name="dir">A DirectoryInfo object describing the directory.</param>
        /// <param name="fileName">A string containing the full path to the file.</param>
        /// <returns>True if the directory contains the file.</returns>
        /// <remarks>This method does not actually test for the existence of either the directory or the file; it merely compares the paths.</remarks>
        public static bool DirectoryContainsFile(DirectoryInfo dir, string fileName)
        {
            return DirectoryContainsFile(dir, new FileInfo(fileName));
        }

        /// <summary>
        /// Checks to see if a directory is local or networked.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        /// <remarks>Via http://stackoverflow.com/a/20671816.</remarks>
        public static bool DirectoryIsLocal(DirectoryInfo dir)
        {
            if (!PathIsUNC(dir.FullName))
            {
                return !PathIsNetworkPath(dir.FullName);
            }

            Uri uri = new Uri(dir.FullName);
            return IsLocalHost(uri.Host);

        }


        /// <summary>
        /// Retrieve or create a writeable directory.
        /// </summary>
        /// <param name="path">The full path to the directory.</param>
        /// <returns>A System.IO.DirectoryInfo object if the directory was created and is writeable, otherwise null.</returns>
        public static DirectoryInfo GetOrCreateDirectory(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                try
                {
                    dirInfo.Create();
                }
                catch (IOException)
                {
                }
            }

            return HasWriteAccess(dirInfo) ? dirInfo : null;
        }

        /// <summary>
        /// Retrieve or create a writeable directory.
        /// </summary>
        /// <param name="dirInfo">A DirectoryInfo object describing the directory.</param>
        /// <returns>A System.IO.DirectoryInfo object if the directory was created and is writeable, otherwise null.</returns>
        public static DirectoryInfo GetOrCreateDirectory(DirectoryInfo dirInfo)
        {
            return GetOrCreateDirectory(dirInfo.FullName);
        }

        public static bool HasAccess(string path, FileSystemRights rights)
        {
            if (String.IsNullOrWhiteSpace(path)) return false;

            try
            {
                AuthorizationRuleCollection rules = Directory.GetAccessControl(path).GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                foreach (FileSystemAccessRule rule in rules)
                {
                    if (identity.Groups.Contains(rule.IdentityReference))
                    {
                        if ((rule.FileSystemRights & rights) != 0)
                        {
                            if (rule.AccessControlType == AccessControlType.Allow) return true;
                        }
                    }
                }

                // Check for exact match failed. Membership in a Domain Users group that matches the domain of a rule
                // will also allow access.
                // For instance, membership in S-1-5-21-2172610581-1013708324-3840361364-513 and permissions in 
                // S-1-5-21-2172610581-1013708324-3840361364-1000 works.
                // See http://www.experts-exchange.com/OS/Microsoft_Operating_Systems/Windows/Q_24687164.html
                // and https://support.microsoft.com/en-us/kb/243330
                foreach (System.Security.Principal.IdentityReference group in identity.Groups)
                {
                    string groupDomain = FindDomainForDomainUserAccount(group.Value);
                    if (groupDomain != null)
                    {
                        foreach (FileSystemAccessRule rule in rules)
                        {
                            string ruleDomain = FindDomainForIdentityReference(rule.IdentityReference.Value);
                            if (ruleDomain != null && groupDomain.Equals(ruleDomain))
                            {
                                if ((rule.FileSystemRights & rights) != 0)
                                {
                                    if (rule.AccessControlType == AccessControlType.Allow) return true;
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Determines if a directory is writeable by the current process.
        /// </summary>
        /// <param name="path">A string containing the full path to the directory.</param>
        /// <returns>true if the directory is writeable, false otherwise.</returns>
        /// <remarks>Via http://stackoverflow.com/a/21996345</remarks>
        public static bool HasWriteAccess(string path)
        {
            return HasAccess(path, FileSystemRights.Write);
        }

        /// <summary>
        /// Determines if a directory is writeable by the current process.
        /// </summary>
        /// <param name="dir">A System.IO.DirectoryInfo object describing the directory.</param>
        /// <returns>true if the directory is writeable, false otherwise.</returns>
        public static bool HasWriteAccess(DirectoryInfo dir)
        {
            return HasWriteAccess(dir.FullName);
        }

        /// <summary>
        /// Determines if a directory is writeable or allows file creation by the current process.
        /// </summary>
        /// <param name="path">A string containing the full path to the directory.</param>
        /// <returns>true if the directory is writeable, false otherwise.</returns>
        /// <remarks>Via http://stackoverflow.com/a/21996345</remarks>
        public static bool HasCreateOrWriteAccess(string path)
        {
            return HasAccess(path, FileSystemRights.CreateFiles | FileSystemRights.Write);
        }

        /// <summary>
        /// Determines if a directory is writeable or allows file creation by the current process.
        /// </summary>
        /// <param name="dir">A System.IO.DirectoryInfo object describing the directory.</param>
        /// <returns>true if the directory is writeable, false otherwise.</returns>
        public static bool HasCreateOrWriteAccess(DirectoryInfo dir)
        {
            return HasCreateOrWriteAccess(dir.FullName);
        }

        /// <summary>
        /// Determines if two paths point to the same file.
        /// </summary>
        /// <param name="file1">Full path to the first file.</param>
        /// <param name="file2">Full path to the second file.</param>
        /// <returns>true if the two paths are the same, false otherwise.</returns>
        /// <remarks>This method will attempt to resolve any directory symbolic links in either path specification.</remarks>
        public static bool IsSameFile(FileInfo file1, FileInfo file2)
        {
            string resolvedPath1 = ResolvePathName(file1);
            string resolvedPath2 = ResolvePathName(file2);
            return String.Equals(resolvedPath1, resolvedPath2, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Resolves any directory symbolic links in a directory specification.
        /// </summary>
        /// <param name="path">A string containing the full path to the directory.</param>
        /// <returns>A string with all symbolic directory links resolved to actual path names.</returns>
        public static string ResolvePathName(string path)
        {
            return ResolvePathName(new DirectoryInfo(path));
        }

        /// <summary>
        /// Resolves any directory symbolic links in a directory specification.
        /// </summary>
        /// <param name="dir">A System.IO.DirectoryInfo object describing the directory.</param>
        /// <returns>A string with all symbolic directory links resolved to actual path names.</returns>
        public static string ResolvePathName(DirectoryInfo dir)
        {
            // See if the ReadReparseData DLL is present
            MethodInfo reparseReader = GetReparseReader();
            if (reparseReader == null) return dir.FullName;

            // C:\dev\pelco-sdk-installed\current  => C:\Program Files (x86)\Pelco\SDK\3.5.1-13594-vc10
            string trailingPath = String.Empty;

            // Resolve any aliases all the way up to the root
            while (dir != null)
            {
                object[] args = new object[2];
                args[0] = dir.FullName;

                bool result = (bool)reparseReader.Invoke(null, args);
                if (result)
                {
                    string resolvedPath = args[1] as string;

                    // Is this a relative symlink?
                    if (Path.IsPathRooted(resolvedPath))
                    {
                        dir = new DirectoryInfo(resolvedPath);
                    }
                    else
                    {
                        dir = new DirectoryInfo(Path.Combine(dir.Parent.FullName, resolvedPath));
                    }
                }

                trailingPath = Path.Combine(dir.Name, trailingPath);
                dir = dir.Parent;
            }

            return trailingPath;
        }

        /// <summary>
        /// Resolves any directory symbolic links in a file path specification.
        /// </summary>
        /// <param name="dir">A System.IO.FileInfo object describing the file.</param>
        /// <returns>A string with all symbolic directory links resolved to actual path names.</returns>
        public static string ResolvePathName(FileInfo file)
        {
            string resolvedDir = ResolvePathName(file.Directory);
            return Path.Combine(resolvedDir, file.Name);
        }

        private static MethodInfo GetReparseReader()
        {
            try
            {
                Assembly asm = Assembly.LoadFrom("ReadReparseData.dll");
                if (asm != null)
                {
                    Type t = asm.GetType("ReadReparseData.ReparseData");
                    if (t != null)
                    {
                        return t.GetMethod("Read");
                    }
                }
            }
            catch (FileNotFoundException)
            {
            }

            return null;
        }


        #region Private utilities

        public static string FindDomainForIdentityReference(string sid)
        {
            Regex domainAccountRgx = new Regex(@"S-1-5-21-(?<domain>.*)-\d+");

            Match m = domainAccountRgx.Match(sid);
            if (m.Success)
            {
                return m.Groups["domain"].Value;
            }

            return null;
        }

        public static string FindDomainForDomainUserAccount(string sid)
        {
            // Via https://support.microsoft.com/en-us/kb/243330 
            // SIDs in format S-1-5-21-<domain>-513 are Domain User accounts.
            Regex domainAccountRgx = new Regex(@"S-1-5-21-(?<domain>.*)-513");

            Match m = domainAccountRgx.Match(sid);
            if (m.Success)
            {
                return m.Groups["domain"].Value;
            }

            return null;
        }

        private static bool IsLocalHost(string input)
        {
            IPAddress[] host;
            //get host addresses
            try { host = Dns.GetHostAddresses(input); }
            catch (Exception) { return false; }
            //get local adresses
            IPAddress[] local = Dns.GetHostAddresses(Dns.GetHostName());
            //check if local
            return host.Any(hostAddress => IPAddress.IsLoopback(hostAddress) || local.Contains(hostAddress));
        }

        [DllImport("Shlwapi.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PathIsNetworkPath(String pszPath);

        [DllImport("Shlwapi.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PathIsUNC(String pszPath);

        #endregion
    }
}
