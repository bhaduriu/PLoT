using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plot_v01
{
    public class blocked : TableEntity
    {
        public string IP { get; set; }
        public blocked()
        {
            this.PartitionKey = helper.getASHWID();
            this.RowKey = DateTime.Now.ToString();
            this.IP = helper.getIP();
        }
    }
}
