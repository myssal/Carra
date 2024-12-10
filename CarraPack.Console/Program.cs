using System.Text;
using Carra.Carra;
using SevenZip.Compression.LZMA;

namespace Carra
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string test = @"C:\Users\sangd\AppData\Roaming\LimbusCompanyMods\Yearning-Mircalla Sancho mod.zip";
            LunarCompress.CompressLunarMod(test, test.Replace(".zip", ""));
            Unpacker unpacker = new Unpacker();
            //unpacker.ExtractAssets();
            //unpacker.PatchAssets();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs);

        }
    }
}