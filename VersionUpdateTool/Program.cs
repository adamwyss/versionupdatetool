using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VersionUpdateTool
{
    // git submodule foreach git reset --hard && git reset --hard

    public delegate bool ManifestUpdateDelegate(string manifestPath, out Version version);

    public enum ExitCode : int
    {
        Success = 0,
        Failure = 1,
    }

    public class Program
    {
        private static readonly Dictionary<string, ManifestUpdateDelegate> ManifestUpdate = new Dictionary<string, ManifestUpdateDelegate>() 
            {
                { "Package.appxmanifest", ManifestParser.UpdateWindowsManifest },
                { "WMAppManifest.xml", ManifestParser.UpdateWindowsPhoneManifest }
            };


        public static int Main(string[] args)
        {
            bool success = Arguments.Parse(args);
            if (success)
            {
                string fullPathToManifest = Path.Combine(Arguments.RootPath, Arguments.ManifestPath);
                string manifestFile = Path.GetFileName(Arguments.ManifestPath);

                ManifestUpdateDelegate updateDelegate;
                success = ManifestUpdate.TryGetValue(manifestFile, out updateDelegate);
                if (success)
                {
                    Version version;
                    success = updateDelegate(fullPathToManifest, out version);
                    if (success)
                    {
                        success = UpdateAssemblyInfoVersions(Arguments.RootPath, version);
                        if (success)
                        {
                            Console.WriteLine("The Manifest and all AssemblyInfo.cs files have been updated to version {0}.", version);
                        }
                    }
                }
            }
            else
            {
                Arguments.ShowUsage();
            }

            return success ? 0 : 1;
        }

        private static bool UpdateAssemblyInfoVersions(string path, Version version)
        {
            try
            {
                IEnumerable<string> files = Directory.EnumerateFiles(path, "AssemblyInfo.cs", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string filedata = null;
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            filedata = sr.ReadToEnd();
                        }
                    }

                    string versionString = version.ToString();


                    string filedata2 = Regex.Replace(filedata, @"\[assembly\: AssemblyVersion\(""\d{1,}.\d{1,}.\d{1,}.\d{1,}""\)\]", "[assembly: AssemblyVersion(\"" + versionString + "\")]");
                    string updated = Regex.Replace(filedata2, @"\[assembly\: AssemblyFileVersion\(""\d{1,}.\d{1,}.\d{1,}.\d{1,}""\)\]", "[assembly: AssemblyFileVersion(\"" + versionString + "\")]");


//                    string updated = filedata
//                                .Replace("[assembly: AssemblyVersion(\"1.0.0.0\")]", "[assembly: AssemblyVersion(\"" + versionString + "\")]")
//                                .Replace("[assembly: AssemblyFileVersion(\"1.0.0.0\")]", "[assembly: AssemblyFileVersion(\"" + versionString + "\")]");

                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(updated);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while updating assemblyinfo.cs", ex.Message);
            }

            return false;
        }


    }
}
