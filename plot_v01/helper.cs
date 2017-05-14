using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace plot_v01
{
    public class helper
    {
        public static bool checkInternetConnection() {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        private static bool dialog = false;
        public static async void popup(string msg,string title)
        {
            var messageDialog = new MessageDialog(msg);
            messageDialog.Title = title;

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers

            messageDialog.Commands.Add(new UICommand(
            "Close",
            new UICommandInvokedHandler(CommandInvokedHandler)));


            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();
        }
        private static void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Yes")
                dialog = true;
            else if (command.Label == "Yes,I Agree")
                dialog = true;
            else
                dialog = false;

        }
        public static string getASHWID()
        {

            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;
            var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

            byte[] bytes = new byte[hardwareId.Length];
            dataReader.ReadBytes(bytes);

            return BitConverter.ToString(bytes);
        }
        public static string getIP()
        {
            foreach (HostName localHostName in NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null)
                {
                    if (localHostName.Type == HostNameType.Ipv4)
                    {
                        return localHostName.DisplayName;
                    }
                }
            }
            return "";
        }
        public static string Calculatesize(double sizeInBytes)
        {
            const double terabyte = 1099511627776;
            const double gigabyte = 1073741824;
            const double megabyte = 1048576;
            const double kilobyte = 1024;

            string result;
            double theSize = 0;
            string units;

            if (sizeInBytes <= 0.1)
            {
                result = "0" + " " + "bytes";
                return result;
            }

            if (sizeInBytes >= terabyte)
            {
                theSize = sizeInBytes / terabyte;
                units = " TB";
            }
            else
            {
                if (sizeInBytes >= gigabyte)
                {
                    theSize = sizeInBytes / gigabyte;
                    units = " GB";
                }
                else
                {
                    if (sizeInBytes >= megabyte)
                    {
                        theSize = sizeInBytes / megabyte;
                        units = " MB";
                    }
                    else
                    {
                        if (sizeInBytes >= kilobyte)
                        {
                            theSize = sizeInBytes / kilobyte;
                            units = " KB";
                        }
                        else
                        {
                            theSize = sizeInBytes;
                            units = " bytes";
                        }
                    }
                }
            }

            if (units != "bytes")
            {
                result = theSize.ToString("0.00") + " " + units;
            }
            else
            {
                result = theSize.ToString("0.0") + " " + units;
            }
            return result;
        }
        public static void setUsername(string value)
        {
            var session = ApplicationData.Current.LocalSettings;
            session.Values["username"] = value;
        }
        public static string getUsername()
        {
            var session = ApplicationData.Current.LocalSettings;
            Object username = session.Values["username"];
            return username.ToString();
        }
        public static bool isSessionPresent()
        {
            var session = ApplicationData.Current.LocalSettings;
            Object username = session.Values["username"];
            if (username != null)
                return true;
            return false;
        }
        public static void setOnline()
        {
            var session = ApplicationData.Current.LocalSettings;
            session.Values["plots"] = "on";
            session.Values["linking"] = "on";
            session.Values["files"] = "on";
        }
        public static bool plots()
        {
            var session = ApplicationData.Current.LocalSettings;
            Object value = session.Values["plots"];
            if (value.ToString() == "on")
                return true;
            else
                return false;
        }
        public static bool linking()
        {
            var session = ApplicationData.Current.LocalSettings;
            Object value = session.Values["linking"];
            if (value.ToString() == "on")
                return true;
            else
                return false;
        }
        public static bool files()
        {
            var session = ApplicationData.Current.LocalSettings;
            Object value = session.Values["files"];
            if (value.ToString() == "on")
                return true;
            else
                return false;
        }
        public static void setLocal(string value)
        {
            var session = ApplicationData.Current.LocalSettings;
            session.Values[value] = "off";
        }
        public static void removeLocalSession()
        {
            var session = ApplicationData.Current.LocalSettings;
            session.Values.Remove("username");
        }
        public static bool validateEmail(string str)
        {

            return Regex.IsMatch(str, @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");     
        }
        public static void setCounter()
        {
            var session = ApplicationData.Current.LocalSettings;
            session.Values["blockCount"] = "0";

        }
        public static int getCounter()
        {
         
                int count = 0;
                var session = ApplicationData.Current.LocalSettings;
                Object key = session.Values["blockCount"];
                if (key != null)
                {
                    int.TryParse(key.ToString(), out count);
                    return count;
                }
           
                else
               {
                session.Values["blockCount"] = "0";
                return 0;
              }
            
        }
        public static void updateCounter()
        {
            int count = 0;
            var session = ApplicationData.Current.LocalSettings;
            Object key = session.Values["blockCount"];
            if(key != null)
                int.TryParse(key.ToString(), out count);

            count++;
            session.Values["blockCount"] = (count).ToString();
        }
        public static void setKey(string value)
        {
            var session = ApplicationData.Current.LocalSettings;
            session.Values["key"] = value;
        }
        public static string getKey()
        {
            var session = ApplicationData.Current.LocalSettings;
            Object key = session.Values["key"];
            return key.ToString();
        }
        public static void removeKey()
        {
            var session = ApplicationData.Current.LocalSettings;
            session.Values.Remove("key");
        }
        public static async Task<bool> renameDisplayPicture(string username)
        {
            StorageFolder media = await KnownFolders.PicturesLibrary.GetFolderAsync("PLoT");
            StorageFolder profile = await media.GetFolderAsync("profile");
            var pictureFile = await profile.GetFileAsync(helper.getUsername());
            await pictureFile.RenameAsync(username);
            return true;
        }
        public static async Task<BitmapImage> getProfileImageLocal(string username)
        {
            try {
                StorageFolder media = await KnownFolders.PicturesLibrary.GetFolderAsync("PLoT");
                StorageFolder profile = await media.GetFolderAsync("profile");
                BitmapImage img = new BitmapImage();
                var pictureFile = await profile.GetFileAsync(username);
                using (var pictureStream = await pictureFile.OpenAsync(FileAccessMode.Read))
                {
                    img.SetSource(pictureStream);
                }
                return img;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<BitmapImage> getTeamProfileImage(string teamname)
        {
            try
            {
                StorageFolder media = await KnownFolders.PicturesLibrary.GetFolderAsync("PLoT");
                StorageFolder profile = await media.GetFolderAsync("team");
                BitmapImage img = new BitmapImage();
                var pictureFile = await profile.GetFileAsync(teamname);
                using (var pictureStream = await pictureFile.OpenAsync(FileAccessMode.Read))
                {
                    img.SetSource(pictureStream);
                }
                return img;
            }
            catch
            {
                return null;
            }
        }

        public static async Task setProfileImageLocal(StorageFile file)
        {
                StorageFolder pictures = KnownFolders.PicturesLibrary;
                try
                {
                    await pictures.CreateFolderAsync("PLoT", CreationCollisionOption.FailIfExists);

                }
                catch { }
                StorageFolder localFolder = await pictures.GetFolderAsync("PLoT");
                try
                {
                    await localFolder.CreateFolderAsync("profile", CreationCollisionOption.FailIfExists);
                }
                catch { }
                StorageFolder media = await localFolder.GetFolderAsync("profile");
                StorageFile profile = await media.CreateFileAsync(getUsername(), CreationCollisionOption.ReplaceExisting);

            await file.CopyAndReplaceAsync(profile);
            
        }
        public static async Task setTeamImageLocal(StorageFile file,string teamName)
        {
            StorageFolder pictures = KnownFolders.PicturesLibrary;
            try
            {
                await pictures.CreateFolderAsync("PLoT", CreationCollisionOption.FailIfExists);

            }
            catch { }
            StorageFolder localFolder = await pictures.GetFolderAsync("PLoT");
            try
            {
                await localFolder.CreateFolderAsync("team", CreationCollisionOption.FailIfExists);
            }
            catch { }
            StorageFolder media = await localFolder.GetFolderAsync("team");
            StorageFile profile = await media.CreateFileAsync(teamName, CreationCollisionOption.ReplaceExisting);

            await file.CopyAndReplaceAsync(profile);

        }
        public static async Task<StorageFile> getDownloadProfileFile(string username)
        {
            StorageFolder pictures = KnownFolders.PicturesLibrary;
            try
            {
                await pictures.CreateFolderAsync("PLoT", CreationCollisionOption.FailIfExists);

            }
            catch { }
            StorageFolder localFolder = await pictures.GetFolderAsync("PLoT");
            try
            {
                await localFolder.CreateFolderAsync("profile", CreationCollisionOption.FailIfExists);
            }
            catch { }
            StorageFolder media = await localFolder.GetFolderAsync("profile");
            StorageFile profile = await media.CreateFileAsync(username, CreationCollisionOption.ReplaceExisting);
            return profile;
        }
        public static async Task<StorageFile> getDownloadTeamProfileFile(string teamname)
        {
            StorageFolder pictures = KnownFolders.PicturesLibrary;
            try
            {
                await pictures.CreateFolderAsync("PLoT", CreationCollisionOption.FailIfExists);

            }
            catch { }
            StorageFolder localFolder = await pictures.GetFolderAsync("PLoT");
            try
            {
                await localFolder.CreateFolderAsync("team", CreationCollisionOption.FailIfExists);
            }
            catch { }
            StorageFolder media = await localFolder.GetFolderAsync("team");

            StorageFile profile = await media.CreateFileAsync(teamname, CreationCollisionOption.ReplaceExisting);
            return profile;
        }

        public static async Task<bool> addFileDataLocal(files temp)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("files.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.CreateFileAsync("files.ini", CreationCollisionOption.OpenIfExists);
            }
            string jsonContents = JsonConvert.SerializeObject(temp);
            await FileIO.AppendTextAsync(textFile, jsonContents + "\n");
            return true;
        }
        public static async Task<bool> refreshFilesLocal(List<files> list)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("files.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.CreateFileAsync("files.ini", CreationCollisionOption.OpenIfExists);
            }
            await FileIO.WriteTextAsync(textFile,"");
            int count = 0;
            foreach (files item in list)
            {
                string jsonContents = JsonConvert.SerializeObject(item);
                if (count == 0)
                    await FileIO.WriteTextAsync(textFile, jsonContents + "\n");
                else
                    await FileIO.AppendTextAsync(textFile, jsonContents + "\n");
                count++;
            }

            return true;
        }
        public static async Task<List<files>> retriveFileDataLocal()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("files.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.CreateFileAsync("files.ini", CreationCollisionOption.OpenIfExists);
            }
            List<files> list = new List<files>();
            string text = await FileIO.ReadTextAsync(textFile);
            string[] templist = text.Split('\n');
            foreach (string temp in templist)
            {
                if (temp != "")
                    list.Add(JsonConvert.DeserializeObject<files>(temp));
            }
            return list;
        }
        public static async Task<files> retriveFileDataLocal(string filename)
        {
            files final = new files();
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("files.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.CreateFileAsync("files.ini", CreationCollisionOption.OpenIfExists);
            }
            string text = await FileIO.ReadTextAsync(textFile);
            string[] templist = text.Split('\n');
            foreach (string temp in templist)
            {
                if (temp != "")
                {
                    if ((JsonConvert.DeserializeObject<files>(temp)).getFilename() == filename)
                        final = JsonConvert.DeserializeObject<files>(temp);
                        }

            }
            return final;
        }
        public static async Task<bool> deleteFileDataLocal(string filename)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("files.ini", CreationCollisionOption.OpenIfExists);
            }
            catch
            {
                return false;
            }
            List<files> list = await retriveFileDataLocal();
            string text = "";
            foreach (files temp in list)
            {
                if (temp.getFilename() != filename)
                {
                    string jsonContents = JsonConvert.SerializeObject(temp);
                    text += jsonContents + "\n";
                }

            }
            await FileIO.WriteTextAsync(textFile, text);
            return true;
        }

        public static async Task<bool> addPlotDataLocal(plots temp)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("plots.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.CreateFileAsync("plots.ini", CreationCollisionOption.OpenIfExists);
            }
            string jsonContents = JsonConvert.SerializeObject(temp);
            await FileIO.AppendTextAsync(textFile, jsonContents + "\n");
            return true;
        }
        public static async Task<List<plots>> retrivePlotDataLocal()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("plots.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.CreateFileAsync("plots.ini", CreationCollisionOption.OpenIfExists);
            }
            List<plots> list = new List<plots>();
            string text = await FileIO.ReadTextAsync(textFile);
            string[] templist = text.Split('\n');
            foreach (string temp in templist)
            {
                if (temp != "")
                    list.Add(JsonConvert.DeserializeObject<plots>(temp));
            }
            return list;
        }
        public static async Task<List<plots>> retrivePlotDataLocal(string username)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("plots.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.CreateFileAsync("plots.ini", CreationCollisionOption.OpenIfExists);
            }
            List<plots> list = new List<plots>();
            string text = await FileIO.ReadTextAsync(textFile);
            string[] templist = text.Split('\n');
            foreach (string temp in templist)
            {
                if (temp != "")
                    list.Add(JsonConvert.DeserializeObject<plots>(temp));
            }
            List<plots> finalList = new List<plots>();
            foreach (plots temp in list)
            {
                if (temp.getUsername() == username)
                {
                    BitmapImage img = await helper.getTeamProfileImage(temp.getTeamName());
                    if (img != null)
                        temp.setImage(img);
                    else {
                        await users.getTeamDisplayPicture(temp.getTeamName());
                        img = await getTeamProfileImage(temp.getTeamName());
                        temp.setImage(img);
                    }
                    finalList.Add(temp);
                }

            }
            return finalList;
        }
        public static async Task<List<plots>> retrivePlotTeamDataLocal(string teamName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("plots.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.CreateFileAsync("plots.ini", CreationCollisionOption.OpenIfExists);
            }
            List<plots> list = new List<plots>();
            string text = await FileIO.ReadTextAsync(textFile);
            string[] templist = text.Split('\n');
            foreach (string temp in templist)
            {
                if (temp != "")
                    list.Add(JsonConvert.DeserializeObject<plots>(temp));
            }
            List<plots> finalList = new List<plots>();
            foreach (plots temp in list)
            {
                if (temp.getTeamName() == teamName)
                {
                    BitmapImage img = await helper.getTeamProfileImage(temp.getTeamName());
                    if (img != null)
                        temp.setImage(img);
                    else {
                        await users.getTeamDisplayPicture(temp.getTeamName());
                        img = await getTeamProfileImage(temp.getTeamName());
                        temp.setImage(img);
                    }
                    finalList.Add(temp);
                }
                    
            }
            return finalList;
        }
        public static async Task<bool> refreshPlotLocal(List<plots> list)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("plots.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.GetFileAsync("plots.ini");
            }
            await FileIO.WriteTextAsync(textFile, "");
            int count = 0;
            foreach(plots item in list)
            {
                string jsonContents = JsonConvert.SerializeObject(item);
                if (count==0)
                    await FileIO.WriteTextAsync(textFile, jsonContents + "\n");
                else
                    await FileIO.AppendTextAsync(textFile, jsonContents + "\n");
                count++;
            }          
            
            return true;
        }
        public static async Task<bool> deletePlotDataLocal(string teamName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("plots.ini", CreationCollisionOption.OpenIfExists);
            }
            catch
            {
                return false;
            }
            List<plots> list = await retrivePlotDataLocal();
            string text = "";
            foreach (plots temp in list)
            {
                if (temp.getTeamName() != teamName)
                {
                    string jsonContents = JsonConvert.SerializeObject(temp);
                    text += jsonContents + "\n";
                }

            }
            await FileIO.WriteTextAsync(textFile, text);
            return true;
        }

        public static async Task<bool> addLinkDataLocal(linking temp)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("linking.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.CreateFileAsync("linking.ini", CreationCollisionOption.OpenIfExists);
            }
            string jsonContents = JsonConvert.SerializeObject(temp);
            await FileIO.AppendTextAsync(textFile, jsonContents + "\n");
            return true;
        }

        public static async Task<List<linking>> retriveLinkTeamDataLocal(string teamName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("linking.ini", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                textFile = await folder.CreateFileAsync("linking.ini", CreationCollisionOption.OpenIfExists);
            }
            List<linking> list = new List<linking>();
            string text = await FileIO.ReadTextAsync(textFile);
            string[] templist = text.Split('\n');
            foreach (string temp in templist)
            {
                if (temp != "")
                    list.Add(JsonConvert.DeserializeObject<linking>(temp));
            }
            List<linking> finalList = new List<linking>();
            foreach (linking temp in list)
            {
                if (temp.getTeamName() == teamName)
                    finalList.Add(temp);
            }
            return finalList;
        }
        public static async Task<bool> deleteLinkLocal(string teamName,string filename)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile textFile;
            try
            {
                textFile = await folder.CreateFileAsync("linking.ini", CreationCollisionOption.OpenIfExists);
            }
            catch
            {
                return false;
            }
            List<linking> list = await retriveLinkTeamDataLocal(teamName);
            string text = "";
            foreach (linking temp in list)
            {
                if (temp.getTeamName() != teamName && temp.getFilename() != filename)
                {
                    string jsonContents = JsonConvert.SerializeObject(temp);
                    text += jsonContents + "\n";
                }

            }
            await FileIO.WriteTextAsync(textFile, text);
            return true;
        }

        public static async Task<List<files>> retriveTeamFiles(string teamName)
        {
            List<linking> list = await retriveLinkTeamDataLocal(teamName);
            List<files> finalList = new List<files>();
            foreach(linking link in list)
            {
                finalList.Add(await retriveFileDataLocal(link.getFilename()));
            }
            return finalList;
        }

        public static async Task<List<users>> retriveTeamMember(string teamName)
        {
            List<plots> list = await retrivePlotTeamDataLocal(teamName);
            List<users> finalList = new List<users>();
            foreach (plots plot in list)
            {
                finalList.Add(new users(plot.getUsername()));
            }
            return finalList;
        }

        public static async Task<bool> dialogPopup(string msg, string title)
        {
            var messageDialog = new MessageDialog(msg);
            messageDialog.Title = title;

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers

            messageDialog.Commands.Add(new UICommand(
            "Yes",
            new UICommandInvokedHandler(CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand(
           "No",
           new UICommandInvokedHandler(CommandInvokedHandler)));


            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();
            return dialog;
        }
        public static async Task<bool> privacyPopup(string msg, string title)
        {
            var messageDialog = new MessageDialog(msg);
            messageDialog.Title = title;

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers

            messageDialog.Commands.Add(new UICommand(
            "Yes,I Agree",
            new UICommandInvokedHandler(CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand(
           "No, I don't",
           new UICommandInvokedHandler(CommandInvokedHandler)));


            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();
            return dialog;
        }

        public static async Task<IBuffer> SampleDataProtectionStream(String descriptor, IBuffer buffMsg)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider Provider = new DataProtectionProvider(descriptor);

            // Create a random access stream to contain the plaintext message.
            InMemoryRandomAccessStream inputData = new InMemoryRandomAccessStream();

            // Create a random access stream to contain the encrypted message.
            InMemoryRandomAccessStream protectedData = new InMemoryRandomAccessStream();

            // Retrieve an IOutputStream object and fill it with the input (plaintext) data.
            IOutputStream outputStream = inputData.GetOutputStreamAt(0);
            DataWriter writer = new DataWriter(outputStream);
            writer.WriteBuffer(buffMsg);
            await writer.StoreAsync();
            await outputStream.FlushAsync();

            // Retrieve an IInputStream object from which you can read the input data.
            IInputStream source = inputData.GetInputStreamAt(0);

            // Retrieve an IOutputStream object and fill it with encrypted data.
            IOutputStream dest = protectedData.GetOutputStreamAt(0);
            await Provider.ProtectStreamAsync(source, dest);
            await dest.FlushAsync();

            //Verify that the protected data does not match the original
            DataReader reader1 = new DataReader(inputData.GetInputStreamAt(0));
            DataReader reader2 = new DataReader(protectedData.GetInputStreamAt(0));
            await reader1.LoadAsync((uint)inputData.Size);
            await reader2.LoadAsync((uint)protectedData.Size);
            IBuffer buffOriginalData = reader1.ReadBuffer((uint)inputData.Size);
            IBuffer buffProtectedData = reader2.ReadBuffer((uint)protectedData.Size);

            if (CryptographicBuffer.Compare(buffOriginalData, buffProtectedData))
            {
                throw new Exception("ProtectStreamAsync returned unprotected data");
            }

            // Return the encrypted data.
            return buffProtectedData;
        }

        public static async Task<StorageFile> Encrpt(StorageFile encrpt)
        {
            IBuffer data = await FileIO.ReadBufferAsync(encrpt);
            IBuffer SecuredData = await SampleDataProtectionStream("LOCAL = user", data);
            StorageFile EncryptedFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
              encrpt.DisplayName + encrpt.FileType, CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteBufferAsync(EncryptedFile, SecuredData);
            return EncryptedFile;
        }
        public static async Task<StorageFile> Decrypt(StorageFile decrpt)
        {

            IBuffer data = await FileIO.ReadBufferAsync(decrpt);
            IBuffer UnSecuredData = await SampleDataUnprotectStream(data);

            StorageFile DecryptedFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
              decrpt.DisplayName + decrpt.FileType, CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteBufferAsync(DecryptedFile, UnSecuredData);
            return DecryptedFile;
        }


        public static async Task<IBuffer> SampleDataUnprotectStream(IBuffer buffProtected)
        {
            // Create a DataProtectionProvider object.
            DataProtectionProvider Provider = new DataProtectionProvider();

            // Create a random access stream to contain the encrypted message.
            InMemoryRandomAccessStream inputData = new InMemoryRandomAccessStream();

            // Create a random access stream to contain the decrypted data.
            InMemoryRandomAccessStream unprotectedData = new InMemoryRandomAccessStream();

            // Retrieve an IOutputStream object and fill it with the input (encrypted) data.
            IOutputStream outputStream = inputData.GetOutputStreamAt(0);
            DataWriter writer = new DataWriter(outputStream);
            writer.WriteBuffer(buffProtected);
            await writer.StoreAsync();
            await outputStream.FlushAsync();

            // Retrieve an IInputStream object from which you can read the input (encrypted) data.
            IInputStream source = inputData.GetInputStreamAt(0);

            // Retrieve an IOutputStream object and fill it with decrypted data.
            IOutputStream dest = unprotectedData.GetOutputStreamAt(0);
            await Provider.UnprotectStreamAsync(source, dest);
            await dest.FlushAsync();

            // Write the decrypted data to an IBuffer object.
            DataReader reader2 = new DataReader(unprotectedData.GetInputStreamAt(0));
            await reader2.LoadAsync((uint)unprotectedData.Size);
            IBuffer buffUnprotectedData = reader2.ReadBuffer((uint)unprotectedData.Size);

            // Return the decrypted data.
            return buffUnprotectedData;
        }

    }

}



