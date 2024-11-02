// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class ACClassWFSearchByURL
    {
        public string ACIdentifier { get; set; }
        public ACClassWF WFNode { get; set; }
        public ACClassWFSearchByURL Parent { get; set; }
        private List<ACClassWFSearchByURL> children;
        public List<ACClassWFSearchByURL> Children
        {
            get
            {
                if (children == null)
                    children = new List<ACClassWFSearchByURL>();
                return children;
            }
        }

        private bool urlCompleted;
        public bool URLCompleted
        {
            get
            {
                return urlCompleted;
            }
            set
            {
                urlCompleted = value;
                if (Parent != null)
                    Parent.URLCompleted = value;
            }
        }

        /// <summary>
        /// Item last on end of URL
        /// </summary>



        public ACClassWFSearchByURL(ACClassWFSearchByURL parent, ACClassWF current, List<string> urlParts)
        {
            ACIdentifier = current.ACIdentifier;
            WFNode = current;
            Parent = parent;
            if (parent != null)
            {
                parent.Children.Add(this);
            }
            if (urlParts != null && urlParts.Count > 0)
            {
                string tmpUrl = urlParts[0];
                urlParts.RemoveAt(0);
                List<ACClassWF> childWFs = new List<gip.core.datamodel.ACClassWF>();
                if (current.RefPAACClassMethodID != null && current.RefPAACClassMethod.ACKindIndex == (short)Global.ACKinds.MSWorkflow)
                {
                    childWFs = current.RefPAACClassMethod.ACClassWF_ACClassMethod.Where(c => c.ParentACClassWFID == null && c.ACIdentifier == tmpUrl).OrderBy(c => c.ACInstanceNo).ToList();
                }
                else
                {
                    childWFs = current.ACClassWF_ParentACClassWF.Where(c => c.ACIdentifier == tmpUrl).OrderBy(c => c.ACInstanceNo).ToList();
                }
                foreach (var element in childWFs)
                {
                    new ACClassWFSearchByURL(this, element, urlParts.ToList());
                }
            }
            else
            {
                URLCompleted = true;
            }
        }

        public string URL
        {
            get
            {
                return (Parent != null ? Parent.URL + @"\" : "") + ACIdentifier;
            }
        }

        public ACClassWFSearchByURL GetItem(string url)
        {
            if (this.URL == url)
                return this;
            else
            {
                if (Children != null)
                    foreach (var child in Children)
                    {
                        ACClassWFSearchByURL tmpItem = child.GetItem(url);
                        if (tmpItem != null)
                            return tmpItem;
                    }
            }
            return null;
        }

        public static Dictionary<int, ACClassWFSearchByURL> Search(Database db, string url)
        {
            List<string> urlParts = ParseURL(url);
            string tmpUrl = urlParts[0];
            urlParts.RemoveAt(0);
            List<ACClassWF> wpfs = db.ACClassWF.Where(c => c.ParentACClassWFID == null && c.ACIdentifier == tmpUrl).ToList();
            Dictionary<int, ACClassWFSearchByURL> result = new Dictionary<int, ACClassWFSearchByURL>();
            int nr = 1;
            foreach (var element in wpfs)
            {
                var treeItem = new ACClassWFSearchByURL(null, element, urlParts.ToList());
                result.Add(nr, treeItem);
                nr++;
            }
            return result;
        }

        public static List<string> ParseURL(string url)
        {
            string pattern = @"([\w\(\)\d]*)\\";
            List<string> items = new List<string>();
            Match mt = Regex.Match(url, pattern);
            while (mt != null && mt.Success)
            {
                items.Add(mt.Groups[1].Value);
                mt = mt.NextMatch();
            }
            return items;
        }

    }
}
