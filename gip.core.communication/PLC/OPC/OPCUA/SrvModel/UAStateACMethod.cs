using System;
using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Server;
using System.Security.Cryptography.X509Certificates;
using gip.core.datamodel;
using System.Linq;
using System.Collections.Concurrent;
using gip.core.autocomponent;

namespace gip.core.communication
{
    public class UAStateACMethod : MethodState, IUAStateIACMember
    {
        public UAStateACMethod(ACComponent instance, ACClassMethod acClassMethod, NodeState parentNode, ushort namespaceIndex, uint? sortOrder) : base(parentNode)
        {
            ACComponent = instance;
            ACMethod = acClassMethod;
            this.NodeId = new NodeId(instance.GetACUrl() + ACUrlHelper.Delimiter_InvokeMethod + acClassMethod.ACIdentifier, namespaceIndex);
            //this.NodeId = new NodeId(acClassMethod.ACClassMethodID, namespaceIndex);
            BrowseName = new QualifiedName(ACUrlHelper.Delimiter_InvokeMethod + acClassMethod.ACIdentifier, namespaceIndex);
            DisplayName = acClassMethod.ACIdentifier;
            Description = acClassMethod.ACCaption;
            WriteMask = AttributeWriteMask.None;
            UserWriteMask = AttributeWriteMask.None;
            ReferenceTypeId = Opc.Ua.ReferenceTypeIds.HasComponent; // HasComponent = 47;
            //TypeDefinitionId = ObjectTypeIds.FolderType; // FolderType = 61;
            ModellingRuleId = Objects.ModellingRule_Mandatory;
            Executable = true;
            UserExecutable = true;
            NumericId = sortOrder.HasValue ? sortOrder.Value + 2000 : Convert.ToUInt32(instance.ACClassMethods.IndexWhere(c => c == acClassMethod)) + 2000;
        }

        public ACComponent ACComponent
        {
            get;
            private set;
        }

        public ACClassMethod ACMethod
        {
            get;
            private set;
        }

        public IACObject ACMember
        {
            get
            {
                return ACMethod;
            }
        }
    }
}
