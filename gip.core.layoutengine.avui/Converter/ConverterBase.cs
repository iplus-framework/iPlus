using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Windows.Markup;
using System.Collections;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the base class for converters.
    /// </summary>
    public abstract class ConverterBase : MarkupExtension
    {
        public ConverterBase()
        {
            UpdateCalculator();
        }


        ExpressionCalculator _Calculator;
        public ExpressionCalculator Calculator
        {
            get
            {
                return _Calculator;
            }
        }

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

        private void UpdateCalculator()
        {
            if (_Calculator == null)
            {
                if (GlobalFunction && (Layoutgenerator.Root != null))
                    _Calculator = new ExpressionCalculator(this, Layoutgenerator.Root.Environment);
                else if (!GlobalFunction && Layoutgenerator.CurrentACComponent != null)
                    _Calculator = new ExpressionCalculator(this, Layoutgenerator.CurrentACComponent);
                else
                    _Calculator = new ExpressionCalculator(this);
            }
            else
            {
                if (GlobalFunction && (Layoutgenerator.Root != null))
                    _Calculator.ACComponent = Layoutgenerator.Root.Environment;
                else if (!GlobalFunction && Layoutgenerator.CurrentACComponent != null)
                    _Calculator.ACComponent = Layoutgenerator.CurrentACComponent;
                else
                    _Calculator.ACComponent = null;
            }
        }

        private string _ACUrlCommand;
        /// <summary>
        /// Name of Script-Method which should be called
        /// </summary>
        public string ACUrlCommand
        {
            get
            {
                return _ACUrlCommand;
            }
            set
            {
                _ACUrlCommand = value;
            }
        }


        private string _Expression;
        /// <summary>
        /// Lambda-Expression to Calculate:
        /// "(int a, double b, int c) => if (a > 10) {return ((a * b) + c);} else {return a * b;}"
        /// a, b, c is VBBinding-Element in Multibinding
        /// <Multibinding>
        ///     <VBBinding/> Is a
        ///     <VBBinding/> Is b
        ///     <VBBinding/> Is c
        /// </Multibinding>
        /// </summary>
        public string Expression
        {
            get
            {
                return _Expression;
            }
            set
            {
                _Expression = value;
                if (Calculator != null)
                    Calculator.ReloadExpression();
            }
        }

        /// <summary>
        /// Name of Script-Method which should be called
        /// </summary>
        List<object> _MethodParameters;
        public IList MethodParameters
        {
            get
            {
                if (_MethodParameters == null)
                    _MethodParameters = new List<object>();
                return _MethodParameters;
            }
        }



        public enum ConvType : short
        {
            /// <summary>
            /// Conversion is done only by Converter without Expression-Calculator
            /// </summary>
            Direct = 0,

            /// <summary>
            /// out-Parameter "result" was calculated by the Script-Engine
            /// The Result-Type of the Script-Function must exactly be the Converted Type.
            /// e.g. Visibility, Thickness, Brush...
            /// The passed parameter-Variable must start with an Equal-Character "="
            /// </summary>
            ScriptEngine = 1,

            /// <summary>
            /// out-Parameter "result" was calculated by Expression-Calculator
            /// The result of the Expression-Calculator can only be a BaseType like a Int, String...
            /// The passed parameter-Variable mustn't start with an Equal-Character "="
            /// </summary>
            Expression = 2,
        }

        public ConvType ConversionBy
        {
            get
            {
                if (!String.IsNullOrEmpty(ACUrlCommand))
                    return ConverterBase.ConvType.ScriptEngine;
                else if (!String.IsNullOrEmpty(Expression))
                    return ConverterBase.ConvType.Expression;
                return ConverterBase.ConvType.Direct;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
