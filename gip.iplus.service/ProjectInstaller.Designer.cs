// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿//using gip.core.autocomponent;
//using gip.core.datamodel;
//using System;

//namespace gip.iplus.service
//{
//    partial class ProjectInstaller
//    {
//        /// <summary>
//        /// Erforderliche VBDesignervariable.
//        /// </summary>
//        private System.ComponentModel.IContainer components = null;

//        /// <summary> 
//        /// Verwendete Ressourcen bereinigen.
//        /// </summary>
//        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        #region Vom Komponenten-VBDesigner generierter Code

//        /// <summary>
//        /// Erforderliche Methode für die VBDesignerunterstützung.
//        /// Erforderliche Methode für die VBDesignerunterstützung.
//        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
//            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
//            // 
//            // serviceProcessInstaller1
//            // 
//            this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.NetworkService;
//            this.serviceProcessInstaller1.Password = null;
//            this.serviceProcessInstaller1.Username = null;
//            // 
//            // serviceInstaller1
//            // 

//            string winServiceName = "IPlusService";
//            //if (!String.IsNullOrEmpty(this.Context.Parameters["ServiceName"]))
//            //{
//            //    winServiceName = this.Context.Parameters["ServiceName"];
//            //    winServiceName = this.Context.Parameters["ServiceName"];
//            //}

//            this.serviceInstaller1.DisplayName = winServiceName;
//            this.serviceInstaller1.ServiceName = winServiceName;
//            this.serviceInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceInstaller1_AfterInstall);
//            // 
//            // ProjectInstaller
//            // 
//            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
//            this.serviceProcessInstaller1,
//            this.serviceInstaller1});

//        }

//        #endregion

//        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
//        private System.ServiceProcess.ServiceInstaller serviceInstaller1;
//    }
//}