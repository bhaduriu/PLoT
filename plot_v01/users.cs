using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.System.Profile;
using Windows.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Json;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;

namespace plot_v01
{
    public class users : TableEntity
    {
        public string EMAIL { get; set; }
        public string PASSWORD { get; set; }
        public string IP { get; set; }
        public string ASHWID { get; set; }
        public DateTime dateTime { get; set; }
        public int fileCount { get; set; }
        public double fileSize { get; set; }

        public int plotCount { get; set; }

        public BitmapImage image { get; set; }

        static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());

        static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        static CloudTable userTable = tableClient.GetTableReference("users");
        static CloudTable filesTable = tableClient.GetTableReference("files");
        static CloudTable plotsTable = tableClient.GetTableReference("plots");
        static CloudTable linkingTable = tableClient.GetTableReference("linking");
        static CloudTable plotSecurityTable = tableClient.GetTableReference("plotSecurity");
        static CloudTable FileSecurityTable = tableClient.GetTableReference("fileSecurity");
        static CloudTable inviteTable = tableClient.GetTableReference("invite");
        static CloudTable chatsTable = tableClient.GetTableReference("chats");
        static CloudTable accessKeysTable = tableClient.GetTableReference("accessKeys");
        public users()
        {
            EMAIL = PASSWORD = "";
            IP = helper.getIP();
            dateTime = DateTime.UtcNow;
            ASHWID = helper.getASHWID();
            PartitionKey = "public";
            RowKey = "";
            fileCount = 0;
            fileSize = 0;
            plotCount = 0;
        }
        public users(string username)
        {
            EMAIL = "";
            PASSWORD = "";
            IP = helper.getIP();
            dateTime = DateTime.UtcNow;
            ASHWID = helper.getASHWID();
            PartitionKey = "public";
            RowKey = username;
            fileCount = 0;
            fileSize = 0;
            plotCount = 0;
        }
        public users(string username, string password)
        {
            EMAIL = "";
            PASSWORD = password;
            IP = helper.getIP();
            dateTime = DateTime.UtcNow;
            ASHWID = helper.getASHWID();
            PartitionKey = "public";
            RowKey = username;
            fileCount = 0;
            fileSize = 0;
        }
        public users(string username, string password, string email)
        {
            EMAIL = email;
            PASSWORD = password;
            IP = helper.getIP();
            dateTime = DateTime.UtcNow;
            ASHWID = helper.getASHWID();
            PartitionKey = "public";
            RowKey = username;
        }

        public string getUsername()
        {
            return RowKey;
        }
        public async Task<ImageSource> getUserProfile()
        {
            try
            {
                StorageFolder media = await KnownFolders.PicturesLibrary.GetFolderAsync("PLoT");
                BitmapImage img = new BitmapImage();
                var pictureFile = await media.GetFileAsync(this.RowKey);
                using (var pictureStream = await pictureFile.OpenAsync(FileAccessMode.Read))
                {
                    img.SetSource(pictureStream);
                }
                return img;
            }
            catch { }
            return null;
        }

        public void setImage(BitmapImage img)
        {
            image = img;
        }
        public void incrementFileCount()
        {
           fileCount++;
        }
        public void decrementFileCount()
        {
            fileCount--;
            if (fileCount < 0)
                fileCount = 0;
        }
        public void incrementPlotCount()
        {
            plotCount++;
        }
        public void decrementPlotCount()
        {
            plotCount--;
            if (plotCount < 0)
                plotCount = 0;
        }

        public void addFileSize(double size)
        {
            fileSize += size;
        }
        public void reduceFileSize(double size)
        {
            fileSize -= size;
            if (fileSize < 0)
                fileSize = 0;
        }

        public static async Task<double> getSizeUsed(string username)
        {
            users temp = await fetchUser(username);
            return temp.fileSize;
        }
        public async Task<bool> register()
        {
            try
            {
                TableOperation insertOperation = TableOperation.Insert(this);
                await userTable.ExecuteAsync(insertOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> addTeam(string teamName, string username, string access)
        {
            try
            {
                plots temp = new plots(teamName, username, access);
                TableOperation insertOperation = TableOperation.Insert(temp);
                await plotsTable.ExecuteAsync(insertOperation);
                await updatePlotSecurity(new plotSecurity(teamName));
                users user = await fetchUser(helper.getUsername());
                user.incrementPlotCount();
                await replaceUser(user);
                await helper.addPlotDataLocal(temp);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> addInvite(string teamName, string username, string access)
        {
            try
            {
                plots temp = new plots(teamName, username, access);
                TableOperation insertOperation = TableOperation.Insert(temp);
                await inviteTable.ExecuteAsync(insertOperation);
                await updatePlotSecurity(new plotSecurity(teamName));
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<List<plots>> getAllInvites()
        {
            TableContinuationToken token = null;
            List<plots> list = new List<plots>();
            List<plots> finalList = new List<plots>();
            try
            {
                TableQuery<plots> rangeQuery = new TableQuery<plots>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, helper.getUsername()));
                var queryResult = await inviteTable.ExecuteQuerySegmentedAsync<plots>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                foreach (plots temp in list)
                {
                    BitmapImage img = await helper.getTeamProfileImage(temp.getTeamName());
                    if (img != null)
                        temp.setImage(img);
                    else {
                        await getTeamDisplayPicture(temp.getTeamName());
                        img = await helper.getTeamProfileImage(temp.getTeamName());
                        if (img != null)
                            temp.setImage(img);
                        else
                            temp.setImage(new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/profile.png", UriKind.Absolute) });
                    }
                    finalList.Add(temp);
                }
                return finalList;
            }
            catch
            {
                return null;
            }

        }
        public static async Task<bool> deleteInvite(plots invite)
        {
            try
            {
                TableOperation deleteOperation = TableOperation.Delete(invite);
                TableResult finalResult = await inviteTable.ExecuteAsync(deleteOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> deleteTeam(plots temp)
        {
            try
            {
                List<plots> list = await helper.retrivePlotTeamDataLocal(temp.getTeamName());
                List<linking> linkingList = await helper.retriveLinkTeamDataLocal(temp.getTeamName());
                plotSecurity security = await fetchPlotSecurity(temp.getTeamName());           
                foreach (plots data in list)
                {
                    TableOperation deletePlotOperation = TableOperation.Delete(data);
                    TableResult finalPlotResult = await plotsTable.ExecuteAsync(deletePlotOperation);
                }
                foreach (linking link in linkingList)
                {
                    TableOperation deleteLinkOperation = TableOperation.Delete(link);
                    TableResult finalLinkResult = await linkingTable.ExecuteAsync(deleteLinkOperation);
                }
                TableOperation deleteSecurityOperation = TableOperation.Delete(security);
                TableResult finalSecurityResult = await plotSecurityTable.ExecuteAsync(deleteSecurityOperation);
                await helper.deletePlotDataLocal(temp.getTeamName());
                StorageFile file = await helper.getDownloadTeamProfileFile(temp.getTeamName());
                await file.DeleteAsync();
                await deleteTeamProfile(temp.getTeamName());
                users user = await fetchUser(helper.getUsername());
                user.decrementPlotCount();
                await replaceUser(user);
                return true;
            }
            catch
            {
                return false;
            }

        }
        public static async Task<bool> deleteTeamMember(string teamname, string username)
        {
            try
            {
                plots data = await fetchPlots(teamname, username);
                TableOperation deleteOperation = TableOperation.Delete(data);
                TableResult finalResult = await plotsTable.ExecuteAsync(deleteOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> addFileOnline(files file)
        {
            try
            {
                TableOperation insertOperation = TableOperation.Insert(file);
                await filesTable.ExecuteAsync(insertOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> deleteFileOnline(files file)
        {
            try
            {
                TableOperation deleteOperation = TableOperation.Delete(file);
                TableResult finalResult = await filesTable.ExecuteAsync(deleteOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> addAccessKey(string key, string filename)
        {
            try
            {

                    accessKeys temp = new accessKeys(key, filename);
                    TableOperation insertOperation = TableOperation.Insert(temp);
                    await accessKeysTable.ExecuteAsync(insertOperation);
                    return true;

               
            }
            catch
            {
               
            }
            return false;
        }
       
        public static async Task<bool> deleteKey(accessKeys key)
        {
            try
            {
                TableOperation deleteOperation = TableOperation.Delete(key);
                TableResult finalResult = await accessKeysTable.ExecuteAsync(deleteOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<List<plots>> refreshPlotData()
        {
            TableContinuationToken token = null;
            List<plots> list = new List<plots>();
            List<plots> finalList = new List<plots>();
            try
            {
                TableQuery<plots> rangeQuery = new TableQuery<plots>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, helper.getUsername()));
                var queryResult = await plotsTable.ExecuteQuerySegmentedAsync<plots>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                foreach (plots temp in list)
                {
                    BitmapImage img = await helper.getTeamProfileImage(temp.getTeamName());
                    if (img != null)
                        temp.setImage(img);
                    else {
                        await getTeamDisplayPicture(temp.getTeamName());
                        img = await helper.getTeamProfileImage(temp.getTeamName());
                        if (img != null)
                            temp.setImage(img);
                        else
                            temp.setImage(new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/profile.png", UriKind.Absolute) });
                    }
                    finalList.Add(temp);
                }

                await helper.refreshPlotLocal(list);
                return finalList;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<List<files>> refreshFilesData()
        {
            TableContinuationToken token = null;
            List<files> list = new List<files>();
            try
            {
                TableQuery<files> rangeQuery = new TableQuery<files>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, helper.getUsername()));
                var queryResult = await filesTable.ExecuteQuerySegmentedAsync<files>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                await helper.refreshFilesLocal(list);
                return list;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<bool> checkASHWID(string ashwid)
        {
            TableContinuationToken token = null;
            List<users> list = new List<users>();
            try
            {
                TableQuery<users> rangeQuery = new TableQuery<users>().Where(TableQuery.GenerateFilterCondition("ASHWID", QueryComparisons.Equal, ashwid));
                var queryResult = await userTable.ExecuteQuerySegmentedAsync<users>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                if (list.Count == 0)
                    return false;
                return true;
            }
            catch { }
            return false;
        }
        public static async Task<List<users>> fetchSpecificRangeUser(string username, string teamname,int choice)
        {
            TableContinuationToken token = null;
            List<users> list = new List<users>();
            List<users> finalList = new List<users>();
            try
            {
                
                
                    TableQuery<users> rangeQuery1 = new TableQuery<users>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "public"));
                    var queryResult1 = await userTable.ExecuteQuerySegmentedAsync<users>(rangeQuery1, token);
                    var entities1 = queryResult1.Results.ToList();
                    token = queryResult1.ContinuationToken;
                    list.AddRange(entities1);

                finalList = list;
                
                /*
                List<users> members = await helper.retriveTeamMember(teamname);
                bool isTeamMember = false;
                foreach (users member in list)
                {
                    foreach (users temp in members)
                    {
                        if (temp.getUsername() == member.getUsername())
                            isTeamMember = true;
                    }
                    if ((member.getUsername() != helper.getUsername()) && (isTeamMember))
                    {
                        try
                        {

                            BitmapImage img = await helper.getProfileImageLocal(member.getUsername());
                            if (img != null)
                                member.setImage(img);
                            else
                            {
                                await getDisplayPicture(member.getUsername());
                                img = await helper.getProfileImageLocal(member.getUsername());

                                if (img == null)
                                    img = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/profile.png", UriKind.Absolute) };

                                member.setImage(img);
                            }
                        }
                        catch { }
                        finalList.Add(member);
                    }
                }
                */
                return finalList;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<linking> fetchLink(string teamname, string filename)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<linking>(teamname, filename);
                TableResult result = await linkingTable.ExecuteAsync(retrieveOperation);
                return (linking)result.Result;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<List<linking>> fetchLink(string teamname)
        {
            TableContinuationToken token = null;
            List<linking> list = new List<linking>();
            try
            {
                TableQuery<linking> rangeQuery = new TableQuery<linking>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, teamname));
                var queryResult = await linkingTable.ExecuteQuerySegmentedAsync<linking>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                if (list.Count == 0)
                    return null;
                return list;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<files> fetchFile(string username, string filename)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<files>(username, filename);
                TableResult result = await filesTable.ExecuteAsync(retrieveOperation);
                return (files)result.Result;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<users> fetchUser(string username)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<users>("public", username);
                TableResult result = await userTable.ExecuteAsync(retrieveOperation);
                return ((users)result.Result);
            }
            catch
            {
                return null;
            }
        }
        public static async Task<plots> fetchPlots(string teamname, string username)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<plots>(teamname, username);
                TableResult result = await plotsTable.ExecuteAsync(retrieveOperation);
                return ((plots)result.Result);
            }
            catch
            {
                return null;
            }
        }
        public static async Task<plotSecurity> fetchPlotSecurity(string teamname)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<plotSecurity>("public", teamname);
                TableResult result = await plotSecurityTable.ExecuteAsync(retrieveOperation);
                return ((plotSecurity)result.Result);
            }
            catch
            {
                return null;
            }
        }
        public static async Task<plotSecurity> fetchFileSecurity(string accessKey)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<plotSecurity>("public", accessKey);
                TableResult result = await FileSecurityTable.ExecuteAsync(retrieveOperation);
                return ((plotSecurity)result.Result);
            }
            catch
            {
                return null;
            }
        }
        public static async Task<List<accessKeys>> fetchAccessKeys()
        {
            TableContinuationToken token = null;
            List<accessKeys> list = new List<accessKeys>();
            try
            {
                TableQuery<accessKeys> rangeQuery = new TableQuery<accessKeys>().Where(TableQuery.GenerateFilterCondition("host", QueryComparisons.Equal, helper.getUsername()));
                var queryResult = await accessKeysTable.ExecuteQuerySegmentedAsync<accessKeys>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                if (list.Count == 0)
                    return null;
                return list;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<List<accessKeys>> fetchAccessKeys(string username,string keys)
        {
            TableContinuationToken token = null;
            List<accessKeys> list = new List<accessKeys>();
            try
            {
                TableQuery<accessKeys> rangeQuery = new TableQuery<accessKeys>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, username+keys));
                var queryResult = await accessKeysTable.ExecuteQuerySegmentedAsync<accessKeys>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                if (list.Count == 0)
                    return null;
               
                return list;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<bool> updatePlotSecurity(plotSecurity temp)
        {
            try
            {

                TableOperation insertOperation = TableOperation.InsertOrReplace(temp);
                TableResult result = await plotSecurityTable.ExecuteAsync(insertOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> updateFileSecurity(plotSecurity temp)
        {
            try
            {
                TableOperation insertOperation = TableOperation.InsertOrReplace(temp);
                TableResult result = await FileSecurityTable.ExecuteAsync(insertOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> deleteFileSecurity(plotSecurity temp)
        {
            try
            {
                TableOperation deleteSecurityOperation = TableOperation.Delete(temp);
                TableResult finalSecurityResult = await FileSecurityTable.ExecuteAsync(deleteSecurityOperation);
                return true;

            }
            catch { }
            return true;
        }
        public static async Task<bool> replaceUser(users temp)
        {
            try
            {

                TableOperation replaceOperation = TableOperation.InsertOrReplace(temp);
                TableResult result = await userTable.ExecuteAsync(replaceOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> login(string username, string password)
        {

            users temp = await fetchUser(username);
            if (temp != null)
            {
                if (password == temp.PASSWORD)
                {
                    var session = ApplicationData.Current.LocalSettings;
                    session.Values["username"] = username;
                    return true;
                }

            }
            return false;
        }


        public static async Task<bool> updateDisplayPicture(StorageFile file, string username)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("profile");
                await container.CreateIfNotExistsAsync();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(username);
                await blockBlob.UploadFromFileAsync(file);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> updateTeamDisplayPicture(StorageFile file, string teamname)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("plots");
                await container.CreateIfNotExistsAsync();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(teamname);
                await blockBlob.UploadFromFileAsync(file);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<StorageFile> getDisplayPicture(string username)
        {
            StorageFile file = await helper.getDownloadProfileFile(username);
            try
            {

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("profile");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(username);

                await blockBlob.DownloadToFileAsync(file);
                return file;

            }
            catch
            {
                try
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference("profile");
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference("profile");
                    await blockBlob.DownloadToFileAsync(file);
                    return file;
                }
                catch
                {
                    return null;
                }
            }
        }
        public static async Task<StorageFile> getTeamDisplayPicture(string teamname)
        {
            StorageFile file = await helper.getDownloadTeamProfileFile(teamname);
            try
            {

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("plots");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(teamname);

                await blockBlob.DownloadToFileAsync(file);
                return file;

            }
            catch
            {
                try
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference("profile");
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference("profile");
                    await blockBlob.DownloadToFileAsync(file);
                    return file;
                }
                catch
                {
                    return null;
                }
            }
        }
        public static async Task<string> uploadFile(StorageFile temp)
        {
            //StorageFile temp = await helper.Encrpt(obj);
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(helper.getUsername());
                await container.CreateIfNotExistsAsync();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(temp.Name);
                if (!await blockBlob.ExistsAsync())
                {
                    await blockBlob.UploadFromFileAsync(temp);
                   
                    var basicProperties = await temp.GetBasicPropertiesAsync();
                    var size = basicProperties.Size;
                    string Extension = temp.FileType;
                    string ext = Extension.Substring(1, Extension.Length - 1);
                    ext = ext.ToLower();
                    if (ext == "jpeg" || ext == "jpg" || ext == "png" || ext == "bmp")
                        ext = "image";
                    files file = new files(temp.Name, ext, size.ToString());
                    await addFileOnline(file);
                    await updateFileCount(size, 0);
                    await helper.addFileDataLocal(file);
                    await temp.DeleteAsync();
                }
                else

                    return "File exist";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }
        public static async Task<bool> updateFileCount(double size, int decision)
        {
            users temp = await fetchUser(helper.getUsername());
            if (temp != null)
            {
                if (decision == 0)
                {
                    temp.incrementFileCount();
                    temp.addFileSize(size);
                }
                else
                {
                    temp.decrementFileCount();
                    temp.reduceFileSize(size);
                }
                try
                {
                    TableOperation retrieveOperation = TableOperation.InsertOrReplace(temp);
                    TableResult result = await userTable.ExecuteAsync(retrieveOperation);
                    return true;
                }
                catch { }
            }
            return false;
        }
        public static async Task<bool> deleteTeamProfile(string teamname)
        {
            try {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("plots");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(teamname);
                await blockBlob.DeleteIfExistsAsync();
                return true;
            }
            catch { }
            return false;
        }
        public static async Task<bool> deleteFile(files file,linking link,accessKeys key)
        {

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(helper.getUsername());
                await container.CreateIfNotExistsAsync();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.getFilename());
                await blockBlob.DeleteIfExistsAsync();
                files temp = await fetchFile(helper.getUsername(), file.getFilename());
                if (temp != null)
                {
                    await updateFileCount(Convert.ToDouble(temp.size), 1);
                    await deleteFileOnline(temp);
                    await helper.deleteFileDataLocal(temp.getFilename());
                    if(link!=null)
                      await removerLink(link);
                    if (key != null)
                        await deleteKey(key);
                }

                return true;
            }
            catch { }
            return false;
        }

        public static async Task<string> makeLink(string teamname, string filename, string size)
        {
            try
            {
                linking temp = new linking(helper.getUsername(), filename, teamname, size);
                TableOperation insertOperation = TableOperation.Insert(temp);
                await linkingTable.ExecuteAsync(insertOperation);
                await helper.addLinkDataLocal(temp);
                return "SUCCESS";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        public static async Task<bool> removerLink(string teamname, string filename)
        {
            try
            {
                linking link = await fetchLink(teamname, filename);

                TableOperation deleteOperation = TableOperation.Delete(link);
                TableResult finalResult = await linkingTable.ExecuteAsync(deleteOperation);

                await helper.deleteLinkLocal(teamname, filename);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> removerLink(linking link)
        {
            try
            {
            
                TableOperation deleteOperation = TableOperation.Delete(link);
                TableResult finalResult = await linkingTable.ExecuteAsync(deleteOperation);
                await helper.deleteLinkLocal(link.getTeamName(), link.getFilename());
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<List<linking>> getLink(string username){
             TableContinuationToken token = null;
             List<linking> list = new List<linking>();
         try
            {
                TableQuery<linking> rangeQuery = new TableQuery<linking>().Where(TableQuery.GenerateFilterCondition("host", QueryComparisons.Equal, username));
                var queryResult = await linkingTable.ExecuteQuerySegmentedAsync<linking>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                return list;
            }
        catch{}
            return null;
      }
        public static async Task<List<files>> getAllPlotFiles(string teamname)
        {
            TableContinuationToken token = null;
            List<linking> list = new List<linking>();
            List<files> finalList = new List<files>();
            try
            {
                TableQuery<linking> rangeQuery = new TableQuery<linking>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, teamname));
                var queryResult = await linkingTable.ExecuteQuerySegmentedAsync<linking>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                foreach (linking link in list)
                {
                    string[] ext = link.getFilename().Split('.');
                    int lastEntry = ext.Length - 1;
                    ext[lastEntry] = ext[lastEntry].ToLower();
                    if (ext[lastEntry] == "jpeg" || ext[lastEntry] == "jpg" || ext[lastEntry] == "png" || ext[lastEntry] == "bmp")
                        ext[lastEntry] = "image";

                    finalList.Add(new files(link.getHost(), link.getFilename(), ext[lastEntry], "0", ""));
                }

                return finalList;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<users>> getAllMembers(string teamname)
        {
            TableContinuationToken token = null;
            List<plots> list = new List<plots>();
            List<users> finalList = new List<users>();
            try
            {
                TableQuery<plots> rangeQuery = new TableQuery<plots>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, teamname));
                var queryResult = await plotsTable.ExecuteQuerySegmentedAsync<plots>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                foreach (plots member in list)
                {
                    users temp = new users(member.getUsername());
                    try
                    {

                        BitmapImage img = await helper.getProfileImageLocal(member.getUsername());
                        if (img != null)
                            temp.setImage(img);
                        else
                        {
                            await getDisplayPicture(member.getUsername());
                            img = await helper.getProfileImageLocal(member.getUsername());
                            temp.setImage(img);
                        }
                    }
                    catch { }
                    finalList.Add(temp);
                }

                return finalList;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<BitmapImage> downloadImage(string filename, string username)
        {
            StorageFolder pictures = KnownFolders.PicturesLibrary;
            try
            {
                await pictures.CreateFolderAsync("PLoT", CreationCollisionOption.FailIfExists);

            }
            catch { }
            StorageFolder localFolder = await pictures.GetFolderAsync("PLoT");

            StorageFile temp = await localFolder.CreateFileAsync("temp", CreationCollisionOption.ReplaceExisting);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(username);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);

            await blockBlob.DownloadToFileAsync(temp);
            BitmapImage img = new BitmapImage();
            using (var pictureStream = await temp.OpenAsync(FileAccessMode.Read))
            {
                img.SetSource(pictureStream);
            }
            return img;
        }

        public static async Task<bool> downloadFile(string username, string filename, StorageFolder temp)
        {
            try
            {
                StorageFile file = await temp.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
                 
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connection.getConnectionString());
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(username);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);
                await blockBlob.DownloadToFileAsync(file);
                StorageFile decrpt = await helper.Decrypt(file);
                await decrpt.CopyAndReplaceAsync(file);
                await file.RenameAsync(filename);
                await decrpt.DeleteAsync();
               // helper.popup(file.Path, "");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> sendMsg(string teamname, string msg)
        {
            try
            {
                chats temp = new chats(teamname, msg);
                TableOperation insertOperation = TableOperation.Insert(temp);
                await chatsTable.ExecuteAsync(insertOperation);
                return true;
            }
            catch
            {                
                return false;
            }
        }
        public static async Task<List<chats>> getAllMsg(string teamname)
        {
            TableContinuationToken token = null;
            List<chats> list = new List<chats>();
            List<chats> finalList = new List<chats>();
            try
            {
                TableQuery<chats> rangeQuery = new TableQuery<chats>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, teamname));
                var queryResult = await chatsTable.ExecuteQuerySegmentedAsync<chats>(rangeQuery, token);
                var entities = queryResult.Results.ToList();
                token = queryResult.ContinuationToken;
                list.AddRange(entities);
                if (list.Count == 0)
                    return null;
                foreach (chats temp in list)
                {
                    temp.setAlign();
                    finalList.Insert(0,temp);
                }
                
                
                return finalList;
            }
            catch
            {
                return null;
            }
        }
    }
}

