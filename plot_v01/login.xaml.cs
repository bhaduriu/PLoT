using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace plot_v01
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class login : Page
    {
        public login()
        {
            this.InitializeComponent();
        }
        private bool enableComponent = true;
        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (enableComponent)
                Frame.Navigate(typeof(register));
        }

        private async void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            await userLogin();
        }
        public async Task<bool> userLogin()
        {
            if (enableComponent)
            {
                enableComponent = false;
                if (username.Text.ToString() != "" && password.Password != "")
                {
                    //INTERNET CHECK
                    if (helper.checkInternetConnection())
                    {
                        if (helper.getCounter() < 2)
                        {
                            if (await users.login(username.Text.ToString(), password.Password.ToString()))
                            {
                                helper.setOnline();
                                Frame.Navigate(typeof(home));
                            }
                            else
                            {

                                    helper.updateCounter();
                                    helper.popup("Invalid login", "Wrong credentials");
                               
                            }
                        }
                        else
                            helper.popup("You have been blocked !!!", "Blocked");
                    }

                }
                enableComponent = true;
            }
            return true;
        }
        private async void password_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                await userLogin();
        }
    }

   

       

    }

