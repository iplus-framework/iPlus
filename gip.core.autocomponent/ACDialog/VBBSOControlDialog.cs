using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
//using gip.core.layoutengine;
//using System.Windows;


namespace gip.core.autocomponent
{
    /// <summary>
    /// Steuerungsdialog für Model- und Workflowkomponenten, die als VBVisual oder VBVisualGroup dargestellt sind
    /// 
    /// In der Steuerungsdialog angezeigt werden kann, muss bei der ACClass oder deren Basisklassen ein ACClassDesign
    /// mit der ACUsage "DUControlDialog" (4230) angelegt sein
    /// 
    /// Modelkomponenten: (Design "ControlDialog" bei PAClassPhysicalBase)
    /// 
    /// 
    /// Workflowkomponenten: (Design "ControlDialog" bei PWBase)
    /// 
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Controldialog'}de{'Steuerungsdialog'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class VBBSOControlDialog : VBBSOSelectionDependentDialog
    {
        public VBBSOControlDialog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._Update = null;
            return base.ACDeInit(deleteACClassTask);
        }

        #region BSO->ACMethod
        [ACMethodCommand("Configuration", "en{'Save Controlproperties'}de{'Steuerelementeigenschaften speichern'}", (short)MISort.Save)]
        public void SaveSolProperties()
        {

            using (ACMonitor.Lock(Root.Database.ContextIPlus.QueryLock_1X000))
            {
                this.Root.Database.ACSaveChanges();
            }
        }
#endregion

        string _Update = "";
        public override string CurrentLayout
        {
            get
            {
                if (CurrentSelection == null)
                    return null;

                ACClassDesign acClassDesign = CurrentSelection.ACType.GetDesign(CurrentSelection, Global.ACUsages.DUControlDialog, Global.ACKinds.DSDesignLayout);
                string layoutXAML = null;
                if (acClassDesign == null)
                {
                    layoutXAML = ACType.GetDesign("Unknown").XMLDesign + _Update;
                }
                else
                {
                    layoutXAML = acClassDesign.XMLDesign + _Update;
                }

                // Sonst reagiert das Steuerelement nicht aufs PropertyChanged
                _Update = _Update == "" ? " " : "";
                return layoutXAML;
            }
        }

        [ACMethodInfo("", "en{'Show parent object'}de{'Zeige Elternobjekt an'}", 999)]
        public void GoToParent()
        {
            IACComponent component = SelectionManager.SelectedACObject as IACComponent;
            if (component != null && component.ParentACComponent != null)
            {
                SelectionManager.HighlightContentACObject(component.ParentACComponent);
                //FrameworkElement element = SelectionManager.SelectedVBControl as FrameworkElement;
                //if (element.Parent == null)
                //    element = element.TemplatedParent as FrameworkElement;
                //else
                //    element = element.Parent as FrameworkElement;
                //if (element != null)
                //    while (element == null || !(element is VBVisual))
                //        element = element.Parent as FrameworkElement;

                //if (element.DataContext == component.ParentACComponent && element is IVBContent)
                //    SelectionManager.OnVBControlClicked(element as IVBContent);
            }
        }

        public bool IsEnabledGoToParent()
        {
            return true;
        }

        private bool _SaveLastSelection = true;
        [ACPropertyInfo(999, "", "en{'Save last selection'}de{'Save last selection'}", "", true)]
        public bool SaveLastSelection
        {
            get
            {
                return _SaveLastSelection;
            }
            set
            {
                _SaveLastSelection = value;
                OnPropertyChanged("SaveLastSelection");
            }
        }

#region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "SaveSolProperties":
                    SaveSolProperties();
                    return true;
                case "GoToParent":
                    GoToParent();
                    return true;
                case Const.IsEnabledPrefix + "GoToParent":
                    result = IsEnabledGoToParent();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
#endregion

    }
}
