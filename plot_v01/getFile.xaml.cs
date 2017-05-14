using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using plot_v01.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;


// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace plot_v01
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class getFile : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public getFile()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }
        private bool enableComponent = true;
        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="Common.NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            object navigationParameter;
            if (e.PageState != null && e.PageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = e.PageState["SelectedItem"];
            }

            // TODO: Assign a bindable group to this.DefaultViewModel["Group"]
            // TODO: Assign a collection of bindable items to this.DefaultViewModel["Items"]
            // TODO: Assign the selected item to this.flipView.SelectedItem
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void getFile_Click(object sender, RoutedEventArgs e)
        {
            await getFileFromKey();
        }

        public async Task<bool> getFileFromKey()
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    if (DownloadFile.Content.ToString() != "Download")
                    {
                        if (helper.checkInternetConnection())
                        {
                            if (host.Text != "" && accessKey.Text != "")
                            {
                                try
                                {

                                    displayLoading("Fetching details...");

                                    if (await users.fetchAccessKeys(host.Text, accessKey.Text) == null)
                                    {
                                        helper.popup("You have entered a invalid access key!\nEnter a valid access key", "INCORRECT ACCESS KEY");
                                        disableLoading();
                                        return false;
                                    }

                                    plotSecurity security = await users.fetchFileSecurity(host.Text + accessKey.Text);
                                    plotSecurity currentSecurity = new plotSecurity();
                                    if (security != null)
                                    {
                                        if (await currentSecurity.check(security))
                                        {
                                            disableLoading();
                                            DownloadFile.Content = "Download";
                                            host.IsEnabled = false;
                                            accessKey.IsEnabled = false;
                                            preview.Visibility = Visibility.Visible;

                                        }
                                        else
                                        {
                                            disableLoading();
                                            helper.popup("Access denied", "ACCESS DENIED");
                                        }
                                    }
                                    else
                                    {
                                        disableLoading();
                                        DownloadFile.Content = "Download";
                                        host.IsEnabled = false;
                                        accessKey.IsEnabled = false;
                                        preview.Visibility = Visibility.Visible;

                                    }
                                }

                                catch
                                {
                                    disableLoading();
                                    helper.popup("You have entered a invalid access code\nEnter a valid access code.", "INVALID ACCESS CODE");
                                }
                            }
                            else
                            {
                                disableLoading();
                                helper.popup("Complete the details required to get a file", "INCOMPLETE");
                            }
                        }
                        else
                        {
                            disableLoading();
                            helper.popup("Please check your internet connection", "Connection failed");
                        }

                        disableLoading();
                    }
                    else
                    {
                        FolderPicker picker = new FolderPicker();
                        picker.FileTypeFilter.Add("*");
                        StorageFolder folder = await picker.PickSingleFolderAsync();
                        List<accessKeys> tempList = await users.fetchAccessKeys(host.Text, accessKey.Text);
                        int current = 0, total = tempList.Count;
                        displayLoading("Downloading...");

                        StorageFolder finalFolder = await folder.CreateFolderAsync("PLoT Access key - ("+accessKey.Text+")", CreationCollisionOption.GenerateUniqueName);

                        foreach (accessKeys temp in tempList)
                        {
                            displayLoading("Downloading files...\nFile: " + current.ToString() + "/" + total);
                            await users.downloadFile(temp.host, temp.RowKey, finalFolder);
                            current++;
                        }
                        disableLoading();
                        helper.popup("All the files are successfully downloaded and saved!", "DOWNLOAD COMPLETE");
                        navigationHelper.GoBack();
                    }

                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");

            }
            return true;
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

        private async void preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                displayLoading("Getting list of files inside this key ...");
                List<accessKeys> tempList = await users.fetchAccessKeys(host.Text , accessKey.Text);
                List<string> parameter = new List<string>();
                foreach (accessKeys key in tempList)
                {
                    if (key.getKeys() == accessKey.Text)
                        parameter.Add(key.getFilename());
                }
                List<files> fileList = new List<files>();
                foreach (string temp in parameter)
                {
                    string[] ext = temp.Split('.');
                    int lastEntry = ext.Length - 1;
                    ext[lastEntry] = ext[lastEntry].ToLower();
                    if (ext[lastEntry] == "jpeg" || ext[lastEntry] == "jpg" || ext[lastEntry] == "png" || ext[lastEntry] == "bmp")
                        ext[lastEntry] = "image";

                    fileList.Add(new files(helper.getUsername(), temp, ext[lastEntry], "0", ""));
                }
                ListName.Visibility = Visibility.Visible;
                ListName.Text = accessKey.Text + " Contains:";
                list.ItemsSource = fileList;
                
            }
            catch(Exception ex) { helper.popup(ex.ToString(), ""); }
            disableLoading();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
                navigationHelper.GoBack();
        }

        private async void host_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                await getFileFromKey();
        }
    }
}
