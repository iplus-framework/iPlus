// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class ACMethodWrapper : ICloneable
    {
        private string _CaptionTranslation;
        private Type _PWClass;
        private ACMethod _Method;
        private Dictionary<string,string> _ParameterTranslation;
        private Dictionary<string, string> _ResultTranslation;

#region Properties

        public string CaptionTranslation
        {
            get { return _CaptionTranslation; }
            set { _CaptionTranslation = value; }
        }

        public Type PWClass
        {
            get { return _PWClass; }
        }

        public ACMethod Method
        {
            get { return _Method; }
        }

        public Dictionary<string, string> ParameterTranslation
        {
            get
            {
                return _ParameterTranslation;
            }
        }

        public Dictionary<string, string> ResultTranslation
        {
            get
            {
                return _ResultTranslation;
            }
        }

#endregion

#region Constructors

        public ACMethodWrapper(ACMethod method, string captionTranslation, Type pwClass)
        {
            if (method == null) throw new ArgumentNullException("Method") { Source = "gip.core.datamodel.ACMethodWrapper.New" };

            _Method = method;
            if (_Method != null)
                _Method.Detach(true);
            _CaptionTranslation = captionTranslation;
            _PWClass = pwClass;
        }

        public ACMethodWrapper(ACMethod method, string captionTranslation, Type pwClass,
            Dictionary<string, string> parameterTranslation, Dictionary<string, string> resultTranslation)
            : this(method, captionTranslation, pwClass)
        {
            _ParameterTranslation = parameterTranslation;
            _ResultTranslation = resultTranslation;
        }

        public string GetParameterACCaption(string paramName, string vbLanguageCode = null)
        {
            if (_ParameterTranslation == null || String.IsNullOrEmpty(paramName))
                return paramName;
            string strTranslation;
            if (!_ParameterTranslation.TryGetValue(paramName, out strTranslation))
                return paramName;
            if (String.IsNullOrEmpty(vbLanguageCode))
                return Translator.GetTranslation(paramName, strTranslation);
            return Translator.GetTranslation(paramName, strTranslation, vbLanguageCode);
        }

        public string GetParameterACCaptionTrans(string paramName)
        {
            if (_ParameterTranslation == null || String.IsNullOrEmpty(paramName))
                return paramName;

            string strTranslation;
            if (_ParameterTranslation.TryGetValue(paramName, out strTranslation))
                return strTranslation;
                
            return paramName;
        }

        public string GetResultParamACCaption(string paramName, string vbLanguageCode = null)
        {
            if (_ResultTranslation == null || String.IsNullOrEmpty(paramName))
                return paramName;
            string strTranslation;
            if (!_ResultTranslation.TryGetValue(paramName, out strTranslation))
                return paramName;
            if (String.IsNullOrEmpty(vbLanguageCode))
                return Translator.GetTranslation(paramName, strTranslation);
            return Translator.GetTranslation(paramName, strTranslation, vbLanguageCode);
        }

        public string GetResultParamACCaptionTrans(string paramName)
        {
            if (_ResultTranslation == null || String.IsNullOrEmpty(paramName))
                return paramName;

            string strTranslation;
            if (_ResultTranslation.TryGetValue(paramName, out strTranslation))
                return strTranslation;

            return paramName;
        }

        public object Clone()
        {
            if (_Method == null || _CaptionTranslation == null || _PWClass == null)
                return null;
            ACMethod clonedMethod = _Method.Clone() as ACMethod;
            Dictionary<string, string> cloneParamTrans = new Dictionary<string, string>();
            if (_ParameterTranslation != null)
            {
                foreach (var entry in _ParameterTranslation)
                {
                    cloneParamTrans.Add(entry.Key, entry.Value);
                }
            }
            Dictionary<string, string> cloneResultTrans = new Dictionary<string, string>();
            if (_ResultTranslation != null)
            {
                foreach (var entry in _ResultTranslation)
                {
                    cloneParamTrans.Add(entry.Key, entry.Value);
                }
            }
            return new ACMethodWrapper(clonedMethod, _CaptionTranslation, _PWClass, cloneParamTrans, cloneResultTrans);
        }

        #endregion

    }
}
