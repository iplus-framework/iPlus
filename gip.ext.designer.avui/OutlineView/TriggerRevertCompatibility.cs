using System;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.OutlineView
{
    [CLSCompliant(false)]
    public enum TriggerRevertCompatibilityMode
    {
        Auto,
        ForceNativeRevertOnFalse,
        ForceLegacyInverseFallback
    }

    [CLSCompliant(false)]
    public static class TriggerRevertCompatibility
    {
#if USE_AVALONIA_FORK
        public static TriggerRevertCompatibilityMode Mode { get; set; } = TriggerRevertCompatibilityMode.Auto;
#else
        public static TriggerRevertCompatibilityMode Mode { get; set; } = TriggerRevertCompatibilityMode.ForceLegacyInverseFallback;
#endif

        public static bool UseLegacyInverseFallback(DesignItem triggerItem)
        {
            switch (Mode)
            {
                case TriggerRevertCompatibilityMode.ForceLegacyInverseFallback:
                    return true;
                case TriggerRevertCompatibilityMode.ForceNativeRevertOnFalse:
                    return !TryEnableRevertOnFalse(triggerItem);
                case TriggerRevertCompatibilityMode.Auto:
                default:
                    return !TryEnableRevertOnFalse(triggerItem);
            }
        }

        public static bool TryEnableRevertOnFalse(DesignItem triggerItem)
        {
            if (triggerItem?.Properties == null)
                return false;

            var revertOnFalseProperty = triggerItem.Properties.HasProperty("RevertOnFalse");
            if (revertOnFalseProperty == null)
                return false;

            revertOnFalseProperty.SetValue(true);
            return true;
        }
    }
}