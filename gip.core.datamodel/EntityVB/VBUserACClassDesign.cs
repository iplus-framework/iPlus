// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace gip.core.datamodel
{
    /// <summary>Stores XAML for a user. It's used whne the user uses the snapshot-icon to store his last opened business objects. At the next logon his last snapshot will be restored.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'personalized layout'}de{'Personalisiertes Layout'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "VBUser", "en{'User'}de{'Benutzer'}", Const.ContextDatabase + "\\" + VBUser.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(2, "ACClassDesign", "en{'Design'}de{'Design'}", Const.ContextDatabase + "\\ACClassDesignList", "", true)]
    [ACPropertyEntity(9999, "XMLDesign", "en{'XMLDesign'}de{'de-XMLDesign'}")]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBUserACClassDesign.ClassName, "en{'Personalized layout'}de{'Personalisiertes Layout'}", typeof(VBUserACClassDesign), VBUserACClassDesign.ClassName, VBUser.ClassName + "\\VBUserName," + ACClassDesign.ClassName + "\\ACIdentifier", ACClassDesign.ClassName + "\\ACIdentifier")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBUserACClassDesign>) })]
    [NotMapped]
    public partial class VBUserACClassDesign : IACObjectEntity
    {
        public const string ClassName = "VBUserACClassDesign";

        #region New/Delete
        public static VBUserACClassDesign NewACObject(Database database, IACObject parentACObject)
        {
            VBUserACClassDesign entity = new VBUserACClassDesign();
            entity.VBUserACClassDesignID = Guid.NewGuid();
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            if (parentACObject is VBUser)
            {
                VBUser user = parentACObject as VBUser;
                entity.VBUserID = user.VBUserID;
                entity.VBUser = user;
            }
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
       }


        public static VBUserACClassDesign NewVBUserACClassDesign(Database database, VBUser vbUser, ACClassDesign acClassDesign)
        {
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            VBUserACClassDesign entity = VBUserACClassDesign.NewACObject(database, vbUser);
            if (acClassDesign != null)
            {
                entity.ACClassDesignID = acClassDesign.ACClassDesignID;
                entity.ACClassDesign = acClassDesign;
            }
            database.VBUserACClassDesign.Add(entity);
            return entity;
        }
        #endregion

        #region IACUrl Member

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return VBUser.ACCaption + "->" + ACClassDesign.ACCaption;
            }
        }

        /// <summary>
        /// Returns VBUser
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to VBUser</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                return VBUser;
            }
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            base.EntityCheckAdded(user, context);
            if (VBUser == null)
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "Key",
                    Message = "Key",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "Key"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            return null;
        }


        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "ACClassDesign\\ACIdentifier";
            }
        }

        #endregion

        #region Misc
        [NotMapped]
        public string XAMLDesign
        {
            get
            {
                if (Database.Root.IsAvaloniaUI)
                {
                    string avaloniaXAML = XMLDesign;
                    foreach (var tuple in ACxmlnsResolver.C_AvaloniaNamespaceMapping)
                    {
                        avaloniaXAML = avaloniaXAML?.Replace(tuple.WpfNamespace, tuple.AvaloniaNamespace);
                    }
                    
                    // Apply regex and string replacements
                    foreach (var tuple in ACClassDesign.C_AvaloniaFindAndReplace)
                    {
                        if (tuple.IsRegex)
                        {
                            avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", tuple.WpfPattern, tuple.AvaloniaReplacement, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        }
                        else
                        {
                            avaloniaXAML = avaloniaXAML?.Replace(tuple.WpfPattern, tuple.AvaloniaReplacement);
                        }
                    }
                    
                       // Apply regex and string replacements
                        foreach (var tuple in ACClassDesign.C_AvaloniaFindAndReplace)
                        {
                            if (tuple.IsRegex)
                            {
                                avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", tuple.WpfPattern, tuple.AvaloniaReplacement, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                            }
                            else
                            {
                                avaloniaXAML = avaloniaXAML?.Replace(tuple.WpfPattern, tuple.AvaloniaReplacement);
                            }
                        }
                        
                        // Handle xmlns removal: split at first root element close, process child content only
                        // This preserves xmlns on root element but removes from child elements
                        if (!string.IsNullOrEmpty(avaloniaXAML))
                        {
                            // Find the end of the first root element tag (after all xmlns declarations)
                            // Pattern matches up to first real element tag (not XML declarations like <?xml...?>)
                            var rootEndMatch = Regex.Match(avaloniaXAML, @"^([\s\S]*?<[a-zA-Z][\w:]*[^>]*?>)([\s\S]*)$");
                            if (rootEndMatch.Success)
                            {
                                string rootPart = rootEndMatch.Groups[1].Value;
                                string childContent = rootEndMatch.Groups[2].Value;
                                
                                // Remove all xmlns declarations from child content only
                                childContent = Regex.Replace(childContent, @"\s+xmlns:?\w*=""[^""]*""", "", RegexOptions.IgnoreCase);
                                
                                // Remove attributes' namespace prefix if it matches the root element prefix (e.g. vb: in <vb:VBButton vb:VBContent="...">)
                                // We skip "x:" and "xmlns:" to ensure standard XAML features are preserved
                                var nameMatch = Regex.Match(rootPart, @"<([a-zA-Z][\w.]*):[a-zA-Z][\w.]*");
                                if (nameMatch.Success)
                                {
                                    string rootPrefix = nameMatch.Groups[1].Value;
                                    if (!string.Equals(rootPrefix, "x", StringComparison.OrdinalIgnoreCase) && 
                                        !string.Equals(rootPrefix, "xmlns", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Pattern matches the prefix only if the attribute name does NOT contain a dot
                                        // This ensures simple attributes like vb:VBContent become VBContent,
                                        // but attached properties like vb:VBDockingManager.IsCloseableBSORoot are preserved.
                                        string prefixAttrPattern = @"(\s+)" + Regex.Escape(rootPrefix) + @":([\w]+)(=)";
                                        rootPart = Regex.Replace(rootPart, prefixAttrPattern, "$1$2$3", RegexOptions.IgnoreCase);
                                        childContent = Regex.Replace(childContent, prefixAttrPattern, "$1$2$3", RegexOptions.IgnoreCase);
                                    }
                                }

                                avaloniaXAML = rootPart + childContent;
                            }
                        }

                        // Convert decimal CenterX/Y to percentage (e.g., 0.669 -> 67)
                        // This handles the conversion from WPF's 0.0-1.0 coordinate system to Avalonia's percentage-based values where required
                        avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", @"\b(Center[XY])=""0\.(\d+)""", m =>
                        {
                            string attr = m.Groups[1].Value;
                            string decimals = m.Groups[2].Value;
                            if (double.TryParse("0." + decimals, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                            {
                                int percent = (int)Math.Round(val * 100);
                                return $"{attr}=\"{percent}\"";
                            }
                            return m.Value;
                        }, RegexOptions.IgnoreCase);

                        // Convert decimal StartPoint/EndPoint to percentage (e.g., 0.46,1.0 -> 46%,100%)
                        // This handles the conversion of relative points in brushes to percentage-based strings required for Avalonia
                        avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", @"\b(StartPoint|EndPoint)=""([^""]+)""", m =>
                        {
                            string attr = m.Groups[1].Value;
                            string points = m.Groups[2].Value;
                            var coords = points.Split(',');
                            if (coords.Length == 2)
                            {
                                if (double.TryParse(coords[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double x) &&
                                    double.TryParse(coords[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double y))
                                {
                                    int px = (int)Math.Round(x * 100);
                                    int py = (int)Math.Round(y * 100);
                                    return $"{attr}=\"{px}%,{py}%\"";
                                }
                            }
                            return m.Value;
                        }, RegexOptions.IgnoreCase);

                    
                    return avaloniaXAML;
                }
                else
                {
                    string avaloniaXAML = XMLDesign;
                    foreach (var tuple in ACxmlnsResolver.C_AvaloniaNamespaceMapping)
                    {
                        if (tuple.WpfNamespace.StartsWith("clr-namespace"))
                            continue;
                        avaloniaXAML = avaloniaXAML?.Replace(tuple.AvaloniaNamespace, tuple.WpfNamespace);
                    }
                    // Note: Reverse conversion (Avalonia to WPF) does not support regex patterns
                    foreach (var tuple in ACClassDesign.C_AvaloniaFindAndReplace)
                    {
                        if (!tuple.IsRegex)
                        {
                            avaloniaXAML = avaloniaXAML?.Replace(tuple.AvaloniaReplacement, tuple.WpfPattern);
                        }
                    }
                    return avaloniaXAML;
                }
            }
            set
            {
                XMLDesign = value;
            }
        }   
        #endregion
    }
}
