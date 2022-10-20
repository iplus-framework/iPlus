<Query Kind="Statements">
  <Output>DataGrids</Output>
</Query>

// 

string sourceFolder = @"C:\Aleksandar\gipSoft\Source\iPlusMES\V4\trunk\BranchSCoffee\gipcoffee.mes.sql\Programmability\AldiMV4";
string targetFolder = @"C:\Aleksandar\gipSoft\Source\iPlusMES\V4\trunk\BranchSCoffee\gipcoffee.mes.sql\Query\buildFPQuery";

string[] filterNames = new string[] 
{

};


string buildFile = string.Format(@"build-file-{0}.sql", DateTime.Now.ToString("yyyy-MM-dd_hh-mm"));


List<SQLScriptGroup> groups = new List<UserQuery.SQLScriptGroup>()
{
	new SQLScriptGroup(){GroupName = "udw"},
	new SQLScriptGroup(){GroupName = "udf"},
	new SQLScriptGroup(){GroupName = "udp"}
};

foreach (var group in groups)
{
	group.Files = 
		new DirectoryInfo(sourceFolder + "\\" + group.GroupName)
		.GetFiles("*.sql")
		.Where(c => !filterNames.Any() || filterNames.Contains(c.Name))
		.OrderBy(c => c.Name)
		.Select(c => c.FullName)
		.ToList();
}


string tempLine = "";
using (FileStream outStream = new FileStream(targetFolder + @"\" + buildFile, FileMode.OpenOrCreate))
{
	using (StreamWriter sw = new StreamWriter(outStream))
	{
		foreach (var group in groups)
		{
			sw.WriteLine("-- " + group.GroupName);
			foreach (var file in group.Files)
			{
				sw.WriteLine("");
				sw.WriteLine(string.Format(@"-- {0}", Path.GetFileName(file)));
				using (FileStream readUdw = new FileStream(file, FileMode.Open))
				{
					using (StreamReader reader = new StreamReader(readUdw))
					{
						while ((tempLine = reader.ReadLine()) != null)
						{
							sw.WriteLine(tempLine);
						}
					}
					sw.WriteLine("GO");
					sw.WriteLine("");
				}
			}
		}
	}
}

}
public class SQLScriptGroup
{
	public string GroupName { get; set; }
	public List<string> Files { get; set; }
