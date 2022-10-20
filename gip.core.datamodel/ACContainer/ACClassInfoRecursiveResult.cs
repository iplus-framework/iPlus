using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class ACClassInfoRecursiveResult
    {

        #region ctor's

        public ACClassInfoRecursiveResult()
        {
            AllItems = new List<ACClassInfoRecursive>();
            Projects = new List<ACClassInfoRecursive>();

        }

        #endregion

        #region Factory

        public static ACClassInfoRecursiveResult Factory(Database database, string projectName, string[] bsoNames)
        {
            ACClassInfoRecursiveResult result = new ACClassInfoRecursiveResult();
            result.AllItems = 
                database
                .ACClass
                .Where(c => c.ACProject.ACProjectName == projectName && bsoNames.Contains(c.ACIdentifier))
                .ToList()
                .Select(c => 
                    new ACClassInfoRecursive(c) 
                    { 
                        OriginalAssigned = true,
                        IsChecked = true
                    
                    }).ToList();
            while (result.AllItems.Any(c => !c.Processed))
            {
                List<ACClassInfoRecursive> notProcessedItems = result.AllItems.Where(c => !c.Processed).ToList();
                foreach (var element in notProcessedItems)
                {
                    element.Build(database, result);
                }
            }
            return result;
        }

        #endregion

        #region Properties

        public List<ACClassInfoRecursive> AllItems { get; set; }
        public List<ACClassInfoRecursive> Projects { get; set; }

        #endregion
    }
}
