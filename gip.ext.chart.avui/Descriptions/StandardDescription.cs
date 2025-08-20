using System;
using System.Windows;

namespace gip.ext.chart.avui
{
    public class StandardDescription : Description
    {
        public StandardDescription() { }
        public StandardDescription(string description)
        {
            if (String.IsNullOrEmpty(description))
                throw new ArgumentNullException("description");

            this.description = description;
        }

        protected override void AttachCore(UIElement element)
        {
            if (description == null)
            {
                string str = element.GetType().Name;
                description = str;
            }
        }

		private string description;
		public string DescriptionString {
			get { return description; }
			set { description = value; }
		}

        public sealed override string Brief
        {
            get { return description; }
        }

        public sealed override string Full
        {
            get { return description; }
        }

        /// <summary>
        /// Gip-Extension: IsLineGraphVisible
        /// </summary>
        public override bool IsLineGraphVisible
        {
            get
            {
                if (ViewportElement == null)
                    return true;
                if (ViewportElement is LineGraph)
                    return (ViewportElement as LineGraph).IsLineGraphVisible;
                return false;
            }
            set
            {
                if ((ViewportElement != null) && (ViewportElement is LineGraph))
                    (ViewportElement as LineGraph).IsLineGraphVisible = value;
            }
        }

    }
}
