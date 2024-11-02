// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.ObjectModel;
using System.Threading;
using System.Reflection;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>
    /// An identifier for an entity.
    /// </summary>
    [DebuggerDisplay("{ConcatKeyValue()}")]
    [DataContract(IsReference = true)]
    [NotMapped]
    public sealed class EntityKey : IEquatable<EntityKey>
    {
        // The implementation of EntityKey is optimized for the following common cases:
        //      1) Keys constructed internally rather by the user - in particular, keys 
        //         created by the bridge on the round-trip from query.
        //      2) Single-valued (as opposed to composite) keys.
        // We accomplish this by maintaining two variables, at most one of which is non-null.
        // The first is of type object and in the case of a singleton key, is set to the
        // single key value.  The second is an object array and in the case of 
        // a composite key, is set to the list of key values.  If both variables are null,
        // the EntityKey is a temporary key.  Note that the key field names
        // are not stored - for composite keys, the values are stored in the order in which
        // metadata reports the corresponding key members.

        // The following 5 fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        private string _entitySetName;
        private string _entityContainerName;
        private object _singletonKeyValue;      // non-null for singleton keys
        private object[] _compositeKeyValues;   // non-null for composite keys
        private string[] _keyNames;             // key names that correspond to the key values
        private bool _isLocked;                 // determines if this key is lock from writing

        // Determines whether the key includes a byte[].
        // Not serialized for backwards compatibility.
        // This value is computed along with the _hashCode, which is also not serialized.
        [NonSerialized]
        private bool _containsByteArray;

        [NonSerialized]
        private EntityKeyMember[] _deserializedMembers;

        // The hash code is not serialized since it can be computed differently on the deserialized system.
        [NonSerialized]
        private int _hashCode;                  // computed as needed


        // Names for constant EntityKeys
        private const string s_NoEntitySetKey = "NoEntitySetKey.NoEntitySetKey";
        private const string s_EntityNotValidKey = "EntityNotValidKey.EntityNotValidKey";

        /// <summary>
        /// A singleton EntityKey by which a read-only entity is identified.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]     // Justification: these are internal so they cannot be modified publically
        public static readonly EntityKey NoEntitySetKey = new EntityKey(s_NoEntitySetKey);

        /// <summary>
        /// A singleton EntityKey identifying an entity resulted from a failed TREAT.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]     // Justification: these are internal so they cannot be modified publically
        public static readonly EntityKey EntityNotValidKey = new EntityKey(s_EntityNotValidKey);

        /// <summary>
        /// A dictionary of names so that singleton instances of names can be used
        /// </summary>
        private static Dictionary<string, string> _nameLookup = new Dictionary<string, string>();

        #region Public Constructors

        /// <summary>
        /// Constructs an empty EntityKey. For use during XmlSerialization.
        /// </summary>
        public EntityKey()
        {
            _isLocked = false;
        }

        public EntityKey(Type typeOfEntity, IEnumerable<KeyValuePair<string, object>> entityKeyValues)
            : this(typeOfEntity.AssemblyQualifiedName, entityKeyValues)
        {
        }

        /// <summary>
        /// Constructs an EntityKey with the given key values.
        /// </summary>
        /// <param name="qualifiedEntitySetName">The EntitySet name, qualified by the EntityContainer name, of the entity</param>
        /// <param name="entityKeyValues">The key-value pairs that identify the entity</param>
        public EntityKey(string qualifiedEntitySetName, IEnumerable<KeyValuePair<string, object>> entityKeyValues)
        {
            GetEntitySetName(qualifiedEntitySetName, out _entitySetName, out _entityContainerName);
            _entityContainerName = qualifiedEntitySetName;
            CheckKeyValues(entityKeyValues, out _keyNames, out _singletonKeyValue, out _compositeKeyValues);
            AssertCorrectState(null, false);
            _isLocked = true;
        }

        public EntityKey(Type typeOfEntity, IEnumerable<EntityKeyMember> entityKeyValues)
            : this(typeOfEntity.AssemblyQualifiedName, entityKeyValues)
        {
        }

        /// <summary>
        /// Constructs an EntityKey with the given key values.
        /// </summary>
        /// <param name="qualifiedEntitySetName">The EntitySet name, qualified by the EntityContainer name, of the entity</param>
        /// <param name="entityKeyValues">The key-value pairs that identify the entity</param>
        public EntityKey(string qualifiedEntitySetName, IEnumerable<EntityKeyMember> entityKeyValues)
        {
            GetEntitySetName(qualifiedEntitySetName, out _entitySetName, out _entityContainerName);
            _entityContainerName = qualifiedEntitySetName;
            EntityUtil.CheckArgumentNull(entityKeyValues, "entityKeyValues");
            CheckKeyValues(new KeyValueReader(entityKeyValues), out _keyNames, out _singletonKeyValue, out _compositeKeyValues);
            AssertCorrectState(null, false);
            _isLocked = true;
        }

        /// <summary>
        /// Constructs an EntityKey with the given single key name and value.
        /// </summary>
        /// <param name="qualifiedEntitySetName">The EntitySet name, qualified by the EntityContainer name, of the entity</param>
        /// <param name="keyName">The key name that identifies the entity</param>
        /// <param name="keyValue">The key value that identifies the entity</param>
        public EntityKey(string qualifiedEntitySetName, string keyName, object keyValue)
        {
            GetEntitySetName(qualifiedEntitySetName, out _entitySetName, out _entityContainerName);
            _entityContainerName = qualifiedEntitySetName;
            EntityUtil.CheckStringArgument(keyName, "keyName");
            EntityUtil.CheckArgumentNull(keyValue, "keyValue");

            _keyNames = new string[1];
            _keyNames[0] = keyName;
            _singletonKeyValue = keyValue;

            AssertCorrectState(null, false);
            _isLocked = true;
        }

        #endregion

        #region Internal Constructors

        /// <summary>
        /// Constructs an EntityKey from an IExtendedDataRecord representing the entity.
        /// </summary>
        /// <param name="entitySet">EntitySet of the entity</param>
        internal EntityKey(EntitySet entitySet)
        {
            Debug.Assert(entitySet != null, "entitySet is null");

            _entitySetName = entitySet.Name;

            AssertCorrectState(entitySet, false);
            _isLocked = true;
        }

        /// <summary>
        /// Constructs an EntityKey from an IExtendedDataRecord representing the entity.
        /// </summary>
        /// <param name="qualifiedEntitySetName">EntitySet of the entity</param>
        internal EntityKey(string qualifiedEntitySetName)
        {
            GetEntitySetName(qualifiedEntitySetName, out _entitySetName, out _entityContainerName);
            _entityContainerName = qualifiedEntitySetName;
            _isLocked = true;
        }

        #endregion

        /// <summary>
        /// Gets the EntitySet name identifying the entity set that contains the entity.
        /// </summary>
        [DataMember]
        public string EntitySetName
        {
            get
            {
                return _entitySetName;
            }
            set
            {
                ValidateWritable(_entitySetName);
                lock (_nameLookup)
                {
                    _entitySetName = EntityKey.LookupSingletonName(value);
                }
            }
        }

        /// <summary>
        /// Gets the EntityContainer name identifying the entity container that contains the entity.
        /// </summary>
        [DataMember]
        public string EntityContainerName
        {
            get
            {
                return _entityContainerName;
            }
            set
            {
                ValidateWritable(_entityContainerName);
                lock (_nameLookup)
                {
                    if (value != null && value.Contains("Version") && value.Contains("Culture") && value.Contains("PublicKeyToken"))
                    {
                        _entityContainerName = EntityKey.LookupSingletonName(value);
                    }

                    else
                    {
                        if (EntityContainerName != null && EntityContainerName.Contains("Version") && EntityContainerName.Contains("Culture") && EntityContainerName.Contains("PublicKeyToken"))
                        {
                            _entityContainerName = EntityContainerName;
                        }

                        else
                        {
                            if (EntityContainerName == "iPlusMESV5_Entities")
                            {
                                _entityContainerName = "gip.mes.datamodel." + EntitySetName + ", gip.mes.datamodel, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = 12adb6357a02d860";
                            }

                            else if (EntityContainerName == "iPlusV5_Entities")
                            {
                                _entityContainerName = "gip.core.datamodel." + EntitySetName + ", gip.core.datamodel, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = adb6357a02d860";
                            }

                            else
                            {
                                _entityContainerName = "gip.core.datamodel." + EntitySetName + ", gip.core.datamodel, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = adb6357a02d860";
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets the key values that identify the entity.
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Required for this feature")]
        public EntityKeyMember[] EntityKeyValues
        {
            get
            {
                if (!IsTemporary)
                {
                    EntityKeyMember[] keyValues;
                    if (_singletonKeyValue != null)
                    {
                        keyValues = new EntityKeyMember[] {
                                new EntityKeyMember(_keyNames[0], _singletonKeyValue) };
                    }
                    else
                    {
                        keyValues = new EntityKeyMember[_compositeKeyValues.Length];
                        for (int i = 0; i < _compositeKeyValues.Length; ++i)
                        {
                            keyValues[i] = new EntityKeyMember(_keyNames[i], _compositeKeyValues[i]);
                        }
                    }
                    return keyValues;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                ValidateWritable(_keyNames);
                if (value != null)
                {
                    if (!CheckKeyValues(new KeyValueReader(value), true, true, out _keyNames, out _singletonKeyValue, out _compositeKeyValues))
                    {
                        // If we did not retrieve values from the setter (i.e. encoded settings), we need to keep track of the 
                        // array instance because the array members will be set next.
                        _deserializedMembers = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this key is a temporary key.
        /// </summary>
        public bool IsTemporary
        {
            get
            {
                return (SingletonKeyValue == null) && (CompositeKeyValues == null);
            }
        }

        private object SingletonKeyValue
        {
            get
            {
                if (RequiresDeserialization)
                {
                    DeserializeMembers();
                }
                return _singletonKeyValue;
            }
        }

        private object[] CompositeKeyValues
        {
            get
            {
                if (RequiresDeserialization)
                {
                    DeserializeMembers();
                }
                return _compositeKeyValues;
            }
        }

        #region Equality/Hashing

        /// <summary>
        /// Compares this instance to a given key by their values.
        /// </summary>
        /// <param name="obj">the key to compare against this instance</param>
        /// <returns>true if this instance is equal to the given key, and false otherwise</returns>
        public override bool Equals(object obj)
        {
            return InternalEquals(this, obj as EntityKey, compareEntitySets: true);
        }

        /// <summary>
        /// Compares this instance to a given key by their values.
        /// </summary>
        /// <param name="other">the key to compare against this instance</param>
        /// <returns>true if this instance is equal to the given key, and false otherwise</returns>
        public bool Equals(EntityKey other)
        {
            return InternalEquals(this, other, compareEntitySets: true);
        }

        /// <summary>
        /// Internal function to compare two keys by their values.
        /// </summary>
        /// <param name="key1">a key to compare</param>
        /// <param name="key2">a key to compare</param>
        /// <param name="compareEntitySets">Entity sets are not significant for conceptual null keys</param>
        /// <returns>true if the two keys are equal, false otherwise</returns>
        internal static bool InternalEquals(EntityKey key1, EntityKey key2, bool compareEntitySets)
        {
            // If both are null or refer to the same object, they're equal.
            if (object.ReferenceEquals(key1, key2))
            {
                return true;
            }

            // If exactly one is null (avoid calling EntityKey == operator overload), they're not equal.
            if (object.ReferenceEquals(key1, null) || object.ReferenceEquals(key2, null))
            {
                return false;
            }

            // If the hash codes differ, the keys are not equal.  Note that 
            // a key's hash code is cached after being computed for the first time, 
            // so this check will only incur the cost of computing a hash code 
            // at most once for a given key.

            // The primary caller is Dictionary<EntityKey,ObjectStateEntry>
            // at which point Equals is only called after HashCode was determined to be equal
            if ((key1.GetHashCode() != key2.GetHashCode() && compareEntitySets) ||
                key1._containsByteArray != key2._containsByteArray)
            {
                return false;
            }

            if (null != key1._singletonKeyValue)
            {
                if (key1._containsByteArray)
                {
                    // Compare the single value (if the second is null, false should be returned)
                    if (null == key2._singletonKeyValue)
                    {
                        return false;
                    }

                    // they are both byte[] because they have the same _containsByteArray value of true, and only a single value
                    if (!ByValueEqualityComparer.CompareBinaryValues((byte[])key1._singletonKeyValue, (byte[])key2._singletonKeyValue))
                    {
                        return false;
                    }
                }
                else
                {
                    // not a byte array
                    if (!key1._singletonKeyValue.Equals(key2._singletonKeyValue))
                    {
                        return false;
                    }
                }

                // Check key names
                if (!String.Equals(key1._keyNames[0], key2._keyNames[0]))
                {
                    return false;
                }
            }
            else
            {
                // If either key is temporary, they're not equal.  This is because
                // temporary keys are compared by CLR reference, and we've already
                // checked reference equality.
                // If the first key is a composite key and the second one isn't, they're not equal.
                if (null != key1._compositeKeyValues && null != key2._compositeKeyValues && key1._compositeKeyValues.Length == key2._compositeKeyValues.Length)
                {
                    if (key1._containsByteArray)
                    {
                        if (!CompositeValuesWithBinaryEqual(key1, key2))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!CompositeValuesEqual(key1, key2))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            if (compareEntitySets)
            {
                // Check metadata.
                if (!String.Equals(key1._entitySetName, key2._entitySetName) ||
                    !String.Equals(key1._entityContainerName, key2._entityContainerName))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool CompositeValuesWithBinaryEqual(EntityKey key1, EntityKey key2)
        {
            for (int i = 0; i < key1._compositeKeyValues.Length; ++i)
            {
                if (key1._keyNames[i].Equals(key2._keyNames[i]))
                {
                    if (!ByValueEqualityComparer.Default.Equals(key1._compositeKeyValues[i], key2._compositeKeyValues[i]))
                    {
                        return false;
                    }
                }
                // Key names might not be in the same order so try a slower approach that matches
                // key names between the keys.
                else if (!ValuesWithBinaryEqual(key1._keyNames[i], key1._compositeKeyValues[i], key2))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ValuesWithBinaryEqual(string keyName, object keyValue, EntityKey key2)
        {
            for (int i = 0; i < key2._keyNames.Length; i++)
            {
                if (String.Equals(keyName, key2._keyNames[i]))
                {
                    return ByValueEqualityComparer.Default.Equals(keyValue, key2._compositeKeyValues[i]);
                }
            }
            return false;
        }

        private static bool CheckKeyValues(IEnumerable<KeyValuePair<string, object>> entityKeyValues,
            out string[] keyNames, out object singletonKeyValue, out object[] compositeKeyValues)
        {
            return CheckKeyValues(entityKeyValues, false, false, out keyNames, out singletonKeyValue, out compositeKeyValues);
        }

        private static bool CheckKeyValues(IEnumerable<KeyValuePair<string, object>> entityKeyValues, bool allowNullKeys, bool tokenizeStrings,
            out string[] keyNames, out object singletonKeyValue, out object[] compositeKeyValues)
        {

            int numExpectedKeyValues;
            int numActualKeyValues = 0;

            keyNames = null;
            singletonKeyValue = null;
            compositeKeyValues = null;

            // Determine if we're a single or composite key.
            foreach (KeyValuePair<string, object> value in entityKeyValues)
            {
                numActualKeyValues++;
            }

            numExpectedKeyValues = numActualKeyValues;
            if (numExpectedKeyValues == 0)
            {
                if (!allowNullKeys)
                {
                }
            }
            else
            {
                keyNames = new string[numExpectedKeyValues];

                if (numExpectedKeyValues == 1)
                {
                    lock (_nameLookup)
                    {
                        foreach (KeyValuePair<string, object> keyValuePair in entityKeyValues)
                        {
                            keyNames[0] = tokenizeStrings ? EntityKey.LookupSingletonName(keyValuePair.Key) : keyValuePair.Key;
                            singletonKeyValue = keyValuePair.Value;
                        }
                    }
                }
                else
                {
                    compositeKeyValues = new object[numExpectedKeyValues];

                    int i = 0;
                    lock (_nameLookup)
                    {
                        foreach (KeyValuePair<string, object> keyValuePair in entityKeyValues)
                        {
                            Debug.Assert(null == keyNames[i], "shouldn't have a name yet");
                            keyNames[i] = tokenizeStrings ? EntityKey.LookupSingletonName(keyValuePair.Key) : keyValuePair.Key;
                            compositeKeyValues[i] = keyValuePair.Value;
                            i++;
                        }
                    }
                }
            }
            return numExpectedKeyValues > 0;
        }

        private static bool CompositeValuesEqual(EntityKey key1, EntityKey key2)
        {
            for (int i = 0; i < key1._compositeKeyValues.Length; ++i)
            {
                if (key1._keyNames[i].Equals(key2._keyNames[i]))
                {
                    if (!Object.Equals(key1._compositeKeyValues[i], key2._compositeKeyValues[i]))
                    {
                        return false;
                    }
                }
                // Key names might not be in the same order so try a slower approach that matches
                // key names between the keys.
                else if (!ValuesEqual(key1._keyNames[i], key1._compositeKeyValues[i], key2))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ValuesEqual(string keyName, object keyValue, EntityKey key2)
        {
            for (int i = 0; i < key2._keyNames.Length; i++)
            {
                if (String.Equals(keyName, key2._keyNames[i]))
                {
                    return Object.Equals(keyValue, key2._compositeKeyValues[i]);
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a value-based hash code, to allow EntityKey to be used in hash tables.
        /// </summary>
        /// <returns>the hash value of this EntityKey</returns>
        public override int GetHashCode()
        {
            int hashCode = _hashCode;
            if (0 == hashCode)
            {
                _containsByteArray = false;

                if (RequiresDeserialization)
                {
                    DeserializeMembers();
                }

                if (_entitySetName != null)
                {
                    hashCode = _entitySetName.GetHashCode();
                }
                if (_entityContainerName != null)
                {
                    hashCode ^= _entityContainerName.GetHashCode();
                }

                // If the key is not temporary, determine a hash code based on the value(s) within the key.
                if (null != _singletonKeyValue)
                {
                    hashCode = AddHashValue(hashCode, _singletonKeyValue);
                }
                else if (null != _compositeKeyValues)
                {
                    for (int i = 0, n = _compositeKeyValues.Length; i < n; i++)
                    {
                        hashCode = AddHashValue(hashCode, _compositeKeyValues[i]);
                    }
                }
                else
                {
                    // If the key is temporary, use default hash code
                    hashCode = base.GetHashCode();
                }

                // cache the hash code if we are a locked or fully specified EntityKey
                if (_isLocked || (!String.IsNullOrEmpty(_entitySetName) &&
                                  !String.IsNullOrEmpty(_entityContainerName) &&
                                  (_singletonKeyValue != null || _compositeKeyValues != null)))
                {
                    _hashCode = hashCode;
                }
            }
            return hashCode;
        }

        private int AddHashValue(int hashCode, object keyValue)
        {
            byte[] byteArrayValue = keyValue as byte[];
            if (null != byteArrayValue)
            {
                hashCode ^= ByValueEqualityComparer.ComputeBinaryHashCode(byteArrayValue);
                _containsByteArray = true;
                return hashCode;
            }
            else
            {
                return hashCode ^ keyValue.GetHashCode();
            }
        }

        #endregion

        #region Key Value Assignment and Validation

        /// <summary>
        /// Returns a string representation of this EntityKey, for use in debugging.
        /// Note that the returned string contains potentially sensitive information
        /// (i.e., key values), and thus shouldn't be publicly exposed.
        /// </summary>
        internal string ConcatKeyValue()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("EntitySet=").Append(_entitySetName);
            if (!IsTemporary)
            {
                foreach (EntityKeyMember pair in EntityKeyValues)
                {
                    builder.Append(';');
                    builder.Append(pair.Key).Append("=").Append(pair.Value);
                }

            }
            return builder.ToString();
        }


        /// <summary>
        /// Asserts that the "state" of the EntityKey is correct, by validating assumptions
        /// based on whether the key is a singleton, composite, or temporary.
        /// </summary>
        /// <param name="entitySet"></param>
        /// <param name="isTemporary">whether we expect this EntityKey to be marked temporary</param>
        [Conditional("DEBUG")]
        private void AssertCorrectState(EntitySetBase entitySet, bool isTemporary)
        {
            if (_singletonKeyValue != null)
            {
                Debug.Assert(!isTemporary, "Singleton keys should not be expected to be temporary.");
                Debug.Assert(_compositeKeyValues == null, "The EntityKey is marked as both a singleton key and a composite key - this is illegal.");
                if (entitySet != null)
                {
                }
            }
            else if (_compositeKeyValues != null)
            {
                Debug.Assert(!isTemporary, "Composite keys should not be expected to be temporary.");
                if (entitySet != null)
                {
                }
                for (int i = 0; i < _compositeKeyValues.Length; ++i)
                {
                    Debug.Assert(_compositeKeyValues[i] != null, "Values passed to a composite EntityKey cannot be null.");
                }
            }
            else if (!IsTemporary)
            {
                // one of our static keys
                Debug.Assert(!isTemporary, "Static keys should not be expected to be temporary.");
                Debug.Assert(this.EntityKeyValues == null, "The EntityKeyValues property for Static EntityKeys must return null.");
                Debug.Assert(this.EntityContainerName == null, "The EntityContainerName property for Static EntityKeys must return null.");
                Debug.Assert(this.EntitySetName != null, "The EntitySetName property for Static EntityKeys must not return null.");
            }
            else
            {
                Debug.Assert(isTemporary, "The EntityKey is marked as neither a singleton or composite.  Therefore, it should be expected to be temporary.");
                Debug.Assert(this.IsTemporary, "The EntityKey is marked as neither a singleton or composite.  Therefore it must be marked as temporary.");
                Debug.Assert(this.EntityKeyValues == null, "The EntityKeyValues property for temporary EntityKeys must return null.");
            }
        }

        internal static void GetEntitySetName(string qualifiedEntitySetName, out string entitySet, out string container)
        {
            if (qualifiedEntitySetName.Contains("Version") && qualifiedEntitySetName.Contains("Culture") && qualifiedEntitySetName.Contains("PublicKeyToken"))
            {
                string[] resultQfName = qualifiedEntitySetName.Split(',');
                qualifiedEntitySetName = resultQfName[0];
            }

            entitySet = null;
            container = null;
            EntityUtil.CheckStringArgument(qualifiedEntitySetName, "qualifiedEntitySetName");

            string[] result = qualifiedEntitySetName.Split('.');
            if (result.Length != 4 && result.Length != 2)
            {
                throw new ArgumentException("InvalidQualifiedEntitySetName", qualifiedEntitySetName);
            }

            if (result.Length == 4)
            {
                container = result[0] + "." + result[1] + "." + result[2];
                entitySet = result[3];
            }
            else if (result.Length == 2)
            {
                container = result[0];
                entitySet = result[1];
            }
            else
            {
                throw new ArgumentException("InvalidQualifiedEntitySetName", qualifiedEntitySetName);
            }


            // both parts must be non-empty
            if (container == null || container.Length == 0 ||
                entitySet == null || entitySet.Length == 0)
            {
                throw new ArgumentException("InvalidQualifiedEntitySetName", qualifiedEntitySetName);
            }

        }

        #endregion

        #region Serialization

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [OnDeserializing]
        public void OnDeserializing(StreamingContext context)
        {
            if (RequiresDeserialization)
            {
                DeserializeMembers();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public void OnDeserialized(StreamingContext context)
        {
            lock (_nameLookup)
            {
                _entitySetName = LookupSingletonName(_entitySetName);
                _entityContainerName = LookupSingletonName(_entityContainerName);
                if (_keyNames != null)
                {
                    for (int i = 0; i < _keyNames.Length; i++)
                    {
                        _keyNames[i] = LookupSingletonName(_keyNames[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Dev Note: this must be called from within a _lock block on _nameLookup
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string LookupSingletonName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }
            if (_nameLookup.ContainsKey(name))
            {
                return _nameLookup[name];
            }
            _nameLookup.Add(name, name);
            return name;
        }

        private void ValidateWritable(object instance)
        {
            if (_isLocked || instance != null)
            {
                throw new ArgumentException("Cannot change Entity Key", instance.ToString());
            }
        }

        private bool RequiresDeserialization
        {
            get { return _deserializedMembers != null; }
        }

        private void DeserializeMembers()
        {
            if (CheckKeyValues(new KeyValueReader(_deserializedMembers), true, true, out _keyNames, out _singletonKeyValue, out _compositeKeyValues))
            {
                // If we received values from the _deserializedMembers, then we do not need to track these any more
                _deserializedMembers = null;
            }
        }

        #endregion

        private class KeyValueReader : IEnumerable<KeyValuePair<string, object>>
        {
            IEnumerable<EntityKeyMember> _enumerator;

            public KeyValueReader(IEnumerable<EntityKeyMember> enumerator)
            {
                _enumerator = enumerator;
            }

            #region IEnumerable<KeyValuePair<string,object>> Members

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                foreach (EntityKeyMember pair in _enumerator)
                {
                    if (pair != null)
                    {
                        yield return new KeyValuePair<string, object>(pair.Key, pair.Value);
                    }
                }
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }

        public Type EntityType
        {
            get
            {
                string assemblyQfName = "";
                if (EntityContainerName.Contains("Version") && EntityContainerName.Contains("Culture") && EntityContainerName.Contains("PublicKeyToken") && EntityContainerName.Contains(EntitySetName))
                {
                    assemblyQfName = EntityContainerName;
                }
                else 
                {
                    if (EntityContainerName == "iPlusMESV5_Entities" || EntityContainerName.Contains("gip.mes.datamodel"))
                    {
                        assemblyQfName = "gip.mes.datamodel." + EntitySetName + ", gip.mes.datamodel, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = 12adb6357a02d860";
                    }

                    else if (EntityContainerName == "iPlusV5_Entities" || EntityContainerName.Contains("gip.core.datamodel"))
                    {
                        assemblyQfName = "gip.core.datamodel." + EntitySetName + ", gip.core.datamodel, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = adb6357a02d860";
                    }
                }

                Type type = Type.GetType(assemblyQfName);
                return type;
            }
        }
    }

    /// <summary>
    /// Information about a key that is part of an EntityKey.
    /// A key member contains the key name and value.
    /// </summary>
    [DataContract]
    [Serializable]
    [NotMapped]
    public class EntityKeyMember
    {
        private string _keyName;
        private object _keyValue;

        /// <summary>
        /// Creates an empty EntityKeyMember. This constructor is used by serialization.
        /// </summary>
        public EntityKeyMember()
        {
        }

        /// <summary>
        /// Creates a new EntityKeyMember with the specified key name and value.
        /// </summary>
        /// <param name="keyName">The key name</param>
        /// <param name="keyValue">The key value</param>
        public EntityKeyMember(string keyName, object keyValue)
        {
            CheckArgumentNull(keyName, "keyName");
            CheckArgumentNull(keyValue, "keyValue");
            _keyName = keyName;
            _keyValue = keyValue;
        }

        static internal T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                EntityUtil.ThrowArgumentNullException(parameterName);
            }
            return value;
        }

        /// <summary>
        /// The key name
        /// </summary>
        [DataMember]
        public string Key
        {
            get
            {
                return _keyName;
            }
            set
            {
                EntityUtil.CheckArgumentNull(value, "value");
                _keyName = value;
            }
        }

        /// <summary>
        /// The key value
        /// </summary>
        [DataMember]
        public object Value
        {
            get
            {
                return _keyValue;
            }
            set
            {
                EntityUtil.CheckArgumentNull(value, "value");
                _keyValue = value;
            }
        }

        /// <summary>
        /// Returns a string representation of the EntityKeyMember
        /// </summary>
        /// <returns>A string representation of the EntityKeyMember</returns>
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.CurrentCulture, "[{0}, {1}]", _keyName, _keyValue);
        }

    }

    [NotMapped]
    public class EntitySet : EntitySetBase
    {
        #region Constructors
        /// <summary>
        /// The constructor for constructing the EntitySet with a given name and an entity type
        /// </summary>
        /// <param name="name">The name of the EntitySet</param>
        /// <param name="schema">The db schema</param>
        /// <param name="table">The db table</param>
        /// <param name="definingQuery">The provider specific query that should be used to retrieve the EntitySet</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the argument name or entityType is null</exception>
        internal EntitySet(string name, string schema, string table, string definingQuery)
            : base(name, schema, table, definingQuery)
        {
        }
        #endregion

        #region Fields
        /*
        private volatile bool _hasForeignKeyRelationships;
        private volatile bool _hasIndependentRelationships;
        */
        #endregion

        #region properties

        #endregion
    }

    public abstract class EntitySetBase
    {
        //----------------------------------------------------------------------------------------------
        // Possible Future Enhancement: revisit factoring of EntitySetBase and delta between C constructs and S constructs
        //
        // Currently, we need to have a way to map an entityset or a relationship set in S space
        // to the appropriate structures in the store. In order to address this we said we would
        // add new ItemAttributes (tableName, schemaName and catalogName to the EntitySetBase)... 
        // problem with this is that we are bleading a leaf-level, store specific set of constructs
        // into the object model for things that may exist at either C or S. 
        //
        // We need to do this for now to push forward on enabling the conversion but we need to re-examine
        // whether we should have separate C and S space constructs or some other mechanism for 
        // maintaining this metadata.
        //----------------------------------------------------------------------------------------------

        #region Constructors
        /// <summary>
        /// The constructor for constructing the EntitySet with a given name and an entity type
        /// </summary>
        /// <param name="name">The name of the EntitySet</param>
        /// <param name="schema">The db schema</param>
        /// <param name="table">The db table</param>
        /// <param name="definingQuery">The provider specific query that should be used to retrieve the EntitySet</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the name or entityType argument is null</exception>
        internal EntitySetBase(string name, string schema, string table, string definingQuery)
        {
            // SQLBU 480236: catalogName, schemaName & tableName are allowed to be null, empty & non-empty

            _name = name;

            //---- name of the 'schema'
            //---- this is used by the SQL Gen utility to support generation of the correct name in the store
            _schema = schema;

            //---- name of the 'table'
            //---- this is used by the SQL Gen utility to support generation of the correct name in the store
            _table = table;

            //---- the Provider specific query to use to retrieve the EntitySet data
            _definingQuery = definingQuery;

        }
        #endregion

        #region Fields
        private string _name;
        private string _table;
        private string _schema;
        private string _definingQuery;
        private string _cachedProviderSql;

        #endregion

        #region Properties
        /// <summary>
        /// Returns the kind of the type
        /// </summary>


        /// <summary>
        /// Gets the identity for this item as a string
        /// </summary>
        internal string Identity
        {
            get
            {
                return this.Name;
            }
        }

        /// <summary>
        /// Gets or sets escaped SQL describing this entity set.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // referenced by System.Data.Entity.Design.dll
        internal string DefiningQuery
        {
            get { return _definingQuery; }
            set { _definingQuery = value; }
        }

        /// <summary>
        /// Get and set by the provider only as a convientent place to 
        /// store the created sql fragment that represetnts this entity set
        /// </summary>
        internal string CachedProviderSql
        {
            get { return _cachedProviderSql; }
            set { _cachedProviderSql = value; }
        }

        /// <summary>
        /// Gets/Sets the name of this entity set
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown if value passed into setter is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called when EntitySetBase instance is in ReadOnly state</exception>
        public String Name
        {
            get
            {
                return _name;
            }
        }

        internal string Table
        {
            get
            {
                return _table;
            }
        }

        internal string Schema
        {
            get
            {
                return _schema;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Overriding System.Object.ToString to provide better String representation 
        /// for this type.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }

    internal sealed class ByValueEqualityComparer : IEqualityComparer<object>
    {
        /// <summary>
        /// Provides by-value comparison for instances of the CLR equivalents of all EDM primitive types.
        /// </summary>
        internal static readonly ByValueEqualityComparer Default = new ByValueEqualityComparer();

        private ByValueEqualityComparer()
        {
        }

        public new bool Equals(object x, object y)
        {
            if (object.Equals(x, y))
            {
                return true;
            }

            // If x and y are both non-null byte arrays, then perform a by-value comparison
            // based on length and element values, otherwise defer to the default comparison.
            //
            byte[] xBytes = x as byte[];
            byte[] yBytes = y as byte[];
            if (xBytes != null && yBytes != null)
            {
                return CompareBinaryValues(xBytes, yBytes);
            }

            return false;
        }

        public int GetHashCode(object obj)
        {
            if (obj != null)
            {
                byte[] bytes = obj as byte[];
                if (bytes != null)
                {
                    return ComputeBinaryHashCode(bytes);
                }
            }
            else
            {
                return 0;
            }

            return obj.GetHashCode();
        }

        internal static int ComputeBinaryHashCode(byte[] bytes)
        {
            Debug.Assert(bytes != null, "Byte array cannot be null");
            int hashCode = 0;
            for (int i = 0, n = Math.Min(bytes.Length, 7); i < n; i++)
            {
                hashCode = ((hashCode << 5) ^ bytes[i]);
            }
            return hashCode;
        }

        internal static bool CompareBinaryValues(byte[] first, byte[] second)
        {
            Debug.Assert(first != null && second != null, "Arguments cannot be null");

            if (first.Length != second.Length)
            {
                return false;
            }

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Extends IComparer support to the (non-IComparable) byte[] type, based on by-value comparison.
    /// </summary>
    internal class ByValueComparer : IComparer
    {
        internal static readonly IComparer Default = new ByValueComparer(Comparer<object>.Default);

        private readonly IComparer nonByValueComparer;
        private ByValueComparer(IComparer comparer)
        {
            Debug.Assert(comparer != null, "Non-ByValue comparer cannot be null");
            this.nonByValueComparer = comparer;
        }

        int IComparer.Compare(object x, object y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return 0;
            }


            //We can convert DBNulls to nulls for the purposes of comparison.
            Debug.Assert(!((object.ReferenceEquals(x, DBNull.Value)) && (object.ReferenceEquals(y, DBNull.Value))), "object.ReferenceEquals should catch the case when both values are dbnull");
            if (object.ReferenceEquals(x, DBNull.Value))
            {
                x = null;
            }
            if (object.ReferenceEquals(y, DBNull.Value))
            {
                y = null;
            }

            if (x != null && y != null)
            {
                byte[] xAsBytes = x as byte[];
                byte[] yAsBytes = y as byte[];
                if (xAsBytes != null && yAsBytes != null)
                {
                    int result = xAsBytes.Length - yAsBytes.Length;
                    if (result == 0)
                    {
                        int idx = 0;
                        while (result == 0 && idx < xAsBytes.Length)
                        {
                            byte xVal = xAsBytes[idx];
                            byte yVal = yAsBytes[idx];
                            if (xVal != yVal)
                            {
                                result = xVal - yVal;
                            }
                            idx++;
                        }
                    }
                    return result;
                }
            }

            return this.nonByValueComparer.Compare(x, y);
        }
    }

    internal static class EntityUtil
    {

        static internal void CheckStringArgument(string value, string parameterName)
        {
            // Throw ArgumentNullException when string is null
            CheckArgumentNull(value, parameterName);

            // Throw ArgumentException when string is empty
            if (value.Length == 0)
            {
                throw new ArgumentException("InvalidStringArgument", parameterName);
            }

        }

        static internal T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                ThrowArgumentNullException(parameterName);
            }
            return value;
        }

        internal static void ThrowArgumentNullException(string parameterName)
        {
            throw ArgumentNull(parameterName);
        }

        static internal ArgumentNullException ArgumentNull(string parameter)
        {
            return new ArgumentNullException(parameter);
        }
    }


}