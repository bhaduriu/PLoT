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

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace plot_v01
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class addMembers : Page
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

        public addMembers()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }
        string teamname = "";
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            teamname = e.Parameter as string;
            if (helper.checkInternetConnection())
            {
                displayLoading("Fetching users ...");
                list.ItemsSource = await users.fetchSpecificRangeUser("a", teamname,0);
                disableLoading();
            }
                
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void search_Click(object sender, RoutedEventArgs e)
        {
            await searchUser();
        }
        public async Task<bool> searchUser()
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                {
                    if (fetch.Text != "")
                    {
                        displayLoading("Fetching users ...");
                        list.ItemsSource = await users.fetchSpecificRangeUser(fetch.Text, teamname, 1);
                        disableLoading();
                    }
                }
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");
            return true;
        }
        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedItems.Count > 0)
                Add.Visibility = Visibility.Visible;
            else
                Add.Visibility = Visibility.Collapsed;
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            if (helper.checkInternetConnection())
            {
                if (enableComponent)
                {
                    List<object> memberList = list.SelectedItems.ToList<object>();
                    foreach (users temp in memberList)
                    {
                        await users.addInvite(teamname, temp.getUsername(), "member");
                    }
                    helper.popup("Your PLoT invite has been sent successfully!", "Invite sent");
                    navigationHelper.GoBack();
                }
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
                navigationHelper.GoBack();
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

        private async void fetch_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
                await searchUser();
        }
    }
}
