using System.IO;
using System.Xml.Linq;

namespace gip.core.datamodel
{
    public class ExportCommandExtended: ExportCommand
    {
        public override bool ExportACClassDesign(ACEntitySerializer aCEntitySerializer, ACClass acClass, ACQueryDefinition qryACClassDesign, string folderPath)
        {
            Database database =  acClass.GetObjectContext() as Database;
            string[] files = Directory.GetFiles(folderPath, ACClassDesign.ClassName + "_*" + Const.ACQueryExportFileType);
            foreach (var file in files)
            {
                File.Delete(file);
            }
            foreach (var acClassDesign in acClass.ACSelect(qryACClassDesign))
            {
                OnExportReportProgress(0, string.Format(@"Export ACClassDesign: {0}", (acClassDesign as ACClassDesign).ACIdentifier));
                ACClassDesign dbDesing = acClassDesign as ACClassDesign;
                database.Entry(dbDesing).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                string xmlDesign = dbDesing.XMLDesign;
                dbDesing.XMLDesign = "";
                XElement element = aCEntitySerializer.SerializeACObject(acClassDesign as IACObject, qryACClassDesign, folderPath, false);
                string xmlACClassDesign = element != null ? element.ToString() : "";
                if (!string.IsNullOrEmpty(xmlACClassDesign))
                {
                    WriteAllText(folderPath + "\\" + ACClassDesign.ClassName + "_" + (acClassDesign as IACObject).ACIdentifier + Const.ACQueryExportFileType, xmlACClassDesign, (acClassDesign as IACObject).GetACUrl());
                    File.WriteAllText(folderPath + "\\" + ACClassDesign.ClassName + "_" + (acClassDesign as IACObject).ACIdentifier + ".xml", xmlDesign);
                }
            }
            return true;
        }
    }
}
