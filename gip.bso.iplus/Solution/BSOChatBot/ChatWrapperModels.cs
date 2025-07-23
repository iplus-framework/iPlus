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

namespace gip.bso.iplus
{
    #region Wrapper Classes for WPF Binding

    /// <summary>
    /// Wrapper class for ChatResponseUpdate to enable WPF data binding
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Wrapper class for ChatResponseUpdate'}de{'Wrapper class for ChatResponseUpdate'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ChatResponseUpdateWrapper : EntityBase
    {
        private ChatResponseUpdate _update;

        public ChatResponseUpdateWrapper(ChatResponseUpdate update)
        {
            _update = update;
        }

        [ACPropertyInfo(1, "", "en{'Update'}de{'Update'}")]
        public ChatResponseUpdate Update => _update;

        [ACPropertyInfo(2, "", "en{'Role'}de{'Rolle'}")]
        public string Role => _update.Role?.ToString() ?? string.Empty;

        [ACPropertyInfo(3, "", "en{'Content'}de{'Inhalt'}")]
        public ChatAIContentWrapper Content => _update.Contents?.FirstOrDefault() != null
            ? new ChatAIContentWrapper(_update.Contents.First())
            : null;

        [ACPropertyInfo(4, "", "en{'Contents'}de{'Inhalte'}")]
        public IEnumerable<ChatAIContentWrapper> Contents => _update.Contents?.Select(c => new ChatAIContentWrapper(c)) ?? Enumerable.Empty<ChatAIContentWrapper>();

        [ACPropertyInfo(5, "", "en{'Text'}de{'Text'}")]
        public string Text => _update.Text ?? string.Empty;

        [ACPropertyInfo(6, "", "en{'Timestamp'}de{'Zeitstempel'}")]
        public DateTime Timestamp { get; } = DateTime.Now;

        [ACPropertyInfo(7, "", "en{'Display Text'}de{'Anzeigetext'}")]
        public string DisplayText
        {
            get
            {
                if (!string.IsNullOrEmpty(Text))
                    return Text;

                if (Contents?.Any() == true)
                {
                    var textContents = Contents.Where(c => c.Kind == "TextContent").Select(c => c.Text);
                    return string.Join(" ", textContents);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Returns usage information if this update contains UsageContent
        /// </summary>
        [ACPropertyInfo(8, "", "en{'Usage Info'}de{'Nutzungsinfo'}")]
        public ChatUsageInfoWrapper UsageInfo
        {
            get
            {
                var usageContent = Contents?.FirstOrDefault(c => c.Kind == "UsageContent");
                return usageContent?.UsageInfo;
            }
        }

        [ACPropertyInfo(9, "", "en{'Has Usage Info'}de{'Hat Nutzungsinfo'}")]
        public bool HasUsageInfo => UsageInfo != null;
    }

    /// <summary>
    /// Wrapper class for AIContent to enable WPF data binding
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Wrapper class for AIContent'}de{'Wrapper class for AIContent'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ChatAIContentWrapper : EntityBase
    {
        private AIContent _content;

        public ChatAIContentWrapper(AIContent content)
        {
            _content = content;
        }

        [ACPropertyInfo(1, "", "en{'Content'}de{'Inhalt'}")]
        public AIContent Content => _content;

        [ACPropertyInfo(2, "", "en{'Kind'}de{'Art'}")]
        public string Kind => _content.GetType().Name;

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

        /// <summary>
        /// Returns usage information if this content is UsageContent
        /// </summary>
        [ACPropertyInfo(4, "", "en{'Usage Info'}de{'Nutzungsinfo'}")]
        public ChatUsageInfoWrapper UsageInfo
        {
            get
            {
                if (_content is UsageContent usageContent)
                    return new ChatUsageInfoWrapper(usageContent);
                return null;
            }
        }

        [ACPropertyInfo(5, "", "en{'Is Text Content'}de{'Ist Textinhalt'}")]
        public bool IsTextContent => _content is TextContent;
        
        [ACPropertyInfo(6, "", "en{'Is Usage Content'}de{'Ist Nutzungsinhalt'}")]
        public bool IsUsageContent => _content is UsageContent;

        [ACPropertyInfo(7, "", "en{'Raw Content'}de{'Rohinhalt'}")]
        public object RawContent => _content;
    }

    /// <summary>
    /// Wrapper class for UsageContent to expose token counts for WPF binding
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Wrapper class for UsageContent'}de{'Wrapper class for UsageContent'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ChatUsageInfoWrapper : EntityBase
    {
        private UsageContent _usageContent;

        public ChatUsageInfoWrapper(UsageContent usageContent)
        {
            _usageContent = usageContent;
        }

        [ACPropertyInfo(1, "", "en{'Usage Content'}de{'Nutzungsinhalt'}")]
        public UsageContent UsageContent => _usageContent;

        [ACPropertyInfo(2, "", "en{'Input Token Count'}de{'Eingabe-Token-Anzahl'}")]
        public long? InputTokenCount => _usageContent.Details?.InputTokenCount;
        
        [ACPropertyInfo(3, "", "en{'Output Token Count'}de{'Ausgabe-Token-Anzahl'}")]
        public long? OutputTokenCount => _usageContent.Details?.OutputTokenCount;
        
        [ACPropertyInfo(4, "", "en{'Total Token Count'}de{'Gesamt-Token-Anzahl'}")]
        public long? TotalTokenCount => _usageContent.Details?.TotalTokenCount;

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
    }

    /// <summary>
    /// Wrapper class for chat messages (both user and AI)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Wrapper class for chat messages'}de{'Wrapper class for chat messages'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ChatMessageWrapper : EntityBase
    {
        private string _role;
        private string _content;
        private DateTime _timestamp;
        private ObservableCollection<ChatResponseUpdateWrapper> _updates;

        public ChatMessageWrapper(string role, string content)
        {
            _role = role;
            _content = content;
            _timestamp = DateTime.Now;
            _updates = new ObservableCollection<ChatResponseUpdateWrapper>();
        }

        [ACPropertyInfo(1, "", "en{'Role'}de{'Rolle'}")]
        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged();
            }
        }

        [ACPropertyInfo(2, "", "en{'Content'}de{'Inhalt'}")]
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayContent));
            }
        }

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

        [ACPropertyInfo(5, "", "en{'Display Content'}de{'Anzeigeinhalt'}")]
        public string DisplayContent
        {
            get
            {
                if (Role == "AI" && Updates?.Any() == true)
                {
                    var allText = string.Join("", Updates.Select(u => u.DisplayText));
                    return !string.IsNullOrEmpty(allText) ? allText : Content;
                }
                return Content;
            }
        }

        /// <summary>
        /// Calculate and display sum of token counts from all updates
        /// </summary>
        [ACPropertyInfo(6, "", "en{'Token Usage Summary'}de{'Token-Nutzungsübersicht'}")]
        public string TokenUsageSummary
        {
            get
            {
                if (Role != "AI" || Updates?.Any() != true)
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

        [ACPropertyInfo(7, "", "en{'Is User'}de{'Ist Benutzer'}")]
        public bool IsUser => Role == "User";
        
        [ACPropertyInfo(8, "", "en{'Is AI'}de{'Ist KI'}")]
        public bool IsAI => Role == "AI";
        
        [ACPropertyInfo(9, "", "en{'Has Token Usage'}de{'Hat Token-Nutzung'}")]
        public bool HasTokenUsage => !string.IsNullOrEmpty(TokenUsageSummary);

        public void Refresh()
        {
            OnPropertyChanged(DisplayContent);
            OnPropertyChanged(TokenUsageSummary);
        }
    }

    #endregion
}