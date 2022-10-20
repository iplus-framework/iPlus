﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class ACMethodWrapper
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

        public string GetResultParamACCaption(string paramName, string vbLanguageCode = null)
        {
            if (_ParameterTranslation == null || String.IsNullOrEmpty(paramName))
                return paramName;
            string strTranslation;
            if (!_ResultTranslation.TryGetValue(paramName, out strTranslation))
                return paramName;
            if (String.IsNullOrEmpty(vbLanguageCode))
                return Translator.GetTranslation(paramName, strTranslation);
            return Translator.GetTranslation(paramName, strTranslation, vbLanguageCode);
        }

#endregion

    }
}
