using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Converter for complex objects'}de{'Konverter für komplexe objekte'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.NotStorable, false, false)]
    public abstract class ACSessionObjSerializer : ACComponent
    {
        #region c´tors
        public ACSessionObjSerializer(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion


        #region Methods, Range: 200
        /// <summary>
        /// returns true if the the serializer can send and write objects which are for the passed typeOrACMethodName
        /// </summary>
        /// <param name="typeOrACMethodName">Is wether the full-TypeName of the object which should be serialized or the ACIdentifier of a ACMethod</param>
        public abstract bool IsSerializerFor(string typeOrACMethodName);

        /// <summary>This method must bei implemented in the derivation class.</summary>
        /// <param name="complexObj">The complexObj can be wether a ACMethod or any serializable Object.</param>
        /// <param name="prevComplexObj">Previous send object</param>
        /// <param name="dbNo">Datablock-Number</param>
        /// <param name="offset">Offset in Datablock</param>
        /// <param name="routeOffset"></param>
        /// <param name="miscParams"></param>
        /// <returns>true if succeeded</returns>
        public abstract bool SendObject(object complexObj, object prevComplexObj, int dbNo, int offset, int? routeOffset, object miscParams);

        /// <summary>This method must bei implemented in the derivation class.</summary>
        /// <param name="complexObj">The complexObj can be wether a ACMethod or any serializable Object. The complexObject should be empty</param>
        /// <param name="dbNo">Datablock-Number</param>
        /// <param name="offset">Offset in Datablock</param>
        /// <param name="miscParams"></param>
        /// <returns>The passed complexObj with filled out properties. If read error the result is null.</returns>
        public abstract object ReadObject(object complexObj, int dbNo, int offset, int?routeOffset, object miscParams);

        public virtual void OnObjectRead(byte[] result)
        {

        }

        public bool MethodNameEquals(string typeOrACMethodName, string compareWith)
        {
            if (String.IsNullOrEmpty(typeOrACMethodName) || String.IsNullOrEmpty(compareWith))
                return false;
            if (typeOrACMethodName.IndexOf(ACUrlHelper.AttachedMethodIDConcatenator) < 0)
                return typeOrACMethodName == compareWith;
            string[] split = typeOrACMethodName.Split(ACUrlHelper.AttachedMethodIDConcatenator);
            if (split == null || !split.Any())
                return false;
            return split[0].Trim() == compareWith;
        }
        #endregion
    }
}
