using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plot_v01
{
    public class accessKeys : TableEntity
    {
        public string host { get; set; }
        public string keys { get; set; }
        public string IP { get; set; }
        public string ASHWID { get; set; }
        public string dateTime { get; set; }

        public accessKeys() { }

        public accessKeys(string accessKey,string filename)
        {
            PartitionKey = helper.getUsername()+accessKey;
            RowKey = filename;
            keys = accessKey;
            host = helper.getUsername();
            IP = helper.getIP();
            ASHWID = helper.getASHWID();
            dateTime = DateTime.Now.ToString();
        }
        public string getFilename()
        {
            return RowKey;
        }
        public string getKeys()
        {
            return keys;
        }
    }
}
