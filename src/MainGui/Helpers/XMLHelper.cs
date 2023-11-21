using System.IO;
using System.Text.RegularExpressions;

namespace MainGui.Helpers;

public class XMLHelper
{
    /// <summary>
    ///     讀取 XML 文件，返回圖形所在 Match
    /// </summary>
    /// <param name="vPath"></param>
    public Match ReadXml(string vPath)
    {
        var streamReader = new StreamReader(vPath);
        var regex = new Regex(@"<Figures>\s*([^F]*)</Figures>");
        var match = regex.Match(streamReader.ReadToEnd());
        streamReader.Close();
        return match;
    }

    /// <summary>
    ///     把圖形寫入 XML 文件中寫入 XML
    /// </summary>
    /// <param name="vPath"></param>
    /// <param name="vGeometryString"></param>
    public void WriteXml(string vPath, string vGeometryString)
    {
        var streamWriter = new StreamWriter(vPath);
        streamWriter.Write(vGeometryString);
        streamWriter.Close();
    }
}