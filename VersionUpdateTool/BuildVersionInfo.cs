using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace VersionUpdateTool
{
    public class BuildVersionInfo
    {
        private static readonly IFormatter DataFormatter = new BinaryFormatter();

        private string storageLocation;

        public BuildVersionInfo(string key)
        {
            this.storageLocation = key + ".buildinfo";

            this.Load();
        }

        public int Build { get; set; }

        public int Revision { get; set; }

        public int RevisionDay
        {
            get
            {
                return this.Revision / 1000;
            }
        }

        public void Update()
        {
            Version v = new Version(1, 0, this.Build, this.Revision);

            try
            {
                using (var file = IsolatedStorageFile.GetUserStoreForAssembly())
                {
                    using (var stream = file.OpenFile(this.storageLocation, FileMode.Create, FileAccess.Write))
                    {
                        DataFormatter.Serialize(stream, v);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error occured saving: {0}", ex.Message);
            }
        }

        private void Load()
        {
            try
            {
                using (var file = IsolatedStorageFile.GetUserStoreForAssembly())
                {
                    using (var stream = file.OpenFile(this.storageLocation, FileMode.Open, FileAccess.Read))
                    {
                        Version version = (Version)DataFormatter.Deserialize(stream);

                        this.Build = version.Build;
                        this.Revision = version.Revision;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error occured loading: {0}", ex.Message);
            }
        }
    }
}
