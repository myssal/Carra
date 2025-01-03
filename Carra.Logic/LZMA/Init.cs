using System.Runtime.InteropServices;
using Joveler.Compression.XZ;
using System.IO;
using System;
namespace Carra.LZMA
{
	public partial class LZMA_XZ
	{
	    public static void InitNativeLibrary()
	    {
	        string libDir = "runtimes";
	        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
	            libDir = Path.Combine(libDir, "win-");
	        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
	            libDir = Path.Combine(libDir, "linux-");
	        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
	            libDir = Path.Combine(libDir, "osx-");

	        switch (RuntimeInformation.ProcessArchitecture)
	        {
	            case Architecture.X86:
	                libDir += "x86";
	                break;
	            case Architecture.X64:
	                libDir += "x64";
	                break;
	            case Architecture.Arm:
	                libDir += "arm";
	                break;
	            case Architecture.Arm64:
	                libDir += "arm64";
	                break;
	        }
	        libDir = Path.Combine(libDir, "native");

	        string libPath = null;
	        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
	            libPath = Path.Combine(libDir, "liblzma.dll");
	        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
	            libPath = Path.Combine(libDir, "liblzma.so");
	        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
	            libPath = Path.Combine(libDir, "liblzma.dylib");

	        if (libPath == null)
	            throw new PlatformNotSupportedException($"Unable to find native library.");
	        if (!File.Exists(libPath))
	            throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");
        
	        XZInit.GlobalInit(libPath);
	    }

	}
}