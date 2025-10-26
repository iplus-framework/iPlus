using Avalonia;
using Avalonia.Controls;
using gip.core.layoutengine.avui.timeline;

namespace gip.core.layoutengine.avui.ganttchart
{
    public class ConnectionsPresenter : ItemsControl
    {
        #region c'tors

        //static ConnectionsPresenter()
        //{
        //    ItemContainerStyleSelectorProperty.AddOwner(typeof(ConnectionsPresenter),
        //        new FrameworkPropertyMetadata(null, CoerceItemContainerStyleSelector));

        //}

        private static object CoerceItemContainerStyleSelector(AvaloniaObject d, object baseValue)
        {
            object value = baseValue ?? new StyleSelectorByItemType();
            return value;
        }

        #endregion

        #region Properties
        public VBGanttChart Timeline { get; set; }
        #endregion

        #region Attached Properties

        #region From
        /// <summary>
        /// Defines the FromItem attached property.
        /// </summary>
        public static readonly AttachedProperty<object> FromItemProperty =
            AvaloniaProperty.RegisterAttached<ConnectionsPresenter, AvaloniaObject, object>(
                "FromItem", 
                defaultValue: null);

        /// <summary>
        /// Gets the FromItem attached property value.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <returns>The FromItem value.</returns>
        public static object GetFromItem(AvaloniaObject obj)
        {
            return obj.GetValue(FromItemProperty);
        }

        /// <summary>
        /// Sets the FromItem attached property value.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="value">The FromItem value to set.</param>
        public static void SetFromItem(AvaloniaObject obj, object value)
        {
            obj.SetValue(FromItemProperty, value);
        }
        #endregion

        #region To
        /// <summary>
        /// Defines the ToItem attached property.
        /// </summary>
        public static readonly AttachedProperty<object> ToItemProperty =
            AvaloniaProperty.RegisterAttached<ConnectionsPresenter, AvaloniaObject, object>(
                "ToItem", 
                defaultValue: null);

        /// <summary>
        /// Gets the ToItem attached property value.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <returns>The ToItem value.</returns>
        public static object GetToItem(AvaloniaObject obj)
        {
            return obj.GetValue(ToItemProperty);
        }

        /// <summary>
        /// Sets the ToItem attached property value.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="value">The ToItem value to set.</param>
        public static void SetToItem(AvaloniaObject obj, object value)
        {
            obj.SetValue(ToItemProperty, value);
        }
        #endregion

        #endregion

        #region Methods
        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey) => new Connection();

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<Connection>(item, out recycleKey);
        }

        protected override void PrepareContainerForItemOverride(Control element, object item, int index)
        {
            if (element is Connection viewItem && Timeline != null)
            {
                object fromItem = GetFromItem(element);
                object toItem = GetToItem(element);

                if (fromItem != null && toItem != null)
                {
                    TimelineGanttItem tlFromItem = Timeline.ContainerFromItem(fromItem);
                    TimelineGanttItem tlToItem = Timeline.ContainerFromItem(toItem);

                    if (tlFromItem != null && tlToItem != null)
                    {

                        Connection conn = (Connection)element;
                        conn.SourceItem = tlFromItem;
                        conn.SinkItem = tlToItem;
                    }
                }
            }
            base.PrepareContainerForItemOverride(element, item, index);
        }

        #endregion
    }

}
