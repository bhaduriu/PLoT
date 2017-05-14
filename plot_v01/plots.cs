using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace plot_v01
{
    public class plots : TableEntity
    {
        public BitmapImage image { get; set; }
        public string IP { get; set; }
        public string ASHWID { get; set; }
        public string dateTime { get; set; }
        public string access { get; set; }

        public plots()
        {
            RowKey = PartitionKey ="";
            access = "member";
            dateTime = DateTime.Now.ToString();
            IP = helper.getIP();
            ASHWID = helper.getASHWID();
        }
        public plots(string teamName,string username,string Access)
        {
            PartitionKey = teamName;
            RowKey = username;
            access = Access;
            dateTime = DateTime.Now.ToString();
            IP = helper.getIP();
            ASHWID = helper.getASHWID();
        }

        public void setImage(BitmapImage img)
        {
            image = img;
        }

        public string getTeamName()
        {
            return PartitionKey;
        }
        public string getUsername()
        {
            return RowKey;
        }
        public string getAccess()
        {
            return access;
        }
        
    }
}
