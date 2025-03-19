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
        public BSOACClassMessageSelector(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

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

        public ACClassMessage SelectMessage(List<ACClassMessage> messagesList, string acCaption = null, string buttonACCaption = null, string dialogHeader = null)
        {
            ACClassMessageList = messagesList;
            if (acCaption != null)
                SelectedACClassMessageCaption = acCaption;

            if (buttonACCaption != null)
                OkButtonCaption = buttonACCaption;

            ShowDialog(this, "MessagesDialog", dialogHeader);

            return SelectedACClassMessage;
        }
    }
}
