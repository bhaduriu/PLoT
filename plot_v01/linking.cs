using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plot_v01
{
    public class linking : TableEntity
    {
        public string IP { get; set; }
        public string ASHWID { get; set; }
        public string host { get; set; }
        public string dateTime { get; set; }
        public string fileSize { get; set; }

        public linking()
        {
            host = RowKey = PartitionKey = "";
            dateTime = "";
            ASHWID = helper.getASHWID();
            IP = helper.getIP();
        }
        public linking(string Host, string Filename, string teamname)
        {
            PartitionKey = teamname;
            RowKey = Filename;
            host = Host;
            fileSize = "0";
            dateTime = DateTime.Now.ToString();
            ASHWID = helper.getASHWID();
            IP = helper.getIP();

        }
        public linking(string Host , string Filename,string teamname,string FileSize)
        {
            PartitionKey = teamname;
            RowKey =  Filename;
            host = Host;
            fileSize = FileSize;
            dateTime = DateTime.Now.ToString();
            ASHWID = helper.getASHWID();
            IP = helper.getIP();

        }

        public string getHost()
        {
            return host;
        }
        public string getFilename()
        {
            return RowKey;
        }
        public string getTeamName()
        {
            return PartitionKey;
        }
    }
}
