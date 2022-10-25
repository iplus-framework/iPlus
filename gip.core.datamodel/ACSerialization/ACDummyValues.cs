using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    internal class ACDummyValues
    {
#if !EFCR
        public ACDummyValues(IACEntityObjectContext db)
        {
            LoadUnknowEntries(db);
        }
#endif
        #region Properties

        /// <summary>
        /// The _ dummy AC class
        /// </summary>
        ACClass _DummyACClass;
        /// <summary>
        /// Gets the dummy AC class.
        /// </summary>
        /// <value>The dummy AC class.</value>
        public ACClass DummyACClass
        {
            get
            {
                return _DummyACClass;
            }
        }

        /// <summary>
        /// The _ dummy AC class property
        /// </summary>
        ACClassProperty _DummyACClassProperty;
        /// <summary>
        /// Gets the dummy AC class property.
        /// </summary>
        /// <value>The dummy AC class property.</value>
        public ACClassProperty DummyACClassProperty
        {
            get
            {
                return _DummyACClassProperty;
            }
        }

        /// <summary>
        /// The _ dummy AC class method
        /// </summary>
        ACClassMethod _DummyACClassMethod;
        /// <summary>
        /// Gets the dummy AC class method.
        /// </summary>
        /// <value>The dummy AC class method.</value>
        public ACClassMethod DummyACClassMethod
        {
            get
            {
                return _DummyACClassMethod;
            }
        }

        /// <summary>
        /// The _ dummy AC class design
        /// </summary>
        ACClassDesign _DummyACClassDesign;
        /// <summary>
        /// Gets the dummy AC class design.
        /// </summary>
        /// <value>The dummy AC class design.</value>
        public ACClassDesign DummyACClassDesign
        {
            get
            {
                return _DummyACClassDesign;
            }
        }

        /// <summary>
        /// The _ dummy AC class WF
        /// </summary>
        ACClassWF _DummyACClassWF;
        /// <summary>
        /// Gets the dummy AC class WF.
        /// </summary>
        /// <value>The dummy AC class WF.</value>
        public ACClassWF DummyACClassWF
        {
            get
            {
                return _DummyACClassWF;
            }
        }

        #endregion

        #region Mehtods

#if !EFCR
        public void LoadUnknowEntries(IACEntityObjectContext db)
        {
            var query = db.ContextIPlus.ACClass.Where(c =>     c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.ClassLibrary 
                                                            && c.ACKindIndex == (short)Global.ACKinds.TACUndefined 
                                                            && c.ACIdentifier == Const.UnknownClass);
            if (query.Any())
                _DummyACClass = query.First();
            if (_DummyACClass != null)
            {
                _DummyACClassProperty = _DummyACClass.ACClassProperty_ACClass.Where(c => c.ACIdentifier == Const.UnknownProperty).FirstOrDefault();
                _DummyACClassMethod = _DummyACClass.ACClassMethod_ACClass.Where(c => c.ACIdentifier == Const.UnknownMethod).FirstOrDefault();
                _DummyACClassDesign = _DummyACClass.ACClassDesign_ACClass.Where(c => c.ACIdentifier == Const.UnknownDesign).FirstOrDefault();
                if (_DummyACClassMethod != null)
                    _DummyACClassWF = _DummyACClassMethod.ACClassWF_ACClassMethod.Where(c => c.ACIdentifier == Const.UnknownWorkflow).FirstOrDefault();
            }
        }


        public ACFSItemChanges SetDummyValue(IACEntityObjectContext db, PropertyInfo pi, IACObject acObject)
        {
            object oldValue = pi.GetValue(acObject, null);
            string newValue = null;
            switch (pi.PropertyType.Name)
            {
                case ACClass.ClassName:
                    newValue = DummyACClass.GetACUrl();
                    pi.SetValue(acObject, DummyACClass, null);
                    break;
                case ACClassProperty.ClassName:
                    newValue = DummyACClassProperty.GetACUrl();
                    pi.SetValue(acObject, DummyACClassProperty, null);
                    break;
                case ACClassMethod.ClassName:
                    newValue = DummyACClassMethod.GetACUrl();
                    pi.SetValue(acObject, DummyACClassMethod, null);
                    break;
                case ACClassDesign.ClassName:
                    newValue = DummyACClassDesign.GetACUrl();
                    pi.SetValue(acObject, DummyACClassDesign, null);
                    break;
                case ACClassWF.ClassName:
                    newValue = DummyACClassWF.GetACUrl();
                    pi.SetValue(acObject, DummyACClassWF, null);
                    break;
            }
            if (newValue == null) return null;
            return new ACFSItemChanges(pi.Name, oldValue, newValue);
        }
#endif

        #endregion


    }
}
