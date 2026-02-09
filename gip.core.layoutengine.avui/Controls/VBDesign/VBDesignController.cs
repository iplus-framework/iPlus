using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using DialogHostAvalonia;
using DocumentFormat.OpenXml.Bibliography;
using DynamicData.Kernel;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    public class VBDesignController : TransitioningContentControl, IVBGui
    {
        public VBDesignController() : base()
        {
            _DesignsStack = new List<VBDesign>();
            IsMainDesignActive = true;
        }

        public VBDesign MainDesign
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty = AvaloniaProperty.Register<VBDockingManager, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        public bool IsMainDesignActive
        {
            get
            {
                return (bool)GetValue(IsMainDesignActiveProperty);
            }
            set
            {
                SetValue(IsMainDesignActiveProperty, value);
            }
        }

        public static readonly StyledProperty<bool> IsMainDesignActiveProperty =
            AvaloniaProperty.Register<VBDesign, bool>(nameof(IsMainDesignActive));


        private List<VBDesign> _DesignsStack;
        public List<VBDesign> DesignsStack
        {
            get => _DesignsStack;
        }

        public List<IVBDialog> DialogStack => throw new NotImplementedException();

        public IACObject ParentACObject => throw new NotImplementedException();

        public IACType ACType => throw new NotImplementedException();

        public IEnumerable<IACObject> ACContentList => throw new NotImplementedException();

        public string ACIdentifier => Name;

        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBDesignController, string>(nameof(ACCaption), "");

        public string ACCaption
        {
            get => GetValue(ACCaptionProperty);
            set => SetValue(ACCaptionProperty, value);
        }


        public void ShowDesign(string designName)
        {
            ACClassDesign acClassDesign = MainDesign.BSOACComponent.GetDesign(designName);
            if (acClassDesign != null)
            {
                VBDesign vbDesign = new VBDesign();
                vbDesign.DataContext = MainDesign.BSOACComponent;
                vbDesign.VBContent = "*" + acClassDesign.ACIdentifier;

                _DesignsStack.Add(vbDesign);
                Content = vbDesign;
                ACCaption = vbDesign.ACCaption;

                IsMainDesignActive = false;
            }
        }

        public void CloseDesign()
        {
            if (DesignsStack.Count > 1)
            {
                DesignsStack.RemoveAt(DesignsStack.Count - 1);
                VBDesign design = DesignsStack.LastOrDefault();
                Content = design;
                ACCaption = design.ACCaption;

                if (Content == MainDesign)
                    IsMainDesignActive = true;
            }
        }

        public void AddMainDesign(VBDesign design)
        {
            if (MainDesign != null && MainDesign.BSOACComponent != null)
                MainDesign.BSOACComponent.Stop();

            _DesignsStack.Add(design);
            MainDesign = design;
            Content = design;
            IsMainDesignActive = true;

            ACCaption = MainDesign.ACCaption;

            this.ClearBinding(ACUrlCmdMessageProperty);

            var binding = new Binding
            {
                Source = MainDesign.BSOACComponent,
                Path = Const.ACUrlCmdMessage,
                Mode = BindingMode.OneWay
            };
            this.Bind(ACUrlCmdMessageProperty, binding);
        }

        public void ShowDialog(IACComponent forObject, string acClassDesignName, string acCaption = "", bool isClosableBSORoot = false, Global.ControlModes ribbonVisibility = Global.ControlModes.Hidden, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {
            //TODO: Implement dialog showing
        }

        public async Task ShowDialogAsync(IACComponent forObject, string acClassDesignName, string acCaption = "", bool isClosableBSORoot = false, Global.ControlModes ribbonVisibility = Global.ControlModes.Hidden, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {
            VBDesign vbDesign = new VBDesign();
            vbDesign.DataContext = forObject;
            vbDesign.VBContent = "*" + acClassDesignName;

            DialogHostStyles style = null;
            if (!this.Styles.OfType<DialogHostStyles>().Any())
            {
                style = new DialogHostStyles();
                this.Styles.Add(style);
            }

            var parentContent = this.Content;

            var dh = new DialogHost
            {
                Identifier = "Dialog" + Guid.NewGuid(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
            };

            this.Content = null;
            dh.Content = parentContent;

            // Fullscreen wrapper root (scrim + your content)
            var fullscreenRoot = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            fullscreenRoot.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xAA, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            });
            fullscreenRoot.Children.Add(new Border
            {
                Background = Brushes.Transparent, // or White, etc.
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Child = vbDesign
            });

            // Force wrapper to match host bounds (this is the key)
            void SyncToHostBounds()
            {
                var w = dh.Bounds.Width;
                var h = dh.Bounds.Height;
                if (w > 0) fullscreenRoot.Width = w;
                if (h > 0) fullscreenRoot.Height = h;
            }

            var boundsSub = dh.GetObservable(Visual.BoundsProperty).Subscribe(_ => SyncToHostBounds());
            SyncToHostBounds();

            this.Content = dh;

            this.SetCloseAction(() =>
            {
                boundsSub.Dispose();

                if (dh.CurrentSession != null && dh.CurrentSession.IsEnded == false)
                    DialogHost.Close(dh.Identifier);

                this.Content = null;
                dh.Content = null;
                this.Content = parentContent;

                if (style != null)
                    this.Styles.Remove(style);
            });

            await DialogHost.Show(fullscreenRoot, dh.Identifier);
        }

        private Action _CloseAction;

        private void SetCloseAction(Action value)
        {
            _CloseAction = value;
        }

        public void ShowWindow(IACComponent forObject, string acClassDesignName, bool isClosableBSORoot, Global.VBDesignContainer containerType, Global.VBDesignDockState dockState, Global.VBDesignDockPosition dockPosition, Global.ControlModes ribbonVisibility, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {
            if (containerType == Global.VBDesignContainer.SingleView)
            {
                 ShowDesign(acClassDesignName);
            }
        }

        public void CloseTopDialog()
        {
            _CloseAction?.Invoke();
        }

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        public string GetACUrl(IACObject rootACObject = null)
        {
            return this.ReflectGetACUrl(rootACObject);
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {

            if (change.Property == ACUrlCmdMessageProperty)
                OnACUrlMessageReceived();

            base.OnPropertyChanged(change);

        }

        public void OnACUrlMessageReceived()
        {
            if (ACUrlCmdMessage != null && ACUrlCmdMessage.ACUrl == Const.CmdFindGUI)
            {
                try
                {
                    IACObject invoker = (IACObject)ACUrlCmdMessage.ACParameter[0];
                    string filterVBControlClassName = (string)ACUrlCmdMessage.ACParameter[1];
                    string filterControlName = (string)ACUrlCmdMessage.ACParameter[2];
                    string filterVBContent = (string)ACUrlCmdMessage.ACParameter[3];
                    string filterACNameOfComponent = (string)ACUrlCmdMessage.ACParameter[4];
                    bool withDialogStack = (bool)ACUrlCmdMessage.ACParameter[5];

                    bool filterVBControlClassNameSet = !String.IsNullOrEmpty(filterVBControlClassName);
                    bool filterControlNameSet = !String.IsNullOrEmpty(filterControlName);
                    bool filterACNameOfComponentSet = !String.IsNullOrEmpty(filterACNameOfComponent);
                    bool filterVBContentSet = !String.IsNullOrEmpty(filterVBContent);
                    if (!filterVBControlClassNameSet && !filterControlNameSet && !filterACNameOfComponentSet && !filterVBContentSet)
                        return;

                    //if (ACUrlHelper.IsSearchedGUIInstance(ACIdentifier, filterVBControlClassName, filterControlName, filterVBContent, filterACNameOfComponent))
                    //{
                    //    if (withDialogStack)
                    //    {
                    //        if (DialogStack.Any())
                    //            invoker.ACUrlCommand(Const.CmdFindGUIResult, this);
                    //    }
                    //    else
                            invoker.ACUrlCommand(Const.CmdFindGUIResult, this);
                    //}
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBDockingManager", "OnACUrlMessagereceived", msg);
                }
            }
        }
    }
}
