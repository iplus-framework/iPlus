namespace gip.core.datamodel
{
    /// <summary>
    /// General format for dialog result
    /// </summary>
    public class VBDialogResult
    {
        public IACObject ReturnValue { get; set; }

        public eMsgButton SelectedCommand { get; set; }
    }
}
