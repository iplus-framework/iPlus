using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.ISOonTCP;
using System.Timers;
using System.Threading;
using System.IO;
//using TwinCAT.Ads;
//using TwinCAT.TypeSystem;

namespace gip.core.communication
{  
    //[ACClassInfo(Const.PackName_VarioSystem, "en{'Beckhoff ADS Session'}de{'Beckhoff ADS Session'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    //public class ADSSession : ACSession
    //{
    //    #region c´tors
    //    public ADSSession(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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
    //        bool result = base.ACDeInit(deleteACClassTask);
    //        return result;
    //    }
    //    #endregion

    //    #region Properties
    //    [ACPropertyInfo(9999)]
    //    public string AmsNetId
    //    {
    //        get;
    //        set;
    //    }

    //    [ACPropertyInfo(9999, DefaultValue = "0")]
    //    public int Port
    //    {
    //        get;
    //        set;
    //    }

    //    private TcAdsClient _AdsClient = null;
    //    #endregion

    //    #region Methods

    //    public override bool InitSession()
    //    {
    //        if (!IsEnabledInitSession())
    //            return true;
    //        _AdsClient = new TcAdsClient();
    //        return true;
    //    }

    //    public override bool IsEnabledInitSession()
    //    {
    //        if (_AdsClient == null)
    //            return true;
    //        return false;
    //    }

    //    public override bool DeInitSession()
    //    {
    //        if (_AdsClient != null)
    //        {
    //            if (_AdsClient.IsConnected)
    //                _AdsClient.Disconnect();
    //            _AdsClient.Dispose();
    //            _AdsClient = null;
    //        }
    //        return true;
    //    }

    //    public override bool IsEnabledDeInitSession()
    //    {
    //        if (_AdsClient != null)
    //            return true;
    //        return true;
    //    }

    //    public override bool Connect()
    //    {
    //        _ManuallyDisConnected = false;

    //        if (!InitSession())
    //            return false;

    //        _AdsClient.Connect(AmsNetId, Port);
    //        IsConnected.ValueT = _AdsClient.IsConnected;

    //        if (ACOperationMode != ACOperationModes.Live)
    //            return true;
    //        if (_AdsClient.IsConnected)
    //            return true;

    //        return true;
    //    }

    //    public override bool IsEnabledConnect()
    //    {
    //        if (ACOperationMode != ACOperationModes.Live)
    //            return false;
    //        if (_AdsClient == null)
    //            return true;
    //        if (_AdsClient.IsConnected)
    //            return false;
    //        return true;
    //    }

    //    private bool _ManuallyDisConnected = false;
    //    public override bool DisConnect()
    //    {
    //        if (!IsEnabledDisConnect())
    //            return true;
    //        if (_AdsClient == null)
    //            return true;
    //        if (_AdsClient.IsConnected)
    //            _AdsClient.Disconnect();
    //        IsConnected.ValueT = _AdsClient.IsConnected;
    //        if (_AdsClient != null)
    //        {
    //            _AdsClient.Disconnect();
    //            _AdsClient.Dispose();
    //            _AdsClient = null;
    //        }

    //        return IsConnected.ValueT;
    //    }

    //    public override bool IsEnabledDisConnect()
    //    {
    //        if (ACOperationMode != ACOperationModes.Live)
    //            return false;
    //        if (_AdsClient == null)
    //            return false;
    //        if (_AdsClient.IsConnected)
    //            return true;
    //        return false;
    //    }

    //    #endregion

    //    #region Event-Handler

    //    #endregion

    //    #region Methods
    //    int _damirHandle = -1;
    //    int curentVal = 0;
    //    [ACMethodInteraction("xxx", "en{'Write Test'}de{'Write Test'}", 200, true)]
    //    public void WriteTest()
    //    {
    //        if (_AdsClient == null)
    //            return;
    //        try
    //        {
    //            object result = null;

    //            ITcAdsSymbol symbol = _AdsClient.ReadSymbolInfo("MAIN._Plant1._Mixer1");
    //            //ITcAdsSymbol symbol = _AdsClient.ReadSymbolInfo("MAIN._Mixer1._Motor");

    //            ITcAdsSymbol5 rpcSymbol = symbol as ITcAdsSymbol5;
    //            if (rpcSymbol != null)
    //            {
    //                var handle1 = _AdsClient.CreateVariableHandle("MAIN._Plant1._Mixer1._ACUrl");
    //                //var resHandle1 = _AdsClient.ReadAny(handle1, typeof(string));
    //                //var handle2 = _AdsClient.CreateVariableHandle("MAIN._Plant1._Mixer1.ACUrl");
    //                //var resHandle2 = _AdsClient.ReadAny(handle2, typeof(string));
    //                //var handle3 = _AdsClient.CreateVariableHandle("MAIN._Plant1._Mixer1.ThisPointer");
    //                //var resHandle3 = _AdsClient.ReadAny(handle3, typeof(UInt32));

    //                if (rpcSymbol.HasRpcMethods)
    //                {
    //                    IRpcMethod rpcMethod = null;
    //                    if (rpcSymbol.RpcMethods.TryGetMethod("__getThisPointer", out rpcMethod))
    //                    {
    //                        object rpcResult = (object)_AdsClient.InvokeRpcMethod("MAIN._Plant1._Mixer1", "__getThisPointer", new object[] { });
    //                        rpcResult = (object)_AdsClient.InvokeRpcMethod(symbol, "__getThisPointer", new object[] { });
    //                    }

    //                    if (rpcSymbol.RpcMethods.TryGetMethod("RPCTest", out rpcMethod))
    //                    {
    //                        bool rpcResult = (bool)_AdsClient.InvokeRpcMethod("MAIN._Plant1._Mixer1", "RPCTest", new object[] { });
    //                        rpcResult = (bool)_AdsClient.InvokeRpcMethod(symbol, "RPCTest", new object[] { });
    //                    }
    //                }
    //            }

    //            _AdsClient.TryInvokeRpcMethod("MAIN._Plant1._Mixer1", "RPCTest", new object[] { }, out result);

    //            //_AdsClient.TryInvokeRpcMethod(symbol, "RPCTest", new object[] { true }, out result);
    //            //if (_damirHandle < 0)
    //            _damirHandle = _AdsClient.CreateVariableHandle("MAIN._Plant1._Mixer1._Motor._IsRunning");
    //            var handle4 = _AdsClient.CreateVariableHandle("MAIN._Plant1._Mixer1._Cmds[1]");
    //            curentVal++;
    //            _AdsClient.WriteAny(_damirHandle, true);
    //        }
    //        catch
    //        {
    //        }

    //    }
    //    #endregion


    //    protected override void StartReconnection()
    //    {
    //    }
    //}
}
