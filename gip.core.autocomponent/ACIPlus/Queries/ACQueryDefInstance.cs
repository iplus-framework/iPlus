// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.ComponentModel;
using System.Data.Entity;
using System.Collections;
using System.Text.Json;
using System.Data.Objects;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Query-Instance'}de{'Abfrage-Instanz'}", Global.ACKinds.TACObject, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACQueryDefInstance : ACQueryDefinition
    {
        public ACQueryDefInstance(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool bSucc = base.ACInit(startChildMode);
            return bSucc;
        }

        public override bool ACPostInit()
        {
            try
            {
                ACClass entityType = this.QueryType as ACClass;
                ACClass databaseClass = entityType?.ParentACObject as ACClass;
                if (databaseClass != null && typeof(IACEntityObjectContext).IsAssignableFrom(databaseClass.ObjectType))
                {
                    string contextId = entityType.GetHashCode().ToString();
                    IACEntityObjectContext context = ACObjectContextManager.GetOrCreateContext(databaseClass.ObjectType, contextId, false);
                    QueryContext = context;
                    RebuildEntitySQLFromItems();
                    ACObjectContextManager.DisposeAndRemove(context);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during post-initialization
                ACRoot.SRoot.Messages.LogException(this.GetACUrl(), nameof(ACPostInit), ex);
            }
            return base.ACPostInit();
        }

        public override IEnumerable<VBEntityObject> QueryDatabase(params Object[] acParameter)
        {
            // Flatten any nested object arrays
            acParameter = FlattenParameters(acParameter);
            
            ACQueryDefinition queryDef = Clone() as ACQueryDefinition;
            ACClass entityType = this.QueryType as ACClass;
            // Convert acParameter into key-value pairs,             // TODO: MAterial.Subproperty
            if (acParameter != null && acParameter.Length > 0)
            {
                for (int i = 0; i < acParameter.Length; i += 2)
                {
                    // Ensure we have both key and value
                    if (i + 1 < acParameter.Length)
                    {
                        string key = acParameter[i]?.ToString();
                        object value = acParameter[i + 1];

                        if (!string.IsNullOrEmpty(key) && value != null)
                        {
                            // Find matching ACFilterItem in QueryDefinition.ACFilterColumns by PropertyName
                            var filterItem = queryDef.ACFilterColumns
                                .FirstOrDefault(f => f.PropertyName == key);

                            if (filterItem != null)
                            {
                                // Set the SearchWord with the value
                                filterItem.SearchWord = value.ToString();
                            }
                            else
                            {
                                ACClassProperty aCClassProperty = entityType.GetProperty(key);
                                if (aCClassProperty != null && aCClassProperty.ObjectType != null)
                                {
                                    if (typeof(VBEntityObject).IsAssignableFrom(aCClassProperty.ObjectType))
                                        key = key + "ID";
                                    // Create new filter item if not found
                                    var newFilterItem = new ACFilterItem(
                                        Global.FilterTypes.filter,
                                        key,
                                        Global.LogicalOperators.equal,
                                        Global.Operators.and,
                                        value.ToString(),
                                        false, // isConfiguration = false (user-defined)
                                        false  // usedInGlobalSearch = false
                                    );
                                    queryDef.ACFilterColumns.Add(newFilterItem);
                                }
                                else
                                {
                                    throw new ArgumentException($"Property '{key}' does not exist in the entity type '{entityType.ACIdentifier}'. " +
                                        $"Read the ACFilterColumns property of this instance to read the current filter parameters. " +
                                        $"Or use get_property_info to read which fields the entity class or table '{entityType.ACIdentifier}' with classID '{entityType.ACClassID}' has.");
                                }
                            }
                        }
                    }
                }
            }

            return QueryDatabase(queryDef, acParameter);
        }

        public override IEnumerable<VBEntityObject> QueryDatabaseSQL(string sqlStatement, params Object[] acParameter)
        {
            if (string.IsNullOrEmpty(sqlStatement))
            {
                throw new ArgumentException("SQL statement cannot be null or empty.", nameof(sqlStatement));
            }
            ACQueryDefinition queryDef = Clone() as ACQueryDefinition;
            queryDef.EntitySQL_FromEdit = sqlStatement;

            List<ObjectParameter> sqlParameters = new List<ObjectParameter>();

            // Convert acParameter into key-value pairs for SQL parameters
            if (acParameter != null && acParameter.Length > 0)
            {
                object firstElement = acParameter[0];

                // Case 1: Single JSON array string like ["j0MaterialNo","FW02","j1UnitName","KG"]
                if (acParameter.Length == 1 && acParameter[0] is string jsonString &&
                    jsonString.TrimStart().StartsWith("[") && jsonString.TrimEnd().EndsWith("]"))
                {
                    try
                    {
                        var document = JsonDocument.Parse(jsonString);
                        var jsonArray = document.RootElement;

                        if (jsonArray.ValueKind == JsonValueKind.Array)
                        {
                            var arrayValues = new List<string>();
                            foreach (var element in jsonArray.EnumerateArray())
                            {
                                if (element.ValueKind == JsonValueKind.String)
                                {
                                    arrayValues.Add(element.GetString());
                                }
                                else if (element.ValueKind == JsonValueKind.Number)
                                {
                                    arrayValues.Add(element.ToString());
                                }
                            }

                            // Process as key-value pairs (every two elements)
                            for (int i = 0; i < arrayValues.Count; i += 2)
                            {
                                if (i + 1 < arrayValues.Count)
                                {
                                    string key = arrayValues[i];
                                    string value = arrayValues[i + 1];

                                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                                    {
                                        sqlParameters.Add(new ObjectParameter(key, value));
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // If JSON parsing fails, fall back to case 2 processing
                    }
                }

                // Case 2: Multiple separate elements as key-value pairs
                if (sqlParameters.Count == 0) // Only if Case 1 didn't process successfully
                {
                    for (int i = 0; i < acParameter.Length; i += 2)
                    {
                        // Ensure we have both key and value
                        if (i + 1 < acParameter.Length)
                        {
                            string key = acParameter[i]?.ToString();
                            object value = acParameter[i + 1];

                            if (!string.IsNullOrEmpty(key) && value != null)
                            {
                                // Create ObjectParameter with key as Name and value as Value
                                sqlParameters.Add(new ObjectParameter(key, value));
                            }
                        }
                    }
                }
            }

            queryDef.SQLParameters = sqlParameters;

            return QueryDatabase(queryDef, acParameter);
        }

        private IEnumerable<VBEntityObject> QueryDatabase(ACQueryDefinition queryDef, params Object[] acParameter)
        {
            ACClass entityType = this.QueryType as ACClass;
            ACClass databaseClass = entityType.ParentACObject as ACClass;
            if (databaseClass == null || !typeof(IACEntityObjectContext).IsAssignableFrom(databaseClass.ObjectType))
            {
                throw new InvalidOperationException("Database class is not defined for the entity type.");
            }
            string contextId = queryDef.GetHashCode().ToString();
            IACEntityObjectContext context = ACObjectContextManager.GetOrCreateContext(databaseClass.ObjectType, contextId, false);
            queryDef.QueryContext = context;
            if (!string.IsNullOrEmpty(queryDef.EntitySQL_FromEdit))
            {
                queryDef.RebuildEntitySQLFromItems();
                if (queryDef.EntitySQL_FromEdit.StartsWith("SELECT") && queryDef.EntitySQL_FromEdit.Contains(String.Format("FROM {0} AS c ", entityType.ACIdentifier)))
                {
                    throw new ArgumentException("sqlStatement must start with 'SELECT TOP {count} c.*' or 'SELECT c.* FROM {Table} AS c'. " +
                        "In the entity framework, no explicit columns can be specified using FromSqlRaw.");
                }
            }
            IQueryable queryable = context.ACSelect(queryDef);
            var result = queryable as IQueryable<VBEntityObject>;
            return new DisposableCallback<VBEntityObject>(context, result);
        }

        public override object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            if (!string.IsNullOrEmpty(acUrl))
            {
                if (acUrl.Contains(nameof(QueryDatabaseSQL)))
                {
                    try
                    {
                        if (acParameter[1] is object[] paramArray)
                        {
                            return QueryDatabaseSQL(acParameter[0] as string, paramArray);
                        }
                        else
                        {
                            return QueryDatabaseSQL(acParameter[0] as string, acParameter[1]);
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
                else if (acUrl.Contains(nameof(QueryDatabase)))
                {
                    try
                    {
                        return QueryDatabase(acParameter);
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
                if (acParameter != null && acParameter.Any())
                {
                    throw new InvalidOperationException("This is a stateless and only readable instance. " +
                        "You can only read parameters because for each query the local state (properties) is cloned before. " +
                        "For Querying the database or changing the search parameters use the available methods.");
                }
            }
            return base.ACUrlCommand(acUrl, acParameter);
        }

        public override IACType ACType
        {
            get
            {
                return this._ACType;
            }
        }

        public override IACType ACTypeIfGeneric
        {
            get
            {
                return this._ACType;
            }
        }

        /// <summary>
        /// Flattens nested object arrays into a single flat array.
        /// Handles cases where acParameter contains nested object[] arrays.
        /// </summary>
        /// <param name="parameters">The parameters array that might contain nested arrays</param>
        /// <returns>A flattened array with all nested arrays expanded</returns>
        private Object[] FlattenParameters(Object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return parameters;

            var flatList = new List<object>();
            FlattenParametersRecursive(parameters, flatList);

            return flatList.ToArray();
        }

        /// <summary>
        /// Recursively flattens parameter arrays
        /// </summary>
        /// <param name="items">Current array to process</param>
        /// <param name="flatList">The flat list to add items to</param>
        private void FlattenParametersRecursive(Object[] items, List<object> flatList)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                if (item is Object[] nestedArray)
                {
                    // Recursively flatten nested arrays
                    FlattenParametersRecursive(nestedArray, flatList);
                }
                else
                {
                    // Add non-array items directly
                    flatList.Add(item);
                }
            }
        }

        private class DisposableCallback<T> : IEnumerable<T>, IDisposable
        {
            public DisposableCallback(IACEntityObjectContext context, IEnumerable<T> value)
            {
                _ValueT = value;
                _Context = context;
            }

            private IACEntityObjectContext _Context;
            private IEnumerable<T> _ValueT;

            public void Dispose()
            {
                ACObjectContextManager.DisposeAndRemove(_Context);
                _Context = null;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _ValueT.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _ValueT.GetEnumerator();
            }
        }
    }
}
