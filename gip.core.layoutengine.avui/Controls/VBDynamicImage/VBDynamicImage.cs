using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using gip.core.datamodel;
using System;
using System.IO;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// An image control that dynamically loads resources based on VBContent and DataContext.
    /// Uses the application resource dictionary for caching to avoid redundant database reads.
    /// </summary>
    public class VBDynamicImage : Image
    {
        /// <summary>
        /// Defines the VBContent property
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBDynamicImage, string>(nameof(VBContent));

        /// <summary>
        /// Gets or sets the VBContent path for resolving the image resource
        /// </summary>
        public string VBContent
        {
            get => GetValue(VBContentProperty);
            set => SetValue(VBContentProperty, value);
        }

        static VBDynamicImage()
        {
            DataContextProperty.Changed.AddClassHandler<VBDynamicImage>((x, e) => x.OnDataContextOrVBContentChanged());
            VBContentProperty.Changed.AddClassHandler<VBDynamicImage>((x, e) => x.OnDataContextOrVBContentChanged());
        }

        private void OnDataContextOrVBContentChanged()
        {
            if (DataContext == null)
                return;

            try
            {
                if (DataContext is IACObject acObject)
                {
                    IACObject objForDesign = acObject;
                    
                    // Resolve VBContent path if specified
                    if (!string.IsNullOrEmpty(VBContent))
                    {
                        object cmdResult = acObject.ACUrlCommand(VBContent);
                        if (cmdResult is IACObject acResult)
                            objForDesign = acResult;
                        else if (cmdResult is string strResult && !string.IsNullOrEmpty(strResult))
                        {
                            // VBContent returned a resource key string
                            LoadFromResourceKey(strResult.Replace(Const.ContextDatabase + "\\", ""));
                            return;
                        }
                    }

                    // Get ACClassDesign for the object
                    ACClassDesign acClassDesign = null;
                    
                    if (objForDesign is ACObjectItem && (objForDesign as ACObjectItem).ACObject != null)
                        objForDesign = (objForDesign as ACObjectItem).ACObject;
                    else if (objForDesign is IACContainer && (objForDesign as IACContainer).Value != null && (objForDesign as IACContainer).Value is IACObject)
                        objForDesign = (objForDesign as IACContainer).Value as IACObject;
                    
                    if (objForDesign?.ACType != null)
                    {
                        if (objForDesign is ACClassDesign)
                            acClassDesign = objForDesign as ACClassDesign;
                        else
                            acClassDesign = objForDesign.ACType.GetDesign(objForDesign, Global.ACUsages.DUIcon, Global.ACKinds.DSBitmapResource);
                        
                        if (acClassDesign != null)
                        {
                            string resourceKey = acClassDesign.GetACUrl().Replace(Const.ContextDatabase + "\\", "");
                            LoadFromResourceKey(resourceKey, acClassDesign);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null && ex.InnerException.Message != null)
                    msg += " Inner:" + ex.InnerException.Message;

                if (datamodel.Database.Root?.Messages != null && 
                    datamodel.Database.Root.InitState == ACInitState.Initialized)
                {
                    datamodel.Database.Root.Messages.LogException(
                        nameof(VBDynamicImage), 
                        nameof(OnDataContextOrVBContentChanged), 
                        msg);
                }
                
                // Set fallback image on error
                Source = new Bitmap(AssetLoader.Open(new Uri("avares://gip.core.layoutengine.avui/Images/QuestionMark.JPG")));
            }
        }

        private void LoadFromResourceKey(string resourceKey, ACClassDesign acClassDesign = null)
        {
            if (string.IsNullOrEmpty(resourceKey))
                return;

            try
            {
                IACComponent acComponent = Layoutgenerator.CurrentACComponent;
                if (acComponent == null)
                    acComponent = Layoutgenerator.Root;

                // Get application for resource dictionary caching
                Application wpfApplication = null;
                if (acComponent?.Root?.RootPageWPF != null && acComponent.Root.RootPageWPF.WPFApplication != null)
                    wpfApplication = acComponent.Root.RootPageWPF.WPFApplication as Application;

                // First, check if already cached in application resources
                if (wpfApplication != null && wpfApplication.Resources.TryGetResource(resourceKey, null, out var cachedResource))
                {
                    if (cachedResource is Bitmap cachedBitmap)
                    {
                        Source = cachedBitmap;
                        return;
                    }
                }

                // Not cached, need to load from database
                if (acClassDesign == null)
                {
                    // Resolve ACClassDesign from resource key
                    if (resourceKey.StartsWith(Const.ContextDatabase + "\\"))
                    {
                        string dbUrl = resourceKey.Substring(9);
                        acClassDesign = acComponent.Database.ContextIPlus.ACUrlCommand(dbUrl) as ACClassDesign;
                    }
                    else
                    {
                        if (resourceKey.Contains(ACClass.ClassName) || resourceKey.Contains(ACClassDesign.ClassName) || 
                            resourceKey.Contains(ACClassMethod.ClassName) || resourceKey.Contains(ACClassProperty.ClassName) || 
                            resourceKey.Contains(ACProject.ClassName))
                        {
                            acClassDesign = acComponent.Database.ContextIPlus.ACUrlCommand(resourceKey) as ACClassDesign;
                        }
                        
                        if (acClassDesign == null)
                        {
                            acClassDesign = acComponent.ACUrlCommand(resourceKey) as ACClassDesign;
                            if (acClassDesign == null)
                                acClassDesign = acComponent.Database.ContextIPlus.ACUrlCommand(resourceKey) as ACClassDesign;
                        }
                    }
                }

                // Load bitmap from ACClassDesign and cache it
                if (acClassDesign != null && acClassDesign.ACKind == Global.ACKinds.DSBitmapResource && acClassDesign.DesignBinary != null)
                {
                    try
                    {
                        Bitmap bitmapImage = new Bitmap(new MemoryStream(acClassDesign.DesignBinary));
                        
                        // Cache in application resources to avoid redundant database reads
                        if (wpfApplication != null && !wpfApplication.Resources.ContainsKey(resourceKey))
                        {
                            wpfApplication.Resources.Add(resourceKey, bitmapImage);
                        }
                        
                        Source = bitmapImage;
                        return;
                    }
                    catch (Exception e)
                    {
                        if (datamodel.Database.Root?.Messages != null && 
                            datamodel.Database.Root.InitState == ACInitState.Initialized)
                        {
                            datamodel.Database.Root.Messages.LogException(nameof(VBDynamicImage), nameof(LoadFromResourceKey), e);
                            datamodel.Database.Root.Messages.LogException(nameof(VBDynamicImage), nameof(LoadFromResourceKey), 
                                $"Can't create icon for {acClassDesign.GetACUrl()}. Invalid Binary");
                        }
                    }
                }
                
                // Fallback image if loading failed
                Source = new Bitmap(AssetLoader.Open(new Uri("avares://gip.core.layoutengine.avui/Images/QuestionMark.JPG")));
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null && ex.InnerException.Message != null)
                    msg += " Inner:" + ex.InnerException.Message;

                if (datamodel.Database.Root?.Messages != null && 
                    datamodel.Database.Root.InitState == ACInitState.Initialized)
                {
                    datamodel.Database.Root.Messages.LogException(nameof(VBDynamicImage), nameof(LoadFromResourceKey), msg);
                }
                
                // Set fallback image on error
                Source = new Bitmap(AssetLoader.Open(new Uri("avares://gip.core.layoutengine.avui/Images/QuestionMark.JPG")));
            }
        }
    }
}
