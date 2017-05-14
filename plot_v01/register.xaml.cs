using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
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
    public sealed partial class register : Page
    {
        public register()
        {
            this.InitializeComponent();
        }
        private bool enableComponent = true;
        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            if(enableComponent)
                Frame.Navigate(typeof(login));
        }

        private async void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            await userRegister();
        }

        public async Task<bool> userRegister()
        {

            if (enableComponent)
            {
                if (await helper.dialogPopup("App will collect your personal information like location , IP and ASHWID only for personalising user", "Privacy Policy"))
                {
                    displayLoading("Registering ...");
                    //VALID EMAIL CHECK
                    if (helper.validateEmail(email.Text))
                    {
                        //EMPTY TEXTBOX CHECK
                        if (username.Text.ToString() != "" && password.Password != "" && email.Text.ToString() != "")
                        {
                            //SAME PASSWORD CHECK
                            if (password.Password == confirmPassword.Password)
                            {
                                //INTERNET CHECK
                                if (helper.checkInternetConnection())
                                {
                                    if (!await users.checkASHWID(helper.getASHWID()))
                                    {
                                        users user = new users(username.Text.ToString(), password.Password.ToString(), email.Text.ToString());
                                        if (username.Text.ToLower() != "plots" && username.Text.ToLower() != "profile" && username.Text.ToLower() != "users")
                                        {
                                            if (await user.register())
                                            {
                                                Frame.Navigate(typeof(login));
                                            }
                                            else
                                            {
                                                helper.popup("The username requested has already been under utilisation currently.\nTry a different unique name.", "UNIQUE USERNAME");
                                            }
                                        }
                                        else
                                            helper.popup("The username requested has already been under utilisation currently.\nTry a different unique name.", "UNIQUE USERNAME");
                                    }
                                    else
                                    {
                                        helper.popup("This device is already registered under a different username.\nTry logging in with the registered ID.", "UNIQUE DEVICE");
                                    }
                                }
                                else
                                {
                                    helper.popup("Check your internet connection", "NO INTERNET");
                                }
                            }
                            else
                            {
                                helper.popup("Password entered is incorrect", "LOGIN FAILED");
                            }
                        }
                        else
                        {
                            helper.popup("Fill in all the fields", "INCOMPLETE");
                        }
                    }
                    else
                    {
                        helper.popup("Enter a valid email id!", "INVALID");
                    }
                    disableLoading();
                }
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
        private async void confirmPassword_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter) 
                   await userRegister();
        }
    }
}
