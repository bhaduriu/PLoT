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
    public sealed partial class ShowAccessKeys : Page
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

        public ShowAccessKeys()
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
        List<accessKeys> accesskeyList = null;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            if (helper.checkInternetConnection())
            {
                displayLoading("Fetching data...");
                await setListView();
                disableLoading();
            }
            else
                helper.popup("Check your internet connection", "NO INTERNET");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

      

        private void changeSecurity_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    accessKeys temp = (accessKeys)list.SelectedItem;
                    if (temp != null)
                    {
                        List<string> parameter = new List<string>();
                        parameter.Add("profile");
                        parameter.Add(temp.getKeys());
                        Frame.Navigate(typeof(plotSecuritySettings), parameter);
                    }
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");

            }
        }

        private void view_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                accessKeys temp = (accessKeys)list.SelectedItem;
                if (temp != null)
                {
                    List<string> parameter = new List<string>();
                    parameter.Add(temp.getKeys());
                    foreach (accessKeys key in accesskeyList)
                    {
                        if (key.getKeys() == temp.getKeys())
                            parameter.Add(key.getFilename());
                    }
                    helper.setKey("");
                    Frame.Navigate(typeof(AccessKeyItems), parameter);
                }
               
            }
        }

        private async void delete_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    accessKeys temp = (accessKeys)list.SelectedItem;

                    if (temp != null)
                    {
                        foreach (accessKeys key in accesskeyList)
                        {
                            if (key.getKeys() == temp.getKeys())
                                await users.deleteKey(key);

                        }
                        await users.deleteFileSecurity(await users.fetchFileSecurity(helper.getUsername() + temp.getKeys()));
                        helper.popup(temp.getKeys() + " has been deleted", "Success");

                        navigationHelper.GoBack();
                    }
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");

            }
        }
        private async Task<bool> setListView()
        {
            accesskeyList = await users.fetchAccessKeys();
            if (accesskeyList != null)
            {
                string temp = "";
                List<accessKeys> keyList = new List<accessKeys>();

                foreach (accessKeys key in accesskeyList)
                {
                    if (key.getKeys() != temp)
                    {
                        keyList.Add(key);
                        temp = key.getKeys();
                    }
                }

                list.ItemsSource = keyList;
                return true;
            }
            return false;
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
    }
}
