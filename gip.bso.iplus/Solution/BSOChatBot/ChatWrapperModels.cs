// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json.Serialization;
using System.IO;

namespace gip.bso.iplus
{
    #region Wrapper Classes for WPF Binding

    [ACClassInfo(Const.PackName_VarioSystem, "en{'Base class for ChatWrapper'}de{'Base class for ChatWrapper'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, false, false)]
    public abstract class ChatWrapperBase : EntityBase, IACObject
    {
        public ChatWrapperBase()
        {
        }

        public ChatWrapperBase(IACObject parentACObject)
        {
            ParentACObject = parentACObject;
        }

        [JsonIgnore]
        public IACObject ParentACObject { get; protected set; }

        [JsonIgnore]
        public IACType ACType => this.ReflectACType();

        [JsonIgnore]
        public IEnumerable<IACObject> ACContentList => this.ReflectGetACContentList();

        [JsonIgnore]
        public virtual string ACIdentifier
        {
            get;
            set;
        }

        [JsonIgnore]
        public virtual string ACCaption
        {
            get;
            set;
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            if (ParentACObject != null && ParentACObject is BSOChatBot && acUrl.Contains(nameof(BSOChatBot.SelectTemplate4ChatMessageWrapper)))
            {
                BSOChatBot bSOChat = ParentACObject as BSOChatBot;
                return bSOChat.SelectTemplate4ChatMessageWrapper(acParameter[0]);
            }
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        public string GetACUrl(IACObject rootACObject = null)
        {
            return this.ACIdentifier;
        }

        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        [JsonIgnore]
        public abstract ChatRole? ChatMessageRole { get;  }
    }

    /// <summary>
    /// Wrapper class for ChatResponseUpdate to enable WPF data binding
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Wrapper class for ChatResponseUpdate'}de{'Wrapper class for ChatResponseUpdate'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ChatResponseUpdateWrapper : ChatWrapperBase
    {
        [JsonConstructor]
        public ChatResponseUpdateWrapper()
        {
        }

        public ChatResponseUpdateWrapper(IACObject parentACObject, ChatResponseUpdate update) :base(parentACObject)
        {
            _update = update;
            _timestamp = DateTime.Now;
        }

        [JsonIgnore]
        private ChatResponseUpdate _update;
        [ACPropertyInfo(1, "", "en{'Update'}de{'Update'}")]
        public ChatResponseUpdate Update
        {
            get
            {
                return _update;
            }
            set
            {
                _update = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        [ACPropertyInfo(2, "", "en{'Role'}de{'Rolle'}")]
        public string Role => _update.Role?.ToString() ?? string.Empty;

        [JsonIgnore]
        public override ChatRole? ChatMessageRole => _update.Role;

        [JsonIgnore]
        [ACPropertyInfo(3, "", "en{'Content'}de{'Inhalt'}")]
        public ChatAIContentWrapper Content => _update.Contents?.FirstOrDefault() != null
            ? new ChatAIContentWrapper(ParentACObject, _update.Contents.First())
            : null;

        [JsonIgnore]
        [ACPropertyInfo(4, "", "en{'Contents'}de{'Inhalte'}")]
        public IEnumerable<ChatAIContentWrapper> Contents => _update.Contents?.Select(c => new ChatAIContentWrapper(ParentACObject, c)) ?? Enumerable.Empty<ChatAIContentWrapper>();

        [JsonIgnore]
        [ACPropertyInfo(5, "", "en{'Text'}de{'Text'}")]
        public string Text => _update.Text ?? string.Empty;

        [JsonIgnore]
        private DateTime _timestamp;
        [ACPropertyInfo(6, "", "en{'Timestamp'}de{'Zeitstempel'}")]
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        [ACPropertyInfo(7, "", "en{'Display Text'}de{'Anzeigetext'}")]
        public string DisplayText
        {
            get
            {
                //string shortText = Text;
                //if (!string.IsNullOrEmpty(shortText))
                //{
                //    if (shortText.Length < 600)
                //        return shortText;
                //    else
                //        return shortText.Substring(600) + "...";
                //}

                StringBuilder sb = new StringBuilder();
                if (Contents?.Any() == true)
                {
                    if (sb.Length > 0)
                        sb.Append(" ");
                    foreach (var content in Contents)
                    {
                        if (content.Content is FunctionCallContent fc)
                        {
                            sb.Append("Call:" + fc.Name);
                        }
                        else if (content.Content is FunctionResultContent fr)
                        {
                            sb.Append("Result:" + fr.CallId);
                        }
                        else if (content.Content is TextReasoningContent tr)
                        {
                            sb.Append(tr.Text);
                        }
                        else if (content.Content is TextContent tx)
                        {
                            sb.Append(tx.Text);
                        }
                        else if (content.Content is DataContent dt)
                        {
                            sb.Append(dt.Uri);
                        }
                        else if (content.Content is ErrorContent ec)
                        {
                            sb.Append(ec.Message);
                        }
                        else if (content.Content is UriContent uc)
                        {
                            sb.Append(uc.Uri.ToString());
                        }
                    }
                    if (sb.Length > 600)
                        return sb.ToString(0, 600) + "...";
                    else
                        return sb.ToString();
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Returns usage information if this update contains UsageContent
        /// </summary>
        [JsonIgnore]
        [ACPropertyInfo(8, "", "en{'Usage Info'}de{'Nutzungsinfo'}")]
        public ChatUsageInfoWrapper UsageInfo
        {
            get
            {
                var usageContent = Contents?.FirstOrDefault(c => c.Kind == nameof(UsageContent));
                return usageContent?.UsageInfo;
            }
        }

        [JsonIgnore]
        [ACPropertyInfo(9, "", "en{'Has Usage Info'}de{'Hat Nutzungsinfo'}")]
        public bool HasUsageInfo => UsageInfo != null;

        [JsonIgnore]
        [ACPropertyInfo(10, "", "en{'Tooltip text'}de{'Tooltip text'}")]
        public string FullContentAsText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (Contents?.Any() == true)
                {
                    //if (sb.Length > 0)
                    //    sb.Append(" ");
                    foreach (var content in Contents)
                    {
                        if (content.Content is FunctionCallContent fc)
                        {
                            sb.Append(fc.Name);
                            if (fc.Arguments != null)
                            {
                                sb.Append(" ");
                                sb.AppendLine(string.Join(", ", fc.Arguments));
                            }
                            else
                                sb.AppendLine(";");
                        }
                        else if (content.Content is FunctionResultContent fr)
                        {
                            sb.Append(fr.CallId);
                            if (fr.Result != null)
                                sb.AppendLine(fr.Result.ToString());
                            else
                                sb.AppendLine(";");
                        }
                        else if (content.Content is TextReasoningContent tr)
                        {
                            if (sb.Length > 0)
                                sb.Append(" ");
                            sb.Append(tr.Text);
                        }
                        else if (content.Content is TextContent tx)
                        {
                            sb.AppendLine(tx.Text);
                        }
                        else if (content.Content is DataContent dt)
                        {
                            sb.AppendLine(dt.Uri);
                        }
                        else if (content.Content is ErrorContent ec)
                        {
                            sb.AppendLine(ec.Message);
                        }
                        else if (content.Content is UriContent uc)
                        {
                            sb.AppendLine(uc.Uri.ToString());
                        }
                    }
                    return sb.ToString();
                }

                return string.Empty;
            }
        }

        public void AttachBSO(BSOChatBot chatBot)
        {
            ParentACObject = chatBot;
            if (_update != null && _update.Contents != null)
            {
                foreach (var content in Contents)
                {
                    content.AttachBSO(chatBot);
                }
            }
        }

        public void PopulateConversationList(List<ChatMessage> conversationList)
        {
            if (conversationList == null || _update == null || _update.Contents == null)
                return;
            
            // Create a new ChatMessage from this update
            var chatMessage = new ChatMessage(_update.Role.Value, _update.Contents)
            {
                MessageId = _update.MessageId,
                AdditionalProperties = _update.AdditionalProperties,
                AuthorName = _update.AuthorName
            };
            // Add the chat message to the conversation list
            conversationList.Add(chatMessage);
        }
    }

    /// <summary>
    /// Wrapper class for AIContent to enable WPF data binding
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Wrapper class for AIContent'}de{'Wrapper class for AIContent'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ChatAIContentWrapper : ChatWrapperBase
    {
        public ChatAIContentWrapper(IACObject parentACObject, AIContent content) : base(parentACObject)
        {
            _content = content;
        }

        [JsonIgnore]
        private AIContent _content;
        [JsonIgnore]
        [ACPropertyInfo(1, "", "en{'Content'}de{'Inhalt'}")]
        public AIContent Content => _content;

        [JsonIgnore]
        [ACPropertyInfo(2, "", "en{'Kind'}de{'Art'}")]
        public string Kind => _content.GetType().Name;

        [JsonIgnore]
        [ACPropertyInfo(3, "", "en{'Text'}de{'Text'}")]
        public string Text
        {
            get
            {
                if (_content is TextContent textContent)
                    return textContent.Text;
                return _content?.ToString() ?? string.Empty;
            }
        }

        [JsonIgnore]
        public override ChatRole? ChatMessageRole => null;

        /// <summary>
        /// Returns usage information if this content is UsageContent
        /// </summary>
        [JsonIgnore]
        [ACPropertyInfo(4, "", "en{'Usage Info'}de{'Nutzungsinfo'}")]
        public ChatUsageInfoWrapper UsageInfo
        {
            get
            {
                if (_content is UsageContent usageContent)
                    return new ChatUsageInfoWrapper(ParentACObject, usageContent);
                return null;
            }
        }

        [JsonIgnore]
        [ACPropertyInfo(5, "", "en{'Is Text Content'}de{'Ist Textinhalt'}")]
        public bool IsTextContent => _content is TextContent;

        [JsonIgnore]
        [ACPropertyInfo(6, "", "en{'Is Usage Content'}de{'Ist Nutzungsinhalt'}")]
        public bool IsUsageContent => _content is UsageContent;

        [JsonIgnore]
        [ACPropertyInfo(7, "", "en{'Raw Content'}de{'Rohinhalt'}")]
        public object RawContent => _content;

        public void AttachBSO(BSOChatBot chatBot)
        {
            ParentACObject = chatBot;
        }
    }

    /// <summary>
    /// Wrapper class for UsageContent to expose token counts for WPF binding
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Wrapper class for UsageContent'}de{'Wrapper class for UsageContent'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ChatUsageInfoWrapper : ChatWrapperBase
    {
        public ChatUsageInfoWrapper(IACObject parentACObject, UsageContent usageContent) : base(parentACObject)
        {
            _usageContent = usageContent;
        }

        [JsonIgnore]
        private UsageContent _usageContent;
        [JsonIgnore]
        [ACPropertyInfo(1, "", "en{'Usage Content'}de{'Nutzungsinhalt'}")]
        public UsageContent UsageContent => _usageContent;

        [JsonIgnore]
        [ACPropertyInfo(2, "", "en{'Input Token Count'}de{'Eingabe-Token-Anzahl'}")]
        public long? InputTokenCount => _usageContent.Details?.InputTokenCount;

        [JsonIgnore]
        [ACPropertyInfo(3, "", "en{'Output Token Count'}de{'Ausgabe-Token-Anzahl'}")]
        public long? OutputTokenCount => _usageContent.Details?.OutputTokenCount;

        [JsonIgnore]
        [ACPropertyInfo(4, "", "en{'Total Token Count'}de{'Gesamt-Token-Anzahl'}")]
        public long? TotalTokenCount => _usageContent.Details?.TotalTokenCount;

        [JsonIgnore]
        [ACPropertyInfo(5, "", "en{'Display Info'}de{'Anzeigeinfo'}")]
        public string DisplayInfo
        {
            get
            {
                var parts = new List<string>();
                if (InputTokenCount.HasValue)
                    parts.Add($"Input: {InputTokenCount}");
                if (OutputTokenCount.HasValue)
                    parts.Add($"Output: {OutputTokenCount}");
                if (TotalTokenCount.HasValue)
                    parts.Add($"Total: {TotalTokenCount}");

                return parts.Count > 0 ? string.Join(", ", parts) : "No usage data";
            }
        }

        [JsonIgnore]
        public override ChatRole? ChatMessageRole => null;
    }

    /// <summary>
    /// Wrapper class for chat messages (both user and AI)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Wrapper class for chat messages'}de{'Wrapper class for chat messages'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ChatMessageWrapper : ChatWrapperBase
    {
        [JsonConstructor]
        public ChatMessageWrapper()
        {
        }

        public ChatMessageWrapper(IACObject parentACObject, ChatRole chatRole, string content) : base(parentACObject)
        {
            _WrappedChatMessage = new ChatMessage(chatRole, content);
            _timestamp = DateTime.Now;
            _updates = new ObservableCollection<ChatResponseUpdateWrapper>();
        }

        public ChatMessageWrapper(IACObject parentACObject, IList<AIContent> content, ChatRole chatRole) : base(parentACObject)
        {
            _WrappedChatMessage = new ChatMessage(chatRole, content);
            _timestamp = DateTime.Now;
            _updates = new ObservableCollection<ChatResponseUpdateWrapper>();
        }

        [JsonIgnore]
        private ChatMessage _WrappedChatMessage;
        [JsonIgnore]
        public ChatMessage WrappedChatMessage
        {
            get => _WrappedChatMessage;
            set
            {
                _WrappedChatMessage = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        [ACPropertyInfo(1, "", "en{'Role'}de{'Rolle'}")]
        public string Role
        {
            get => _WrappedChatMessage.Role.ToString();
        }

        [JsonIgnore]
        public override ChatRole? ChatMessageRole => _WrappedChatMessage.Role;

        [JsonIgnore]
        [ACPropertyInfo(2, "", "en{'Content'}de{'Inhalt'}")]
        public string Content
        {
            get => _WrappedChatMessage.Text;
        }

        [JsonIgnore]
        private DateTime _timestamp;
        [ACPropertyInfo(3, "", "en{'Timestamp'}de{'Zeitstempel'}")]
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private ObservableCollection<ChatResponseUpdateWrapper> _updates;
        [ACPropertyInfo(4, "", "en{'Updates'}de{'Updates'}")]
        public ObservableCollection<ChatResponseUpdateWrapper> Updates
        {
            get => _updates;
            set
            {
                _updates = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayContent));
                OnPropertyChanged(nameof(TokenUsageSummary));
            }
        }

        [JsonIgnore]
        [ACPropertyInfo(5, "", "en{'Short Content'}de{'Verkürzter Inhalt'}")]
        public string DisplayContent
        {
            get
            {
                if (ChatMessageRole.HasValue && ChatMessageRole.Value == ChatRole.Assistant && Updates?.Any() == true)
                {
                    var allText = string.Join("", Updates.Where(c => c.Content != null && c.Content.Kind == nameof(TextContent)).Select(c => c.DisplayText));
                    if (!string.IsNullOrEmpty(allText))
                    {
                        // Check line count first
                        var lines = allText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        if (lines.Length > 8)
                            allText = string.Join(System.Environment.NewLine, lines.Take(8)) + "...";
                        // Check length and truncate if necessary
                        if (allText.Length > 600)
                            allText = allText.Substring(0, 600) + "...";

                        return allText;
                    }
                    else
                        return Content;
                }
                return Content;
            }
        }

        [JsonIgnore]
        [ACPropertyInfo(5, "", "en{'Full Content as Text'}de{'Vollständiger Inhalt als Text'}")]
        public string FullContentAsText
        {
            get
            {
                if (ChatMessageRole.HasValue && ChatMessageRole.Value == ChatRole.Assistant && Updates?.Any() == true)
                {
                    return string.Join("", Updates.Select(c => c.FullContentAsText));
                }
                return Content;
            }
        }

        [JsonIgnore]
        [ACPropertyInfo(5, "", "en{'Only Content which is Text'}de{'Nur Inhalte die Text sind'}")]
        public string OnlyTextContent
        {
            get
            {
                if (Updates?.Any() == true)
                {
                    return string.Join("", Updates.Select(u => u.Text ?? ""));
                }
                return Content;
            }
        }

        /// <summary>
        /// Calculate and display sum of token counts from all updates
        /// </summary>
        [JsonIgnore]
        [ACPropertyInfo(6, "", "en{'Token Usage Summary'}de{'Token-Nutzungsübersicht'}")]
        public string TokenUsageSummary
        {
            get
            {
                if (!IsAI || Updates?.Any() != true)
                    return string.Empty;

                var usageUpdates = Updates.Where(u => u.HasUsageInfo).Select(u => u.UsageInfo).ToList();
                if (!usageUpdates.Any())
                    return string.Empty;

                long totalInput = usageUpdates.Where(u => u.InputTokenCount.HasValue).Sum(u => u.InputTokenCount.Value);
                long totalOutput = usageUpdates.Where(u => u.OutputTokenCount.HasValue).Sum(u => u.OutputTokenCount.Value);
                long totalTokens = usageUpdates.Where(u => u.TotalTokenCount.HasValue).Sum(u => u.TotalTokenCount.Value);

                var parts = new List<string>();
                if (totalInput > 0)
                    parts.Add($"Input: {totalInput}");
                if (totalOutput > 0)
                    parts.Add($"Output: {totalOutput}");
                if (totalTokens > 0)
                    parts.Add($"Total: {totalTokens}");

                return parts.Count > 0 ? $"Tokens - {string.Join(", ", parts)}" : string.Empty;
            }
        }

        [JsonIgnore]
        [ACPropertyInfo(7, "", "en{'Is User'}de{'Ist Benutzer'}")]
        public bool IsUser => ChatMessageRole.HasValue ? ChatMessageRole.Value == ChatRole.User : false;

        [JsonIgnore]
        [ACPropertyInfo(8, "", "en{'Is AI'}de{'Ist KI'}")]
        public bool IsAI => ChatMessageRole.HasValue ? ChatMessageRole.Value == ChatRole.Assistant || ChatMessageRole.Value == ChatRole.Tool : false;

        [JsonIgnore]
        [ACPropertyInfo(9, "", "en{'Has Token Usage'}de{'Hat Token-Nutzung'}")]
        public bool HasTokenUsage => !string.IsNullOrEmpty(TokenUsageSummary);

        public void Refresh()
        {
            OnPropertyChanged(nameof(DisplayContent));
            OnPropertyChanged(nameof(TokenUsageSummary));
        }

        public void AttachBSO(BSOChatBot chatBot)
        {
            ParentACObject = chatBot;
            if (Updates != null)
            {
                foreach (var update in Updates)
                {
                    update.AttachBSO(chatBot);
                }
            }
        }

        public void PopulateConversationList(List<ChatMessage> conversationList)
        {
            if (conversationList == null)
                return;
            // Add the wrapped chat message to the conversation list
            conversationList.Add(_WrappedChatMessage);
            // If this is an AI message, add all updates as well
            if (Updates != null)
            {
                foreach (var update in Updates)
                {
                    update.PopulateConversationList(conversationList);
                }
            }
        }
    }


    public static class ChatWrapperExtensions
    {
        public static void AttachBSO(this ObservableCollection<ChatMessageWrapper> updates, BSOChatBot chatBot)
        {
            foreach (var item in updates)
            {
                item.AttachBSO(chatBot);
            }
        }

        public static void AddUserMessage(this ObservableCollection<ChatMessageWrapper> messages, BSOChatBot chatBot, string content)
        {
            var message = new ChatMessageWrapper(chatBot, ChatRole.User, content);
            messages.Add(message);
        }

        public static void AddUserMessage(this ObservableCollection<ChatMessageWrapper> messages, BSOChatBot chatBot, string messageText, IList<string> images)
        {
            List<AIContent> listAIContent = new List<AIContent>();
            listAIContent.Add(new TextContent(messageText));

            foreach (string imagePath in images)
            {
                // Convert local file path to file URI
                Uri imageUri = new Uri(imagePath, UriKind.Absolute);
                if (imageUri.IsFile || !imageUri.IsAbsoluteUri)
                {
                    string localPath = imageUri.IsFile ? imageUri.LocalPath : imageUri.OriginalString;
                    var imageBytes = File.ReadAllBytes(localPath);
                    string mimeType = GetMimeTypeFromPath(localPath);
                    listAIContent.Add(new DataContent(imageBytes, mimeType));
                }
                else
                {
                    string mimeType = GetMimeTypeFromUri(imageUri);
                    listAIContent.Add(new UriContent(imageUri, mimeType));
                }
            }
            var message = new ChatMessageWrapper(chatBot, listAIContent, ChatRole.User);
            messages.Add(message);
        }

        public static ChatMessageWrapper CreateAndAddNewAssistentMessage(this ObservableCollection<ChatMessageWrapper> messages, BSOChatBot chatBot)
        {
            var message = new ChatMessageWrapper(chatBot, ChatRole.Assistant, null);
            messages.Add(message);
            return message;
        }

        public static IEnumerable<ChatMessage> GetConversations(this IEnumerable<ChatMessageWrapper> messageWrappers)
        {
            List<ChatMessage> conversationList = new List<ChatMessage>();
            foreach (var item in messageWrappers)
            {
                item.PopulateConversationList(conversationList);
            }

            return conversationList;
        }

        public static string GetMimeTypeFromUri(Uri uri)
        {
            string path = uri.IsFile ? uri.LocalPath : uri.AbsolutePath;
            return GetMimeTypeFromPath(path);
        }

        public static string GetMimeTypeFromPath(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            // If no extension found, try to infer from URL or use default
            if (string.IsNullOrEmpty(extension))
            {
                return "image/jpeg"; // Default fallback
            }

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                _ => "image/jpeg"
            };
        }
    }

    #endregion
}