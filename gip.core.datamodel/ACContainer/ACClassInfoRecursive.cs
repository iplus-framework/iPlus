using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Classinfo'}de{'Klasseninformation'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACClassInfoRecursive : ACClassInfoWithItems
    {
        #region c´tors
       
        public ACClassInfoRecursive(ACClass acClass)
            : base(acClass)
        {
            ACClassID = acClass.ACClassID;
            ParentACClassID = acClass.ParentACClassID;
            if (acClass.ParentACClassID == null)
                ACProjectID = acClass.ACProjectID;
            ACCaption = acClass.ACIdentifier;
        }

        public ACClassInfoRecursive(ACProject aCProject)
        {
            ACProjectID = aCProject.ACProjectID;
            ACCaption = aCProject.ACProjectName;
        }

        #endregion

        #region Recursive
        public bool Processed { get; set; }

        public bool OriginalAssigned { get; set; }

        public Guid ACClassID { get; set; }
        public Guid? ParentACClassID { get; set; }
        public Guid? ACProjectID { get; set; }

        public void Build(Database database, ACClassInfoRecursiveResult result)
        {
            BuildParents(database, result);
            BuildChildren(database, result);
            Processed = true;
        }


        public void BuildParents(Database database, ACClassInfoRecursiveResult result)
        {
            if (ParentACClassID == null && ACClassID != Guid.Empty)
            {
                var tmpParent = result.AllItems.FirstOrDefault(c => ((c.ACProjectID ?? Guid.Empty) == (ACProjectID ?? Guid.Empty)) && c.ACClassID == Guid.Empty);
                if (tmpParent == null)
                {
                    ACProject acProject = database.ACProject.FirstOrDefault(c => c.ACProjectID == (ACProjectID ?? Guid.Empty));
                    tmpParent = new ACClassInfoRecursive(acProject);
                    if (!result.AllItems.Any(c => (c.ACProjectID ?? Guid.Empty) == (tmpParent.ACProjectID ?? Guid.Empty) && c.ACClassID == Guid.Empty))
                        result.AllItems.Add(tmpParent);
                    if (!result.Projects.Any(c => (c.ACProjectID ?? Guid.Empty) == (tmpParent.ACProjectID ?? Guid.Empty) && c.ACClassID == Guid.Empty))
                        result.Projects.Add(tmpParent);
                }
                ParentContainerT = tmpParent;
            }
            else
            {
                var tmpParent = result.AllItems.FirstOrDefault(c => c.ACClassID == (ParentACClassID ?? Guid.Empty) && c.ACClassID != Guid.Empty);
                if (tmpParent == null && ACClassID != Guid.Empty)
                {
                    ACClass parentACClass = database.ACClass.FirstOrDefault(c => c.ACClassID == (ParentACClassID ?? Guid.Empty));
                    tmpParent = new ACClassInfoRecursive(parentACClass);
                    if (!result.AllItems.Any(c => c.ACClassID == tmpParent.ACClassID))
                        result.AllItems.Add(tmpParent);
                }
                ParentContainerT = tmpParent;
            }
        }

        public void BuildChildren(Database database, ACClassInfoRecursiveResult result)
        {
            if (ACClassID == Guid.Empty && ACProjectID != null)
            {
                _ItemsT = result.AllItems.Where(c => (c.ACProjectID ?? Guid.Empty) == (ACProjectID ?? Guid.Empty) && c.ACClassID != Guid.Empty).Select(c=>c as ACClassInfoWithItems).ToList();
            }
            else
            {
                Guid[] childrenIds = database.ACClass.Where(c => c.ParentACClassID == ACClassID).Select(c => c.ACClassID).ToArray();
                foreach (var acClassID in childrenIds)
                {
                    ACClassInfoRecursive childPresentation = result.AllItems.FirstOrDefault(c => c.ACClassID == acClassID);
                    if (childPresentation == null && OriginalAssigned)
                    {
                        ACClass childACClass = database.ACClass.FirstOrDefault(c => c.ACClassID == acClassID);
                        childPresentation = new ACClassInfoRecursive(childACClass);
                        childPresentation.IsChecked = IsChecked;
                        if (!result.AllItems.Any(c => c.ACClassID == childPresentation.ACClassID))
                            result.AllItems.Add(childPresentation);
                    }
                    if (childPresentation != null)
                        _ItemsT.Add(childPresentation);
                }
            }
        }
        #endregion

        #region overrides

        public override string ToString()
        {
            return ((ACClassID == Guid.Empty) ? "P" : "C") + " - " + ACIdentifier;
        }

        #endregion
    }
}
