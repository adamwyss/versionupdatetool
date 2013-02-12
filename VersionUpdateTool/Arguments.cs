using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionUpdateTool
{
    public static class Arguments
    {
        private static readonly string[] ArgumentDelimiters = new string[] { ":" };

        public static string RootPath { get; private set; }

        public static string ManifestPath { get; private set; }

        public static string ApplicationKey { get; private set; }

        public static bool Parse(string[] args)
        {
            foreach (string arg in args)
            {
                string[] kvp = arg.Split(ArgumentDelimiters, 2, StringSplitOptions.None);
                string key = kvp[0];
                if (key.StartsWith("/") || key.StartsWith("-"))
                {
                    key = key.Substring(1).ToLowerInvariant();
                }
                string value = null;
                if (kvp.Length > 1)
                {
                    value = kvp[1];
                }

                switch (key)
                {
                    case "root":
                        RootPath = value;
                        break;

                    case "manifest":
                        ManifestPath = value;
                        break;

                    case "key":
                        ApplicationKey = value;
                        break;

                    default:
                        Console.WriteLine("The argument '{0}' is invalid.", key);
                        break;
                }

            }

            if (RootPath == null)
            {
                RootPath = Directory.GetCurrentDirectory();
                Console.WriteLine("No root was specified, using '{0}'", RootPath);
            }

            if (!Directory.Exists(RootPath))
            {
                Console.WriteLine("The directory '{0}' must exist!");
                return false;
            }

            if (ManifestPath == null)
            {
                IEnumerable<string> win8 = Directory.EnumerateFiles(RootPath, "Package.appxmanifest", SearchOption.AllDirectories);
                IEnumerable<string> wp = Directory.EnumerateFiles(RootPath, "WMAppManifest.xml", SearchOption.AllDirectories);

                List<string> allManifestsInRoot = new List<string>();

                foreach (string manifestPath in win8)
                {
                    if (manifestPath.IndexOf("\\bin\\debug\\", StringComparison.OrdinalIgnoreCase) == -1 &&
                        manifestPath.IndexOf("\\bin\\release\\", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        allManifestsInRoot.Add(manifestPath);
                    }
                }

                foreach (string manifestPath in wp)
                {
                    if (manifestPath.IndexOf("\\bin\\debug\\", StringComparison.OrdinalIgnoreCase) == -1 &&
                        manifestPath.IndexOf("\\bin\\release\\", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        allManifestsInRoot.Add(manifestPath);
                    }
                }


                if (allManifestsInRoot.Count == 0)
                {
                    Console.WriteLine("No manifest files were found in the directory");
                    return false;
                }

                if (allManifestsInRoot.Count > 1)
                {
                    Console.WriteLine("Mulitple manifest files were found, use the /manifest:{relative path} argument to specify the correct one.");
                    return false;
                }

                ManifestPath = allManifestsInRoot[0];
                Console.WriteLine("No manifest was specified, using '{0}'", ManifestPath);
            }

            if (ApplicationKey == null)
            {
                ApplicationKey = Path.GetFileName(RootPath);
                Console.WriteLine("No key was specified, using '{0}'", ApplicationKey);
            }

            return true;
        }

        public static void ShowUsage()
        {
            Console.WriteLine("v1.2");
            Console.WriteLine("[/root:<absolutepath>] [/manifest:<relativepath>] [/key:<somevalue>]");
        }
    }
}
