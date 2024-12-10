using static Carra.Carra.Init;

namespace Carra
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            InitNativeLibrary();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // string test = @"";
            // LunarCompress.CompressLunarMod(test, test.Replace(".zip", ""));
            // Unpacker unpacker = new Unpacker();
            //unpacker.ExtractAssets();
            //unpacker.PatchAssets();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs);

        }
        
    }
}