// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public static class ResourceFactory
    {
        public static IResources Factory(string path, ACBackgroundWorker worker)
        {
            IResources resource = null;
            if(path != null)
            {
                if(path.Contains("|"))
                {
                    resource = new ResourcesSQL();
                }   
                else if (IsResourceZip(path))
                {
                    resource = new ResourcesZip();
                }
                    
                else if(IsResourceXML(path))
                {
                    resource = new ResourcesXML();
                }
                    
                int pos = path.LastIndexOf("\\");
                string folderName = path.Substring(pos + 1);
                if (resource == null && (folderName.Contains(ACProject.ClassName) || folderName.EndsWith(Const.ACQueryExportFileType)))
                    resource = new Resources();
            }
            if(resource == null)
                resource = new ResourcesRoot();
            resource.Worker = worker;
            return resource;
        }


        public static bool IsResourceZip(string path)
        {
            int indexOfZip = path.LastIndexOf(".zip");
            string zipFile = path.Substring(0, indexOfZip + ".zip".Length);
            return path.Contains(".zip") && Regex.IsMatch(zipFile, RegexZipPattern);
        }

        public static bool IsResourceXML(string path)
        {
            return path.EndsWith(".xml");
        }

        public static string RegexZipPattern
        {
            get
            {
                return regexZipPattern;
            }
            set
            {
                regexZipPattern = value;
            }
        }
        private static string regexZipPattern = @"(19|2)[0-9]{1}[0-9]{1}[0-9]{1}-[0-1]{1}[0-9]{1}-[0-3]{1}[0-9]{1} [0-2]{1}[0-9]{1}-[0-5]{1}[0-9]{1}_(\w*).zip";
        
    }
}
