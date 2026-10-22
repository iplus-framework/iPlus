using System;
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
using System.Text.RegularExpressions;

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
                bool isConvertedFromAvalonia = false;
                string xamlToLoad = GetWpfDesignXaml(acClassDesign, ref isConvertedFromAvalonia);
                object result = LoadXAML(xamlToLoad, dataContext, bso, layoutName);

                // Auto-save the reverse-converted WPF XAML on successful runtime load
                // (VBDesign/VBVisual). The XAML designer binds ACClassDesign.XAMLDesign
                // directly and does NOT run through the Layoutgenerator, so there the
                // user controls saving via the save button.
                if (result != null && isConvertedFromAvalonia && acClassDesign != null)
                {
                    AutoSaveWpfDesign(acClassDesign, xamlToLoad);
                }
                return result as UIElement;
            }
        }

        /// <summary>
        /// Resolves the WPF XAML for a design. If the WPF design (XMLDesign) exists it is
        /// returned unchanged. If only an Avalonia design exists (XMLDesign2, authored
        /// directly in Avalonia), it is converted back to WPF XAML via
        /// XAMLConversionHelper.ConvertAvaloniaToWpfXaml and isConvertedFromAvalonia is
        /// set to true. The converted result is also cached in memory by
        /// ACClassDesign.XAMLDesign (CacheConvertedDesign, without database write).
        /// </summary>
        static public string GetWpfDesignXaml(ACClassDesign acClassDesign, ref bool isConvertedFromAvalonia)
        {
            isConvertedFromAvalonia = false;
            if (acClassDesign == null)
                return null;

            if (!String.IsNullOrEmpty(acClassDesign.XMLDesign))
                return acClassDesign.XMLDesign;

            if (!String.IsNullOrEmpty(acClassDesign.XMLDesign2))
            {
                isConvertedFromAvalonia = true;
                return XAMLConversionHelper.ConvertAvaloniaToWpfXaml(acClassDesign.XMLDesign2);
            }

            return null;
        }

        /// <summary>
        /// Auto-saves a successfully reverse-converted WPF XAML to XMLDesign and marks
        /// both designs as synchronized. Only called from runtime load paths.
        /// </summary>
        static private void AutoSaveWpfDesign(ACClassDesign acClassDesign, string wpfXaml)
        {
            try
            {
                acClassDesign.XMLDesign = wpfXaml;
                acClassDesign.XMLDesign2UpdateDate = DateTime.Now;
                acClassDesign.XMLDesignUpdateDate = acClassDesign.XMLDesign2UpdateDate;
                if (acClassDesign.Context != null)
                {
                    var msgWithDetails = acClassDesign.Context.ACSaveChanges();
                    if (msgWithDetails != null && Root != null && Root.Messages != null)
                    {
                        Root.Messages.LogMessageMsg(msgWithDetails);
                    }
                }
            }
            catch
            {
                // Ignore caching errors, the conversion still worked
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
            try
            {
                string converted = CheckOrUpdateNamespaceInLayout(xmlLayout);
                object result;
                DataTemplate cachedTemplate = GetOrCompileTemplate(converted);
                if (cachedTemplate != null)
                {
                    result = cachedTemplate.LoadContent();
                }
                else
                {
                    result = XamlReader.Parse(converted);
                }
                return result;
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

        #region XAML-Template-Cache

        // Caches compiled XAML as DataTemplate factories (like DataTemplates, each
        // LoadContent() creates a fresh control tree). Markup extensions (VBBindingExt
        // etc.) run inside LoadContent(), so they still resolve against the static
        // CurrentDataContext/CurrentBSO that LoadXAML sets before each build.
        static readonly object _templateCacheLock = new object();
        static readonly Dictionary<string, DataTemplate> _templateCache = new Dictionary<string, DataTemplate>();

        // Each cached template holds compiled BAML state; cap the cache and clear it
        // wholesale when exceeded (simple, avoids unbounded memory growth).
        const int MaxCachedTemplates = 500;

        /// <summary>
        /// Gets a cached compiled factory for the given (already namespace-converted) XAML,
        /// or compiles and caches one. Returns null if the XAML cannot be wrapped into a
        /// template (caller falls back to a plain XamlReader.Parse).
        /// </summary>
        static DataTemplate GetOrCompileTemplate(string convertedXaml)
        {
            if (string.IsNullOrEmpty(convertedXaml))
                return null;

            // A/B test switch: set IPLUS_DISABLE_TEMPLATE_CACHE=1 to compare the cached
            // path against the plain XamlReader.Parse path.
            if (Environment.GetEnvironmentVariable("IPLUS_DISABLE_TEMPLATE_CACHE") == "1")
                return null;

            string key = HashXaml(convertedXaml);
            lock (_templateCacheLock)
            {
                if (_templateCache.TryGetValue(key, out var cached))
                    return cached;
            }

            try
            {
                DataTemplate template = WrapAsDataTemplate(convertedXaml);
                if (template == null)
                    return null;

                // Verify the template actually builds before caching it, so a design
                // that fails inside the wrapper never poisons the cache.
                template.LoadContent();

                lock (_templateCacheLock)
                {
                    if (_templateCache.Count >= MaxCachedTemplates)
                        _templateCache.Clear();
                    _templateCache[key] = template;
                }
                return template;
            }
            catch
            {
                // Wrapping not possible for this design (e.g. x:Class root, parser
                // restrictions) - caller falls back to the plain load path.
                return null;
            }
        }

        static string HashXaml(string xaml)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(xaml));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Wraps a design XAML root into a DataTemplate so the compiled template can be
        /// reused for every subsequent LoadContent(). All xmlns declarations of the
        /// design root are moved to the DataTemplate root, because WPF only allows
        /// xmlns on the root element.
        /// </summary>
        static DataTemplate WrapAsDataTemplate(string convertedXaml)
        {
            convertedXaml = RewriteVBBindingForTemplate(convertedXaml);
            XDocument doc = XDocument.Parse(convertedXaml);
            XElement designRoot = doc.Root;
            if (designRoot == null)
                return null;

            // ResourceDictionaries and similar non-control roots cannot be templated.
            if (designRoot.Name.LocalName == "ResourceDictionary")
                return null;

            XNamespace wpfNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XElement templateRoot = new XElement(wpfNs + "DataTemplate");

            // Re-declare every namespace of the design root on the template root,
            // preserving prefixes, then strip them from the child element.
            var nsDeclarations = designRoot.Attributes().Where(a => a.IsNamespaceDeclaration).ToList();
            foreach (var nsDecl in nsDeclarations)
            {
                string prefix = nsDecl.Name.LocalName == "xmlns" ? "" : nsDecl.Name.LocalName;
                templateRoot.Add(new XAttribute(nsDecl.Name.Namespace == XNamespace.Xmlns && prefix != ""
                    ? XNamespace.Xmlns + prefix : nsDecl.Name, nsDecl.Value));
            }
            foreach (var nsDecl in nsDeclarations)
                nsDecl.Remove();

            // x:Class has no meaning inside a template and breaks compilation.
            designRoot.Attributes("{http://schemas.microsoft.com/winfx/2006/xaml}Class").Remove();

            templateRoot.Add(designRoot);

            return XamlReader.Parse(templateRoot.ToString()) as DataTemplate;
        }

        /// <summary>
        /// A WPF Binding (VBBinding derives from Binding) is sealed once a
        /// BindingExpression has been created from it, so the single VBBinding instance
        /// parsed inside a cached DataTemplate cannot be re-pointed per instantiation.
        /// Instead, rewrite vb:VBBinding to vb:VBBindingExt (a MarkupExtension that
        /// creates a fresh, correctly resolved Binding per ProvideValue call) before
        /// the template is parsed. The prefix bound to the iplus XAML namespace is
        /// detected from the root element, since designs may use any prefix.
        /// </summary>
        static string RewriteVBBindingForTemplate(string convertedXaml)
        {
            Match nsMatch = Regex.Match(convertedXaml, "xmlns:([\\w.-]+)\\s*=\\s*\"http://www\\.iplus-framework\\.com/xaml\"");
            if (!nsMatch.Success)
                return convertedXaml;
            string prefix = nsMatch.Groups[1].Value;
            // Matches both "prefix:VBBinding" and "prefix:VBBindingExt" (kept as-is);
            // the lookahead prevents rewriting longer type names like VBBindingWrapper.
            string rewritten = Regex.Replace(convertedXaml,
                Regex.Escape(prefix) + ":VBBinding(Ext)?(?![A-Za-z])",
                prefix + ":VBBindingExt");
            return rewritten;
        }

        #endregion

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

                bool isConvertedFromAvalonia = false;
                string xamlToLoad = GetWpfDesignXaml(acClassDesign, ref isConvertedFromAvalonia);
                object result = LoadXAML(xamlToLoad, dataContext, bso, acClassDesign.ACIdentifier);

                // Auto-save the reverse-converted WPF XAML on successful runtime load
                // (see LoadLayout(ACClassDesign, ...)).
                if (result != null && isConvertedFromAvalonia && acClassDesign != null)
                {
                    AutoSaveWpfDesign(acClassDesign, xamlToLoad);
                }
                return result;
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
