using System.IO.Compression;
using System.IO.Hashing;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
namespace Carra.Carra;

public class LunarCompress
{
    /// <summary>
    /// Scan for original bundles in Lunartique-format .zip file.
    /// </summary>
    /// <param name="zipPath"></param>
    /// <returns>List of orginal bundles</returns>
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
    
    /// <summary>
    /// Equivalent of UnityPy get_raw_data()
    /// </summary>
    /// <param name="aFileInfo"></param>
    /// <param name="aFile"></param>
    /// <returns>Bytes array of certain asset</returns>
    public static byte[] GetRawData(string filePath, AssetFileInfo aFileInfo, AssetsFile aFile)
    {
        // aFile.Reader.BaseStream.Seek(aFileInfo.GetAbsoluteByteOffset(aFile), SeekOrigin.Begin);
        // return aFile.Reader.ReadBytes((int)aFileInfo.ByteSize);
        byte[] buffer = new byte[aFileInfo.ByteSize];
	    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
	      fileStream.Seek(aFileInfo.GetAbsoluteByteOffset(aFile), SeekOrigin.Begin);
	      fileStream.Read(buffer, 0, (int)aFileInfo.ByteSize);
        }
	    return buffer;
    }
    
    public static void CompressLunarMod(string zipPath, string output)
		{
			// TODO: fix get_raw_data()

			AssetsManager manager = new AssetsManager();
			List<string> vanillaPaths = new List<string>();
			foreach (string strr in ScanLunarModRoot(zipPath))
			{
				if (strr.Contains("06dfd7ab3324395b906aa15e043548fd")) vanillaPaths.Add(strr);
			}
			List<string> moddedPaths = new List<string>();
			string tmpBaseZip = Path.Combine(Path.GetTempPath(), "tmpZipFod");
			if (!Directory.Exists(tmpBaseZip))
			{
				Console.WriteLine("Create tmp...");
				Directory.CreateDirectory(tmpBaseZip);
			}
			if (vanillaPaths.Count == 0)
				Console.WriteLine("No asset files found.");
			else
			{
				Dictionary<string, byte[]> vanillaDicts = new Dictionary<string, byte[]>();
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
						Console.WriteLine($"File size: {new FileInfo(tempOutput).Length}");
						Console.WriteLine("PathID - Offset - Byte size");
						BundleFileInstance bundleInstance = manager.LoadBundleFile(tempOutput, true);	
						AssetsFileInstance fileInstance = manager.LoadAssetsFileFromBundle(bundleInstance, 0, false);
						AssetsFile data = fileInstance.file;

						foreach (var asset in data.AssetInfos)
						{
							Console.WriteLine($"{asset.PathId} - {asset.GetAbsoluteByteOffset(data)} - {asset.ByteSize} - {asset.TypeId}");
							string absPath = @"";
							// get key for dict
							string key = Path.Combine(vanillaPath.Substring(vanillaPath.IndexOf("Uninstallation") + 15).Replace(@"/__data", string.Empty), asset.PathId.ToString());
							// get hash value
							// not work?					
							byte[] rawData = GetRawData(tempOutput,asset, data);
							File.WriteAllBytes(Path.Combine(absPath, "1ce", "raw", asset.PathId.ToString().Replace("-", String.Empty)), rawData);
							byte[] hashVal = XxHash128.Hash(rawData);
							vanillaDicts.Add(key, hashVal);
						}						
					}
					
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
							Console.WriteLine("PathID - Offset - Byte size");
							BundleFileInstance bundleInstance = manager.LoadBundleFile(tempOutput, true);
							AssetsFileInstance fileInstance =
								manager.LoadAssetsFileFromBundle(bundleInstance, 0, false);
							AssetsFile data = fileInstance.file;

							foreach (var asset in data.AssetInfos)
							{
								Console.WriteLine($"{asset.PathId} - {asset.GetAbsoluteByteOffset(data)} - {asset.ByteSize} - {asset.TypeId}");
								string absPath =
									@"";
								// get key for dict
								string key =
									Path.Combine(
										vanillaPath.Substring(vanillaPath.IndexOf("Uninstallation") + 15)
											.Replace(@"/__data", string.Empty), asset.PathId.ToString());
								// get hash value
								// not work?					
								byte[] rawData = GetRawData(tempOutput, asset, data);
								File.WriteAllBytes(Path.Combine(absPath, "1ce", "mod", asset.PathId.ToString().Replace("-", String.Empty)), rawData);
								byte[] hashVal = XxHash128.Hash(rawData);
								// if (vanillaDicts[key].SequenceEqual(hashVal))
								// {
								// 	Console.WriteLine($"{asset.PathId.ToString()} - Equal");
								// 	continue;
								// }
								// else Console.WriteLine($"{asset.PathId.ToString()} - Not equal");

								if (!vanillaDicts.ContainsKey(key)) Console.WriteLine($"New object found: {key}");
								key += $".{asset.TypeId}";
							}
						}
					}
				}
			}
		}
}