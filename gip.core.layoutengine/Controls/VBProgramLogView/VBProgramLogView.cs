using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBProgramLogView'}de{'VBProgramLogView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBProgramLogView : VBTreeView
    {

        #region Properties

        #endregion

        #region Constructors

        static VBProgramLogView()
        {
            _styleInfoList = new List<CustomControlStyleInfo>
                             { 
                                new CustomControlStyleInfo
                                {
                                    wpfTheme = eWpfTheme.Gip, 
                                    styleName = "ProgramLogViewStyleGip", 
                                    styleUri = "/gip.core.layoutengine;Component/Controls/VBProgramLogView/Themes/ProgramLogViewStyleGip.xaml" 
                                }
                             };

            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBProgramLogView), new FrameworkPropertyMetadata(typeof(VBProgramLogView)));
        }

        #endregion

        #region Protected

        /// <summary>
        /// Gets the container for item override.
        /// </summary>
        /// <returns>The new instance of VBProgramLogViewItem.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new VBProgramLogViewItem();
        }

        /// <summary>
        /// Determines is item overrides it's own container.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if item is VBProgramLogViewItem, otherwise false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is VBProgramLogViewItem;
        }

        /// <summary>
        /// Prepares container for item override.
        /// </summary>
        /// <param name="element">The element parameter.</param>
        /// <param name="item">The item parameter.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element != item)
            {
                base.PrepareContainerForItemOverride(element, item);
                if (element is VBProgramLogViewItem)
                    ((VBProgramLogViewItem)element).PrepareContainer((VBProgramLogViewItem)element, item);
            }
        }

        #endregion

    }
}
