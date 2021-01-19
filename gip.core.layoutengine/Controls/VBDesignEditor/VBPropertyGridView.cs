using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using gip.ext.designer.PropertyGrid;
using gip.ext.designer;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a grid view for properties.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine Rasteransicht für Eigenschaften dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPropertyGridView'}de{'VBPropertyGridView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBPropertyGridView : PropertyGridView
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "PropertyGridViewStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDesignEditor/Themes/PropertyGridViewStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "PropertyGridViewStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDesignEditor/Themes/PropertyGridViewStyleAero.xaml" },
        };

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBPropertyGridView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBPropertyGridView), new FrameworkPropertyMetadata(typeof(VBPropertyGridView)));
        }

        bool _themeApplied = false;

        protected override gip.ext.designer.PropertyGrid.PropertyGrid OnCreatePropertyGrid()
        {
            return new VBPropertyGrid();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);

            //DragEnter += new DragEventHandler(OnDragEnter);
            //DragLeave += new DragEventHandler(OnDragLeave);
            //Drop += new DragEventHandler(OnDrop);
            //DragOver += new DragEventHandler(OnDragOver);

            // Preview MUSSS wegen Textbox und Combobox gemacht werden,
            // Da Textbox und Combobox das verhindern
            PreviewDragEnter += new DragEventHandler(OnDragEnter);
            PreviewDragLeave += new DragEventHandler(OnDragLeave);
            PreviewDrop += new DragEventHandler(OnDrop);
            PreviewDragOver += new DragEventHandler(OnDragOver);

            //PreviewMouseMove += new MouseEventHandler(VBPropertyGridView_PreviewMouseMove);
        }

        //void VBPropertyGridView_PreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    Point x = e.GetPosition(this);
        //    Point y = MouseUtilities.GetMousePosition(this);
        //    Point z = Mouse.GetPosition(this);
        //    System.Diagnostics.Debug.WriteLine("Over " + x.ToString() + " " + y.ToString() + " " + z.ToString());
        //}

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }
        #endregion

        #region DragAndDrop
        void OnDragEnter(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
        }

        void OnDragLeave(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
        }

        void OnDragOver(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            HandleDrop(sender, e);
        }

        public void HandleDragEnter(object sender, DragEventArgs e)
        {
        }

        public void HandleDragLeave(object sender, DragEventArgs e)
        {
        }

        public void HandleDragOver(object sender, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            switch (e.AllowedEffects)
            {
                case DragDropEffects.Move: // Vorhandene Elemente verschieben
                    e.Handled = true;
                    return;
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move:
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    HandleDragOver_Copy(sender, 0, 0, e);
                    return;
                default:
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
            }
        }

        private void HandleDragOver_Copy(object sender, double x, double y, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            CurrentPropertyNode = GetPropertyNode(sender, e, false);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null || CurrentPropertyNode == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das kopieren (einfügen) erlaubt ist
            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Drop);
            if (IsEnabledACAction(actionArgs))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            // Drag und Drop auf Unterelement nicht erlauben!!
            e.Handled = true;
        }

        public void HandleDrop(object sender, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            CurrentPropertyNode = GetPropertyNode(sender, e, true);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            switch (e.AllowedEffects)
            {
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    {
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, 0, 0, Global.ElementActionType.Drop);
                        ACAction(actionArgs);
                        e.Handled = true;
                    }
                    return;
                default:
                    e.Effects = DragDropEffects.None;
                    return;
            }
        }


        private VBPropertyNode GetPropertyNode(object sender, DragEventArgs e, bool IsDrop)
        {
            Point x = e.GetPosition(this);
            HitTestResult result = VisualTreeHelper.HitTest(this, x);
            if (result == null)
                return null;
            if ((result.VisualHit == null) || !(result.VisualHit is DependencyObject))
                return null;
#if DEBUG
            if (IsDrop)
            {
                System.Diagnostics.Debug.WriteLine("GetPropertyNode() P:" + x.ToString() + " Type:" + result.VisualHit.GetType().FullName);
            }
#endif
            Border row;
            VBPropertyNode node = GetNodeOverVisualPos(e.OriginalSource as DependencyObject, out row) as VBPropertyNode;
            return node;
        }

        //private List<HitTestResult> hitTestResults = new List<HitTestResult>();

        //private VBPropertyNode GetPropertyNode(object sender, DragEventArgs e)
        //{
        //    UIElement uiElement = sender as UIElement;
        //    //Point x = Mouse.GetPosition(uiElement);
        //    Point x = e.GetPosition(this);
        //    HitTestResult result = VisualTreeHelper.HitTest(this, x);
        //    hitTestResults.Clear();
        //    VisualTreeHelper.HitTest(this, new HitTestFilterCallback(FilterCallback), new HitTestResultCallback(ResultCallback), new PointHitTestParameters(x));
        //    if (hitTestResults.Count > 1)
        //    {
        //        foreach (HitTestResult hitRes in hitTestResults)
        //        {
        //            System.Diagnostics.Debug.WriteLine("More Items found " + x.ToString() + " at Type " + hitRes.VisualHit.GetType().FullName);
        //        }
        //    }
        //    if (result == null)
        //    {
        //        //this.UpdateLayout();
        //        //x = e.GetPosition(uiElement);
        //        //VisualTreeHelper.HitTest(uiElement, new HitTestFilterCallback(FilterCallback), new HitTestResultCallback(ResultCallback), new PointHitTestParameters(x));
        //        //x.ToString();
        //        //System.Diagnostics.Debug.WriteLine("Point out " + x.ToString());
        //    }
        //    //else 
        //        //System.Diagnostics.Debug.WriteLine("Point in " + x.ToString());

        //    if (result == null)
        //    {
        //        return null;
        //    }
        //    if ((result.VisualHit == null) || !(result.VisualHit is DependencyObject))
        //        return null;
        //    Border row;
        //    if (result.VisualHit is Border)
        //        row = result.VisualHit as Border;
        //    else
        //    {
        //        var ancestors = (result.VisualHit as DependencyObject).GetVisualAncestors();
        //        row = ancestors.OfType<Border>().Where(b => b.Name == "uxPropertyNodeRow").FirstOrDefault();
        //    }
        //        if (row == null)
        //        {
        //            //Point m = e.GetPosition(this.Parent);
        //            Point y = MouseUtilities.GetMousePosition(this);
        //            Point z = Mouse.GetPosition(this);
        //            System.Diagnostics.Debug.WriteLine("Points " + x.ToString() + " " + y.ToString() + " " + z.ToString());
        //            result = VisualTreeHelper.HitTest(this, y);
        //            if (result == null)
        //            {
        //                return null;
        //            }
        //            if ((result.VisualHit == null) || !(result.VisualHit is DependencyObject))
        //                return null;
        //            ancestors = (result.VisualHit as DependencyObject).GetVisualAncestors();
        //            row = ancestors.OfType<Border>().Where(b => b.Name == "uxPropertyNodeRow").FirstOrDefault();
        //            if (row == null)
        //                return null;
        //            else
        //            {
        //                result = null;
        //            }
        //            System.Diagnostics.Debug.WriteLine("Row not found in " + x.ToString() + " at Type " + result.VisualHit.GetType().FullName);
        //            int nCount = 0;
        //            foreach (DependencyObject obj in ancestors)
        //            {
        //                if (obj is FrameworkElement)
        //                {
        //                    System.Diagnostics.Debug.WriteLine("Name " + (obj as FrameworkElement).Name + "Type " + obj.GetType().FullName);
        //                }
        //                nCount++;
        //                if (nCount > 5)
        //                    break;
        //            }

        //            var row2 = ancestors.OfType<Grid>().Where(b => b.Name == "LayoutRoot").FirstOrDefault();
        //            if (row2 != null)
        //            {
        //                row2 = null;
        //            }
        //            return null;
        //        }

        //    if (row.DataContext is VBPropertyNode)
        //        return row.DataContext as VBPropertyNode;
        //    else
        //    {
        //        VBPropertyNode node = row.DataContext as VBPropertyNode;
        //        return null;
        //    }
        //}

        //private HitTestFilterBehavior FilterCallback(DependencyObject target)
        //{
        //    return HitTestFilterBehavior.Continue;
        //}

        //private HitTestResultBehavior ResultCallback(HitTestResult result)
        //{
        //    hitTestResults.Add(result);
        //    return HitTestResultBehavior.Continue;
        //}

        /// <summary>
        /// The ACAction method.
        /// </summary>
        /// <param name="actionArgs">The ACAction arguments.</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            BSOACComponent.ACActionToTarget(CurrentPropertyNode, actionArgs);
        }

        /// <summary>
        /// Determines is enabled ACAction.
        /// </summary>
        /// <param name="actionArgs">The ACAction arguments.</param>
        ///<returns>Returns true if is ACAction enabled, otherwise false.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return BSOACComponent.IsEnabledACActionToTarget(CurrentPropertyNode, actionArgs);
        }

        /// <summary>
        /// Zielobjekt beim ACDropData
        /// </summary>
        VBPropertyNode CurrentPropertyNode
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBPropertyGridView));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }
        #endregion
    }
}
