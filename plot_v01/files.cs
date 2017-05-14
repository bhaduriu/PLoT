using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace plot_v01
{
    public class files : TableEntity 
    {
        public string dateTime { get; set; }
        public string extension { get; set; }
        public string size { get; set; }
        public string IP { get; set; }
        public string ASHWID { get; set; }
        public files() {
            PartitionKey = helper.getUsername();
            RowKey = "";
            dateTime = DateTime.Now.ToString();
            extension = "";
            size = "";
            IP = helper.getIP();
            ASHWID = helper.getASHWID();
        }
        public files(string Filename,string Extension,string Size) {
            PartitionKey = helper.getUsername();
            RowKey = Filename;
            dateTime = DateTime.Now.ToString();
            Extension.Replace(".", "");
            extension = "Assets/" + Extension + ".png";
            size = Size;
            IP = helper.getIP();
            ASHWID = helper.getASHWID();
        }
        public files(string Filename,string Extension,string Size,string date)
        {

            PartitionKey = helper.getUsername();
            RowKey = Filename;
            dateTime = date;            
            extension = "Assets/"+ Extension + ".png"; 
            size = Size;
            IP = helper.getIP();
            ASHWID = helper.getASHWID();
        }
        public files(string username,string Filename, string Extension, string Size, string date)
        {

            PartitionKey = username;
            RowKey = Filename;
            dateTime = date;
            extension = "Assets/" + Extension + ".png";
            size = Size;
            IP = helper.getIP();
            ASHWID = helper.getASHWID();
        }
        public string getFilename() {
            return this.RowKey;
        }

        public string getHost()
        {
            return PartitionKey;
        }
        public string getSize()
        {
            return size;
        }

        public bool IsImage()
        {
            if(extension == "Assets/image.png")
              return true;
            return false;
        }
    }
}
