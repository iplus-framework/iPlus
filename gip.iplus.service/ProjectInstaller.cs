//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Configuration.Install;
//using System.Linq;


//namespace gip.iplus.service
//{
//    [RunInstaller(true)]
//    public partial class ProjectInstaller : System.Configuration.Install.Installer
//    {
//        public ProjectInstaller()
//        {
//            InitializeComponent();
//            this.BeforeInstall += new InstallEventHandler(ProjectInstaller_BeforeInstall);
//            this.BeforeUninstall += new InstallEventHandler(ProjectInstaller_BeforeUninstall);
//        }

//        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
//        {

//        }

//        /// <summary>
//        /// Example: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil /ServiceName=iPlusServiceDJ D:\Devel\iPlusGit\iPlusV4\trunk\iPlus\bin\Debug\gip.iplus.service.exe
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        void ProjectInstaller_BeforeInstall(object sender, InstallEventArgs e)
//        {
//            if (!String.IsNullOrEmpty(this.Context.Parameters["ServiceName"]))
//            {
//                this.serviceInstaller1.DisplayName = this.Context.Parameters["ServiceName"];
//                this.serviceInstaller1.ServiceName = this.Context.Parameters["ServiceName"];
//            }
//        }

//        /// <summary>
//        /// Example: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil /ServiceName=iPlusServiceDJ /u D:\Devel\iPlusGit\iPlusV4\trunk\iPlus\bin\Debug\gip.iplus.service.exe
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        void ProjectInstaller_BeforeUninstall(object sender, InstallEventArgs e)
//        {
//            if (!String.IsNullOrEmpty(this.Context.Parameters["ServiceName"]))
//            {
//                this.serviceInstaller1.ServiceName = this.Context.Parameters["ServiceName"];
//            }
//        }
//    }
//}
