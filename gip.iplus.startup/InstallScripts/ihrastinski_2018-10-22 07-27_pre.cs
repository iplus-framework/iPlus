/// <Precompiler>
/// refassembly System.Xml;
using System.Linq;
using System.Xml;
using System.IO;
/// </Precompiler>

public bool ExtractConnectionStrings(string serverInstallPath, string localInstallPath)
{
    if(!Directory.Exists(localInstallPath) || !Directory.EnumerateFileSystemEntries(localInstallPath).Any())
        return true;
    
    string connStringsFilePath = Path.Combine(localInstallPath, "ConnectionStrings.config");
    if (File.Exists(connStringsFilePath))
    {
        //Console.WriteLine("ihrastinski_2018-10-22 07-27_pre: The file ConnectionStrings.config already exists in the local install path. Press any key to continue...");
        //Console.Read();
        return true;
    }

    string appConfigPath = Path.Combine(localInstallPath, "gip.mes.client.exe.config");
    if (!File.Exists(appConfigPath))
        appConfigPath = Path.Combine(localInstallPath, "gip.iplus.client.exe.config");

    if(!File.Exists(appConfigPath))
    {
        Console.WriteLine(string.Format("ihrastinski_2018-10-22 07-27_pre: Can't find the app.config file in {0}. Do you want continue?[y/n]", appConfigPath));
        string result = Console.ReadLine();
        if(result.ToLower() == "y")
            return true;
        else 
            return false;
    }

    XmlDocument doc = new XmlDocument();
    try
    {
        doc.Load(appConfigPath);
    }
    catch (Exception e)
    {
        Console.WriteLine(string.Format("ihrastinski_2018-10-22 07-27_pre: XML load exception: {0}. Press any key to exit...", e.Message));
        Console.Read();
        return false;
    }

    var connectionStrings = doc.GetElementsByTagName("connectionStrings");
    if (connectionStrings == null)
        connectionStrings = doc.GetElementsByTagName("ConnectionStrings");

    XmlNode connString = connectionStrings.OfType<XmlNode>().FirstOrDefault();
    if(connString == null)
    {
        Console.WriteLine("ihrastinski_2018-10-22 07-27_pre: Can't find a connection strings node in xml. Press any key to exit...");
        Console.Read();
        return false;
    }
    
    try
    {
        File.WriteAllText(connStringsFilePath, connString.OuterXml);
    }
    catch(Exception e)
    {
        Console.WriteLine(string.Format("ihrastinski_2018-10-22 07-27_pre: Write file exception: {0}. Press any key to exit...", e.Message));
        Console.Read();
        return false;
    }
    return true;
}