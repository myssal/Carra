using System.IO.Compression;

namespace Carra.Carra;

public class Unpacker
{
    private string modFolder { get; set; }
    public string limbusCompanyFolder { get; set; }
    public string tmpAssetFolder {get; set;}

    public Unpacker()
    {
        tmpAssetFolder = CreateTmpFolder().FullName;
        modFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            , "LimbusCompanyMods");
        limbusCompanyFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "LocalLow", "Unity", "ProjectMoon_LimbusCompany");
    }
    public DirectoryInfo CreateTmpFolder() => Directory.CreateTempSubdirectory("carra_");

    public void ExtractAssets()
    {
        List<string> carraFiles = Directory.GetFiles(modFolder, "*.carra*", SearchOption.AllDirectories).ToList();
        // extract all .carraX to tmp folder
        foreach (var carraFile in carraFiles)
        {
            Console.WriteLine(carraFile);
            string carraName = Path.GetFileName(carraFile).Substring(0, Path.GetFileName(carraFile).IndexOf(".carra"));
            Console.WriteLine($"Extracting {carraName}...");
            ZipFile.ExtractToDirectory(carraFile, tmpAssetFolder);
        }
        // remove second subfolder level
        List<string> assetFiles = Directory.GetDirectories(tmpAssetFolder, "*", SearchOption.TopDirectoryOnly).ToList();
        foreach (var assetFile in assetFiles)
        {
            foreach (var asset in Directory.GetFiles(assetFile, "*", SearchOption.AllDirectories))
            {
                FileInfo mFile = new FileInfo(asset);
                Console.WriteLine($"Intital file: {mFile.FullName}");
                if (new FileInfo(Path.Combine(assetFile, mFile.Name)).Exists == false) 
                {
                    Console.WriteLine($"Move to: {Path.Combine(assetFile, mFile.Name)}");
                    mFile.MoveTo(Path.Combine(assetFile, mFile.Name));
                }

            }
        }
    }

    public void PatchAssets()
    {
        foreach (var bundle in Directory.GetDirectories(limbusCompanyFolder, "*", SearchOption.TopDirectoryOnly))
        {
            // check if that bundle is modified
            string expectedPath = Path.Combine(tmpAssetFolder, bundle.Substring(bundle.LastIndexOf("\\") + 1));
            if (!Directory.Exists(expectedPath)) continue;
            
            string __data = Directory.GetFiles(expectedPath, "*__data*", SearchOption.AllDirectories).FirstOrDefault();
            string __original = __data.Replace("__data", "__original");
            Console.WriteLine($"Backing up {__data}...");
            File.Copy(__data, __original, true);
            
            // patching assets
            Console.WriteLine($"Patching {__data}...");
            PatchBundle();
            Console.WriteLine($"Patching complete {__original} ({new FileInfo(__original).Length}) ->{__data} ({new FileInfo(__data).Length})");
        }
    }

    public void PatchBundle()
    {
        // replacing assets logic goes here;
    }
}