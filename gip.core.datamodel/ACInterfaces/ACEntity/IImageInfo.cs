namespace gip.core.datamodel
{
    /// <summary>
    /// Entity items supports default image (Grid preview)
    /// </summary>
    public interface  IImageInfo
    {
        string DefaultImage { get; set; }
        string DefaultThumbImage { get; set; }
    }
}
