using System.IO.Compression;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Extensions.Data;

namespace Carra.Carra;

public class LunarCompress
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
        return vanillaData;
    }
    
    public static byte[] GetRawData(AssetFileInfo aFileInfo, AssetsFile aFile)
    {
	    // written by Zeno <3
	    var buffer = new byte[aFileInfo.ByteSize];
	    var oldOffset = aFile.Reader.Position;
	    aFile.Reader.Position = aFileInfo.GetAbsoluteByteOffset(aFile);
        
	    int read, offset = 0;
	    var leftToRead = buffer.Length;
	    while (leftToRead > 0 && (read = aFile.Reader.Read(buffer, offset, leftToRead)) > 0) {
		    leftToRead -= read;
		    offset += read;
	    }
        
	    aFile.Reader.Position = oldOffset;
	    return buffer;
    }
    static Dictionary<string, ulong> Dump(string bundleFile, string type, string relativePath)
    {
	    var manager = new AssetsManager();
	    var bunInst = manager.LoadBundleFile(bundleFile, true);
	    var afileInst = manager.LoadAssetsFileFromBundle(bunInst, 0, false);
	    var afile = afileInst.file;
		
	    var dump = new Dictionary<string, ulong>();

	    foreach (var texInfo in afile.Metadata.AssetInfos)
	    {
		    string key = type == "Installation" ? Path.Combine(relativePath.Substring(relativePath.IndexOf("Installation") + 13).Replace(@"/__data", string.Empty), texInfo.PathId.ToString())
				    : Path.Combine(relativePath.Substring(relativePath.IndexOf("Uninstallation") + 15).Replace(@"/__data", string.Empty), texInfo.PathId.ToString());
		    var body = GetRawData(texInfo, afile);
		    var state = XXHash.CreateState64();
		    XXHash.UpdateState64(state, body);
		    var digest = XXHash.DigestState64(state);
		    dump[key] = digest;
	    }

	    return dump;
    }

    public static void CompareDifference(Dictionary<string, ulong> vanilla, Dictionary<string, ulong> modded)
    {
	    
    }
    public static void CompressLunarMod(string zipPath, string output)
		{
			AssetsManager manager = new AssetsManager();
			List<string> vanillaPaths = ScanLunarModRoot(zipPath);
			List<string> moddedPaths = new List<string>();
			var moddedDicts = new Dictionary<string, ulong>();
			var vanillaDicts = new Dictionary<string, ulong>();
			string tmpBaseZip = Path.Combine(Path.GetTempPath(), "tmpZipFod");
			if (!Directory.Exists(tmpBaseZip))
			{
				Console.WriteLine("Creating tmp...");
				Directory.CreateDirectory(tmpBaseZip);
			}
			if (vanillaPaths.Count == 0)
				Console.WriteLine("No asset files found.");
			else
			{
				// vanilla assets processing
				foreach (string vanillaPath in vanillaPaths)
				{
					Console.WriteLine($"Processing {vanillaPath}...");
					moddedPaths.Add(vanillaPath.Replace("Uninstallation", "Installation"));
					using (ZipArchive zip = ZipFile.OpenRead(zipPath))
					{
						// set up file for reading
						ZipArchiveEntry entry = zip.GetEntry(vanillaPath);
						string tempOutput = Path.Combine(tmpBaseZip, vanillaPath.Replace("/", string.Empty));
						entry.ExtractToFile(tempOutput, true);
						vanillaDicts = Dump(tempOutput, "Uninstallation", vanillaPath);
					}
				}
				int count = 0;
				// replacing mod assets
				foreach (var moddedPath in moddedPaths)
				{
					Console.WriteLine($"Processing {moddedPath}...");
					using (ZipArchive zip = ZipFile.OpenRead(zipPath))
					{
						// set up file for reading
						ZipArchiveEntry entry = zip.GetEntry(moddedPath);
						string tempOutput = Path.Combine(tmpBaseZip, $"md{moddedPath.Replace("/", string.Empty)}");
						entry.ExtractToFile(tempOutput, true);
						Console.WriteLine($"File size: {new FileInfo(tempOutput).Length}");
						var bunInst = manager.LoadBundleFile(tempOutput, true);
						var afileInst = manager.LoadAssetsFileFromBundle(bunInst, 0, false);
						var afile = afileInst.file;
						foreach (var texInfo in afile.Metadata.AssetInfos)
						{
							string key =
								Path.Combine(
									moddedPath.Substring(moddedPath.IndexOf("Installation") + 13)
										.Replace(@"/__data", string.Empty), texInfo.PathId.ToString());
							byte[] body = GetRawData(texInfo, afile);
							var state = XXHash.CreateState64();
							XXHash.UpdateState64(state, body);
							var digest = XXHash.DigestState64(state);
							if (vanillaDicts.ContainsKey(key) && vanillaDicts[key] == digest)
							{
								Console.WriteLine($"Duplicate key: {key}");
								continue;
							}
							if (!vanillaDicts.ContainsKey(key))
							{
								Console.WriteLine($"New object found: {key}");
								count++;
							}
							key += $".{texInfo.TypeId.ToString()}";
							Console.WriteLine($"Writing {key}...");
							string outputSubFolder = Path.Combine(output, key.Substring(0, key.IndexOf("\\")));
							if (!Directory.Exists(outputSubFolder))
								Directory.CreateDirectory(outputSubFolder);
							File.WriteAllBytes("test.bytes", body);
							Compress.XZCompress("test.bytes", Path.Combine(output, key));
						}
					}
				}
				Console.WriteLine($"New obj count: {count}");
				if (File.Exists("test.bytes")) File.Delete("test.bytes");
				if (File.Exists($"{output}.carra2")) Console.WriteLine($"{output}.carra2 already exists!");
				else
				{
					ZipFile.CreateFromDirectory(output, $"{output}.carra2");
					Directory.Delete(output, true);
				}
				
			}
		}
}