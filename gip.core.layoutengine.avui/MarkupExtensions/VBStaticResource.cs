using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Reflection;
using System.Xaml;
using System.Windows.Media;
using System.IO;
using gip.core.datamodel;
using System.Windows.Media.Imaging;

namespace gip.core.layoutengine.avui
{
    public class VBStaticResourceExtension : StaticResourceExtension
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
            : base(resourceKey)
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object result = null;
            if (serviceProvider is IRootObjectProvider)
            {
                // Parent Object <DockPanel><vbTextbox Style={VBCurrentThemeExtension ...>
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
                            ResourceDictionary resDict = new ResourceDictionary();
                            resDict.Source = new Uri(CustomControlStyleInfo.styleUri, UriKind.Relative);
                            Style style = resDict[CustomControlStyleInfo.styleName] as Style;
                            if (style != null)
                                return style;
                        }
                    }
                }
            }
            else if ((ResourceKey == null) || (ResourceKey is String))
            {
                IProvideValueTarget targetProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

                Type targetType;
                if (targetProvider.TargetProperty is DependencyProperty)
                    targetType = (targetProvider.TargetProperty as DependencyProperty).PropertyType;
                else if (targetProvider.TargetProperty is PropertyInfo)
                    targetType = (targetProvider.TargetProperty as PropertyInfo).PropertyType;
                else
                    targetType = targetProvider.TargetProperty.GetType();
                if (targetType == null)
                    return null;
                object targetObject = targetProvider.TargetObject;

                ACClassDesign acClassDesign = null;

                Type typeImageSource = typeof(ImageSource);
                Type typeImageStream = typeof(Stream);
                Type typeBrush = typeof(Brush);
                bool needBitmap = typeImageSource.IsAssignableFrom(targetType);
                bool needBrush = typeBrush.IsAssignableFrom(targetType);
                bool needStream = typeImageStream.IsAssignableFrom(targetType);
                bool needString = typeof(string).IsAssignableFrom(targetType);

                if (ResourceKey == null)
                {
                    if ((targetProvider.TargetObject != null) && (targetProvider.TargetObject is FrameworkElement) && (targetProvider.TargetProperty != null))
                    {
                        if ((AssemblyResource != null) && (AssemblyResourceKey != null))
                        {
                            try
                            {
                                ResourceDictionary dict = new ResourceDictionary();
                                dict.Source = AssemblyResource;
                                object resource = dict[AssemblyResourceKey];
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

                        FrameworkElement fwElement = targetProvider.TargetObject as FrameworkElement;
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
                                    ResourceDictionary dict = new ResourceDictionary();
                                    dict.Source = iconProvider.IconResourceDictUri;
                                    object resource = dict[iconProvider.ResourceKey];
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
                                if ((needBrush && (result is Brush)) || (needBitmap && (result is ImageSource)) || (needStream && (result is Stream)))
                                    return result;
                                else
                                {
                                    if (needBitmap)
                                        return new BitmapImage(new Uri("pack://application:,,,/gip.core.layoutengine.avui;component/Images/QuestionMark.JPG", UriKind.Absolute));
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
                                        BitmapImage bitmapImage = new BitmapImage();
                                        bitmapImage.BeginInit();
                                        bitmapImage.StreamSource = new MemoryStream(acClassDesign.DesignBinary);
                                        bitmapImage.EndInit();

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
                                        return new BitmapImage(new Uri("pack://application:,,,/gip.core.layoutengine.avui;component/Images/QuestionMark.JPG", UriKind.Absolute));
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
                                return new BitmapImage(new Uri("pack://application:,,,/gip.core.layoutengine.avui;component/Images/QuestionMark.JPG", UriKind.Absolute));
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
            

            result = base.ProvideValue(serviceProvider);
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
