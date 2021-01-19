using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using combit.ListLabel17;
using System.Globalization;
using System.ComponentModel;
using combit.ListLabel17.DataProviders;
using System.IO;

namespace gip.core.reporthandler
{
    public class VBListLabel : ListLabel
    {
        #region c'tors
        public VBListLabel()
            : base()
        {
        }

        public VBListLabel(CultureInfo culture)
            : base(culture)
        {
        }

        public VBListLabel(IContainer container)
            : base(container)
        {
        }

        public VBListLabel(LlLanguage language)
            : base(language)
        {
        }

        public VBListLabel(CultureInfo culture, bool enableCallbacks)
            : base(culture, enableCallbacks)
        {
        }

        public VBListLabel(LlLanguage language, bool enableCallbacks)
            : base(language, enableCallbacks)
        {
        }

        public VBListLabel(CultureInfo culture, bool enableCallbacks, string debugLogFilePath)
            : base(culture, enableCallbacks, debugLogFilePath)
        {
        }

        public VBListLabel(LlLanguage language, bool enableCallbacks, string debugLogFilePath)
            : base(language, enableCallbacks, debugLogFilePath)
        {
        }
        #endregion

        #region public methods
        public void Print(Stream projectStream, LlProject projectType, string printerName)
        {
            if (projectStream == null) 
            {
                throw new ArgumentNullException("projectStream");
            }
            string text2 = StreamSerializationHelper.StreamToFile(projectStream, "llstream", this.FileExtensions.GetFileExtension(projectType, LlFileType.Project), true);
            try
            {
                if (String.IsNullOrEmpty(printerName))
                    this.Print(projectType, text2, false);
                else
                    this.Print(printerName, projectType, text2);
            }
            finally
            {
                string path = Path.ChangeExtension(text2, "*");
                string[] files = Directory.GetFiles(Path.GetDirectoryName(path), Path.GetFileName(path));
                for (int i = 0; i < files.Length; i++)
                {
                    string path2 = files[i];
                    if (File.Exists(path2) && string.Compare(Path.GetExtension(path2), ".ll", true) != 0)
                    {
                        File.Delete(path2);
                    }
                }
            }
        }
        #endregion
        //#region override
        //protected override void DefineChildTables(IDataProvider dataSource, ITable table)
        //{
        //    base.DefineChildTables(dataSource, table);
        //}

        //protected override void OnAutoDefineField(AutoDefineElementEventArgs e)
        //{
        //    base.OnAutoDefineField(e);
        //}

        //protected override void OnAutoDefineVariable(AutoDefineElementEventArgs e)
        //{
        //    base.OnAutoDefineVariable(e);
        //}

        //protected override void OnAutoDefineTable(AutoDefineDataItemEventArgs e)
        //{
        //    base.OnAutoDefineTable(e);
        //}

        //protected override void OnAutoDefineTableRelation(AutoDefineDataItemEventArgs e)
        //{
        //    base.OnAutoDefineTableRelation(e);
        //}
        //#endregion
    }
}
