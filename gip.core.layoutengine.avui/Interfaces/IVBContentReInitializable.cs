namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Implemented by VB-Controls whose VBContent-Binding is resolved and cached
    /// during <c>InitVBControl()</c>. <see cref="ReInitVBContent"/> forces a new
    /// resolution against the current <c>DataContext</c>.
    /// Used e.g. by shared tooltip content, which must re-bind to the currently
    /// hovered item each time a tooltip opens.
    /// </summary>
    public interface IVBContentReInitializable
    {
        /// <summary>
        /// Re-evaluates the VBContent-Binding against the current DataContext.
        /// </summary>
        void ReInitVBContent();
    }
}
