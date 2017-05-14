using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace plot_v01
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class plot : Page
    {
        public plot()
        {
            this.InitializeComponent();
        }
        private bool enableComponent = true;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await setListView();
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
        private void changeDisplayPicture(object sender, TappedRoutedEventArgs e)
        {
            if(enableComponent)
            Frame.Navigate(typeof(changeProfilePicture));
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

        private void ViewFiles_Click(object sender, RoutedEventArgs e)
        {
            if(enableComponent)
            Frame.Navigate(typeof(home));
        }

        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            if(enableComponent)
             Frame.Navigate(typeof(addPLoT));
        }

        private async Task<bool> setListView()
        {
            List<plots> items;
            displayLoading("Fetching data");
            if (helper.checkInternetConnection())
                {
                progressBarText.Text = helper.Calculatesize(await users.getSizeUsed(helper.getUsername())) + " used of 5 GB";
                usedStorageProgressBar.Value = (int)(((await users.getSizeUsed(helper.getUsername()) / 5) / (1024 * 1024 * 1024)) * 100);
                items = await users.refreshPlotData();
                    helper.setLocal("plots");
                }
                    
                else
                    items = await helper.retrivePlotDataLocal(helper.getUsername());

            disableLoading();
            if (items != null)
            {
                list.ItemsSource = items;
                
                return true;
            }
            return false;
        }


        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedItems.Count != 0)
                controlBtnStack.Visibility = Visibility.Visible;
            else
                controlBtnStack.Visibility = Visibility.Collapsed;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
            {
                plots temp = (plots)list.SelectedItem;
                Frame.Navigate(typeof(viewPlot), temp.getTeamName());
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
                Frame.Navigate(typeof(editProfile));
        }

       

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            plots temp = (plots) e.ClickedItem;
            Frame.Navigate(typeof(viewPlot), temp);
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

        private void getFile_Click(object sender, RoutedEventArgs e)
        {
            if(enableComponent)
                 Frame.Navigate(typeof(getFile));
        }
    }
}
