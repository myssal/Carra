using Joveler.Compression.XZ;
namespace Carra;

public partial class LZMA_XZ
{
    public static void XZCompress(string input, string output)
    { 
        // Compress in single-threaded mode
        XZCompressOptions compOpts = new XZCompressOptions
        {
            Level = LzmaCompLevel.Level9,
        };

        using (FileStream fsOrigin = new FileStream(input, FileMode.Open))
        using (FileStream fsComp = new FileStream(output, FileMode.Create))
        using (XZStream zs = new XZStream(fsComp, compOpts))
        {
            fsOrigin.CopyTo(zs);
        }
    }
}