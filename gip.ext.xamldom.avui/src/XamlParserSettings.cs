// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.xamldom.avui
{
	/// <summary>
	/// Delegate used for XamlParserSettings.CreateInstanceCallback.
	/// </summary>
	public delegate object CreateInstanceCallback(Type type, object[] arguments);
	
	/// <summary>
	/// Settings used for the XamlParser.
	/// </summary>
	public sealed class XamlParserSettings
	{
		CreateInstanceCallback _createInstanceCallback = Activator.CreateInstance;
		XamlTypeFinder _typeFinder = XamlTypeFinder.CreateWpfTypeFinder();
		IServiceProvider _serviceProvider = DummyServiceProvider.Instance;
		
		/// <summary>
		/// Gets/Sets the method used to create object instances.
		/// </summary>
		public CreateInstanceCallback CreateInstanceCallback {
			get { return _createInstanceCallback; }
			set {
				if (value == null)
					throw new ArgumentNullException("value");
				_createInstanceCallback = value;
			}
		}
		
		/// <summary>
		/// Gets/Sets the type finder to do type lookup.
		/// </summary>
		public XamlTypeFinder TypeFinder {
			get { return _typeFinder; }
			set {
				if (value == null)
					throw new ArgumentNullException("value");
				_typeFinder = value;
			}
		}
		
		/// <summary>
		/// Gets/Sets the service provider to use to initialize markup extensions.
		/// </summary>
		public IServiceProvider ServiceProvider {
			get { return _serviceProvider; }
			set {
				if (value == null)
					throw new ArgumentNullException("value");
				_serviceProvider = value;
			}
		}
		
		sealed class DummyServiceProvider : IServiceProvider
		{
			public static readonly DummyServiceProvider Instance = new DummyServiceProvider();
			
			public object GetService(Type serviceType)
			{
				return null;
			}
		}
	}
}
