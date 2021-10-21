using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace gip.core.datamodel
{
    #region enums
    internal enum TOKEN_TYPE { NONE, COMMENT, COMMENT_BLOCK, TEXT, INT, UINT, LONG, ULONG, FLOAT, DOUBLE, DECIMAL, BOOL, IDENTIFIER, OPERATOR }
    public enum OPERATOR_TYPE
    {
        UNKNOWN,
        OPEN,   // (,[,{
        CLOSE,  // ),],}
        PREFIX_UNARY,  // +,-,++,--
        POST_UNARY, // ++, --
        BINARY,  // +,-,*,/
        CONDITIONAL,    // (c)?x:y
        ASSIGN, // =, +=
        PRIMARY   // , ;
    }
    #endregion enums

    /// <summary>
    /// http://simproexpr.codeplex.com/
    /// ExprParser has 3 naming scope
    ///     Global : available for all Parser instance.
    ///     Parser : available for current Parser instance.
    ///     Inline : available for current expression.
    ///     
    /// User can create multiple parser instance for different naming scope
    /// One parser can create multiple expression with the same naming scope
    /// One expression can define in-line namespace and parameter only for current expression scope
    /// </summary>
    public class ExprParser
    {
        #region namespace, parameter and type: case sensitive

        internal static Dictionary<string, Type> SystemType = SetSystemType();
        private static Dictionary<string, Type> SetSystemType()
        {
            Dictionary<string, Type> dic = new Dictionary<string, Type>();
            dic.Add("bool", typeof(bool));
            dic.Add("Boolean", typeof(Boolean));
            dic.Add("char", typeof(char));
            dic.Add("Char", typeof(Char));
            dic.Add("string", typeof(string));
            dic.Add("String", typeof(String));
            dic.Add("byte", typeof(byte));
            dic.Add("Byte", typeof(Byte));
            dic.Add("sbyte", typeof(sbyte));
            dic.Add("SByte", typeof(SByte));
            dic.Add("short", typeof(short));
            dic.Add("Int16", typeof(Int16));
            dic.Add("ushort", typeof(ushort));
            dic.Add("UInt16", typeof(UInt16));
            dic.Add("int", typeof(int));
            dic.Add("Int32", typeof(Int32));
            dic.Add("uint", typeof(uint));
            dic.Add("UInt32", typeof(UInt32));
            dic.Add("long", typeof(long));
            dic.Add("Int64", typeof(Int64));
            dic.Add("ulong", typeof(ulong));
            dic.Add("UInt64", typeof(UInt64));
            dic.Add("float", typeof(float));
            dic.Add("Single", typeof(Single));
            dic.Add("double", typeof(double));
            dic.Add("Double", typeof(Double));
            dic.Add("decimal", typeof(decimal));
            dic.Add("Decimal", typeof(Decimal));
            dic.Add("enum", typeof(Enum));
            dic.Add("object", typeof(object));
            dic.Add("Object", typeof(Object));
            return dic;
        }
        internal static Dictionary<Type, Type> NullableType = SetNullableType();
        private static Dictionary<Type, Type> SetNullableType()
        {
            Dictionary<Type, Type> dic = new Dictionary<Type, Type>();
            dic.Add(typeof(bool), typeof(bool?));
            dic.Add(typeof(char), typeof(char?));
            dic.Add(typeof(byte), typeof(byte?));
            dic.Add(typeof(sbyte), typeof(sbyte?));
            dic.Add(typeof(short), typeof(short?));
            dic.Add(typeof(ushort), typeof(ushort?));
            dic.Add(typeof(int), typeof(int?));
            dic.Add(typeof(uint), typeof(uint?));
            dic.Add(typeof(long), typeof(long?));
            dic.Add(typeof(ulong), typeof(ulong?));
            dic.Add(typeof(float), typeof(float?));
            dic.Add(typeof(double), typeof(double?));
            dic.Add(typeof(decimal), typeof(decimal?));
            return dic;
        }


        // namespace for static data type
        public static Using Using = new Using();
        internal Assembly caller_assembly = null;

        // parameter type
        public static Dictionary<string, Type> GlobalParameterType = new Dictionary<string, Type>();    // Global scope
        public Dictionary<string, Type> ParameterType = new Dictionary<string, Type>();                  // Parser scope
        private Dictionary<string, Type> inlineParameterType = null;                                    // Expr scope
        // parameter value
        public static Dictionary<string, object> GlobalParameterValue = new Dictionary<string, object>();   // Global scope
        public Dictionary<string, object> ParameterValue = new Dictionary<string, object>();                // Parser scope


        /// <summary>
        /// check by order:
        ///     - System Type               // user type cannot be the same with system type. check system type first is for performance
        ///     - Type.GetType(type_name)
        ///     - Type from Local Namespace
        ///     - Type from Globel Namespace
        /// limitation:
        ///     Type.GetType use AssmblyQulifiedName: "ns.type, ns". not include version info, "type" part not include "."
        /// </summary>
        /// <param name="type_name">type or instance object name</param>
        /// <returns></returns>
        internal Type QueryStaticType(string type_name)
        {
            // check with dicSystemType
            if (SystemType.ContainsKey(type_name))
                return SystemType[type_name];

            // check type_name itself
            Type t = null;
            if ((t = Type.GetType(type_name)) != null) return t;    // check System(mscorlib) assembly
            if (caller_assembly != null && (t = caller_assembly.GetType(type_name)) != null) return t;  // check caller assembly

            foreach (string ns in Using.namespace_list.Keys)
            {
                string full_type_name = ns + "." + type_name;
                if (Using.namespace_list[ns] == null)
                {
                    if ((t = Type.GetType(full_type_name)) != null) 
                        return t;    // check System(mscorlib) assembly
                    if (caller_assembly != null && (t = caller_assembly.GetType(full_type_name)) != null) 
                        return t;  // check caller assembly
                    if ((t = Type.GetType(full_type_name + ", " + ns)) != null) 
                        return t;   // check namespace as assembly name
                }
                else if (Using.namespace_list[ns] != null && (t = Using.namespace_list[ns].GetType(full_type_name)) != null) 
                    return t;
            }
            if (type_name.Contains(".") && (t = Type.GetType(type_name + ", " + type_name.Substring(0, type_name.LastIndexOf('.')))) != null)
                return t;

            //// check with Namespace
            //if (this.Namespace != null)
            //{
            //    foreach (string ns in this.Namespace)
            //        if ((t = Type.GetType(ns + "." + type_name)) != null
            //            || (t = Type.GetType(ns + "." + type_name + ", " + ns)) != null) return t;
            //}
            //// check with GlobalNamespace
            //if (GlobalNamespace != null)
            //{
            //    foreach (string ns in GlobalNamespace)
            //        if ((t = Type.GetType(ns + "." + type_name)) != null
            //            || (t = Type.GetType(ns + "." + type_name + ", " + ns)) != null) return t;
            //}
            //// check with localNamespace
            //if (inlineNamespace != null)
            //{
            //    foreach (string ns in inlineNamespace)
            //        if ((t = Type.GetType(ns + "." + type_name)) != null
            //            || (t = Type.GetType(ns + "." + type_name + ", " + ns)) != null) return t;
            //}
            return null;
        }

        /// <summary>
        /// check by order
        ///     Parameter Type
        ///     Globel Parameter Type
        ///     queryParameterValue(parameter_name)
        /// </summary>
        /// <param name="parameter_name"></param>
        /// <returns></returns>
        internal Type QueryParameterType(string parameter_name)
        {
            if (ParameterType != null && ParameterType.ContainsKey(parameter_name))
                return ParameterType[parameter_name];
            else if (GlobalParameterType.ContainsKey(parameter_name))
                return GlobalParameterType[parameter_name];
            else if (inlineParameterType != null && inlineParameterType.ContainsKey(parameter_name))
                return inlineParameterType[parameter_name];
            return null;
        }


        // implicit conversion
        internal static Dictionary<Type, List<Type>> dicImplicitConversion = SetImplicitConversionMap();
        internal static Dictionary<Type, List<Type>> SetImplicitConversionMap()
        {
            Dictionary<Type, List<Type>> dic = new Dictionary<Type, List<Type>>();
            dic.Add(typeof(char), new List<Type>(new Type[] { typeof(char), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(char?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(byte), new List<Type>(new Type[] { typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(byte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(sbyte), new List<Type>(new Type[] { typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(sbyte?), typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(short), new List<Type>(new Type[] { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(ushort), new List<Type>(new Type[] { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(int), new List<Type>(new Type[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(uint), new List<Type>(new Type[] { typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(long), new List<Type>(new Type[] { typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(ulong), new List<Type>(new Type[] { typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(float), new List<Type>(new Type[] { typeof(float), typeof(double), typeof(decimal), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(double), new List<Type>(new Type[] { typeof(double), typeof(double?) }));
            dic.Add(typeof(decimal), new List<Type>(new Type[] { typeof(decimal), typeof(decimal?) }));

            dic.Add(typeof(char?), new List<Type>(new Type[] { typeof(char?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(byte?), new List<Type>(new Type[] { typeof(byte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(sbyte?), new List<Type>(new Type[] { typeof(sbyte?), typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(short?), new List<Type>(new Type[] { typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(ushort?), new List<Type>(new Type[] { typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(int?), new List<Type>(new Type[] { typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(uint?), new List<Type>(new Type[] { typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(long?), new List<Type>(new Type[] { typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(ulong?), new List<Type>(new Type[] { typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(float?), new List<Type>(new Type[] { typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(double?), new List<Type>(new Type[] { typeof(double?) }));
            dic.Add(typeof(decimal?), new List<Type>(new Type[] { typeof(decimal?) }));

            return dic;
        }
        internal Type QueryImplicitConversionType(Expression ex1, Expression ex2)
        {
            if (dicImplicitConversion.ContainsKey(ex1.Type) && dicImplicitConversion.ContainsKey(ex2.Type))
            {
                foreach (Type t in dicImplicitConversion[ex1.Type])
                    if (dicImplicitConversion[ex2.Type].Contains(t)) return t;
            }
            if (ex1 is ConstantExpression && ((ConstantExpression)ex1).Value == null && !ex2.Type.IsValueType) return ex2.Type;
            if (ex2 is ConstantExpression && ((ConstantExpression)ex2).Value == null && !ex1.Type.IsValueType) return ex1.Type;
            return null;
        }

        #endregion parameter and type

        public LambdaExpression Parse(string expression_string)
        {
            caller_assembly = Assembly.GetCallingAssembly();       // need to reset by every parsing
            inlineParameterType = new Dictionary<string, Type>();   // need to reset by every parsing
            TokenStore declare_ts;
            ParseInfo parseInfo = new ParseInfo();

            TokenStore ts = TokenStore.Parse(expression_string, out declare_ts);
            if (declare_ts != null)
            {
                //ParseDeclaration(declare_ts);
                parseInfo.localVariableStack.Push(new Dictionary<string, ParameterExpression>());
                Assert(ParseNextExpression(declare_ts, null, parseInfo) == null, "Incorrect parameter declaration.");
                var lv = parseInfo.localVariableStack.Pop();
                foreach (string name in lv.Keys)
                    inlineParameterType.Add(name, lv[name].Type);
            }
            return Expression.Lambda(ParseStatementBlock(ts, parseInfo, false), parseInfo.referredParameterList.ToArray());
        }

        internal Expression ParseStatementBlock(TokenStore ts, ParseInfo parseInfo, bool isVoid)
        {
            List<Expression> expr_list = new List<Expression>();
            parseInfo.localVariableStack.Push(new Dictionary<string, ParameterExpression>());
            //AToken tok;
            //Expression test, exp1, exp2, statement, statement2;
            if (ts != null)
            {
                while (ts.Current != null)
                {
                    Expression statement = ParseNextStatement(ts, parseInfo);
                    if (statement != null) expr_list.Add(statement);
                }
            }
            if (isVoid && expr_list.Count > 0 && expr_list[expr_list.Count - 1].Type != typeof(void)) 
                expr_list.Add(Expression.Empty());
            var localVariable = parseInfo.localVariableStack.Pop();
            if (parseInfo.localVariableStack.Count == 0 && parseInfo.returnLabel != null) // this is the initial block, add return label if has been referred inside the block.
                expr_list.Add(parseInfo.returnLabel);
            if (expr_list.Count == 0) 
                return Expression.Empty();    //.Constant(null);
            else if (expr_list.Count == 1 && localVariable.Count == 0) 
                return expr_list[0];
            else
                return Expression.Block(localVariable.Values, expr_list.ToArray());
        }

        internal Expression ParseNextStatement(TokenStore ts, ParseInfo parseInfo)
        {
            Expression test, exp1, exp2, statement = null, statement2;
            AToken tok = ts.Current;
            if (tok.tok_type == TOKEN_TYPE.IDENTIFIER)
            {
                switch (tok.value)
                {
                    case "if":
                        #region if(test) statement else statement2
                        Assert((tok = ts.Next()) != null && tok.value == "(", tok);
                        Assert((test = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null && ts.Current != null, ts.Current);
                        statement = ParseNextStatement(ts, parseInfo);
                        //if (ts.CurrentValue == "{")
                        //    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                        //else
                        //{
                        //    statement = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        //    Assert(ts.CurrentValue == ";", ts.Current); ts.Next();
                        //}
                        if (statement == null) 
                            statement = Expression.Empty();
                        if (ts.CurrentValue == "else")
                        {
                            ts.Next();  // skip "else"
                            statement2 = ParseNextStatement(ts, parseInfo);
                            //if (ts.CurrentValue == "{")
                            //    statement2 = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                            //else
                            //    statement2 = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                            if (statement2 == null) 
                                statement2 = Expression.Empty();
                            statement = Expression.IfThenElse(test, statement, statement2);
                        }
                        else
                            statement = Expression.IfThen(test, statement);
                        break;
                        #endregion
                    case "while":
                        #region while(test) statement : loop { if(test) statement; continueLabel; } breakLabel;
                        parseInfo.jumpInfoStack.Push(new JumpInfo());
                        Assert((tok = ts.Next()) != null && tok.value == "(", tok); // skip "while"
                        Assert((test = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null && ts.Current != null, ts.Current);
                        statement = ParseNextStatement(ts, parseInfo);
                        //if (ts.CurrentValue == "{")
                        //    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                        //else
                        //    statement = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (statement == null) 
                            statement = Expression.Empty();
                        JumpInfo jumpInfo = parseInfo.jumpInfoStack.Pop();
                        if (jumpInfo.breakLabel == null) 
                            jumpInfo.breakLabel = Expression.Label(Expression.Label());
                        statement = Expression.Loop(Expression.IfThenElse(test, statement, Expression.Break(jumpInfo.breakLabel.Target)), jumpInfo.breakLabel.Target, (jumpInfo.continueLabel == null) ? null : jumpInfo.continueLabel.Target);
                        break;
                        #endregion
                    case "do":
                        #region do statement while(test) : loop { statement; if(test) break; continueLabel } breakLabel
                        ts.Next(); // skip "do"
                        parseInfo.jumpInfoStack.Push(new JumpInfo());
                        statement = ParseNextStatement(ts, parseInfo);
                        //if (ts.CurrentValue == "{")
                        //    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                        //else
                        //    statement = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (statement == null) statement = Expression.Empty();
                        Assert(ts.CurrentValue == "while", ts.Current);
                        Assert((tok = ts.Next()) != null && tok.value == "(", tok); // skip "while"
                        Assert((test = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null, ts.Current);
                        //Assert(ts.CurrentValue == ";", ts.Current); ts.Next();  // skip ";"
                        jumpInfo = parseInfo.jumpInfoStack.Pop();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());
                        statement = Expression.Loop(Expression.Block(statement, Expression.IfThenElse(test, Expression.Empty(), Expression.Break(jumpInfo.breakLabel.Target))), jumpInfo.breakLabel.Target, (jumpInfo.continueLabel == null) ? null : jumpInfo.continueLabel.Target);
                        break;
                        #endregion
                    case "for":
                        #region for(exp1,test,exp2) statement : exp1; loop{if(test) statement; continuelabel;exp2;} breaklabel;
                        parseInfo.localVariableStack.Push(new Dictionary<string, ParameterExpression>());   // exp1 can contain local variable only for "for statement"
                        parseInfo.jumpInfoStack.Push(new JumpInfo());
                        Assert((tok = ts.Next()) != null && tok.value == "(", tok); // skip "for"
                        TokenStore sub_ts = ts.NextUntilOperatorClose();
                        exp1 = ParseNextExpression(sub_ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (exp1 == null) exp1 = Expression.Empty();
                        Assert(sub_ts.CurrentValue == ";", sub_ts.Current);
                        sub_ts.Next();  // skip ";"
                        test = ParseNextExpression(sub_ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        Assert(sub_ts.CurrentValue == ";", sub_ts.Current);
                        sub_ts.Next();  // skip ";"
                        exp2 = ParseNextExpression(sub_ts, null, parseInfo);

                        statement = ParseNextStatement(ts, parseInfo);
                        //if (ts.CurrentValue == "{")
                        //    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                        //else
                        //    statement = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (statement == null) statement = Expression.Empty();
                        var lv = parseInfo.localVariableStack.Pop();
                        jumpInfo = parseInfo.jumpInfoStack.Pop();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());

                        //if (exp1 != null) expr_list.Add(exp1);
                        List<Expression> sub_exp_list = new List<Expression>();
                        if (test != null) statement = Expression.IfThenElse(test, statement, Expression.Break(jumpInfo.breakLabel.Target));
                        sub_exp_list.Add(statement);
                        if (jumpInfo.continueLabel != null) sub_exp_list.Add(jumpInfo.continueLabel);
                        if (exp2 != null) sub_exp_list.Add(exp2);
                        statement = Expression.Loop(Expression.Block(sub_exp_list), jumpInfo.breakLabel.Target);
                        //expr_list.Add(jumpInfo.breakLabel);
                        statement = Expression.Block(lv.Values, exp1, statement);
                        break;
                        #endregion
                    case "foreach":
                        #region foreach(exp1 var in exp2) statement : IEnumerator ie=exp2.GetEnumerator(); Loop { if(ie.MoveNext()) {var=ie.Current; statement;} continueLabel } breakLabel

                        // begin stack
                        lv = new Dictionary<string, ParameterExpression>();
                        parseInfo.localVariableStack.Push(lv);   // exp1 can contain local variable only for "for statement"
                        parseInfo.jumpInfoStack.Push(new JumpInfo());
                        Assert(ts.Next() != null && ts.CurrentValue == "(", ts.Current);  // skip "foreach" to "("
                        sub_ts = ts.NextUntilOperatorClose();
                        // parse T
                        Assert((exp1 = ParseReferredParameterOrType(sub_ts, parseInfo, true, false, true)) != null && exp1 is ConstantExpression && ((ConstantExpression)exp1).Value is Type, sub_ts.Current);
                        Type t = (Type)((ConstantExpression)exp1).Value;
                        // parse var
                        Assert(sub_ts.Current != null && sub_ts.Current.tok_type == TOKEN_TYPE.IDENTIFIER, sub_ts.Current);    // to var
                        ParameterExpression var = Expression.Variable(t, sub_ts.Current.value);
                        parseInfo.localVariableStack.Peek().Add(sub_ts.Current.value, var);
                        // parse exp - IEnumerator
                        Assert((tok = sub_ts.Next()) != null && tok.value == "in", tok);
                        sub_ts.Next();  // skip "in"
                        Assert((exp2 = ParseNextExpression(sub_ts, null, parseInfo)) != null, (AToken)null);
                        // organize expressions
                        ParameterExpression ie = Expression.Variable(typeof(IEnumerator));
                        lv.Add("$$$$ie$$$$", ie);
                        exp1 = Expression.Assign(ie, Expression.Call(exp2, "GetEnumerator", null, null));   // IEnumerator ie=exp2.GetEnumerator();
                        test = Expression.Call(ie, "MoveNext", null, null);     // ie.MoveNext()
                        exp2 = Expression.Assign(var, Expression.Convert(Expression.Property(ie, "Current"), var.Type));  // var=ie.Current;
                        // parse statement
                        statement = ParseNextStatement(ts, parseInfo);
                        //if (ts.CurrentValue == "{")
                        //    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                        //else
                        //    statement = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (statement == null) statement = Expression.Empty();
                        // end stack
                        lv = parseInfo.localVariableStack.Pop();
                        jumpInfo = parseInfo.jumpInfoStack.Pop();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());
                        // create foreach
                        //expr_list.Add(exp1);
                        statement = Expression.Loop(Expression.IfThenElse(test, Expression.Block(exp2, statement), Expression.Break(jumpInfo.breakLabel.Target)), jumpInfo.breakLabel.Target, jumpInfo.continueLabel == null ? null : jumpInfo.continueLabel.Target);
                        statement = Expression.Block(lv.Values, exp1, statement);
                        break;
                        #endregion
                    case "break":
                        #region break;
                        ts.Next();  // skip "break"
                        jumpInfo = parseInfo.jumpInfoStack.Peek();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());
                        statement = Expression.Break(jumpInfo.breakLabel.Target);
                        break;
                        #endregion
                    case "continue":
                        #region continue;
                        ts.Next();  // skip "continue"
                        jumpInfo = parseInfo.jumpInfoStack.Peek();
                        if (jumpInfo.continueLabel == null) jumpInfo.continueLabel = Expression.Label(Expression.Label());
                        statement = Expression.Continue(jumpInfo.continueLabel.Target);
                        break;
                        #endregion
                    case "switch":
                        #region switch(test) { case test_values: case_statements; default:default_statements; }

                        // start stack
                        parseInfo.localVariableStack.Push(new Dictionary<string, ParameterExpression>());
                        parseInfo.jumpInfoStack.Push(new JumpInfo());
                        // parse exp
                        Assert(ts.Next() != null && ts.CurrentValue == "(", ts.Current);  // skip "switch" to "("
                        Assert((test = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null, ts.Current);

                        // parse case list
                        Assert(ts.CurrentValue == "{", ts.Current);
                        sub_ts = ts.NextUntilOperatorClose();
                        List<SwitchCase> case_list = new List<SwitchCase>();
                        List<Expression> test_values = new List<Expression>();
                        List<Expression> case_statements = new List<Expression>();
                        Expression default_statements = null;
                        bool isDefault = false;
                        while ((tok = sub_ts.Current) != null)
                        {
                            if (tok.value == "case")
                            {
                                Assert(!isDefault, tok);
                                if (test_values.Count > 0 && case_statements.Count > 0)
                                {
                                    case_list.Add(Expression.SwitchCase(Expression.Block(case_statements), test_values));
                                    case_statements.Clear();
                                    test_values.Clear();
                                }
                                sub_ts.Next();  // skip "case"
                                test_values.Add(ParseNextExpression(sub_ts, AnOperator.dicSystemOperator[":"], parseInfo));
                                Assert(sub_ts.CurrentValue == ":" && sub_ts.Next() != null, sub_ts.Current);  // skip :
                            }
                            else if (tok.value == "default")
                            {
                                isDefault = true;
                                if (test_values.Count > 0 && case_statements.Count > 0)
                                {
                                    case_list.Add(Expression.SwitchCase(Expression.Block(case_statements), test_values));
                                    case_statements.Clear();
                                    test_values.Clear();
                                }
                                Assert(sub_ts.Next() != null && sub_ts.CurrentValue == ":" && sub_ts.Next() != null, sub_ts.Current);  // skip "default" and ":"
                            }
                            else
                            {
                                statement = ParseNextStatement(sub_ts, parseInfo);
                                if (statement != null) case_statements.Add(statement);
                            }
                            //else if (tok.value == "{")
                            //    case_statements.Add(ParseStatementBlock(sub_ts.NextUntilOperatorClose(), parseInfo));
                            //else
                            //    case_statements.Add(GetNextExpression(sub_ts, AnOperator.dicSystemOperator[";"], parseInfo));
                        }
                        if (case_statements.Count > 0)
                        {
                            if (test_values.Count > 0)
                                case_list.Add(Expression.SwitchCase(Expression.Block(case_statements), test_values));
                            if (isDefault)
                                default_statements = Expression.Block(case_statements);
                        }
                        // end stack
                        lv = parseInfo.localVariableStack.Pop();
                        jumpInfo = parseInfo.jumpInfoStack.Pop();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());
                        // create switch
                        statement = Expression.Switch(test, default_statements, case_list.ToArray());
                        statement = Expression.Block(lv.Values, statement, jumpInfo.breakLabel);
                        break;
                        #endregion
                    case "try":
                        #region try { statement } catch(T var) { statement2 } finally { statement2 }
                        // parse try block
                        Assert((tok = ts.Next()) != null && tok.value == "{", tok);  // skip "try" to "{"
                        statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo, true);
                        if (statement == null) statement = Expression.Empty();
                        // parse catch blocks
                        List<CatchBlock> catch_list = new List<CatchBlock>();
                        while (ts.CurrentValue == "catch")
                        {
                            ts.Next();  // skip "catch"
                            bool hasParameter = false;
                            if (ts.CurrentValue == "(")
                            {
                                hasParameter = true;
                                parseInfo.localVariableStack.Push(new Dictionary<string, ParameterExpression>());
                                Assert(ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo) == null && parseInfo.localVariableStack.Peek().Count == 1, "Invalid syntax for catch."); // can only have one variable declare and no value assignment
                            }
                            Assert(ts.CurrentValue == "{", ts.Current);
                            statement2 = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo, true);
                            if (statement2 == null) statement2 = Expression.Empty();
                            if (hasParameter)
                            {
                                Dictionary<string, ParameterExpression> dic = parseInfo.localVariableStack.Pop();
                                foreach (string key in dic.Keys)    // there is only one key
                                    catch_list.Add(Expression.Catch(dic[key], statement2));
                            }
                            else
                                catch_list.Add(Expression.Catch(typeof(Exception), statement2));
                        }
                        // parse finally block
                        statement2 = null;
                        if (ts.CurrentValue == "finally")
                        {
                            Assert((tok = ts.Next()) != null && tok.value == "{", tok);  // skip "finally"
                            statement2 = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo, false);
                            if (statement2 == null) statement2 = Expression.Empty();
                        }
                        statement = Expression.TryCatchFinally(statement, statement2, catch_list.ToArray());
                        break;
                        #endregion
                    case "throw":
                        #region throw new T;
                        ts.Next();  // skip "throw"
                        statement2 = ParseNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (statement2 == null)
                            statement = Expression.Rethrow();
                        else
                            statement = Expression.Throw(statement2);
                        break;
                        #endregion
                    case "return":
                        #region return exp;
                        ts.Next();  // skip "return"
                        exp1 = ParseNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (exp1 == null)
                        {
                            if (parseInfo.returnLabel == null) parseInfo.returnLabel = Expression.Label(Expression.Label());
                            statement = Expression.Return(parseInfo.returnLabel.Target);
                        }
                        else
                        {
                            if (exp1.Type != typeof(object)) exp1 = Expression.Convert(exp1, typeof(object));   // convert all return value to object in case return different type
                            if (parseInfo.returnLabel == null) parseInfo.returnLabel = Expression.Label(Expression.Label(exp1.Type), Expression.Default(exp1.Type));
                            statement = Expression.Return(parseInfo.returnLabel.Target, exp1);
                        }
                        break;
                        #endregion

                }
            }
            if (statement == null)
            {
                if (tok.value == "{")
                    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo, false);
                else
                    statement = ParseNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
            }
            if (ts.CurrentValue == ";") ts.Next();  // skip ";"
            return statement;
        }

        internal Expression ParseNextExpression(TokenStore ts, AnOperator leftOperator, ParseInfo parseInfo)
        {
            Expression exp = null, exp2 = null, exp3 = null, expPrev = null;
            Type t = null;
            MethodInfo mi = null;
            AToken tok;
            while ((tok = ts.Current) != null)
            {
                if (leftOperator != null && tok.op != null && tok.op.precedence >= leftOperator.precedence) return exp;

                // current operator has high precedence than leftOp
                switch (tok.tok_type)
                {
                    case TOKEN_TYPE.INT: Assert(exp == null, tok); exp = Expression.Constant(int.Parse(tok.value)); break;
                    case TOKEN_TYPE.UINT: Assert(exp == null, tok); exp = Expression.Constant(uint.Parse(tok.value)); break;
                    case TOKEN_TYPE.LONG: Assert(exp == null, tok); exp = Expression.Constant(long.Parse(tok.value)); break;
                    case TOKEN_TYPE.ULONG: Assert(exp == null, tok); exp = Expression.Constant(ulong.Parse(tok.value)); break;
                    case TOKEN_TYPE.FLOAT: Assert(exp == null, tok); exp = Expression.Constant(float.Parse(tok.value)); break;
                    case TOKEN_TYPE.DOUBLE: Assert(exp == null, tok); exp = Expression.Constant(double.Parse(tok.value)); break;
                    case TOKEN_TYPE.DECIMAL: Assert(exp == null, tok); exp = Expression.Constant(decimal.Parse(tok.value, System.Globalization.NumberStyles.AllowExponent)); break;
                    case TOKEN_TYPE.BOOL: Assert(exp == null, tok); exp = Expression.Constant(bool.Parse(tok.value)); break;
                    case TOKEN_TYPE.TEXT: Assert(exp == null, tok); exp = Expression.Constant(tok.value); break;
                    case TOKEN_TYPE.IDENTIFIER:   // x, x[]
                        #region Parameter or Static Type reference
                        if (exp != null && exp is ConstantExpression && ((ConstantExpression)exp).Value is Type) // static type
                        {
                            #region declare local variable: T var1=exp1, var2=exp2;
                            t = (Type)((ConstantExpression)exp).Value;
                            Dictionary<string, ParameterExpression> localVaribles = parseInfo.localVariableStack.Peek();
                            List<Expression> exp_list = new List<Expression>();
                            while (ts.Current != null)
                            {
                                Assert(ts.Current.tok_type == TOKEN_TYPE.IDENTIFIER, ts.Current);

                                ParameterExpression var = Expression.Variable(t, ts.Current.value);
                                localVaribles.Add(ts.Current.value, var);
                                ts.Next();   // skip variable

                                if (ts.CurrentValue == "=") // variable initializer
                                {
                                    ts.Next();  // skip "="
                                    if (var.Type.IsArray && ts.CurrentValue == "{") // array initializer: T[] var = {1,2,3};
                                        exp = Expression.NewArrayInit(var.Type.GetElementType(), ParseParameters(ts, parseInfo, null, false));
                                    else
                                        exp = ParseNextExpression(ts, AnOperator.dicSystemOperator[","], parseInfo);
                                    if (exp.Type != t)
                                        exp = Expression.Convert(exp, t);
                                    exp_list.Add(Expression.Assign(var, exp));
                                }

                                if (ts.Current == null || ts.Current.value == ";")
                                    break;  // end of statement
                                if (ts.Current.value == ",")
                                {
                                    ts.Next(); // skip "," to next variable
                                    if (!var.Type.IsArray)
                                        break; 
                                }
                            }
                            if (exp_list.Count == 0)
                            {
                                expPrev = exp;
                                exp = null;
                            }
                            else if (exp_list.Count == 1)
                                exp = exp_list[0];
                            else
                                exp = Expression.Block(exp_list);
                            #endregion
                        }
                        else
                        {
                            #region Parameter or Static Type reference
                            bool testIfNextIsTypeDecl = (exp == null && ts.PreviousToken != null && ts.PreviousToken.value == ",");
                            exp = ParseReferredParameterOrType(ts, parseInfo, true, true, true, testIfNextIsTypeDecl);

                            if (testIfNextIsTypeDecl)
                            {
                                if (exp == null)
                                {
                                    exp = expPrev;
                                    tok = null;
                                }
                                else
                                    ParseReferredParameterOrType(ts, parseInfo, true, true, true, false);
                            }
                            Assert(exp != null, ts.Current);
                            #endregion
                        }
                        break;
                        #endregion
                    case TOKEN_TYPE.OPERATOR:
                        #region Operator
                        switch (tok.op.op_type)
                        {
                            case OPERATOR_TYPE.OPEN:
                                #region Open operators: (, [. "{" will be proccessed in ParseStatement()
                                switch (tok.value)
                                {
                                    case "(":
                                        #region ParenthesisLeft: (nested expression)
                                        Assert(exp == null, tok);
                                        exp = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo);
                                        if (exp is ConstantExpression && ((ConstantExpression)exp).Value is Type) // (T)x
                                        {
                                            exp2 = ParseNextExpression(ts, AnOperator.dicSystemOperator["(T)"], parseInfo);
                                            if (exp2 != null)
                                                exp = Expression.Convert(exp2, (Type)((ConstantExpression)exp).Value);
                                        }
                                        break;
                                        #endregion
                                    case "[":
                                        #region BracketLeft: indexer[]
                                        Assert(exp != null, tok);
                                        {
                                            List<Type> param_types = new List<Type>();
                                            List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, false);

                                            //TokenStore sub_ts = ts.NextUntilOperatorClose();
                                            //List<Expression> param_list = new List<Expression>();
                                            //while ((exp2 = GetNextExpression(sub_ts, AnOperator.dicSystemOperator[","], exprParameterList)) != null)
                                            //{
                                            //    param_list.Add(exp2);
                                            //    sub_ts.Next();   // to ','
                                            //}
                                            ////List<TokenStore> ts_list = ts.NextUntilOperatorClose().Split(OPERATOR_NAME.Comma);
                                            ////List<Expression> param_list = new List<Expression>();
                                            ////foreach (TokenStore one_ts in ts_list)
                                            ////{
                                            ////    if(one_ts.token_list.Count>0)
                                            ////        param_list.Add(GetNextExpression(one_ts, null, exprParameterList));
                                            ////}
                                            //List<Type> param_types = new List<Type>();
                                            //foreach (Expression e in param_list) param_types.Add(e.Type);
                                            if ((mi = exp.Type.GetMethod(tok.op.overload_name, BindingFlags.Instance | BindingFlags.Public, null, param_types.ToArray(), null)) != null)
                                                exp = Expression.Call(exp, mi, param_list.ToArray());
                                            else
                                                exp = Expression.ArrayIndex(exp, param_list.ToArray());
                                        }
                                        break;
                                        #endregion
                                    default:
                                        Assert(false, tok);
                                        break;
                                }
                                break;
                                #endregion
                            case OPERATOR_TYPE.PREFIX_UNARY:
                                #region prefix unary operators
                                Assert(exp == null && ts.Next() != null, tok);
                                Assert((exp2 = ParseNextExpression(ts, tok.op, parseInfo)) != null, ts.Current);
                                mi = tok.op.overload_name == null ? null : exp2.Type.GetMethod(tok.op.overload_name, new Type[] { exp2.Type });
                                exp = ((d1m)tok.op.exp_call)(exp2, mi);
                                break;
                                #endregion
                            case OPERATOR_TYPE.POST_UNARY:
                                #region POST_UNARY
                                Assert(exp != null, tok);
                                mi = tok.op.overload_name == null ? null : exp.Type.GetMethod(tok.op.overload_name, new Type[] { exp.Type });
                                exp = ((d1m)tok.op.exp_call)(exp, mi);
                                break;
                                #endregion
                            case OPERATOR_TYPE.BINARY:
                            case OPERATOR_TYPE.ASSIGN:
                                #region Binary or Assign Operators
                                Assert(exp != null && ts.Next() != null && (exp2 = ParseNextExpression(ts, tok.op, parseInfo)) != null, tok);
                                mi = null; t = null;
                                if (tok.value == "is")
                                {
                                    Assert(exp2 is ConstantExpression && ((ConstantExpression)exp2).Value is Type, tok);
                                    exp = Expression.TypeIs(exp, (Type)((ConstantExpression)exp2).Value);
                                    break;
                                }
                                else if (tok.value == "as")
                                {
                                    Assert(exp2 is ConstantExpression && ((ConstantExpression)exp2).Value is Type, tok);
                                    exp = Expression.TypeAs(exp, (Type)((ConstantExpression)exp2).Value);
                                    break;
                                }
                                if (tok.value == "+" && exp.Type == typeof(string) || exp2.Type == typeof(string))
                                {
                                    if (exp.Type != typeof(string)) exp = Expression.Convert(exp, typeof(string));
                                    if (exp2.Type != typeof(string)) exp2 = Expression.Convert(exp2, typeof(string));
                                    mi = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                                }
                                else
                                {
                                    if (tok.op.overload_name != null)
                                    {
                                        mi = exp.Type.GetMethod(tok.op.overload_name, new Type[] { exp.Type, exp2.Type });
                                        if (mi == null)
                                            mi = exp2.Type.GetMethod(tok.op.overload_name, new Type[] { exp.Type, exp2.Type });
                                    }
                                    if (mi == null)
                                    {
                                        // use default operator, need implicit type conversion
                                        if (tok.op.required1stType != null) exp = Expression.Convert(exp, tok.op.required1stType);
                                        if (tok.op.required2ndType != null) exp2 = Expression.Convert(exp2, tok.op.required2ndType);
                                        if (tok.op.requiredOperandType == RequiredOperandType.SAME
                                            && exp.Type != exp2.Type
                                            && (t = QueryImplicitConversionType(exp, exp2)) != null)
                                        {
                                            if (exp.Type != t) exp = Expression.Convert(exp, t);
                                            if (exp2.Type != t) exp2 = Expression.Convert(exp2, t);
                                        }
                                    }
                                }
                                if (tok.op.exp_call is d2)
                                    exp = ((d2)tok.op.exp_call)(exp, exp2);
                                else if (tok.op.exp_call is d2m)
                                    exp = ((d2m)tok.op.exp_call)(exp, exp2, mi);
                                else
                                    exp = ((d2bm)tok.op.exp_call)(exp, exp2, false, mi);
                                break;
                                #endregion
                            case OPERATOR_TYPE.CONDITIONAL:
                                #region Conditional
                                Assert(exp != null && ts.Next() != null && (exp2 = ParseNextExpression(ts, tok.op, parseInfo)) != null, tok);
                                Assert(ts.CurrentValue == ":" && ts.Next() != null, tok);
                                Assert((exp3 = ParseNextExpression(ts, tok.op, parseInfo)) != null, tok);
                                if (exp2.Type != exp3.Type && (t = QueryImplicitConversionType(exp2, exp3)) != null)
                                {
                                    if (exp2.Type != t) exp2 = Expression.Convert(exp2, t);
                                    if (exp3.Type != t) exp3 = Expression.Convert(exp3, t);
                                }
                                exp = Expression.Condition(exp, exp2, exp3);
                                break;
                                #endregion
                            case OPERATOR_TYPE.PRIMARY:
                                #region Primary:
                                switch (tok.value)
                                {
                                    case "new":
                                        #region New
                                        {
                                            Assert(exp == null && ts.Next() != null && ts.Current.tok_type == TOKEN_TYPE.IDENTIFIER, tok); // skip "new"
                                            Assert((exp = ParseReferredParameterOrType(ts, parseInfo, true, false, false)) != null, tok);
                                            t = (Type)((ConstantExpression)exp).Value;
                                            Assert(ts.Current != null && ts.Current.value == "(" || ts.Current.value == "[", ts.Current);    // next is '(' or '['
                                            if (ts.Current.value == "(")   // new object().
                                            {
                                                List<Type> param_types = new List<Type>();
                                                List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, false);

                                                ConstructorInfo ci = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, param_types.ToArray(), null);
                                                Assert(ci != null, ts.Current);
                                                // check parameter implicit conversion
                                                ParameterInfo[] pis = ci.GetParameters();
                                                for (int idx = 0; idx < param_list.Count; idx++)
                                                {
                                                    if (param_list[idx].Type != pis[idx].ParameterType)
                                                        param_list[idx] = Expression.Convert(param_list[idx], pis[idx].ParameterType);
                                                }
                                                exp = Expression.New(ci, param_list.ToArray());
                                            }
                                            else // new int[x,y] or new type[]{x,y}
                                            {
                                                List<Expression> param_list = ParseParameters(ts, parseInfo, null, false);  // [x,y]
                                                if (ts.CurrentValue == "{")   // array initializer {1,2,3}
                                                    exp = Expression.NewArrayInit(t, ParseParameters(ts, parseInfo, null, false));   // new array[] {x,y}
                                                else
                                                    exp = Expression.NewArrayBounds(t, param_list); // new array[x,y]
                                            }
                                        }
                                        break;
                                        #endregion
                                    case "typeof":
                                        #region typeof
                                        Assert(exp == null && ts.Next() != null && ts.Current.value == "(", tok); // must be "("
                                        Assert((exp = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null, tok);
                                        Assert(exp is ConstantExpression && ((ConstantExpression)exp).Value is Type, "Invalid parameter for typeof(T)"); // typeof(T)
                                        break;
                                        #endregion
                                    case "sizeof":
                                        #region sizeof
                                        Assert(exp == null, tok);
                                        Assert(ts.Next() != null && ts.Current.value == "(", tok); // must be "("
                                        Assert((exp = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null, tok);
                                        Assert(exp is ConstantExpression
                                            && ((ConstantExpression)exp).Value is Type
                                            && ((Type)((ConstantExpression)exp).Value).IsValueType, "Invalid parameter for sizeof(T)"); // sizeof(T)
                                        exp = Expression.Constant(System.Runtime.InteropServices.Marshal.SizeOf((Type)((ConstantExpression)exp).Value));
                                        break;
                                        #endregion
                                    case ".":
                                        #region Property, Field or method call
                                        Assert(exp != null && (tok = ts.Next()) != null && tok.tok_type == TOKEN_TYPE.IDENTIFIER, tok); // skip "." => tok=variable
                                        ts.Next();  // skip variable
                                        if (ts.CurrentValue == "(")  // method call
                                        {
                                            #region Method Call
                                            List<Type> param_types = new List<Type>();
                                            List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, false);

                                            if (exp is ConstantExpression && ((ConstantExpression)exp).Value is Type) // static call
                                            {
                                                t = (Type)(((ConstantExpression)exp).Value);
                                                Assert((mi = t.GetMethod(tok.value, BindingFlags.Static | BindingFlags.Public, null, param_types.ToArray(), null)) != null, tok);
                                                // check parameter implicit conversion
                                                ParameterInfo[] pis = mi.GetParameters();
                                                for (int idx = 0; idx < param_list.Count; idx++)
                                                {
                                                    Type et = (pis[idx].ParameterType.IsByRef) ? pis[idx].ParameterType.GetElementType() : pis[idx].ParameterType;
                                                    if (param_list[idx].Type != et)
                                                        param_list[idx] = Expression.Convert(param_list[idx], et);
                                                }
                                                exp = Expression.Call(mi, param_list.ToArray());
                                            }
                                            else // instance object method call
                                            {
                                                mi = exp.Type.GetMethod(tok.value, BindingFlags.Instance | BindingFlags.Public, null, param_types.ToArray(), null);
                                                Assert(mi != null, tok);
                                                // check parameter implicit conversion
                                                ParameterInfo[] pis = mi.GetParameters();
                                                for (int idx = 0; idx < param_list.Count; idx++)
                                                {
                                                    Type et = (pis[idx].ParameterType.IsByRef) ? pis[idx].ParameterType.GetElementType() : pis[idx].ParameterType;
                                                    if (param_list[idx].Type != et)
                                                        param_list[idx] = Expression.Convert(param_list[idx], et);
                                                }
                                                exp = Expression.Call(exp, mi, param_list.ToArray());
                                            }
                                            #endregion
                                        }
                                        else if (exp is ConstantExpression && ((ConstantExpression)exp).Value is Type)
                                        {
                                            #region Static property of field
                                            PropertyInfo pi = null; FieldInfo fi = null;
                                            t = (Type)(((ConstantExpression)exp).Value);
                                            if ((pi = t.GetProperty(tok.value, BindingFlags.Public | BindingFlags.Static)) != null)
                                                exp = Expression.Property(null, pi);
                                            else if ((fi = t.GetField(tok.value, BindingFlags.Public | BindingFlags.Static)) != null)
                                                exp = Expression.Field(null, fi);
                                            else if ((pi = exp.Type.GetProperty(tok.value, BindingFlags.Instance | BindingFlags.Public)) != null)
                                                exp = Expression.Property(exp, pi);
                                            else if ((fi = exp.Type.GetField(tok.value, BindingFlags.Instance | BindingFlags.Public)) != null)
                                                exp = Expression.Field(exp, fi);
                                            else if (ts.CurrentValue == "<")
                                            {
                                                // v1.1: Static Generic Method call
                                                List<Type> param_types = new List<Type>();
                                                List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, true);

                                                Assert((mi = t.GetMethod(tok.value, BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(param_types.ToArray())) != null, tok);
                                                //Assert((mi = mi.MakeGenericMethod(param_types.ToArray())) != null, tok);
                                                Assert(ts.CurrentValue == "(", ts.Current);    // to "("

                                                param_list = ParseParameters(ts, parseInfo, null, false);
                                                exp = Expression.Call(mi, param_list.ToArray());
                                            }
                                            else
                                                Assert(false, tok);
                                            #endregion
                                        }
                                        else
                                        {
                                            #region instance property or field
                                            PropertyInfo pi = null; FieldInfo fi = null;
                                            if ((pi = exp.Type.GetProperty(tok.value, BindingFlags.Instance | BindingFlags.Public)) != null)
                                                exp = Expression.Property(exp, pi);
                                            else if ((fi = exp.Type.GetField(tok.value, BindingFlags.Instance | BindingFlags.Public)) != null)
                                                exp = Expression.Field(exp, fi);
                                            else if (ts.CurrentValue == "<")
                                            {
                                                // v1.1: Instance Generic method call
                                                List<Type> param_types = new List<Type>();
                                                List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, true);
                                                Assert((mi = exp.Type.GetMethod(tok.value, BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(param_types.ToArray())) != null, tok);
                                                //Assert((mi = mi.MakeGenericMethod(param_types.ToArray())) != null, tok);
                                                Assert(ts.CurrentValue == "(", ts.Current);    // to "("
                                                param_list = ParseParameters(ts, parseInfo, null, false);
                                                exp = Expression.Call(exp, mi, param_list.ToArray());
                                            }
                                            else
                                                Assert(false, tok);
                                            #endregion
                                        }
                                        break;
                                        #endregion
                                    default:
                                        // ,(not in method and indexer) and ;(need ASP.NET4.0)
                                        Assert(false, tok);
                                        break;
                                }
                                break;
                                #endregion
                        }
                        break;
                        #endregion
                    default:
                        Assert(false, tok);
                        break;
                }

                if (ts.Current == tok)
                    ts.Next();
            }
            return exp;
        }

        internal Type ParseStaticTypeExtension(TokenStore ts, Type t, bool allowArrayType)
        {
            // check nullable
            if (t.IsValueType && ts.CurrentValue == "?" && NullableType.ContainsKey(t))
            {
                t = NullableType[t];
                ts.Next();  // skip "?"
            }
            // check nested type
            Type t2 = null;
            while (ts.CurrentValue == "."
                && ts.PeekToken(1) != null && ts.PeekToken(1).tok_type == TOKEN_TYPE.IDENTIFIER
                && (t2 = t.GetNestedType(ts.PeekToken(1).value)) != null)
            {
                t = t2;
                ts.Next();  // skip '.'
                ts.Next();  // skip nested type
            }
            // check array: t[,][], if after new operater, do not check array here
            if (allowArrayType)
            {
                while (ts.CurrentValue == "[")
                {
                    StringBuilder arr = new StringBuilder("[");
                    AToken tok = ts.Next(); // skip "["
                    while ((tok = ts.Current) != null && tok.value != "]") arr.Append(tok.value);
                    Assert((t = t.Assembly.GetType(t.FullName + arr.Append("]").ToString())) != null, tok);
                    Assert(tok.value == "]", tok);
                    tok = ts.Next();  // skip "]"
                }
            }
            return t;
        }
        internal List<Expression> ParseParameters(TokenStore ts, ParseInfo parseInfo, List<Type> param_types, bool typeByValue)
        {
            TokenStore sub_ts = ts.NextUntilOperatorClose();
            List<Expression> param_list = new List<Expression>();
            while (sub_ts.Current != null)
            {
                bool isRef = false;
                if (sub_ts.CurrentValue == "ref" || sub_ts.CurrentValue == "out")
                {
                    isRef = true;
                    sub_ts.Next();
                }
                Expression e = ParseNextExpression(sub_ts, AnOperator.dicSystemOperator[","], parseInfo);
                param_list.Add(e);
                if (param_types != null)
                {
                    if (isRef)
                    {
                        if (typeByValue)
                            param_types.Add(((Type)((ConstantExpression)e).Value).MakeByRefType());
                        else
                            param_types.Add(e.Type.MakeByRefType());
                    }
                    else
                    {
                        if (typeByValue)
                            param_types.Add((Type)((ConstantExpression)e).Value);
                        else
                            param_types.Add(e.Type);
                    }
                }
                Assert(sub_ts.Current == null || sub_ts.CurrentValue == "," && sub_ts.Next() != null, sub_ts.Current);    // skip ','
            }
            //if (param_types != null)
            //{
            //    if(typeByValue)
            //        foreach (Expression e in param_list) param_types.Add((Type)((ConstantExpression)e).Value);
            //    else
            //        foreach (Expression e in param_list) param_types.Add(e.Type);
            //}
            return param_list;
        }

        internal Expression ParseReferredParameterOrType(TokenStore ts, ParseInfo parseInfo, bool allowType, bool allowVariable, bool allowArrayType, bool testOnly = false)
        {
            Type t = null;
            Expression exp = null;
            StringBuilder var_fullname = new StringBuilder();
            TokenStore tsClone = testOnly ? ts.Clone() as TokenStore : ts;
            AToken tok = tsClone.Current;
            while (exp == null && tok != null && tok.tok_type == TOKEN_TYPE.IDENTIFIER)
            {
                var_fullname.Append(tok.value);
                // try local variable
                if (allowVariable && (exp = parseInfo.GetReferredVariable(var_fullname.ToString())) != null) 
                {
                    tsClone.Next(); 
                    break; 
                }
                // try referred parameter
                else if (allowVariable && (t = this.QueryParameterType(var_fullname.ToString())) != null)
                {
                    exp = parseInfo.GetReferredParameter(t, tok.value);
                    tsClone.Next();
                    break;
                }
                // try static Generic Type
                else if (allowType && tsClone.PeekTokenValue(1) == "<")
                {
                    // v1.1: Generic Type reference
                    tok = tsClone.Next();    // to "<"
                    List<Type> param_types = new List<Type>();
                    List<Expression> param_list = ParseParameters(tsClone, parseInfo, param_types, true);
                    Assert((t = this.QueryStaticType(var_fullname.Append("`").Append(param_list.Count).ToString()).MakeGenericType(param_types.ToArray())) != null, tok);
                    t = ParseStaticTypeExtension(tsClone, t, allowArrayType);
                    exp = Expression.Constant(t);   // Type: (T)x or T.xxx
                    break;
                }
                // try static Type
                else if (allowType && (t = this.QueryStaticType(var_fullname.ToString())) != null)
                {
                    // Static Type reference
                    tsClone.Next();
                    t = ParseStaticTypeExtension(tsClone, t, allowArrayType);
                    exp = Expression.Constant(t);
                    break;
                }
                // neither parameter or static type
                if ((tok = tsClone.Next()) == null || tok.value != ".") 
                    break;   // unexpected
                var_fullname.Append(".");
                tok = tsClone.Next();    // skip "."
            }
            return exp;
        }

        public object Run(LambdaExpression lambda, params object[] inline_parameter_values)
        {
            Delegate f = lambda.Compile();
            if (inline_parameter_values != null && inline_parameter_values.Length > 0)
                return f.DynamicInvoke(inline_parameter_values);

            List<object> parameterList = new List<object>();
            if (lambda.Parameters != null && lambda.Parameters.Count > 0)
            {
                foreach (ParameterExpression pe in lambda.Parameters)
                {
                    if (this.ParameterValue.ContainsKey(pe.Name))
                        parameterList.Add(this.ParameterValue[pe.Name]);
                    else if (GlobalParameterValue.ContainsKey(pe.Name))
                        parameterList.Add(GlobalParameterValue[pe.Name]);
                    else
                        throw new ExprException(string.Format("The value for {0} is undefined.", pe.Name));
                }
            }
            return f.DynamicInvoke(parameterList.ToArray());
        }

        private void Assert(bool check, AToken tok)
        {
            if (!check)
                if (tok == null)
                    throw new ExprException("Unexpected end.");
                else
                    throw new ExprException(string.Format("Unexpected token({0}) at: {1}.", tok.value, tok.start_pos + 1));
        }
        private void Assert(bool check, string message)
        {
            if (!check)
                throw new ExprException(message);
        }
    }

    internal class ParseInfo
    {
        public LabelExpression returnLabel = null;
        public List<ParameterExpression> referredParameterList = new List<ParameterExpression>();
        public Stack<Dictionary<string, ParameterExpression>> localVariableStack = new Stack<Dictionary<string, ParameterExpression>>();   // local variables in every statement block
        public Stack<JumpInfo> jumpInfoStack = new Stack<JumpInfo>();

        public Expression GetReferredVariable(string var_name)
        {
            foreach (Dictionary<string, ParameterExpression> dic in localVariableStack)
                if (dic.ContainsKey(var_name)) return dic[var_name];
            return null;
        }
        public Expression GetReferredParameter(Type t, string name)
        {
            Expression exp = null;
            // Parameter reference
            foreach (ParameterExpression pe in referredParameterList)
            {
                if (pe.Type == t && pe.Name == name) { exp = pe; break; }
            }
            if (exp == null)
            {
                exp = Expression.Parameter(t, name);
                referredParameterList.Add((ParameterExpression)exp);
            }
            return exp;
        }
    }
    internal class JumpInfo
    {
        public LabelExpression breakLabel;
        public LabelExpression continueLabel;
    }

    public class Using
    {
        internal Dictionary<string, Assembly> namespace_list = new Dictionary<string, Assembly>();
        public Using()
        {
            Add("System");
        }
        public void Add(string namespace_name)
        {
            if (namespace_list.ContainsKey(namespace_name))
                namespace_list[namespace_name] = null;
            else
                namespace_list.Add(namespace_name, null);
        }
        public void Add(string namespace_name, string assembly_name)
        {
            foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assem.FullName.StartsWith(assembly_name, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (namespace_list.ContainsKey(namespace_name))
                        namespace_list[namespace_name] = assem;
                    else
                        namespace_list.Add(namespace_name, assem);
                    return;
                }
            }
            throw new ExprException("Can't find assembly.");
        }
    }

    internal class TokenStore : ICloneable
    {
        internal string original_string;
        internal List<AToken> token_list;
        internal int current_token_idx;

        private TokenStore(string exp_string, List<AToken> token_list)
        {
            this.original_string = exp_string;
            this.token_list = token_list;
            current_token_idx = 0;
        }

        internal static TokenStore Parse(string exp_string, out TokenStore declare_ts)
        {
            List<AToken> list = new List<AToken>();
            List<AToken> declare_list = null;
            int start_pos = 0;
            TOKEN_TYPE tok_type = TOKEN_TYPE.NONE;
            bool isHex = false;         // 0xFFFF
            bool isExponent = false;    // 1e10, 1E-2
            bool isVerbatim = false;    // @"xxx"
            char start_quotation = '"';
            StringBuilder text_value = new StringBuilder();
            // blank : c <= ' '
            // text: '"'
            // number : 0-9
            // decimal : .
            // variable : A-Z,a-z,_
            // operator: else
            for (int idx = 0; idx < exp_string.Length; idx++)
            {
                char c = exp_string[idx];
                char c2 = (idx + 1) < exp_string.Length ? exp_string[idx + 1] : (char)0;
                switch (tok_type)
                {
                    case TOKEN_TYPE.NONE:
                        #region NONE
                        if (c <= ' ') continue; // skip space
                        start_pos = idx;
                        if (c == '/' && c2 == '/') { idx++; tok_type = TOKEN_TYPE.COMMENT; }
                        else if (c == '/' && c2 == '*') { idx++; tok_type = TOKEN_TYPE.COMMENT_BLOCK; }
                        else if (c == '@' && (c2 == '"' || c2 == '\'')) { idx++; start_quotation = c2; tok_type = TOKEN_TYPE.TEXT; isVerbatim = true; }
                        else if (c == '"' || c == '\'') { start_quotation = c; tok_type = TOKEN_TYPE.TEXT; }
                        else if (c == '0' && (c2 == 'x' || c2 == 'X')) { idx++; tok_type = TOKEN_TYPE.INT; isHex = true; }
                        else if (c >= '0' && c <= '9') tok_type = TOKEN_TYPE.INT;
                        else if (c == '.')
                        {
                            if (c2 >= '0' && c2 <= '9')
                                tok_type = TOKEN_TYPE.DOUBLE;  // no leading digit number
                            else
                                tok_type = TOKEN_TYPE.OPERATOR; // (x).y; x[idx].y
                        }
                        else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_') tok_type = TOKEN_TYPE.IDENTIFIER;
                        else tok_type = TOKEN_TYPE.OPERATOR;
                        break;
                        #endregion NONE
                    case TOKEN_TYPE.COMMENT:    // // xxxx \n
                        if (c == '\n') tok_type = TOKEN_TYPE.NONE;  // skip eveything until new line;
                        break;
                    case TOKEN_TYPE.COMMENT_BLOCK:  // /* xxxx */
                        if (c == '*' && c2 == '/') { idx++; tok_type = TOKEN_TYPE.NONE; }   // skip eveything until "*/";
                        break;
                    case TOKEN_TYPE.TEXT:   // "xxxx" or 'xxxx'
                        #region TEXT
                        if (!isVerbatim && c == '\\')  // escape for string
                        {
                            idx++;  // use c2
                            switch (c2)
                            {
                                case '0': text_value.Append('\0'); break;   // \0
                                case 'n': text_value.Append('\n'); break;   // new line
                                case 't': text_value.Append('\t'); break;   // tab
                                case 'r': text_value.Append('\r'); break;   // carriage return
                                case 'a': text_value.Append('\a'); break;   // alert
                                case 'b': text_value.Append('\b'); break;   // back space
                                case 'v': text_value.Append('\v'); break;   // vertical tab
                                case 'f': text_value.Append('\f'); break;   // Form Feed
                                case 'x': // hexadecimal \xF \xFF \xFFF \xFFFF
                                case 'u': // unicode \uFFFF
                                case 'U': // unicode \UFFFFFFFF
                                    {
                                        int additional_digits = 0;
                                        while (additional_digits < (c2 == 'U' ? 8 : 4)
                                            && (c = exp_string.Length > idx + additional_digits + 1 ? exp_string[idx + additional_digits + 1] : (char)0) != (char)0
                                            && (c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f'))
                                            additional_digits++;
                                        if (additional_digits == 0 || c2 == 'u' && additional_digits < 4 || c2 == 'U' && additional_digits < 8)
                                            throw new ExprException("Expecting hexadecimal for operator: '\\" + c2 + "'");
                                        else
                                        {
                                            text_value.Append((char)Convert.ToInt16("0x" + exp_string.Substring(idx + 1, additional_digits), 16));
                                            idx += additional_digits;
                                        }
                                    }
                                    break;
                                default: text_value.Append(c2); break;   // '\\', '\"', '\''
                            }
                        }
                        else if (isVerbatim && c == start_quotation && c2 == start_quotation) // escape for verbatim string
                        {
                            text_value.Append(c);
                            idx++;
                        }
                        else if (c == start_quotation)  // end text
                        {
                            list.Add(new AToken(start_pos, TOKEN_TYPE.TEXT, null, text_value.ToString()));
                            text_value = new StringBuilder();
                            tok_type = TOKEN_TYPE.NONE;
                            isVerbatim = false;
                        }
                        else
                            text_value.Append(c);
                        break;
                        #endregion TEXT
                    case TOKEN_TYPE.INT:
                        #region INT
                        if (c >= '0' && c <= '9') { }
                        else if (isHex && (c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F')) { }
                        else if (c == '.' && !isHex) tok_type = TOKEN_TYPE.DOUBLE;
                        else if ((c == 'e' || c == 'E') && !isHex) { tok_type = TOKEN_TYPE.DOUBLE; isExponent = true; }
                        else
                        {
                            string val = isHex ? Convert.ToUInt64(exp_string.Substring(start_pos, idx - start_pos), 16).ToString() : exp_string.Substring(start_pos, idx - start_pos);
                            if (c <= ' ')  // space, \t, \n, ... control charaters
                            {
                                list.Add(new AToken(start_pos, tok_type, null, val));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c == '/' && c2 == '/')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, val));
                                tok_type = TOKEN_TYPE.COMMENT;
                                idx++;
                            }
                            else if (c == '/' && c2 == '*')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, val));
                                tok_type = TOKEN_TYPE.COMMENT_BLOCK;
                                idx++;
                            }
                            else if (c == 'U' || c == 'u')
                            {
                                if (c2 == 'L' || c2 == 'l')
                                {
                                    list.Add(new AToken(start_pos, TOKEN_TYPE.ULONG, null, val));
                                    tok_type = TOKEN_TYPE.NONE;
                                    idx++;
                                }
                                else
                                {
                                    list.Add(new AToken(start_pos, TOKEN_TYPE.UINT, null, val));
                                    tok_type = TOKEN_TYPE.NONE;
                                }
                            }
                            else if (c == 'L' || c == 'l')
                            {
                                if (c2 == 'U' || c2 == 'u')
                                {
                                    list.Add(new AToken(start_pos, TOKEN_TYPE.ULONG, null, val));
                                    tok_type = TOKEN_TYPE.NONE;
                                    idx++;
                                }
                                else
                                {
                                    list.Add(new AToken(start_pos, TOKEN_TYPE.LONG, null, val));
                                    tok_type = TOKEN_TYPE.NONE;
                                }
                            }
                            else if (!isHex && (c == 'F' || c == 'f'))
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.FLOAT, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (!isHex && (c == 'D' || c == 'd'))
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.DOUBLE, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (!isHex && (c == 'M' || c == 'm'))
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.DECIMAL, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, val));
                                start_pos = idx;
                                tok_type = TOKEN_TYPE.IDENTIFIER; // report error later
                            }
                            else
                            {
                                // operator, add previous token
                                list.Add(new AToken(start_pos, tok_type, null, val));
                                start_pos = idx;
                                tok_type = TOKEN_TYPE.OPERATOR;
                            }
                            isHex = false;
                        }
                        break;
                        #endregion NUMBER
                    case TOKEN_TYPE.DOUBLE:
                        #region DOUBLE
                        if (c >= '0' && c <= '9') { }
                        else if ((c == '-' || c == '+') && isExponent) { }  // 1e-2. can only have one sign
                        else
                        {
                            if (c <= ' ')  // space, \t, \n, ... control charaters
                            {
                                list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c == '/' && c2 == '/')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.COMMENT;
                                idx++;
                            }
                            else if (c == '/' && c2 == '*')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.COMMENT_BLOCK;
                                idx++;
                            }
                            else if (c == 'F' || c == 'f')
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.FLOAT, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c == 'D' || c == 'd')
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.DOUBLE, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c == 'M' || c == 'm')
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.DECIMAL, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, idx - start_pos)));
                                start_pos = idx;
                                tok_type = TOKEN_TYPE.IDENTIFIER; // report error later
                            }
                            else
                            {
                                // token change, save previous token
                                list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, idx - start_pos)));
                                start_pos = idx;
                                tok_type = TOKEN_TYPE.OPERATOR;
                            }
                        }
                        isExponent = false; // can only have one sign right after 'e'
                        break;
                        #endregion DOUBLE
                    case TOKEN_TYPE.IDENTIFIER:
                        #region VARIABLE
                        // VARIABLE(x or ns.x) can be a token of
                        //      - boolean:      true / false    => BOOL
                        //      - null:         null            => TEXT
                        //      - is                            => operator
                        //      - new                           => operator
                        //      - property:     .x              => VARIABLE. continue to see operator '[' and '(' for x[] and x()
                        //      - static type:  ns.x            => TYPE. continue to see operator '[' and '(' for x[] and x()
                        //      - parameter:    x               => VARIABLE
                        //      - un-typed parameter: error     => VARIABLE, will report error later
                        if (c >= '0' && c <= '9' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_') { }
                        else
                        {
                            string variable_name = exp_string.Substring(start_pos, idx - start_pos);
                            if (variable_name == "true" || variable_name == "false") // boolean
                                list.Add(new AToken(start_pos, TOKEN_TYPE.BOOL, null, variable_name));
                            else if (variable_name == "null")  // null
                                list.Add(new AToken(start_pos, TOKEN_TYPE.TEXT, null, null));
                            else if (variable_name == "is" // is
                                || variable_name == "as"    // as
                                || variable_name == "new" // new
                                || variable_name == "typeof" // typeof
                                || variable_name == "sizeof" // sizeof
                                )
                                ParseOperatorTokens(start_pos, variable_name, ref list, ref declare_list);
                            else // un-typed parameter, will return error later
                                list.Add(new AToken(start_pos, tok_type, null, variable_name));

                            // set next token type
                            if (c <= ' ') tok_type = TOKEN_TYPE.NONE;
                            else if (c == '/' && c2 == '/')
                            {
                                tok_type = TOKEN_TYPE.COMMENT;
                                idx++;
                            }
                            else if (c == '/' && c2 == '*')
                            {
                                tok_type = TOKEN_TYPE.COMMENT_BLOCK;
                                idx++;
                            }
                            else
                                tok_type = TOKEN_TYPE.OPERATOR;
                            start_pos = idx;
                        }
                        break;
                        #endregion VARIABLE
                    case TOKEN_TYPE.OPERATOR:
                        #region OPERATOR
                        if (c <= ' ')  // space, \t, \n, ... control charaters
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            tok_type = TOKEN_TYPE.NONE;
                        }
                        else if (c == '/' && c2 == '/')
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            tok_type = TOKEN_TYPE.COMMENT;
                            idx++;
                        }
                        else if (c == '/' && c2 == '*')
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            tok_type = TOKEN_TYPE.COMMENT_BLOCK;
                            idx++;
                        }
                        else if (c == '0' && (c2 == 'x' || c2 == 'X'))
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            start_pos = idx;
                            isHex = true;
                            tok_type = TOKEN_TYPE.INT;
                            idx++;
                        }
                        else if (c >= '0' && c <= '9')
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            start_pos = idx;
                            tok_type = TOKEN_TYPE.INT;
                        }
                        else if (c == '@' && (c2 == '"' || c2 == '\''))
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            idx++;
                            start_pos = idx;    // start from " instead of @
                            tok_type = TOKEN_TYPE.TEXT;
                            start_quotation = c2;
                            isVerbatim = true;
                        }
                        else if (c == '"' || c == '\'')
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            start_pos = idx;
                            tok_type = TOKEN_TYPE.TEXT;
                            start_quotation = c;
                        }
                        else if (c == '.' && c2 >= '0' && c2 <= '9')  // no leading digit decimal, like .999
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            start_pos = idx;
                            OPERATOR_TYPE previous_token_op_type = (list.Count > 0 && list[list.Count - 1].op != null) ? list[list.Count - 1].op.op_type : OPERATOR_TYPE.UNKNOWN;
                            if (previous_token_op_type == OPERATOR_TYPE.CLOSE)
                                tok_type = TOKEN_TYPE.OPERATOR; // (x).y; x[idx].y
                            else
                                tok_type = TOKEN_TYPE.DOUBLE;  // no leading digit number
                        }
                        else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_')
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            start_pos = idx;
                            tok_type = TOKEN_TYPE.IDENTIFIER;
                        }
                        else { } // continue operator
                        break;
                        #endregion OPERATOR
                }
            }
            if (tok_type == TOKEN_TYPE.COMMENT_BLOCK || tok_type == TOKEN_TYPE.TEXT)
                throw new ExprException("Unexpected end of the expression.");
            else if (tok_type != TOKEN_TYPE.NONE && tok_type != TOKEN_TYPE.COMMENT)
            {
                if (tok_type == TOKEN_TYPE.OPERATOR)
                    ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, exp_string.Length - start_pos), ref list, ref declare_list);
                else
                    list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, exp_string.Length - start_pos)));
            }
            declare_ts = declare_list != null && declare_list.Count > 0 ? new TokenStore(exp_string, declare_list) : null;
            return new TokenStore(exp_string, list);
        }

        // for operator
        internal static void ParseOperatorTokens(int start_pos, string operator_string, ref List<AToken> list, ref List<AToken> declare_list)
        {
            int op_start = 0;
            int op_len = operator_string.Length;
            // need check Prefix Unary operator if there is no operand before or there is non-CLOSE operator before
            AToken lastToken = list.Count > 0 ? list[list.Count - 1] : null;
            bool checkPreUnary = lastToken == null || lastToken.op != null && lastToken.op.op_type != OPERATOR_TYPE.CLOSE;

            while (op_len > 0)
            {
                string op_str = operator_string.Substring(op_start, op_len);
                AnOperator exp_op = null;

                if (checkPreUnary && AnOperator.dicSystemOperator.ContainsKey(op_str + "U"))
                    exp_op = AnOperator.dicSystemOperator[op_str + "U"];
                if (exp_op == null && AnOperator.dicSystemOperator.ContainsKey(op_str))
                    exp_op = AnOperator.dicSystemOperator[op_str];
                if (exp_op != null)
                {
                    //if (op_str == "(" && lastToken != null && lastToken.tok_type == TOKEN_TYPE.VARIABLE) lastToken.tok_type = TOKEN_TYPE.FUNC;
                    AToken tok = new AToken(start_pos + op_start, TOKEN_TYPE.OPERATOR, exp_op, op_str);
                    if (tok.value == "=>" && declare_list == null)
                    {
                        declare_list = list;
                        list = new List<AToken>();
                        lastToken = null;
                    }
                    else
                    {
                        list.Add(tok);
                        lastToken = tok;
                    }
                    op_start += op_len;
                    op_len = operator_string.Length - op_start;
                    checkPreUnary = exp_op.op_type != OPERATOR_TYPE.CLOSE;
                }
                else
                    op_len--;
            }
            if (op_len == 0 && op_start < operator_string.Length)
            {
                // unknown operator
                string op_str = operator_string.Substring(op_start);
                list.Add(new AToken(start_pos + op_start, TOKEN_TYPE.OPERATOR, new AnOperator(OPERATOR_TYPE.UNKNOWN, 0), op_str));
            }
        }
        //internal bool IsEnd() { return current_token_idx >= token_list.Count; }
        internal AToken Current { get { return current_token_idx >= token_list.Count ? null : token_list[current_token_idx]; } }
        internal string CurrentValue { get { return current_token_idx >= token_list.Count ? null : token_list[current_token_idx].value; } }
        //internal AToken Previous { get { return current_token_idx > 0 && token_list.Count > 0 ? token_list[current_token_idx - 1] : null; } }
        internal AToken Next()
        {
            if (current_token_idx < token_list.Count) 
                current_token_idx++;
            return current_token_idx < token_list.Count ? token_list[current_token_idx] : null;
        }
        internal AToken PeekToken(int offsetIdx)
        {
            return (current_token_idx + offsetIdx >= 0 && current_token_idx + offsetIdx < token_list.Count) ? token_list[current_token_idx + offsetIdx] : null;
        }
        internal string PeekTokenValue(int offsetIdx)
        {
            AToken tok = this.PeekToken(offsetIdx);
            return tok == null ? null : tok.value;
        }
        internal TokenStore NextUntilOperatorClose()
        {
            AToken start_tok = this.Current;
            int level = 1;
            List<AToken> list = new List<AToken>();
            for (this.current_token_idx++; this.current_token_idx < this.token_list.Count; this.current_token_idx++)
            {
                AToken curr_tok = this.token_list[this.current_token_idx];
                if (curr_tok.op != null)
                {
                    if (start_tok.value == curr_tok.value) level++;
                    else if (start_tok.value == "(" && curr_tok.value == ")" // ( )
                            || start_tok.value == "[" && curr_tok.value == "]"       // [ ]
                            || start_tok.value == "{" && curr_tok.value == "}"         // { }
                            || start_tok.value == "<" && curr_tok.value == ">")          // < >
                        level--;
                    else if (start_tok.value == "<" && curr_tok.value == ">>" && level >= 2)    // v<xxx<xx>>
                    {
                        level -= 2;
                        list.Add(new AToken(curr_tok.start_pos, TOKEN_TYPE.OPERATOR, AnOperator.dicSystemOperator[">"], ">"));
                        curr_tok = new AToken(curr_tok.start_pos + 1, TOKEN_TYPE.OPERATOR, AnOperator.dicSystemOperator[">"], ">");
                    }
                    if (level == 0) break;
                }
                list.Add(curr_tok);
            }
            if (level != 0) throw new ExprException("Expecting closing operator for operator: '" + start_tok.value + "'");
            this.current_token_idx++;   // move to next token after close
            return new TokenStore(this.original_string, list);
        }

        public AToken PreviousToken
        {
            get
            {
                if (current_token_idx <= 0)
                    return null;
                return token_list[current_token_idx - 1];
            }
        }

        public object Clone()
        {
            TokenStore clone = new TokenStore(this.original_string, this.token_list.ToList());
            clone.current_token_idx = this.current_token_idx;
            return clone;
        }
    }

    internal class AToken
    {
        internal int start_pos;
        //internal int end_pos;
        internal TOKEN_TYPE tok_type;
        internal AnOperator op;
        internal string value;

        internal AToken(int start_pos/*, int end_pos*/, TOKEN_TYPE tok_type, AnOperator op, string value)
        {
            this.start_pos = start_pos;
            //this.end_pos = end_pos;
            this.tok_type = tok_type;
            this.op = op;
            this.value = value;
        }


    }
    internal delegate Expression d1m(Expression exp, MethodInfo mi);
    internal delegate Expression d2(Expression exp1, Expression exp2);
    internal delegate Expression d2m(Expression exp1, Expression exp2, MethodInfo mi);
    internal delegate Expression d2bm(Expression exp1, Expression exp2, bool LiftToNull, MethodInfo mi);

    internal enum RequiredOperandType { NONE, SAME }
    internal class AnOperator
    {
        //public OPERATOR_NAME op_name;
        public OPERATOR_TYPE op_type;
        public int precedence;
        public Type required1stType;
        public Type required2ndType;
        public RequiredOperandType requiredOperandType;
        public Delegate exp_call;
        public string overload_name;    // http://msdn.microsoft.com/en-us/library/2sk3x8a7.aspx

        public AnOperator(OPERATOR_TYPE op_type, int precedence) : this(op_type, precedence, null, null, RequiredOperandType.NONE, null, null) { }
        public AnOperator(OPERATOR_TYPE op_type, int precedence, Delegate exp_call, string overload_name) : this(op_type, precedence, null, null, RequiredOperandType.NONE, exp_call, overload_name) { }
        public AnOperator(OPERATOR_TYPE op_type, int precedence, Type required1stType, Type required2ndType, RequiredOperandType requiredOperandType, Delegate exp_call, string overload_name)
        {
            //this.op_name = op_name;
            this.op_type = op_type;
            this.precedence = precedence;
            this.required1stType = required1stType;
            this.required2ndType = required2ndType;
            this.requiredOperandType = requiredOperandType;
            this.exp_call = exp_call;
            this.overload_name = overload_name;
        }

        internal static Dictionary<string, AnOperator> dicSystemOperator = GetSystemOperators();
        internal static Dictionary<string, AnOperator> GetSystemOperators()
        {
            Dictionary<string, AnOperator> dic = new Dictionary<string, AnOperator>();
            // special
            dic.Add(";", new AnOperator(OPERATOR_TYPE.PRIMARY, 999)); // Semicolon,   // end of statement; lowest priority
            dic.Add(",", new AnOperator(OPERATOR_TYPE.PRIMARY, 990));  // "op_Comma")); // end of parameter. ex. x,y
            dic.Add("[", new AnOperator(OPERATOR_TYPE.OPEN, 0, null, "get_Item")); // BracketLeft,     //a[x]	0.ArrayAccess; ArrayIndex; ArrayLength
            dic.Add("]", new AnOperator(OPERATOR_TYPE.CLOSE, 0)); // BracketRight,     //a[x]	0.ArrayAccess; ArrayIndex; ArrayLength
            dic.Add("(", new AnOperator(OPERATOR_TYPE.OPEN, 0)); // ParenthesisLeft,    //f(x) 0.Call; (T)x	10.??; () 200.??
            dic.Add(")", new AnOperator(OPERATOR_TYPE.CLOSE, 0)); // ParenthesisRight,    //f(x) 0.Call; (T)x	10.??; () 200.??
            dic.Add("{", new AnOperator(OPERATOR_TYPE.OPEN, 0)); // BlockLeft,      // { block; }
            dic.Add("}", new AnOperator(OPERATOR_TYPE.CLOSE, 0)); // BlockRight,      // { block; }
            dic.Add(".", new AnOperator(OPERATOR_TYPE.PRIMARY, 0)); // Dot,                           //x.y	0.FieldOrProperty; x.y(); x.y[]
            dic.Add("new", new AnOperator(OPERATOR_TYPE.PRIMARY, 0));    // new
            dic.Add("typeof", new AnOperator(OPERATOR_TYPE.PRIMARY, 0));    // TypeOf
            //dic.Add("as", new AnOperator(OPERATOR_TYPE.PRIMARY, 0)); // var as T : Expression.TypeAs(exp, Type)
            dic.Add("sizeof", new AnOperator(OPERATOR_TYPE.PRIMARY, 1));   // SizeOf, it should be PREFIX_UNARY

            dic.Add("=>", new AnOperator(OPERATOR_TYPE.PRIMARY, 0));  // LINQ goes to
            //[Unary] : U - unary operator, xU-prefix operator, only available when there is no previous operand; 
            dic.Add("(T)", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 1));   // Cast
            dic.Add("+U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.UnaryPlus, "op_UnaryPlus")); // UnaryPlus,                     //+x		10.UnaryPlus			// Unary
            dic.Add("-U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.Negate, "op_UnaryNegation")); // UnaryMinus,                    //-x		10.Negate				// Unary
            dic.Add("!U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.Not, null)); // UnaryLogicalNot,               //!		10.Not					// Unary
            dic.Add("~U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.Not, "op_OnesComplement")); // UnaryBitNot,                   //~		10.Not					// Unary
            dic.Add("--", new AnOperator(OPERATOR_TYPE.POST_UNARY, 0, (d1m)Expression.PostDecrementAssign, "op_Decrement")); // PostDecrementAssign,    ////x--		0.PostDecrementAssign	// Unary
            dic.Add("++", new AnOperator(OPERATOR_TYPE.POST_UNARY, 0, (d1m)Expression.PostIncrementAssign, "op_Increment")); // PostIncrementAssign,    ////x++		0.PostIncrementAssign	// Unary
            dic.Add("--U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.PreDecrementAssign, null)); // PreDecrementAssign,     ////--x		10.PreDecrementAssign   // Unary
            dic.Add("++U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.PreIncrementAssign, null)); // PreIncrementAssign,     ////++x		10.PreIncrementAssign   // Unary
            //[Binary]
            dic.Add("**", new AnOperator(OPERATOR_TYPE.BINARY, 15, typeof(double), typeof(double), RequiredOperandType.NONE, (d2)Expression.Power, null)); // Power,     //x**y	15.Power
            dic.Add("*", new AnOperator(OPERATOR_TYPE.BINARY, 20, null, null, RequiredOperandType.SAME, (d2m)Expression.Multiply, "op_Multiply")); // Multiply,     //x*y		20.Multiply
            dic.Add("/", new AnOperator(OPERATOR_TYPE.BINARY, 20, null, null, RequiredOperandType.SAME, (d2m)Expression.Divide, "op_Division")); // Divide,    //x/y		20.Divide
            dic.Add("%", new AnOperator(OPERATOR_TYPE.BINARY, 20, null, null, RequiredOperandType.SAME, (d2m)Expression.Modulo, "op_Modulus")); // Modulo,      //x%y		20.Modulo(Reminder)
            dic.Add("+", new AnOperator(OPERATOR_TYPE.BINARY, 30, null, null, RequiredOperandType.SAME, (d2m)Expression.Add, "op_Addition")); // Add,   //x+y		30.Add
            dic.Add("-", new AnOperator(OPERATOR_TYPE.BINARY, 30, null, null, RequiredOperandType.SAME, (d2m)Expression.Subtract, "op_Subtraction")); // Subtract,  //x-y		30.Subtract
            dic.Add("<<", new AnOperator(OPERATOR_TYPE.BINARY, 40, null, typeof(int), RequiredOperandType.NONE, (d2m)Expression.LeftShift, "op_LeftShift")); // LeftShift,   //x<<y	40.LeftShift
            dic.Add(">>", new AnOperator(OPERATOR_TYPE.BINARY, 40, null, typeof(int), RequiredOperandType.NONE, (d2m)Expression.RightShift, "op_RightShift")); // RightShift,    //x>>y	40.RightShift
            dic.Add(">", new AnOperator(OPERATOR_TYPE.BINARY, 50, null, null, RequiredOperandType.SAME, (d2bm)Expression.GreaterThan, "op_GreaterThan")); // GreaterThan,   //x>y		50.GreaterThan
            dic.Add(">=", new AnOperator(OPERATOR_TYPE.BINARY, 50, null, null, RequiredOperandType.SAME, (d2bm)Expression.GreaterThanOrEqual, "op_GreaterThanOrEqual")); // GreaterThanOrEqual,    //x>=y	50.GreaterThanOrEqual
            dic.Add("<", new AnOperator(OPERATOR_TYPE.BINARY, 50, null, null, RequiredOperandType.SAME, (d2bm)Expression.LessThan, "op_LessThan")); // LessThan,  //x<y		50.LessThan
            dic.Add("<=", new AnOperator(OPERATOR_TYPE.BINARY, 50, null, null, RequiredOperandType.SAME, (d2bm)Expression.LessThanOrEqual, "op_LessThanOrEqual")); // LessThanOrEqual,   //x<=y	50.LessThanOrEqual
            dic.Add("is", new AnOperator(OPERATOR_TYPE.BINARY, 50)); // is,   //x is T	50.TypeIs
            dic.Add("as", new AnOperator(OPERATOR_TYPE.BINARY, 50)); // as,   //x as T	50.TypeAs
            dic.Add("==", new AnOperator(OPERATOR_TYPE.BINARY, 60, null, null, RequiredOperandType.SAME, (d2bm)Expression.Equal, "op_Equality")); // Equal, //x==y	30.Equal
            dic.Add("!=", new AnOperator(OPERATOR_TYPE.BINARY, 60, null, null, RequiredOperandType.SAME, (d2bm)Expression.NotEqual, "op_Inequality")); // NotEqual,  //x!=y	30.NotEqual
            dic.Add("&", new AnOperator(OPERATOR_TYPE.BINARY, 70, null, null, RequiredOperandType.SAME, (d2m)Expression.And, "op_BitwiseAnd")); // BitAnd,   //x&y		30.And
            dic.Add("^", new AnOperator(OPERATOR_TYPE.BINARY, 72, null, null, RequiredOperandType.SAME, (d2m)Expression.ExclusiveOr, "op_ExclusiveOr")); // BitExclusiveOr,    //x^y		80.ExclusiveOr(XOR)
            dic.Add("|", new AnOperator(OPERATOR_TYPE.BINARY, 74, null, null, RequiredOperandType.SAME, (d2m)Expression.Or, "op_BitwiseOr")); // BitOr, //x|y		90.Or
            dic.Add("&&", new AnOperator(OPERATOR_TYPE.BINARY, 80, null, null, RequiredOperandType.NONE, (d2m)Expression.AndAlso, "op_LogicalAnd")); // And,   //x&&y	100.AndAlso
            dic.Add("||", new AnOperator(OPERATOR_TYPE.BINARY, 85, null, null, RequiredOperandType.NONE, (d2m)Expression.OrElse, "op_LogicalOr")); // Or,    //x||y	110.OrElse
            dic.Add("??", new AnOperator(OPERATOR_TYPE.BINARY, 90, null, null, RequiredOperandType.NONE, (d2)Expression.Coalesce, null)); // Coalesce,  //x??y	120.Coalesce
            //[Conditional]
            dic.Add("?", new AnOperator(OPERATOR_TYPE.CONDITIONAL, 90)); // Condition, //  c?x:y	130.Condition
            dic.Add(":", new AnOperator(OPERATOR_TYPE.BINARY, 90)); // ConditionElse, // c?x:y	130.Condition
            //[Assignment]
            dic.Add("+=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.AddAssign, "op_AdditionAssignment")); // AddAssign, //x+=y	140.AddAssign
            dic.Add("-=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.SubtractAssign, "op_SubtractionAssignment")); // SubtractAssign,    //x-=y	140.SubtractAssign
            dic.Add("&=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.AddAssign, "op_BitwiseAndAssignment")); // BitAndAssign, //x&=y	140.AndAssign
            dic.Add("=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2)Expression.Assign, "op_Assign")); // Assign,    //x=y		140.Assign
            dic.Add("/=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.DivideAssign, "op_DivisionAssignment")); // DivideAssign,  //x/=y	140.DivideAssign
            dic.Add("^=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.ExclusiveOrAssign, "op_ExclusiveOrAssignment")); // BitExclusiveOrAssign,  //x^=y	140.ExclusiveOrAssign
            dic.Add("<<=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, typeof(int), RequiredOperandType.NONE, (d2m)Expression.LeftShiftAssign, "op_LeftShiftAssignment")); // LeftShiftAssign,   //x<<=y	140.LeftShiftAssign
            dic.Add(">>=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, typeof(int), RequiredOperandType.NONE, (d2m)Expression.RightShiftAssign, "op_RightShiftAssignment")); // RightShiftAssign,  //x>>=y	140.RightShiftAssign
            dic.Add("%=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.ModuloAssign, "op_ModulusAssignment")); // ModuloAssign,  //x%=y	140.ModuloAssign
            dic.Add("*=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.MultiplyAssign, "op_MultiplicationAssignment")); // MultiplyAssign,    //x*=y	140.MultiplyAssign
            dic.Add("**=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.PowerAssign, null)); // PowserAssign,    //x**=y	140.PowerAssign
            dic.Add("|=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.OrAssign, "op_BitwiseOrAssignment")); // BitOrAssign   //x|=y	140.OrAssign
            return dic;
        }
    }

    public class ExprException : ApplicationException
    {
        public ExprException(string message) : base(message) { }
    }

}
