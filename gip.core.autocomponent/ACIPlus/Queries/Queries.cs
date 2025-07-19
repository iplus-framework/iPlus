// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.ComponentModel;

namespace gip.core.autocomponent
{
    //Der Queries dient zum erzeugen von ACQueryDefinition-Instanzen.
    //1. Eine ACQueryDefinition ohne Childs wird mit startChildMode = None erzeugt
    //Die Konfiguration in Tabelle ACClassSetting wird unter dem ACKey = [acKey]_1 abgelegt, sie umfaßt nur Informationen für diese ACQueryDefinition

    //2. Eine ACQueryDefinition mit Childs wird mit startChildMode = Manual erzeugt
    //Die Konfiguration in Tabelle ACClassConfig wird unter dem KeyACUrl = [acKey]_N abgelegt, sie umfaßt nur Informationen für diese und 
    //untergeordnete ACQueryDefinition

    //Wird kein acKey übergeben, dann wird keine Konfiguration aus der Datenbank geladen. Diese Variante ist vor allem 
    //bei Exporten von Tabellen zwecks Installation sinnvoll, damit nicht etwas falsche Konfiguriert werden kann.
    /// <summary>
    /// Manager für Abfrageklassen
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Query-Manager'}de{'Abfrage-Manager'}", Global.ACKinds.TACQueries, Global.ACStorableTypes.Required, false, false)]
    public class Queries : ACComponentManager, IQueries
    {
        public const string ClassName = "Queries";

        #region c´tors
        public Queries(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        [ACMethodInfo("", "en{'Create Query'}de{'Anfrage erzeugen'}",9999)]
        public ACQueryDefinition CreateQuery(IACComponent acComponentParent, string qryACName, string keyForLocalConfigACUrl, bool forVBControl = false)
        {
            ACClass acClass = null;
            ACClass acClassComponent = ComponentClass;

            using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000)) // ACType ist immer vom globalen Datenbankkontext
            {
                acClass = acClassComponent.ACClass_ParentACClass.Where(c => c.ACIdentifier == qryACName).FirstOrDefault();
            }
            if (acClass == null)
                return null;

            if (string.IsNullOrEmpty(keyForLocalConfigACUrl))
            {
                ACValueList acValueList = new ACValueList();
                acValueList.Add(new ACValue(Const.PN_LocalConfigACUrl, Const.TNameString, "", acClass.ACIdentifier));
                ACQueryDefinition queryDef = new ACQueryDefinition(acClass, acClass, acComponentParent, acValueList);
                queryDef.ACInit(forVBControl ? Global.ACStartTypes.None : Global.ACStartTypes.Manually);
                queryDef.ACPostInit();
                return queryDef;
            }
            else
            {
                ACValueList acValueList = new ACValueList();
                acValueList.Add(new ACValue(Const.PN_LocalConfigACUrl, Const.TNameString, "", keyForLocalConfigACUrl));
                ACQueryDefinition queryDef = new ACQueryDefinition(acClass, acClass, acComponentParent, acValueList);
                queryDef.ACInit(forVBControl ? Global.ACStartTypes.None : Global.ACStartTypes.Manually);
                queryDef.ACPostInit();
                return queryDef;
            }
        }

        [ACMethodInfo("", "en{'Create Query by Class'}de{'Anfrage anhand Klasse erzeugen'}", 9999)]
        public ACQueryDefinition CreateQueryByClass(IACComponent acComponentParent, ACClass qryACClass, string keyForLocalConfigACUrl, bool forVBControl = false)
        {
            if (qryACClass == null)
                return null;
            if (string.IsNullOrEmpty(keyForLocalConfigACUrl))
            {
                ACValueList acValueList = new ACValueList();
                acValueList.Add(new ACValue(Const.PN_LocalConfigACUrl, Const.TNameString, "", qryACClass.ACIdentifier));
                ACQueryDefinition queryDef = new ACQueryDefinition(qryACClass, qryACClass, acComponentParent, acValueList);
                queryDef.ACInit(forVBControl ? Global.ACStartTypes.None : Global.ACStartTypes.Manually);
                queryDef.ACPostInit();
                return queryDef;
            }
            else
            {
                ACValueList acValueList = new ACValueList();
                acValueList.Add(new ACValue(Const.PN_LocalConfigACUrl, Const.TNameString, "", keyForLocalConfigACUrl));
                ACQueryDefinition queryDef = new ACQueryDefinition(qryACClass, qryACClass, acComponentParent, acValueList);
                queryDef.ACInit(forVBControl ? Global.ACStartTypes.None : Global.ACStartTypes.Manually);
                queryDef.ACPostInit();
                return queryDef;
            }
        }

        public ACQueryDefInstance CreateQueryInstanceByClass(IACComponent acComponentParent, ACClass qryACClass, string keyForLocalConfigACUrl, bool forVBControl = false)
        {
            if (qryACClass == null)
                return null;
            if (string.IsNullOrEmpty(keyForLocalConfigACUrl))
            {
                ACValueList acValueList = new ACValueList();
                acValueList.Add(new ACValue(Const.PN_LocalConfigACUrl, Const.TNameString, "", qryACClass.ACIdentifier));
                ACQueryDefInstance queryDef = new ACQueryDefInstance(qryACClass, qryACClass, acComponentParent, acValueList);
                queryDef.ACInit(forVBControl ? Global.ACStartTypes.None : Global.ACStartTypes.Manually);
                queryDef.ACPostInit();
                return queryDef;
            }
            else
            {
                ACValueList acValueList = new ACValueList();
                acValueList.Add(new ACValue(Const.PN_LocalConfigACUrl, Const.TNameString, "", keyForLocalConfigACUrl));
                ACQueryDefInstance queryDef = new ACQueryDefInstance(qryACClass, qryACClass, acComponentParent, acValueList);
                queryDef.ACInit(forVBControl ? Global.ACStartTypes.None : Global.ACStartTypes.Manually);
                queryDef.ACPostInit();
                return queryDef;
            }
        }

        [ACMethodInfo("", "en{'Create Query by Class Config'}de{'Anfrage anhand Klasse Config erzeugen'}", 9999)]
        public ACQueryDefinition CreateQueryByClassWithConfig(IACComponent acComponentParent, ACClass qryACClass, string configXML, bool forVBControl = false)
        {
            ACQueryDefinition acQueryDefinition = new ACQueryDefinition(qryACClass, qryACClass, acComponentParent, null);
            acQueryDefinition.ACInit(forVBControl ? Global.ACStartTypes.None : Global.ACStartTypes.Manually);
            acQueryDefinition.ACPostInit();
            return acQueryDefinition;
        }

        public ACQueryDefInstance GetOrCreateQueryInstance(ACClass requestedClass)
        {
            if (requestedClass == null)
                return null;
            if (!(requestedClass.ACKind == Global.ACKinds.TACDBA || requestedClass.ACKind == Global.ACKinds.TACQRY))
                return null;
            ACClass qryClass = requestedClass;
            if (requestedClass.ACKind == Global.ACKinds.TACDBA)
            {
                qryClass = requestedClass.PrimaryNavigationquery();
                if (qryClass == null)
                    return null; // no query class defined for this entity class
            }

            ACQueryDefInstance defInstance = ACMemberList.Where(c => c.ACIdentifier == qryClass.ACIdentifier && c is ACQueryDefInstance).FirstOrDefault() as ACQueryDefInstance;
            if (defInstance == null)
            {
                defInstance = CreateQueryInstanceByClass(this, qryClass, null);
                if (defInstance == null)
                    return null; // query definition could not be created
                if (defInstance.TakeCount <= 0 && Root != null && Root.Environment != null)
                    defInstance.TakeCount = Root.Environment.AccessDefaultTakeCount;
                ACMemberList.Add(defInstance);
            }
            return defInstance;
        }

        public override object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return base.ACUrlCommand(acUrl, acParameter);
        }

        protected override IACObjectWithInit StartComponent(StartCompResult startCompResult, object content, Object[] acParameter, ACStartCompOptions startOptions = ACStartCompOptions.Default)
        {
            if (startCompResult.ACClass == null)
            {
                startCompResult.ACClass = Root.Database.ContextIPlus.GetACType(startCompResult.ACClassName);
                if (startCompResult.ACClass == null && startCompResult.ACClass.ACKind != Global.ACKinds.TACDBA)
                    return null;
            }
            return GetOrCreateQueryInstance(startCompResult.ACClass);
        }


        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(CreateQuery):
                    result = CreateQuery((IACComponent)acParameter[0], (String)acParameter[1], (String)acParameter[2], acParameter.Count() == 4 ? (bool)acParameter[3] : false);
                    return true;
                case nameof(CreateQueryByClass):
                    result = CreateQueryByClass((IACComponent)acParameter[0], (ACClass)acParameter[1], (String)acParameter[2], acParameter.Count() == 4 ? (bool)acParameter[3] : false);
                    return true;
                case nameof(CreateQueryByClassWithConfig):
                    result = CreateQueryByClassWithConfig((IACComponent)acParameter[0], (ACClass)acParameter[1], (String)acParameter[2], acParameter.Count() == 4 ? (bool)acParameter[3] : false);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }
}
