using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Windows.Markup;
using System.Windows.Controls;

namespace gip.core.layoutengine
{
    public class VBValidationRule : ValidationRule, IACObject
    {
        #region c'tors
        public VBValidationRule() 
            : base()
        {
        }

        public VBValidationRule(IACObject acComponent, string vbContent, string acUrlCommand)
        {
            ACComponent = acComponent;
            this.VBContent = vbContent;
            this.ACUrlCmd = acUrlCommand;
        }

        public VBValidationRule(ValidationStep validationStep, bool validatesOnTargetUpdated, IACObject acComponent, string vbContent, string acUrlCommand)
            : base(validationStep, validatesOnTargetUpdated)
        {
            ACComponent = acComponent;
            this.VBContent = vbContent;
            this.ACUrlCmd = acUrlCommand;
        }
        #endregion

        #region XAML-Deklarative-Properties
        private bool _GlobalFunction;
        public bool GlobalFunction
        {
            get
            {
                return _GlobalFunction;
            }
            set
            {
                _GlobalFunction = value;
                UpdateCalculator();
            }
        }

        private string _VBContent = "";
        public string VBContent
        {
            get
            {
                return _VBContent;
            }
            set
            {
                _VBContent = value;
            }
        }

        // Property kann nicht ACUrlCommand heissen, da ACUrlCommand bereits eine Methode ist.
        private string _ACUrlCmd;
        /// <summary>
        /// Name of Script-Method which should be called
        /// </summary>
        public string ACUrlCmd
        {
            get
            {
                return _ACUrlCmd;
            }
            set
            {
                _ACUrlCmd = value;
                UpdateCalculator();
            }
        }
        #endregion

        private void UpdateCalculator()
        {
            //if (ACComponent == null)
            {
                if (GlobalFunction && (Layoutgenerator.Root != null))
                    this.ACComponent = Layoutgenerator.Root.Environment;
                else if (!GlobalFunction && Layoutgenerator.CurrentACComponent != null)
                    this.ACComponent = Layoutgenerator.CurrentACComponent;
                else
                    this.ACComponent = null;
            }
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (ACComponent == null)
                return new ValidationResult(false, "ACComponent == null");
            else if (String.IsNullOrEmpty(ACUrlCmd))
                return new ValidationResult(false, "ACUrlCommand is Empty");

            object result = ACComponent.ACUrlCommand(ACUrlCmd, VBContent, value, cultureInfo);
            if (result == null)
                return ValidationResult.ValidResult;
            else if (result is Boolean)
            {
                bool valid = (bool)result;
                if (valid)
                    return ValidationResult.ValidResult;
                else
                    return new ValidationResult(false, "");
            }
            else if (result is Msg)
            {
                Msg msg = result as Msg;
                string message = msg.Message;
                if (String.IsNullOrEmpty(message) && msg is MsgWithDetails)
                    message = (msg as MsgWithDetails).InnerMessage;
                if (msg.MessageLevel == eMsgLevel.Failure
                    || msg.MessageLevel == eMsgLevel.Error
                    || msg.MessageLevel == eMsgLevel.Exception
                    || msg.MessageLevel == eMsgLevel.Warning)
                {
                    return new ValidationResult(false, message);
                }
                else
                    return new ValidationResult(true, message);
            }
            return ValidationResult.ValidResult;
        }

        #region public member
        private ACRef<IACObject> _ACComponentRef;
        public IACObject ACComponent
        {
            get
            {
                if (_ACComponentRef == null)
                    return null;
                return _ACComponentRef.ValueT;
            }
            set
            {
                if (_ACComponentRef != null)
                    _ACComponentRef.Detach();
                if (value == null)
                    _ACComponentRef = null;
                else
                    _ACComponentRef = new ACRef<IACObject>(value, this, true);
            }
        }
        #endregion

        #region IACObject member
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return ""; }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get;
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return null; }
        }

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
            return null;
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return false;
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
        public string GetACUrl(IACObject rootACObject = null)
        {
            return "";
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return ""; }
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
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }
        #endregion
    }
}
