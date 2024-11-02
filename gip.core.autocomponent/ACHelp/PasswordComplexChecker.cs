// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public class PasswordComplexChecker
    {

        public static bool IsPasswordComplex(string password, int minUpperLetters, int minDigits, int minOtherElements)
        {
            bool isValid = false;
            var letters = 0;
            var digits = 0;
            var uppers = 0;
            var lowers = 0;
            var symbols = 0;
            var notLetterOrDigits = 0;
            foreach (var ch in password)
            {
                if (char.IsLetter(ch)) letters++;
                if (char.IsDigit(ch)) digits++;
                if (char.IsUpper(ch)) uppers++;
                if (char.IsLower(ch)) lowers++;
                if (char.IsSymbol(ch)) symbols++;
                if (!char.IsLetter(ch) && !char.IsDigit(ch)) notLetterOrDigits++;
            }

            isValid =
                uppers >= minUpperLetters &&
                digits >= minDigits &&
                notLetterOrDigits >= minOtherElements;

            return isValid;
        }
    }
}
