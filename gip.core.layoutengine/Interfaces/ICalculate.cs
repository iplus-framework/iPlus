﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    public enum ICalculatorResult : short
    {
        Error = 0,

        /// <summary>
        /// out-Parameter "result" was calculated by the Script-Engine
        /// The Result-Type of the Script-Function must exactly be the Converted Type.
        /// e.g. Visibility, Thickness, Brush...
        /// The passed parameter-Variable must start with an Equal-Character "="
        /// </summary>
        FromScriptEngine = 1,

        /// <summary>
        /// out-Parameter "result" was calculated by Expression-Calculator
        /// The result of the Expression-Calculator can only be a BaseType like a Int, String...
        /// The passed parameter-Variable mustn't start with an Equal-Character "="
        /// </summary>
        FromExpression = 2,
    }

    public interface ICalculator
    {
        ICalculatorResult Calculate(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture, out object result);
    }
}
