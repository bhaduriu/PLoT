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
using Windows.UI.Xaml.Media.Imaging;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace plot_v01
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class changeProfilePicture : Page
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

        public changeProfilePicture()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }
        private bool enableComponent = true;
        private string parameter = "";
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
            navigationHelper.OnNavigatedTo(e);
            parameter = e.Parameter as string;

            if (parameter == "profile")
            {
                try
                {
                    var img = await helper.getProfileImageLocal(helper.getUsername());
                    if (img != null)
                        dp.ImageSource = img;
                }
                catch { }
            }
            else
            {
                pageTitle.Text = "Edit display image";
                try
                {
                    var img = await helper.getTeamProfileImage(parameter);
                    if (img != null)
                        dp.ImageSource = img;
                }
                catch { }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void ChangePictureBtn_Click(object sender, RoutedEventArgs e)
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
                        StorageFile file = await picker.PickSingleFileAsync();
                        if (file != null)
                        {
                            var properties = await file.GetBasicPropertiesAsync();
                            if (properties.Size < 2097152)
                            {
                                displayLoading();
                                if (parameter == "profile")
                                {
                                    await helper.setProfileImageLocal(file);
                                    await users.updateDisplayPicture(file, helper.getUsername());
                                    dp.ImageSource = await helper.getProfileImageLocal(helper.getUsername());
                                }
                                else
                                {
                                    await helper.setTeamImageLocal(file, parameter);
                                    await users.updateTeamDisplayPicture(file, parameter);
                                    dp.ImageSource = await helper.getTeamProfileImage(parameter);
                                }
                                disableLoading();
                            }
                            else
                                helper.popup("Image size limit: 2 MB", "LIMIT SIZE EXCEEDED");
                        }

                    }
                    catch
                    {
                        
                    }
                }
                else
                    helper.popup("Check your internet connection", "NO INTERNET");
            }
        }

        private void displayLoading()
        {
            loading.IsActive = true;
            enableComponent = false;
        }

        private void disableLoading()
        {
            loading.IsActive = false;
            enableComponent = true;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
                navigationHelper.GoBack();
        }
    }
}
