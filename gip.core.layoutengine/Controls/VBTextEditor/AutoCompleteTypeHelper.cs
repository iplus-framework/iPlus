using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace gip.core.layoutengine
{
    class AutoCompleteTypeHelper
    {
        public List<VBTextEditorCompletionData> AutomCompleteRegisteredTypeNames = new List<VBTextEditorCompletionData>();

        public int FillCompletionDataList(string partOfTypeName, IList<ICompletionData> data)
        {
            int nCopied = 0;
            IEnumerable<VBTextEditorCompletionData> queryList = null;
            if (partOfTypeName.Length <= 0)
                queryList = AutomCompleteRegisteredTypeNames;
            else
            {
                try
                {
                    queryList = AutomCompleteRegisteredTypeNames.Where(c => c.Text.StartsWith(partOfTypeName)).AsEnumerable();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("AutoCompleteTypeHelper", "FillCompletionDataList", msg);
                }
            }
            if (queryList != null && queryList.Any())
            {
                foreach (VBTextEditorCompletionData element in queryList)
                {
                    data.Add(element);
                    nCopied++;
                }
            }
            return nCopied;
        }
    }
}
