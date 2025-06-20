using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace QnAutoSend.InjectQn;

public class ZipStaticDataSource : IStaticDataSource
{

    private string _content;
    public ZipStaticDataSource(string content)
    {
        _content = content;
    }

    public Stream GetSource()
    {
        byte[] bytes = Encoding.UTF8.GetBytes(_content);
        return new MemoryStream(bytes);
    }
}