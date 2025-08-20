using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Baml2006;
using System.Windows.Markup;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Baml reader.
    /// </summary>
    public static class BamlReader
    {
        /// <summary>
        /// Load baml from byte array and create object.
        /// </summary>
        /// <param name="designBinary">Baml in byte array.</param>
        /// <returns>Created object(UIElement).</returns>
        public static object Load(byte[] designBinary)
        {
            using (MemoryStream ms = new MemoryStream(designBinary))
            {
                var reader = new Baml2006Reader(ms);
                Assembly assembly = typeof(XamlReader).Assembly;
                if (assembly == null)
                    return null;
                Type wpfXamlLoader = assembly.GetType("System.Windows.Markup.WpfXamlLoader");
                if (wpfXamlLoader == null)
                    return null;
                MethodInfo loadBaml = wpfXamlLoader.GetMethod("LoadBaml", BindingFlags.Public | BindingFlags.Static);
                if (loadBaml != null)
                    return loadBaml.Invoke(null, new object[] { reader, true, null, null, null });
                else
                    return null;
            }
        }
    }
}
