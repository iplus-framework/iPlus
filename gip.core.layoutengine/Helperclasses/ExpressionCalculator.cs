using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Reflection;
using gip.core.datamodel;
using System.Linq.Expressions;

namespace gip.core.layoutengine
{
    public class ExpressionCalculator : IACObject, ICalculator
    {
        #region private members
        private ConverterBase _Converter;

        private ExprParser _Parser;
        private LambdaExpression _ParserLambdaExpr;
        #endregion

        #region c'tors
        public ExpressionCalculator(ConverterBase converter)
            : this(converter, null)
        {
        }

        public ExpressionCalculator(ConverterBase converter, IACComponent acComponent)
        {
            ACComponent = acComponent;
            _Converter = converter;
            ReloadExpression();
        }
        #endregion

        #region public member
        private ACRef<IACComponent> _ACComponentRef;
        public IACComponent ACComponent
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
                    _ACComponentRef = new ACRef<IACComponent>(value, this, true);
            }
        }
        #endregion

        #region public methods
        public void ReloadExpression()
        {
            if (_Converter.ConversionBy == ConverterBase.ConvType.Expression)
            {
                try
                {
                    _Parser = new ExprParser();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ExpressionCalculator", "ReloadExpression", msg);

                    _Parser = null;
                    _ParserLambdaExpr = null;
                }
            }
        }

        public ICalculatorResult Calculate(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture, out object result)
        {
            if (_Converter.ConversionBy == ConverterBase.ConvType.ScriptEngine)
            {
                if ((_ACComponentRef != null) && (_ACComponentRef.ValueT != null))
                {
                    object[] parameters;
                    if ((_Converter.MethodParameters != null) && (_Converter.MethodParameters.Count > 0))
                    {
                        foreach (object arrVal in values)
                        {
                            _Converter.MethodParameters.Add(arrVal);
                        }
                        parameters = (_Converter.MethodParameters as List<object>).ToArray();
                    }
                    else
                    {
                        parameters = values.ToArray();
                    }
                    result = _ACComponentRef.ValueT.ACUrlCommand(_Converter.ACUrlCommand, parameters);
                    return ICalculatorResult.FromScriptEngine;
                }
                else
                {
                    result = values[0];
                    return ICalculatorResult.Error;
                }
            }
            else if (_Converter.ConversionBy == ConverterBase.ConvType.Expression)
            {
                if (_Parser != null)
                {
                    try
                    {
                        foreach (object value in values)
                        {
                            if (value is String)
                            {
                                if ((value as String) == Const.ACRootClassName)
                                {
                                    result = value;
                                    return ICalculatorResult.Error;
                                }
                            }
                        }

                        if (_ParserLambdaExpr == null)
                            _ParserLambdaExpr = _Parser.Parse(_Converter.Expression);
                        result = _Parser.Run(_ParserLambdaExpr, values);
                        return ICalculatorResult.FromExpression;
                    }
                    catch (Exception e)
                    {
                        string acUrl = "";
                        IRoot vb = null;
                        if ((_ACComponentRef != null) && (_ACComponentRef.ValueT != null))
                        {
                            acUrl = _ACComponentRef.ValueT.GetACUrl();
                            vb = _ACComponentRef.ValueT.Root;
                        }
                        if (vb == null)
                            vb = Database.Root;
                        if (vb != null)
                        {
                            vb.Messages.LogException("ExpressionCalculator", acUrl, e.Message);
                            if (e.InnerException != null)
                                vb.Messages.LogException("ExpressionCalculator", acUrl, e.InnerException.Message);
                        }
                    }
                }
                result = values[0];
                return ICalculatorResult.Error;
            }

            result = values[0];
            return ICalculatorResult.Error;
        }
        #endregion


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
            get { return null; }
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

        #region IACObject Member

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get;
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
