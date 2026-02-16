// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
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
                ACClassDesign dbDesign = acClassDesign as ACClassDesign;
                database.Entry(dbDesign).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                string xmlDesign = dbDesign.XMLDesign;
                dbDesign.XMLDesign = "";
                XElement element = aCEntitySerializer.SerializeACObject(acClassDesign as IACObject, qryACClassDesign, folderPath, false);
                string xmlACClassDesign = element != null ? element.ToString() : "";
                if (!string.IsNullOrEmpty(xmlACClassDesign))
                {
                    WriteAllText(Path.Combine(folderPath, ACClassDesign.ClassName + "_" + (acClassDesign as IACObject).ACIdentifier + Const.ACQueryExportFileType), xmlACClassDesign, (acClassDesign as IACObject).GetACUrl());
                    File.WriteAllText(Path.Combine(folderPath, ACClassDesign.ClassName + "_" + (acClassDesign as IACObject).ACIdentifier + ".xml"), xmlDesign);
                }

                xmlDesign = dbDesign.XMLDesign2;
                dbDesign.XMLDesign2 = "";
                element = aCEntitySerializer.SerializeACObject(acClassDesign as IACObject, qryACClassDesign, folderPath, false);
                xmlACClassDesign = element != null ? element.ToString() : "";
                if (!string.IsNullOrEmpty(xmlACClassDesign))
                {
                    WriteAllText(Path.Combine(folderPath, ACClassDesign.ClassName + "_" + (acClassDesign as IACObject).ACIdentifier + Const.ACQueryExportFileType), xmlACClassDesign, (acClassDesign as IACObject).GetACUrl());
                    File.WriteAllText(Path.Combine(folderPath, ACClassDesign.ClassName + "_" + (acClassDesign as IACObject).ACIdentifier + ".axaml"), xmlDesign);
                }
            }
            return true;
        }
    }
}
