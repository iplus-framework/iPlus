// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.IO;
using static gip.core.autocomponent.PARole;

namespace gip.core.media
{
    /// <summary>
    /// Server-side media service. It performs all file system access on the machine where the
    /// media root folder is reachable and returns the content as <see cref="byte"/> arrays
    /// together with format information (<see cref="MediaContentResult"/>).
    /// <para>
    /// Clients (desktop, browser, mobile) reach this component through its proxy instance at
    /// <see cref="DefaultServiceACUrl"/>. The runtime handles the transport, therefore no
    /// client/server plumbing is required here.
    /// </para>
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Media Service'}de{'Media Service'}", Global.ACKinds.TPARole, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACMediaService : PARole
    {
        #region const

        public const string C_DefaultServiceACIdentifier = "ACMediaService";
        public const string DefaultServiceACUrl = "\\Service\\ACMediaService";

        public const string Const_DefImage = MediaFileOperations.Const_DefImage;
        public const string Const_DefThumbImage = MediaFileOperations.Const_DefThumbImage;

        #endregion

        #region c'tors

        public ACMediaService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            MediaSettings settings = new MediaSettings(null);
            _Operations = new MediaFileOperations(settings, (method, ec) => Messages.LogException(this.GetACUrl(), method, ec));
            if (!settings.IsMediaRootConfigured)
            {
                Messages.LogWarning(this.GetACUrl(), nameof(ACInit),
                    $"Media root folder is not configured. Set the config value '{settings.MediaRootConfigName}' of BSOMedia for this operating system.");
            }
            else if (!_Operations.IsMediaRootAccessible)
            {
                Messages.LogWarning(this.GetACUrl(), nameof(ACInit),
                    $"Media root folder '{settings.MediaRootFolder}' is not accessible on this machine. Check that the share is mounted.");
            }
            return true;
        }

        public override System.Threading.Tasks.Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            _Operations = null;
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Service resolution

        /// <summary>
        /// Resolves the media service. The result is the real <see cref="ACMediaService"/> if the service runs
        /// in this process, otherwise an ACComponentProxy. Therefore the return type is IACComponent and
        /// all calls must go through the static Invoke... wrappers (ExecuteMethod / ACUrlCommand).
        /// </summary>
        public static ACComponent GetServiceInstance(ACComponent requester)
        {
            return GetServiceInstance<ACComponent>(requester, C_DefaultServiceACIdentifier, CreationBehaviour.Default);
        }

        public static ACRef<ACComponent> ACRefToServiceInstance(ACComponent requester)
        {
            ACComponent serviceInstance = GetServiceInstance(requester);
            if (serviceInstance != null)
                return new ACRef<ACComponent>(serviceInstance, requester);
            return null;
        }

        public static void DetachACRefFromServiceInstance(ACComponent requester, ACRef<ACComponent> acRef)
        {
            if (acRef == null)
                return;
            acRef.Detach();
        }

        #endregion

        #region Properties

        private MediaFileOperations _Operations;
        protected MediaFileOperations Operations
        {
            get { return _Operations; }
        }

        public MediaSettings MediaSettings
        {
            get { return _Operations?.MediaSettings; }
        }

        public bool IsMediaRootConfigured
        {
            get { return _Operations != null && _Operations.IsMediaRootConfigured; }
        }

        #endregion

        #region Methods (ACMethods, invoked via ExecuteMethod / ACUrlCommand)

        /// <summary>
        /// These methods are the server-side implementation. Do not call them directly on a reference
        /// that was resolved via <see cref="GetServiceInstance"/>, because it may be an ACComponentProxy.
        /// Use the static wrappers of this class (e.g. <see cref="InvokeGetFileContent"/>) or
        /// <see cref="ACMediaController"/>, which route the call through ExecuteMethod.
        /// </summary>

        [ACMethodInfo("", "en{'Get file content'}de{'Dateiinhalt lesen'}", 300)]
        public MediaContentResult GetFileContent(string filePath)
        {
            return Operations.GetFileContent(filePath);
        }

        [ACMethodInfo("", "en{'Get thumb content'}de{'Daumenbildinhalt lesen'}", 301)]
        public MediaContentResult GetThumbContent(string filePath, MediaItemTypeEnum mediaType)
        {
            return Operations.GetThumbContent(filePath, mediaType);
        }

        [ACMethodInfo("", "en{'Get empty image'}de{'Standardbild lesen'}", 302)]
        public MediaContentResult GetEmptyImageContent()
        {
            return Operations.GetEmptyImageContent();
        }

        [ACMethodInfo("", "en{'Get empty thumb image'}de{'Standard-Daumenbild lesen'}", 303)]
        public MediaContentResult GetEmptyThumbImageContent()
        {
            return Operations.GetEmptyThumbImageContent();
        }

        [ACMethodInfo("", "en{'Get file type icon'}de{'Dateityp-Symbol lesen'}", 304)]
        public MediaContentResult GetIconContent(string extension)
        {
            return Operations.GetIconContent(extension);
        }

        /// <summary>
        /// Lists the media files of an AC object. The AC object is identified by its ACUrl and
        /// the ACUrl of its type, so that no server-side instance is required.
        /// </summary>
        [ACMethodInfo("", "en{'Get media files'}de{'Mediendateien lesen'}", 310)]
        public MediaItemListResult GetFiles(string typeACUrl, string itemACUrl, MediaItemTypeEnum mediaType, int pageIndex, string filterName, string order, bool ascending)
        {
            return Operations.GetFiles(typeACUrl, itemACUrl, mediaType, pageIndex, filterName, order, ascending);
        }

        [ACMethodInfo("", "en{'Upload media file'}de{'Mediendatei speichern'}", 320)]
        public MediaContentResult UploadFile(string typeACUrl, string itemACUrl, MediaItemTypeEnum mediaType, string fileName, byte[] content, bool isDefault, bool generateThumb, byte[] thumbContent)
        {
            return Operations.UploadFile(typeACUrl, itemACUrl, mediaType, fileName, content, isDefault, generateThumb, thumbContent);
        }

        [ACMethodInfo("", "en{'Delete media file'}de{'Mediendatei löschen'}", 321)]
        public bool DeleteFile(string filePath)
        {
            return Operations.DeleteFile(filePath);
        }

        [ACMethodInfo("", "en{'Delete media folder'}de{'Medienordner löschen'}", 322)]
        public bool CleanUpFolder(string typeACUrl, string itemACUrl, bool forceDelete)
        {
            return Operations.CleanUpFolder(typeACUrl, itemACUrl, forceDelete);
        }

        [ACMethodInfo("", "en{'Resize image'}de{'Bild skalieren'}", 330)]
        public MediaContentResult ResizeImage(byte[] content, int maxWidth, int maxHeight)
        {
            return Operations.ResizeImage(content, maxWidth, maxHeight);
        }

        [ACMethodInfo("", "en{'Get media root folder'}de{'Medien-Stammordner lesen'}", 340)]
        public string GetMediaRootFolder()
        {
            return Operations.GetMediaRootFolder();
        }

        #endregion

        #region Static wrappers (proxy-safe)

        // Every wrapper goes through ExecuteMethod: on an ACComponentProxy this is a remote
        // invocation, on the real instance it is dispatched locally via HandleExecuteACMethod.

        public static MediaContentResult InvokeGetFileContent(IACComponent service, string filePath)
        {
            return service.ExecuteMethod(nameof(GetFileContent), filePath) as MediaContentResult;
        }

        public static MediaContentResult InvokeGetThumbContent(IACComponent service, string filePath, MediaItemTypeEnum mediaType)
        {
            return service.ExecuteMethod(nameof(GetThumbContent), filePath, mediaType) as MediaContentResult;
        }

        public static MediaContentResult InvokeGetEmptyImageContent(IACComponent service)
        {
            return service.ExecuteMethod(nameof(GetEmptyImageContent)) as MediaContentResult;
        }

        public static MediaContentResult InvokeGetEmptyThumbImageContent(IACComponent service)
        {
            return service.ExecuteMethod(nameof(GetEmptyThumbImageContent)) as MediaContentResult;
        }

        public static MediaContentResult InvokeGetIconContent(IACComponent service, string extension)
        {
            return service.ExecuteMethod(nameof(GetIconContent), extension) as MediaContentResult;
        }

        public static MediaItemListResult InvokeGetFiles(IACComponent service, string typeACUrl, string itemACUrl, MediaItemTypeEnum mediaType, int pageIndex, string filterName, string order, bool ascending)
        {
            return service.ExecuteMethod(nameof(GetFiles), typeACUrl, itemACUrl, mediaType, pageIndex, filterName, order, ascending) as MediaItemListResult;
        }

        public static MediaContentResult InvokeUploadFile(IACComponent service, string typeACUrl, string itemACUrl, MediaItemTypeEnum mediaType, string fileName, byte[] content, bool isDefault, bool generateThumb, byte[] thumbContent)
        {
            return service.ExecuteMethod(nameof(UploadFile), typeACUrl, itemACUrl, mediaType, fileName, content, isDefault, generateThumb, thumbContent) as MediaContentResult;
        }

        public static bool InvokeDeleteFile(IACComponent service, string filePath)
        {
            return service.ExecuteMethod(nameof(DeleteFile), filePath) is bool result && result;
        }

        public static bool InvokeCleanUpFolder(IACComponent service, string typeACUrl, string itemACUrl, bool forceDelete)
        {
            return service.ExecuteMethod(nameof(CleanUpFolder), typeACUrl, itemACUrl, forceDelete) is bool result && result;
        }

        public static MediaContentResult InvokeResizeImage(IACComponent service, byte[] content, int maxWidth, int maxHeight)
        {
            return service.ExecuteMethod(nameof(ResizeImage), content, maxWidth, maxHeight) as MediaContentResult;
        }

        public static string InvokeGetMediaRootFolder(IACComponent service)
        {
            return service.ExecuteMethod(nameof(GetMediaRootFolder)) as string;
        }

        #endregion

        #region HandleExecuteACMethod

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(GetFileContent):
                    result = GetFileContent(acParameter[0] as string);
                    return true;
                case nameof(GetThumbContent):
                    result = GetThumbContent(acParameter[0] as string, (MediaItemTypeEnum)acParameter[1]);
                    return true;
                case nameof(GetEmptyImageContent):
                    result = GetEmptyImageContent();
                    return true;
                case nameof(GetEmptyThumbImageContent):
                    result = GetEmptyThumbImageContent();
                    return true;
                case nameof(GetIconContent):
                    result = GetIconContent(acParameter[0] as string);
                    return true;
                case nameof(GetFiles):
                    result = GetFiles(acParameter[0] as string, acParameter[1] as string, (MediaItemTypeEnum)acParameter[2],
                                      (int)acParameter[3], acParameter[4] as string, acParameter[5] as string, (bool)acParameter[6]);
                    return true;
                case nameof(UploadFile):
                    result = UploadFile(acParameter[0] as string, acParameter[1] as string, (MediaItemTypeEnum)acParameter[2],
                                        acParameter[3] as string, acParameter[4] as byte[], (bool)acParameter[5], (bool)acParameter[6], acParameter[7] as byte[]);
                    return true;
                case nameof(DeleteFile):
                    result = DeleteFile(acParameter[0] as string);
                    return true;
                case nameof(CleanUpFolder):
                    result = CleanUpFolder(acParameter[0] as string, acParameter[1] as string, (bool)acParameter[2]);
                    return true;
                case nameof(ResizeImage):
                    result = ResizeImage(acParameter[0] as byte[], (int)acParameter[1], (int)acParameter[2]);
                    return true;
                case nameof(GetMediaRootFolder):
                    result = GetMediaRootFolder();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
