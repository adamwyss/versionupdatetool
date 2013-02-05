using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VersionUpdateTool
{
    public static class ManifestParser
    {
        private const string VersionNotUpdated = "";

        private const string AttributeNotFound = "The '{0}' element does not contain a '{1}' attribute.";

        private const string ElementNotFound = "The element '{0}' was not found.";

        private const string ManifestError = "The manifest is invalid or was not found at '{0}'.";

        private static readonly XNamespace AppXManifestNamespace = "http://schemas.microsoft.com/appx/2010/manifest";
        
        private static readonly XNamespace WindowsPhone7ManifestNamespace = "http://schemas.microsoft.com/windowsphone/2009/deployment";

        private static readonly XNamespace WindowsPhone8ManifestNamespace = "http://schemas.microsoft.com/windowsphone/2012/deployment";

        public static bool UpdateWindowsManifest(string path, out Version version)
        {
            XDocument doc = XDocument.Load(path);
            if (doc != null)
            {
                XElement packageElement = doc.Element(AppXManifestNamespace + "Package");
                if (packageElement != null)
                {
                    XElement identityElement = packageElement.Element(AppXManifestNamespace + "Identity");
                    if (identityElement != null)
                    {
                        XAttribute versionAttribute = identityElement.Attribute("Version");
                        if (versionAttribute != null)
                        {
                            bool success = UpdateVersionAttributeWithLatestVersion(versionAttribute, out version);
                            if (success)
                            {
                                doc.Save(path);
                                return true;
                            }
                            else
                            {
                                Console.WriteLine(VersionNotUpdated);
                            }
                        }
                        else
                        {
                            Console.WriteLine(AttributeNotFound, "Identity", "Version");
                        }
                    }
                    else
                    {
                        Console.WriteLine(ElementNotFound, "Identity");
                    }
                }
                else
                {
                    Console.WriteLine(ElementNotFound, "Package");
                }
            }
            else
            {
                Console.WriteLine(ManifestError, path);
            }

            version = null;
            return false;
        }

        public static bool UpdateWindowsPhoneManifest(string path, out Version version)
        {
            XDocument doc = XDocument.Load(path);
            if (doc != null)
            {
                XElement deploymentElement = doc.Element(WindowsPhone7ManifestNamespace + "Deployment");
                if (deploymentElement == null)
                {
                    // windows phone 8 uses a different namespace, so we will check for that one if the wp7 fails.
                    deploymentElement = doc.Element(WindowsPhone8ManifestNamespace + "Deployment");
                }

                if (deploymentElement != null)
                {
                    XElement appElement = deploymentElement.Element("App");
                    if (appElement != null)
                    {
                        XAttribute versionAttribute = appElement.Attribute("Version");
                        if (versionAttribute != null)
                        {
                            bool success = UpdateVersionAttributeWithLatestVersion(versionAttribute, out version);
                            if (success)
                            {
                                doc.Save(path);
                                return true;
                            }
                            else
                            {
                                Console.WriteLine(VersionNotUpdated);
                            }
                        }
                        else
                        {
                            Console.WriteLine(AttributeNotFound, "App", "Version");
                        }
                    }
                    else
                    {
                        Console.WriteLine(ElementNotFound, "App");
                    }
                }
                else
                {
                    Console.WriteLine(ElementNotFound, "Deployment");
                }
            }
            else
            {
                Console.WriteLine(ManifestError, path);
            }

            version = null;
            return false;
        }

        private static bool UpdateVersionAttributeWithLatestVersion(XAttribute versionAttribute, out Version version)
        {
            string versionString = versionAttribute.Value;

            Version manifestVersion;
            bool success = Version.TryParse(versionString, out manifestVersion);
            if (success)
            {
                string yearMonthString = DateTime.Today.ToString("yyMM");
                int build = int.Parse(yearMonthString);

                string dayString = DateTime.Today.ToString("dd");
                int revision = int.Parse(dayString);

                BuildVersionInfo buildInfo = new BuildVersionInfo(Arguments.ApplicationKey);

                if (buildInfo.Build == build && buildInfo.RevisionDay == revision)
                {
                    revision = buildInfo.Revision + 1;
                }
                else
                {
                    revision *= 1000;
                }

                buildInfo.Build = build;
                buildInfo.Revision = revision;
                buildInfo.Update();

                version = new Version(manifestVersion.Major, manifestVersion.Minor, build, revision);
                versionAttribute.Value = version.ToString();
                return true;
            }
            else
            {
                Console.WriteLine("{0} is not a valid version string.", versionString);
            }

            version = null;
            return false;
        }

    }
}
