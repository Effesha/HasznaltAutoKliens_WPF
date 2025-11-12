using Azure;
using HasznaltAuto.API;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static HasznaltAuto.API.UserGrpc;

namespace HasznaltAutoKliens;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    private readonly UserGrpcClient _userGrpClient;

    public LoginWindow()
    {
        InitializeComponent();

        _userGrpClient = App.GrpcService.UserGrpcClient;

        loginButton.Click += new RoutedEventHandler(Login);
        registerButton.Click += new RoutedEventHandler(Register);
        guestLoginButton.Click += new RoutedEventHandler(GuestLogin);
    }

    public async void Login(object sender, RoutedEventArgs e)
    {
        // bypass login for quicker dev logins
        //resultMessage.Content += "Debug mode, logging in...";
        //await Task.Delay(1000);
        //MainWindow mwDebug = new("1", 1);
        //Close();
        //mwDebug.Show();

        if (string.IsNullOrWhiteSpace(usernameTextbox.Text) || string.IsNullOrWhiteSpace(passwordBox.Password))
        {
            Error("Please enter a username and a password.");
            return;
        }

        try
        {
            resultMessage.Content = "Trying to log you in...";
            var response = await _userGrpClient.LoginAsync(new LoginRequest
            {
                User = new UserType
                {
                    Name = usernameTextbox.Text,
                    Password = passwordBox.Password
                }
            }, null);

            if (string.IsNullOrWhiteSpace(response.SessionId))
            {
                Error(response.Message);
                return;
            }
            else
            {
                Success(response.Message);
                resultMessage.Content += " Redirecting...";
                await Task.Delay(1500); // Note AB: for demo purposes
                MainWindow mw = new(response.SessionId, response.CurrentUser);
                Close();
                mw.Show();
            }
        }
        catch (Exception ex)
        {
            Error("Server unavailable.");
            return;
        }
    }

    public async void GuestLogin(object sender, RoutedEventArgs e)
    {
        try
        {
            resultMessage.Content = "Trying to log you in...";
            await Task.Delay(500);
            Success("Guest login request successful.");
            resultMessage.Content += " Redirecting...";
            await Task.Delay(1500);
            MainWindow mw = new(isGuest: true);
            Close();
            mw.Show();

        }
        catch (Exception ex)
        {
            Error("Server unavailable.");
            return;
        }
    }

    public async void Register(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(usernameTextbox.Text) || string.IsNullOrWhiteSpace(passwordBox.Password))
        {
            Error("Please enter a username and a password.");
            return;
        }

        try
        {
            var response = await _userGrpClient.RegisterAsync(new RegistrationRequest
            {
                User = new UserType
                {
                    Name = usernameTextbox.Text,
                    Password = passwordBox.Password
                }
            }, null);

            if (response.Success)
            {
                Success(response.Message);
                resultMessage.Content += ", logging you in...";
                await Task.Delay(1000);
                Login(sender, e);
            }
            else
            {
                Error(response.Message);
            }
        }
        catch (Exception ex)
        {
            Error("Server unavailable.");
            return;
        }
    }

    void Error(string message)
    {
        resultMessage.Foreground = Brushes.Red;
        resultMessage.Content = message;
    }

    void Success(string message)
    {
        resultMessage.Foreground = Brushes.Green;
        resultMessage.Content = message;
    }
}
