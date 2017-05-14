using System;
using System.Collections.Generic;
using System.IO;

using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace plot_v01
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class home : Page
    {
        public home()
        {
            this.InitializeComponent();
        }
        private static bool enableComponent = true;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await setListView();
            helper.setCounter();
            helper.setKey("");
            try
            {
                var img = await helper.getProfileImageLocal(helper.getUsername());
                if (img != null)
                    dp.ImageSource = img;
            }
            catch
            {
                try
                {
                    if (helper.checkInternetConnection())
                    {
                        await users.getDisplayPicture(helper.getUsername());
                        var img = await helper.getProfileImageLocal(helper.getUsername());
                        if (img != null)
                            dp.ImageSource = img;

                    }

                }
                catch
                { }
            }

        }

        private async void addFileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    try
                    {
                        FileOpenPicker picker = new FileOpenPicker();
                        picker.FileTypeFilter.Add(".pdf");
                        picker.FileTypeFilter.Add(".zip");
                        picker.FileTypeFilter.Add(".jpeg");
                        picker.FileTypeFilter.Add(".jpg");
                        picker.FileTypeFilter.Add(".png");
                        picker.FileTypeFilter.Add(".bmp");
                        IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();
                        int current = 0, total = files.Count;
                        double size = 0;
                        if (files.Count != 0)
                        {
                            foreach (StorageFile file in files)
                            {
                                var properties = await file.GetBasicPropertiesAsync();
                                size += properties.Size;
                            }
                            if ((await users.getSizeUsed(helper.getUsername()) + size) / 5 > (1024 * 1024 * 1024))
                            {
                                helper.popup("You have exhausted the container storage limit!\nUpgrade your subscription for more benefits.", "STORAGE LIMIT REACHED");
                                disableLoading();
                                await setListView();
                                return;
                            }
                            foreach (StorageFile file in files)
                            {
                                displayLoading("Uploading the file to your container...\nFile: " + current.ToString() + "/" + total);
                                
                                string temp = await users.uploadFile(file);

                                current++;
                            }
                            helper.popup("Files have been uploaded successfully", "UPLOAD COMPLETE");
                        }
                        else
                        {
                            disableLoading();
                            return;
                        }
                    }



                    catch { }
                }
                else
                {
                    helper.popup("Check your internet connection", "NO INTERNET");
                }

                disableLoading();
                await setListView();

            }
        }

        private async Task<bool> setListView()
        {

            List<files> items = null;

            displayLoading("Fetching Data ...");
            if (helper.checkInternetConnection())
            {
                progressBarText.Text = helper.Calculatesize(await users.getSizeUsed(helper.getUsername())) + " used of 5 GB";
                usedStorageProgressBar.Value = (int)(((await users.getSizeUsed(helper.getUsername()) / 5) / (1024 * 1024 * 1024)) * 100);

                items = await users.refreshFilesData();
                helper.setLocal("files");

            }
            else
                items = await helper.retriveFileDataLocal();


            disableLoading();

            if (items != null)
            {
                list.ItemsSource = items;
                return true;
            }

            return false;
        }

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {

                helper.removeLocalSession();
                helper.setOnline();
                Frame.Navigate(typeof(login));
            }
        }

        private void changeDisplayPicture(object sender, TappedRoutedEventArgs e)
        {
            if (enableComponent)
            {
                Frame.Navigate(typeof(changeProfilePicture), "profile");
            }
        }


        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedItems.Count == 1)
            {
                download.Visibility = Visibility.Visible;
                deleteFileBtn.Visibility = Visibility.Visible;
                plotBtn.Visibility = Visibility.Visible;
                share.Visibility = Visibility.Visible;
                files file = (files)list.SelectedItem;
                if (file.IsImage())
                    viewBtn.Visibility = Visibility.Visible;
                else
                    viewBtn.Visibility = Visibility.Collapsed;
            }

            else if (list.SelectedItems.Count > 1)
            {
                download.Visibility = Visibility.Visible;
                deleteFileBtn.Visibility = Visibility.Visible;
                plotBtn.Visibility = Visibility.Visible;
                share.Visibility = Visibility.Visible;
                viewBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                download.Visibility = Visibility.Collapsed;
                deleteFileBtn.Visibility = Visibility.Collapsed;
                share.Visibility = Visibility.Collapsed;
                plotBtn.Visibility = Visibility.Collapsed;
                viewBtn.Visibility = Visibility.Collapsed;

            }
        }


        private void displayLoading(string temp)
        {
            loading.IsActive = true;
            loadingText.Text = temp;
            enableComponent = false;
        }

        private void disableLoading()
        {
            loading.IsActive = false;
            loadingText.Text = "";
            enableComponent = true;
        }


        private async Task refreshFiles()
        {
            helper.setOnline();
            await setListView();
        }
        private async void refresh_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
                await refreshFiles();
        }



        private void editProfile_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    Frame.Navigate(typeof(editProfile));
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");
            }

        }

        private void viewBtn_Click(object sender, RoutedEventArgs e)
        {

            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    files file = (files)list.SelectedItem;
                    if (file.IsImage())
                        Frame.Navigate(typeof(ImageView), file);
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");

            }
        }

        private async void deleteFileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    object[] listSelected = list.SelectedItems.ToArray<object>();
                    bool userCheck = await helper.dialogPopup("Do you want to delete these files", "Delete");
                    if (userCheck)
                    {
                        displayLoading("Deleting...");

                        int current = 0, total = listSelected.Count<object>();
                        List<linking> linkingList = await users.getLink(helper.getUsername());
                        List<accessKeys> keys = await users.fetchAccessKeys();


                        foreach (files temp in listSelected)
                        {
                            linking parameterLink = null;
                            accessKeys parameterKey = null;
                            if (linkingList != null)
                            {
                                foreach (linking link in linkingList)
                                {
                                    if (link.getFilename() == temp.getFilename())
                                        parameterLink = link;
                                }
                            }
                            if (keys != null)
                            {
                                foreach (accessKeys key in keys)
                                {
                                    if (key.getFilename() == temp.getFilename())
                                        parameterKey = key;
                                }
                            }
                            displayLoading("Deleting files ...\nFile: " + current.ToString() + "/" + total);
                            if (!await users.deleteFile(temp, parameterLink, parameterKey))
                                helper.popup("The file couldn't be deleted. Please try again", "DELETION FAILED");
                            current++;
                        }
                        await setListView();

                        disableLoading();
                    }
                    else
                    {
                        foreach (files file in listSelected)
                        {
                            list.SelectedItems.Remove(file);
                        }

                    }
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");
            }
        }

        private async void download_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    List<object> temp = list.SelectedItems.ToList<object>();
                    try
                    {
                        FolderPicker picker = new FolderPicker();
                        picker.FileTypeFilter.Add("*");
                        StorageFolder folder = await picker.PickSingleFolderAsync();
                        int current = 0, total = temp.Count;

                        foreach (files file in temp)
                        {
                            displayLoading("Downloading files...\nFile: " + current.ToString() + "/" + total);
                            await users.downloadFile(helper.getUsername(), file.getFilename(), folder);
                            list.SelectedItems.Remove(file);
                            current++;
                        }
                        disableLoading();
                        helper.popup("Files have been downloaded and saved successfully.", "DOWNLOAD COMPLETE");
                    }
                    catch { }
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");
            }
        }

        private void plotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    List<object> temp = list.SelectedItems.ToList<object>();
                    Frame.Navigate(typeof(selectPlot), temp);
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");
            }
        }

        private void viewPLoT_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                Frame.Navigate(typeof(plot));
            }

        }

        private void invite_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    Frame.Navigate(typeof(invites));
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");
            }

        }

        private void share_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                List<object> file = list.SelectedItems.ToList<object>();
                var myList = new List<string>();
                foreach (files temp in file)
                {
                    myList.Add(temp.getFilename());
                }
                Frame.Navigate(typeof(shareFiles), myList);
            }
        }

        private void getFile_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
                Frame.Navigate(typeof(getFile));
        }

        private void Viewkeys_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    Frame.Navigate(typeof(ShowAccessKeys));
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");
            }

        }

        private async void encrpt_Click(object sender, RoutedEventArgs e)
        {
            try {
                FileOpenPicker filePicker = new FileOpenPicker();
                filePicker.FileTypeFilter.Add("*");
                StorageFile temp = await filePicker.PickSingleFileAsync();
                StorageFile encrpt = await helper.Encrpt(temp);
               // helper.popup("File is encrpted and please select the folder to place the encrpted file ", "File encrpted");
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add("*");
                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                StorageFile local = await folder.CreateFileAsync(temp.Name, CreationCollisionOption.GenerateUniqueName);
                await encrpt.CopyAndReplaceAsync(local);
                await encrpt.DeleteAsync();
            }
            catch { }
        }

        private async void decrpt_Click(object sender, RoutedEventArgs e)
        {
            try {
                FileOpenPicker filePicker = new FileOpenPicker();
                filePicker.FileTypeFilter.Add("*");
                StorageFile temp = await filePicker.PickSingleFileAsync();
                StorageFile decrpt = await helper.Decrypt(temp);
                //helper.popup("File is Decrpted and please select the folder to place the Decrpted file ", "File Decrpted");
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add("*");
                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                StorageFile local = await folder.CreateFileAsync(temp.Name, CreationCollisionOption.GenerateUniqueName);
                await decrpt.CopyAndReplaceAsync(local);
                await decrpt.DeleteAsync();
            }
            catch { }
        }
    }
}
