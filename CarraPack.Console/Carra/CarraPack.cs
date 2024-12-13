using System.IO.Compression;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Extensions.Data;

namespace Carra.Carra;

public class Carra
{
	public string originalBundleFolder { get; set; }
	public string modBundleFolder { get; set; }
	public string? modName {get; set;}
	public string? modAuthor {get; set;}
	public string? modDescription {get; set;}
	public DirectoryInfo tmpFolder { get; set; }

	public Carra(string originalBundleFolder, string modBundleFolder, string modName, string? modAuthor, string? modDescription)
	{
		this.originalBundleFolder = originalBundleFolder;
		this.modBundleFolder = modBundleFolder;
		this.modName = modName;
		this.modAuthor = modAuthor;
		this.modDescription = modDescription;
		tmpFolder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),
			$"carra_{DateTime.Now.ToString("yyyy-MM-dd-H-m-ss")}"));
	}

	public Carra(string zipFile)
	{
		(originalBundleFolder, modBundleFolder) = ScanLunarModRoot(zipFile);
		tmpFolder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),
			$"carra_{DateTime.Now.ToString("yyyy-MM-dd-H-m-ss")}"));
	}
	public string CreateModName()
	{
		if (string.IsNullOrEmpty(modName)) return new DirectoryInfo(modBundleFolder).Name; 
		return modName;
	}
    public static (string, string) ScanLunarModRoot(string zipPath)
    {
	    // => (original,mod)
	    var zipTemp = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),
		    $"carraLunar_{DateTime.Now.ToString("yyyy-MM-dd-H-m-ss")}"));
	    ZipFile.ExtractToDirectory(zipPath, zipTemp.FullName);
		return (Directory.GetDirectories(zipTemp.FullName, "Uninstallation", SearchOption.AllDirectories).Where(x => x.EndsWith("Uninstallation")).FirstOrDefault(),
				Directory.GetDirectories(zipTemp.FullName, "Installation", SearchOption.AllDirectories).Where(x => x.EndsWith("Installation")).FirstOrDefault());   
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

    public static Dictionary<string, ulong> Dump(string bundleName, string rootFolder)
    {
	    var manager = new AssetsManager();
	    var bunInst = manager.LoadBundleFile(bundleName, true);
	    var afileInst = manager.LoadAssetsFileFromBundle(bunInst, 0, false);
	    var afile = afileInst.file;
		
	    var dump = new Dictionary<string, ulong>();

	    foreach (var texInfo in afile.Metadata.AssetInfos)
	    {
		    string key = Path.Combine(bundleName.Replace(@"\__data", string.Empty).Replace($"{rootFolder}\\", String.Empty), texInfo.PathId.ToString());
		    Console.WriteLine($"- {key}");
		    var body = GetRawData(texInfo, afile);
		    var state = XXHash.CreateState64();
		    XXHash.UpdateState64(state, body);
		    var digest = XXHash.DigestState64(state);
		    dump[key] = digest;
	    }
	    return dump;
    }
    public void CompressLunarMod(string output)
		{
			AssetsManager manager = new AssetsManager();
			List<string> originalBundles =
				Directory.GetFiles(originalBundleFolder, "*__data", SearchOption.AllDirectories).ToList();
			List<string> modBundles = Directory.GetFiles(modBundleFolder, "*__data", SearchOption.AllDirectories).ToList();
			Dictionary<string, ulong> vanillaDicts = new Dictionary<string, ulong>();
			if (originalBundles.Count == 0)
				Console.WriteLine("No asset files found.");
			else
			{
				foreach (string vanillaPath in originalBundles)
				{
					// vanilla assets processing
					Console.WriteLine($"Processing {vanillaPath}...");
					Console.WriteLine($"Mapping assets...");
					vanillaDicts = Dump(vanillaPath, originalBundleFolder);
					
					string expectedModBundlePath = vanillaPath.Replace(originalBundleFolder, modBundleFolder);
					if (!File.Exists(expectedModBundlePath))
						Console.WriteLine($"File {expectedModBundlePath} does not exist, skipping.");
					else
					{
						// mod assets processing
						var bunInst = manager.LoadBundleFile(expectedModBundlePath, true);
						var afileInst = manager.LoadAssetsFileFromBundle(bunInst, 0, false);
						var afile = afileInst.file;
						foreach (var texInfo in afile.Metadata.AssetInfos)
						{
							string key = Path.Combine(expectedModBundlePath.Replace(@"\__data", string.Empty).Replace(modBundleFolder, String.Empty), texInfo.PathId.ToString());
							byte[] body = GetRawData(texInfo, afile);
							var state = XXHash.CreateState64();
							XXHash.UpdateState64(state, body);
							var digest = XXHash.DigestState64(state);
							if (vanillaDicts.ContainsKey(key) && vanillaDicts[key] == digest) continue;
							if (!vanillaDicts.ContainsKey(key))
								Console.WriteLine($"New object found: {key}");
							
							key += $".{texInfo.ScriptTypeIndex.ToString()}";
							Console.WriteLine($"Writing {key}...");
							
							// File.WriteAllBytes("test.bytes", body);
							// LZMA_XZ.XZCompress("test.bytes", Path.Combine(output, key));
						}
					}
				}
				// if (File.Exists("test.bytes")) File.Delete("test.bytes");
				// if (File.Exists($"{output}.carra3")) Console.WriteLine($"{output}.carra3 already exists!");
				// else
				// {
				// 	ZipFile.CreateFromDirectory(output, $"{output}.carra2");
				// 	Directory.Delete(output, true);
				// }
				
			}
		}
}