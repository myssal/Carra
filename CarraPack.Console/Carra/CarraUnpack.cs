using System.IO.Compression;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using SharpCompress.Compressors.Xz;
namespace Carra;

public class CarraUnpack
{
    private string modFolder { get; set; }
    public string limbusCompanyFolder { get; set; }
    public DirectoryInfo tmpAssetFolder {get; set;}

    public CarraUnpack()
    {
        tmpAssetFolder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),
            $"carra_{DateTime.Now.ToString("yyyy-MM-dd-H-m-ss")}"));
        modFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            , "LimbusCompanyMods");
        limbusCompanyFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "LocalLow", "Unity", "ProjectMoon_LimbusCompany");
    }
    
    public void Patch()
	{
		try
		{
			CleanUpAtLaunch();
			Console.WriteLine($"Patching assets...");
			bool hasCarra = false;

			foreach (var bundlePath in Directory.GetFiles(modFolder, "*.carra*", SearchOption.AllDirectories))
			{
				hasCarra = true;
				Console.WriteLine($"Processing {bundlePath}...");
				string tmpOutput = Path.Combine(tmpAssetFolder.FullName, new FileInfo(bundlePath).Name);
				using (ZipArchive archive = ZipFile.Open(bundlePath, ZipArchiveMode.Read))
				{
					archive.ExtractToDirectory(tmpOutput);
				}
				AssetsPatch(tmpOutput);
			}
			if (!hasCarra) Console.WriteLine("No .carra file found.");
		}catch (Exception ex)
		{
			Console.WriteLine($"Exception: {ex}");
		}
	}

	public void AssetsPatch(string carraTmp)
	{
		Console.WriteLine("hello");
		foreach (var bnd in Directory.GetDirectories(carraTmp, "*", SearchOption.TopDirectoryOnly))
		{	
			DirectoryInfo bundleInfo = Directory.CreateDirectory(bnd);
			string expectedRoot = Path.Combine(limbusCompanyFolder, bundleInfo.Name);
			Console.WriteLine(expectedRoot);
			if (!Directory.Exists(expectedRoot))
				Console.WriteLine($"Can't find {expectedRoot}, skip patching assets...");
			else
			{
				try
				{
					string expectedPath = Directory.GetFiles(expectedRoot, "*__data*", SearchOption.AllDirectories)
						.FirstOrDefault();
					if (String.IsNullOrEmpty(expectedPath)) continue;
					Console.WriteLine($"Backing up {expectedPath}...");
					File.Copy(expectedPath, expectedPath.Replace("__data", "__original"), true);
					Console.WriteLine($"Patching {expectedPath}...");

					var rnd = new System.Random();
					string randomName = $"tmp_{rnd.Next(1000, 10000)}.bytes";
					File.Copy(expectedPath, Path.Combine(tmpAssetFolder.FullName, randomName), true);
					Console.WriteLine($"Initiating asset tools...");
					var manager = new AssetsManager();
					var bundleInst = manager.LoadBundleFile(Path.Combine(tmpAssetFolder.FullName, randomName));
					var assetInst = manager.LoadAssetsFileFromBundle(bundleInst, 0, true);

					var asset = assetInst.file;
					var bundle = bundleInst.file;

					// decompress and patch
					foreach (string rawData in Directory.GetFiles(bundleInfo.FullName, "*", SearchOption.AllDirectories))
					{
						using (var xz = new XZStream(File.OpenRead(rawData)))
						using (Stream toFile = new FileStream(rawData + ".raw_asset", FileMode.Create))
						{
							xz.CopyTo(toFile);
						}

						var bjgbgb = Path.GetFileName(rawData).Split('.');
						long.TryParse(bjgbgb.First(), out long pathID);
						var success = int.TryParse(bjgbgb.Last(), out int treeID);
						AssetFileInfo __new = new();
						if (success)
						{
							var treeInfo = asset.Metadata.TypeTreeTypes[treeID];
							var scriptidx = treeInfo.ScriptTypeIndex;
							var typeID = treeInfo.TypeId;
							Console.WriteLine($"finding treeidx {treeID} scriptidx {scriptidx} for {pathID}");
							__new = AssetFileInfo.Create(asset, pathID, typeID, scriptidx);
							__new.SetNewData(File.ReadAllBytes($"{rawData}.raw_asset"));
						}

						var overwrite_exist = asset.GetAssetInfo(pathID);
						if (overwrite_exist != null)
						{
							Console.WriteLine($"Overwrting {pathID}");
							overwrite_exist.SetNewData(File.ReadAllBytes($"{rawData}.raw_asset"));
						}
						else asset.Metadata.AddAssetInfo(__new);
					}

					//finally pack uncompressed bundle
					Console.WriteLine("Writing modded assets to file...");
					bundle.BlockAndDirInfo.DirectoryInfos[0].SetNewData(asset);
					using (AssetsFileWriter writer = new AssetsFileWriter(expectedPath))
						bundle.Write(writer);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex}");
				}
			}
		}
	}
	public void CleanUpAtLaunch()
	{
		try
		{
			// remove carra_ temp folder from previous launch
			foreach (var tmpCarra in Directory.GetDirectories(Path.GetTempPath(), "carra_*", SearchOption.TopDirectoryOnly))
				Directory.Delete(tmpCarra, true);
		} catch {}
	}

	public void CleanUpAtClose()
	{
		string bundleRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"..",
				"LocalLow",
				"Unity",
				"ProjectMoon_LimbusCompany"
			);
		Console.WriteLine($"Cleaning up assets");
		foreach (var og in Directory.GetFiles(bundleRoot, "*__original", SearchOption.AllDirectories))
		{
			Console.WriteLine($"Restoring {og} -> {og.Replace("__original", "__data")}");
			File.Copy(og, og.Replace("__original", "__data"), true);
			File.Delete(og);
		}	
	}

}