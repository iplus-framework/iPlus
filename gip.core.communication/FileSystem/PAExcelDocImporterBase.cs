using ClosedXML.Excel;
using gip.core.datamodel;
using System;
using System.IO;
using System.Linq;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Excel Importer'}de{'Excel Importer'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class PAExcelDocImporterBase : PADocImporterBase
    {
        #region c´tors
        public PAExcelDocImporterBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion

        #region Methods

        public virtual bool DoImportAndArchive(ACEventArgs fileInfoArgs)
        {
            PerformanceEvent perfEvent = null;
            bool parseSucc = false;
            var vbDump = Root.VBDump;
            if (vbDump != null)
                perfEvent = vbDump.PerfLoggerStart(this.GetACUrl() + "!" + nameof(DoImportAndArchive), 100);

            try
            {

                _CurrentFileName = fileInfoArgs["FullPath"] as string;

                try
                {
                    using (var workbook = new XLWorkbook(_CurrentFileName))
                    {
                        parseSucc = ProcessObject(workbook, workbook);
                    }
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "DoImportAndArchive(Duration)", $"{_CurrentFileName} : {ex.Message}");
                }


                if (!String.IsNullOrWhiteSpace(ForwardDir))
                    ForwardFile(CurrentFileName, ForwardDir);

                // Archiviere falls nötig
                if (parseSucc)
                {
                    string movePath = FindAndCreateArchivePath(CurrentFileName);
                    MoveOrDeleteFile(ArchivingOn, movePath, CurrentFileName);
                }
                // Verschiebe in Mülleimer
                else
                {
                    string movePath = FindAndCreateTrashPath(CurrentFileName);
                    MoveOrDeleteFile(true, movePath, CurrentFileName);
                }
            }
            finally
            {
                if (vbDump != null && perfEvent != null)
                {
                    vbDump.PerfLoggerStop(this.GetACUrl() + "!" + nameof(DoImportAndArchive), 100, perfEvent, PerfTimeoutStackTrace);
                    if (perfEvent.IsTimedOut)
                        Messages.LogDebug(this.GetACUrl(), "DoImportAndArchive(Duration)", CurrentFileName);
                }
            }

            return parseSucc;
        }


        public abstract bool ProcessObject(object workbook, object workbookParseObj);

        public virtual bool IsImporterForExcelDocType(ACEventArgs eventArgs, string fileName)
        {
            return true;
        }

        #endregion

    }
}
