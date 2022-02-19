// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-07-2012
// ***********************************************************************
// <copyright file="XMLToObjectConverter.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
#if NETFRAMEWORK
using System.Data.Objects.DataClasses;
#endif
using System.Data;
using System.Globalization;
using System.Threading;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACConvert
    /// </summary>
    public static class ACConvert
    {
#if NETFRAMEWORK
        public static DataContractResolver MyDataContractResolver { get; set; }


#region Public Methods
        /// <summary>
        /// Gibt ein Objekt vom angegebenen Typ zurück, dessen Wert dem angegebenen Objekt entspricht.
        /// </summary>
        /// <param name="value">Ein TimeSpan-, oder DateTime-, oder EntityKey-, oder EntityObject-, oder ACQueryDefinition-Objekt
        /// oder ein Objekt, das die System.IConvertible-Schnittstelle implementiert
        /// oder ein serialisierbares ACKnownTypes-Objekt</param>
        /// <param name="conversionType">Der System.Type des zurückzugebenden Objekts.</param>
        /// <param name="invariantCulture">invariantCulture</param>
        /// <param name="database">Referenz zum ObjectContext, falls ein Entity-Objekt attached wird</param>
        /// <param name="entityAsEntityKey">Falls value ein EntityObject: Soll das EntityObject als EntityKey serialisiert werden oder als ACUrl-String falls value auch ein IACObject ist</param>
        /// <param name="xmlIndented">Falls conversionType ein String und value ACKnownTypes-Objekt ist, soll XML-String Einzüge haben</param>
        /// <returns>Ein Objekt, dessen Typ gleich conversionType ist und dessen Wert value entspricht.–
        /// oder –Ein NULL-Verweis (Nothing in Visual Basic), wenn valuenull ist und
        /// conversionType kein Werttyp ist.</returns>
        /// <exception cref="System.InvalidCastException">Diese Konvertierung wird nicht unterstützt. – oder –value ist null, und conversionType
        /// ist ein Werttyp.– oder –value implementiert die System.IConvertible-Schnittstelle
        /// nicht.</exception>
        /// <exception cref="System.FormatException">value weist kein von conversionType erkanntes Format auf.</exception>
        /// <exception cref="System.OverflowException">value stellt eine Zahl dar, die außerhalb des Bereichs von conversionType liegt.</exception>
        /// <exception cref="System.ArgumentNullException">conversionType ist null</exception>
        public static object ChangeType(object value, Type conversionType, bool invariantCulture, IACEntityObjectContext database, bool entityAsEntityKey = false, bool xmlIndented = false)
        {
            return ChangeType(value, null, conversionType, invariantCulture, database, entityAsEntityKey, xmlIndented);
        }

        /// <summary>
        /// Gibt ein Objekt vom angegebenen Typ zurück, dessen Wert dem angegebenen Objekt entspricht.
        /// </summary>
        /// <param name="value">Ein TimeSpan-, oder DateTime-, oder EntityKey-, oder EntityObject-, oder ACQueryDefinition-Objekt
        /// oder ein Objekt, das die System.IConvertible-Schnittstelle implementiert
        /// oder ein serialisierbares ACKnownTypes-Objekt</param>
        /// <param name="previouslyConvertedValue">value, der zuvor in den conversionType gewandelt wurde.</param>
        /// <param name="conversionType">Der System.Typ des zurückzugebenden Objekts.</param>
        /// <param name="invariantCulture">invariantCulture</param>
        /// <param name="database">Referenz zum ObjectContext, falls ein Entity-Objekt attached wird</param>
        /// <param name="entityAsEntityKey">Falls value ein EntityObject: Soll das EntityObject als EntityKey serialisiert werden oder als ACUrl-String falls value auch ein IACObject ist</param>
        /// <param name="xmlIndented">Falls conversionType ein String und value ACKnownTypes-Objekt ist, soll XML-String Einzüge haben</param>
        /// <returns>Ein Objekt, dessen Typ gleich conversionType ist und dessen Wert value entspricht.–
        /// oder –Ein NULL-Verweis (Nothing in Visual Basic), wenn valuenull ist und
        /// conversionType kein Werttyp ist.</returns>
        /// <exception cref="System.InvalidCastException">Diese Konvertierung wird nicht unterstützt. – oder –value ist null, und conversionType
        /// ist ein Werttyp.– oder –value implementiert die System.IConvertible-Schnittstelle
        /// nicht.</exception>
        /// <exception cref="System.FormatException">value weist kein von conversionType erkanntes Format auf.</exception>
        /// <exception cref="System.OverflowException">value stellt eine Zahl dar, die außerhalb des Bereichs von conversionType liegt.</exception>
        /// <exception cref="System.ArgumentNullException">conversionType ist null</exception>
        public static object ChangeType(object value, object previouslyConvertedValue, Type conversionType, bool invariantCulture, IACEntityObjectContext database, bool entityAsEntityKey = false, bool xmlIndented = false)
        {
            return ChangeType(value, previouslyConvertedValue, conversionType, invariantCulture, database, null, entityAsEntityKey, xmlIndented);
        }

        /// <summary>
        /// XMLs to object.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="valueXML">The value XML.</param>
        /// <param name="invariantCulture">invariantCulture</param>
        /// <param name="database">The database.</param>
        /// <returns>System.Object.</returns>
        static public object XMLToObject(Type objectType, string valueXML, bool invariantCulture, IACEntityObjectContext database)
        {
            return ACConvert.ChangeType(valueXML, null, objectType, invariantCulture, database);
        }

        static public T XMLToObject<T>(string valueXML, bool invariantCulture, IACEntityObjectContext database)
        {
            Type type = typeof(T);
            object result = XMLToObject(type, valueXML, invariantCulture, database);
            if (result == null)
                return default(T);
            return (T)result;
        }

        /// <summary>
        /// Objects to XML.
        /// </summary>
        /// <param name="valueObject">The value object.</param>
        /// <param name="invariantCulture">invariantCulture</param>
        /// <param name="entityAsEntityKey">if set to <c>true</c> [entity as entity key].</param>
        /// <param name="xmlIndented">if set to <c>true</c> [XML indented].</param>
        /// <returns>System.String.</returns>
        static public string ObjectToXML(object valueObject, bool invariantCulture, bool entityAsEntityKey = false, bool xmlIndented = false)
        {
            return ACConvert.ChangeType(valueObject, null, typeof(string), invariantCulture, null, entityAsEntityKey, xmlIndented) as string;
        }

#endregion

#region Private Methods
        /// <summary>
        /// Changes the type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="previouslyConvertedValue">The previously converted value.</param>
        /// <param name="conversionType">Type of the conversion.</param>
        /// <param name="invariantCulture">invariantCulture</param>
        /// <param name="database">The database.</param>
        /// <param name="conversionACType">Type of the conversion AC.</param>
        /// <param name="entityAsEntityKey">if set to <c>true</c> [entity as entity key].</param>
        /// <param name="xmlIndented">if set to <c>true</c> [XML indented].</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="System.ArgumentNullException">conversionType ist null</exception>
        /// <exception cref="System.InvalidCastException">
        /// Not supported Type
        /// or
        /// Type of oldValue is different to conversionType
        /// or
        /// Not convertable
        /// or
        /// Not convertable
        /// or
        /// Not convertable
        /// or
        /// EntityKey not convertable to  + conversionType.Name
        /// or
        /// EntityObject not convertable to  + conversionType.Name
        /// or
        /// ACKnownType only convertable to string
        /// or
        /// ACQueryDefinition only convertable to string
        /// or
        /// Not supported Type
        /// </exception>
        private static object ChangeType(object value, object previouslyConvertedValue, Type conversionType, bool invariantCulture, IACEntityObjectContext database, IACType conversionACType, bool entityAsEntityKey, bool xmlIndented)
        {
            // TODO: IACType Conversion
            if (conversionType == null)
                throw new ArgumentNullException("conversionType ist null");
            if (value == null)
            {

                // Konvertiere in ein TimeSpan
                if (typeof(TimeSpan).IsAssignableFrom(conversionType))
                {
                    return TimeSpan.Zero;
                }
               
                // Konvertiere in ein DateTime
                else if (typeof(DateTime).IsAssignableFrom(conversionType))
                {
                    return DateTime.MinValue;
                }
                // Konvertiere in eine primitiver Typ ist der IConvertible implementiert z.B. String, int, ....
                else if (typeof(IConvertible).IsAssignableFrom(conversionType))
                {
                    return conversionType.GetDefault();
                    //return conversionType.IsValueType ? (!conversionType.IsGenericType ? Activator.CreateInstance(conversionType) : conversionType.GenericTypeArguments[0].GetDefault()) : null;
                    //if (invariantCulture)
                    //    return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
                    //else
                    //    return Convert.ChangeType(value, conversionType);
                }
                // Konvertiere in ein EntityKey
                else if (typeof(EntityKey).IsAssignableFrom(conversionType))
                {
                    //throw new InvalidCastException("Null-Value can't be converted to EntityKey");
                    return null;
                }
                // Konvertiere in ein EntityObject
                else if (typeof(EntityObject).IsAssignableFrom(conversionType))
                {
                    //throw new InvalidCastException("Null-Value can't be converted to EntityObject");
                    return null;
                }
                // Falls ACQueryDefinition
                else if (typeof(ACQueryDefinition).IsAssignableFrom(conversionType))
                {
                    //throw new InvalidCastException("Null-Value can't be converted to ACQueryDefinition");
                    return null;
                }
                else if (conversionType.Name == Const.TNameNullable)
                {
                    return null;
                }
                else if (ACKnownTypes.IsKnownType(conversionType))
                {
                    if (conversionType.IsValueType)
                    {
                        return conversionType.GetDefault();
                    }
                    return null;
                }
                else if (!conversionType.IsValueType)
                {
                    return null;
                }

                throw new InvalidCastException("Not supported Type");
            }
            else
            {
                if (value is RuleValueList || conversionType == typeof(RuleValueList))
                {
                    if (typeof(string).IsAssignableFrom(conversionType))
                    {
                        return SerializeObject(value.GetType(), value, xmlIndented);
                    }
                    else if (typeof(string).IsAssignableFrom(value.GetType()))
                    {
                        string valueString = value as string;
                        using (StringReader ms = new StringReader(valueString))
                        using (XmlTextReader xmlReader = new XmlTextReader(ms))
                        {
                            DataContractSerializer serializer = new DataContractSerializer(conversionType, ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
                            object valueObject = serializer.ReadObject(xmlReader);
                            return valueObject;
                        }
                    }
                    return value;
                }
                if (previouslyConvertedValue != null)
                {
                    if (!conversionType.IsAssignableFrom(previouslyConvertedValue.GetType()))
                        throw new InvalidCastException("Type of oldValue is different to conversionType");
                }

                if (conversionType.IsAssignableFrom(value.GetType()))
                {
                    return value;
                }
                else if (typeof(EntityObject).IsAssignableFrom(conversionType) && value.GetType() == typeof(string))
                {
                    string valueString = value as string;
                    using (ACMonitor.Lock(database.QueryLock_1X000))
                    {
                        if (valueString.StartsWith(Const.ContextDatabase + "\\"))
                            return database.ACUrlCommand(valueString.Substring(9));
                        else
                            return database.ACUrlCommand(valueString);
                    }
                }
                // Falls zu konvertierender Wert ein TimeSpan ist
                else if (value is TimeSpan)
                {
                    // Umwandlung von TimeSpan nach String
                    if (conversionType == typeof(string))
                        return ((TimeSpan)(object)value).ToString("c");
                    // Umwandlung von TimeSpan nach Ticks
                    else if (conversionType == typeof(long))
                        return ((TimeSpan)(object)value).Ticks;
                    // Umwandlung von TimeSpan nach IConvertible
                    else if (typeof(IConvertible).IsAssignableFrom(conversionType))
                    {
                        if (invariantCulture)
                            return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
                        else
                            return Convert.ChangeType(value, conversionType);
                    }
                    else
                        throw new InvalidCastException("Not convertable");
                }
                // Falls zu konvertierender Wert ein DateTime ist
                else if (value is DateTime)
                {
                    // Umwandlung von DateTime nach String
                    if (conversionType == typeof(string))
                    {
                        DateTime utcDate = ((DateTime)value).ToUniversalTime();
                        if (invariantCulture)
                        {
                            return utcDate.ToString("o", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            return utcDate.ToString("o");
                        }
                    }
                    // Umwandlung von DateTime nach IConvertible
                    else if (typeof(IConvertible).IsAssignableFrom(conversionType))
                    {
                        if (invariantCulture)
                            return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
                        else
                            return Convert.ChangeType(value, conversionType);
                    }
                    else
                        throw new InvalidCastException("Not convertable");
                }
                // Falls value ein primitiver Typ ist der IConvertible implementiert z.B. String, int, ....
                else if (value is IConvertible)
                {
                    Type innerType = conversionType;
                    Type genericNullableType = null;
                    if (conversionType.IsGenericType)
                    {
                        innerType = conversionType.GetGenericArguments()[0];
                        genericNullableType = conversionType.GetGenericTypeDefinition();
                        if (!genericNullableType.IsAssignableFrom(typeof(Nullable<>)))
                        {
                            genericNullableType = null;
                            innerType = conversionType;
                        }
                    }
                    // Umwandlung von String/IConvertible nach TimeSpan
                    if (typeof(TimeSpan).IsAssignableFrom(innerType))
                    {
                        if (value is string)
                        {
                            TimeSpan result;
                            if (!TimeSpan.TryParseExact(value as string, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None, out result))
                            {
                                if (genericNullableType != null)
                                    return new Nullable<TimeSpan>(new TimeSpan());
                                return new TimeSpan();
                            }
                            if (genericNullableType != null)
                                return new Nullable<TimeSpan>(result);
                            return result;
                        }
                        else if (value is long)
                        {
                            TimeSpan convValue = TimeSpan.FromTicks((long)value);
                            if (genericNullableType != null)
                                return new Nullable<TimeSpan>(convValue);
                            return convValue;
                        }
                        else if (value is TimeSpanStyles)
                        {
                            if (genericNullableType != null)
                                return new Nullable<TimeSpan>(new TimeSpan());
                            return new TimeSpan();
                        }
                        else
                        {
                            TimeSpan convValue = TimeSpan.ParseExact((value as IConvertible).ToString(), "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
                            if (genericNullableType != null)
                                return new Nullable<TimeSpan>(convValue);
                            return convValue;
                        }
                    }
                    // Umwandlung von String/IConvertible nach DateTime
                    else if (typeof(DateTime).IsAssignableFrom(innerType))
                    {
                        if (value is string)
                        {
                            DateTime convValue = DateTime.MinValue;
                            try
                            {
                                convValue = DateTime.ParseExact(value as string, "o", CultureInfo.InvariantCulture, DateTimeStyles.None);
                            }
                            catch (Exception e)
                            {
                                convValue = DateTime.Parse(value as string);

                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (Database.Root != null && Database.Root.Messages != null)
                                    Database.Root.Messages.LogException("ACConvert", "ChangeType", msg);
                            }
                            if (genericNullableType != null)
                                return new Nullable<DateTime>(convValue);
                            return convValue;
                        }
                        else
                        {
                            DateTime convValue;
                            if (invariantCulture)
                                convValue = (DateTime)Convert.ChangeType(value, innerType, CultureInfo.InvariantCulture);
                            else
                                convValue = (DateTime)Convert.ChangeType(value, innerType);

                            if (genericNullableType != null)
                                return new Nullable<DateTime>(convValue);
                            return convValue;
                        }
                    }
                    else if (typeof(Guid).IsAssignableFrom(innerType))
                    {
                        string valueString = "";
                        if (value is string)
                            valueString = value as string;
                        if (!String.IsNullOrEmpty(valueString))
                        {
                            try
                            {
                                if (valueString.IndexOf("guid") < 0)
                                    return new Guid(valueString);
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (Database.Root != null && Database.Root.Messages != null)
                                    Database.Root.Messages.LogException("ACConvert", "ChangeType(10)", msg);
                            }
                            if (ACKnownTypes.IsKnownType(conversionType))
                            {
                                using (StringReader ms = new StringReader(valueString))
                                using (XmlTextReader xmlReader = new XmlTextReader(ms))
                                {
                                    DataContractSerializer serializer = new DataContractSerializer(conversionType, ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
                                    object valueObject = serializer.ReadObject(xmlReader);
                                    return valueObject;
                                }
                            }
                            else
                                return Guid.Empty;
                        }
                        else
                            return Guid.Empty;
                    }
                    // Umwandlung von String/IConvertible nach IConvertible
                    else if (typeof(IConvertible).IsAssignableFrom(innerType))
                    {
                        if (typeof(IBitAccessBase).IsAssignableFrom(innerType))
                        {
                            if (previouslyConvertedValue == null)
                            {
                                if (conversionACType != null)
                                    previouslyConvertedValue = Activator.CreateInstance(innerType, conversionACType) as IBitAccessBase;
                                else
                                    return null;
                            }
                            if (previouslyConvertedValue != null)
                            {
                                if (value is String)
                                    (previouslyConvertedValue as IBitAccessBase).SetFromString((String)value);
                                else
                                    (previouslyConvertedValue as IBitAccessBase).SetFromString((value as IConvertible).ToString());
                            }
                            return previouslyConvertedValue;
                        }
                        else if (value is string && innerType.IsEnum)
                        {
                            object convValue = Enum.Parse(innerType, (String)value);
                            if (genericNullableType != null)
                            {
                                object acRefInstance = Activator.CreateInstance(conversionType, new Object[] { convValue });
                                return acRefInstance;
                            }
                            return convValue;
                        }
                        else if (innerType.IsEnum && value is IConvertible)
                        {
                            object convValue = Enum.ToObject(innerType, value);
                            if (genericNullableType != null)
                            {
                                object acRefInstance = Activator.CreateInstance(conversionType, new Object[] { convValue });
                                return acRefInstance;
                            }
                            return convValue;
                        }
                        else
                        {
                            object convValue = null;
                            if (invariantCulture)
                            {
                                if (value is string && (((string)value) == "n. def." || String.IsNullOrEmpty((string)value)))
                                {
                                    if (genericNullableType == null)
                                    {
                                        if (innerType == typeof(bool))
                                            convValue = false;
                                        else
                                            convValue = Convert.ChangeType("0", innerType, CultureInfo.InvariantCulture);
                                    }
                                    else
                                        convValue = null;
                                }
                                else
                                    convValue = Convert.ChangeType(value, innerType, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                if (value is string && (((string)value) == "n. def." || String.IsNullOrEmpty((string)value)))
                                {
                                    if (genericNullableType == null)
                                    {
                                        if (innerType == typeof(bool))
                                            convValue = false;
                                        else
                                            convValue = Convert.ChangeType("0", innerType);
                                    }
                                    else
                                        convValue = null;
                                }
                                else
                                    convValue = Convert.ChangeType(value, innerType);
                            }
                            if (genericNullableType != null && convValue != null)
                            {
                                object acRefInstance = Activator.CreateInstance(conversionType, new Object[] { convValue });
                                return acRefInstance;
                            }
                            return convValue;
                        }
                    }
                    // Umwandlung von String/IConvertible nach Entity oder Deserialisierbaren Typen
                    else
                    {
                        string valueString = "";
                        if (value is string)
                            valueString = value as string;
                        else
                        {
                            if (invariantCulture)
                                valueString = (value as IConvertible).ToString(CultureInfo.InvariantCulture);
                            else
                                valueString = (value as IConvertible).ToString();
                        }
                        if (String.IsNullOrEmpty(valueString))
                            return null;
                        if (database == null)
                            database = Database.GlobalDatabase;
                        if (ACKnownTypes.IsKnownType(conversionType))
                        {
                            object valueObject = null;
                            using (StringReader ms = new StringReader(valueString))
                            using (XmlTextReader xmlReader = new XmlTextReader(ms))
                            {
                                DataContractSerializer serializer = new DataContractSerializer(conversionType, ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
                                valueObject = serializer.ReadObject(xmlReader);
                            }

                            if (valueObject is ACMethod)
                            {
                                // Clone ACMethod, because Enumerator-Reference will not be Disposed from DataContractSerializer (Bug) therefore the ReadLock will remain
                                ACMethod acMethod = valueObject as ACMethod;
                                //if (acMethod.ParameterValueList is ISafeList)
                                    valueObject = acMethod.Clone();
                            }

                            // Attach is implemented Thread-Safe on accessing object-Context
                            if (valueObject is IACAttach)
                                (valueObject as IACAttach).AttachTo(database);
                            return valueObject;
                        }
                        else if (conversionType == typeof(ACQueryDefinition))
                        {
                            // TODO: var o = ACSerializer.Deserialize(value as IACObject, Database._QRYACQueryDefinition, value);
                            return null;
                        }
                        throw new InvalidCastException("Not convertable");
                    }
                }
                // Falls zu konvertierender Wert ein EntityKey ist
                else if (value is EntityKey)
                {
                    // Hole EntityObject über EntityKey
                    if (typeof(EntityObject).IsAssignableFrom(conversionType))
                    {
                        object result = null;
                        if (database == null)
                            database = Database.GlobalDatabase;
                        EntityKey _EntityKey = value as EntityKey;
                        if (_EntityKey != null)
                        {
                            try
                            {

                                using (ACMonitor.Lock(database.QueryLock_1X000))
                                {
                                    result = database.GetObjectByKey(_EntityKey);
                                }
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (Database.Root != null && Database.Root.Messages != null)
                                    Database.Root.Messages.LogException("ACConvert", "ChangeType(20)", msg);
                            }
                        }
                        return result;
                    }
                    // Serialisiere Entity-Key nach String
                    else if (typeof(string).IsAssignableFrom(conversionType))
                    {
                        return SerializeObject(value.GetType(), value, xmlIndented);
                    }
                    else
                        throw new InvalidCastException("EntityKey not convertable to " + conversionType.Name);
                }
                // Falls zu konvertierender Wert ein EntityObject ist
                else if (value is EntityObject)
                {
                    // Hole EntityObject nach entityKey
                    if (typeof(EntityKey).IsAssignableFrom(conversionType))
                        return (value as EntityObject).EntityKey;
                    // Serialisiere EntityObject nach String
                    else if (typeof(string).IsAssignableFrom(conversionType))
                    {
                        if (!entityAsEntityKey && (value is IACObject))
                            return (value as IACObject).GetACUrl();
                        else
                        {
                            if ((value as EntityObject).EntityState == EntityState.Detached)
                                return null;
                            return SerializeObject((value as EntityObject).EntityKey.GetType(), (value as EntityObject).EntityKey, xmlIndented);
                        }
                    }
                    else if (!conversionType.IsAssignableFrom(value.GetType()))
                        throw new InvalidCastException("EntityObject not convertable to " + conversionType.Name);
                    return value;
                }

                // Falls zu konvertierender Wert ein bekanntes Serialisierbares Objekt ist
                else if (ACKnownTypes.IsKnownType(value.GetType()))
                {
                    if (typeof(string).IsAssignableFrom(conversionType))
                        return SerializeObject(value.GetType(), value, xmlIndented);
                    else
                        throw new InvalidCastException(String.Format("ACKnownType {0} only convertable to string", value.GetType().FullName));
                }
                // Falls ACQueryDefinition
                else if (value is ACQueryDefinition)
                {
                    if (typeof(string).IsAssignableFrom(conversionType))
                        return ACSerializer.Serialize(value as IACObject, Database._QRYACQueryDefinition);
                    else
                        throw new InvalidCastException("ACQueryDefinition only convertable to string");
                }
                else if (conversionType == typeof(string))
                {
                    return value.ToString();
                }
                else if (conversionType.IsAssignableFrom(value.GetType()))
                    return value;

                throw new InvalidCastException("Not supported Type");
            }
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="valueObject">The value object.</param>
        /// <param name="xmlIndented">if set to <c>true</c> [XML indented].</param>
        /// <returns>System.String.</returns>
        static private string SerializeObject(Type objectType, object valueObject, bool xmlIndented = false)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
            {
                if (xmlIndented)
                    xmlWriter.Formatting = Formatting.Indented;
                DataContractSerializer serializer = new DataContractSerializer(objectType, ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
                if (valueObject is ACMethod)
                {
                    // Clone ACMethod, because Enumerator-Reference will not be Disposed from DataContractSerializer (Bug) therefore the ReadLock will remain
                    ACMethod acMethod = valueObject as ACMethod;
                    valueObject = acMethod.Clone();
                }
                serializer.WriteObject(xmlWriter, valueObject);
                return sw.ToString();
            }
        }
        #endregion

#else
        /// <summary>
        /// Gibt ein Objekt vom angegebenen Typ zurück, dessen Wert dem angegebenen Objekt entspricht.
        /// </summary>
        /// <param name="value">Ein TimeSpan-, oder DateTime-, oder EntityKey-, oder EntityObject-, oder ACQueryDefinition-Objekt
        /// oder ein Objekt, das die System.IConvertible-Schnittstelle implementiert
        /// oder ein serialisierbares ACKnownTypes-Objekt</param>
        /// <param name="conversionType">Der System.Type des zurückzugebenden Objekts.</param>
        /// <param name="invariantCulture">invariantCulture</param>
        /// <returns>Ein Objekt, dessen Typ gleich conversionType ist und dessen Wert value entspricht.–
        /// oder –Ein NULL-Verweis (Nothing in Visual Basic), wenn valuenull ist und
        /// conversionType kein Werttyp ist.</returns>
        /// <exception cref="System.InvalidCastException">Diese Konvertierung wird nicht unterstützt. – oder –value ist null, und conversionType
        /// ist ein Werttyp.– oder –value implementiert die System.IConvertible-Schnittstelle
        /// nicht.</exception>
        /// <exception cref="System.FormatException">value weist kein von conversionType erkanntes Format auf.</exception>
        /// <exception cref="System.OverflowException">value stellt eine Zahl dar, die außerhalb des Bereichs von conversionType liegt.</exception>
        /// <exception cref="System.ArgumentNullException">conversionType ist null</exception>
        public static object ChangeType(object value, Type conversionType, bool invariantCulture)
        {
            // TODO: IACType Conversion
            if (conversionType == null)
                throw new ArgumentNullException("conversionType ist null");
            if (value == null)
            {

                // Konvertiere in ein TimeSpan
                if (typeof(TimeSpan).IsAssignableFrom(conversionType))
                {
                    return TimeSpan.Zero;
                }
               
                // Konvertiere in ein DateTime
                else if (typeof(DateTime).IsAssignableFrom(conversionType))
                {
                    return DateTime.MinValue;
                }
                // Konvertiere in eine primitiver Typ ist der IConvertible implementiert z.B. String, int, ....
                else if (typeof(IConvertible).IsAssignableFrom(conversionType))
                {
                    return  conversionType.GetDefault();
                    //return conversionType.IsValueType ? (!conversionType.IsGenericType ? Activator.CreateInstance(conversionType) : conversionType.GenericTypeArguments[0].GetDefault()) : null;
                    //if (invariantCulture)
                    //    return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
                    //else
                    //    return Convert.ChangeType(value, conversionType);
                }
                else if (conversionType.Name == Const.TNameNullable)
                {
                    return null;
                }
                else if (!conversionType.IsValueType)
                {
                    return null;
                }

                throw new InvalidCastException("Not supported Type");
            }
            else
            {
                if (conversionType.IsAssignableFrom(value.GetType()))
                {
                    return value;
                }
                // Falls zu konvertierender Wert ein TimeSpan ist
                else if (value is TimeSpan)
                {
                    // Umwandlung von TimeSpan nach String
                    if (conversionType == typeof(string))
                        return ((TimeSpan)(object)value).ToString("c");
                    // Umwandlung von TimeSpan nach Ticks
                    else if (conversionType == typeof(long))
                        return ((TimeSpan)(object)value).Ticks;
                    // Umwandlung von TimeSpan nach IConvertible
                    else if (typeof(IConvertible).IsAssignableFrom(conversionType))
                    {
                        if (invariantCulture)
                            return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
                        else
                            return Convert.ChangeType(value, conversionType);
                    }
                    else
                        throw new InvalidCastException("Not convertable");
                }
                // Falls zu konvertierender Wert ein DateTime ist
                else if (value is DateTime)
                {
                    // Umwandlung von DateTime nach String
                    if (conversionType == typeof(string))
                    {
                        DateTime utcDate = ((DateTime)value).ToUniversalTime();
                        if (invariantCulture)
                        {
                            return utcDate.ToString("o", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            return utcDate.ToString("o");
                        }
                    }
                    // Umwandlung von DateTime nach IConvertible
                    else if (typeof(IConvertible).IsAssignableFrom(conversionType))
                    {
                        if (invariantCulture)
                            return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
                        else
                            return Convert.ChangeType(value, conversionType);
                    }
                    else
                        throw new InvalidCastException("Not convertable");
                }
                // Falls value ein primitiver Typ ist der IConvertible implementiert z.B. String, int, ....
                else if (value is IConvertible)
                {
                    Type innerType = conversionType;
                    Type genericNullableType = null;
                    if (conversionType.IsGenericType)
                    {
                        innerType = conversionType.GetGenericArguments()[0];
                        genericNullableType = conversionType.GetGenericTypeDefinition();
                        if (!genericNullableType.IsAssignableFrom(typeof(Nullable<>)))
                        {
                            genericNullableType = null;
                            innerType = conversionType;
                        }
                    }
                    // Umwandlung von String/IConvertible nach TimeSpan
                    if (typeof(TimeSpan).IsAssignableFrom(innerType))
                    {
                        if (value is string)
                        {
                            TimeSpan result;
                            if (!TimeSpan.TryParseExact(value as string, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None, out result))
                            {
                                if (genericNullableType != null)
                                    return new Nullable<TimeSpan>(new TimeSpan());
                                return new TimeSpan();
                            }
                            if (genericNullableType != null)
                                return new Nullable<TimeSpan>(result);
                            return result;
                        }
                        else if (value is long)
                        {
                            TimeSpan convValue = TimeSpan.FromTicks((long)value);
                            if (genericNullableType != null)
                                return new Nullable<TimeSpan>(convValue);
                            return convValue;
                        }
                        else if (value is TimeSpanStyles)
                        {
                            if (genericNullableType != null)
                                return new Nullable<TimeSpan>(new TimeSpan());
                            return new TimeSpan();
                        }
                        else
                        {
                            TimeSpan convValue = TimeSpan.ParseExact((value as IConvertible).ToString(), "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
                            if (genericNullableType != null)
                                return new Nullable<TimeSpan>(convValue);
                            return convValue;
                        }
                    }
                    // Umwandlung von String/IConvertible nach DateTime
                    else if (typeof(DateTime).IsAssignableFrom(innerType))
                    {
                        if (value is string)
                        {
                            DateTime convValue = DateTime.MinValue;
                            try
                            {
                                convValue = DateTime.ParseExact(value as string, "o", CultureInfo.InvariantCulture, DateTimeStyles.None);
                            }
                            catch (Exception e)
                            {
                                convValue = DateTime.Parse(value as string);

                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;
                            }
                            if (genericNullableType != null)
                                return new Nullable<DateTime>(convValue);
                            return convValue;
                        }
                        else
                        {
                            DateTime convValue;
                            if (invariantCulture)
                                convValue = (DateTime)Convert.ChangeType(value, innerType, CultureInfo.InvariantCulture);
                            else
                                convValue = (DateTime)Convert.ChangeType(value, innerType);

                            if (genericNullableType != null)
                                return new Nullable<DateTime>(convValue);
                            return convValue;
                        }
                    }
                    else if (typeof(Guid).IsAssignableFrom(innerType))
                    {
                        string valueString = "";
                        if (value is string)
                            valueString = value as string;
                        if (!String.IsNullOrEmpty(valueString))
                        {
                            try
                            {
                                if (valueString.IndexOf("guid") < 0)
                                    return new Guid(valueString);
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;
                            }
                            return Guid.Empty;
                        }
                        else
                            return Guid.Empty;
                    }
                    // Umwandlung von String/IConvertible nach IConvertible
                    else if (typeof(IConvertible).IsAssignableFrom(innerType))
                    {
                        if (value is string && innerType.IsEnum)
                        {
                            object convValue = Enum.Parse(innerType, (String)value);
                            if (genericNullableType != null)
                            {
                                object acRefInstance = Activator.CreateInstance(conversionType, new Object[] { convValue });
                                return acRefInstance;
                            }
                            return convValue;
                        }
                        else if (innerType.IsEnum && value is IConvertible)
                        {
                            object convValue = Enum.ToObject(innerType, value);
                            if (genericNullableType != null)
                            {
                                object acRefInstance = Activator.CreateInstance(conversionType, new Object[] { convValue });
                                return acRefInstance;
                            }
                            return convValue;
                        }
                        else
                        {
                            object convValue = null;
                            if (invariantCulture)
                            {
                                if (value is string && (((string)value) == "n. def." || String.IsNullOrEmpty((string)value)))
                                {
                                    if (genericNullableType == null)
                                    {
                                        if (innerType == typeof(bool))
                                            convValue = false;
                                        else
                                            convValue = Convert.ChangeType("0", innerType, CultureInfo.InvariantCulture);
                                    }
                                    else
                                        convValue = null;
                                }
                                else
                                    convValue = Convert.ChangeType(value, innerType, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                if (value is string && (((string)value) == "n. def." || String.IsNullOrEmpty((string)value)))
                                {
                                    if (genericNullableType == null)
                                    {
                                        if (innerType == typeof(bool))
                                            convValue = false;
                                        else
                                            convValue = Convert.ChangeType("0", innerType);
                                    }
                                    else
                                        convValue = null;
                                }
                                else
                                    convValue = Convert.ChangeType(value, innerType);
                            }
                            if (genericNullableType != null && convValue != null)
                            {
                                object acRefInstance = Activator.CreateInstance(conversionType, new Object[] { convValue });
                                return acRefInstance;
                            }
                            return convValue;
                        }
                    }
                    // Umwandlung von String/IConvertible nach Entity oder Deserialisierbaren Typen
                    else
                    {
                        string valueString = "";
                        if (value is string)
                            valueString = value as string;
                        else
                        {
                            if (invariantCulture)
                                valueString = (value as IConvertible).ToString(CultureInfo.InvariantCulture);
                            else
                                valueString = (value as IConvertible).ToString();
                        }
                        if (String.IsNullOrEmpty(valueString))
                            return null;
                        throw new InvalidCastException("Not convertable");
                    }
                }
                else if (conversionType == typeof(string))
                {
                    return value.ToString();
                }
                else if (conversionType.IsAssignableFrom(value.GetType()))
                    return value;

                throw new InvalidCastException("Not supported Type");
            }
        }

#endif
        public static object GetDefault(this Type type)
        {
            return type.IsValueType ? (!type.IsGenericType ? Activator.CreateInstance(type) : type.GenericTypeArguments[0].GetDefault()) : null;
        }

    }
}
