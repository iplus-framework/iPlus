using System;
using System.Diagnostics;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
//using PixelLab.Common;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gip.core.layoutengine.CodeCompletion
{
    /// <summary>
    /// Enum for completion data provider key result.
    /// </summary>
    public enum CompletionDataProviderKeyResult
    {
        NormalKey = 0,
        InsertionKey = 1,
        BeforeStartKey = 2
    }

    /// <summary>
    /// Interface for completion data provider.
    /// </summary>
    public interface ICompletionDataProvider
    {
        /// <summary>
        /// Gets the image list.
        /// </summary>
        ImageList ImageList
        {
            get;
        }

        /// <summary>
        /// Gets the pre selection.
        /// </summary>
        string PreSelection
        {
            get;
        }

        /// <summary>
        /// Gets the default index.
        /// </summary>
        int DefaultIndex
        {
            get;
        }

        /// <summary>
        /// The process key.
        /// </summary>
        /// <param name="key">The char key.</param>
        /// <returns>Completion data provider key result.</returns>
        CompletionDataProviderKeyResult ProcessKey(char key);

        /// <summary>
        /// The insert action.
        /// </summary>
        /// <param name="data">The data for insert.</param>
        /// <param name="textArea">The text area parameter.</param>
        /// <param name="insertionOffset">The isnertion offset.</param>
        /// <param name="key">The char key.</param>
        /// <returns>True if insertion is successfull, otherwise false.</returns>
        bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key);

        /// <summary>
        /// Generates the completion data.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="textArea">The text area.</param>
        /// <param name="charTyped">The typed char.</param>
        /// <returns>The Array of a completion data.</returns>
        ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped);
    }

    /// <summary>
    /// Represents the xml completion provider.
    /// </summary>
    public class XmlCompletionDataProvider : ICompletionDataProvider
    {

        #region Static Fields

        XmlSchemaCompletionData defaultSchemaCompletionData = null;

        #endregion Static Fields

        #region Fields


        protected string preSelection = null;
        string defaultNamespacePrefix = String.Empty;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Determines is code completion schema loaded.
        /// </summary>
        public bool IsSchemaLoaded
        {
            get { return (defaultSchemaCompletionData != null); }
        }


        #endregion Properties

        #region Static Methods

        /// <summary>
        /// Loads code comepletion schema from file.
        /// </summary>
        /// <param name="filename">The path of code completion schema file.</param>
        public void LoadSchema(string filename)
        {
            Task.Run(() => { LoadSchemaFromFile(filename); });
//            Func<Exception> doLoad = () => LoadSchemaFromFile(filename);
//            doLoad.BeginInvoke((result) =>
//            {
//                var ex = doLoad.EndInvoke(result);
//                if (ex != null)
//                {
//                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null)
//                        datamodel.Database.Root.Messages.Warning(datamodel.Database.Root, "Fail to load schema. Exception: " + ex.Message, true);


//#if DEBUG
//                    Debug.WriteLine(ex);
//#endif
//                }
//            }, null);
        }

        private Exception LoadSchemaFromFile(string filename)
        {
            try
            {
                defaultSchemaCompletionData = new XmlSchemaCompletionData(filename);
                return null;
            }
            catch (Exception ex)
            {
                if (false)
                {
                    throw;
                }
                else
                {
                    return ex;
                }
            }
        }

        #endregion Static Methods

        #region ICompletionDataProvider Members

        /// <summary>
        /// Gets the default index.
        /// </summary>
        public int DefaultIndex
        {
            get { return 0; }
        }

        /// <summary>
        /// Generates the completion data.
        /// </summary>
        /// <param name="prefixName">The prefix name.</param>
        /// <param name="textArea">The text area.</param>
        /// <param name="charTyped">The typed char.</param>
        /// <returns>The array of a completion data.</returns>
        public ICompletionData[] GenerateCompletionData(string prefixName, TextArea textArea, char charTyped)
        {
            string text = String.Concat(textArea.Document.GetText(0, textArea.Caret.Offset), charTyped);
            var declNs = CheckDeclaredNamespaces(text);

            switch (charTyped)
            {
                case '<':
                    // Child element intellisense.
                    XmlElementPath parentPath = XmlParser.GetParentElementPath(text);
                    if (parentPath.Elements.Count > 0)
                    {
                        ICompletionData[] data = GetChildElementCompletionData(parentPath, declNs);
                        return data;
                    }
                    else if (defaultSchemaCompletionData != null)
                    {
                        return defaultSchemaCompletionData.GetElementCompletionData();
                    }
                    break;

                default:
                case ' ':
                    if (!Char.IsLetter(charTyped))
                        break;

                    // Attribute intellisense.
                    if (!XmlParser.IsInsideAttributeValue(text, text.Length))
                    {
                        XmlElementPath path = XmlParser.GetActiveElementStartPath(text, text.Length);
                        if (path.Elements.Count > 0)
                        {
                            return GetAttributeCompletionData(path);
                        }
                    }
                    break;

                case '\'':
                case '\"':

                    // Attribute value intellisense.
                    //if (XmlParser.IsAttributeValueChar(charTyped)) {
                    text = text.Substring(0, text.Length - 1);
                    string attributeName = XmlParser.GetAttributeName(text, text.Length);
                    if (attributeName.Length > 0)
                    {
                        XmlElementPath elementPath = XmlParser.GetActiveElementStartPath(text, text.Length);
                        if (elementPath.Elements.Count > 0)
                        {
                            preSelection = charTyped.ToString();
                            return GetAttributeValueCompletionData(elementPath, attributeName);
                            //		}
                        }
                    }
                    break;
                case ':':
                    XmlElementPath pPath = XmlParser.GetParentElementPath(text);
                    return defaultSchemaCompletionData.GetElementCompletionData(prefixName, defaultSchemaCompletionData.Schema, pPath);
            }

            return null;

        }

        
        ICompletionData[] GetChildElementCompletionData(XmlElementPath path, Dictionary<string,string> declNs)
        {
            ICompletionData[] completionData = null;

            XmlSchemaCompletionData schema = defaultSchemaCompletionData;
            if (schema != null)
            {
                completionData = schema.GetChildElementCompletionData(path, declNs);
            }

            return completionData;
        }

        ICompletionData[] GetAttributeCompletionData(XmlElementPath path)
        {
            ICompletionData[] completionData = null;

            XmlSchemaCompletionData schema = defaultSchemaCompletionData;
            if (schema != null)
            {
                completionData = schema.GetAttributeCompletionData(path);
            }

            return completionData;
        }

        ICompletionData[] GetAttributeValueCompletionData(XmlElementPath path, string name)
        {
            ICompletionData[] completionData = null;

            XmlSchemaCompletionData schema = defaultSchemaCompletionData;
            if (schema != null)
            {
                completionData = schema.GetAttributeValueCompletionData(path, name);
            }

            return completionData;
        }

        ImageList _ImageList;
        /// <summary>
        /// Gets the image list;
        /// </summary>
        public ImageList ImageList
        {
            get
            {
                if (_ImageList == null)
                {
                    _ImageList = new ImageList();
                    //_ImageList.Images.Add(new System.Drawing.Bitmap(@"C:\element2.png"));

                }

                return _ImageList;
            }
        }

        /// <summary>
        /// The insert action.
        /// </summary>
        /// <param name="data">The completion data.</param>
        /// <param name="textArea">The text area.</param>
        /// <param name="insertionOffset">The insertion offset.</param>
        /// <param name="key">The char key.</param>
        /// <returns>True if insert successfull, otherwise false.</returns>
        public bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
        {
            //textArea.InsertString(data.Text);
            return false;
            //throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets the pre selection.
        /// </summary>
        public string PreSelection
        {
            get
            {
                return "";
            }

            //get { throw new Exception("The method or operation is not implemented."); }
        }

        /// <summary>
        /// The process key.
        /// </summary>
        /// <param name="key">The char key.</param>
        /// <returns>The completion data provider normal key result.</returns>
        public CompletionDataProviderKeyResult ProcessKey(char key)
        {
            return CompletionDataProviderKeyResult.NormalKey;
            //throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        private Dictionary<string, string> CheckDeclaredNamespaces(string text)
        {
            Dictionary<string, string> declaredNamespaces = new Dictionary<string, string>();

            var splited = text.Split(new char[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
            if (!splited.Any() || !splited.Any(x => !string.IsNullOrEmpty(x) && x.Contains("xmlns")))
                return null;
            string targetText = "<" + splited.FirstOrDefault(x => !string.IsNullOrEmpty(x) && x.Contains("xmlns")).Replace(">","/>");
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(targetText);
                XmlNode root = doc.FirstChild;
                foreach(XmlAttribute attribute in root.Attributes)
                {
                    if(attribute.Name.Contains("xmlns") && !declaredNamespaces.Keys.Contains(attribute.Value))
                    {
                        declaredNamespaces.Add(attribute.Value, attribute.LocalName == "xmlns" ? "" : attribute.LocalName);
                    }
                }
            }
            catch
            {

            }

            return declaredNamespaces;
        }
    }
}
