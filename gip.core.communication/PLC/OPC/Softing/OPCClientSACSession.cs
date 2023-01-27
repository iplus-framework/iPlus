using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
//using Softing.OPCToolbox.Client;
//using Softing.OPCToolbox;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    //[ACClassInfo(Const.PackName_VarioSystem, "en{'OPCClientSACSession'}de{'OPCClientSACSession'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    //public class OPCClientSACSession : OPCClientACSession
    //{
    //    #region c´tors
    //    public OPCClientSACSession(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
    //        : base(acType, content, parentACObject, parameter, acIdentifier)
    //    {
    //    }

    //    public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
    //    {
    //        if (!base.ACInit(startChildMode))
    //            return false;
    //        return true;
    //    }

    //    public override bool ACPostInit()
    //    {
    //        return base.ACPostInit();
    //    }

    //    public override bool ACDeInit(bool deleteACClassTask = false)
    //    {
    //        return base.ACDeInit(deleteACClassTask);
    //    }
    //    #endregion

    //    #region Properties
    //    [ACPropertyInfo(9999)]
    //    public string OPCUrl
    //    {
    //        get
    //        {
    //            if (String.IsNullOrEmpty(HostOfOPCServer) || String.IsNullOrEmpty(OPCServerName) || String.IsNullOrEmpty(OPCServerCLSID))
    //                return "";

    //            //Beispiel url: "opcda:localhost/INAT TcpIpH1 OPC Server/{3DA28330-68CB-11D2-9C65-0021A0020009}"
    //            return string.Format("opcda://{0}/{1}/{2}", HostOfOPCServer, OPCServerName, OPCServerCLSID);
    //        }
    //    }

    //    OPCClientSoftingDaSession _DaSession = null;
    //    public OPCClientSoftingDaSession DaSession
    //    {
    //        get
    //        {
    //            return _DaSession;
    //        }
    //    }

    //    public Softing.OPCToolbox.Client.Application SoftingApp
    //    {
    //        get
    //        {
    //            if (ParentACComponent == null)
    //                return null;
    //            if (ParentACComponent is OPCClientSACService)
    //                return (ParentACComponent as OPCClientSACService).SoftingApp;
    //            return null;
    //        }
    //    }

    //    #endregion

    //    #region Methods

    //    public override bool InitSession()
    //    {
    //        if (_DaSession != null)
    //            return true;

    //        if (_ReconnectTries <= 0)
    //            Messages.LogDebug(this.GetACUrl(), "OPCClientSACSession.InitSession()", "Start InitSession");

    //        if (String.IsNullOrEmpty(OPCUrl))
    //            return false;

    //        // 1. Instanz der Session erzeugen
    //        if (_ReconnectTries <= 0)
    //            Messages.LogDebug(this.GetACUrl(), "OPCClientSACSession.InitSession()", "New Session");

    //        _DaSession = new OPCClientSoftingDaSession(OPCUrl);
    //        _DaSession.StateChangeCompleted += OnStateChanged;

    //        if (_ReconnectTries <= 0)
    //            Messages.LogDebug(this.GetACUrl(), "OPCClientSACSession.InitSession()", "Init Subscriptions");

    //        foreach (IACObject child in this.ACComponentChilds)
    //        {
    //            if (child is OPCClientACSubscr)
    //            {
    //                OPCClientACSubscr acSubscription = child as OPCClientACSubscr;
    //                acSubscription.InitSubscription();
    //            }
    //        }

    //        if (_ReconnectTries <= 0)
    //            Messages.LogDebug(this.GetACUrl(), "OPCClientSACSession.InitSession()", "Init Completed");
    //        return true;
    //    }

    //    public override bool IsEnabledInitSession()
    //    {
    //        if (_DaSession != null)
    //            return false;
    //        if (String.IsNullOrEmpty(OPCUrl))
    //            return false;
    //        return true;
    //    }

    //    public override bool DeInitSession()
    //    {
    //        if (_DaSession == null)
    //            return true;

    //        // 1. Disconnect
    //        if (!DisConnect())
    //            return false;

    //        // 2. Remove Events
    //        _DaSession.StateChangeCompleted -= OnStateChanged;

    //        // 3. Deinit Subscriptions. (Is already done, when called ACDeInit())
    //        foreach (IACComponent child in this.ACComponentChilds)
    //        {
    //            if (child is OPCClientACSubscr)
    //            {
    //                OPCClientACSubscr acSubscription = child as OPCClientACSubscr;
    //                acSubscription.DeInitSubscription();
    //            }
    //        }

    //        // 4. Remove Session from Softing.Application
    //        if (SoftingApp != null)
    //            SoftingApp.RemoveDaSession(_DaSession);

    //        _DaSession = null;
    //        return true;
    //    }

    //    public override bool IsEnabledDeInitSession()
    //    {
    //        if (_DaSession == null)
    //            return false;
    //        return true;
    //    }

    //    protected override void StartReconnection()
    //    {
    //    }

    //    private int _ReconnectTries = 0;
    //    public override bool Connect()
    //    {
    //        if (ACOperationMode != ACOperationModes.Live)
    //            return true;
    //        if (!InitSession())
    //            return false;

    //        if (!IsEnabledConnect())
    //        {
    //            _ReconnectTries++;
    //            return false;
    //        }

    //        if (_ReconnectTries <= 0)
    //            Messages.LogDebug(this.GetACUrl(), "OPCClientSACSession.Connect()", "Start Connect");

    //        int res = _DaSession.Connect(true, true, new ExecutionOptions(EnumExecutionType.ASYNCHRONOUS,0));
    //        //int res = _DaSession.Connect(true, true, new ExecutionOptions(EnumExecutionType.SYNCHRONOUS, 0));
    //        if (!ResultCode.SUCCEEDED(res))
    //        {
    //            if (_ReconnectTries <= 0)
    //                Messages.LogDebug(this.GetACUrl(), "OPCClientSACSession.Connect()", "Not Connected");
    //            return false;
    //        }
    //        _ReconnectTries = 0;
    //        Messages.LogDebug(this.GetACUrl(), "OPCClientSACSession.Connect()", "Connected");
    //        return true;
    //    }

    //    public override bool IsEnabledConnect()
    //    {
    //        if (!base.IsEnabledConnect())
    //            return false;
    //        if (_DaSession == null
    //            || _DaSession.TargetState == EnumObjectState.CONNECTED
    //            || _DaSession.TargetState == EnumObjectState.ACTIVATED)
    //            return false;
    //        return true;
    //    }

    //    public override bool DisConnect()
    //    {
    //        if (ACOperationMode != ACOperationModes.Live)
    //            return true;
    //        if (!IsEnabledDisConnect())
    //            return false;
    //        int res = _DaSession.Disconnect(new ExecutionOptions(EnumExecutionType.ASYNCHRONOUS, 0));
    //        if (!ResultCode.SUCCEEDED(res))
    //            return false;
    //        return true;
    //    }

    //    public override bool IsEnabledDisConnect()
    //    {
    //        if (ACOperationMode != ACOperationModes.Live)
    //            return true;
    //        if (_DaSession == null
    //            || (   _DaSession.TargetState != EnumObjectState.CONNECTED
    //                && _DaSession.TargetState != EnumObjectState.ACTIVATED))
    //            return false;
    //        return true;
    //    }
    //    #endregion

    //    #region Event-Handler
    //    private void OnStateChanged(ObjectSpaceElement obj, EnumObjectState state)
    //    {
    //        if (IsConnected != null)
    //        {
    //            if ((state == EnumObjectState.CONNECTED) || (state == EnumObjectState.ACTIVATED))
    //                IsConnected.ValueT = true;
    //            else
    //                IsConnected.ValueT = false;
    //        }
    //    }
    //    #endregion
    //}
}
