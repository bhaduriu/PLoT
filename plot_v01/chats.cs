using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plot_v01
{
    public class chats : TableEntity 
    {
        public string msg { get; set; }
        public string dateTime { get; set; }
        public string sender { get; set; }
        public string IP { get; set; }
        public string ASHWID { get; set; }
        public string align { get; set; }
        public string background { get; set; }
        public string textColor { get; set; }
        public string hintColor { get; set; }
        public chats() { }

        public chats(string teamname,string MSG)
        {
            PartitionKey = teamname;
            RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d20");
            dateTime = DateTime.Now.ToString();
            sender = helper.getUsername();
            msg = MSG;
            IP = helper.getIP();
            ASHWID = helper.getASHWID();
        }

        public chats(string teamname,string username, string MSG)
        {
            PartitionKey = teamname;
            RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d20");
            dateTime = DateTime.Now.ToString();
            sender = username;
            msg = MSG;
            IP = helper.getIP();
            ASHWID = helper.getASHWID();
        }
        
        public string getSender()
        {
            return sender;
        }

        public void setAlign()
        {
            if (getSender() == helper.getUsername())
            {
                align = "Right";
                background = "Gray";
                textColor = "White";
                hintColor = "Black";
            }

            else {
                align = "Left";
                background = "#81d4cd";
                textColor = "White";
                hintColor = "Gray";

            }
                
        }

    }
}
