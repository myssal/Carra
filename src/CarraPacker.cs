using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO.Compression;
namespace Carra
{
    public class CarraPacker
    {
        public static List<string> ScanLunarModRoot(string zipPath)
        {
			
            List<string> vanillaData = new List<string>();
            using (ZipArchive zip = ZipFile.OpenRead(zipPath))
            {
				
                // Console.OutputEncoding = System.Text.Encoding.UTF8;
                foreach (var entry in zip.Entries)
                {
				
                    string root = entry.FullName;
                    if (root.Contains("Uninstallation") && root.EndsWith("__data")) vanillaData.Add(root);
                }
            }
            // add logging
            return vanillaData;
        }
    }
}