using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.manager;
using gip.ext.design.avui;
//using Irony.Parsing.Construction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static gip.core.manager.VBDesigner;

namespace gip.core.wpfservices.avui.Manager
{
    public class WFLayoutCalculatorProxy : IVBWFLayoutCalculatorProxy
    {
        public WFLayoutCalculatorProxy(IACComponent component)
        {
            _WFLayoutCalcComp = component;
        }

        #region Proxy

        private IACComponent _WFLayoutCalcComp;
        public WFLayoutCalculator WFLayoutCalcComp
        {
            get
            {
                if (_WFLayoutCalcComp == null)
                    return null;
                return _WFLayoutCalcComp as WFLayoutCalculator;
            }
        }

        #endregion

        #region Methods

        private bool CalculateNodePositionOnEdge(IEnumerable<IACWorkflowEdge> incomingEdges, IEnumerable<IACWorkflowEdge> outgoingEdges, WFItem current, WFItemList wfItemList)
        {
            IACWorkflowNode toWFNode = outgoingEdges.FirstOrDefault().ToWFNode;
            IACWorkflowNode fromWFNode = incomingEdges.FirstOrDefault().FromWFNode;

            WFItem nodeParent = wfItemList.FirstOrDefault(c => c.XName == toWFNode.XName);
            WFItem nodeChild = wfItemList.FirstOrDefault(c => c.XName == fromWFNode.XName);
            if (toWFNode == null || fromWFNode == null || nodeParent == null || nodeChild == null)
                return false;

            double currentTop = 0;
            if (nodeParent.VisualTop > nodeChild.VisualTop)
                currentTop = nodeChild.VisualTop + (nodeParent.VisualTop - nodeChild.VisualTop) / 2;
            else
                currentTop = nodeParent.VisualTop + (nodeChild.VisualTop - nodeParent.VisualTop) / 2;
            double currentLeft = 0;
            if (nodeParent.VisualLeft < nodeChild.VisualLeft)
                currentLeft = nodeParent.VisualLeft + (nodeChild.VisualLeft - nodeParent.VisualLeft) / 2;
            else
                currentLeft = nodeChild.VisualLeft + (nodeParent.VisualLeft - nodeChild.VisualLeft) / 2;
            current.VisualTop = currentTop;
            current.VisualLeft = currentLeft;

            return true;
        }

        public void CalculateNodePos(IEnumerable<IACWorkflowEdge> parentNodes, WFItem current, WFItemList wfItemList, Size size)
        {
            IACWorkflowNode parentNode = parentNodes.FirstOrDefault().ToWFNode;
            WFItem nodeParent = wfItemList.FirstOrDefault(c => c.XName == parentNode.XName);
            if (nodeParent == null)
                return;
            current.VisualTop = nodeParent.VisualTop - 60;
            current.VisualLeft = nodeParent.VisualLeft;
            wfItemList.CheckPosition(current, size);
        }

        private void UpdateItemsPosition(IACWorkflowContext wfContext, WFItemList wfItemList, VBVisualGroup parent, Size newSize)
        {
            if (wfItemList.Count() > 1)
            {
                foreach (WFItem item in wfItemList)
                {
                    VBVisual vbVisual = item.DesignItem.View as VBVisual;
                    vbVisual.ApplyTemplate();
                    IACWorkflowNode node = ((PWOfflineNode)vbVisual.ContentACObject).Content as IACWorkflowNode;
                    IEnumerable<IACWorkflowEdge> parentNodes = node.GetIncomingWFEdges(wfContext).Where(c => c.ParentACObject.ACIdentifier == vbVisual.ContentACObject.ParentACObject.ACIdentifier);
                    if (!parentNodes.Any())
                    {
                        CalculateRootItemPosition(item, newSize);
                    }
                    else
                    {
                        double differnece = newSize.Height - parent.Height;
                        item.VisualTop = item.VisualTop + differnece;
                        if (item.VisualLeft + (item.VisualWidth / 2) < parent.Width / 2)
                        {
                            item.VisualLeft = (newSize.Width - (item.VisualWidth * 2)) / 3;
                        }
                        else
                            item.VisualLeft = newSize.Width - item.VisualWidth - (newSize.Width - (item.VisualWidth * 2)) / 3;
                    }
                }
            }
        }

        public void CalculateRootItemPosition(WFItem rootItem, Size size)
        {
            rootItem.VisualTop = size.Height - rootItem.VisualTotalHeight - 20;
            rootItem.VisualLeft = size.Width / 2 - rootItem.VisualWidth / 2;
        }

        public void WFLayoutGroup(short layoutAction, object designContext, object designItemGroup, object designItem)
        {
            LayoutActionType layoutActionType = TransformShortToLayoutAction(layoutAction);
            DesignItem designItemGroupDesign = designItemGroup as DesignItem;
            DesignItem designItemDesign = designItem as DesignItem;
            var x = designItem.ToString();
            WFItemList wfItemList = new WFItemList(designItemGroupDesign, designItemDesign);

            switch (layoutActionType)
            {
                case VBDesigner.LayoutActionType.Insert:
                    // Bei neuen Classs wird die erste Position errechnet
                    wfItemList.CalculateClassPosition();
                    break;
                case VBDesigner.LayoutActionType.Delete:
                    return;
                case VBDesigner.LayoutActionType.Move:
                    break;
                case VBDesigner.LayoutActionType.InsertEdge:
                    // Bei neuen Edges wird die erste Position errechnet
                    wfItemList.CalculateClassPosition();
                    break;
                case VBDesigner.LayoutActionType.DeleteEdge:
                    break;
            }

            // Beseitigung von Layoutkonflikten
            wfItemList.ResolveConflicts();

            // Start- und Endknoten werden immer horizontal zentriert
            wfItemList.CalculateStartEndClass();

            // Richtet das Layout am linken Rand aus, damit kein unnötiger Platz verschwendet wird
            wfItemList.CalculateLeftSided();

            // Berechnen der Groupengröße und der darin enthaltenen Classs
            if (wfItemList.UpdateGroupSize1())
            {
                DesignItem parentDesignItemGroup = designItemGroupDesign.ParentDesignItemGroup();
                if (parentDesignItemGroup != null)
                {
                    // Wenn die Gruppengröße sich geändert hat, dann muß die übergeordnete Gruppe auch neu berechnet werden.
                    WFLayoutGroup((short)VBDesigner.LayoutActionType.Move, designContext, parentDesignItemGroup, designItemGroup);
                }
            }

        }

        public LayoutActionType TransformShortToLayoutAction(short layoutAction)
        {
            LayoutActionType layoutActionType;
            switch (layoutAction)
            {
                case 0:
                    layoutActionType = LayoutActionType.Insert;
                    break;
                case 1:
                    layoutActionType = LayoutActionType.Delete;
                    break;
                case 2:
                    layoutActionType = LayoutActionType.Move;
                    break;
                case 3:
                    layoutActionType = LayoutActionType.InsertEdge;
                    break;
                case 4:
                    layoutActionType = LayoutActionType.DeleteEdge;
                    break;
                default:
                    layoutActionType = LayoutActionType.Insert;
                    break;
            }
            return layoutActionType;
        }

        public void WFUpdateGroupSize(DesignItem designItemGroup /*IACWorkflowNode groupClass*/)
        {
            WFItemList classItems = new WFItemList(designItemGroup, null);
            classItems.UpdateGroupSize1();

            var parentDesignItemGroup = designItemGroup.ParentDesignItemGroup();
            if (parentDesignItemGroup != null)
            {
                WFUpdateGroupSize(parentDesignItemGroup);
            }
        }

        public void LayoutMaterialWF(IACWorkflowContext wfContext, object parent, object designItem, short layoutAction)
        {
            VBDesigner.LayoutActionType newLayoutAction = TransformShortToLayoutAction(layoutAction);
            DesignItem newDesignItem = designItem as DesignItem;
            DesignItem newParent = parent as DesignItem;

            if (VBDesigner.LayoutActionType.Insert != newLayoutAction)
                return;
            VBVisualGroup vbVisualGroup = newParent.View as VBVisualGroup;
            WFItemList wfItemList = new WFItemList(newParent, newDesignItem, false);
            Size size = SizeValue(wfItemList.Count());

            if (vbVisualGroup.RenderSize.Width < size.Width && vbVisualGroup.RenderSize.Height < size.Height)
            {
                UpdateItemsPosition(wfContext, wfItemList, vbVisualGroup, size);
                newParent.Properties[FrameworkElement.WidthProperty].CurrentValue = size.Width;
                newParent.Properties[FrameworkElement.HeightProperty].CurrentValue = size.Height;
            }
            else
                size = new Size(vbVisualGroup.RenderSize.Width, vbVisualGroup.RenderSize.Height);

            WFItem current = wfItemList.FirstOrDefault(c => c.DesignItem == newDesignItem);
            current.VisualTop = 0;
            current.VisualLeft = 0;

            VBVisual vbVisual = newDesignItem.View as VBVisual;
            vbVisual.OnApplyTemplate();
            IACWorkflowNode node = ((PWOfflineNode)vbVisual.ContentACObject).Content as IACWorkflowNode;
            //IEnumerable<IACWorkflowEdge> outgoingEdges = node.GetOutgoingWFEdges(wfContext).Where(c => c.ParentACObject.ACIdentifier == vbVisual.ContentACObject.ParentACObject.ACIdentifier);
            //IEnumerable<IACWorkflowEdge> incomingEdges = node.GetIncomingWFEdges(wfContext).Where(c => c.ParentACObject.ACIdentifier == vbVisual.ContentACObject.ParentACObject.ACIdentifier);
            IEnumerable<IACWorkflowEdge> outgoingEdges = node.GetOutgoingWFEdges(wfContext);
            IEnumerable<IACWorkflowEdge> incomingEdges = node.GetIncomingWFEdges(wfContext);

            if (!outgoingEdges.Any() || (outgoingEdges.Count() == 1 && outgoingEdges.FirstOrDefault().FromWFNode.ACIdentifier == outgoingEdges.FirstOrDefault().ToWFNode.ACIdentifier))
                CalculateRootItemPosition(current, size);

            else
            {
                if (incomingEdges.Count() == 1 && outgoingEdges.Count() == 1)
                {
                    if (!CalculateNodePositionOnEdge(incomingEdges, outgoingEdges, current, wfItemList))
                        CalculateNodePos(outgoingEdges, current, wfItemList, size);
                }
                else
                    CalculateNodePos(outgoingEdges, current, wfItemList, size);
            }
        }

        public Size SizeValue(int numOfElements)
        {
            Size size = new Size(200, 200);
            if (numOfElements > 3 && numOfElements <= 6)
                size = new Size(400, 400);

            else if (numOfElements > 6 && numOfElements <= 9)
                size = new Size(500, 500);

            else if (numOfElements > 9)
                size = new Size(600, 600);

            return size;
        }

        #endregion
    }

    /// <summary>
    /// Sortierte Liste von ClassItems (oben nach unten und links nach rechts)
    /// </summary>
    public class WFItemList : List<WFItem>
    {
        //IACWorkflowNode _GroupClass = null;
        WFItem _WFItemGroup = null;
        WFItem _WFItemSelf = null;
        /// <summary>Im Konstruktor wird die sortierte Liste erzeugt.
        /// Falls der "visualClass" mit anderen ClassItems kolidiert, wird
        /// dieser in der Sortierreihenfolge davorgestellt.</summary>
        /// <param name="designItemGroup"></param>
        /// <param name="designItem"></param>
        /// <param name="isStartMethod"></param>
        public WFItemList(DesignItem designItemGroup, DesignItem designItem, bool isStartMethod = true)
        {
            if (designItemGroup.ComponentType.Name != Const.VBVisualGroup_ClassName)
                return;
            if (designItemGroup.StartMethodWF() == null && isStartMethod)
                return;
            _WFItemGroup = new WFItem(designItemGroup);

            List<WFItem> wfItemList = new List<WFItem>();

            foreach (var di in designItemGroup.GroupDesignItemChilds())
            {
                wfItemList.Add(new WFItem(di));
            }
            var query = wfItemList.OrderBy(c => c.VisualTop).ThenBy(c => c.VisualLeft);

            // Alle Einfügen, außer das zu betrachtende visualClass selbst
            foreach (var wfItem in query)
            {
                if (wfItem.DesignItem == designItem)
                    _WFItemSelf = wfItem;
                this.Add(wfItem);
            }
        }

        /// <summary>
        /// Berechnet die optimale Position für ein neu eingefügtes Element
        /// Die Berechnung erfolgt in zwei Schritten:
        /// 1. Entsprechend der Edges die optimale Position berechnen
        /// 2. Die Position so weit wie sinnvoll möglich nach rechts schieben
        /// 
        /// 
        /// </summary>
        public void CalculateClassPosition()
        {
            ////////////////////////////////////////////////////////////////////
            // 1. Entsprechend der Edges die optimale Position berechnen
            ////////////////////////////////////////////////////////////////////
            Double minMiddle = 0;
            Double maxMiddle = 0;
            Double minTop = 0;
            // Mittlere Position über die Parentclasss ermitteln
            GetMiddleParentPositions(_WFItemSelf, ref minMiddle, ref maxMiddle);
            // Falls mehr wie ein Parent, dann auch die Childclasss berücksichtigen
            if (minMiddle != maxMiddle)
            {
                GetMiddleChildPositions(_WFItemSelf, ref minMiddle, ref maxMiddle);
            }
            // Oberste Position ermitteln
            GetTopPosition(_WFItemSelf, ref minTop);

            // Neue Position zuordnen
            _WFItemSelf.VisualTotalLeft = ((2 * minMiddle) + (maxMiddle - minMiddle) - _WFItemSelf.VisualTotalWidth) / 2;
            if (_WFItemSelf.VisualTotalLeft < WFLayoutCalculator.LeftSpace / 2)
                _WFItemSelf.VisualTotalLeft = WFLayoutCalculator.LeftSpace / 2;
            _WFItemSelf.VisualTotalTop = minTop;

            ////////////////////////////////////////////////////////////////////
            // 2. Die Position so weit wie sinnvoll möglich nach rechts schieben
            ////////////////////////////////////////////////////////////////////
            // Alle Class in gleicher Reihe
            var query = this.Where(c => c.VisualTotalTop == _WFItemSelf.VisualTotalTop && c != _WFItemSelf).OrderBy(c => c.VisualTotalLeft).Select(c => c).ToArray();
            var pClasssSelf = _WFItemSelf.ParentVisualEdgesInGroup.Select(c => c.FromWFNode).ToList();
            var cClasssSelf = _WFItemSelf.ChildVisualEdgesInGroup.Select(c => c.ToWFNode).ToList();

            foreach (var classItem in query)
            {
                //if (classItem.VisualTotalRight <= _WFItemSelf.VisualTotalLeft)
                //    continue; // Sind ohnehin weiter links, also nicht beachten

                int nextItemIndex = Array.IndexOf(query, classItem) + 1;
                if (nextItemIndex < query.Length)
                {
                    var nextItem = query[nextItemIndex];
                    double availableSpace = nextItem.VisualTotalLeft - classItem.VisualTotalRight;
                    if (availableSpace > _WFItemSelf.VisualTotalWidth)
                    {
                        _WFItemSelf.VisualTotalLeft = classItem.VisualTotalRight;
                        break;
                    }
                }

                // Wenn alle Parent und Childs identisch sind, dann neuen Knoten nach rechts verschieben
                bool ok = true;
                foreach (var pClass in pClasssSelf)
                {
                    if (!classItem.ParentVisualEdgesInGroup.Where(c => c.FromWFNode == pClass).Any())
                    {
                        ok = false;
                        break;
                    }
                }
                foreach (var cClass in cClasssSelf)
                {
                    if (!classItem.ChildVisualEdgesInGroup.Where(c => c.ToWFNode == cClass).Any())
                    {
                        ok = false;
                        break;
                    }
                }
                if (!ok)
                    break;
                _WFItemSelf.VisualTotalLeft = classItem.VisualTotalRight;
            }

        }

        /// <summary>
        /// Auflösung der positionellen und logischen Konflikte
        /// </summary>
        /// <returns></returns>
        public bool ResolveConflicts()
        {
            CheckConflicts(_WFItemSelf);

            return true;
        }

        public bool CheckPosition(WFItem item, Size containerSize)
        {
            Rect itemRect = new Rect(new Point(item.VisualLeft, item.VisualTop), new Size(item.VisualWidth, item.VisualHeight));
            bool checkFlag = false;
            foreach (WFItem wfItem in this.Where(c => c != item))
            {
                Rect wfItemRect = new Rect(new Point(wfItem.VisualLeft, wfItem.VisualTop), new Size(wfItem.VisualWidth, wfItem.VisualHeight));
                if (itemRect.Left < wfItemRect.Right && itemRect.Right > wfItemRect.Left && itemRect.Top < wfItemRect.Bottom && itemRect.Bottom > wfItemRect.Top)
                    checkFlag = true;
            }
            if (checkFlag)
            {
                item.VisualTop = item.VisualTop - 60;
                CheckPosition(item, containerSize);
            }
            return false;
        }


        /// <summary>
        /// Berechnet die Position von Start- und Endknoten
        /// </summary>
        public bool CalculateStartEndClass()
        {
            if (this.Count() < 2)
                return true;
            var queryStart = this.Where(c => !c.ParentVisualEdgesInGroup.Any());

            if (queryStart.Any())
            {
                if (!CalculateCenterPostion(queryStart.First(), false, true))
                    return false;
            }

            var queryEnd = this.Where(c => !c.ChildVisualEdgesInGroup.Any());
            if (queryEnd.Any())
            {
                if (!CalculateCenterPostion(queryEnd.First(), true, false))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Richtet das Layout am linken Rand aus, damit kein unnötiger Platz verschwendet wird
        /// </summary>
        public void CalculateLeftSided()
        {
            var min = this.Min(c => c.VisualTotalLeft);
            var delta = WFLayoutCalculator.LeftSpace / 2 - this.Min(c => c.VisualTotalLeft);
            if (delta != 0)
            {
                foreach (var classItem in this)
                {
                    classItem.VisualTotalLeft += delta;
                }
            }
        }

        public bool CalculateCenterPostion(WFItem classItem, bool checkParent, bool checkChild)
        {
            Double minMiddle = 0;
            Double maxMiddle = 0;
            if (checkParent) GetMiddleParentPositions(classItem, ref minMiddle, ref maxMiddle);
            if (checkChild) GetMiddleChildPositions(classItem, ref minMiddle, ref maxMiddle);
            double left = ((2 * minMiddle) + (maxMiddle - minMiddle) - classItem.VisualTotalWidth) / 2;

            if (classItem.VisualTotalLeft == left)
                return false;
            classItem.VisualTotalLeft = left;
            return true;
        }

        public void GetMiddleParentPositions(WFItem classItem, ref Double minMiddle, ref Double maxMiddle)
        {
            foreach (var pEdge in classItem.ParentVisualEdgesInGroup)
            {
                WFItem pClass = FindClassByRControlClass(pEdge.FromWFNode.XName);
                if (pClass == null)
                    continue;
                if (minMiddle == 0 && maxMiddle == 0)
                {
                    minMiddle = pClass.VisualTotalMiddle;
                    maxMiddle = pClass.VisualTotalMiddle;
                }
                else
                {
                    if (pClass.VisualTotalMiddle < minMiddle) minMiddle = pClass.VisualTotalMiddle;
                    if (pClass.VisualTotalMiddle > maxMiddle) maxMiddle = pClass.VisualTotalMiddle;
                }
            }
        }

        public void GetMiddleChildPositions(WFItem classItem, ref Double minMiddle, ref Double maxMiddle)
        {
            foreach (var cEdge in classItem.ChildVisualEdgesInGroup)
            {
                WFItem cClass = FindClassByRControlClass(cEdge.ToWFNode.XName);
                if (cClass == null)
                    continue;
                if (minMiddle == 0 && maxMiddle == 0)
                {
                    minMiddle = cClass.VisualTotalMiddle;
                    maxMiddle = cClass.VisualTotalMiddle;
                }
                else
                {
                    if (cClass.VisualTotalMiddle < minMiddle) minMiddle = cClass.VisualTotalMiddle;
                    if (cClass.VisualTotalMiddle > maxMiddle) maxMiddle = cClass.VisualTotalMiddle;
                }
            }
        }

        public void GetTopPosition(WFItem classItem, ref Double minTop)
        {
            foreach (var pEdge in _WFItemSelf.ParentVisualEdgesInGroup)
            {
                WFItem pClass = FindClassByRControlClass(pEdge.FromWFNode.XName);
                if (pClass == null)
                    continue;
                if (minTop < pClass.VisualTotalBottom)
                {
                    minTop = pClass.VisualTotalBottom;
                }
            }
        }

        public bool UpdateGroupSize1()
        {
            double maxBottom = 0;
            double maxLeft = 0;
            foreach (var classItem in this)
            {
                classItem.VisualTop = classItem.VisualTop;
                if (classItem.VisualTop + classItem.VisualHeight > maxBottom)
                    maxBottom = classItem.VisualTop + classItem.VisualHeight;
                classItem.VisualLeft = classItem.VisualMiddle - classItem.VisualWidth / 2;
                if (classItem.VisualLeft + classItem.VisualWidth > maxLeft)
                    maxLeft = classItem.VisualLeft + classItem.VisualWidth;
            }
            bool ret = false;
            if (_WFItemGroup.VisualHeight != maxBottom + WFLayoutCalculator.BottomSpace + WFLayoutCalculator.HeaderSpace)
            {
                _WFItemGroup.VisualHeight = maxBottom + WFLayoutCalculator.BottomSpace + WFLayoutCalculator.HeaderSpace;
                ret = true;
            }

            if (_WFItemGroup.VisualWidth != maxLeft + WFLayoutCalculator.LeftSpace)
            {
                _WFItemGroup.VisualWidth = maxLeft + WFLayoutCalculator.LeftSpace;
                ret = true;
            }

            return ret;
        }

        public WFItem FindClassByRControlClass(string xName)
        {
            return this.Where(c => c.XName == xName).FirstOrDefault();
        }

        private bool CheckConflicts(WFItem classItemCheck, bool isFromInsertedClassItem = true)
        {
            foreach (var classItem in this)
            {
                // Falls verschoben, dann dieses Element auf neue Konflikte prüfen
                if (classItemCheck.ResolveConflict(this, classItem, isFromInsertedClassItem))
                {
                    CheckConflicts(classItem, false);
                }
            }
            return true;
        }

    }

    /// <summary>
    /// Das ClassItem Kapselt die einzelnen RControl-Classs und versieht diese mit einem virtuellen Margin.
    /// Die Positionsberechnung erfolgt immer auf den ClassItems
    /// </summary>
    public class WFItem
    {
        public WFItem(DesignItem designItem/*IACWorkflowNode vbVisualWF*/)
        {
            DesignItem = designItem;
        }

        public DesignItem DesignItem { get; set; }

        public IEnumerable<IACWorkflowEdge> ParentVisualEdgesInGroup
        {
            get
            {
                string xName = this.XName;
                List<IACWorkflowEdge> acVisualEdgeList = new List<IACWorkflowEdge>();
                foreach (var designItemEdge in this.DesignItem.DesignItemEdges())
                {
                    VBEdge vbEdge = designItemEdge.View as VBEdge;
                    IACWorkflowEdge a = vbEdge.ContentACObject as IACWorkflowEdge;
                    if (a != null && a.ToWFNode.XName == xName)
                    {
                        acVisualEdgeList.Add(a);
                    }
                }
                return acVisualEdgeList;
                //if (this.EntityState != System.Data.EntityState.Added && !ACClassMethod.ACClassWFEdge_ACClassMethod.IsLoaded)
                //    ACClassMethod.ACClassWFEdge_ACClassMethod.Load();
                //return ParentVisualEdges.Where(c => ((ACClassWF)c.FromVisualClass).ACClassWF1_ParentACClassWF.ACClassWFID == this.ACClassWF1_ParentACClassWF.ACClassWFID).Select(c => c);
            }
        }
        public IEnumerable<IACWorkflowEdge> ChildVisualEdgesInGroup
        {
            get
            {
                string xName = this.XName;
                List<IACWorkflowEdge> acVisualEdgeList = new List<IACWorkflowEdge>();
                foreach (var designItemEdge in this.DesignItem.DesignItemEdges())
                {
                    VBEdge vbEdge = designItemEdge.View as VBEdge;
                    IACWorkflowEdge a = vbEdge.ContentACObject as IACWorkflowEdge;
                    if (a != null && a.FromWFNode.XName == xName)
                    {
                        acVisualEdgeList.Add(a);
                    }
                }
                return acVisualEdgeList;
                //if (this.EntityState != System.Data.EntityState.Added && !ACClassMethod.ACClassWFEdge_ACClassMethod.IsLoaded)
                //    ACClassMethod.ACClassWFEdge_ACClassMethod.Load();
                //return ChildVisualEdges.Where(c => ((ACClassWF)c.ToVisualClass).ACClassWF1_ParentACClassWF.ACClassWFID == this.ACClassWF1_ParentACClassWF.ACClassWFID).Select(c => c);
            }
        }

        public string XName
        {
            get
            {
                return DesignItem.Properties["Name"].CurrentValue as string;
            }
        }

        #region Position und Größe
        public Double VisualLeft
        {
            get
            {
                return (Double)DesignItem.Properties.GetAttachedProperty(Canvas.LeftProperty).CurrentValue;
            }
            set
            {
                DesignItem.Properties.GetAttachedProperty(Canvas.LeftProperty).CurrentValue = value;
            }
        }

        public Double VisualTop
        {
            get
            {
                return (Double)DesignItem.Properties.GetAttachedProperty(Canvas.TopProperty).CurrentValue;
            }
            set
            {
                DesignItem.Properties.GetAttachedProperty(Canvas.TopProperty).CurrentValue = value;
            }
        }

        public Double VisualWidth
        {
            get
            {
                return (Double)DesignItem.Properties[FrameworkElement.WidthProperty].CurrentValue;
            }
            set
            {
                DesignItem.Properties[FrameworkElement.WidthProperty].CurrentValue = value;
            }
        }

        public Double VisualHeight
        {
            get
            {
                return (Double)DesignItem.Properties[FrameworkElement.HeightProperty].CurrentValue;
            }
            set
            {
                DesignItem.Properties[FrameworkElement.HeightProperty].CurrentValue = value;
            }
        }

        public Double VisualMiddle
        {
            get
            {
                return VisualLeft + (VisualWidth / 2);
            }
            set
            {
                VisualLeft = value - (VisualWidth / 2);
            }
        }

        public Double VisualTotalLeft
        {
            get
            {
                return VisualLeft - WFLayoutCalculator.HorzSpace / 2;
            }
            set
            {
                VisualLeft = value + WFLayoutCalculator.HorzSpace / 2;
            }
        }

        public Double VisualTotalTop
        {
            get
            {
                return VisualTop - WFLayoutCalculator.VertSpace / 2;
            }
            set
            {
                VisualTop = value + WFLayoutCalculator.VertSpace / 2;
            }
        }

        public Double VisualTotalWidth
        {
            get
            {
                return VisualWidth + WFLayoutCalculator.HorzSpace;
            }
        }

        public Double VisualTotalHeight
        {
            get
            {
                return VisualHeight + WFLayoutCalculator.VertSpace;
            }
        }

        public Double VisualTotalMiddle
        {
            get
            {
                return VisualTotalLeft + VisualTotalWidth / 2;
            }
        }

        public Double VisualTotalBottom
        {
            get
            {
                return VisualTotalTop + VisualTotalHeight;
            }
        }

        public Double VisualTotalRight
        {
            get
            {
                return VisualTotalLeft + VisualTotalWidth;
            }
        }
        #endregion

        #region Konflikte
        /// <summary>Prüft auf Kollission und verschiebt das Element
        /// this        -&gt; bleibt immer auf der Position
        /// classItem    -&gt; wird nach rechts oder unten verschoben</summary>
        /// <param name="classItemList"></param>
        /// <param name="classItem"></param>
        /// <param name="isFromInsertedClassItem"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        public bool ResolveConflict(WFItemList classItemList, WFItem classItem, bool isFromInsertedClassItem = true)
        {
            if (classItem == this)
                return false;

            if (ResolveEdgeConflict(classItemList, classItem, isFromInsertedClassItem))
                return true;

            if (ResolveCollisionConflict(classItemList, classItem))
                return true;

            return false;
        }

        bool ResolveEdgeConflict(WFItemList classItemList, WFItem classItem, bool isFromInsertedClassItem)
        {
            foreach (var pEdge in classItem.ParentVisualEdgesInGroup)
            {
                if (pEdge.FromWFNode.XName == this.XName)
                {
                    WFItem pClassItem = classItemList.FindClassByRControlClass(pEdge.FromWFNode.XName);
                    if (pClassItem == null)
                        continue;
                    if (classItem.VisualTotalTop < this.VisualTotalBottom)
                    {
                        if (!isFromInsertedClassItem && classItem.VisualTotalBottom < this.VisualTotalTop)
                            return false;

                        classItem.VisualTotalTop = this.VisualTotalBottom;
                        return true;
                    }
                }
            }

            return false;
        }

        bool ResolveCollisionConflict(WFItemList classItemList, WFItem classItem)
        {
            if (this.VisualTotalLeft >= classItem.VisualTotalRight || this.VisualTotalRight <= classItem.VisualTotalLeft)
                return false; // Keine horizontale Überschneidung

            if (this.VisualTotalTop >= classItem.VisualTotalBottom || this.VisualTotalBottom <= classItem.VisualTotalTop)
                return false; // Keine vertikale Überschneidung

            classItem.VisualTotalLeft = this.VisualTotalRight;

            return true;
        }
        #endregion

        public override string ToString()
        {
            return this.XName;
        }
    }

    static public class DesignInfos
    {
        static public DesignItem StartMethodWF(this DesignItem designItem)
        {
            DesignItem canvasItem = (designItem.ContentProperty).Value;

            if (canvasItem.ContentProperty != null && !canvasItem.ContentProperty.IsCollection)
                return null;
            var collection = canvasItem.ContentProperty.CollectionElements;
            foreach (var content in collection)
            {
                if (!(content.View is VBVisual))
                    continue;

                string vbContent = content.Properties["VBContent"].ValueOnInstance as string;
                if (vbContent == Const.TPWNodeStart)
                    return content;
            }
            return null;
        }

        /// <summary>
        /// Childs innerhalb eines VBVisualGroup-DesignItems
        /// </summary>
        /// <param name="designItem"></param>
        /// <returns></returns>
        static public IEnumerable<DesignItem> GroupDesignItemChilds(this DesignItem designItem)
        {
            DesignItem canvasItem = (designItem.ContentProperty).Value;

            if (canvasItem.ContentProperty != null && !canvasItem.ContentProperty.IsCollection)
                return null;
            return canvasItem.ContentProperty.CollectionElements;
        }

        /// <summary>
        /// Childs innerhalb eines VBCanvas-DesignItems
        /// </summary>
        /// <param name="designItem"></param>
        /// <returns></returns>
        static public IEnumerable<DesignItem> DesignItemEdges(this DesignItem designItem)
        {
            DesignItem vbCanvas = designItem.Context.RootItem;

            if (vbCanvas.ContentProperty != null && !vbCanvas.ContentProperty.IsCollection)
                return null;

            return vbCanvas.ContentProperty.CollectionElements.Where(c => c.View is VBEdge);
        }

        /// <summary>
        /// Ermittelt die übergeordnete Gruppe für ein DesignItem vom Typ VBVisul oder VBVisualGroup
        /// </summary>
        /// <param name="designItem"></param>
        /// <returns></returns>
        static public DesignItem ParentDesignItemGroup(this DesignItem designItem)
        {
            if (designItem.Parent == null)
                return null;
            if (designItem.Parent.Parent == null)
                return null;
            if (!(designItem.Parent.Parent.View is VBVisualGroup))
                return null;
            return designItem.Parent.Parent;
        }
    }
}
