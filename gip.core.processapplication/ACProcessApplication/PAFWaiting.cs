using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Waiting'}de{'Warten'}", Global.ACKinds.TPAProcessFunction, Global.ACStorableTypes.Required, false, PWWaiting.PWClassName, true)]
    public class PAFWaiting : PAProcessFunction
    {
        #region Constructors

        static PAFWaiting()
        {
            ACMethod.RegisterVirtualMethod(typeof(PAFWaiting), ACStateConst.TMStart, CreateVirtualMethod("Waiting", "en{'Waiting'}de{'Warten'}", typeof(PWWaiting)));
        }

        public PAFWaiting(core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion 

        #region Public 

        [ACMethodAsync("Process", "en{'Start'}de{'Start'}", (short)MISort.Start, false)]
        public override ACMethodEventArgs Start(ACMethod acMethod)
        {
            return base.Start(acMethod);
        }

        #endregion

        #region Private

        private void OnTimer()
        {
            while (true)
            {
                Thread.Sleep(2000);
                continue;
                //Istgewicht += 1;
                //if (Quellzelle != null)
                //    Quellzelle.ValueT += 1;

                //if (MixingInfo.ValueT == null)
                //    MixingInfo.ValueT = new ACMixingInfo() { MixingName = "Test", MixingAge = Quellzelle.ValueT };
                //else
                //    MixingInfo.ValueT.MixingAge += 2;

                //if ((MixingInfoList.ValueT == null) || (MixingInfoList.ValueT.Count() > 5))
                //{
                //    ACMixingInfo info = new ACMixingInfo() { MixingName = "Test", MixingAge = 0 };
                //    BindingList<ACMixingInfo> newList = new BindingList<ACMixingInfo>();
                //    newList.Add(info);
                //    MixingInfoList.ValueT = newList;
                //}
                //else
                //{
                //    if (MixingInfoList.ValueT.Last().MixingAge == 0)
                //        MixingInfoList.ValueT.Last().MixingAge = 1;
                //    else
                //    {
                //        ACMixingInfo info = new ACMixingInfo() { MixingName = "Test", MixingAge = 0 };
                //        MixingInfoList.ValueT.Add(info);
                //    }
                //}
            }
        }

        protected static ACMethodWrapper CreateVirtualMethod(string acIdentifier, string captionTranslation, Type pwClass)
        {
            ACMethod method = new ACMethod(acIdentifier);

            //Method.ParameterValueList.Add(new ACValue("Temperature", typeof(Double), 0.0, Global.ParamOption.Required));

            return new ACMethodWrapper(method, captionTranslation, pwClass);
        }

        #endregion

    }
}
