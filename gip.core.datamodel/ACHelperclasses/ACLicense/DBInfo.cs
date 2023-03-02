﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace gip.core.datamodel.Licensing
{
    public class DBInfo
    {
        public DBInfo(DateTime create_date, Guid service_broker_guid, Guid database_guid)
        {
            this.create_date = create_date;
            this.service_broker_guid = service_broker_guid;
            this.database_guid = database_guid;
        }
        
        public DateTime create_date
        {
            get;
            set;
        }

        public Guid service_broker_guid
        {
            get;
            set;
        }

        public Guid database_guid
        {
            get;
            set;
        }

        public byte[] GetCheckSum()
        {
            byte[] checkSum;
            using(MD5 md = MD5.Create())
            {
                checkSum = md.ComputeHash(Encoding.UTF8.GetBytes(service_broker_guid.ToString() + create_date.ToString("dd/MM/yyyy-hh/mm/ss/fff") + database_guid.ToString()));
            }
            return checkSum;
        }

        public string GetCheckSumCode()
        {
            string result = "-";
            byte[] checkSum = GetCheckSum();
            foreach(byte b in checkSum)
                result += b+"-";
            result = result.TrimStart('-').TrimEnd('-');
            return result;
        }
    }
}
