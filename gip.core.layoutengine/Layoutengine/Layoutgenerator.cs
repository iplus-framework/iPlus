﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Windows.Markup;
using System.Xml.Schema;
using gip.core.datamodel;
using System.Reflection;

namespace gip.core.layoutengine
{
    public class Layoutgenerator 
    {      
        static public string CheckOrUpdateNamespaceInLayout(string xmlLayout)
        {
            if (String.IsNullOrEmpty(xmlLayout))
                return xmlLayout;
            // Beispiel XAML-Layout
            // <StackPanel>
            //      <Button x:Name="BtnKlick">Klick mich</Button>
            //      <TextBox x:Name="TBAusgabe">HalliHallo</TextBox>
            //  </StackPanel>

            // Falls <?xml ....> Deklaration
            int posXmlDekl = xmlLayout.IndexOf("<?xml");
            if (posXmlDekl >= 0)
            {
                // Finde nächste öffnende Klammer
                posXmlDekl = xmlLayout.IndexOf("<", posXmlDekl + 1);
                if (posXmlDekl >= 0)
                {
                    xmlLayout = xmlLayout.Substring(posXmlDekl);
                }
            }

            int pos1 = xmlLayout.IndexOf(">");
            string part1 = "";
            string part2 = "";
            if (pos1 > 0)
            {
                part1 = xmlLayout.Substring(0, pos1);
                part2 = xmlLayout.Substring(pos1);
            }
            // Falls keine Namespace-Deklaration im Root-Element vorhanden, dann füge ein
            if (part1.IndexOf("xmlns") < 0)
            {
                foreach (string nameSpace in ACxmlnsResolver.NamespaceList)
                {
                    part1 += " " + nameSpace;
                }
            }
            part1 += part2;
            return part1;
        }

        static public ResourceDictionary LoadResource(string xmlLayout, IACObject dataContext, IACBSO bso)
        {
            CurrentDataContext = dataContext;
            CurrentBSO = bso;
            using (MemoryStream memoryStream = GetEncodedStream(xmlLayout))
            {
                if (memoryStream == null)
                    return null;

                try
                {
                    ResourceDictionary sp = (ResourceDictionary)XamlReader.Load(memoryStream);
                    return sp;
                }
                catch (XamlParseException e)
                {
                    AsyncMessageBox.BeginMessageBoxAsync(
                        e.Message,
                        "XAML Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return null;
                }
            }
        }

        static private bool _WPFAssembliesLoaded = false;
        static public UIElement LoadLayout(string xmlLayout)
        {
            if (!_WPFAssembliesLoaded)
            {
                foreach (Assembly assembly in ACxmlnsResolver.Assemblies)
                {
                    var queryIsLoaded = AppDomain.CurrentDomain.GetAssemblies().Where(c => c.FullName == assembly.FullName);
                    if (!queryIsLoaded.Any())
                    {
                        AppDomain.CurrentDomain.Load(assembly.FullName);
                    }
                }
                _WPFAssembliesLoaded = true;
            }
            _CurrentDataContext = null;
            _CurrentBSO = null;
            using (MemoryStream memoryStream = GetEncodedStream(xmlLayout))
            {
                if (memoryStream == null)
                    return null;

                try
                {
                    UIElement sp = (UIElement)XamlReader.Load(memoryStream);
                    return sp;
                }
                catch (XamlParseException e)
                {
                    AsyncMessageBox.BeginMessageBoxAsync(
                        e.Message,
                        "XAML Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return null;
                }
            }
        }

        static public DependencyObject LoadXAMLResource(string xmlLayout)
        {
            if (!_WPFAssembliesLoaded)
            {
                foreach (Assembly assembly in ACxmlnsResolver.Assemblies)
                {
                    var queryIsLoaded = AppDomain.CurrentDomain.GetAssemblies().Where(c => c.FullName == assembly.FullName);
                    if (!queryIsLoaded.Any())
                    {
                        AppDomain.CurrentDomain.Load(assembly.FullName);
                    }
                }
                _WPFAssembliesLoaded = true;
            }
            using (MemoryStream memoryStream = GetEncodedStream(xmlLayout))
            {
                if (memoryStream == null)
                    return null;

                try
                {
                    DependencyObject sp = (DependencyObject)XamlReader.Load(memoryStream);
                    return sp;
                }
                catch (XamlParseException e)
                {
                    AsyncMessageBox.BeginMessageBoxAsync(
                        e.Message,
                        "XAML Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return null;
                }
            }
        }


        static protected IACObject _CurrentDataContext;
        static public IACObject CurrentDataContext
        {
            get
            {
                return _CurrentDataContext;
            }
            set
            {
                _CurrentDataContext = value;
                if ((_CurrentDataContext != null) 
                    && (_CurrentDataContext is IACComponent) 
                    && (_Root == null))
                {
                    _Root = (_CurrentDataContext as IACComponent).Root;
                }
            }
        }

        static public IACComponent CurrentACComponent
        {
            get
            {
                if (_CurrentDataContext == null)
                    return null;
                if (_CurrentDataContext is IACComponent)
                    return _CurrentDataContext as IACComponent;
                return null;
            }
        }


        static protected IACBSO _CurrentBSO;
        static public IACBSO CurrentBSO
        {
            get
            {
                return _CurrentBSO;
            }
            set
            {
                _CurrentBSO = value;
                if (_CurrentBSO != null && _Root == null)
                {
                    _Root = _CurrentBSO.Root;
                }
            }
        }
        
        static protected IRoot _Root;
        static public IRoot Root
        {
            get
            {
                return _Root;
            }
        }


        static public UIElement LoadLayout(string xmlLayout, IACObject dataContext, IACBSO bso, string layoutName)
        {
           return LoadXAML(xmlLayout, dataContext, bso, layoutName) as UIElement;
        }

        static public UIElement LoadLayout(ACClassDesign acClassDesign, IACObject dataContext, IACBSO bso, string layoutName)
        {
            if (acClassDesign != null && acClassDesign.BAMLDesign != null && acClassDesign.IsDesignCompiled)
            {
                return LoadBAML(acClassDesign, dataContext, bso) as UIElement;
            }
            else
            {
                return LoadXAML(acClassDesign.XMLDesign, dataContext, bso, layoutName) as UIElement;
            }
        }

        static public object LoadXAML(string xmlLayout, IACObject dataContext, IACBSO bso, string layoutName)
        {
            CurrentDataContext = dataContext;
            CurrentBSO = bso;
            if (!_WPFAssembliesLoaded)
            {
                foreach (Assembly assembly in ACxmlnsResolver.Assemblies)
                {
                    var queryIsLoaded = AppDomain.CurrentDomain.GetAssemblies().Where(c => c.FullName == assembly.FullName);
                    if (!queryIsLoaded.Any())
                    {
                        AppDomain.CurrentDomain.Load(assembly.FullName);
                    }
                }
                _WPFAssembliesLoaded = true;
            }
            using (MemoryStream memoryStream = GetEncodedStream(xmlLayout))
            {
                if (memoryStream == null)
                    return null;
                try
                {
                    return XamlReader.Load(memoryStream);
                }
                catch (Exception e)
                {
                    string errorMessage = "";
                    Exception tmpEc = e;
                    while (tmpEc != null)
                    {
                        errorMessage += tmpEc.Message;
                        tmpEc = tmpEc.InnerException;
                        if (tmpEc != null)
                            errorMessage += " Inner Exception: ";
                    }
                    errorMessage = string.Format("Error loading XAML:{0}BSO:{1}{2}XAMLLayout:{3}{4}Error:{5}",
                        Environment.NewLine, bso != null ? bso.ACIdentifier : "", Environment.NewLine, layoutName, Environment.NewLine, errorMessage);
                    AsyncMessageBox.BeginMessageBoxAsync(
                        errorMessage,
                        "XAML Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return null;
                }
            }
        }

        public static object LoadBAML(ACClassDesign acClassDesign, IACObject dataContext, IACBSO bso)
        {
            CurrentDataContext = dataContext;
            CurrentBSO = bso;
            if (!_WPFAssembliesLoaded)
            {
                foreach (Assembly assembly in ACxmlnsResolver.Assemblies)
                {
                    var queryIsLoaded = AppDomain.CurrentDomain.GetAssemblies().Where(c => c.FullName == assembly.FullName);
                    if (!queryIsLoaded.Any())
                        AppDomain.CurrentDomain.Load(assembly.FullName);
                }
                _WPFAssembliesLoaded = true;
            }
            try
            {
                return BamlReader.Load(acClassDesign.BAMLDesign);
            }
            catch (Exception e)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(e.Message);
#endif
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Layoutgenerator", "LoadBAML", msg);

                return LoadXAML(acClassDesign.XMLDesign, dataContext, bso, acClassDesign.ACIdentifier);
            }
        }

        static public XElement LoadLayoutAsXElement(string xmlLayout)
        {
            using (MemoryStream memoryStream = GetEncodedStream(xmlLayout))
            {
                if (memoryStream == null)
                    return null;
                try
                {
                    XElement sp = XElement.Load(memoryStream);
                    return sp;
                }
                catch (Exception e)
                {
                    AsyncMessageBox.BeginMessageBoxAsync(
                        e.Message,
                        "XAML Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return null;
                }
            }
        }

        static public MemoryStream GetEncodedStream(string xmlLayout)
        {
            if (String.IsNullOrEmpty(xmlLayout))
                return null;
            Encoding aEncoding = null;
            if (xmlLayout.IndexOf("utf-16") > 0 || xmlLayout.IndexOf("UTF-16") > 0)
                aEncoding = System.Text.Encoding.GetEncoding("utf-16");
            else
                aEncoding = System.Text.Encoding.GetEncoding("utf-8");

            xmlLayout = CheckOrUpdateNamespaceInLayout(xmlLayout);

            return new MemoryStream(aEncoding.GetBytes(xmlLayout));
        }

        static public KeyValuePair<string, ACxmlnsInfo> GetNamespaceInfo(UIElement uiElement)
        {
            if (uiElement == null)
                return new KeyValuePair<string, ACxmlnsInfo>();
            return ACxmlnsResolver.GetNamespaceInfo(uiElement.GetType());
        }
    }
}
