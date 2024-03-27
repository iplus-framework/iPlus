using gip.core.datamodel;
using gip.core.processapplication;
using System;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Scale calibratable'}de{'Waage alibi)'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEScaleCalibratable : PAEScaleGravimetric
    {
        static PAEScaleCalibratable()
        {
            RegisterExecuteHandler(typeof(PAEScaleCalibratable), HandleExecuteACMethod_PAEScaleCalibratable);
        }

        public PAEScaleCalibratable(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") : 
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #region Handle execute helpers
        public static bool HandleExecuteACMethod_PAEScaleCalibratable(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEScaleGravimetric(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(RegisterAlibiWeight):
                    result = RegisterAlibiWeight();
                    return true;
                case nameof(IsEnabledRegisterAlibiWeight):
                    result = IsEnabledRegisterAlibiWeight();
                    return true;
                case nameof(RegisterAlibiWeightEntity):
                    result = RegisterAlibiWeightEntity(acParameter[0] as PAOrderInfoEntry);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion


        [ACPropertyBindingTarget(810, "Read from PLC", "en{'Alibi weight [kg]'}de{'Alibigewicht [kg]'}", "", false, false, RemotePropID = 86)]
        public IACContainerTNet<double> AlibiWeight
        {
            get;
            set;
        }

        [ACPropertyBindingTarget(820, "Read from PLC", "en{'Alibi No'}de{'Alibi Nr'}", "", false, false, RemotePropID = 87)]
        public IACContainerTNet<string> AlibiNo
        {
            get;
            set;
        }


        [ACMethodInteraction("", "en{'Register alibi weight'}de{'Registriere Gewicht'}", 450, true)]
        public Msg RegisterAlibiWeight()
        {
            if (!IsEnabledRegisterAlibiWeight())
                return null;
            Msg msg = OnRegisterAlibiWeight();
            if (msg != null)
                return msg;
            return SaveAlibiWeighing();
        }

        public virtual bool IsEnabledRegisterAlibiWeight()
        {
            return true;
        }

        [ACMethodInfo("", "en{'Register alibi weight fOr entity'}de{'Registriere Gewicht für Entität'}", 451, true)]
        public Msg RegisterAlibiWeightEntity(PAOrderInfoEntry entity)
        {
            Msg msg = OnRegisterAlibiWeight();
            if (msg != null)
                return msg;
            return SaveAlibiWeighing(entity);
        }

        //PAOrderInfoEntry

        public virtual Msg OnRegisterAlibiWeight()
        {
            if (!IsEnabledOnRegisterAlibiWeight())
                return null;
            if (IsSimulationOn)
                SimulateAlibi();

            return null;
        }

        public virtual void SimulateAlibi()
        {
            double maxWeight = this.MaxScaleWeight.ValueT;
            if (Math.Abs(maxWeight) <= double.Epsilon)
                maxWeight = 1000;
            Random random = new Random();
            double weight = random.NextDouble() * maxWeight;
            AlibiWeight.ValueT = weight;
            AlibiNo.ValueT = DateTime.Now.ToString();
        }

        public virtual bool IsEnabledOnRegisterAlibiWeight()
        {
            return true;
        }

        public virtual Msg SaveAlibiWeighing(PAOrderInfoEntry entity = null)
        {
            Msg msg = new Msg(eMsgLevel.Error, "Not implemented");
            return msg;
        }
    }
}
