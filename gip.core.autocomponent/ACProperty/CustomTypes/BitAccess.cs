using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'BitAccessMap'}de{'BitAccessMap'}", Global.ACKinds.TACClass)]
    public class BitAccessMap
    {
        [DataMember]
        public string NameOfBit {get;set;}
        
        [DataMember]
        public string Bit {get;set;}
    }

    public interface IBitAccess : IBitAccessBase
    {
        List<BitAccessMap> BitAccessMap { get; }
        bool GetBitValue(string Bit);
        void SetBitValue(string Bit, bool value);
        bool GetBitValue(short Bit);
        void SetBitValue(short Bit, bool value);
        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        object Value { get; set; }
        void CloneCustomProperties(ACCustomTypeBase Target);
        Type UnderlyingType { get; }
    }

    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'BitAccess'}de{'Bitzugriff'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public abstract class BitAccess<T> : ACCustomType<T>, IBitAccess, IACObject, IFormattable, IConvertible where T : IComparable<T>
    {
        #region c'tors
        public BitAccess()
        {
        }

        public BitAccess(IACType acValueType) 
               : base(acValueType)
        {
            if (acValueType != null)
            {
                var query = acValueType.ValueTypeACClass.Properties
                                        .Where(c => c.ACIdentifier.Length == 5 && c.ACIdentifier.StartsWith("Bit"))
                                        .Select(c => new BitAccessMap { Bit = c.ACIdentifier, NameOfBit = c.ACCaption });
                if (query.Any())
                    _BitAccessMap = query.ToList();
            }
        }
        #endregion

        #region public Member
        [ACPropertyInfo(9999)]
        public bool this[string NameOfBit]
        {
            get
            {
                var query = BitAccessMap.Where(c => c.NameOfBit == NameOfBit).Select(c => c.Bit);
                if (query.Any())
                    return GetBitValue(query.First());
                else
                {
                    query = BitAccessMap.Where(c => c.Bit == NameOfBit).Select(c => c.Bit);
                    if (query.Any())
                        return GetBitValue(query.First());
                }
                return false;
            }
            set
            {
                var query = BitAccessMap.Where(c => c.NameOfBit == NameOfBit).Select(c => c.Bit);
                if (query.Any())
                    SetBitValue(query.First(), value);
                else
                {
                    query = BitAccessMap.Where(c => c.Bit == NameOfBit).Select(c => c.Bit);
                    if (query.Any())
                        SetBitValue(query.First(), value);
                }
            }
        }

        [IgnoreDataMember]
        protected List<BitAccessMap> _BitAccessMap = new List<BitAccessMap>();
        [ACPropertyInfo(9999)]
        public List<BitAccessMap> BitAccessMap
        {
            get
            {
                return _BitAccessMap;
            }

            internal set
            {
                _BitAccessMap = value;
            }
        }

        public string GetBitNameFromMap(string bit)
        {
            if (_BitAccessMap == null)
                return null;
            var entry = _BitAccessMap.Where(c => c.Bit == bit).FirstOrDefault();
            if (entry == null)
                return null;
            return entry.NameOfBit;
        }

        /// <summary>
        /// Gets the underlying type of bit access.
        /// </summary>
        /// <param name="bitAccessType">Type of the bit access.</param>
        /// <returns>System.String.</returns>
        public static string GetUnderlyingTypeOfBitAccess(Type bitAccessType)
        {
            if (bitAccessType == null)
                return "";
            Type[] genericArgs = bitAccessType.GetGenericArguments();
            if (genericArgs != null && genericArgs.Any())
            {
                Type type = genericArgs.FirstOrDefault();
                return type.Name;
            }
            if (bitAccessType.BaseType != null)
            {
                if (bitAccessType.BaseType == typeof(object))
                    return "";
                return GetUnderlyingTypeOfBitAccess(bitAccessType.BaseType);
            }
            return "";
        }

        public Type UnderlyingType 
        {
            get
            {
                return typeof(T);
            }
        }
        #endregion

        #region Override
        public override void CloneCustomProperties(ACCustomTypeBase Target)
        {
            if (Target == null)
                return;
            ((BitAccess<T>)Target).BitAccessMap = this.BitAccessMap;
        }
        #endregion

        #region Abstract Member
        public abstract bool GetBitValue(string Bit);
        public abstract void SetBitValue(string Bit, bool value);
        public abstract bool GetBitValue(short Bit);
        public abstract void SetBitValue(short Bit, bool value);
        #endregion

        #region IACUrl Member

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
        public object ACUrlCommand(string acUrl, params Object[] acParameter)
        {

            if (acParameter == null || !acParameter.Any())
            {
                return this[acUrl];
            }
            else
            {
                if (acParameter[0] is bool)
                    this[acUrl] = (bool) acParameter[0];
            }

            return false;
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return true;
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

        public string GetACUrlComponent(IACObject rootACObject = null)
        {
            return null;
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
            return null;
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

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        public override string ACCaption
        {
            get;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        public override string ACIdentifier
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
        }
        #endregion

        #region IConvertible
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(ValueT);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(ValueT);
        }

        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(ValueT);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(ValueT);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(ValueT);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(ValueT);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(ValueT);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(ValueT);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(ValueT);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(ValueT);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(ValueT);
        }

        public string ToString(IFormatProvider provider)
        {
            return Convert.ToString(ValueT);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            Type typeBitAccess = typeof(IBitAccess);
            if (typeBitAccess.IsAssignableFrom(conversionType))
            {
                IBitAccess newBitAccess = Activator.CreateInstance(conversionType, this.ACType) as IBitAccess;
                if (newBitAccess != null)
                {
                    if (newBitAccess is ACCustomTypeBase)
                        CloneCustomProperties((ACCustomTypeBase)newBitAccess);
                    newBitAccess.Value = this.Value;
                }
                return newBitAccess;
            }
            else
                return Convert.ChangeType(ValueT, conversionType);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(ValueT);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(ValueT);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(ValueT);
        }
        #endregion

        #region IFormatable
        public override string ToString()
        {
            if (Value == null)
                return base.ToString();
            return this.Value.ToString();
        }

        public string DumpBitAccessMapWithValues(bool onlyTrueValues = true)
        {
            if (BitAccessMap == null)
                return "";
            StringBuilder sb = new StringBuilder();
            foreach (var mapElement in BitAccessMap)
            {
                if (onlyTrueValues)
                {
                    if (GetBitValue(mapElement.Bit))
                        sb.AppendFormat("<{0}/{1}>", mapElement.Bit, mapElement.NameOfBit);
                }
                else
                    sb.AppendFormat("<{0}/{1}: {2}>", mapElement.Bit, mapElement.NameOfBit, GetBitValue(mapElement.Bit));
            }
            return sb.ToString();
        }

        public abstract string ToString(string format, IFormatProvider formatProvider);
        #endregion


        public abstract void SetFromString(string value);
    }
}
