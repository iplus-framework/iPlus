using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.manager
{
    public class NodeInfo : IACObject, IACClassDesignProvider
    {
        public NodeInfo(ACClass pwACClass)
        {
            PWACClass = pwACClass;
            PAACClass = null;
            PAACClassMethod = null;
            ParentPAACClass = null;
            NodeItem = null;
        }

        public NodeInfo(ACClass pwACClass, IACObject nodeItem)
        {
            PWACClass = pwACClass;
            NodeItem = nodeItem;
            PAACClass = null;
            PAACClassMethod = null;
            ParentPAACClass = null;
        }

        public NodeInfo(ACClass pwACClass, ACClass paACClass, ACClass parentPAACClass)
        {
            PWACClass = pwACClass;
            PAACClass = paACClass;
            PAACClassMethod = null;
            ParentPAACClass = parentPAACClass;
            NodeItem = null;
        }

        public NodeInfo(ACClass pwACClass, ACClass paACClass, ACClass parentPAACClass, ACClassMethod paACClassMethod)
        {
            PWACClass = pwACClass;
            PAACClass = paACClass;
            PAACClassMethod = paACClassMethod;
            ParentPAACClass = parentPAACClass;
            NodeItem = null;
        }

        public ACClass PWACClass { get; set; }
        public ACClass PAACClass { get; set; }
        public ACClassMethod PAACClassMethod { get; set; }
        public ACClass ParentPAACClass { get; set; }
        public IACObject NodeItem { get; set; }

        #region IACObjectWithBinding Member

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return PWACClass.ACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return PWACClass.IsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return PWACClass.ACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }


        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return PWACClass.ACContentList;
            }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                if (PWACClass == null)
                    return null;
                return PWACClass.ACType;
            }
        }

        public string GetACUrlComponent(IACObject rootACObject = null)
        {
            return PWACClass.GetACUrlComponent(rootACObject);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("","", 9999)]
        public string GetACUrl(IACObject rootACObject = null)
        {
            return PWACClass.GetACUrl(rootACObject);
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get { return PWACClass.ACCaption; }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        public string ACIdentifier
        {
            get
            {
                return PWACClass.ACIdentifier;
            }
        }


        /// <summary>Returns a ACClassDesign for presenting itself on the gui</summary>
        /// <param name="acUsage">Filter for selecting designs that belongs to this ACUsages-Group</param>
        /// <param name="acKind">Filter for selecting designs that belongs to this ACKinds-Group</param>
        /// <param name="vbDesignName">Optional: The concrete acIdentifier of the design</param>
        /// <returns>ACClassDesign</returns>
        public ACClassDesign GetDesign(Global.ACUsages acUsage, Global.ACKinds acKind, string vbDesignName = "")
        {
            return PWACClass.GetDesign(PWACClass, acUsage, acKind, vbDesignName);
        }
        #endregion
    }
}
