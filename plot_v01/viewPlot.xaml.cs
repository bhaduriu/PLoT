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
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace plot_v01
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class viewPlot : Page
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

        public viewPlot()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }
        string teamName = "";
        bool isAdmin = true;
        bool enableComponent = true;
        plots temp = new plots();
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                temp = e.Parameter as plots;
                teamName = temp.getTeamName();
                pageTitle.Text = temp.getTeamName();
                if (temp.getAccess() != "admin")
                {
                    addMemberBtn.Visibility = Visibility.Collapsed;
                    deletePlotBtn.Visibility = Visibility.Collapsed;
                    removeMemberBtn.Visibility = Visibility.Collapsed;
                    setting.Visibility = Visibility.Collapsed;
                    isAdmin = false;
                }
                var img = await helper.getTeamProfileImage(teamName);
                if (img != null)
                    teamDp.ImageSource = img;
                else
                {
                    await users.getTeamDisplayPicture(teamName);
                    img = await helper.getTeamProfileImage(teamName);
                    teamDp.ImageSource = img;
                }
                await setListView();
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        private async Task<bool> setListView()
        {
           displayLoading("Fetching data from "+teamName);
           List<files> items;
           List<users> members;
            if (helper.checkInternetConnection())
            {
                items = await users.getAllPlotFiles(teamName);
                members = await users.getAllMembers(teamName);
            }
           
           else
            {
                items = await helper.retriveTeamFiles(teamName);
                members = await helper.retriveTeamMember(teamName);
            }


            disableLoading();
            if (items != null)
             {
               list.ItemsSource = items;
                memberList.ItemsSource = members;
               return true;
            }
            return false;
        }

        private void setting_Click(object sender, RoutedEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                {
                    List<string> parameter = new List<string>();
                    parameter.Add("Team");
                    parameter.Add(teamName);
                    Frame.Navigate(typeof(plotSecuritySettings), parameter);
                }
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");

        }

        private async void deletePlotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                {
                    if (await helper.dialogPopup("Do you want to delete team ( " + teamName + " ) ?", "Alert"))
                    {
                        displayLoading("Deleting PLoT - " + teamName);
                        if (!await users.deleteTeam(temp))
                            helper.popup("Failed to delete", "Fail");
                        NavigationHelper.GoBack();
                    }
                }
            }
            else
            helper.popup("Check your internet connection", "NO INTERNET");

            disableLoading();
        }

        private void viewFile_Click(object sender, RoutedEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                {
                    files file = (files)list.SelectedItem;
                    if (file.IsImage())
                        Frame.Navigate(typeof(ImageView), file);
                }
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");
        }

        private async void download_Click(object sender, RoutedEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                {
                    plotSecurity temp = await users.fetchPlotSecurity(teamName);
                    try
                    {
                        FolderPicker picker = new FolderPicker();
                        picker.FileTypeFilter.Add("*");
                        StorageFolder folder = await picker.PickSingleFolderAsync();
                        displayLoading("Verifying ...");
                        List<object> listSelected =list.SelectedItems.ToList<object>();

                        plotSecurity current = new plotSecurity();

                        if (await current.check(temp))
                        {
                            int total = listSelected.Count;
                            int cur = 0;
                            //helper.popup(listSelected.Count.ToString(), "");
                            foreach (files data in listSelected)
                            {
                               
                                displayLoading("Downloading \n Files:" + cur  + "/" + total + "...");
                                await users.downloadFile(data.getHost(), data.getFilename(), folder);
                                
                                list.SelectedItems.Remove(data);
                                cur++;
                            }
                            disableLoading();
                            helper.popup("The requested file has been downloaded and saved successfully", "Downloaded");
                        }
                        else
                        {
                            disableLoading();
                            helper.popup("Access denied", "Access denied");
                        }

                    }
                    catch { }
                }
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");

        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedItems.Count == 1) {
                files file = (files)list.SelectedItem;
                if (file.IsImage())
                    viewFile.Visibility = Visibility.Visible;
                if (isAdmin)
                {
                    dePlotBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    if (file.getHost() == helper.getUsername())
                        dePlotBtn.Visibility = Visibility.Visible;
                    else
                        dePlotBtn.Visibility = Visibility.Collapsed;
                }
                
                download.Visibility = Visibility.Visible;
            }
            else if (list.SelectedItems.Count > 1)
            {
                if (isAdmin)
                {
                    dePlotBtn.Visibility = Visibility.Visible;
                }
                else {
                    bool isHost = true;
                    List<object> temp = list.SelectedItems.ToList<object>();
                    foreach (files file in temp)
                    {
                        if (file.getHost() != helper.getUsername())
                            isHost = false;
                    }
                    if (isHost)
                        dePlotBtn.Visibility = Visibility.Visible;
                    else
                        dePlotBtn.Visibility = Visibility.Collapsed;
                }
                viewFile.Visibility = Visibility.Collapsed;
                download.Visibility = Visibility.Visible;
            }
            else
            {
                dePlotBtn.Visibility = Visibility.Collapsed;
                download.Visibility = Visibility.Collapsed;
                viewFile.Visibility = Visibility.Collapsed;
            }

        }

        private async void dePlotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                {
                    object[] listSelected = list.SelectedItems.ToArray<object>();
                    if (await helper.dialogPopup("Do you want to delete these files?", "Alert"))
                    {
                        displayLoading("De-ploting files ...");

                        foreach (files data in listSelected)
                        {
                            await users.removerLink(teamName, data.getFilename());
                        }
                        disableLoading();
                        helper.setOnline();
                        await setListView();
                    }
                    else
                    {
                        foreach (files data in listSelected)
                        {
                            list.SelectedItems.Remove(data);
                        }
                    }
                }
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");
        }

        private void addMemberBtn_Click(object sender, RoutedEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                    Frame.Navigate(typeof(addMembers), teamName);
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");
        }

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                    Frame.Navigate(typeof(changeProfilePicture), teamName);
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");

        }

        private async void removeMemberBtn_Click(object sender, RoutedEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                {
                    List<object> temp = memberList.SelectedItems.ToList<object>();
                    if (await helper.dialogPopup("Do you want to remove these members ?", "Alert"))
                    {
                        displayLoading("Removing ...");

                        foreach (users member in temp)
                        {
                            if (member.getUsername() != helper.getUsername())
                                await users.deleteTeamMember(teamName, member.getUsername());
                        }
                        disableLoading();
                        helper.setOnline();
                        await setListView();
                    }
                    else
                    {
                        foreach (users member in temp)
                        {
                            memberList.SelectedItems.Remove(member);
                        }
                    }
                }
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");
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

        private void chatBoxBtn_Click(object sender, RoutedEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                    Frame.Navigate(typeof(ChatBox), teamName);
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
                navigationHelper.GoBack();
        }
    }
}
