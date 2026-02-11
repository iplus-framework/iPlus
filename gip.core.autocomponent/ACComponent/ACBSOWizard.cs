// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Baseclass Wizard'}de{'Basisklasse Wizard'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, true, true)]
    public abstract class ACBSOWizard : ACBSO
    {
        #region c´tors
        public ACBSOWizard(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            return true;
        }
        #endregion

        #region BSO->ACProperty
        public abstract List<ACClassDesign> WizardDesignList { get; }

        ACClassDesign _CurrendWizardDesign;
        public ACClassDesign CurrendWizardDesign
        {
            get
            {
                return _CurrendWizardDesign;
            }
            set
            {
                _CurrendWizardDesign = value;

                var pos = WizardDesignList.IndexOf(value);
                if (pos > 0)
                    CaptionPrev = WizardDesignList[pos - 1].ACCaption;
                else
                    CaptionPrev = "";

                if (pos < WizardDesignList.Count() - 1)
                    CaptionNext = WizardDesignList[pos + 1].ACCaption;
                else
                    CaptionNext = "Finish";

                OnPropertyChanged("CurrendWizardDesign");
                OnPropertyChanged("CurrentLayout");
            }
        }
        #endregion

        #region BSO->ACMethod
        [ACMethodCommand("Report", "en{'Wizard'}de{'Wizard'}", 9999)]
        public async void ShowWizardDlg()
        {
            CurrendWizardDesign = WizardDesignList.First();
            await ShowDialogAsync(this, "WizardDlg");
        }

        [ACMethodCommand("Wizard", "en{'Next'}de{'Nächste'}", (short)MISort.WizardNext)]
        public void NextPage()
        {
            var pos = WizardDesignList.IndexOf(CurrendWizardDesign);
            if (pos < WizardDesignList.Count() - 1)
            {
                CurrendWizardDesign = WizardDesignList[pos + 1];
            }
            else
            {
                if (Finish())
                {
                    CloseTopDialog();
                }
            }
        }

        public bool IsEnabledNextPage()
        {
            var pos = WizardDesignList.IndexOf(CurrendWizardDesign);
            bool isFinish = !(pos < WizardDesignList.Count() - 1);
            return CheckNextPage(isFinish);
        }

        public abstract bool CheckNextPage(bool isFinish);
        public abstract bool Finish();

        [ACMethodCommand("Wizard", "en{'Prev'}de{'Vorherige'}", (short)MISort.WizardPrev)]
        public void PrevPage()
        {
            var pos = WizardDesignList.IndexOf(CurrendWizardDesign);
            CurrendWizardDesign = WizardDesignList[pos - 1];
        }

        public bool IsEnabledPrevPage()
        {
            var pos = WizardDesignList.IndexOf(CurrendWizardDesign);
            return pos > 0;
        }

        [ACMethodCommand("Wizard", Const.Cancel, (short)MISort.Cancel)]
        public void Cancel()
        {
            CloseTopDialog();
        }
        #endregion

        #region Layoutsteuerung
        string _CaptionPrev;
        public string CaptionPrev
        {
            get
            {
                return _CaptionPrev;
            }
            set
            {
                _CaptionPrev = value;
                OnPropertyChanged("CaptionPrev");
            }
        }

        string _CaptionNext;
        public string CaptionNext
        {
            get
            {
                return _CaptionNext;
            }
            set
            {
                _CaptionNext = value;
                OnPropertyChanged("CaptionNext");
            }
        }


        string _Update = "";
        public string CurrentLayout
        {
            get
            {
                //ACClassDesign acClassDesign = CurrentSelection.ACType.GetDesign(CurrentSelection, Global.ACUsages.DUMain, Global.ACKinds.DSDesignLayout);
                string layoutXAML = null;
                //if (acClassDesign == null)
                //{
                //    layoutXAML = ACType.MyACClassDesign("Unknown").XMLDesign + _Update;
                //}
                //else
                //{
                //    layoutXAML = acClassDesign.XMLDesign + _Update;
                //}

                layoutXAML = "<vb:VBDockPanel><vb:VBTextBlock ACCaption=\"" + CurrendWizardDesign + "\"></vb:VBTextBlock></vb:VBDockPanel>";


                // Sonst reagiert das Steuerelement nicht aufs PropertyChanged
                _Update = _Update == "" ? " " : "";
                return layoutXAML;
            }
        }
        #endregion
    }
}
