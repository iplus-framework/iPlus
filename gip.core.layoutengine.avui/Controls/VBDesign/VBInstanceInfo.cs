using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    // Folgende Regeln gelten für die Instanziierung von Managern:
    // 1. Ist nichts im XAML definiert, wird eine Managerinstanz anhand des Key´s erzeugt
    // 2. Bei VBDesign´s die einen Workflow anzeigen müssen die Instanzinfos angegeben werden. Der ACIdentifier ist hier 
    // in der Regel der VBContent vom VBDesign
    //   <vb:VBDesign VBContent="CurrentWorkOrderMethod">
    //       <vb:VBInstanceInfo Key="VBDesigner"    ACIdentifier="VBDesignerWorkflowMethod(CurrentWorkOrderMethod)" />
    //       <vb:VBInstanceInfo Key="VBBSOSelectionManager" ACIdentifier="VBBSOSelectionManager(CurrentWorkOrderMethod)" />
    //       <vb:VBInstanceInfo Key="VBBSOControlDialog"  ACIdentifier="VBBSOControlDialog(CurrentWorkOrderMethod)" />
    //   </vb:VBDesign>
    //
    // Dadurch ist auch die Entität in der Lage den passenden Designmanager zu erzeugen.

    /// <summary>
    /// Represents the control which holds the data for a another components instantiating.
    /// </summary>
    public class VBInstanceInfo
    {
        #region XAML Properties

        /// <summary>
        /// Schlüssel nach dem gesucht wird um die Instanziformationen zu erhalten.
        /// Sollte nach Best-Practise in der Regel der ACClass-Identifier sein, der eindeutig ist
        /// </summary>
        [Category("VBControl")]
        public string Key
        {
            get;
            set;
        }

        string _ACIdentifier;
        /// <summary>
        /// Vollständiger ACIdentifier der ACComponent mit der sie instanziiert werden soll
        /// z.B. VBBSOControlDialog(CurrentWorkOrderMethod)
        /// </summary>
        /// <summary>
        /// Gets or sets the ACIdentifier.
        /// </summary>
        [Category("VBControl")]
        public string ACIdentifier
        {
            get
            {
                return _ACIdentifier;
            }
            set
            {
                _ACIdentifier = value;
            }
        }

        /// <summary>
        /// Determines is setted as data context or not.
        /// </summary>
        [Category("VBControl")]
        public bool SetAsDataContext
        {
            get;
            set;
        }

        /// <summary>
        /// Determines is setted as BSOACComponent or not.
        /// </summary>
        [Category("VBControl")]
        public bool SetAsBSOACComponet
        {
            get;
            set;
        }

        /// <summary>
        /// Determines is a instance automatically started or not.
        /// </summary>
        [Category("VBControl")]
        public bool AutoStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the parameters for automatic start.
        /// </summary>
        [Category("VBControl")]
        public string AutoStartParameter
        {
            get;
            set;
        }

        /// <summary>
        /// Builds the automatic start parameter.
        /// </summary>
        /// <returns>The automatic start parameter as objects array.</returns>
        public object[] BuildStartParameter()
        {
            if (String.IsNullOrEmpty(AutoStartParameter))
                return new object[]{};
            string[] parameter = AutoStartParameter.Split(',');
            return parameter;
        }
        #endregion
    }

    /// <summary>
    /// Represents the list of VBInstanceInfo.
    /// </summary>
    public class VBInstanceInfoList : List<VBInstanceInfo>
    {
        /// <summary>
        /// Gets the instance info by key.
        /// </summary>
        /// <param name="key">The key parameter.</param>
        /// <returns>The instance info if is found.</returns>
        public VBInstanceInfo GetInstanceInfoByKey(string key)
        {
            return this.Where(c => c.Key == key).FirstOrDefault();
        }

        /// <summary>
        /// Gets the instance info by ACIdentifier.
        /// </summary>
        /// <param name="acIdentifier">The acIdentifier parameter.</param>
        /// <returns>The instance info if is found.</returns>
        public VBInstanceInfo GetInstanceInfoByACIdentifier(string acIdentifier)
        {
            return this.Where(c => c.ACIdentifier == acIdentifier).FirstOrDefault();
        }
    }
}
