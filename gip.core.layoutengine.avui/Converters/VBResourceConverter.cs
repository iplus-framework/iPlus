using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using gip.core.datamodel;
using System;
using System.Globalization;
using System.IO;

namespace gip.core.layoutengine.avui.Converters
{
    /// <summary>
    /// Converter that loads VB resources based on DataContext
    /// </summary>
    public class VBResourceConverter : IValueConverter
    {
        /// <summary>
        /// The VBContent path to resolve
        /// </summary>
        public string VBContent { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            try
            {
                if (value is IACObject acObject)
                {
                    IACObject objForDesign = acObject;
                    
                    // Resolve VBContent path if specified
                    if (!string.IsNullOrEmpty(VBContent))
                    {
                        object cmdResult = acObject.ACUrlCommand(VBContent);
                        if (cmdResult is IACObject acResult)
                            objForDesign = acResult;
                    }

                    // Try to get the resource based on target type
                    if (typeof(IImage).IsAssignableFrom(targetType) || 
                        typeof(Bitmap).IsAssignableFrom(targetType))
                    {
                        // Load bitmap
                        if (objForDesign?.ACType is ACClass acClass)
                        {
                            var acClassDesign = acClass.GetDesign(acClass, Global.ACUsages.DUIcon, Global.ACKinds.DSBitmapResource);
                            
                            if (acClassDesign?.DesignBinary != null)
                            {
                                using (var ms = new MemoryStream(acClassDesign.DesignBinary))
                                {
                                    return new Bitmap(ms);
                                }
                            }
                        }
                    }
                    else if (typeof(IBrush).IsAssignableFrom(targetType))
                    {
                        // Load brush from XAML design
                        if (objForDesign?.ACType is ACClass acClass)
                        {
                            var acClassDesign = acClass.GetDesign(acClass, Global.ACUsages.DUIcon, Global.ACKinds.DSBitmapResource);
                            
                            if (!string.IsNullOrEmpty(acClassDesign?.XAMLDesign))
                            {
                                Brush brushRes = Layoutgenerator.LoadXAMLResource(acClassDesign.XAMLDesign) as Brush;
                                if (brushRes != null)
                                {
                                    return brushRes;
                                }
                            }
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
                        nameof(VBResourceConverter), 
                        nameof(Convert), 
                        msg);
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
