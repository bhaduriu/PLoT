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

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace plot_v01
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class shareFiles : Page
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

        public shareFiles()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }
        private bool enableComponent = true;
        List<string> list;
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

            accessKey.Text = helper.getKey();
            if(accessKey.Text!="")
                 accessKey.IsEnabled = false;
            
            list = e.Parameter as List<string>;
            List<files> fileList = new List<files>();
            foreach (string temp in list)
            {
                string[] ext = temp.Split('.');
                int lastEntry = ext.Length - 1;
                ext[lastEntry] = ext[lastEntry].ToLower();
                if (ext[lastEntry] == "jpeg" || ext[lastEntry] == "jpg" || ext[lastEntry] == "png" || ext[lastEntry] == "bmp")
                    ext[lastEntry] = "image";

                fileList.Add(new files(helper.getUsername(), temp, ext[lastEntry], "0", ""));
            }

            previewList.ItemsSource = fileList;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void getAccessKey_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                int accessCode;
                bool isNumeric = int.TryParse(accessKey.Text, out accessCode);
                if (isNumeric && accessKey.Text.Length == 4)
                {
                    if (await users.fetchAccessKeys(helper.getUsername(), accessKey.Text) != null)
                    {
                        helper.popup("This access key is already assigned to a different file.\nTry entering a different key.", "Already Taken");
                        return;
                    }
                    foreach (string temp in list)
                    {
                        await users.addAccessKey(accessCode.ToString(), temp);

                    }
                    helper.popup(accessKey.Text + " is been created ... ", "Success");
                    navigationHelper.GoBack();
                }
                else
                    helper.popup("Please enter any 4-digit number", "INVALID ACCESS KEY");
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
                navigationHelper.GoBack();
        }

        private async void setSecurity_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                if (helper.checkInternetConnection())
                {
                    if (await users.fetchAccessKeys(helper.getUsername(), accessKey.Text) == null)
                    {
                        bool check = false;
                        if (helper.getKey() != "")
                        {
                            check = true;
                        }
                        else
                        {
                            if (await helper.dialogPopup("You can't change the access key after going to security setting \n Are you sure you want this key ?", "Alert"))
                                check = true;
                        }
                        if (check)
                        {
                            List<string> parameter = new List<string>();
                            parameter.Add("profile");
                            parameter.Add(accessKey.Text);
                            Frame.Navigate(typeof(plotSecuritySettings), parameter);
                        }
                    }
                    else
                        helper.popup("Access Key is already assigned.\nTry using a differnt key.", "ACCESS KEY FAILED");
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");
            }
                
        }

        private void generateKey_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            int key = rnd.Next(1000, 9999);
            accessKey.Text = key.ToString();
        }
    }
}
