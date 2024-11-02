// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using System.Data.Objects;

namespace gip.core.autocomponent
{
    public class ACEntityOpQueue : ACDelegateQueue
    {
        public ACEntityOpQueue(ObjectContext database)
        {
            _Context = database;
        }

        public ACEntityOpQueue(ObjectContext database, int workerInterval_ms)
            : base(workerInterval_ms)
        {
            _Context = database;
        }

        private ObjectContext _Context;
        public ObjectContext Context
        {
            get
            {
                
                return _Context;
            }
        }

        protected override bool OnStartQueueProcessing(int countActions)
        {
            lock (Context)
            {
                try
                {
                    Context.Connection.Open();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        public override void ProcessAction(Action action)
        {
            lock (Context)
            {
                action();
            }
        }


        protected override void OnQueueProcessed(int countActions)
        {
            lock (Context)
            {
                try
                {
                    if (Context.HasModifiedObjectStateEntries())
                    {
                        Context.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    Context.ACUndoChanges();
                }
                finally
                {
                    try
                    {
                        Context.Connection.Close();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
