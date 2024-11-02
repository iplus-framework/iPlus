// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Runtime.Serialization;
using System.Reflection;
using gip.core.datamodel;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.webservices
{
    [DataContract]
    public class WSTranslation
    {
        [DataMember]
        public List<ACClass> Classes
        {
            get; set;
        }

        [IgnoreDataMember]
        Dictionary<string, Dictionary<string, string>> _TypeCache;
        [IgnoreDataMember]
        public Dictionary<string, Dictionary<string, string>> TypeCache
        {
            get
            {
                return _TypeCache;
            }
        }

        public void RebuildDictionary()
        {
            _TypeCache = new Dictionary<string, Dictionary<string, string>>();
            foreach (var acClass in Classes)
            {
                Dictionary<string, string> propertyCache = null;
                if (!_TypeCache.TryGetValue(acClass.FullQName, out propertyCache))
                {
                    propertyCache = new Dictionary<string, string>();
                    foreach (var property in acClass.Properties)
                    {
                        propertyCache.Add(property.ACIdentifier, property.ACCaptionTranslation);
                    }
                    _TypeCache.Add(acClass.FullQName, propertyCache);
                }
            }
        }

        public string GetTranslation(string fullQName, string propertyName)
        {
            if (_TypeCache == null)
                RebuildDictionary();
            Dictionary<string, string> propertyCache = null;
            if (_TypeCache.TryGetValue(fullQName, out propertyCache))
            {
                string translationTuple = null;
                if (propertyCache.TryGetValue(propertyName, out translationTuple))
                {
                    return Translator.GetTranslation(translationTuple);
                }
            }
            return null;
        }
    }

}
