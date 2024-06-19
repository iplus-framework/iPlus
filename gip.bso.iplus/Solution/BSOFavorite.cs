using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Favourites'}de{'Favoriten'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, false, true)]
    public class BSOFavorite : ACBSO
    {
        #region c'tors

        public BSOFavorite(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            CloseWindow(this, "AddFavDialog");
            bool result = base.ACDeInit(deleteACClassTask);
            _CurrentMenuEntry = null;
            _CurrentMenuEntryRoot = null;
            _StartupItems = null;
            _SelectedStartupItem = null;
            _vbUserACClassDesign = null;
            _FavoriteList = null;
            if (result && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return result;
        }

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

        #region Favorites

        private VBUserACClassDesign _vbUserACClassDesign;
        private VBUserACClassDesign _VBUserACClassDesign
        {
            get
            {
                if (_vbUserACClassDesign == null)
                {
                    _vbUserACClassDesign = Db.VBUserACClassDesign.FirstOrDefault(c => c.VBUserID == Root.Environment.User.VBUserID && c.ACClassDesign == null && c.ACIdentifier == "ACFavorite");
                    if (_vbUserACClassDesign == null)
                    {
                        _vbUserACClassDesign = VBUserACClassDesign.NewACObject(Db, null);
                        _vbUserACClassDesign.ACIdentifier = "ACFavorite";
                        _vbUserACClassDesign.VBUserID = Root.Environment.User.VBUserID;
                        _vbUserACClassDesign.XMLDesign = "";
                        Db.VBUserACClassDesign.AddObject(_vbUserACClassDesign);
                        Db.ACSaveChanges();
                    }
                }
                return _vbUserACClassDesign;
            }
        }

        private List<ACFavorite> _FavoriteList;
        /// <summary>
        /// Gets or sets the list of <see cref="ACFavorite"/>
        /// </summary>
        [ACPropertyList(401, "Favorite")]
        public List<ACFavorite> FavoriteList
        {
            get
            {
                if (_FavoriteList == null)
                {
                    if (!string.IsNullOrEmpty(_VBUserACClassDesign.XMLDesign))
                    {
                        try
                        {
                            using (StringReader ms = new StringReader(_VBUserACClassDesign.XMLDesign))
                            using (XmlTextReader xmlReader = new XmlTextReader(ms))
                            {
                                DataContractSerializer serializer = new DataContractSerializer(typeof(ACFavorite[]));
                                var favList = serializer.ReadObject(xmlReader);
                                if (favList is ACFavorite[])
                                    _FavoriteList = ((ACFavorite[])favList).ToList();
                            }
                        }
                        catch (Exception e)
                        {
                            Messages.LogException(this.GetACUrl(), "FavoriteList", e);
                        }
                    }
                    if (_FavoriteList == null)
                        _FavoriteList = new List<ACFavorite>();
                }
                return _FavoriteList;
            }
            set
            {
                _FavoriteList = value;
                OnPropertyChanged("FavoriteList");
            }
        }

        private ACMenuItem _CurrentMenuEntryRoot;
        /// <summary>
        /// Gets the current root menu entry (item) of the main menu. 
        /// </summary>
        [ACPropertyCurrent(402, "MenuEntryRoot", "en{'Menu'}de{'Menu'}")]
        public ACMenuItem CurrentMenuEntryRoot
        {
            get
            {
                if (_CurrentMenuEntryRoot == null)
                {
                    if (Root.Environment.User.MenuACClassDesign != null)
                        _CurrentMenuEntryRoot = Root.Environment.User.MenuACClassDesign.MenuEntry;
                    else
                        _CurrentMenuEntryRoot = core.autocomponent.ACRoot.SRoot.GetDesign(Global.ACKinds.DSDesignMenu).MenuEntry;

                }
                return _CurrentMenuEntryRoot;
            }
        }

        private ACMenuItem _CurrentMenuEntry;
        /// <summary>
        /// Gets or sets the current menu entry (item) in main menu tree. 
        /// </summary>
        [ACPropertyCurrent(403, "MenuEntry", "en{'Menu'}de{'Menu'}")]
        public ACMenuItem CurrentMenuEntry
        {
            get
            {
                return _CurrentMenuEntry;
            }
            set
            {
                _CurrentMenuEntry = value;
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        [ACMethodInteraction("", "en{'Save'}de{'Speichern'}", 401, true)]
        public void Save()
        {
            string xml = "";
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<ACFavorite>));
                serializer.WriteObject(xmlWriter, FavoriteList);
                xml = sw.ToString();
            }
            _VBUserACClassDesign.XMLDesign = xml;
            Db.ACSaveChanges();
        }

        /// <summary>
        /// Shows the dialog with main menu items. 
        /// </summary>
        [ACMethodInteraction("", "en{'Add favorite'}de{'Add favorite'}", 402, true)]
        public void AddFavorite()
        {
            ShowWindow(this, "AddFavDialog", false, Global.VBDesignContainer.DockableWindow, Global.VBDesignDockState.FloatingWindow, Global.VBDesignDockPosition.Top);
        }

        /// <summary>
        /// Creates a new favorite item (ACFavorite) and adds it to the FavoriteList based on <see cref="IVBTileGrid"/>  parameter. 
        /// </summary>
        /// <param name="vbTile">The vbTile item.</param>
        [ACMethodInfo("", "", 403)]
        public void AddVBFavorite(IVBTileGrid vbTile)
        {
            if (vbTile != null)
            {
                ACFavorite fav = new ACFavorite();
                fav.TileColumn = vbTile.TileColumn;
                fav.TileRow = vbTile.TileRow;
                fav.Title = vbTile.Title;
                fav.ACUrl = vbTile.ACUrl;
                fav.IconACUrl = vbTile.IconACUrl;
                fav.Parameters = vbTile.Parameters;
                FavoriteList.Add(fav);
                FavoriteList = FavoriteList.ToList();
            }
        }

        /// <summary>
        /// Deletes favorite item from Favorites, based on <see cref="IVBTileGrid"/> parameter. 
        /// </summary>
        /// <param name="vbTile">The vbTile item.</param>
        [ACMethodInfo("", "", 404)]
        public void DeleteVBFavorite(IVBTileGrid vbTile)
        {
            ACFavorite fav = FavoriteList.FirstOrDefault(c => c.TileColumn == vbTile.TileColumn && c.TileRow == vbTile.TileRow && c.ACUrl == vbTile.ACUrl);
            if (fav != null)
                FavoriteList.Remove(fav);
        }

        /// <summary>
        /// Starts the business object according to parameter acUrl. 
        /// </summary>
        /// <param name="acUrl">The acUrl of a business object.</param>
        /// <param name="parameters">parameters</param>
        [ACMethodInfo("", "", 405)]
        public void OnTileClicked(string acUrl, ACValueList parameters)
        {
            Root.RootPageWPF.StartBusinessobject(acUrl, parameters);
        }

        #endregion

        #region Fast startup

        List<ACFastStartupItem> _StartupItems;
        [ACPropertyList(404, "StartupItem")]
        public List<ACFastStartupItem> StartupItems
        {
            get
            {
                if (_StartupItems == null)
                {
                    _StartupItems = new List<ACFastStartupItem>();

                    ACMenuItem mainMenu;
                    if (Root.Environment.User.MenuACClassDesign == null)
                    {
                        ACClassDesign acClassDesign = ((ACRoot)Root).GetDesign(Global.ACKinds.DSDesignMenu);
                        mainMenu = acClassDesign.GetMenuEntryWithCheck(Root);
                    }
                    else
                    {
                        using (ACMonitor.Lock(Database.QueryLock_1X000))
                        {
                            mainMenu = Root.Environment.User.MenuACClassDesign.GetMenuEntryWithCheck(Root);
                        }
                    }
                    FillList(mainMenu);

                    if (Root.Environment.User.IsSuperuser)
                    {
                        IEnumerable<ACClass> BSOs =
                            Db
                            .ACClass
                            .Where(c =>
                                    c.ACKindIndex == (short)Global.ACKinds.TACBSO
                                    || c.ACKindIndex == (short)Global.ACKinds.TACBSOGlobal)
                            .ToArray()
                            .Where(bso =>
                                    !_StartupItems.Any(sti => sti.ACIdentifier == bso.ACIdentifier) && !bso.IsAbstract
                                    && bso.ACClass1_ParentACClass.ObjectFullType != null
                                    && bso.ACClass1_ParentACClass.ObjectFullType.IsAssignableFrom(typeof(Businessobjects)));

                        IEnumerable<ACClass> DiffACCaptionBSOs = BSOs.Where(bso => !_StartupItems.Any(sti => sti.ACCaption == bso.ACCaption)).ToArray();
                        IEnumerable<ACClass> SameACCaptionBSOs = BSOs.Except(DiffACCaptionBSOs).ToArray();

                        _StartupItems.AddRange(DiffACCaptionBSOs.Select(bso => new ACFastStartupItem(bso.ACCaption, bso.ACIdentifier, "#" + bso.ACIdentifier, "")));
                        if (SameACCaptionBSOs.Any())
                        {
                            _StartupItems.AddRange(SameACCaptionBSOs.Select(bso => new ACFastStartupItem(string.Format("{0}({1})", bso.ACCaption, bso.ACIdentifier),
                                                                                                     bso.ACIdentifier, "#" + bso.ACIdentifier, "")));
                        }
                    }
                    _StartupItems = _StartupItems.OrderBy(c => c.ACIdentifier).ToList();
                }
                return _StartupItems;
            }
        }

        private ACFastStartupItem _SelectedStartupItem;
        [ACPropertySelected(405, "StartupItem")]
        public ACFastStartupItem SelectedStartupItem
        {
            get
            {
                return _SelectedStartupItem;
            }
            set
            {
                _SelectedStartupItem = value;
                OnPropertyChanged("SelectedStartupItem");
            }
        }

        private void FillList(ACMenuItem menuItem)
        {
            if (menuItem == null || menuItem.Items == null)
                return;

            foreach (ACMenuItem item in menuItem.Items)
            {
                if (!string.IsNullOrEmpty(item.ACUrlCommandString))
                {
                    var newItem = new ACFastStartupItem(item.ACCaption, item.ACUrlCommandString.Split(new char[] { '#' }).LastOrDefault(), item.ACUrlCommandString, item.IconACUrl);
                    _StartupItems.Add(newItem);
                }
                FillList(item);
            }
        }

        [ACMethodInfo("", "en{'Start Application'}de{'Anwendung starten'}", 406)]
        public void StartBSO()
        {
            if (SelectedStartupItem != null)
                this.Root.RootPageWPF.ACUrlCommand(Const.CmdStartBusinessobject, SelectedStartupItem.ACUrlCommandString);
            SelectedStartupItem = null;
        }

        [ACMethodInfo("", "en{'Add'}de{'Hinzufügen'}", 999)]
        public void Add()
        {
            if (SelectedStartupItem != null)
                this.Root.RootPageWPF.ACUrlCommand(Const.CmdStartBusinessobject, SelectedStartupItem.ACUrlCommandString);
            SelectedStartupItem = null;
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Save":
                    Save();
                    return true;
                case "AddFavorite":
                    AddFavorite();
                    return true;
                case "AddVBFavorite":
                    AddVBFavorite((IVBTileGrid)acParameter[0]);
                    return true;
                case "DeleteVBFavorite":
                    DeleteVBFavorite((IVBTileGrid)acParameter[0]);
                    return true;
                case "OnTileClicked":
                    OnTileClicked((String)acParameter[0], acParameter[1] as ACValueList);
                    return true;
                case "StartBSO":
                    StartBSO();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
