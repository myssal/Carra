using System.IO.Compression;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Extensions.Data;
using Microsoft.Extensions.Logging;
using Carra.Carra.Logging;
namespace Carra.Carra;

public class Carra
{
	public string originalBundleFolder { get; set; }
	public string modBundleFolder { get; set; }
	public string modName { get; set; } 
	public string modAuthor { get; set; } 
	public string modDescription { get; set; } 
	public DirectoryInfo tmpFolder { get; set; }
	public ILogger logFactory { get; set; }	
	public Carra(string originalBundleFolder, string modBundleFolder, string? modName, string? modAuthor, string? modDescription)
	{
		this.originalBundleFolder = originalBundleFolder;
		this.modBundleFolder = modBundleFolder;
		this.modName = modName ?? "";
		this.modAuthor = modAuthor ?? "";
		this.modDescription = modDescription ?? "";
		tmpFolder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),
			$"carra_{DateTime.Now.ToString("yyyy-MM-dd-H-m-ss")}"));
		logFactory = Logging.Logging.CreateLogFactory("Carra");
	}

	public Carra(string zipFile)
	{
		(originalBundleFolder, modBundleFolder) = ScanLunarModRoot(zipFile);
		tmpFolder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),
			$"carra_{DateTime.Now.ToString("yyyy-MM-dd-H-m-ss")}"));
		logFactory = Logging.Logging.CreateLogFactory("Carra");
	}
	public string CreateModName()
	{
		if (string.IsNullOrEmpty(modName)) return new DirectoryInfo(modBundleFolder).Name; 
		return modName;
	}

	public void Metadata(string output)
	{
		using (StreamWriter sw = new StreamWriter(Path.Combine(tmpFolder.FullName, "metadata")))
		{
			sw.WriteLine("# Metadata");
			sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			sw.WriteLine($"name: {modName}");
			sw.WriteLine($"author: {modAuthor}");
			sw.WriteLine($"description: {modDescription}");
		}
		LZMA_XZ.XZCompress(Path.Combine(tmpFolder.FullName, "metadata"), output);
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

    public Dictionary<string, ulong> Dump(string bundleName, string rootFolder)
    {
	    var manager = new AssetsManager();
	    var bunInst = manager.LoadBundleFile(bundleName, true);
	    var afileInst = manager.LoadAssetsFileFromBundle(bunInst, 0, false);
	    var afile = afileInst.file;

	    var dump = new Dictionary<string, ulong>();
	    foreach (var texInfo in afile.Metadata.AssetInfos)
	    {
		    string key = Path.Combine(bundleName.Replace(@"\__data", string.Empty).Replace($"{rootFolder}\\", String.Empty), texInfo.PathId.ToString());
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
				logFactory.LogInformation("No bundles found in root directory!");
			else
			{
				foreach (string vanillaPath in originalBundles)
				{
					// vanilla assets processing
					logFactory.LogInformation($"Processing {vanillaPath}...");
					logFactory.LogInformation($"Mapping assets...");
					vanillaDicts = Dump(vanillaPath, originalBundleFolder);
					string expectedModBundlePath = vanillaPath.Replace(originalBundleFolder, modBundleFolder);
					if (!File.Exists(expectedModBundlePath))
						logFactory.LogInformation($"File {expectedModBundlePath} does not exist, skipping.");
					else
					{
						// mod assets processing
						var bunInst = manager.LoadBundleFile(expectedModBundlePath, true);
						var afileInst = manager.LoadAssetsFileFromBundle(bunInst, 0, false);
						var afile = afileInst.file;
						foreach (var texInfo in afile.Metadata.AssetInfos)
						{
							string key = Path.Combine(expectedModBundlePath.Replace(@"\__data", string.Empty).Replace($"{modBundleFolder}\\", String.Empty), texInfo.PathId.ToString());
							byte[] body = GetRawData(texInfo, afile);
							var state = XXHash.CreateState64();	
							XXHash.UpdateState64(state, body);
							var digest = XXHash.DigestState64(state);
							if (vanillaDicts.ContainsKey(key) && vanillaDicts[key] == digest) continue;
							if (!vanillaDicts.ContainsKey(key))
								logFactory.LogInformation($"New object found: {key}");

							key += $".{texInfo.TypeIdOrIndex.ToString()}";
							logFactory.LogInformation($"Writing {key}...");
							
							File.WriteAllBytes(Path.Combine(tmpFolder.FullName, "test.bytes"), body);
							var outputDir = Directory.CreateDirectory(Path.Combine(output, key.Remove(key.LastIndexOf("\\"))));
							LZMA_XZ.XZCompress(Path.Combine(tmpFolder.FullName, "test.bytes"), Path.Combine(output, key));
						}
						Metadata(Path.Combine(output, "metadata"));
					}
				}
				
				if (File.Exists("test.bytes")) File.Delete("test.bytes");
				if (File.Exists($"{output}.carra3")) logFactory.LogInformation($"{output}.carra3 already exists!");
				else
				{
					ZipFile.CreateFromDirectory(output, $"{output}.carra3");
					Directory.Delete(output, true);
				}
				
			}
		}
}