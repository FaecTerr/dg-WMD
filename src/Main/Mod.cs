using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
//using System.Xml.Linq;

// The title of your mod, as displayed in menus
[assembly: AssemblyTitle("W. M. D.")]

// The author of the mod
[assembly: AssemblyCompany("FaecTerr")]

// The description of the mod
[assembly: AssemblyDescription("Worms and Ducks in one mod. What can go wrong?")]

// The mod's version
[assembly: AssemblyVersion("1.0.0.0")]

namespace DuckGame.WMD
{               
    //workshopIDFacade = 2094212553;
    public class WMD : Mod
    {
        internal static WMDMain mm;
        // The mod's priority; this property controls the load order of the mod.
        public override Priority priority
        {
            get { return base.priority; }
        }

        //public override ulong workshopIDFacade => base.workshopIDFacade;

        // This function is run before all mods are finished loading.
        protected override void OnPreInitialize()
        {
            base.OnPreInitialize();
        }

        // This function is run after all mods are loaded.
        protected override void OnPostInitialize()
        {
            NetMessageAdder.UpdateNetmessageTypes();
            base.OnPostInitialize();
            copyLevels();
        }
        private byte[] GetMD5Hash(byte[] sourceBytes)
        {
            return new MD5CryptoServiceProvider().ComputeHash(sourceBytes);
        }
        private static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }
            int iterations = (int)Math.Ceiling(first.Length / 8.0);
            using (FileStream fs = first.OpenRead())
            {
                using (FileStream fs2 = second.OpenRead())
                {
                    byte[] one = new byte[8];
                    byte[] two = new byte[8];
                    for (int i = 0; i < iterations; i++)
                    {
                        fs.Read(one, 0, 8);
                        fs2.Read(two, 0, 8);
                        if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private static void copyLevels()
        {
            string levelFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "DuckGame\\Levels\\WMD (Where My Ducks)");
            if (!Directory.Exists(levelFolder))
            {
                Directory.CreateDirectory(levelFolder);
            }
            foreach (string sourcePath in Directory.GetFiles(GetPath<WMD>("Levels")))
            {
                string destPath = Path.Combine(levelFolder, Path.GetFileName(sourcePath));
                bool file_exists = File.Exists(destPath);
                if (!file_exists || !FilesAreEqual(new FileInfo(sourcePath), new FileInfo(destPath)))
                {
                    if (file_exists)
                    {
                        File.Delete(destPath);
                    }
                    File.Copy(sourcePath, destPath);
                    
                }
            }
        }
    }
}


