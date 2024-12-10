﻿using Carra.Carra;
using static Carra.Carra.LZMA_XZ;

namespace Carra
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            InitNativeLibrary();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string test = @"";
            CarraPacking.CompressLunarMod(test, test.Replace(".zip", ""));
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs);

        }
        
    }
}