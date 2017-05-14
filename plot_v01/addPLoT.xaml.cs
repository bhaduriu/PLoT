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

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace plot_v01
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class addPLoT : Page
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

        public addPLoT()
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
        StorageFile file=null;
        private async void addPlot_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                displayLoading("Adding plot ...");
                if (helper.checkInternetConnection())
                {
                    if (plotName.Text != "")
                    {
                        try
                        {
                            if ((await users.fetchUser(helper.getUsername())).plotCount < 5)
                            {
                                if (file != null)
                                {
                                    await file.RenameAsync(plotName.Text);
                                    await users.updateTeamDisplayPicture(file, plotName.Text);
                                }
                                if (!(await users.addTeam(plotName.Text, helper.getUsername(), "admin")))
                                {
                                    helper.popup("The mentioned PLoT name is already under usage! \nProvide a new unique PLoT name", "Unique PLoT name");
                                    return;
                                }
                                helper.setOnline();
                                Frame.Navigate(typeof(plot));
                            }
                            else
                                helper.popup("You have exhausted your PLoT limit. \nUpgrade your subscription!", "LIMIT REACHED!");
                        }
                        catch
                        {
                            helper.popup("The mentioned PLoT name is already under usage! \nProvide a new unique PLoT name", "Unique PLoT name");
                            return;
                        }
                    }

                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");

                disableLoading();
            }
            }

        private async void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (enableComponent)
            {
                
                if (helper.checkInternetConnection())
                {
                    try
                    {
                        FileOpenPicker picker = new FileOpenPicker();
                        picker.FileTypeFilter.Add(".jpeg");
                        picker.FileTypeFilter.Add(".jpg");
                        picker.FileTypeFilter.Add(".png");
                        picker.FileTypeFilter.Add(".bmp");
                        StorageFile temp = await picker.PickSingleFileAsync();
                        displayLoading("Updating team profile ...");
                        file = await helper.getDownloadTeamProfileFile("temp");
                        await temp.CopyAndReplaceAsync(file);
                        dp.ImageSource = await helper.getTeamProfileImage("temp");
                    }
                    catch { }
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");

                disableLoading();
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
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if(enableComponent)
                navigationHelper.GoBack();
        }
    }
}
