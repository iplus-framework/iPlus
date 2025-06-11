using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    public class StartCompResult
    {
        public StartCompResult(string childACIdentifier)
        {
            this.ACIdentifier = childACIdentifier;
            this.ACClassName = ACUrlHelper.ExtractTypeName(childACIdentifier);
            this.ACInstance = ACUrlHelper.ExtractInstanceName(childACIdentifier);
        }
        public string ACIdentifier { get; set; }
        public string ACClassName { get; set; }
        public string ACInstance { get; set; }
        public ACClass ACClass { get; set; }
        public Global.ControlModes ControlMode { get; set; }
        public ACChildInstanceInfo ChildInstanceInfo { get; set; }
        public bool IsComponentChild { get { return this.ACClass != null || ChildInstanceInfo != null; } }
        public bool HasRightsToStart { get { return !(ControlMode < Global.ControlModes.Disabled && this.ACClass.ParentACClassID.HasValue); } }
    }
}
