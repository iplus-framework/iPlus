using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Message Selector'}de{'Message Selector'}", Global.ACKinds.TACBSOGlobal)]
    public class BSOACClassMessageSelector : ACBSO
    {
        #region c'tors

        public BSOACClassMessageSelector(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        #region Properties

        private ACClassMessage _SelectedACClassMessage;
        [ACPropertySelected(9999, "ACClassMessage", "en{'Select'}de{'Wählen'}")]
        public ACClassMessage SelectedACClassMessage
        {
            get => _SelectedACClassMessage;
            set
            {
                _SelectedACClassMessage = value;
                OnPropertyChanged();
            }
        }

        private List<ACClassMessage> _ACClassMessageList;
        [ACPropertyList(9999, "ACClassMessage")]
        public List<ACClassMessage> ACClassMessageList
        {
            get
            {
                return _ACClassMessageList;
            }
            set
            {
                _ACClassMessageList = value;
                OnPropertyChanged();
            }
        }

        private string _SelectedACClassMessageCaption;
        [ACPropertyInfo(9999)]
        public string SelectedACClassMessageCaption
        {
            get
            {
                return _SelectedACClassMessageCaption;
            }
            set
            {
                _SelectedACClassMessageCaption = value;
                OnPropertyChanged();
            }
        }

        private string _OkButtonCaption;
        [ACPropertyInfo(9999)]
        public string OkButtonCaption
        {
            get
            {
                return _OkButtonCaption;
            }
            set
            {
                _OkButtonCaption = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        [ACMethodInfo("","",9999)]
        public async Task<ACClassMessage> SelectMessage(List<ACClassMessage> messagesList, string acCaption = null, string buttonACCaption = null, string dialogHeader = null)
        {
            ACClassMessageList = messagesList;
            if (acCaption != null)
                SelectedACClassMessageCaption = acCaption;

            if (buttonACCaption != null)
                OkButtonCaption = buttonACCaption;

            await ShowDialogAsync(this, "MessagesDialog", dialogHeader);

            return SelectedACClassMessage;
        }

        #endregion

        #region HandleExecuteACMethod

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;

            switch (acMethodName)
            {
                case nameof(SelectMessage):
                    result = SelectMessage(acParameter[0] as List<ACClassMessage>, acParameter[1] as string, acParameter[2] as string, acParameter[3] as string);
                    return true;
            }

            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
