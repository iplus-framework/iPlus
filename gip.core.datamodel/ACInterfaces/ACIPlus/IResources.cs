// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IResources.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface IResources
    /// </summary>
    public interface IResources 
    {
        ACBackgroundWorker Worker { get; set; }
        IVBProgress VBProgress { get; set; }

        #region Filesystem
        /// <summary>
        /// Dir
        /// </summary>
        /// <param name="db"></param>
        /// <param name="container"></param>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        /// <param name="withFiles"></param>
        /// <returns></returns>
        ACFSItem Dir(IACEntityObjectContext db, ACFSItemContainer container, string path, bool recursive, bool withFiles = true);

        /// <summary>
        /// Reads the text.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>System.String.</returns>
        string ReadText(string filename);

        string ReadTextEncoding(string filename, Encoding encoding);

        string[] ReadLinesEncoding(string filename, Encoding encoding);

        /// <summary>
        /// Writes the text.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool WriteText(string filename, string text);
       
        /// <summary>
        /// Reads the binary.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>Byte[][].</returns>
        Byte[] ReadBinary(string filename);

        /// <summary>
        /// Writes the binary.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool WriteBinary(string filename, Byte[] text);

        /// <summary>
        /// Creates the dir.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool CreateDir(string path);

        /// <summary>
        /// Prüft oder erstellt das Verzeichnis für den überghebenen Dateinamen
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool CheckOrCreateDir(string filename);

        /// <summary>
        /// Deletes the dir.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool DeleteDir(string path, bool recursive = false);

        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="sourceFilename">The source filename.</param>
        /// <param name="destFilename">The dest filename.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool CopyFile(string sourceFilename, string destFilename, bool overwrite = false);

        /// <summary>
        /// Moves the file.
        /// </summary>
        /// <param name="sourceFilename">The source filename.</param>
        /// <param name="destFilename">The dest filename.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool MoveFile(string sourceFilename, string destFilename);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool DeleteFile(string filename);
        #endregion
        
    }
}
