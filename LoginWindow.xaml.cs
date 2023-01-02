using Grpc.Net.Client;
using HasznaltAuto;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static HasznaltAuto.HasznaltAuto;

namespace HasznaltAutoKliens
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        GrpcChannel _grpcChannel = GrpcChannel.ForAddress("https://localhost:32767");
        HasznaltAutoClient _hasznaltAutoClient;

        public LoginWindow()
        {
            InitializeComponent();
            _hasznaltAutoClient = new HasznaltAutoClient(_grpcChannel);
            loginButton.Click += new RoutedEventHandler(Login);
            registerButton.Click += new RoutedEventHandler(Register);
        }

        public async void Login(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameTextbox.Text) || string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                Error("Please enter a username and a password.");
                return;
            }

            try
            {
                resultMessage.Content = "Trying to log you in...";
                var response = await _hasznaltAutoClient.LoginAsync(new LoginRequest
                {
                    Name = usernameTextbox.Text,
                    Password = passwordBox.Password
                });

                if (string.IsNullOrWhiteSpace(response.SessionId))
                {
                    Error(response.Message);
                    return;
                }
                else
                {
                    Success(response.Message);
                    resultMessage.Content += " Redirecting...";
                    await Task.Delay(3000);
                    MainWindow mw = new(_hasznaltAutoClient, response.SessionId, response.CurrentUser);
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

        public async void Register(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameTextbox.Text) || string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                Error("Please enter a username and a password.");
                return;
            }

            try
            {
                var response = await _hasznaltAutoClient.RegisterAsync(new RegistrationRequest
                {
                    Name = usernameTextbox.Text,
                    Password = passwordBox.Password
                });

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
}
