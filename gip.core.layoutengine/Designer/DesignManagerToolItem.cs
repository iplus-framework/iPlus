using System;
using gip.ext.design;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    public class DesignManagerToolItem : ACObjectItem 
    {
        public DesignManagerToolItem(string caption, ITool tool, string iconName)
            : base(caption)
        {
            _CreateControlTool = tool;
            _IconName = iconName;
        }

        private string _IconName;

        private ITool _CreateControlTool;
        public ITool CreateControlTool
        {
			get {
                return _CreateControlTool;
			}
			set {
                _CreateControlTool = value;
			}
		}

        public Uri IconResourceDictUri
        {
            get { return new Uri("/gip.core.layoutengine;Component/Controls/VBRibbon/Icons/" + _IconName + ".xaml", UriKind.Relative); }
        }


        public string ResourceKey
        {
            get { return "Icon" + _IconName + "StyleGip"; }
        }
    }
}
