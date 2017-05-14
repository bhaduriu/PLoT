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
    public sealed partial class plotSecuritySettings : Page
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

        public plotSecuritySettings()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

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
        private string teamName = "";
        private string accessKey = "";
        private bool enableComponent = false;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try {
                List<string> list = e.Parameter as List<string>;
                if (list[0] != "profile")
                {
                    teamName = list[1];
                    plotSecurity temp = await users.fetchPlotSecurity(teamName);

                    if (temp.latitude != "" && temp.longitude != "" && temp.range != "")
                    {
                        latitude.Text = temp.latitude;
                        longitude.Text = temp.longitude;
                        int Range = 0;
                        int.TryParse(temp.range, out Range);
                        range.Value = Range;
                        geoToggle.IsOn = true;
                    }
                    /*if (temp.password != "") {
                        password.Password = temp.password;
                        passwordToggle.IsOn = true;
                    }*/
                    if (temp.ssid != "")
                    {
                        ssid.Text = temp.ssid;
                        networkToggle.IsOn = true;
                    }

                    if (temp.registeredDevice == 1)
                    {
                        ASHWIDcheckbox.IsChecked = true;
                        extraToggle.IsOn = true;
                    }

                    checkToggle();
                }
                else
                {
                    accessKey = list[1];
                    plotSecurity temp = await users.fetchFileSecurity(helper.getUsername() + accessKey);
                    if (temp == null)
                    {
                        temp = new plotSecurity(helper.getUsername()+accessKey);
                    }
                        if (temp.latitude != "" && temp.longitude != "" && temp.range != "")
                        {
                            latitude.Text = temp.latitude;
                            longitude.Text = temp.longitude;
                            int Range = 0;
                            int.TryParse(temp.range, out Range);
                            range.Value = Range;
                            geoToggle.IsOn = true;
                        }
                        /*if (temp.password != "") {
                            password.Password = temp.password;
                            passwordToggle.IsOn = true;
                        }*/
                        if (temp.ssid != "")
                        {
                            ssid.Text = temp.ssid;
                            networkToggle.IsOn = true;
                        }

                        if (temp.registeredDevice == 1)
                        {
                            ASHWIDcheckbox.IsChecked = true;
                            extraToggle.IsOn = true;
                        }

                        checkToggle();
                    
                }
            }
            catch(Exception s)
            {
                helper.popup(s.ToString(), "");
            }
            navigationHelper.OnNavigatedTo(e);
            enableComponent = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void currentNetworkCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ssid.Text = plotSecurity.setCurrentNetwork();
        }

        private async void currentGeoCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            plotSecurity temp = new plotSecurity();
            try {
                await temp.setCurrentGeo();
                latitude.Text = temp.latitude;
                longitude.Text = temp.longitude;
            }
            catch
            {
                helper.popup("Error in fetching the device location. Try again later","LOCATION FAILED");
            }
        
        }

        private void checkToggle()
        {
            if (geoToggle.IsOn)
            {
                latitude.IsEnabled = true;
                longitude.IsEnabled = true;
                range.IsEnabled = true;
                currentGeoCheckBox.IsEnabled = true;
            }
            else {
                latitude.IsEnabled = false;
                longitude.IsEnabled = false;
                range.IsEnabled = false;
                currentGeoCheckBox.IsEnabled = false;
            }
            if (networkToggle.IsOn)
            {
                ssid.IsEnabled = true;
                currentNetworkCheckBox.IsEnabled = true;
            }
            else
            {
                ssid.IsEnabled = false;
                currentNetworkCheckBox.IsEnabled = false;
            }

           /* if (passwordToggle.IsOn)
            {
                password.IsEnabled = true;
            }
            else
                password.IsEnabled = false;
*/
            if(extraToggle.IsOn)
            {
               ASHWIDcheckbox.IsEnabled = true;
            }
            else
                ASHWIDcheckbox.IsEnabled = false;
        }
        
        private void geoToggle_Toggled(object sender, RoutedEventArgs e)
        {
            checkToggle();
        }

        private void networkToggle_Toggled(object sender, RoutedEventArgs e)
        {
            checkToggle();
        }

        private void passwordToggle_Toggled(object sender, RoutedEventArgs e)
        {
            checkToggle();
        }

        private void extraToggle_Toggled(object sender, RoutedEventArgs e)
        {
            checkToggle();
        }

        private async void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                enableComponent = false;
                plotSecurity temp = null;
                if (teamName != "")
                    temp = new plotSecurity(teamName);
                else
                    temp = new plotSecurity(helper.getUsername() + accessKey);

                if (geoToggle.IsOn)
                {
                    temp.setGeo(latitude.Text, longitude.Text, range.Value.ToString());
                }


                if (networkToggle.IsOn)
                {
                    temp.setSSID(ssid.Text);
                }

                /* if (passwordToggle.IsOn)
                 {
                     temp.setPassword(password.Password);
                 }
                */
                if (extraToggle.IsOn)
                {
                    if (ASHWIDcheckbox.IsChecked == true)
                    {
                        temp.registeredDevice = 1;
                    }
                }
                if (teamName != "")
                    await users.updatePlotSecurity(temp);
                else
                {
                    await users.updateFileSecurity(temp);
                    helper.setKey(accessKey);
                }
                enableComponent = true;
                navigationHelper.GoBack();
            }    
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
                navigationHelper.GoBack();
        }
    }
}
