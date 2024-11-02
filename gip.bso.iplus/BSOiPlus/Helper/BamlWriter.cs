// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//using Microsoft.Build.BuildEngine;
//using Microsoft.Build.Framework;
using System.Xml.Linq;
using gip.core.datamodel;

namespace gip.bso.iplus.VarioBatch.Helper
{
    /// <summary>
    /// Baml writer.
    /// </summary>
    public static class BamlWriter
    {
        /// <summary>
        /// Creates a new project and compile xaml to baml.
        /// </summary>
        /// <param name="xaml">The xaml for compilaton.</param>
        /// <returns>Compiled xaml in byte array</returns>
        public static byte[] Save(string xaml)
        {
            //BuildLogger logger = new BuildLogger();

            //if (xaml.Contains("{vb:VisibilityNullConverter}"))
            //    xaml = InsertVBNStaticResource(xaml);
            //if (xaml.Contains("{vb:ConverterVisibilityBool}"))
            //    xaml = InsertCVBStaticResource(xaml);

            //Encoding aEncoding = null;
            //if (xaml.IndexOf("utf-16") > 0 || xaml.IndexOf("UTF-16") > 0)
            //    aEncoding = System.Text.Encoding.GetEncoding("utf-16");
            //else
            //    aEncoding = System.Text.Encoding.GetEncoding("utf-8");

            //xaml = CheckOrUpdateNamespaceInLayout(xaml);

            //string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            //Directory.CreateDirectory(path);

            //try
            //{
            //    string xamlFile = Path.Combine(path, "input.xaml");
            //    string projFile = Path.Combine(path, "project.csproj");

            //    using (FileStream fs = File.Create(xamlFile))
            //    {
            //        byte[] xamlBytes = aEncoding.GetBytes(xaml);
            //        fs.Write(xamlBytes, 0, xamlBytes.Length);
            //    }

            //    Engine engine = new Engine();
            //    engine.RegisterLogger(logger);
            //    Project project = engine.CreateNewProject();

            //    BuildPropertyGroup pgroup = project.AddNewPropertyGroup(false);
            //    pgroup.AddNewProperty("AssemblyName", "temp");
            //    pgroup.AddNewProperty("OutputType", "Library");
            //    pgroup.AddNewProperty("IntermediateOutputPath", ".");
            //    pgroup.AddNewProperty("MarkupCompilePass1DependsOn", "ResolveReferences");
            //    pgroup.AddNewProperty("TargetFrameworkVersion", "v4.5.2");

            //    BuildItemGroup igroup = project.AddNewItemGroup();
            //    igroup.AddNewItem("Page", "input.xaml");

            //    BuildItem buildItem = igroup.AddNewItem("Reference", "WindowsBase");
            //    string windowsBasePath = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(c => c.GetName().Name == "WindowsBase").Location.ToString();
            //    buildItem.SetMetadata("HintPath", windowsBasePath);

            //    BuildItem buildItem2 = igroup.AddNewItem("Reference", "PresentationCore");
            //    string presentationCorePath = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(c => c.GetName().Name == "PresentationCore").Location.ToString();
            //    buildItem2.SetMetadata("HintPath", presentationCorePath);

            //    BuildItem buildItem3 = igroup.AddNewItem("Reference", "PresentationFramework");
            //    string presentationFrameworkPath = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(c => c.GetName().Name == "PresentationFramework").Location.ToString();
            //    buildItem3.SetMetadata("HintPath", presentationFrameworkPath);

            //    string corePath = AppDomain.CurrentDomain.BaseDirectory;
            //    BuildItem buildItem4 = igroup.AddNewItem("Reference", "gip.core.layoutengine");
            //    string layoutEnginePath = corePath + "gip.core.layoutengine.dll";
            //    buildItem4.SetMetadata("HintPath", layoutEnginePath);

            //    BuildItem buildItem5 = igroup.AddNewItem("Reference", "gip.core.datamodel");
            //    string dataModelPath = corePath + "gip.core.datamodel.dll";
            //    buildItem5.SetMetadata("HintPath", dataModelPath);

            //    BuildItem buildItem6 = igroup.AddNewItem("Reference", "gip.ext.graphics.shapes");
            //    string graphicsPath = corePath + "gip.ext.graphics.dll";
            //    buildItem6.SetMetadata("HintPath", graphicsPath);

            //    BuildItem buildItem7 = igroup.AddNewItem("Reference", "gip.ext.visualcontrols");
            //    string visualControlsPath = corePath + "gip.core.visualcontrols.dll";
            //    buildItem7.SetMetadata("HintPath", visualControlsPath);

            //    BuildItem buildItem8 = igroup.AddNewItem("Reference", "gip.core.reporthandler.Flowdoc");
            //    string reportHandlerPath = corePath + "gip.core.reporthandler.dll";
            //    buildItem8.SetMetadata("HintPath", reportHandlerPath);

            //    BuildItem buildItem9 = igroup.AddNewItem("Reference", "gip.core.manager");
            //    string managerPath = corePath + "gip.core.manager.dll";
            //    buildItem9.SetMetadata("HintPath", managerPath);

            //    project.AddNewImport(@"$(MSBuildBinPath)\Microsoft.CSharp.targets", null);
            //    project.AddNewImport(@"$(MSBuildBinPath)\Microsoft.WinFX.targets", null);

            //    project.FullFileName = projFile;

            //    if (engine.BuildProject(project, "MarkupCompilePass1"))
            //    {
            //        using (FileStream fs = File.OpenRead(Path.Combine(path, "input.baml")))
            //        {
            //            byte[] buffer = new byte[fs.Length];
            //            fs.Read(buffer, 0, buffer.Length);
            //            return buffer;
            //        }
            //    }
            //    else
            //    {
            //        // attach a logger to the Engine if you need better errors  
            //        //throw new System.Exception("Baml compilation failed.");
            //        return null;
            //    }
            //}
            //finally
            //{
            //    Directory.Delete(path, true);
            //    if (logger.Messages.Last().Contains("FAILED"))
            //        foreach (string msg in logger.Messages.Skip(logger.Messages.Count - 20))
            //            Debug.WriteLine(msg);
            //    else
            //        Debug.WriteLine("Baml successfully compiled.");
            //}
            return null;
        }

        /// <summary>
        /// Insert VisibilityNullConverter in resource dictionary, and replace in xaml {vb:VisibilityNullConverter} with {StaticResource VisibilityNullConverter}.
        /// </summary>
        /// <param name="xaml">The xaml.</param>
        /// <returns>Xaml with inserted VisibilityNullConverter in resource dictionary</returns>
        private static string InsertVBNStaticResource(string xaml)
        {
            xaml = xmlUpdate(xaml, "    <vb:VisibilityNullConverter x:Key=\"VisibilityNullConverter\"/>\r\n");
            xaml = xaml.Replace("{vb:VisibilityNullConverter}", "{StaticResource VisibilityNullConverter}");
            return xaml;
        }

        /// <summary>
        /// Insert ConverterVisibityBool in resource dictionary, and replace in xaml {vb:ConvertorVisibityBool} with {StaticResource ConverterVisibityBool}.
        /// </summary>
        /// <param name="xaml">The xaml.</param>
        /// <returns>Xaml with inserted ConverterVisibityBool in resource dictionary</returns>
        private static string InsertCVBStaticResource(string xaml)
        {
            xaml = xmlUpdate(xaml, "    <vb:ConverterVisibilityBool x:Key=\"ConverterVisibilityBool\"/>\r\n");
            xaml = xaml.Replace("{vb:ConverterVisibilityBool}", "{StaticResource ConverterVisibilityBool}");
            return xaml;
        }

        /// <summary>
        /// Insert or update resource dictionary in xaml.
        /// </summary>
        /// <param name="xaml">The xaml</param>
        /// <param name="insertRecord">The record.</param>
        /// <returns>The xaml with inserted record in resource dictionary</returns>
        private static string xmlUpdate(string xaml, string insertRecord)
        {
            if (xaml.Contains("<?xml"))
            {
                string control = xaml.Split(' ')[2].Split('<')[1];
                if (!xaml.Contains("<ResourceDictionary"))
                {
                    string insertDict = "\r\n<" + control + ".Resources>\r\n  " + "<ResourceDictionary>\r\n   " + insertRecord + "  </ResourceDictionary>\r\n" + "</" + control + ".Resources>\r\n  ";
                    int insertPos = xaml.IndexOf("\">");
                    if (insertPos > 1)
                    {
                        xaml = xaml.Insert(insertPos + 2, insertDict);
                        insertPos = 0;
                    }
                    else
                        insertPos = xaml.IndexOf(" >");
                    if (insertPos > 1)
                        xaml = xaml.Insert(insertPos + 2, insertDict);
                }
                else if (xaml.Contains("<ResourceDictionary"))
                {
                    int insertPos = xaml.IndexOf("</ResourceDictionary>") - 2;
                    if (insertPos > 1)
                        xaml = xaml.Insert(insertPos, insertRecord);
                }
            }
            else
            {
                string control = xaml.Split('<')[1].Split(' ')[0];
                if (!xaml.Contains("<ResourceDictionary"))
                {
                    string insertDict = "\r\n<" + control + ".Resources>\r\n  " + "<ResourceDictionary>\r\n   " + insertRecord + "  </ResourceDictionary>\r\n" + "</" + control + ".Resources>\r\n  ";
                    int insertPos = xaml.IndexOf("\">");
                    if (insertPos > 1)
                    {
                        xaml = xaml.Insert(insertPos + 2, insertDict);
                        insertPos = 0;
                    }
                    else
                        insertPos = xaml.IndexOf(" >");
                    if (insertPos > 1)
                        xaml = xaml.Insert(insertPos + 2, insertDict);
                }
                else if (xaml.Contains("<ResourceDictionary"))
                {
                    int insertPos = xaml.IndexOf("</ResourceDictionary>") - 22;
                    if (insertPos > 1)
                        xaml = xaml.Insert(insertPos, insertRecord);
                }
            }
            return xaml;
        }

        private static string CheckOrUpdateNamespaceInLayout(string xmlLayout)
        {
            if (String.IsNullOrEmpty(xmlLayout))
                return xmlLayout;

            int posXmlDekl = xmlLayout.IndexOf("<?xml");
            if (posXmlDekl >= 0)
            {
                posXmlDekl = xmlLayout.IndexOf("<", posXmlDekl + 1);
                if (posXmlDekl >= 0)
                    xmlLayout = xmlLayout.Substring(posXmlDekl);
            }

            int pos1 = xmlLayout.IndexOf(">");
            string part1 = xmlLayout.Substring(0, pos1);
            string part2 = xmlLayout.Substring(pos1);
            // Falls keine Namespace-Deklaration im Root-Element vorhanden, dann füge ein
            if (part1.IndexOf("xmlns") < 0)
            {
                foreach (string nameSpace in ACxmlnsResolver.NamespaceList)
                    part1 += " " + nameSpace;
            }
            part1 += part2;
            return part1;
        }
    }
}
