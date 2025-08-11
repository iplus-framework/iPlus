// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACQuery.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Collections;
using gip.core.datamodel;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class DynamicQueryable
    /// </summary>
    public static class DynamicQueryable
    {

        /// <summary>
        /// Wheres the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="values">The values.</param>
        /// <returns>IQueryable{``0}.</returns>
        public static IQueryable<T> Where<T>(this IQueryable<T> source, string predicate, params object[] values) {
            return (IQueryable<T>)Where((IQueryable)source, predicate, values);
        }

        //public static IQueryable<T> Take<T>(this IQueryable<T> source, int count)
        //{
        //    return (IQueryable<T>)Take((IQueryable)source, count);
        //}


        /// <summary>
        /// Wheres the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="values">The values.</param>
        /// <returns>IQueryable.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// predicate
        /// </exception>
        public static IQueryable Where(this IQueryable source, string predicate, params object[] values) {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");
            LambdaExpression lambda = DynamicExpression.ParseLambda(source.ElementType, typeof(bool), predicate, values);
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Where",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Quote(lambda)));
        }

        /// <summary>
        /// Selects the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="values">The values.</param>
        /// <returns>IQueryable.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// selector
        /// </exception>
        public static IQueryable Select(this IQueryable source, string selector, params object[] values) {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            LambdaExpression lambda = DynamicExpression.ParseLambda(source.ElementType, null, selector, values);
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Select",
                    new Type[] { source.ElementType, lambda.Body.Type },
                    source.Expression, Expression.Quote(lambda)));
        }

        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="ordering">The ordering.</param>
        /// <param name="values">The values.</param>
        /// <returns>IQueryable{``0}.</returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, params object[] values) {
            return (IQueryable<T>)OrderBy((IQueryable)source, ordering, values);
        }

        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="ordering">The ordering.</param>
        /// <param name="values">The values.</param>
        /// <returns>IQueryable.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// ordering
        /// </exception>
        public static IQueryable OrderBy(this IQueryable source, string ordering, params object[] values) {
            if (source == null) throw new ArgumentNullException("source");
            if (ordering == null) throw new ArgumentNullException("ordering");
            ParameterExpression[] parameters = new ParameterExpression[] {
                Expression.Parameter(source.ElementType, "") };
            ExpressionParser parser = new ExpressionParser(parameters, ordering, values);
            IEnumerable<DynamicOrdering> orderings = parser.ParseOrdering();
            Expression queryExpr = source.Expression;
            string methodAsc = "OrderBy";
            string methodDesc = "OrderByDescending";
            foreach (DynamicOrdering o in orderings) {
                queryExpr = Expression.Call(
                    typeof(Queryable), o.Ascending ? methodAsc : methodDesc,
                    new Type[] { source.ElementType, o.Selector.Type },
                    queryExpr, Expression.Quote(Expression.Lambda(o.Selector, parameters)));
                methodAsc = "ThenBy";
                methodDesc = "ThenByDescending";
            }
            return source.Provider.CreateQuery(queryExpr);
        }

        /// <summary>
        /// Takes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>IQueryable.</returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public static IQueryable Take(this IQueryable source, int count) {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Take",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }

        /// <summary>
        /// Skips the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>IQueryable.</returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public static IQueryable Skip(this IQueryable source, int count) {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Skip",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }

        /// <summary>
        /// Groups the by.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="elementSelector">The element selector.</param>
        /// <param name="values">The values.</param>
        /// <returns>IQueryable.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// keySelector
        /// or
        /// elementSelector
        /// </exception>
        public static IQueryable GroupBy(this IQueryable source, string keySelector, string elementSelector, params object[] values) {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (elementSelector == null) throw new ArgumentNullException("elementSelector");
            LambdaExpression keyLambda = DynamicExpression.ParseLambda(source.ElementType, null, keySelector, values);
            LambdaExpression elementLambda = DynamicExpression.ParseLambda(source.ElementType, null, elementSelector, values);
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "GroupBy",
                    new Type[] { source.ElementType, keyLambda.Body.Type, elementLambda.Body.Type },
                    source.Expression, Expression.Quote(keyLambda), Expression.Quote(elementLambda)));
        }

        /// <summary>
        /// Anies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public static bool Any(this IQueryable source) {
            if (source == null) throw new ArgumentNullException("source");
            return (bool)source.Provider.Execute(
                Expression.Call(
                    typeof(Queryable), "Any",
                    new Type[] { source.ElementType }, source.Expression));
        }

        /// <summary>
        /// Counts the ex.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public static int CountEx(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return (int)source.Provider.Execute(
                Expression.Call(
                    typeof(Queryable), "Count",
                    new Type[] { source.ElementType }, source.Expression));
        }

        public static Expression<Func<TElement, bool>> BuildOrExpression<TElement, TValue>(
        Expression<Func<TElement, TValue>> valueSelector,
        IEnumerable<TValue> values)
        {
            if (null == valueSelector)
                throw new ArgumentNullException("valueSelector");
            if (null == values)
                throw new ArgumentNullException("values");
            ParameterExpression p = valueSelector.Parameters.Single();

            if (!values.Any())
                return e => false;

            var equals = values.Select(value =>
                (Expression)Expression.Equal(
                     valueSelector.Body,
                     Expression.Constant(
                         value,
                         typeof(TValue)
                     )
                )
            );
            var body = equals.Aggregate<Expression>(
                     (accumulate, equal) => Expression.Or(accumulate, equal)
             );

            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            if (source == null) { throw new ArgumentNullException("source"); }

            Queue<T> lastElements = new Queue<T>();
            foreach (T element in source)
            {
                lastElements.Enqueue(element);
                if (lastElements.Count > count)
                {
                    lastElements.Dequeue();
                }
            }

            return lastElements;
        }
    }

    /// <summary>
    /// Class DynamicClass
    /// </summary>
    public abstract class DynamicClass
    {
        public override string ToString() {
            PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < props.Length; i++) {
                if (i > 0) sb.Append(", ");
                sb.Append(props[i].Name);
                sb.Append("=");
                sb.Append(props[i].GetValue(this, null));
            }
            sb.Append("}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Class DynamicProperty
    /// </summary>
    public class DynamicProperty
    {
        /// <summary>
        /// The name
        /// </summary>
        string name;
        /// <summary>
        /// The type
        /// </summary>
        Type type;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProperty"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentNullException">
        /// name
        /// or
        /// type
        /// </exception>
        public DynamicProperty(string name, Type type) {
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");
            this.name = name;
            this.type = type;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name {
            get { return name; }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type {
            get { return type; }
        }
    }

    /// <summary>
    /// Class DynamicExpression
    /// </summary>
    public static class DynamicExpression
    {
        /// <summary>
        /// Parses the specified result type.
        /// </summary>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="values">The values.</param>
        /// <returns>Expression.</returns>
        public static Expression Parse(Type resultType, string expression, params object[] values) {
            ExpressionParser parser = new ExpressionParser(null, expression, values);
            return parser.Parse(resultType);
        }

        /// <summary>
        /// Parses the lambda.
        /// </summary>
        /// <param name="itType">It type.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="values">The values.</param>
        /// <returns>LambdaExpression.</returns>
        public static LambdaExpression ParseLambda(Type itType, Type resultType, string expression, params object[] values) {
            return ParseLambda(new ParameterExpression[] { Expression.Parameter(itType, "") }, resultType, expression, values);
        }

        /// <summary>
        /// Parses the lambda.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="values">The values.</param>
        /// <returns>LambdaExpression.</returns>
        public static LambdaExpression ParseLambda(ParameterExpression[] parameters, Type resultType, string expression, params object[] values) {
            ExpressionParser parser = new ExpressionParser(parameters, expression, values);
            return Expression.Lambda(parser.Parse(resultType), parameters);
        }

        /// <summary>
        /// Parses the lambda.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="values">The values.</param>
        /// <returns>Expression{Func{``0``1}}.</returns>
        public static Expression<Func<T, S>> ParseLambda<T, S>(string expression, params object[] values) {
            return (Expression<Func<T, S>>)ParseLambda(typeof(T), typeof(S), expression, values);
        }

        /// <summary>
        /// Creates the class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>Type.</returns>
        public static Type CreateClass(params DynamicProperty[] properties) {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }

        /// <summary>
        /// Creates the class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>Type.</returns>
        public static Type CreateClass(IEnumerable<DynamicProperty> properties) {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }
    }

    /// <summary>
    /// Class DynamicOrdering
    /// </summary>
    public class DynamicOrdering
    {
        /// <summary>
        /// The selector
        /// </summary>
        public Expression Selector;
        /// <summary>
        /// The ascending
        /// </summary>
        public bool Ascending;
    }

    /// <summary>
    /// Class Signature
    /// </summary>
    public class Signature : IEquatable<Signature>
    {
        /// <summary>
        /// The properties
        /// </summary>
        public DynamicProperty[] properties;
        /// <summary>
        /// The hash code
        /// </summary>
        public int hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="Signature"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public Signature(IEnumerable<DynamicProperty> properties) {
            this.properties = properties.ToArray();
            hashCode = 0;
            foreach (DynamicProperty p in properties) {
                hashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() {
            return hashCode;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">Das <see cref="T:System.Object" />, das mit dem aktuellen <see cref="T:System.Object" /> verglichen werden soll.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) {
            return obj is Signature ? Equals((Signature)obj) : false;
        }

        /// <summary>
        /// Gibt an, ob das aktuelle Objekt einem anderen Objekt des gleichen Typs entspricht.
        /// </summary>
        /// <param name="other">Ein Objekt, das mit diesem Objekt verglichen werden soll.</param>
        /// <returns>true, wenn das aktuelle Objekt gleich dem <paramref name="other" />-Parameter ist, andernfalls false.</returns>
        public bool Equals(Signature other) {
            if (properties.Length != other.properties.Length) return false;
            for (int i = 0; i < properties.Length; i++) {
                if (properties[i].Name != other.properties[i].Name ||
                    properties[i].Type != other.properties[i].Type) return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Class ClassFactory
    /// </summary>
    public class ClassFactory
    {
        /// <summary>
        /// The instance
        /// </summary>
        public static readonly ClassFactory Instance = new ClassFactory();

        /// <summary>
        /// Initializes static members of the <see cref="ClassFactory"/> class.
        /// </summary>
        static ClassFactory() { }  // Trigger lazy initialization of static fields

        /// <summary>
        /// The module
        /// </summary>
        ModuleBuilder module;
        /// <summary>
        /// The classes
        /// </summary>
        Dictionary<Signature, Type> classes;
        /// <summary>
        /// The class count
        /// </summary>
        int classCount;
        /// <summary>
        /// The rw lock
        /// </summary>
        ReaderWriterLock rwLock;

        /// <summary>
        /// Prevents a default instance of the <see cref="ClassFactory"/> class from being created.
        /// </summary>
        private ClassFactory() {
            AssemblyName name = new AssemblyName("DynamicClasses");
            AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            //AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
#if ENABLE_LINQ_PARTIAL_TRUST
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
            try 
            {
                module = assembly.DefineDynamicModule("Module");
            }
            finally {
#if ENABLE_LINQ_PARTIAL_TRUST
                PermissionSet.RevertAssert();
#endif
            }
            classes = new Dictionary<Signature, Type>();
            rwLock = new ReaderWriterLock();
        }

        /// <summary>
        /// Gets the dynamic class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>Type.</returns>
        public Type GetDynamicClass(IEnumerable<DynamicProperty> properties) {
            rwLock.AcquireReaderLock(Timeout.Infinite);
            try {
                Signature signature = new Signature(properties);
                Type type;
                if (!classes.TryGetValue(signature, out type)) {
                    type = CreateDynamicClass(signature.properties);
                    classes.Add(signature, type);
                }
                return type;
            }
            finally {
                rwLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Creates the dynamic class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>Type.</returns>
        Type CreateDynamicClass(DynamicProperty[] properties) {
            LockCookie cookie = rwLock.UpgradeToWriterLock(Timeout.Infinite);
            try {
                string typeName = "DynamicClass" + (classCount + 1);
#if ENABLE_LINQ_PARTIAL_TRUST
                new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
                try {
                    TypeBuilder tb = this.module.DefineType(typeName, TypeAttributes.Class |
                        TypeAttributes.Public, typeof(DynamicClass));
                    FieldInfo[] fields = GenerateProperties(tb, properties);
                    GenerateEquals(tb, fields);
                    GenerateGetHashCode(tb, fields);
                    Type result = tb.CreateType();
                    classCount++;
                    return result;
                }
                finally {
#if ENABLE_LINQ_PARTIAL_TRUST
                    PermissionSet.RevertAssert();
#endif
                }
            }
            finally {
                rwLock.DowngradeFromWriterLock(ref cookie);
            }
        }

        /// <summary>
        /// Generates the properties.
        /// </summary>
        /// <param name="tb">The tb.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>FieldInfo[][].</returns>
        FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties) {
            FieldInfo[] fields = new FieldBuilder[properties.Length];
            for (int i = 0; i < properties.Length; i++) {
                DynamicProperty dp = properties[i];
                FieldBuilder fb = tb.DefineField("_" + dp.Name, dp.Type, FieldAttributes.Private);
                PropertyBuilder pb = tb.DefineProperty(dp.Name, PropertyAttributes.HasDefault, dp.Type, null);
                MethodBuilder mbGet = tb.DefineMethod("get_" + dp.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    dp.Type, Type.EmptyTypes);
                ILGenerator genGet = mbGet.GetILGenerator();
                genGet.Emit(OpCodes.Ldarg_0);
                genGet.Emit(OpCodes.Ldfld, fb);
                genGet.Emit(OpCodes.Ret);
                MethodBuilder mbSet = tb.DefineMethod("set_" + dp.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    null, new Type[] { dp.Type });
                ILGenerator genSet = mbSet.GetILGenerator();
                genSet.Emit(OpCodes.Ldarg_0);
                genSet.Emit(OpCodes.Ldarg_1);
                genSet.Emit(OpCodes.Stfld, fb);
                genSet.Emit(OpCodes.Ret);
                pb.SetGetMethod(mbGet);
                pb.SetSetMethod(mbSet);
                fields[i] = fb;
            }
            return fields;
        }

        /// <summary>
        /// Generates the equals.
        /// </summary>
        /// <param name="tb">The tb.</param>
        /// <param name="fields">The fields.</param>
        void GenerateEquals(TypeBuilder tb, FieldInfo[] fields) {
            MethodBuilder mb = tb.DefineMethod("Equals",
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(bool), new Type[] { typeof(object) });
            ILGenerator gen = mb.GetILGenerator();
            LocalBuilder other = gen.DeclareLocal(tb);
            Label next = gen.DefineLabel();
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Isinst, tb);
            gen.Emit(OpCodes.Stloc, other);
            gen.Emit(OpCodes.Ldloc, other);
            gen.Emit(OpCodes.Brtrue_S, next);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ret);
            gen.MarkLabel(next);
            foreach (FieldInfo field in fields) {
                Type ft = field.FieldType;
                Type ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                next = gen.DefineLabel();
                gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.Emit(OpCodes.Ldloc, other);
                gen.Emit(OpCodes.Ldfld, field);
                gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("Equals", new Type[] { ft, ft }), null);
                gen.Emit(OpCodes.Brtrue_S, next);
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(next);
            }
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Generates the get hash code.
        /// </summary>
        /// <param name="tb">The tb.</param>
        /// <param name="fields">The fields.</param>
        void GenerateGetHashCode(TypeBuilder tb, FieldInfo[] fields) {
            MethodBuilder mb = tb.DefineMethod("GetHashCode",
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(int), Type.EmptyTypes);
            ILGenerator gen = mb.GetILGenerator();
            gen.Emit(OpCodes.Ldc_I4_0);
            foreach (FieldInfo field in fields) {
                Type ft = field.FieldType;
                Type ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("GetHashCode", new Type[] { ft }), null);
                gen.Emit(OpCodes.Xor);
            }
            gen.Emit(OpCodes.Ret);
        }
    }

    /// <summary>
    /// Class ParseException
    /// </summary>
    public sealed class ParseException : Exception
    {
        /// <summary>
        /// The position
        /// </summary>
        int position;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="position">The position.</param>
        public ParseException(string message, int position)
            : base(message) {
            this.position = position;
        }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>The position.</value>
        public int Position {
            get { return position; }
        }

        public override string ToString() {
            return string.Format(Res.ParseExceptionFormat, Message, position);
        }
    }

    /// <summary>
    /// Class ExpressionParser
    /// </summary>
    public class ExpressionParser
    {
        /// <summary>
        /// Struct Token
        /// </summary>
        struct Token
        {
            /// <summary>
            /// The id
            /// </summary>
            public TokenId id;
            /// <summary>
            /// The text
            /// </summary>
            public string text;
            /// <summary>
            /// The pos
            /// </summary>
            public int pos;
        }

        /// <summary>
        /// Enum TokenId
        /// </summary>
        enum TokenId
        {
            /// <summary>
            /// The unknown
            /// </summary>
            Unknown,
            /// <summary>
            /// The end
            /// </summary>
            End,
            /// <summary>
            /// The identifier
            /// </summary>
            Identifier,
            /// <summary>
            /// The string literal
            /// </summary>
            StringLiteral,
            /// <summary>
            /// The integer literal
            /// </summary>
            IntegerLiteral,
            /// <summary>
            /// The real literal
            /// </summary>
            RealLiteral,
            /// <summary>
            /// The exclamation
            /// </summary>
            Exclamation,
            /// <summary>
            /// The percent
            /// </summary>
            Percent,
            /// <summary>
            /// The amphersand
            /// </summary>
            Amphersand,
            /// <summary>
            /// The open paren
            /// </summary>
            OpenParen,
            /// <summary>
            /// The close paren
            /// </summary>
            CloseParen,
            /// <summary>
            /// The asterisk
            /// </summary>
            Asterisk,
            /// <summary>
            /// The plus
            /// </summary>
            Plus,
            /// <summary>
            /// The comma
            /// </summary>
            Comma,
            /// <summary>
            /// The minus
            /// </summary>
            Minus,
            /// <summary>
            /// The dot
            /// </summary>
            Dot,
            /// <summary>
            /// The slash
            /// </summary>
            Slash,
            /// <summary>
            /// The colon
            /// </summary>
            Colon,
            /// <summary>
            /// The less than
            /// </summary>
            LessThan,
            /// <summary>
            /// The equal
            /// </summary>
            Equal,
            /// <summary>
            /// The greater than
            /// </summary>
            GreaterThan,
            /// <summary>
            /// The question
            /// </summary>
            Question,
            /// <summary>
            /// The open bracket
            /// </summary>
            OpenBracket,
            /// <summary>
            /// The close bracket
            /// </summary>
            CloseBracket,
            /// <summary>
            /// The bar
            /// </summary>
            Bar,
            /// <summary>
            /// The exclamation equal
            /// </summary>
            ExclamationEqual,
            /// <summary>
            /// The double amphersand
            /// </summary>
            DoubleAmphersand,
            /// <summary>
            /// The less than equal
            /// </summary>
            LessThanEqual,
            /// <summary>
            /// The less greater
            /// </summary>
            LessGreater,
            /// <summary>
            /// The double equal
            /// </summary>
            DoubleEqual,
            /// <summary>
            /// The greater than equal
            /// </summary>
            GreaterThanEqual,
            /// <summary>
            /// The double bar
            /// </summary>
            DoubleBar
        }

        /// <summary>
        /// Interface ILogicalSignatures
        /// </summary>
        interface ILogicalSignatures
        {
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">if set to <c>true</c> [x].</param>
            /// <param name="y">if set to <c>true</c> [y].</param>
            void F(bool x, bool y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">if set to <c>true</c> [x].</param>
            /// <param name="y">if set to <c>true</c> [y].</param>
            void F(bool? x, bool? y);
        }

        /// <summary>
        /// Interface IArithmeticSignatures
        /// </summary>
        interface IArithmeticSignatures
        {
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(int x, int y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(uint x, uint y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(long x, long y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(ulong x, ulong y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(float x, float y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(double x, double y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(decimal x, decimal y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(int? x, int? y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(uint? x, uint? y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(long? x, long? y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(ulong? x, ulong? y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(float? x, float? y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(double? x, double? y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(decimal? x, decimal? y);
        }

        /// <summary>
        /// Interface IRelationalSignatures
        /// </summary>
        interface IRelationalSignatures : IArithmeticSignatures
        {
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(string x, string y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(char x, char y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(DateTime x, DateTime y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(TimeSpan x, TimeSpan y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(char? x, char? y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(DateTime? x, DateTime? y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(TimeSpan? x, TimeSpan? y);
        }

        /// <summary>
        /// Interface IEqualitySignatures
        /// </summary>
        interface IEqualitySignatures : IRelationalSignatures
        {
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">if set to <c>true</c> [x].</param>
            /// <param name="y">if set to <c>true</c> [y].</param>
            void F(bool x, bool y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">if set to <c>true</c> [x].</param>
            /// <param name="y">if set to <c>true</c> [y].</param>
            void F(bool? x, bool? y);
        }

        /// <summary>
        /// Interface IAddSignatures
        /// </summary>
        interface IAddSignatures : IArithmeticSignatures
        {
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(DateTime x, TimeSpan y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(TimeSpan x, TimeSpan y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(DateTime? x, TimeSpan? y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(TimeSpan? x, TimeSpan? y);
        }

        /// <summary>
        /// Interface ISubtractSignatures
        /// </summary>
        interface ISubtractSignatures : IAddSignatures
        {
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(DateTime x, DateTime y);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            void F(DateTime? x, DateTime? y);
        }

        /// <summary>
        /// Interface INegationSignatures
        /// </summary>
        interface INegationSignatures
        {
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            void F(int x);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            void F(long x);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            void F(float x);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            void F(double x);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            void F(decimal x);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            void F(int? x);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            void F(long? x);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            void F(float? x);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            void F(double? x);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">The x.</param>
            void F(decimal? x);
        }

        /// <summary>
        /// Interface INotSignatures
        /// </summary>
        interface INotSignatures
        {
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">if set to <c>true</c> [x].</param>
            void F(bool x);
            /// <summary>
            /// Fs the specified x.
            /// </summary>
            /// <param name="x">if set to <c>true</c> [x].</param>
            void F(bool? x);
        }

        /// <summary>
        /// Interface IEnumerableSignatures
        /// </summary>
        interface IEnumerableSignatures
        {
            /// <summary>
            /// Wheres the specified predicate.
            /// </summary>
            /// <param name="predicate">if set to <c>true</c> [predicate].</param>
            void Where(bool predicate);
            /// <summary>
            /// Anies this instance.
            /// </summary>
            void Any();
            /// <summary>
            /// Anies the specified predicate.
            /// </summary>
            /// <param name="predicate">if set to <c>true</c> [predicate].</param>
            void Any(bool predicate);
            /// <summary>
            /// Alls the specified predicate.
            /// </summary>
            /// <param name="predicate">if set to <c>true</c> [predicate].</param>
            void All(bool predicate);
            /// <summary>
            /// Counts this instance.
            /// </summary>
            void Count();
            /// <summary>
            /// Counts the specified predicate.
            /// </summary>
            /// <param name="predicate">if set to <c>true</c> [predicate].</param>
            void Count(bool predicate);
            /// <summary>
            /// Mins the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Min(object selector);
            /// <summary>
            /// Maxes the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Max(object selector);
            /// <summary>
            /// Sums the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Sum(int selector);
            /// <summary>
            /// Sums the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Sum(int? selector);
            /// <summary>
            /// Sums the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Sum(long selector);
            /// <summary>
            /// Sums the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Sum(long? selector);
            /// <summary>
            /// Sums the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Sum(float selector);
            /// <summary>
            /// Sums the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Sum(float? selector);
            /// <summary>
            /// Sums the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Sum(double selector);
            /// <summary>
            /// Sums the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Sum(double? selector);
            /// <summary>
            /// Sums the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Sum(decimal selector);
            /// <summary>
            /// Sums the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Sum(decimal? selector);
            /// <summary>
            /// Averages the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Average(int selector);
            /// <summary>
            /// Averages the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Average(int? selector);
            /// <summary>
            /// Averages the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Average(long selector);
            /// <summary>
            /// Averages the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Average(long? selector);
            /// <summary>
            /// Averages the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Average(float selector);
            /// <summary>
            /// Averages the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Average(float? selector);
            /// <summary>
            /// Averages the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Average(double selector);
            /// <summary>
            /// Averages the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Average(double? selector);
            /// <summary>
            /// Averages the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Average(decimal selector);
            /// <summary>
            /// Averages the specified selector.
            /// </summary>
            /// <param name="selector">The selector.</param>
            void Average(decimal? selector);
        }

        /// <summary>
        /// The predefined types
        /// </summary>
        static readonly Type[] predefinedTypes = {
            typeof(Object),
            typeof(Boolean),
            typeof(Char),
            typeof(String),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert)
        };

        /// <summary>
        /// The true literal
        /// </summary>
        static readonly Expression trueLiteral = Expression.Constant(true);
        /// <summary>
        /// The false literal
        /// </summary>
        static readonly Expression falseLiteral = Expression.Constant(false);
        /// <summary>
        /// The null literal
        /// </summary>
        static readonly Expression nullLiteral = Expression.Constant(null);

        /// <summary>
        /// The keyword it
        /// </summary>
        static readonly string keywordIt = "it";
        /// <summary>
        /// The keyword iif
        /// </summary>
        static readonly string keywordIif = "iif";
        /// <summary>
        /// The keyword new
        /// </summary>
        static readonly string keywordNew = "new";

        /// <summary>
        /// The keywords
        /// </summary>
        static Dictionary<string, object> keywords;

        /// <summary>
        /// The symbols
        /// </summary>
        Dictionary<string, object> symbols;
        /// <summary>
        /// The externals
        /// </summary>
        IDictionary<string, object> externals;
        /// <summary>
        /// The literals
        /// </summary>
        Dictionary<Expression, string> literals;
        /// <summary>
        /// It
        /// </summary>
        ParameterExpression it;
        /// <summary>
        /// The text
        /// </summary>
        string text;
        /// <summary>
        /// The text pos
        /// </summary>
        int textPos;
        /// <summary>
        /// The text len
        /// </summary>
        int textLen;
        /// <summary>
        /// The ch
        /// </summary>
        char ch;
        /// <summary>
        /// The token
        /// </summary>
        Token token;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionParser"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="values">The values.</param>
        /// <exception cref="System.ArgumentNullException">expression</exception>
        public ExpressionParser(ParameterExpression[] parameters, string expression, object[] values) {
            if (expression == null) throw new ArgumentNullException("expression");
            if (keywords == null) keywords = CreateKeywords();
            symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            literals = new Dictionary<Expression, string>();
            if (parameters != null) ProcessParameters(parameters);
            if (values != null) ProcessValues(values);
            text = expression;
            textLen = text.Length;
            SetTextPos(0);
            NextToken();
        }

        /// <summary>
        /// Processes the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        void ProcessParameters(ParameterExpression[] parameters) {
            foreach (ParameterExpression pe in parameters)
                if (!String.IsNullOrEmpty(pe.Name))
                    AddSymbol(pe.Name, pe);
            if (parameters.Length == 1 && String.IsNullOrEmpty(parameters[0].Name))
                it = parameters[0];
        }

        /// <summary>
        /// Processes the values.
        /// </summary>
        /// <param name="values">The values.</param>
        void ProcessValues(object[] values) {
            for (int i = 0; i < values.Length; i++) {
                object value = values[i];
                if (i == values.Length - 1 && value is IDictionary<string, object>) {
                    externals = (IDictionary<string, object>)value;
                }
                else {
                    AddSymbol("@" + i.ToString(System.Globalization.CultureInfo.InvariantCulture), value);
                }
            }
        }

        /// <summary>
        /// Adds the symbol.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        void AddSymbol(string name, object value) {
            if (symbols.ContainsKey(name))
                throw ParseError(Res.DuplicateIdentifier, name);
            symbols.Add(name, value);
        }

        /// <summary>
        /// Parses the specified result type.
        /// </summary>
        /// <param name="resultType">Type of the result.</param>
        /// <returns>Expression.</returns>
        public Expression Parse(Type resultType) {
            
            int exprPos = token.pos;
            Expression expr = ParseExpression();
            if (resultType != null)
                if ((expr = PromoteExpression(expr, resultType, true)) == null)
                    throw ParseError(exprPos, Res.ExpressionTypeMismatch, GetTypeName(resultType));
            ValidateToken(TokenId.End, Res.SyntaxError);
            return expr;
        }

#pragma warning disable 0219
        /// <summary>
        /// Parses the ordering.
        /// </summary>
        /// <returns>IEnumerable{DynamicOrdering}.</returns>
        public IEnumerable<DynamicOrdering> ParseOrdering() {
            List<DynamicOrdering> orderings = new List<DynamicOrdering>();
            while (true) {
                Expression expr = ParseExpression();
                bool ascending = true;
                if (TokenIdentifierIs("asc") || TokenIdentifierIs("ascending")) {
                    NextToken();
                }
                else if (TokenIdentifierIs("desc") || TokenIdentifierIs("descending")) {
                    NextToken();
                    ascending = false;
                }
                orderings.Add(new DynamicOrdering { Selector = expr, Ascending = ascending });
                if (token.id != TokenId.Comma) break;
                NextToken();
            }
            ValidateToken(TokenId.End, Res.SyntaxError);
            return orderings;
        }
#pragma warning restore 0219

        // ?: operator
        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseExpression() {
            int errorPos = token.pos;
            Expression expr = ParseLogicalOr();
            if (token.id == TokenId.Question) {
                NextToken();
                Expression exp = ParseExpression();
                ValidateToken(TokenId.Colon, Res.ColonExpected);
                NextToken();
                Expression expr2 = ParseExpression();
                expr = GenerateConditional(expr, exp, expr2, errorPos);
            }
            return expr;
        }

        // ||, or operator
        /// <summary>
        /// Parses the logical or.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseLogicalOr() {
            Expression left = ParseLogicalAnd();
            while (token.id == TokenId.DoubleBar || TokenIdentifierIs("or")) {
                Token op = token;
                NextToken();
                Expression right = ParseLogicalAnd();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                left = Expression.OrElse(left, right);
            }
            return left;
        }

        // &&, and operator
        /// <summary>
        /// Parses the logical and.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseLogicalAnd() {
            Expression left = ParseComparison();
            while (token.id == TokenId.DoubleAmphersand || TokenIdentifierIs("and")) {
                Token op = token;
                NextToken();
                Expression right = ParseComparison();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                left = Expression.AndAlso(left, right);
            }
            return left;
        }

        // =, ==, !=, <>, >, >=, <, <= operators
        /// <summary>
        /// Parses the comparison.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseComparison() {
            Expression left = ParseAdditive();
            while (token.id == TokenId.Equal || token.id == TokenId.DoubleEqual ||
                token.id == TokenId.ExclamationEqual || token.id == TokenId.LessGreater ||
                token.id == TokenId.GreaterThan || token.id == TokenId.GreaterThanEqual ||
                token.id == TokenId.LessThan || token.id == TokenId.LessThanEqual) {
                Token op = token;
                NextToken();
                Expression right = ParseAdditive();
                bool isEquality = op.id == TokenId.Equal || op.id == TokenId.DoubleEqual ||
                    op.id == TokenId.ExclamationEqual || op.id == TokenId.LessGreater;
                if (isEquality && !left.Type.IsValueType && !right.Type.IsValueType) {
                    if (left.Type != right.Type) {
                        if (left.Type.IsAssignableFrom(right.Type)) {
                            right = Expression.Convert(right, left.Type);
                        }
                        else if (right.Type.IsAssignableFrom(left.Type)) {
                            left = Expression.Convert(left, right.Type);
                        }
                        else {
                            throw IncompatibleOperandsError(op.text, left, right, op.pos);
                        }
                    }
                }
                else if (IsEnumType(left.Type) || IsEnumType(right.Type)) {
                    if (left.Type != right.Type) {
                        Expression e;
                        if ((e = PromoteExpression(right, left.Type, true)) != null) {
                            right = e;
                        }
                        else if ((e = PromoteExpression(left, right.Type, true)) != null) {
                            left = e;
                        }
                        else {
                            throw IncompatibleOperandsError(op.text, left, right, op.pos);
                        }
                    }
                }
                else {
                    CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures),
                        op.text, ref left, ref right, op.pos);
                }
                switch (op.id) {
                    case TokenId.Equal:
                    case TokenId.DoubleEqual:
                        left = GenerateEqual(left, right);
                        break;
                    case TokenId.ExclamationEqual:
                    case TokenId.LessGreater:
                        left = GenerateNotEqual(left, right);
                        break;
                    case TokenId.GreaterThan:
                        left = GenerateGreaterThan(left, right);
                        break;
                    case TokenId.GreaterThanEqual:
                        left = GenerateGreaterThanEqual(left, right);
                        break;
                    case TokenId.LessThan:
                        left = GenerateLessThan(left, right);
                        break;
                    case TokenId.LessThanEqual:
                        left = GenerateLessThanEqual(left, right);
                        break;
                }
            }
            return left;
        }

        // +, -, & operators
        /// <summary>
        /// Parses the additive.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseAdditive() {
            Expression left = ParseMultiplicative();
            while (token.id == TokenId.Plus || token.id == TokenId.Minus ||
                token.id == TokenId.Amphersand) {
                Token op = token;
                NextToken();
                Expression right = ParseMultiplicative();
                switch (op.id) {
                    case TokenId.Plus:
                        if (left.Type == typeof(string) || right.Type == typeof(string))
                            goto case TokenId.Amphersand;
                        CheckAndPromoteOperands(typeof(IAddSignatures), op.text, ref left, ref right, op.pos);
                        left = GenerateAdd(left, right);
                        break;
                    case TokenId.Minus:
                        CheckAndPromoteOperands(typeof(ISubtractSignatures), op.text, ref left, ref right, op.pos);
                        left = GenerateSubtract(left, right);
                        break;
                    case TokenId.Amphersand:
                        left = GenerateStringConcat(left, right);
                        break;
                }
            }
            return left;
        }

        // *, /, %, mod operators
        /// <summary>
        /// Parses the multiplicative.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseMultiplicative() {
            Expression left = ParseUnary();
            while (token.id == TokenId.Asterisk || token.id == TokenId.Slash ||
                token.id == TokenId.Percent || TokenIdentifierIs("mod")) {
                Token op = token;
                NextToken();
                Expression right = ParseUnary();
                CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.text, ref left, ref right, op.pos);
                switch (op.id) {
                    case TokenId.Asterisk:
                        left = Expression.Multiply(left, right);
                        break;
                    case TokenId.Slash:
                        left = Expression.Divide(left, right);
                        break;
                    case TokenId.Percent:
                    case TokenId.Identifier:
                        left = Expression.Modulo(left, right);
                        break;
                }
            }
            return left;
        }

        // -, !, not unary operators
        /// <summary>
        /// Parses the unary.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseUnary() {
            if (token.id == TokenId.Minus || token.id == TokenId.Exclamation ||
                TokenIdentifierIs("not")) {
                Token op = token;
                NextToken();
                if (op.id == TokenId.Minus && (token.id == TokenId.IntegerLiteral ||
                    token.id == TokenId.RealLiteral)) {
                    token.text = "-" + token.text;
                    token.pos = op.pos;
                    return ParsePrimary();
                }
                Expression expr = ParseUnary();
                if (op.id == TokenId.Minus) {
                    CheckAndPromoteOperand(typeof(INegationSignatures), op.text, ref expr, op.pos);
                    expr = Expression.Negate(expr);
                }
                else {
                    CheckAndPromoteOperand(typeof(INotSignatures), op.text, ref expr, op.pos);
                    expr = Expression.Not(expr);
                }
                return expr;
            }
            return ParsePrimary();
        }

        /// <summary>
        /// Parses the primary.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParsePrimary() {
            Expression expr = ParsePrimaryStart();
            while (true) {
                if (token.id == TokenId.Dot) {
                    NextToken();
                    expr = ParseMemberAccess(null, expr);
                }
                else if (token.id == TokenId.OpenBracket) {
                    expr = ParseElementAccess(expr);
                }
                else {
                    break;
                }
            }
            return expr;
        }

        /// <summary>
        /// Parses the primary start.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParsePrimaryStart() {
            switch (token.id) {
                case TokenId.Identifier:
                    return ParseIdentifier();
                case TokenId.StringLiteral:
                    return ParseStringLiteral();
                case TokenId.IntegerLiteral:
                    return ParseIntegerLiteral();
                case TokenId.RealLiteral:
                    return ParseRealLiteral();
                case TokenId.OpenParen:
                    return ParseParenExpression();
                default:
                    throw ParseError(Res.ExpressionExpected);
            }
        }

        /// <summary>
        /// Parses the string literal.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseStringLiteral() {
            ValidateToken(TokenId.StringLiteral);
            char quote = token.text[0];
            string s = token.text.Substring(1, token.text.Length - 2);
            int start = 0;
            while (true) {
                int i = s.IndexOf(quote, start);
                if (i < 0) break;
                s = s.Remove(i, 1);
                start = i + 1;
            }
            if (quote == '\'') {
                if (s.Length != 1)
                    throw ParseError(Res.InvalidCharacterLiteral);
                NextToken();
                return CreateLiteral(s[0], s);
            }
            NextToken();
            return CreateLiteral(s, s);
        }

        /// <summary>
        /// Parses the integer literal.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseIntegerLiteral() {
            ValidateToken(TokenId.IntegerLiteral);
            string text = token.text;
            if (text[0] != '-') {
                ulong value;
                if (!UInt64.TryParse(text, out value))
                    throw ParseError(Res.InvalidIntegerLiteral, text);
                NextToken();
                if (value <= (ulong)Int32.MaxValue) return CreateLiteral((int)value, text);
                if (value <= (ulong)UInt32.MaxValue) return CreateLiteral((uint)value, text);
                if (value <= (ulong)Int64.MaxValue) return CreateLiteral((long)value, text);
                return CreateLiteral(value, text);
            }
            else {
                long value;
                if (!Int64.TryParse(text, out value))
                    throw ParseError(Res.InvalidIntegerLiteral, text);
                NextToken();
                if (value >= Int32.MinValue && value <= Int32.MaxValue)
                    return CreateLiteral((int)value, text);
                return CreateLiteral(value, text);
            }
        }

        /// <summary>
        /// Parses the real literal.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseRealLiteral() {
            ValidateToken(TokenId.RealLiteral);
            string text = token.text;
            object value = null;
            char last = text[text.Length - 1];
            if (last == 'F' || last == 'f') {
                float f;
                if (Single.TryParse(text.Substring(0, text.Length - 1), out f)) value = f;
            }
            else {
                double d;
                if (Double.TryParse(text, out d)) value = d;
            }
            if (value == null) throw ParseError(Res.InvalidRealLiteral, text);
            NextToken();
            return CreateLiteral(value, text);
        }

        /// <summary>
        /// Creates the literal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="text">The text.</param>
        /// <returns>Expression.</returns>
        Expression CreateLiteral(object value, string text) {
            ConstantExpression expr = Expression.Constant(value);
            literals.Add(expr, text);
            return expr;
        }

        /// <summary>
        /// Parses the paren expression.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseParenExpression() {
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            Expression e = ParseExpression();
            ValidateToken(TokenId.CloseParen, Res.CloseParenOrOperatorExpected);
            NextToken();
            return e;
        }

        /// <summary>
        /// Parses the identifier.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseIdentifier() {
            ValidateToken(TokenId.Identifier);
            object value;
            if (keywords.TryGetValue(token.text, out value)) {
                if (value is Type) return ParseTypeAccess((Type)value);
                if (value == (object)keywordIt) return ParseIt();
                if (value == (object)keywordIif) return ParseIif();
                if (value == (object)keywordNew) return ParseNew();
                NextToken();
                return (Expression)value;
            }
            if (symbols.TryGetValue(token.text, out value) ||
                externals != null && externals.TryGetValue(token.text, out value)) {
                Expression expr = value as Expression;
                if (expr == null) {
                    expr = Expression.Constant(value);
                }
                else {
                    LambdaExpression lambda = expr as LambdaExpression;
                    if (lambda != null) return ParseLambdaInvocation(lambda);
                }
                NextToken();
                return expr;
            }
            if (it != null) return ParseMemberAccess(null, it);
            throw ParseError(Res.UnknownIdentifier, token.text);
        }

        /// <summary>
        /// Parses it.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseIt() {
            if (it == null)
                throw ParseError(Res.NoItInScope);
            NextToken();
            return it;
        }

        /// <summary>
        /// Parses the iif.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseIif() {
            int errorPos = token.pos;
            NextToken();
            Expression[] args = ParseArgumentList();
            if (args.Length != 3)
                throw ParseError(errorPos, Res.IifRequiresThreeArgs);
            return GenerateConditional(args[0], args[1], args[2], errorPos);
        }

        /// <summary>
        /// Generates the conditional.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="expr2">The expr2.</param>
        /// <param name="errorPos">The error pos.</param>
        /// <returns>Expression.</returns>
        Expression GenerateConditional(Expression test, Expression exp, Expression expr2, int errorPos) {
            if (test.Type != typeof(bool))
                throw ParseError(errorPos, Res.FirstExprMustBeBool);
            if (exp.Type != expr2.Type) {
                Expression expas2 = expr2 != nullLiteral ? PromoteExpression(exp, expr2.Type, true) : null;
                Expression expr2as1 = exp != nullLiteral ? PromoteExpression(expr2, exp.Type, true) : null;
                if (expas2 != null && expr2as1 == null) {
                    exp = expas2;
                }
                else if (expr2as1 != null && expas2 == null) {
                    expr2 = expr2as1;
                }
                else {
                    string type1 = exp != nullLiteral ? exp.Type.Name : "null";
                    string type2 = expr2 != nullLiteral ? expr2.Type.Name : "null";
                    if (expas2 != null && expr2as1 != null)
                        throw ParseError(errorPos, Res.BothTypesConvertToOther, type1, type2);
                    throw ParseError(errorPos, Res.NeitherTypeConvertsToOther, type1, type2);
                }
            }
            return Expression.Condition(test, exp, expr2);
        }

        /// <summary>
        /// Parses the new.
        /// </summary>
        /// <returns>Expression.</returns>
        Expression ParseNew() {
            NextToken();
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            List<DynamicProperty> properties = new List<DynamicProperty>();
            List<Expression> expressions = new List<Expression>();
            while (true) {
                int exprPos = token.pos;
                Expression expr = ParseExpression();
                string propName;
                if (TokenIdentifierIs("as")) {
                    NextToken();
                    propName = GetIdentifier();
                    NextToken();
                }
                else {
                    MemberExpression me = expr as MemberExpression;
                    if (me == null) throw ParseError(exprPos, Res.MissingAsClause);
                    propName = me.Member.Name;
                }
                expressions.Add(expr);
                properties.Add(new DynamicProperty(propName, expr.Type));
                if (token.id != TokenId.Comma) break;
                NextToken();
            }
            ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();
            Type type = DynamicExpression.CreateClass(properties);
            MemberBinding[] bindings = new MemberBinding[properties.Count];
            for (int i = 0; i < bindings.Length; i++)
                bindings[i] = Expression.Bind(type.GetProperty(properties[i].Name), expressions[i]);
            return Expression.MemberInit(Expression.New(type), bindings);
        }

        /// <summary>
        /// Parses the lambda invocation.
        /// </summary>
        /// <param name="lambda">The lambda.</param>
        /// <returns>Expression.</returns>
        Expression ParseLambdaInvocation(LambdaExpression lambda) {
            int errorPos = token.pos;
            NextToken();
            Expression[] args = ParseArgumentList();
            MethodBase method;
            if (FindMethod(lambda.Type, "Invoke", false, args, out method) != 1)
                throw ParseError(errorPos, Res.ArgsIncompatibleWithLambda);
            return Expression.Invoke(lambda, args);
        }

        /// <summary>
        /// Parses the type access.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Expression.</returns>
        Expression ParseTypeAccess(Type type) {
            int errorPos = token.pos;
            NextToken();
            if (token.id == TokenId.Question) {
                if (!type.IsValueType || IsNullableType(type))
                    throw ParseError(errorPos, Res.TypeHasNoNullableForm, GetTypeName(type));
                type = typeof(Nullable<>).MakeGenericType(type);
                NextToken();
            }
            if (token.id == TokenId.OpenParen) {
                Expression[] args = ParseArgumentList();
                MethodBase method;
                switch (FindBestMethod(type.GetConstructors(), args, out method)) {
                    case 0:
                        if (args.Length == 1)
                            return GenerateConversion(args[0], type, errorPos);
                        throw ParseError(errorPos, Res.NoMatchingConstructor, GetTypeName(type));
                    case 1:
                        return Expression.New((ConstructorInfo)method, args);
                    default:
                        throw ParseError(errorPos, Res.AmbiguousConstructorInvocation, GetTypeName(type));
                }
            }
            ValidateToken(TokenId.Dot, Res.DotOrOpenParenExpected);
            NextToken();
            return ParseMemberAccess(type, null);
        }

        /// <summary>
        /// Generates the conversion.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <param name="type">The type.</param>
        /// <param name="errorPos">The error pos.</param>
        /// <returns>Expression.</returns>
        Expression GenerateConversion(Expression expr, Type type, int errorPos) {
            Type exprType = expr.Type;
            if (exprType == type) return expr;
            if (exprType.IsValueType && type.IsValueType) {
                if ((IsNullableType(exprType) || IsNullableType(type)) &&
                    GetNonNullableType(exprType) == GetNonNullableType(type))
                    return Expression.Convert(expr, type);
                if ((IsNumericType(exprType) || IsEnumType(exprType)) &&
                    (IsNumericType(type)) || IsEnumType(type))
                    return Expression.ConvertChecked(expr, type);
            }
            if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) ||
                exprType.IsInterface || type.IsInterface)
                return Expression.Convert(expr, type);
            throw ParseError(errorPos, Res.CannotConvertValue,
                GetTypeName(exprType), GetTypeName(type));
        }

        /// <summary>
        /// Parses the member access.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>Expression.</returns>
        Expression ParseMemberAccess(Type type, Expression instance) {
            if (instance != null) type = instance.Type;
            int errorPos = token.pos;
            string id = GetIdentifier();
            NextToken();
            if (token.id == TokenId.OpenParen) {
                if (instance != null && type != typeof(string)) {
                    Type enumerableType = FindGenericType(typeof(IEnumerable<>), type);
                    if (enumerableType != null) {
                        Type elementType = enumerableType.GetGenericArguments()[0];
                        return ParseAggregate(instance, elementType, id, errorPos);
                    }
                }
                Expression[] args = ParseArgumentList();
                MethodBase mb;
                switch (FindMethod(type, id, instance == null, args, out mb)) {
                    case 0:
                        throw ParseError(errorPos, Res.NoApplicableMethod,
                            id, GetTypeName(type));
                    case 1:
                        MethodInfo method = (MethodInfo)mb;
                        if (!IsPredefinedType(method.DeclaringType))
                            throw ParseError(errorPos, Res.MethodsAreInaccessible, GetTypeName(method.DeclaringType));
                        if (method.ReturnType == typeof(void))
                            throw ParseError(errorPos, Res.MethodIsVoid,
                                id, GetTypeName(method.DeclaringType));
                        return Expression.Call(instance, (MethodInfo)method, args);
                    default:
                        throw ParseError(errorPos, Res.AmbiguousMethodInvocation,
                            id, GetTypeName(type));
                }
            }
            else {
                MemberInfo member = FindPropertyOrField(type, id, instance == null);
                if (member == null)
                    throw ParseError(errorPos, Res.UnknownPropertyOrField,
                        id, GetTypeName(type));
                return member is PropertyInfo ?
                    Expression.Property(instance, (PropertyInfo)member) :
                    Expression.Field(instance, (FieldInfo)member);
            }
        }

        /// <summary>
        /// Finds the type of the generic.
        /// </summary>
        /// <param name="generic">The generic.</param>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        static Type FindGenericType(Type generic, Type type) {
            while (type != null && type != typeof(object)) {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == generic) return type;
                if (generic.IsInterface) {
                    foreach (Type intfType in type.GetInterfaces()) {
                        Type found = FindGenericType(generic, intfType);
                        if (found != null) return found;
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Parses the aggregate.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="elementType">Type of the element.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="errorPos">The error pos.</param>
        /// <returns>Expression.</returns>
        Expression ParseAggregate(Expression instance, Type elementType, string methodName, int errorPos) {
            ParameterExpression outerIt = it;
            ParameterExpression innerIt = Expression.Parameter(elementType, "");
            it = innerIt;
            Expression[] args = ParseArgumentList();
            it = outerIt;
            MethodBase signature;
            if (FindMethod(typeof(IEnumerableSignatures), methodName, false, args, out signature) != 1)
                throw ParseError(errorPos, Res.NoApplicableAggregate, methodName);
            Type[] typeArgs;
            if (signature.Name == "Min" || signature.Name == "Max") {
                typeArgs = new Type[] { elementType, args[0].Type };
            }
            else {
                typeArgs = new Type[] { elementType };
            }
            if (args.Length == 0) {
                args = new Expression[] { instance };
            }
            else {
                args = new Expression[] { instance, Expression.Lambda(args[0], innerIt) };
            }
            return Expression.Call(typeof(Enumerable), signature.Name, typeArgs, args);
        }

        /// <summary>
        /// Parses the argument list.
        /// </summary>
        /// <returns>Expression[][].</returns>
        Expression[] ParseArgumentList() {
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            Expression[] args = token.id != TokenId.CloseParen ? ParseArguments() : new Expression[0];
            ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();
            return args;
        }

        /// <summary>
        /// Parses the arguments.
        /// </summary>
        /// <returns>Expression[][].</returns>
        Expression[] ParseArguments() {
            List<Expression> argList = new List<Expression>();
            while (true) {
                argList.Add(ParseExpression());
                if (token.id != TokenId.Comma) break;
                NextToken();
            }
            return argList.ToArray();
        }

        /// <summary>
        /// Parses the element access.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns>Expression.</returns>
        Expression ParseElementAccess(Expression expr) {
            int errorPos = token.pos;
            ValidateToken(TokenId.OpenBracket, Res.OpenParenExpected);
            NextToken();
            Expression[] args = ParseArguments();
            ValidateToken(TokenId.CloseBracket, Res.CloseBracketOrCommaExpected);
            NextToken();
            if (expr.Type.IsArray) {
                if (expr.Type.GetArrayRank() != 1 || args.Length != 1)
                    throw ParseError(errorPos, Res.CannotIndexMultiDimArray);
                Expression index = PromoteExpression(args[0], typeof(int), true);
                if (index == null)
                    throw ParseError(errorPos, Res.InvalidIndex);
                return Expression.ArrayIndex(expr, index);
            }
            else {
                MethodBase mb;
                switch (FindIndexer(expr.Type, args, out mb)) {
                    case 0:
                        throw ParseError(errorPos, Res.NoApplicableIndexer,
                            GetTypeName(expr.Type));
                    case 1:
                        return Expression.Call(expr, (MethodInfo)mb, args);
                    default:
                        throw ParseError(errorPos, Res.AmbiguousIndexerInvocation,
                            GetTypeName(expr.Type));
                }
            }
        }

        /// <summary>
        /// Determines whether [is predefined type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if [is predefined type] [the specified type]; otherwise, <c>false</c>.</returns>
        public static bool IsPredefinedType(Type type) {
            foreach (Type t in predefinedTypes) if (t == type) return true;
            return false;
        }

        /// <summary>
        /// Determines whether [is nullable type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if [is nullable type] [the specified type]; otherwise, <c>false</c>.</returns>
        public static bool IsNullableType(Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Gets the type of the non nullable.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        public static Type GetNonNullableType(Type type) {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        public static string GetTypeName(Type type) {
            Type baseType = GetNonNullableType(type);
            string s = baseType.Name;
            if (type != baseType) s += '?';
            return s;
        }

        /// <summary>
        /// Determines whether [is numeric type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if [is numeric type] [the specified type]; otherwise, <c>false</c>.</returns>
        public static bool IsNumericType(Type type) {
            return GetNumericTypeKind(type) != 0;
        }

        /// <summary>
        /// Determines whether [is signed integral type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if [is signed integral type] [the specified type]; otherwise, <c>false</c>.</returns>
        public static bool IsSignedIntegralType(Type type) {
            return GetNumericTypeKind(type) == 2;
        }

        /// <summary>
        /// Determines whether [is unsigned integral type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if [is unsigned integral type] [the specified type]; otherwise, <c>false</c>.</returns>
        public static bool IsUnsignedIntegralType(Type type) {
            return GetNumericTypeKind(type) == 3;
        }

        /// <summary>
        /// Gets the kind of the numeric type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.Int32.</returns>
        public static int GetNumericTypeKind(Type type) {
            type = GetNonNullableType(type);
            if (type.IsEnum) return 0;
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Char:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return 1;
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return 2;
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return 3;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Determines whether [is enum type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if [is enum type] [the specified type]; otherwise, <c>false</c>.</returns>
        public static bool IsEnumType(Type type) {
            return GetNonNullableType(type).IsEnum;
        }

        /// <summary>
        /// Checks the and promote operand.
        /// </summary>
        /// <param name="signatures">The signatures.</param>
        /// <param name="opName">Name of the op.</param>
        /// <param name="expr">The expr.</param>
        /// <param name="errorPos">The error pos.</param>
        void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos) {
            Expression[] args = new Expression[] { expr };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
                throw ParseError(errorPos, Res.IncompatibleOperand,
                    opName, GetTypeName(args[0].Type));
            expr = args[0];
        }

        /// <summary>
        /// Checks the and promote operands.
        /// </summary>
        /// <param name="signatures">The signatures.</param>
        /// <param name="opName">Name of the op.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="errorPos">The error pos.</param>
        void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPos) {
            Expression[] args = new Expression[] { left, right };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
            {
                if ((left.Type != typeof(Guid)) && (left.Type != typeof(Nullable<Guid>)))
                    throw IncompatibleOperandsError(opName, left, right, errorPos);
            }
            left = args[0];
            right = args[1];
        }

        /// <summary>
        /// Incompatibles the operands error.
        /// </summary>
        /// <param name="opName">Name of the op.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="pos">The pos.</param>
        /// <returns>Exception.</returns>
        Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos) {
            return ParseError(pos, Res.IncompatibleOperands,
                opName, GetTypeName(left.Type), GetTypeName(right.Type));
        }

        /// <summary>
        /// Finds the property or field.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="staticAccess">if set to <c>true</c> [static access].</param>
        /// <returns>MemberInfo.</returns>
        MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess) {
            BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type)) {
                MemberInfo[] members = t.FindMembers(MemberTypes.Property | MemberTypes.Field,
                    flags, Type.FilterNameIgnoreCase, memberName);
                if (members.Length != 0) return members[0];
            }
            return null;
        }

        /// <summary>
        /// Finds the method.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="staticAccess">if set to <c>true</c> [static access].</param>
        /// <param name="args">The args.</param>
        /// <param name="method">The method.</param>
        /// <returns>System.Int32.</returns>
        int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase method) {
            BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type)) {
                MemberInfo[] members = t.FindMembers(MemberTypes.Method,
                    flags, Type.FilterNameIgnoreCase, methodName);
                int count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
                if (count != 0) return count;
            }
            method = null;
            return 0;
        }

        /// <summary>
        /// Finds the indexer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="args">The args.</param>
        /// <param name="method">The method.</param>
        /// <returns>System.Int32.</returns>
        int FindIndexer(Type type, Expression[] args, out MethodBase method) {
            foreach (Type t in SelfAndBaseTypes(type)) {
                MemberInfo[] members = t.GetDefaultMembers();
                if (members.Length != 0) {
                    IEnumerable<MethodBase> methods = members.
                        OfType<PropertyInfo>().
                        Select(p => (MethodBase)p.GetGetMethod()).
                        Where(m => m != null);
                    int count = FindBestMethod(methods, args, out method);
                    if (count != 0) return count;
                }
            }
            method = null;
            return 0;
        }

        /// <summary>
        /// Selfs the and base types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>IEnumerable{Type}.</returns>
        static IEnumerable<Type> SelfAndBaseTypes(Type type) {
            if (type.IsInterface) {
                List<Type> types = new List<Type>();
                AddInterface(types, type);
                return types;
            }
            return SelfAndBaseClasses(type);
        }

        /// <summary>
        /// Selfs the and base classes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>IEnumerable{Type}.</returns>
        static IEnumerable<Type> SelfAndBaseClasses(Type type) {
            while (type != null) {
                yield return type;
                type = type.BaseType;
            }
        }

        /// <summary>
        /// Adds the interface.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="type">The type.</param>
        static void AddInterface(List<Type> types, Type type) {
            if (!types.Contains(type)) {
                types.Add(type);
                foreach (Type t in type.GetInterfaces()) AddInterface(types, t);
            }
        }

        /// <summary>
        /// Class MethodData
        /// </summary>
        class MethodData
        {
            /// <summary>
            /// The method base
            /// </summary>
            public MethodBase MethodBase;
            /// <summary>
            /// The parameters
            /// </summary>
            public ParameterInfo[] Parameters;
            /// <summary>
            /// The args
            /// </summary>
            public Expression[] Args;
        }

        /// <summary>
        /// Finds the best method.
        /// </summary>
        /// <param name="methods">The methods.</param>
        /// <param name="args">The args.</param>
        /// <param name="method">The method.</param>
        /// <returns>System.Int32.</returns>
        int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method) {
            MethodData[] applicable = methods.
                Select(m => new MethodData { MethodBase = m, Parameters = m.GetParameters() }).
                Where(m => IsApplicable(m, args)).
                ToArray();
            if (applicable.Length > 1) {
                applicable = applicable.
                    Where(m => applicable.All(n => m == n || IsBetterThan(args, m, n))).
                    ToArray();
            }
            if (applicable.Length == 1) {
                MethodData md = applicable[0];
                for (int i = 0; i < args.Length; i++) args[i] = md.Args[i];
                method = md.MethodBase;
            }
            else {
                method = null;
            }
            return applicable.Length;
        }

        /// <summary>
        /// Determines whether the specified method is applicable.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="args">The args.</param>
        /// <returns><c>true</c> if the specified method is applicable; otherwise, <c>false</c>.</returns>
        bool IsApplicable(MethodData method, Expression[] args) {
            if (method.Parameters.Length != args.Length) return false;
            Expression[] promotedArgs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++) {
                ParameterInfo pi = method.Parameters[i];
                if (pi.IsOut) return false;
                Expression promoted = PromoteExpression(args[i], pi.ParameterType, false);
                if (promoted == null) return false;
                promotedArgs[i] = promoted;
            }
            method.Args = promotedArgs;
            return true;
        }

        /// <summary>
        /// Promotes the expression.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <param name="type">The type.</param>
        /// <param name="exact">if set to <c>true</c> [exact].</param>
        /// <returns>Expression.</returns>
        Expression PromoteExpression(Expression expr, Type type, bool exact) {
            if (expr.Type == type) return expr;
            if (expr is ConstantExpression) {
                ConstantExpression ce = (ConstantExpression)expr;
                if (ce == nullLiteral) {
                    if (!type.IsValueType || IsNullableType(type))
                        return Expression.Constant(null, type);
                }
                else {
                    string text;
                    if (literals.TryGetValue(ce, out text)) {
                        Type target = GetNonNullableType(type);
                        Object value = null;
                        switch (Type.GetTypeCode(ce.Type)) {
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                value = ParseNumber(text, target);
                                break;
                            case TypeCode.Double:
                                if (target == typeof(decimal)) value = ParseNumber(text, target);
                                break;
                            case TypeCode.String:
                                value = ParseEnum(text, target);
                                break;
                        }
                        if (value != null)
                            return Expression.Constant(value, type);
                    }
                }
            }
            if (IsCompatibleWith(expr.Type, type)) {
                if (type.IsValueType || exact) return Expression.Convert(expr, type);
                return expr;
            }
            return null;
        }

        /// <summary>
        /// Parses the number.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="type">The type.</param>
        /// <returns>System.Object.</returns>
        static object ParseNumber(string text, Type type) {
            switch (Type.GetTypeCode(GetNonNullableType(type))) {
                case TypeCode.SByte:
                    sbyte sb;
                    if (sbyte.TryParse(text, out sb)) return sb;
                    break;
                case TypeCode.Byte:
                    byte b;
                    if (byte.TryParse(text, out b)) return b;
                    break;
                case TypeCode.Int16:
                    short s;
                    if (short.TryParse(text, out s)) return s;
                    break;
                case TypeCode.UInt16:
                    ushort us;
                    if (ushort.TryParse(text, out us)) return us;
                    break;
                case TypeCode.Int32:
                    int i;
                    if (int.TryParse(text, out i)) return i;
                    break;
                case TypeCode.UInt32:
                    uint ui;
                    if (uint.TryParse(text, out ui)) return ui;
                    break;
                case TypeCode.Int64:
                    long l;
                    if (long.TryParse(text, out l)) return l;
                    break;
                case TypeCode.UInt64:
                    ulong ul;
                    if (ulong.TryParse(text, out ul)) return ul;
                    break;
                case TypeCode.Single:
                    float f;
                    if (float.TryParse(text, out f)) return f;
                    break;
                case TypeCode.Double:
                    double d;
                    if (double.TryParse(text, out d)) return d;
                    break;
                case TypeCode.Decimal:
                    decimal e;
                    if (decimal.TryParse(text, out e)) return e;
                    break;
            }
            return null;
        }

        /// <summary>
        /// Parses the enum.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns>System.Object.</returns>
        static object ParseEnum(string name, Type type) {
            if (type.IsEnum) {
                MemberInfo[] memberInfos = type.FindMembers(MemberTypes.Field,
                    BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static,
                    Type.FilterNameIgnoreCase, name);
                if (memberInfos.Length != 0) return ((FieldInfo)memberInfos[0]).GetValue(null);
            }
            return null;
        }

        /// <summary>
        /// Determines whether [is compatible with] [the specified source].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns><c>true</c> if [is compatible with] [the specified source]; otherwise, <c>false</c>.</returns>
        static bool IsCompatibleWith(Type source, Type target) {
            if (source == target) return true;
            if (!target.IsValueType) return target.IsAssignableFrom(source);
            Type st = GetNonNullableType(source);
            Type tt = GetNonNullableType(target);
            if (st != source && tt == target) return false;
            TypeCode sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
            TypeCode tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
            switch (sc) {
                case TypeCode.SByte:
                    switch (tc) {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Byte:
                    switch (tc) {
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int16:
                    switch (tc) {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt16:
                    switch (tc) {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int32:
                    switch (tc) {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt32:
                    switch (tc) {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int64:
                    switch (tc) {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt64:
                    switch (tc) {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Single:
                    switch (tc) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                    }
                    break;
                default:
                    if (st == tt) return true;
                    break;
            }
            return false;
        }

        /// <summary>
        /// Determines whether [is better than] [the specified args].
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="m1">The m1.</param>
        /// <param name="m2">The m2.</param>
        /// <returns><c>true</c> if [is better than] [the specified args]; otherwise, <c>false</c>.</returns>
        static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2) {
            bool better = false;
            for (int i = 0; i < args.Length; i++) {
                int c = CompareConversions(args[i].Type,
                    m1.Parameters[i].ParameterType,
                    m2.Parameters[i].ParameterType);
                if (c < 0) return false;
                if (c > 0) better = true;
            }
            return better;
        }

        // Return 1 if s -> t1 is a better conversion than s -> t2
        // Return -1 if s -> t2 is a better conversion than s -> t1
        // Return 0 if neither conversion is better
        /// <summary>
        /// Compares the conversions.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="t1">The t1.</param>
        /// <param name="t2">The t2.</param>
        /// <returns>System.Int32.</returns>
        static int CompareConversions(Type s, Type t1, Type t2) {
            if (t1 == t2) return 0;
            if (s == t1) return 1;
            if (s == t2) return -1;
            bool t1t2 = IsCompatibleWith(t1, t2);
            bool t2t1 = IsCompatibleWith(t2, t1);
            if (t1t2 && !t2t1) return 1;
            if (t2t1 && !t1t2) return -1;
            if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2)) return 1;
            if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1)) return -1;
            return 0;
        }

        /// <summary>
        /// Generates the equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Expression.</returns>
        Expression GenerateEqual(Expression left, Expression right) {
            return Expression.Equal(left, right);
        }

        /// <summary>
        /// Generates the not equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Expression.</returns>
        Expression GenerateNotEqual(Expression left, Expression right) {
            return Expression.NotEqual(left, right);
        }

        /// <summary>
        /// Generates the greater than.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Expression.</returns>
        Expression GenerateGreaterThan(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.GreaterThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.GreaterThan(left, right);
        }

        /// <summary>
        /// Generates the greater than equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Expression.</returns>
        Expression GenerateGreaterThanEqual(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.GreaterThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.GreaterThanOrEqual(left, right);
        }

        /// <summary>
        /// Generates the less than.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Expression.</returns>
        Expression GenerateLessThan(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.LessThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.LessThan(left, right);
        }

        /// <summary>
        /// Generates the less than equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Expression.</returns>
        Expression GenerateLessThanEqual(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.LessThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.LessThanOrEqual(left, right);
        }

        /// <summary>
        /// Generates the add.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Expression.</returns>
        Expression GenerateAdd(Expression left, Expression right) {
            if (left.Type == typeof(string) && right.Type == typeof(string)) {
                return GenerateStaticMethodCall("Concat", left, right);
            }
            return Expression.Add(left, right);
        }

        /// <summary>
        /// Generates the subtract.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Expression.</returns>
        Expression GenerateSubtract(Expression left, Expression right) {
            return Expression.Subtract(left, right);
        }

        /// <summary>
        /// Generates the string concat.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Expression.</returns>
        Expression GenerateStringConcat(Expression left, Expression right) {
            return Expression.Call(
                null,
                typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }),
                new[] { left, right });
        }

        /// <summary>
        /// Gets the static method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>MethodInfo.</returns>
        MethodInfo GetStaticMethod(string methodName, Expression left, Expression right) {
            return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
        }

        /// <summary>
        /// Generates the static method call.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>Expression.</returns>
        Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right) {
            return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
        }

        /// <summary>
        /// Sets the text pos.
        /// </summary>
        /// <param name="pos">The pos.</param>
        void SetTextPos(int pos) {
            textPos = pos;
            ch = textPos < textLen ? text[textPos] : '\0';
        }

        /// <summary>
        /// Nexts the char.
        /// </summary>
        void NextChar() {
            if (textPos < textLen) textPos++;
            ch = textPos < textLen ? text[textPos] : '\0';
        }

        /// <summary>
        /// Nexts the token.
        /// </summary>
        void NextToken() {
            while (Char.IsWhiteSpace(ch)) NextChar();
            TokenId t;
            int tokenPos = textPos;
            switch (ch) {
                case '!':
                    NextChar();
                    if (ch == '=') {
                        NextChar();
                        t = TokenId.ExclamationEqual;
                    }
                    else {
                        t = TokenId.Exclamation;
                    }
                    break;
                case '%':
                    NextChar();
                    t = TokenId.Percent;
                    break;
                case '&':
                    NextChar();
                    if (ch == '&') {
                        NextChar();
                        t = TokenId.DoubleAmphersand;
                    }
                    else {
                        t = TokenId.Amphersand;
                    }
                    break;
                case '(':
                    NextChar();
                    t = TokenId.OpenParen;
                    break;
                case ')':
                    NextChar();
                    t = TokenId.CloseParen;
                    break;
                case '*':
                    NextChar();
                    t = TokenId.Asterisk;
                    break;
                case '+':
                    NextChar();
                    t = TokenId.Plus;
                    break;
                case ',':
                    NextChar();
                    t = TokenId.Comma;
                    break;
                case '-':
                    NextChar();
                    t = TokenId.Minus;
                    break;
                case '.':
                    NextChar();
                    t = TokenId.Dot;
                    break;
                case '/':
                    NextChar();
                    t = TokenId.Slash;
                    break;
                case ':':
                    NextChar();
                    t = TokenId.Colon;
                    break;
                case '<':
                    NextChar();
                    if (ch == '=') {
                        NextChar();
                        t = TokenId.LessThanEqual;
                    }
                    else if (ch == '>') {
                        NextChar();
                        t = TokenId.LessGreater;
                    }
                    else {
                        t = TokenId.LessThan;
                    }
                    break;
                case '=':
                    NextChar();
                    if (ch == '=') {
                        NextChar();
                        t = TokenId.DoubleEqual;
                    }
                    else {
                        t = TokenId.Equal;
                    }
                    break;
                case '>':
                    NextChar();
                    if (ch == '=') {
                        NextChar();
                        t = TokenId.GreaterThanEqual;
                    }
                    else {
                        t = TokenId.GreaterThan;
                    }
                    break;
                case '?':
                    NextChar();
                    t = TokenId.Question;
                    break;
                case '[':
                    NextChar();
                    t = TokenId.OpenBracket;
                    break;
                case ']':
                    NextChar();
                    t = TokenId.CloseBracket;
                    break;
                case '|':
                    NextChar();
                    if (ch == '|') {
                        NextChar();
                        t = TokenId.DoubleBar;
                    }
                    else {
                        t = TokenId.Bar;
                    }
                    break;
                case '"':
                case '\'':
                    char quote = ch;
                    do {
                        NextChar();
                        while (textPos < textLen && ch != quote) NextChar();
                        if (textPos == textLen)
                            throw ParseError(textPos, Res.UnterminatedStringLiteral);
                        NextChar();
                    } while (ch == quote);
                    t = TokenId.StringLiteral;
                    break;
                default:
                    if (Char.IsLetter(ch) || ch == '@' || ch == '_') {
                        do {
                            NextChar();
                        } while (Char.IsLetterOrDigit(ch) || ch == '_');
                        t = TokenId.Identifier;
                        break;
                    }
                    if (Char.IsDigit(ch)) {
                        t = TokenId.IntegerLiteral;
                        do {
                            NextChar();
                        } while (Char.IsDigit(ch));
                        if (ch == '.') {
                            t = TokenId.RealLiteral;
                            NextChar();
                            ValidateDigit();
                            do {
                                NextChar();
                            } while (Char.IsDigit(ch));
                        }
                        if (ch == 'E' || ch == 'e') {
                            t = TokenId.RealLiteral;
                            NextChar();
                            if (ch == '+' || ch == '-') NextChar();
                            ValidateDigit();
                            do {
                                NextChar();
                            } while (Char.IsDigit(ch));
                        }
                        if (ch == 'F' || ch == 'f') NextChar();
                        break;
                    }
                    if (textPos == textLen) {
                        t = TokenId.End;
                        break;
                    }
                    throw ParseError(textPos, Res.InvalidCharacter, ch);
            }
            token.id = t;
            token.text = text.Substring(tokenPos, textPos - tokenPos);
            token.pos = tokenPos;
        }

        /// <summary>
        /// Tokens the identifier is.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool TokenIdentifierIs(string id) {
            return token.id == TokenId.Identifier && String.Equals(id, token.text, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <returns>System.String.</returns>
        string GetIdentifier() {
            ValidateToken(TokenId.Identifier, Res.IdentifierExpected);
            string id = token.text;
            if (id.Length > 1 && id[0] == '@') id = id.Substring(1);
            return id;
        }

        /// <summary>
        /// Validates the digit.
        /// </summary>
        void ValidateDigit() {
            if (!Char.IsDigit(ch)) throw ParseError(textPos, Res.DigitExpected);
        }

        /// <summary>
        /// Validates the token.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="errorMessage">The error message.</param>
        void ValidateToken(TokenId t, string errorMessage) {
            if (token.id != t) throw ParseError(errorMessage);
        }

        /// <summary>
        /// Validates the token.
        /// </summary>
        /// <param name="t">The t.</param>
        void ValidateToken(TokenId t) {
            if (token.id != t) throw ParseError(Res.SyntaxError);
        }

        /// <summary>
        /// Parses the error.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        /// <returns>Exception.</returns>
        Exception ParseError(string format, params object[] args) {
            return ParseError(token.pos, format, args);
        }

        /// <summary>
        /// Parses the error.
        /// </summary>
        /// <param name="pos">The pos.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        /// <returns>Exception.</returns>
        Exception ParseError(int pos, string format, params object[] args) {
            return new ParseException(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), pos);
        }

        /// <summary>
        /// Creates the keywords.
        /// </summary>
        /// <returns>Dictionary{System.StringSystem.Object}.</returns>
        static Dictionary<string, object> CreateKeywords() {
            Dictionary<string, object> d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            d.Add("true", trueLiteral);
            d.Add("false", falseLiteral);
            d.Add("null", nullLiteral);
            d.Add(keywordIt, keywordIt);
            d.Add(keywordIif, keywordIif);
            d.Add(keywordNew, keywordNew);
            foreach (Type type in predefinedTypes) d.Add(type.Name, type);
            return d;
        }
    }

    /// <summary>
    /// Class Res
    /// </summary>
    static class Res
    {
        /// <summary>
        /// The duplicate identifier
        /// </summary>
        public const string DuplicateIdentifier = "The identifier '{0}' was defined more than once";
        /// <summary>
        /// The expression type mismatch
        /// </summary>
        public const string ExpressionTypeMismatch = "Expression of type '{0}' expected";
        /// <summary>
        /// The expression expected
        /// </summary>
        public const string ExpressionExpected = "Expression expected";
        /// <summary>
        /// The invalid character literal
        /// </summary>
        public const string InvalidCharacterLiteral = "Character literal must contain exactly one character";
        /// <summary>
        /// The invalid integer literal
        /// </summary>
        public const string InvalidIntegerLiteral = "Invalid integer literal '{0}'";
        /// <summary>
        /// The invalid real literal
        /// </summary>
        public const string InvalidRealLiteral = "Invalid real literal '{0}'";
        /// <summary>
        /// The unknown identifier
        /// </summary>
        public const string UnknownIdentifier = "Unknown identifier '{0}'";
        /// <summary>
        /// The no it in scope
        /// </summary>
        public const string NoItInScope = "No 'it' is in scope";
        /// <summary>
        /// The iif requires three args
        /// </summary>
        public const string IifRequiresThreeArgs = "The 'iif' function requires three arguments";
        /// <summary>
        /// The first expr must be bool
        /// </summary>
        public const string FirstExprMustBeBool = "The first expression must be of type 'Boolean'";
        /// <summary>
        /// The both types convert to other
        /// </summary>
        public const string BothTypesConvertToOther = "Both of the types '{0}' and '{1}' convert to the other";
        /// <summary>
        /// The neither type converts to other
        /// </summary>
        public const string NeitherTypeConvertsToOther = "Neither of the types '{0}' and '{1}' converts to the other";
        /// <summary>
        /// The missing as clause
        /// </summary>
        public const string MissingAsClause = "Expression is missing an 'as' clause";
        /// <summary>
        /// The args incompatible with lambda
        /// </summary>
        public const string ArgsIncompatibleWithLambda = "Argument list incompatible with lambda expression";
        /// <summary>
        /// The type has no nullable form
        /// </summary>
        public const string TypeHasNoNullableForm = "Type '{0}' has no nullable form";
        /// <summary>
        /// The no matching constructor
        /// </summary>
        public const string NoMatchingConstructor = "No matching constructor in type '{0}'";
        /// <summary>
        /// The ambiguous constructor invocation
        /// </summary>
        public const string AmbiguousConstructorInvocation = "Ambiguous invocation of '{0}' constructor";
        /// <summary>
        /// The cannot convert value
        /// </summary>
        public const string CannotConvertValue = "A value of type '{0}' cannot be converted to type '{1}'";
        /// <summary>
        /// The no applicable method
        /// </summary>
        public const string NoApplicableMethod = "No applicable method '{0}' exists in type '{1}'";
        /// <summary>
        /// The methods are inaccessible
        /// </summary>
        public const string MethodsAreInaccessible = "Methods on type '{0}' are not accessible";
        /// <summary>
        /// The method is void
        /// </summary>
        public const string MethodIsVoid = "Method '{0}' in type '{1}' does not return a value";
        /// <summary>
        /// The ambiguous method invocation
        /// </summary>
        public const string AmbiguousMethodInvocation = "Ambiguous invocation of method '{0}' in type '{1}'";
        /// <summary>
        /// The unknown property or field
        /// </summary>
        public const string UnknownPropertyOrField = "No property or field '{0}' exists in type '{1}'";
        /// <summary>
        /// The no applicable aggregate
        /// </summary>
        public const string NoApplicableAggregate = "No applicable aggregate method '{0}' exists";
        /// <summary>
        /// The cannot index multi dim array
        /// </summary>
        public const string CannotIndexMultiDimArray = "Indexing of multi-dimensional arrays is not supported";
        /// <summary>
        /// The invalid index
        /// </summary>
        public const string InvalidIndex = "Array index must be an integer expression";
        /// <summary>
        /// The no applicable indexer
        /// </summary>
        public const string NoApplicableIndexer = "No applicable indexer exists in type '{0}'";
        /// <summary>
        /// The ambiguous indexer invocation
        /// </summary>
        public const string AmbiguousIndexerInvocation = "Ambiguous invocation of indexer in type '{0}'";
        /// <summary>
        /// The incompatible operand
        /// </summary>
        public const string IncompatibleOperand = "Operator '{0}' incompatible with operand type '{1}'";
        /// <summary>
        /// The incompatible operands
        /// </summary>
        public const string IncompatibleOperands = "Operator '{0}' incompatible with operand types '{1}' and '{2}'";
        /// <summary>
        /// The unterminated string literal
        /// </summary>
        public const string UnterminatedStringLiteral = "Unterminated string literal";
        /// <summary>
        /// The invalid character
        /// </summary>
        public const string InvalidCharacter = "Syntax error '{0}'";
        /// <summary>
        /// The digit expected
        /// </summary>
        public const string DigitExpected = "Digit expected";
        /// <summary>
        /// The syntax error
        /// </summary>
        public const string SyntaxError = "Syntax error";
        /// <summary>
        /// The token expected
        /// </summary>
        public const string TokenExpected = "{0} expected";
        /// <summary>
        /// The parse exception format
        /// </summary>
        public const string ParseExceptionFormat = "{0} (at index {1})";
        /// <summary>
        /// The colon expected
        /// </summary>
        public const string ColonExpected = "':' expected";
        /// <summary>
        /// The open paren expected
        /// </summary>
        public const string OpenParenExpected = "'(' expected";
        /// <summary>
        /// The close paren or operator expected
        /// </summary>
        public const string CloseParenOrOperatorExpected = "')' or operator expected";
        /// <summary>
        /// The close paren or comma expected
        /// </summary>
        public const string CloseParenOrCommaExpected = "')' or ',' expected";
        /// <summary>
        /// The dot or open paren expected
        /// </summary>
        public const string DotOrOpenParenExpected = "'.' or '(' expected";
        /// <summary>
        /// The open bracket expected
        /// </summary>
        public const string OpenBracketExpected = "'[' expected";
        /// <summary>
        /// The close bracket or comma expected
        /// </summary>
        public const string CloseBracketOrCommaExpected = "']' or ',' expected";
        /// <summary>
        /// The identifier expected
        /// </summary>
        public const string IdentifierExpected = "Identifier expected";
    }

    /// <summary>
    /// Class DynamicEnumerable
    /// </summary>
    public static class DynamicEnumerable
    {
        //public static IEnumerable<T> Where<T>(this IEnumerable<T> source, string predicate, params object[] values)
        //{
        //    return (IEnumerable<T>)Where((IEnumerable)source, predicate, typeof(T), values);
        //}

        //public static IEnumerable Where(this IEnumerable source, string predicate, Type t, params object[] values)
        //{
        //    if (source == null) throw new ArgumentNullException("source");
        //    if (predicate == null) throw new ArgumentNullException("predicate");
        //    LambdaExpression lambda = DynamicExpression.ParseLambda(t, typeof(bool), predicate, values);
        //    return source.Provider.CreateQuery(
        //        Expression.Call(
        //            typeof(Queryable), "Where",
        //            new Type[] { t },
        //            source.Expression, Expression.Quote(lambda)));
        //    return null;
        //}

        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="ordering">The ordering.</param>
        /// <param name="values">The values.</param>
        /// <returns>IEnumerable{``0}.</returns>
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, string ordering, params object[] values)
        {
            return (IEnumerable<T>)OrderBy(source, ordering, values, true);
        }

        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <returns>IEnumerable{``0}.</returns>
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string propertyName, bool ascending)
        {
            ParameterExpression prm = System.Linq.Expressions.Expression.Parameter(typeof(T), "it");

            Expression property = prm;
            if (propertyName.IndexOf(".") != -1)
            {
                string[] properties = propertyName.Split('.');
                foreach (var p in properties)
                {
                    property = System.Linq.Expressions.Expression.Property(property, p);
                }
            }
            else
            {
                property = System.Linq.Expressions.Expression.Property(prm, propertyName);
            }
            Type propertyType = property.Type;

            MethodInfo method = typeof(DynamicEnumerable).GetMethod("OrderByProperty", BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(typeof(T), propertyType);

            return (IEnumerable<T>)method.Invoke(null, new object[] { query, prm, property, ascending });
        }

        /// <summary>
        /// Orders the by property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="query">The query.</param>
        /// <param name="prm">The PRM.</param>
        /// <param name="property">The property.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <returns>IEnumerable{``0}.</returns>
        private static IEnumerable<T> OrderByProperty<T, P>(this IEnumerable<T> query, ParameterExpression prm, System.Linq.Expressions.Expression property, bool ascending)
        {
            Func<IEnumerable<T>, Func<T, P>, IEnumerable<T>> orderBy = (q, p) => ascending ? q.OrderBy(p) : q.OrderByDescending(p);
            return orderBy(query, System.Linq.Expressions.Expression.Lambda<Func<T, P>>(property, prm).Compile());
        }


        /// <summary>
        /// Finds the index in the collection where the predicate evaluates to true.
        /// Returns -1 if no matching item found
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="predicate">Function to evaluate</param>
        /// <returns>Index where predicate is true, or -1 if not found.</returns>
        public static int IndexWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var enumerator = source.GetEnumerator();
            int index = 0;
            while (enumerator.MoveNext())
            {
                T obj = enumerator.Current;
                if (predicate(obj))
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Finds the index in the collection where the predicate evaluates to true.
        /// Returns -1 if no matching item found
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="predicate">Function to evaluate</param>
        /// <returns>Index where predicate is true, or -1 if not found.</returns>
        public static int IndexWhere<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            var enumerator = source.GetEnumerator();
            int index = 0;
            while (enumerator.MoveNext())
            {
                T obj = enumerator.Current;
                if (predicate(obj, index))
                    return index;
                index++;
            }
            return -1;
        }

        public static IQueryable<T> AutoMergeOption<T>(this IQueryable<T> source, IACEntityObjectContext context)
        {
            if (source == null || context == null)
               throw new ArgumentNullException(nameof(context));
#if EFCR
            if (context != null)
                source = source.Refresh(context.RecommendedMergeOption);
#endif
            return source;
        }

        public static IQueryable<T> SetMergeOption<T>(this IQueryable<T> source, MergeOption option)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
#if EFCR
            source = source.Refresh(option);
#endif
            return source;
        }

        private static string GetPath(Expression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var name = GetPath(((MemberExpression)exp).Expression) ?? "";

                    if (name.Length > 0)
                        name += ".";

                    return name + ((MemberExpression)exp).Member.Name;

                case ExpressionType.Convert:
                case ExpressionType.Quote:
                    return GetPath(((UnaryExpression)exp).Operand);

                case ExpressionType.Lambda:
                    return GetPath(((LambdaExpression)exp).Body);

                default:
                    return null;
            }
        }

    }

}
