using gip.core.datamodel;
using gip.ext.design.avui;
using System;

namespace gip.core.layoutengine.avui
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
            get { return new Uri("/gip.core.layoutengine.avui;Component/Controls/VBRibbon/Icons/" + _IconName + ".xaml", UriKind.Relative); }
        }


        public string ResourceKey
        {
            get { return "Icon" + _IconName + "StyleGip"; }
        }
    }
}
