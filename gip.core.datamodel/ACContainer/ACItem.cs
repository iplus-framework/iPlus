using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace gip.core.datamodel
{
    [Serializable]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACItem'}de{'ACItem'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACItem
    {
        #region private members
        private List<ACItem> items;
        #endregion

        #region  ctor's

        public ACItem()
        {

        }

        public ACItem(ACClassInfoWithItems inputItem, Action<ACClass, ACItem> handleCreationEvent = null)
        {
            if (inputItem.ValueT != null)
            {
                ReadACClassData(inputItem.ValueT);
                if (handleCreationEvent != null)
                    handleCreationEvent(inputItem.ValueT, this);
            }
            else
            {
                ACIdentifier = inputItem.ACIdentifier;
                ID = Guid.Empty.ToString();
                ACTypeACIdentifier = "IPlusGroup";
            }

            if (inputItem.Items != null && inputItem.Items.Any())
            {
                Items = new List<ACItem>();
                foreach (var child in inputItem.Items)
                    Items.Add(new ACItem(child as ACClassInfoWithItems, handleCreationEvent));
            }
        }
        
        #endregion

        #region Properties

        #region Properties -> Export part

        public string ID { get; set; }

        public string ParentID { get; set; }

        [ACPropertyInfo(9999, "ACIdentifier", "en{'Key'}de{'Schlüssel'}")]
        public string ACIdentifier { get; set; }

        [ACPropertyInfo(9999, "ACUrl", "en{'ACUrl'}de{'ACUrl'}")]
        public string ACUrl { get; set; }

        [ACPropertyInfo(9999, "ACUrlComponent", "en{'ACUrlComponent'}de{'ACUrlComponent'}")]
        public string ACUrlComponent { get; set; }

        public bool IsAbstract { get; set; }

        public bool IsInterface { get; set; }

        public string AssemblyQualifiedName { get; set; }

        public string ACTypeACIdentifier { get; set; }

        [ACPropertyInfo(9999, "IsChecked", "en{'Translation'}de{'Übersetzung'}")]
        public string ACCaptionTranslation { get; set; }

        [ACPropertyInfo(9999, "Comment", "en{'Comment'}de{'Bemerkung'}")]
        public string Comment { get; set; }

        #endregion

        public List<ACItem> Items
        {
            get
            {
                if (items == null) items = new List<ACItem>();
                return items;
            }
            set
            {
                items = value;
            }
        }

        #endregion


        #region Methods
        public void ReadACClassData(ACClass acClass)
        {
            try
            {
                ID = acClass.ACClassID.ToString();
                // ParentID
                ACIdentifier = acClass.ACIdentifier;
                ACTypeACIdentifier = "ACClass";
                ACCaptionTranslation = acClass.ACCaptionTranslation;
                AssemblyQualifiedName = acClass.AssemblyQualifiedName;
                IsAbstract = acClass.IsAbstract;
                IsInterface = acClass.IsInterface;
                Comment = acClass.Comment;
                ACUrl = acClass.GetACUrl();
                ACUrlComponent = FactoryACUrlComponent(acClass);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                this.Root().Messages.LogException("ReadACClassData", "ReadACClassData", msg);
            }
        }

        public static string FactoryACUrlComponent(ACClass item)
        {
            string url = item.ACURLComponentCached;
            if (String.IsNullOrEmpty(url))
                url = item.GetACUrlComponent();
            string pattern = @"\((\d*)\)";
            url = Regex.Replace(url, pattern, "()");
            return url;
        }
        #endregion
    }
}
