﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using System.Threading;

namespace gip.tool.installerAndUpdater
{
    public class VBZipFile : ZipFile
    {
        public VBZipFile(string fileName, CancellationToken cancelToken) : base(fileName)
        {
            CancellationToken = cancelToken;
        }

        public CancellationToken CancellationToken
        {
            get;
            set;
        }
    }
}
