using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent.ACDialog
{

    [ACClassInfo(Const.PackName_VarioSystem, "en{'Inputbox'}de{'Eingabebox'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, true, false)]
    public class VBBSOInputBox : ACBSO
    {
        public VBBSOInputBox(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._ACValueItemList = null;
            this._CurrentInputDesign = null;
            return base.ACDeInit(deleteACClassTask);
        }

        #region BSO->ACProperty
        [ACPropertyInfo(9999)]
        public ACValueItem CurrentValue1
        {
            get
            {
                return _ACValueItemList[0];
            }
        }

        [ACPropertyInfo(9999)]
        public ACValueItem CurrentValue2
        {
            get
            {
                return _ACValueItemList[1];
            }
        }
        [ACPropertyInfo(9999)]
        public ACValueItem CurrentValue3
        {
            get
            {
                return _ACValueItemList[2];
            }
        }
        [ACPropertyInfo(9999)]
        public ACValueItem CurrentValue4
        {
            get
            {
                return _ACValueItemList[3];
            }
        }
        [ACPropertyInfo(9999)]
        public ACValueItem CurrentValue5
        {
            get
            {
                return _ACValueItemList[4];
            }
        }
        [ACPropertyInfo(9999)]
        public ACValueItem CurrentValue6
        {
            get
            {
                return _ACValueItemList[5];
            }
        }
        [ACPropertyInfo(9999)]
        public ACValueItem CurrentValue7
        {
            get
            {
                return _ACValueItemList[6];
            }
        }
        [ACPropertyInfo(9999)]
        public ACValueItem CurrentValue8
        {
            get
            {
                return _ACValueItemList[7];
            }
        }
        [ACPropertyInfo(9999)]
        public ACValueItem CurrentValue9
        {
            get
            {
                return _ACValueItemList[8];
            }
        }
        [ACPropertyInfo(9999)]
        public ACValueItem CurrentValue10
        {
            get
            {
                return _ACValueItemList[9];
            }
        }

        ACValueItemList _ACValueItemList;
        #endregion

        [ACMethodInfo("Msg", "en{'Messagebox'}de{'Meldungsfenster'}", 9999)]
        public object[] ShowInputBoxValues(String header, object[] valueList, string[] captionList, string designXML = null)
        {
            _CurrentInputDesign = designXML;
            if (valueList.Count() != captionList.Count())
                return null;
            _ACValueItemList = new ACValueItemList("");
            for (int i = 0; i < valueList.Count(); i++)
            {
                ACValueItem acValueItem = new ACValueItem(captionList[i], valueList[i], null);
                _ACValueItemList.Add(acValueItem);
            }

            ShowDialog(this, "InputBox", header);
            if (_ACValueItemList == null)
                return null;
            List<object> values = new List<object>();
            foreach (var acValueItem in _ACValueItemList)
            {
                values.Add(acValueItem.Value);
            }
            return values.ToArray();
        }

        [ACMethodInfo("Msg", "en{'Messagebox'}de{'Meldungsfenster'}", 9999)]
        public string ShowInputBox(String header, string value, string designXML = null)
        {
            object[] valueList = ShowInputBoxValues(header, new object[] { value }, new string[] { header }, designXML);

            if (valueList == null)
                return null;
            return valueList[0] as string;
        }

        [ACMethodInfo("", "en{'OK'}de{'OK'}", 9999)]
        public void OK()
        {
            CloseTopDialog();
        }

        [ACMethodInfo("", "en{'Cancel'}de{'Abbrechen'}", 9999)]
        public void Cancel()
        {
            _ACValueItemList = null;
            CloseTopDialog();
        }

        string _CurrentInputDesign;
        [ACPropertyInfo(9999)]
        public string CurrentInputDesign
        {
            get
            {
                if (!string.IsNullOrEmpty(_CurrentInputDesign))
                    return _CurrentInputDesign;
                StringBuilder xml = new StringBuilder();
                xml.Append("<vb:VBGrid>");
                xml.Append("<Grid.ColumnDefinitions>");
                xml.Append("<ColumnDefinition></ColumnDefinition>");
                xml.Append("<ColumnDefinition></ColumnDefinition>");
                xml.Append("</Grid.ColumnDefinitions>");
                xml.Append("<Grid.RowDefinitions>");

                foreach (var acValueItem in _ACValueItemList)
                {
                    xml.Append("<RowDefinition></RowDefinition>");
                }

                xml.Append("<RowDefinition></RowDefinition>");

                xml.Append("</Grid.RowDefinitions>");
                xml.Append("<vb:VBFrame Grid.ColumnSpan=\"2\" Grid.RowSpan=\"");
                xml.Append((_ACValueItemList.Count + 1).ToString());
                xml.Append("\"></vb:VBFrame>");

                for (int i = 0; i < _ACValueItemList.Count; i++)
                {
                    var caption = _ACValueItemList[i].ACCaption;
                    Type t = _ACValueItemList[i].Value.GetType();
                    if (t == typeof(System.Boolean))
                    {
                        xml.Append("<vb:VBCheckBox VBContent=\"CurrentValue");
                        xml.Append((i + 1).ToString());
                        xml.Append("\\Value\" Grid.Row=\"");
                        xml.Append(i.ToString());
                        xml.Append("\" Grid.ColumnSpan=\"2\" ACCaption=\"");
                        xml.Append(caption);
                        if (i == 0)
                            xml.Append("\" AutoFocus=\"true\"");
                        else
                            xml.Append("\"");
                        xml.Append("></vb:VBCheckBox>");
                    }
                    else if (t == typeof(System.DateTime))
                    {
                        xml.Append("<vb:VBDateTimePicker VBContent=\"CurrentValue");
                        xml.Append((i + 1).ToString());
                        xml.Append("\\Value\" Grid.Row=\"");
                        xml.Append(i.ToString());
                        xml.Append("\" Grid.ColumnSpan=\"2\" ACCaption=\"");
                        xml.Append(caption);
                        if (i == 0)
                            xml.Append("\" AutoFocus=\"true\"");
                        else
                            xml.Append("\"");
                        xml.Append("></vb:VBDateTimePicker>");
                    }
                    else
                    {
                        xml.Append("<vb:VBTextBox VBContent=\"CurrentValue");
                        xml.Append((i + 1).ToString());
                        xml.Append("\\Value\" Grid.Row=\"");
                        xml.Append(i.ToString());
                        xml.Append("\" Grid.ColumnSpan=\"2\" ACCaption=\"");
                        xml.Append(caption);
                        if (i == 0)
                            xml.Append("\" AutoFocus=\"true\"");
                        else
                            xml.Append("\"");
                        xml.Append("></vb:VBTextBox>");
                    }
                }

                xml.Append("<vb:VBButton VBContent=\"!OK\" Grid.Row=\"");
                xml.Append(_ACValueItemList.Count.ToString());
                xml.Append("\"></vb:VBButton>");

                xml.Append("<vb:VBButton VBContent=\"!Cancel\" Grid.Row=\"");
                xml.Append(_ACValueItemList.Count.ToString());
                xml.Append("\" Grid.Column=\"1\"></vb:VBButton>");

                xml.Append("</vb:VBGrid>");

                return xml.ToString();
            }
        }

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "ShowInputBoxValues":
                    result = ShowInputBoxValues(acParameter[0] as string, acParameter[1] as object[], acParameter[2] as string[], acParameter.Length > 3 ? acParameter[3] as string : null);
                    return true;
                case "ShowInputBox":
                    result = ShowInputBox(acParameter[0] as string, acParameter[1] as string, acParameter.Length > 2 ? acParameter[2] as string : null);
                    return true;
                case "OK":
                    OK();
                    return true;
                case "Cancel":
                    Cancel();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

    }
}
