using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Softing.OPCToolbox.Client;
using Softing.OPCToolbox;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.ComponentModel;

namespace gip.core.communication
{
    /// <summary>
    /// Diese Implementierung von ISPSCommunication dient zur Kommunikation mit einem 
    /// Standard-OPC-Server (Inat), wie er für die S7 eingesetzt wird. 
    /// 
    /// Die Communication findet über den Softing OPC-Client statt.
    /// 
    /// Die Objekthierarchie bei Softing entspricht unserer Objektstrukturen:
    /// 
    /// Softing                 VarioBatch
    /// ------------------------------------
    /// DaSession               VBSPSSession
    /// DaSubscription          VBSPSSubscription
    /// DaItem                  VBSPSItem
    /// Die Übertragung der Daten vom Softing-OPC-Client erfolgt in Form von zwei Arrays,
    /// 1. DaItem[] Liste von Items (z.B. "S7.DB800.DBD102") 
    /// 2. ValueQT[] Liste von neuen Wert in Form von Objekten enthalten. 
    /// 
    /// Um das teure, sprich zeitaufwendige, Matching zwischen DaItem und VBSPSItem bei jeder
    /// Wertänderung (davon gibt es sehr, sehr viele) zu minimieren, wird das OPCClientDaItem (Ableitung von DaItem)
    /// eingesetzt, welches einen direkten Pointer auf das entsprechende VBSPSItem besitzt. Dadurch muß
    /// der ankommende Wert nur in der verpointerte VBSPSItem übertragen werden. 
    /// 
    /// Damit die von VarioBatch-Seite geänderten Werte zum OPC-Server übertragen werden, wird
    /// über das Event "OnSendValueToSPS" das DaItem informiert.
    /// 
    /// TOD: Gedanken zur Optimierung:
    /// 
    /// In VBSPSSubscription wird die RequiredUpdateRate in Milisekunden festgelegt. Wenn diese 0 ist,
    /// erfolgt keine automatische Aktualisierung, sondern der Wert soll erst auf Anforderung gelesen 
    /// werden. 
    /// 
    /// Bei VBSPSItem kann auch eine RequiredUpdateRate hinterlegt werden. Wenn diese > 0 ist,
    /// muß beim OnGetValueFromSPS überprüft werden, ob der Wert zu lesen ist und gegenenfalls
    /// von SPS/OPC angefordert werden.
    /// 
    /// Grundsätzlich könnten auch erst bei erstmaliger Wertanforderung, diese in die Zeitscheibe
    /// eingetragen werden und falls eine bestimmte Zeit kein Zugriff mehr von VarioBatch erfolgt,
    /// können diese automatisch wieder auf Inaktiv gesetzt werden. Also quasi eine Garbagekollektor-
    /// implementierung.
    /// 
    /// Fazit: Das Treiberkonzept ermöglicht eine flexible Implementierung und Optimierung, ohne das
    /// der VarioBatch-Interpreter oder gar im Anwendungsquellcode Anpassungen notwendig sind. 
    /// </summary>

    public class OPCClientSoftingDaItem : DaItem
    {
        public OPCClientSoftingDaItem(IACPropertyNetServer acProperty, string opcAddr, DaSubscription parentSubscription)
            : base(opcAddr, parentSubscription)
        {
            _ACProperty = acProperty;
            if ((_ACProperty.Value != null) && (_ACProperty.Value is ACCustomTypeBase))
                RequestedDatatype = (_ACProperty.Value as ACCustomTypeBase).TypeOfValueT;
            else
                RequestedDatatype = acProperty.ACType.ObjectType;
            _ACProperty.ValueUpdatedOnReceival += OnSendValueToOPCServer;
        }

        private IACPropertyNetServer _ACProperty;
        public IACPropertyNetServer ACProperty
        {
            get
            {
                return _ACProperty;
            }
        }

        public void DeInitOPC()
        {
            if (_ACProperty != null)
            {
                _ACProperty.ValueUpdatedOnReceival -= OnSendValueToOPCServer;
                _ACProperty = null;
            }
        }

        public enum ResendLock
        {
            Unlocked = 0,
            Locked = 1,
            ResendDone = 2
        }

        internal ResendLock _ReSendLocked = ResendLock.Unlocked;

        void OnSendValueToOPCServer(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase)
        {
            if (phase == ACPropertyChangedPhase.AfterBroadcast)
                return;
            if (ACProperty == null)
                return;
            if (_ReSendLocked >= ResendLock.Locked)
            {
                if (e.ValueEvent.InvokerInfo != null && e.ValueEvent.InvokerInfo is ValueQT)
                    return;
                else if ((ACProperty.Value != null) && ACProperty.Value is ACCustomTypeBase && (ACProperty.Value as ACCustomTypeBase).Value == this.ValueQT.Data)
                    return;
                else if (ACProperty.Value == this.ValueQT.Data)
                    return;
                else
                    _ReSendLocked = ResendLock.ResendDone;
            }
            ValueQT valueQT = new ValueQT();
            int result;
            ExecutionOptions executionOptions = new ExecutionOptions();
            EnumQuality quality = new EnumQuality();

            if ((ACProperty.Value != null) && ACProperty.Value is ACCustomTypeBase)
                valueQT.SetData((ACProperty.Value as ACCustomTypeBase).Value, quality, DateTime.Now);
            else
                valueQT.SetData(ACProperty.Value, quality, DateTime.Now);

            Write(valueQT, out result, executionOptions);

            if (ResultCode.FAILED(result))
            {
                 //TODO: Fehlerbehandlung
            }
        }

        internal bool _Ready = false;
        internal bool QualitySwitchedToGood()
        {
            if (this.ValueQT.Quality == EnumQuality.GOOD && !_Ready)
            {
                _Ready = true;
                return true;
            }
            return false;
        }
    }

}
