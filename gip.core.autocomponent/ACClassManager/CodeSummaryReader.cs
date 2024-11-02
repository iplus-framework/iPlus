// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="XMLHelper.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Class XMLHelper
    /// </summary>
    public class CodeSummaryReader
    {
        /// <summary>
        /// The _ XML comments
        /// </summary>
        XElement _XMLComments = null;

        /// <summary>
        /// Opens the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        public void Open(string file)
        {
            try
            {
                if (string.IsNullOrEmpty(file))
                    _XMLComments = null;
                else
                    _XMLComments = XElement.Load(file);
            }
            catch (Exception e)
            {
                _XMLComments = null;

                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("XMLHelper", "Open", msg);
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            _XMLComments = null;
        }

        /// <summary>
        /// Gets the summary class.
        /// </summary>
        /// <param name="AssemblyQualifiedName">Name of the assembly qualified.</param>
        /// <returns>System.String.</returns>
        public string GetSummaryClass(string AssemblyQualifiedName)
        {
            if (_XMLComments == null)
                return null;
            //         <member name="T:gip.core.autocomponent.ACComponent">
            try
            {
                string searchKey = "T:" + AssemblyQualifiedName;

                var query = _XMLComments.Elements("members").First().Elements("member").Where(c => (string)c.Attribute("name") == searchKey);

                if (!query.Any())
                    return null;
                string comment = query.First().Elements("summary").First().Value;
                if (string.IsNullOrEmpty(comment))
                    return null;
                if (comment[0] == '\n')
                    comment = comment.Substring(1);
                return comment.Trim();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("XMLHelper", "GetSummaryClass", msg);
            }
            return null;
        }

        /// <summary>
        /// Gets the summary method.
        /// </summary>
        /// <param name="AssemblyQualifiedName">Name of the assembly qualified.</param>
        /// <param name="mi">The mi.</param>
        /// <returns>System.String.</returns>
        public string GetSummaryMethod(string AssemblyQualifiedName, MethodInfo mi)
        {
            if (_XMLComments == null)
                return null;
            // <member name="M:gip.core.autocomponent.ACComponent.OnPropertyChanged(System.String)">
            try
            {
                string method = mi.ToString();
                method = method.Substring(method.IndexOf(' ') + 1);
                string searchKey = "M:" + AssemblyQualifiedName + "." + method;

                var query = _XMLComments.Elements("members").First().Elements("member").Where(c => (string)c.Attribute("name") == searchKey);

                if (!query.Any())
                    return null;
                string comment = query.First().Elements("summary").First().Value;
                if (!string.IsNullOrEmpty(comment) && comment[0] == '\n')
                    comment = comment.Substring(1);
                return comment.Trim();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("XMLHelper", "GetSummaryMethod", msg);
            }
            return null;
        }

        /// <summary>
        /// Gets the summary property.
        /// </summary>
        /// <param name="AssemblyQualifiedName">Name of the assembly qualified.</param>
        /// <param name="pi">The pi.</param>
        /// <returns>System.String.</returns>
        public string GetSummaryProperty(string AssemblyQualifiedName, PropertyInfo pi)
        {
            if (_XMLComments == null)
                return null;
            try
            {
                // <member name="P:gip.core.autocomponent.ACComponent.ACState">
                string property = pi.ToString();
                property = property.Substring(property.IndexOf(' ') + 1);
                string searchKey = "P:" + AssemblyQualifiedName + "." + property;
                var query = _XMLComments.Elements("members").First().Elements("member").Where(c => (string)c.Attribute("name") == searchKey);

                if (!query.Any())
                    return null;
                string comment = query.First().Elements("summary").First().Value;
                if (!string.IsNullOrEmpty(comment) && comment[0] == '\n')
                    comment = comment.Substring(1);
                return comment.Trim();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("XMLHelper", "GetSummaryProperty", msg);
            }
            return null;
        }
    }
}
