// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public class ACCleanManagerJsonReport
    {

        #region DI
        public string RootFolder { get; private set; }
        public string ReportPath { get; private set; }
        #endregion

        #region ctor's
        public ACCleanManagerJsonReport(string rootFolder)
        {
            RootFolder = rootFolder;
            CreateReportFolder();
        }
        #endregion

        #region private methods

        public void CreateReportFolder()
        {
            string timeStamp = DateTime.Now.ToString("dd.MM.yyyy_HH-mm");
            string rootPath = Path.Combine(RootFolder, string.Format(@"ACClassCleanManager-{0}", timeStamp));
            Directory.CreateDirectory(rootPath);
            ReportPath = rootPath;
        }

        public void WriteACClassCleanManagerJsonData(ACClassCleanManager cleanManager)
        {
            string fileMissingAssemblyJson = "ACClassCleanManager[0]-missing-assembly.json";
            string fileMissingClassJson = "ACClassCleanManager[1]-missing-class.json";
            string fileNotCollectedDlls = "ACClassCleanManager[2]-not-collected-dlls.json";

            string assemblyJson = JsonConvert.SerializeObject(cleanManager.DBAssembilesWithoutFile, Formatting.Indented);
            string classJson = JsonConvert.SerializeObject(cleanManager.MissingClasses, Formatting.Indented);
            string notCollectedJson = JsonConvert.SerializeObject(cleanManager.NotCollectedDlls, Formatting.Indented);

            File.WriteAllText(Path.Combine(ReportPath, fileMissingAssemblyJson), assemblyJson, Encoding.UTF8);
            File.WriteAllText(Path.Combine(ReportPath, fileMissingClassJson), classJson, Encoding.UTF8);
            File.WriteAllText(Path.Combine(ReportPath, fileNotCollectedDlls), notCollectedJson, Encoding.UTF8);
        }

        public void WriteRemoveClassesReport(List<RemoveClassReport> removeClassReports)
        {
            string fileRemoveClassReport = "ACClassCleanManager[3]-remove-classes-report.json";
            string removeClassReportJson = JsonConvert.SerializeObject(removeClassReports, Formatting.Indented);
            File.WriteAllText(Path.Combine(ReportPath, fileRemoveClassReport), removeClassReportJson, Encoding.UTF8);
        }
        #endregion
    }
}
