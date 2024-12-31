using Carra;
using static Carra.LZMA_XZ;

namespace Carra
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // timing instance
            InitNativeLibrary();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // main test code
            string testUnit = @"";
            Carra cr = new Carra(testUnit);
            cr.CompressLunarMod(testUnit);
            // end main test code
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs);
        }
        
    }
}   