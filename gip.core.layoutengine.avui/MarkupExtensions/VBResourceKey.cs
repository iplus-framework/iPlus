using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Reflection;
using System.Windows.Data;

namespace gip.core.layoutengine.avui
{
    public class VBResourceKey : ResourceKey
    {
        public VBResourceKey()
            : base()
        {
        }

        private string _ACDesignProperty;
        /// <summary>
        /// Entity-ACUrl of ACDesign
        /// Usage Example: Find a DataTemplate for BSO-Mask
        /// </summary>
        public string ACDesignProperty
        {
            get
            {
                return _ACDesignProperty;
            }

            set
            {
                _ACDesignProperty = value;
            }
        }

        private string _xNameInDesign;
        /// <summary>
        /// Name of Framework-Element in Design
        /// </summary>
        public string XNameInDesign
        {
            get
            {
                return _xNameInDesign;
            }

            set
            {
                _xNameInDesign = value;
            }
        }


        private string _ACUrlWPF;
        /// <summary>
        /// Entity-ACUrl of Frameworkelement (Includes Inheritance)
        /// </summary>
        public string ACUrlWPF
        {
            get
            {
                return _ACUrlWPF;
            }

            set
            {
                _ACUrlWPF = value;
            }
        }

        private string _ACUrlProperty;
        /// <summary>
        /// Entity-ACUrl of ACProperty
        /// </summary>
        public string ACUrlProperty
        {
            get
            {
                return _ACUrlProperty;
            }

            set
            {
                _ACUrlProperty = value;
            }
        }


        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return GetHashCode();
        }


        public override bool Equals(object o)
        {
            if (o.GetHashCode() == GetHashCode())
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            if (!String.IsNullOrEmpty(_ACDesignProperty))
            {
                if (hashCode == 0)
                    hashCode = _ACDesignProperty.GetHashCode();
                else
                    hashCode = hashCode ^ _ACDesignProperty.GetHashCode();
            }
            if (!String.IsNullOrEmpty(_xNameInDesign))
            {
                if (hashCode == 0)
                    hashCode = _xNameInDesign.GetHashCode();
                else
                    hashCode = hashCode ^ _xNameInDesign.GetHashCode();
            }
            if (!String.IsNullOrEmpty(_ACUrlWPF))
            {
                if (hashCode == 0)
                    hashCode = _ACUrlWPF.GetHashCode();
                else
                    hashCode = hashCode ^ _ACUrlWPF.GetHashCode();
            }
            if (!String.IsNullOrEmpty(_ACUrlProperty))
            {
                if (hashCode == 0)
                    hashCode = _ACUrlProperty.GetHashCode();
                else
                    hashCode = hashCode ^ _ACUrlProperty.GetHashCode();
            }
            return hashCode;
        }

        public override string ToString()
        {
            return _ACDesignProperty + _xNameInDesign + _ACUrlWPF + _ACUrlProperty;
        }

        public override Assembly Assembly 
        {
            get
            {
                return null;
            }
        }
    }
}
