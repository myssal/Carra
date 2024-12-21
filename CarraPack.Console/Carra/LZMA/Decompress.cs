using Joveler.Compression.XZ;

namespace Carra.Carra;

public class Decompress
{
    public static void XZDecompress(string input, string output)
    {
        XZDecompressOptions decompOpts = new XZDecompressOptions();

        using (FileStream fsComp = new FileStream(input, FileMode.Create))
        using (FileStream fsDecomp = new FileStream(output, FileMode.Create))
        using (XZStream zs = new XZStream(fsComp, decompOpts))
        {
            zs.CopyTo(fsDecomp);
        }
    }
}