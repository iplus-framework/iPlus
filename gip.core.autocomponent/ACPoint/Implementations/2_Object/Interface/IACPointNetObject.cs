// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACPointNetObject<T> : IACPointNet<T, ACPointNetWrapObject<T>>
        where T : IACObject 
    {
        /// <summary>
        /// Returns a "WrapObject" by addressing it over a acURL.
        /// </summary>
        /// <param name="acURL"></param>
        /// <returns>Returns a "WrapObject"</returns>
        ACPointNetWrapObject<T> GetWrapObject(string acURL);

        /// <summary>
        /// Returns a "WrapObject" by addressing it over the "refObject".
        /// </summary>
        /// <param name="refObject"></param>
        /// <returns>Returns a "WrapObject"</returns>
        ACPointNetWrapObject<T> GetWrapObject(T refObject);

        /// <summary>
        /// Returns a "refObject" by addressing it over a acURL.
        /// </summary>
        /// <param name="acURL"></param>
        /// <returns>Returns a "refObject"</returns>
        T GetRefObject(string acURL);

        /// <summary>
        /// Checks if a "refObject" exists in the List
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="refObject"></param>
        /// <returns></returns>
        bool Contains(T refObject);

        /// <summary>
        /// Removes a "refObject" from the list including it's wrapping "wrapObject" 
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="refObject"></param>
        /// <returns>returns true if object existed and was removed</returns>
        bool Remove(T refObject);

        /// <summary>
        /// Removes the "wrapObject" from the list including it's wrapped "refObject" 
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="acURL">Addressing over acURL</param>
        /// <returns>returns true if object existed and was removed</returns>
        bool Remove(string acURL);

        /// <summary>
        /// Adds a "wrapObject" which contains a "refObject" to the List
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="wrapObject"></param>
        bool Add(ACPointNetWrapObject<T> wrapObject);

        /// <summary>
        /// Adds a "refObject" to the List by creating a ACPointRefNetObject-Instance and wrapping the "refObject".
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="refObject"></param>
        /// <returns></returns>
        ACPointNetWrapObject<T> Add(T refObject);
        
        /// <summary>
        /// Appends a List of "refObjects" by automatically creating ACPointRefNetObject-Instances and wrapping the "refObjects".
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="refObjects"></param>
        void Add(IEnumerable<T> refObjects);

        /// <summary>
        /// Replaces the current List with the passed List of "refObjects" by automatically creating ACPointRefNetObject-Instances and wrapping the "refObjects".
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="refObjects"></param>
        void Set(IEnumerable<T> refObjects);
    }
}

