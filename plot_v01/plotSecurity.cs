using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace plot_v01
{
    public class plotSecurity : TableEntity 
    {
        public string IP { get; set; }
        public string ASHWID { get; set; }
        public string password { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string range { get; set; }
        public string ssid { get; set; }
        public int registeredDevice { get; set; }


        public plotSecurity()
        { }
        public plotSecurity(string teamname)
        {
            PartitionKey = "public";
            RowKey = teamname;
            latitude = longitude = range = ssid = password = "";
            registeredDevice = 0;
            IP = helper.getIP();
            ASHWID = helper.getASHWID();
        }
        public async Task setCurrentGeo()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            geolocator.DesiredAccuracyInMeters = 50;
            try {
                
                Geoposition geoposition = await geolocator.GetGeopositionAsync(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1));
                latitude = geoposition.Coordinate.Point.Position.Latitude.ToString("0.0000000000");
                longitude = geoposition.Coordinate.Point.Position.Longitude.ToString("0.0000000000");
            }
            catch
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1));
                latitude = geoposition.Coordinate.Point.Position.Latitude.ToString("0.0000000000");
                longitude = geoposition.Coordinate.Point.Position.Longitude.ToString("0.0000000000");
            }
        }
        public static string setCurrentNetwork()
        {
            var icp = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            if (icp != null)
            {
                if (icp.WlanConnectionProfileDetails != null)
                {
                    return icp.WlanConnectionProfileDetails.GetConnectedSsid();
                }
            }
            return null;
        }
        public void setGeo(string Latitude, string Longitude, string Range)
        {
            latitude = Latitude;
            longitude = Longitude;
            range = Range;
        }
        public void setPassword(string Password)
        {
            password = Password;
        }
        public void setSSID(string SSID)
        {
            ssid = SSID;
        }
        public void setRegisteredDevice()
        {
            registeredDevice = 1;
        }
        
        public async Task<bool> check(plotSecurity temp)
        {
            if((temp.latitude!="") && (temp.longitude!="") && (temp.range != ""))
            {
                await setCurrentGeo();
                if (!checkGeo(temp.latitude,temp.longitude,temp.range))
                    return false;
            }
            if (temp.ssid != "")
            {
                ssid = setCurrentNetwork();
                if (!checkNetwork(temp.ssid))
                    return false;
            }
            /*if (temp.password != "")
            {
                if (!checkPassword(temp.password))
                    return false;
            }*/
            if(registeredDevice != 0)
            {
                if (! await checkRegisteredDevice())
                    return false;
            }
            return true;
        }

        public bool checkNetwork(string SSID)
        {
            if(ssid.Equals(SSID))
                 return true; 
            return false;
        }

        public bool checkPassword(string pass)
        {
            if (password.Equals(pass))
                return true;
            return false;
        }

        public bool checkGeo(string latitude,string longitude,string range)
        {
            double R = 6371; // km
            double lat1, lat2, lon1, lon2,perimeter;
            Double.TryParse(latitude, out lat1);
            Double.TryParse(this.latitude, out lat2);
            Double.TryParse(longitude, out lon1);
            Double.TryParse(this.longitude, out lon2);
            Double.TryParse(range, out perimeter);
            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;


            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = R * c;
            if (d < perimeter)
                return true;
            else
                return false;
        }
        public async Task<bool> checkRegisteredDevice()
        {         
            if (helper.getASHWID().Equals( (await users.fetchUser(helper.getUsername())).ASHWID ))
                return true;
            return false;
        }


    }
}
