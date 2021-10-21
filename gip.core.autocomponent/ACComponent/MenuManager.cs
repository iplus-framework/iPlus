using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Windows;
using System.Collections;
using System.Configuration;

namespace gip.core.autocomponent
{
    static class MenuManager
    {
        static public IEnumerable<ACMenuItem> GetMenu(IACComponent acComponent, IACInteractiveObject acElement)
        {
            List<ACMenuItem> acMenuItemList = new List<ACMenuItem>();

            // 1. Menü vom Inhalt
            List<ACMenuItem> menuFromContent = GetMenuFromContent(acComponent, acElement);
            acMenuItemList.AddRange(menuFromContent);

            // 2. Get menu for help
            //if (ConfigurationManager.AppSettings["UseNewHelp"] != null && bool.Parse(ConfigurationManager.AppSettings["UseNewHelp"]))
            {
                //ACMenuItem menuItemHelp = GetMenuForHelp(acComponent, acElement);
                //acMenuItemList.Add(menuItemHelp);
            }

            // 2. Menü vom acComponent 
            List<ACMenuItem> menuFromComponent = GetMenuFromComponent(acComponent, acElement);
            acMenuItemList.AddRange(menuFromComponent);

            // 3. Menü vom acElement (In der Regel also vom Steuerelement z.B. VBTextbox "Copy", "Paste", etc.)
            List<ACMenuItem> menuFromElement = GetMenuFromElement(acComponent, acElement);
            acMenuItemList.AddRange(menuFromElement);

            List<ACMenuItem> acMenuList = new List<ACMenuItem>();

            foreach (var menuItem in acMenuItemList.OrderBy(c => c.HandlerACElement != c.BSO))
            {
                short helper;
                if (short.TryParse(menuItem.ACUrl, out helper) || acMenuList.Any(c => c.ACUrl == menuItem.ACUrl && c.ACCaption == menuItem.ACCaption))
                    continue;

                if (menuItem.CategoryIndex == null)
                    acMenuList.Add(menuItem);
                else
                {
                    if (acMenuList.Any(c => c.ACUrl == menuItem.CategoryIndex.ToString()))
                    {
                        var category = acMenuList.FirstOrDefault(c => c.ACUrl == menuItem.CategoryIndex.ToString());
                        if (!category.Items.Any(c => c.ACUrl == menuItem.ACUrl && c.ACCaption == menuItem.ACCaption))
                            category.Items.Add(menuItem);
                    }

                    else if (acMenuItemList.FirstOrDefault(c => c.ACUrl == menuItem.CategoryIndex).CategoryIndex != null)
                    {
                        ACMenuItem root = acMenuList.FirstOrDefault(c => c.ACUrl == acMenuItemList.FirstOrDefault(x => x.ACUrl == menuItem.CategoryIndex).CategoryIndex);
                        if (root != null)
                        {
                            ACMenuItem parent = root.Items.FirstOrDefault(c => c.ACUrl == menuItem.CategoryIndex);
                            if (parent != null)
                                parent.Items.Add(menuItem);
                        }
                        else
                        {
                            root = acMenuItemList.FirstOrDefault(c => c.ACUrl == acMenuItemList.FirstOrDefault(x => x.ACUrl == menuItem.CategoryIndex).CategoryIndex);
                            acMenuList.Add(root);
                            ACMenuItem parent = root.Items.FirstOrDefault(c => c.ACUrl == menuItem.CategoryIndex);
                            if (parent != null)
                                parent.Items.Add(menuItem);
                            else
                            {
                                parent = acMenuItemList.FirstOrDefault(c => c.ACUrl == menuItem.CategoryIndex);
                                parent.Items.Add(menuItem);
                                root.Items.Add(parent);
                            }
                        }
                    }
                    else
                    {
                        acMenuList.Add(acMenuItemList.FirstOrDefault(c => c.ACUrl == menuItem.CategoryIndex));
                        acMenuList.FirstOrDefault(c => c.ACUrl == menuItem.CategoryIndex).Items.Add(menuItem);
                    }
                }
            }

            return SortMenu(acMenuList);
        }

        #region 1 From Content
        static private List<ACMenuItem> GetMenuFromContent(IACComponent acComponent, IACInteractiveObject acElement)
        {
            List<ACMenuItem> acMenuItemList = new List<ACMenuItem>();
            // Kein Inhalt vorhanden
            if (acElement.ACContentList == null || !acElement.ACContentList.Any())
                return acMenuItemList;
            IACObject acContent1 = acElement.ACContentList.First();                      // => ACTypeValue : ACValueType = ACClassProperty(CompanyNo), Value = "1002")
            if (acContent1 is ACClassInfoWithItems || acContent1 is ACObjectItem)
            {
                acContent1 = (acContent1 as IACContainer).Value as IACObject;
            }
            //else if (acContent1 is ACValueItem)
            //{
            //    ACValueItem item = acContent1 as ACValueItem;
            //    acContent1 = item.Value as IACObject;
            //}

            IACObject acContent2 = null;
            if (acElement.ACContentList.Count() > 1)
                acContent2 = acElement.ACContentList.ToArray()[1];

            if (acContent1 is IACMenuBuilder)
            {
                IACMenuBuilder acMenuBuilder = acContent1 as IACMenuBuilder;
                var m1 = acMenuBuilder.GetMenu(acElement.VBContent, acElement.GetType().FullName);
                if (m1 != null && m1.Any())
                {
                    foreach (var acMenuItem in m1)
                    {
                        acMenuItem.BSO = acComponent as IACBSO;
                        acMenuItemList.Add(acMenuItem);
                    }
                    //acMenuItemList.Add(new ACMenuItem(null, "-", "", null));
                }
            }
            ACCommandMsgList acCommandMsgList = null;
            // Falls 1. Content XMLConfig unterstützt, dann schauen ob eine ACCommandMsgList vorhanden ist
            if (acContent1 is IACEntityProperty)
            {
                ACCommandMsgList acCommandMsgList1 = (acContent1 as IACEntityProperty)["ACCommandMsgList"] as ACCommandMsgList;
                if (acCommandMsgList1 != null)
                    acCommandMsgList = acCommandMsgList1;
            }
            // Ansonsten falls 2. Content XMLConfig unterstützt und 1 Content ein ACValueItem ist, dann schauen ob eine ACCommandMsgList vorhanden ist
            else if (acElement.ACContentList.Count() > 1 && acContent1 is ACValueItem)
            {
                if (acContent2 is IACEntityProperty)
                {
                    ACCommandMsgList acCommandMsgList1 = (acContent2 as IACEntityProperty)["ACCommandMsgList"] as ACCommandMsgList;
                    if (acCommandMsgList1 != null)
                        acCommandMsgList = acCommandMsgList1;
                }
            }

            if (acCommandMsgList != null)
            {
                if (acContent1 == null || acContent1.ACType == null || acContent1.ACType.ValueTypeACClass.ACKind == Global.ACKinds.TACLRBaseTypes)
                {
                    if (acContent2 != null && acContent2.ACType != null && acContent2.ACType.ValueTypeACClass.ACKind != Global.ACKinds.TACLRBaseTypes)
                    {
                        List<ACMenuItem> menuForacCommandMsg = GetACCommandMsgList(acCommandMsgList, acComponent);
                        acMenuItemList.AddRange(menuForacCommandMsg);
                    }
                }
            }

            return acMenuItemList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="acComponent"></param>
        /// <param name="acElement"></param>
        static private ACMenuItem GetMenuForHelp(IACComponent acComponent, IACInteractiveObject acElement)
        {
            ACValueList helpValueList = new ACValueList();
            string bsoHelpACUrl = @"\Businessobjects#VBBSOHelp";


            List<string> acElementURLs = new List<string>();

            if (acElement.ACContentList != null)
            {
                foreach (IACObject acObject in acElement.ACContentList)
                {
                    if (acObject == null)
                        continue;
                    string acObjectURL = acObject.GetACUrl();
                    if (!string.IsNullOrEmpty(acObjectURL))
                        acElementURLs.Add(acObjectURL);
                }
            }

            helpValueList.Add(new ACValue("ACComponentURL", acComponent.GetACUrl()));
            if (acElementURLs.Any())
                helpValueList.Add(new ACValue("ACElementURLs", acElementURLs));
            //IACObject acContent1 = acElement.ACContentList.First();                      // => ACTypeValue : ACValueType = ACClassProperty(CompanyNo), Value = "1002")
            //if (acContent1 is ACClassInfoWithItems || acContent1 is ACObjectItem)
            //{6
            //    acContent1 = (acContent1 as IACContainer).Value as IACObject;
            //}

            //IACObject acContent2 = null;
            //if (acElement.ACContentList.Count() > 1)
            //    acContent2 = acElement.ACContentList.ToArray()[1];

            //// build help menu
            //if (acContent1 != null)
            //{
            //    // helpACUrl ermitteln
            //    // Ist immer eine ACUrl zu einem IACType (ACClass, ACClassProperty, ACClassMethod)
            //    // Als Hilfe wird immer der "Comment" angezeigt
            //    string caption = "";
            //    string helpACUrl = "";

            //    if (acContent1 is ACValueItem)
            //    {
            //        ACValueItem acValueItem = acContent1 as ACValueItem;
            //        if (acValueItem.ValueACType != null && !string.IsNullOrEmpty(acValueItem.ValueACType.Comment))
            //        {
            //            helpACUrl = acValueItem.ValueACType.GetACUrl();
            //            caption = acValueItem.ValueACType.ACCaption;
            //        }
            //        else if (acValueItem.ValueTypeACClass != null && acValueItem.ValueTypeACClass.ACKind == Global.ACKinds.TACSimpleType)
            //        {
            //            if (acContent2 is IACType && !string.IsNullOrEmpty((acContent2 as IACType).Comment))
            //            {
            //                helpACUrl = acContent2.GetACUrl();
            //                caption = acContent2.ACCaption;
            //            }
            //        }
            //    }
            //    else if (acContent1 is IACType && !string.IsNullOrEmpty((acContent1 as IACType).Comment))
            //    {
            //        helpACUrl = acContent1.GetACUrl();
            //        caption = acContent1.ACCaption;
            //    }

            //    // docACUrl ermitteln
            //    // Ist immer die ACUrl zu einem komplexen Typen (z.B. Company, BSOMaterial, ACClass, etc.)
            //    // da nur hierfür Dokumente verwaltet werden können
            //    string docACUrl = "";

            //    IACObject docACContent = acContent1;
            //    if (acContent1 is ACValueItem)
            //    {
            //        ACValueItem acValueItem = acContent1 as ACValueItem;
            //        if (acValueItem.ValueTypeACClass != null && acValueItem.ValueTypeACClass.ACKind == Global.ACKinds.TACSimpleType)
            //        {
            //            docACContent = acContent2;
            //        }
            //    }

            //    if (docACContent != null)
            //    {
            //        if (docACContent is ACClassProperty || docACContent is ACClassMethod)
            //        {
            //            docACContent = docACContent.ParentACObject;
            //        }
            //        if (docACContent != null)
            //        {
            //            if (docACContent.ACType is ACClassProperty)
            //            {
            //                if ((docACContent.ACType as ACClassProperty).ACClass.DocumentationList.Any())
            //                {
            //                    docACUrl = docACContent.GetACUrl();
            //                    if (string.IsNullOrEmpty(caption))
            //                        caption = docACContent.ACCaption;
            //                }
            //            }
            //            else if (docACContent.ACType is ACClass)
            //            {
            //                if ((docACContent.ACType as ACClass).DocumentationList.Any())
            //                {
            //                    docACUrl = docACContent.GetACUrl();
            //                    if (string.IsNullOrEmpty(caption))
            //                        caption = docACContent.ACCaption;
            //                }
            //            }
            //        }
            //    }
            //}

            return new ACMenuItem(null, "Help", bsoHelpACUrl, 99, helpValueList);
        }

        static private List<ACMenuItem> GetACCommandMsgList(ACCommandMsgList acCommandMsgList, IACComponent acComponent)
        {
            List<ACMenuItem> acMenuItemList = new List<ACMenuItem>();
            foreach (var acCommandMsg in acCommandMsgList)
            {
                // Falls eine ACUrl angegeben wurde, dann ist es ein direkter ACUrlCommand
                if (!string.IsNullOrEmpty(acCommandMsg.ACUrl))
                {
                    acMenuItemList.Add(new ACMenuItem(null, acCommandMsg.ACCaption, acCommandMsg.ACUrl, 0, acCommandMsg.ParameterList));
                }
                // Falls nur eine ACUrlInfo angegeben wurde, versuchen anhand des ACTypes ACUrlCommand(s) zu generieren
                else if (!string.IsNullOrEmpty(acCommandMsg.ACUrlInfo))
                {
                    IACType acType = Database.Root.GetACTypeFromACUrl(acCommandMsg.ACUrlInfo);
                    if (acType == null)
                        return acMenuItemList;
                    // TODO: Noch Menüs einfügen
                }
                acCommandMsg.BSO = acComponent as IACBSO;
            }
            return acMenuItemList;
        }
        #endregion

        #region 2 From Component
        static private List<ACMenuItem> GetMenuFromComponent(IACComponent acComponent, IACInteractiveObject acElement)
        {
            List<ACMenuItem> acMenuItemList = new List<ACMenuItem>();
            if (acComponent is IACMenuBuilder)                                      // => BSOCompany
            {
                IACMenuBuilder acMenuBuilder = acComponent as IACMenuBuilder;
                var m1 = acMenuBuilder.GetMenu(acElement.VBContent, acElement.GetType().FullName);
                if (m1 != null && m1.Any())
                {
                    foreach (var acMenuItem in m1)
                    {
                        acMenuItem.BSO = acComponent as IACBSO;
                        acMenuItemList.Add(acMenuItem);
                    }
                    // acMenuItemList.Add(new ACMenuItem(null, "-", "", null));
                }
            }
            return acMenuItemList;
        }
        #endregion

        #region 3 From Element
        static private List<ACMenuItem> GetMenuFromElement(IACComponent acComponent, IACInteractiveObject acElement)
        {
            List<ACMenuItem> acMenuItemList = new List<ACMenuItem>();
            if (acElement is IACMenuBuilder)                                        // => VBTextbox
            {
                IACMenuBuilder acMenuBuilder = acElement as IACMenuBuilder;
                var m1 = acMenuBuilder.GetMenu(acElement.VBContent, acElement.GetType().FullName);
                if (m1 != null && m1.Any())
                {
                    foreach (var acMenuItem in m1)
                    {
                        short helper;
                        if ((!string.IsNullOrEmpty(acMenuItem.ACUrlCommandString) && acMenuItemList.Where(c => c.ACUrlCommandString == acMenuItem.ACUrlCommandString).Any())
                                                                                 || (!short.TryParse(acMenuItem.ACUrl, out helper) && acMenuItemList.Any(x => x.ACUrl == acMenuItem.ACUrl)))
                            continue;
                        acMenuItem.BSO = acComponent as IACBSO;
                        acMenuItemList.Add(acMenuItem);
                    }
                    //acMenuItemList.Add(new ACMenuItem(null, "-", "", null));
                }
            }
            return acMenuItemList;
        }
        #endregion

        #region 4 SortMenu
        static private ACMenuItemList SortMenu(List<ACMenuItem> acMenuItemList)
        {
            ACMenuItemList acMenuItemList1 = new ACMenuItemList();
            short helper;

            short prevSortIndex = Convert.ToInt16(Math.Round(acMenuItemList.Min(c => c.SortIndex) / 100d, 0) * 100);

            foreach (var acMenuItem in acMenuItemList.OrderBy(c => c.SortIndex))
            {
                if (acMenuItem.Items.Any())
                    acMenuItem.Items = SortMenu(acMenuItem.Items);

                if (acMenuItem.SortIndex < 20000)
                {
                    short calcSortIndex = Convert.ToInt16(Math.Abs(acMenuItem.SortIndex / 100) * 100);
                    if (prevSortIndex < calcSortIndex && !short.TryParse(acMenuItem.ACUrl, out helper))
                    {
                        acMenuItemList1.Add(new ACMenuItem(null, "-", "", 0, null));
                    }
                    acMenuItemList1.Add(acMenuItem);
                    prevSortIndex = calcSortIndex;
                }
            }
            var query = acMenuItemList.Where(c => c.SortIndex >= 20000).OrderBy(c => c.SortIndex).ThenBy(c => c.ACCaption);
            if (query.Any())
            {
                if (acMenuItemList1.Any())
                {
                    acMenuItemList1.Add(new ACMenuItem(null, "-", "", 0, null));
                }
                foreach (var acMenuItem in query)
                {
                    acMenuItemList1.Add(acMenuItem);
                }
            }
            return acMenuItemList1;
        }
        #endregion



        //static public void ShowACElementDialog(ACComponent acComponent, IACInteractiveObject acElement)
        //{
        //    var acElementVBContent = acElement.VBContent;                           // => CurrentCompany\CompanyNo

        //    IACType dcACTypeInfo = null;                                       // => ACClassProperty "CurrentCompany" mit ACGroup = Company.ClassName
        //    object dcSource = null;
        //    string dcPath = "";
        //    Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

        //    var y = acComponent.ACUrlBinding("CurrentCompany", ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode);

        //    // Beispiel BSOCompany mit VBTextBox mit VBContent "CurrentCompany.CompanyNo"
        //    var acElementValueType = acElement.ACType;                         // =>ACClass "VBTextBox"
        //    if (acElement is IVBContent)
        //    {
        //        IVBContent vbContent = acElement as IVBContent;
        //        var vbContentValueType = vbContent.VBContentValueType;              // =>ACClassProperty "CompanyNo"
        //    }
        //    List<ACMenuItem> acMenuItemList = new List<ACMenuItem>();

        //    string message = "DropObject          : " + acElement.GetType().ToString();
        //    message += "\n>ElementACComponent : " + acElement.ContextACObject.GetACUrl();
        //    message += "\n>VBContent          : " + acElement.VBContent + "\n";
        //    foreach (var acContent in acElement.ACContentList)
        //    {
        //        message += "\n>" + acContent.ACType.ACIdentifier;
        //        if (acContent is IACObject)
        //        {
        //            IACObject acObject = acContent as IACObject;
        //            message += " : " + acObject.ACIdentifier;
        //        }
        //        if (acContent is ACValueItem)
        //        {
        //            ACValueItem acTypedValue = acContent as ACValueItem;
        //            message += " : " + acTypedValue.Value.ToString() + " (Primitive Type)";
        //        }
        //    }

        //    acComponent.Messages.Error(acComponent, "Message00001", message);
        //}
    }
}
