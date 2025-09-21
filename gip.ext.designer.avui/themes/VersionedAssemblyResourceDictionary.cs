// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace gip.ext.designer.avui.themes
{
	internal class VersionedAssemblyResourceDictionary : ResourceDictionary, ISupportInitialize
	{
		private static readonly string _uriStart;

		private static readonly int _subLength;

		static VersionedAssemblyResourceDictionary()
		{
			var assemblyName = typeof(VersionedAssemblyResourceDictionary).Assembly.GetName();
			_uriStart = string.Format(@"/{0};v{1};component/", assemblyName.Name, assemblyName.Version);
			_subLength = assemblyName.Name.Length + 1;
		}

		public string RelativePath {get;set;}

		void ISupportInitialize.EndInit()
		{
			//this.Source = new Uri(_uriStart + this.RelativePath, UriKind.Relative);
			//base.EndInit();
		}

		public static string GetXamlNameForType(Type t)
		{
			return _uriStart + t.FullName.Substring(_subLength).Replace(".","/").ToLower() + ".xaml";
		}

        public void BeginInit()
        {
            throw new NotImplementedException();
        }
    }
}
