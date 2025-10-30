using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using System;

namespace gip.core.layoutengine.avui.CodeCompletion
{
    /// <summary>
    /// Holds the text for  namespace, child element or attribute 
    /// autocomplete (intellisense).
    /// </summary>
    public class XmlCompletionData : ICompletionData
    {
        string text;
        DataType dataType = DataType.XmlElement;
        string description = String.Empty;

        /// <summary>
        /// The type of text held in this object.
        /// </summary>
        public enum DataType
        {
            XmlElement = 1,
            XmlAttribute = 2,
            NamespaceUri = 3,
            XmlAttributeValue = 4,
            Snippet = 5,
            Comment = 6,
            Other = 7,
        }

        /// <summary>
        /// Creates a new instance of XmlCompletionData.
        /// </summary>
        /// <param name="text">The text parameter.</param>
        public XmlCompletionData(string text)
            : this(text, String.Empty, DataType.XmlElement)
        {
        }

        /// <summary>
        /// Creates a new instance of XmlCompletionData.
        /// </summary>
        /// <param name="text">The text parameter.</param>
        /// <param name="description">The description parameter.</param>
        public XmlCompletionData(string text, string description)
            : this(text, description, DataType.XmlElement)
        {
        }

        /// <summary>
        /// Creates a new instance of XmlCompletionData.
        /// </summary>
        /// <param name="text">The text parameter.</param>
        /// <param name="dataType">The data type parameter.</param>
        public XmlCompletionData(string text, DataType dataType)
            : this(text, String.Empty, dataType)
        {
        }

        /// <summary>
        /// Creates a new instance of XmlCompletionData.
        /// </summary>
        /// <param name="text">The text parameter.</param>
        /// <param name="description">The description parameter.</param>
        /// <param name="dataType">The data type parameter.</param>
        public XmlCompletionData(string text, string description, DataType dataType)
        {
            this.Text = text;
            this.description = description;
            this.dataType = dataType;
        }

        /// <summary>
        /// Gets the image index.
        /// </summary>
        public int ImageIndex
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the Text.
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                Content = value;
                text = value;
            }
        }

        /// <summary>
        /// Returns the xml item's documentation as retrieved from
        /// the xs:annotation/xs:documentation element.
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
        }

        /// <summary>
        /// Gets the Priority.
        /// </summary>
        public double Priority
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the completion data type.
        /// </summary>
        public DataType CompletionDataType
        {
            get
            {
                return dataType;
            }
        }

        /// <summary>
        /// Gets or sets the Image.
        /// </summary>
        public IImage Image
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public object Content
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        object ICompletionData.Description
        {
            get
            {
                return description;
            }
        }

        /// <summary>
        /// Gets or sets the VBXMLEditor.
        /// </summary>
        public VBXMLEditor vBXMLEditor
        {
            get;
            set;
        }

        IImage ICompletionData.Image => throw new NotImplementedException();

        /// <summary>
        /// The insert action.
        /// </summary>
        /// <param name="textArea">The text area.</param>
        /// <param name="ch">The char parameter.</param>
        /// <returns></returns>
        public bool InsertAction(TextArea textArea, char ch)
        {
            if ((dataType == DataType.XmlElement) || (dataType == DataType.XmlAttributeValue))
            {
                //textArea.InsertString(text);
            }
            else if (dataType == DataType.NamespaceUri)
            {
                //textArea.InsertString(String.Concat("\"", text, "\""));
            }
            else
            {
                // Insert an attribute.
                Caret caret = textArea.Caret;
                //textArea.InsertString(String.Concat(text, "=\"\""));

                // Move caret into the middle of the attribute quotes.
                //caret.Position = textArea.Document.OffsetToPosition(caret.Offset - 1);
            }
            return false;
        }

        /// <summary>
        /// Compares this instance with a specified System.String object and indicates whether
        ///     this instance precedes, follows, or appears in the same position in the sort
        ///     order as the specified string.
        /// </summary>
        /// <param name="obj">The object parameter.</param>
        /// <returns>A 32-bit signed integer that indicates whether this instance precedes, follows,
        ///     or appears in the same position in the sort order as the strB parameter.Value
        ///    Condition Less than zero This instance precedes strB. Zero This instance has
        ///    the same position in the sort order as strB. Greater than zero This instance
        ///    follows strB.-or- strB is null.
        ///</returns>
        public int CompareTo(object obj)
        {
            if ((obj == null) || !(obj is XmlCompletionData))
            {
                return -1;
            }
            return text.CompareTo(((XmlCompletionData)obj).text);
        }

        /// <summary>
        /// Inserts a completion part in the content.
        /// </summary>
        /// <param name="textArea">The text area parameter.</param>
        /// <param name="completionSegment">The completion segment.</param>
        /// <param name="insertionRequestEventArgs">The insertion request event arguments.</param>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            if(Text.Contains(":") && !this.Text.EndsWith(":"))
            {
                string prefix = Text.Split(new char[] { ':' })[0];
                int prefixLen = prefix.Length+1;

                string check = textArea.Document.GetText(completionSegment.Offset - prefixLen, prefixLen-1);
                if(check == prefix)
                    completionSegment = new AnchorSegment(textArea.Document, completionSegment.Offset - prefixLen, completionSegment.Length + prefixLen);
            }

            textArea.Document.Replace(completionSegment, this.Text);
            if (this.Text.EndsWith(":") && vBXMLEditor != null)
                vBXMLEditor.ShowCompletionWindow(this.Text);
                
        }
    }
}
