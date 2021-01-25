using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.manager;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Program log'}de{'Program log'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACProgram.ClassName)]
    public class BSOProgramLog : ACBSO
    {
        #region c'tors

        public BSOProgramLog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._AccessACProgram = null;
            this._CurrentACProgram = null;
            this._SelectedACProgram = null;
            bool done = base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return done;
        }

        #endregion

        #region Private members

        private ACAccess<ACProgram> _AccessACProgram;
        private ACProgram _CurrentACProgram, _SelectedACProgram;

        #endregion

        #region DB

        private Database _BSODatabase = null;
        /// <summary>
        /// Overriden: Returns a separate database context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_BSODatabase == null)
                    _BSODatabase = ACObjectContextManager.GetOrCreateContext<Database>(this.GetACUrl());
                return _BSODatabase;
            }
        }

        public Database Db
        {
            get
            {
                return Database as Database;
            }
        }

        #endregion

        #region Properties

        [ACPropertyAccessPrimary(490, "ACProgram")]
        public ACAccess<ACProgram> AccessACProgram
        {
            get
            {
                if (_AccessACProgram == null && ACType != null)
                {
                    ACQueryDefinition acQueryDefinition = Root.Queries.CreateQuery(null, Const.QueryPrefix + ACProgram.ClassName, Const.QueryPrefix + ACProgram.ClassName);
                    _AccessACProgram = acQueryDefinition.NewAccess<ACProgram>("OnlineValue", this);
                }
                return _AccessACProgram;
            }
        }

        [ACPropertyCurrent(401, "ACProgram")]
        public ACProgram CurrentACProgram
        {
            get
            {
                return _CurrentACProgram;
            }
            set
            {
                if (_CurrentACProgram != value)
                {
                    _CurrentACProgram = value;
                    OnPropertyChanged("CurrentACProgram");
                }
            }
        }

        [ACPropertySelected(402, "ACProgram")]
        public ACProgram SelectedACProgram
        {
            get
            {
                return _SelectedACProgram;
            }
            set
            {
                if (_SelectedACProgram != value)
                {
                    _SelectedACProgram = value;
                    OnPropertyChanged("SelectedACProgram");
                    OnPropertyChanged("SelectedProgramLogList");
                }
            }
        }

        [ACPropertyList(403, "ACProgram")]
        public IEnumerable<ACProgram> ACProgramList
        {
            get
            {
                if (AccessACProgram == null)
                    return null;
                return (IEnumerable<ACProgram>)Db.ACSelect(AccessACProgram.NavACQueryDefinition);
            }
        }

        public IEnumerable <ACProgramLog > SelectedProgramLogList
        {
            get
            {
                if (_SelectedACProgram != null)
                    return _SelectedACProgram.ACProgramLog_ACProgram.Where(C => C.ParentACProgramLogID == null);
                else
                    return null; 
            }
        }

        #endregion
    }
}
