using Carra.Carra;
using static Carra.Carra.LZMA_XZ;

namespace Carra
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            InitNativeLibrary();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CarraUnpack crp = new CarraUnpack();
            crp.CleanUpAtClose();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs);

        }
        
    }
}   