using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui
{
    public class VBStaticResourceExtension : MarkupExtension
    {
        public static object DataTemplateContext
        {
            get;
            set;
        }

        public VBStaticResourceExtension()
            : base()
        {
        }

        public VBStaticResourceExtension(object resourceKey)
        {
            ResourceKey = resourceKey;
        }

        public object ResourceKey { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object result = null;
            if (serviceProvider is IRootObjectProvider)
            {
                // Parent Object <DockPanel><vbTextbox ControlTheme={VBCurrentThemeExtension ...>
                object rootObject = (serviceProvider as IRootObjectProvider).RootObject;
                if (rootObject != null)
                {
                }
            }

            if ((ResourceKey != null) && (ResourceKey is Type))
            {
                Type controlType = ResourceKey as Type;
                PropertyInfo pi = controlType.GetProperty("StyleInfoList");
                if (pi != null)
                {
                    List<CustomControlStyleInfo> styleInfoList = pi.GetValue(controlType, null) as List<CustomControlStyleInfo>;
                    if (styleInfoList != null)
                    {
                        CustomControlStyleInfo CustomControlStyleInfo = (from o in styleInfoList where o.wpfTheme == ControlManager.WpfTheme select o).FirstOrDefault();
                        if (CustomControlStyleInfo != null)
                        {
                            ResourceInclude dict = new ResourceInclude(new Uri(CustomControlStyleInfo.styleUri, UriKind.Relative));
                            object res;
                            if (!dict.TryGetResource(CustomControlStyleInfo.styleName, null, out res))
                                return null;
                            return res as ControlTheme;

                        }
                    }
                }
            }
            else if ((ResourceKey == null) || (ResourceKey is String))
            {
                IProvideValueTarget targetProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

                Type targetType;
                if (targetProvider.TargetProperty is AvaloniaProperty)
                    targetType = (targetProvider.TargetProperty as AvaloniaProperty).PropertyType;
                else if (targetProvider.TargetProperty is PropertyInfo)
                    targetType = (targetProvider.TargetProperty as PropertyInfo).PropertyType;
                else
                    targetType = targetProvider.TargetProperty.GetType();
                if (targetType == null)
                    return null;
                object targetObject = targetProvider.TargetObject;

                ACClassDesign acClassDesign = null;

                Type typeImageSource = typeof(Bitmap);
                Type typeImageStream = typeof(Stream);
                Type typeBrush = typeof(Brush);
                bool needBitmap = typeImageSource.IsAssignableFrom(targetType);
                bool needBrush = typeBrush.IsAssignableFrom(targetType);
                bool needStream = typeImageStream.IsAssignableFrom(targetType);
                bool needString = typeof(string).IsAssignableFrom(targetType);

                if (ResourceKey == null)
                {
                    if ((targetProvider.TargetObject != null) && (targetProvider.TargetObject is Control) && (targetProvider.TargetProperty != null))
                    {
                        if ((AssemblyResource != null) && (AssemblyResourceKey != null))
                        {
                            try
                            {
                                ResourceInclude dict = new ResourceInclude(AssemblyResource);
                                object resource;
                                if (!dict.TryGetResource(AssemblyResourceKey, null, out resource))
                                    return null;

                                if (resource == null)
                                    return null;
                                if (targetType.IsAssignableFrom(resource.GetType()))
                                    return resource;
                                return null;
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                    datamodel.Database.Root.Messages.LogException("VBStaticResourceExtension", "ProvideValue", msg);
                                return null;
                            }
                        }

                        Control fwElement = targetProvider.TargetObject as Control;
                        object dtCntxt = DataTemplateContext;
                        if (fwElement.DataContext != null)
                            dtCntxt = fwElement.DataContext;
                        if (dtCntxt == null)
                            dtCntxt = Layoutgenerator.CurrentACComponent;

                        if(!string.IsNullOrEmpty(Mode) && Mode.Equals("FindAncestor") && AncestorType != null)
                        {
                            var parent = Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(fwElement, AncestorType);
                            if (parent != null)
                                dtCntxt = parent;
                        }

                        if ((dtCntxt != null) && (dtCntxt is IACObject))
                        {
                            IACObject dataContext = (dtCntxt as IACObject);
                            IACObject objForDesign = dataContext;
                            if (!String.IsNullOrEmpty(VBContent))
                            {
                                object cmdResult = dataContext.ACUrlCommand(VBContent);
                                if ((cmdResult != null) && (cmdResult is IACObject))
                                    objForDesign = cmdResult as IACObject;

                                if(cmdResult is string && !string.IsNullOrEmpty(cmdResult.ToString()))
                                {
                                    ResourceKey = cmdResult.ToString().Replace(Const.ContextDatabase + "\\", "");
                                }
                            }

                            if (objForDesign is DesignManagerToolItem)
                            {
                                try
                                {
                                    DesignManagerToolItem iconProvider = dataContext as DesignManagerToolItem;

                                    ResourceInclude dict = new ResourceInclude(iconProvider.IconResourceDictUri);
                                    object resource;
                                    if (!dict.TryGetResource(iconProvider.ResourceKey, null, out resource))
                                        return null;
                                    if (resource == null)
                                        return null;
                                    if (targetType.IsAssignableFrom(resource.GetType()))
                                        return resource;
                                    return null;
                                }
                                catch (Exception e)
                                {
                                    string msg = e.Message;
                                    if (e.InnerException != null && e.InnerException.Message != null)
                                        msg += " Inner:" + e.InnerException.Message;

                                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                        datamodel.Database.Root.Messages.LogException("VBStaticResourceExtension", "ProvideValue(10)", msg);

                                    return null;
                                }
                            }

                            if ((needBitmap || needBrush || needStream) && ResourceKey == null)
                            {
                                if ((objForDesign is ACObjectItem) && (objForDesign as ACObjectItem).ACObject != null)
                                    objForDesign = (objForDesign as ACObjectItem).ACObject;
                                else if (objForDesign is IACContainer && (objForDesign as IACContainer).Value != null && (objForDesign as IACContainer).Value is IACObject)
                                    objForDesign = (objForDesign as IACContainer).Value as IACObject;
                                if (objForDesign.ACType != null)
                                {
                                    try
                                    {
                                        if (objForDesign is ACClassDesign)
                                            acClassDesign = objForDesign as ACClassDesign;
                                        else
                                            acClassDesign = objForDesign.ACType.GetDesign(objForDesign, Global.ACUsages.DUIcon, Global.ACKinds.DSBitmapResource);
                                        if (acClassDesign != null)
                                        {
                                            ResourceKey = acClassDesign.GetACUrl().Replace(Const.ContextDatabase + "\\", "");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        string msg = e.Message;
                                        if (e.InnerException != null && e.InnerException.Message != null)
                                            msg += " Inner:" + e.InnerException.Message;

                                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                            datamodel.Database.Root.Messages.LogException("VBStatiResourceExtension", "ProvideValue(20)", msg);
                                    }
                                }
                            }
                        }
                    }
                }

                if (ResourceKey == null)
                    return null;

                String strResKey = ResourceKey as String;
                if (targetProvider != null && !String.IsNullOrEmpty(strResKey))
                {
                    if (targetProvider.TargetProperty != null)
                    {
                        // Falls Bitmap-Image oder Brush-Resource
                        if (needBitmap || needBrush)
                        {
                            // Lade Image und Füge es dem Resource-Dicitonary hinzu
                            IACComponent acComponent = Layoutgenerator.CurrentACComponent;
                            if (acComponent == null)
                                acComponent = Layoutgenerator.Root;

                            // Falls WPF-Applikation gestartet und nicht als Service
                            Application wpfApplication = null;
                            if (acComponent.Root.RootPageWPF != null && acComponent.Root.RootPageWPF.WPFApplication != null)
                                wpfApplication = acComponent.Root.RootPageWPF.WPFApplication as Application;
                            if (wpfApplication != null)
                                result = wpfApplication.Resources[strResKey];
                            if (result != null)
                            {
                                if ((needBrush && (result is Brush)) || (needBitmap && (result is Bitmap)) || (needStream && (result is Stream)))
                                    return result;
                                else
                                {
                                    if (needBitmap)
                                        return new Bitmap(AssetLoader.Open(new Uri("avares://gip.core.layoutengine.avui/Images/QuestionMark.JPG")));
                                    else if (needBrush)
                                        return Brushes.Transparent;
                                    else
                                        return null;
                                }
                            }

                            if (acClassDesign == null)
                            {
                                if (strResKey.StartsWith(Const.ContextDatabase + "\\"))
                                {
                                    string dbUrl = strResKey.Substring(9);
                                    acClassDesign = acComponent.Database.ContextIPlus.ACUrlCommand(dbUrl) as ACClassDesign;
                                }
                                else //if (acClassDesign == null)
                                {
                                    if (strResKey.Contains(ACClass.ClassName) || strResKey.Contains(ACClassDesign.ClassName) || strResKey.Contains(ACClassMethod.ClassName) || strResKey.Contains(ACClassProperty.ClassName) || strResKey.Contains(ACProject.ClassName))
                                        acClassDesign = acComponent.Database.ContextIPlus.ACUrlCommand(strResKey) as ACClassDesign;
                                    if (acClassDesign == null)
                                    {
                                        acClassDesign = acComponent.ACUrlCommand(strResKey) as ACClassDesign;
                                        if (acClassDesign == null)
                                            acClassDesign = acComponent.Database.ContextIPlus.ACUrlCommand(strResKey) as ACClassDesign;
                                    }
                                }
                            }

                            if (acClassDesign != null)
                            {
                                if (needBitmap && (acClassDesign.ACKind == Global.ACKinds.DSBitmapResource) && (acClassDesign.DesignBinary != null))
                                {
                                    try
                                    {
                                        Bitmap bitmapImage = new Bitmap(new MemoryStream(acClassDesign.DesignBinary));
                                        if (wpfApplication != null)
                                            wpfApplication.Resources.Add(strResKey, bitmapImage);
                                        return bitmapImage;
                                    }
                                    catch (Exception e)
                                    {
                                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                        {
                                            datamodel.Database.Root.Messages.LogException("VBStaticResourceExtension", "ProvideValue(20)", e);
                                            datamodel.Database.Root.Messages.LogException("VBStaticResourceExtension", "ProvideValue(21)", String.Format("Can't create icon for {0}. Invalid Binary", acClassDesign.GetACUrl()));
                                        }
                                        return new Bitmap(AssetLoader.Open(new Uri("avares://gip.core.layoutengine.avui/Images/QuestionMark.JPG")));
                                    }
                                }
                                else if (needBrush && !String.IsNullOrEmpty(acClassDesign.XMLDesign))
                                {
                                    Brush brushRes = Layoutgenerator.LoadXAMLResource(acClassDesign.XMLDesign) as Brush;
                                    if (brushRes != null)
                                    {
                                        if (wpfApplication != null)
                                            wpfApplication.Resources.Add(strResKey, brushRes);
                                        return brushRes;
                                    }
                                }
                            }

                            if (needBitmap)
                                return new Bitmap(AssetLoader.Open(new Uri("avares://gip.core.layoutengine.avui/Images/QuestionMark.JPG")));
                            else if (needBrush)
                                return Brushes.Transparent;
                            else
                                return null;
                        }
                        // Falls Stream (z.B. Es wurde eine Resource-dictionary angelegt mit einem BitmapImage und die StreamSource-Property wird gesetzt)
                        // Achtung es muss immer das BaseUri Property auf null gesetzt werden: BaseUri="{x:Null}"
                        // Hintergrund: Wird eine BitmapImage per XAML erszeugt, wird die BaseUri defaultmäßig gesetzt und die BitmapImage sucht dann nach einer URI
                        // anstatt die StreamSource-Property zu lesen
                        else if (needStream)
                        {
                            IACComponent acComponent = Layoutgenerator.CurrentACComponent;
                            if (acComponent == null)
                                acComponent = Layoutgenerator.Root;
                            if (acClassDesign == null)
                            {
                                if (strResKey.StartsWith(Const.ContextDatabase + "\\"))
                                {
                                    string dbUrl = strResKey.Substring(9);
                                    acClassDesign = acComponent.Database.ContextIPlus.ACUrlCommand(dbUrl) as ACClassDesign;
                                }
                                else //if (acClassDesign == null)
                                {
                                    if (strResKey.Contains(ACClass.ClassName) || strResKey.Contains(ACClassDesign.ClassName) || strResKey.Contains(ACClassMethod.ClassName) || strResKey.Contains(ACClassProperty.ClassName) || strResKey.Contains(ACProject.ClassName))
                                        acClassDesign = acComponent.Database.ContextIPlus.ACUrlCommand(strResKey) as ACClassDesign;
                                    if (acClassDesign == null)
                                    {
                                        acClassDesign = acComponent.ACUrlCommand(strResKey) as ACClassDesign;
                                        if (acClassDesign == null)
                                            acClassDesign = acComponent.Database.ContextIPlus.ACUrlCommand(strResKey) as ACClassDesign;
                                    }
                                }
                            }
                            if ((acClassDesign != null) && (acClassDesign.ACKind == Global.ACKinds.DSBitmapResource) && (acClassDesign.DesignBinary != null))
                            {
                                return new MemoryStream(acClassDesign.DesignBinary);
                            }
                            return new MemoryStream();
                        }
                        else if (needString)
                        {
                            // Lade Image und Füge es dem Resource-Dicitonary hinzu
                            IACComponent acComponent = Layoutgenerator.CurrentACComponent;
                            if (acComponent == null)
                                acComponent = Layoutgenerator.Root;

                            // Falls WPF-Applikation gestartet und nicht als Service
                            Application wpfApplication = null;
                            if (acComponent.Root.RootPageWPF != null && acComponent.Root.RootPageWPF.WPFApplication != null)
                                wpfApplication = acComponent.Root.RootPageWPF.WPFApplication as Application;
                            if (wpfApplication != null)
                                result = wpfApplication.Resources[strResKey];
                            if (result != null)
                                return result;
                            else
                            {
                                if (strResKey.StartsWith(Const.ContextDatabase + "\\"))
                                {
                                    string dbUrl = strResKey.Substring(9);
                                    result = acComponent.Database.ContextIPlus.ACUrlCommand(dbUrl) as string;
                                    if (result != null)
                                    {
                                        if (wpfApplication != null)
                                            wpfApplication.Resources.Add(strResKey, result);
                                        return result;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            result = new StaticResourceExtension(ResourceKey).ProvideValue(serviceProvider);
            return result;
        }


        private string _VBContent;
        public string VBContent
        {
            get
            {
                return _VBContent;
            }

            set
            {
                _VBContent = value;
            }
        }

        public Uri AssemblyResource
        {
            get;
            set;
        }

        public String AssemblyResourceKey
        {
            get;
            set;
        }

        public string Mode
        {
            get;
            set;
        }

        public Type AncestorType
        {
            get;
            set;
        }

    }
}
