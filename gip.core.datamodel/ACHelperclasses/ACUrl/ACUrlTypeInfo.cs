// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace gip.core.datamodel
{
    public class ACUrlTypeSegmentInfo
    {
        public string ACUrl { get; set; }
        public IACType ACType { get; set; }
        public Type CLRType { get; set; }
        public Type ObjectFullType         
        {
            get
            {
                if (ACType != null)
                    return ACType.ObjectFullType;
                return CLRType;
            }
        }
        public object Value { get; set; }
        public Global.ControlModes RightControlMode { get; set; }
    }

    public class ACUrlTypeInfo : List<ACUrlTypeSegmentInfo>
    {
        private string _SubPath = string.Empty;
        public string SubPath { get { return _SubPath; }  set { _SubPath = value; } }

        public ACUrlTypeSegmentInfo GetLastComponent()
        {
            if (this.Count == 0)
                return null;
            ACUrlTypeSegmentInfo lastObject = null;
            foreach (var segment in this)
            {
                if (segment.Value is IACObjectWithInit acObject)
                {
                    lastObject = segment;
                }
            }
            return lastObject;
        }

        public void AddSegment(string acUrl, IACType acType, object value, Global.ControlModes rightControlMode)
        {
            this.Add(new ACUrlTypeSegmentInfo
            {
                ACUrl = acUrl,
                ACType = acType,
                Value = value,
                RightControlMode = rightControlMode
            });
        }

        public void AddSegment(string acUrl, Type type, object value, Global.ControlModes rightControlMode)
        {
            this.Add(new ACUrlTypeSegmentInfo
            {
                ACUrl = acUrl,
                CLRType = type,
                Value = value,
                RightControlMode = rightControlMode
            });
        }
    }
}
