using SharpCompress.Compressors.Xz;
namespace Carra.Carra;

public class LZMA
{
    public static byte[] Compress(byte[] data)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var xzStream = new XZStream(memoryStream))
            {
                
                xzStream.Write(data, 0, data.Length);
            }

            return memoryStream.ToArray();
        }
    }

    public static void TestCompres(string input, string output)
    {
        using (var xz = new XZStream(File.OpenRead(input)))
        using (Stream toFile = new FileStream(output, FileMode.Create))
        {
            xz.CopyTo(toFile);
        }
    }
}