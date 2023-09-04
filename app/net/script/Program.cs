using script;
using System.Text;

Console.WriteLine("Hello, IPTV!");
string basePath = @"..\..\..\..\..\..\";

var filter_file = (basePath + @"src\filter_file.txt").filter();
Api.filters = (basePath + @"src\filter.txt").ifilter();
Api.filterurls = (basePath + @"src\filter-url.txt").ifilter();
var dir = new Dictionary<string, List<string>>();
foreach (var item in new DirectoryInfo(basePath + @"src\wcb1969\iptv").GetFiles())
{
    var key = Path.GetFileNameWithoutExtension(item.Name);
    if (key.filter(filter_file) || item.Name == ".git" || key.Contains("(失效)") || item.Name.EndsWith(".xlsx") || item.Name.EndsWith(".m3u") || item.Name.EndsWith(".m3u8") || item.Name.EndsWith(".rar")) continue;
    else if (key.Contains("爱尚"))
    {
        item.FullName.xtxt(ref dir);
    }
    else
    {
        item.FullName.xtxt(ref dir);
        break;
    }
}
File.WriteAllText(basePath + @"tv\" + "default.json", dir.xtxt().ToJsonCore(), Encoding.UTF8);
Console.WriteLine("Hello, IPTV!");
