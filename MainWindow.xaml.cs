using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Grpc.Core;
using Grpc.Net.Client;
using HasznaltAuto;
using static HasznaltAuto.HasznaltAuto;

namespace HasznaltAutoKliens
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _sessionId = string.Empty;
        int _currentUser = 0;
        public static HasznaltAutoClient _hasznaltAutoClient;
        public static ObservableCollection<Car> CarList = new();

        public MainWindow(
            HasznaltAutoClient hasznaltAutoClient,
            string sessionId,
            int currentuser)
        {
            InitializeComponent();
            _hasznaltAutoClient = hasznaltAutoClient;
            _sessionId = sessionId;
            _currentUser = currentuser;
            Title += " - " + sessionId;
            List();
            Default();
            CarsGrid.ItemsSource = CarList;
            createButton.Click += new RoutedEventHandler(Create);
            updateButton.Click += new RoutedEventHandler(Update);
            deleteButton.Click += new RoutedEventHandler(Delete);
            buyButton.Click += new RoutedEventHandler(Buy);
            logoutButton.Click += new RoutedEventHandler(Logout);
        }

        public async void Logout(object sender, RoutedEventArgs e)
        {
            if (_sessionId is null)
            {
                Error("Unauthorized access.");
                return;
            }


        }

        public static async Task ReloadList()
        {
            CarList.Clear();
            using (var call = _hasznaltAutoClient.ListCars(new Empty()))
            {
                var responseStream = call.ResponseStream;
                while (await responseStream.MoveNext())
                {
                    Car car = responseStream.Current;
                    CarList.Add(car);
                }
            }
        }

        public async void List()
        {
            CarList.Clear();
            using (var call = _hasznaltAutoClient.ListCars(new Empty()))
            {
                var responseStream = call.ResponseStream;
                while (await responseStream.MoveNext())
                {
                    Car car = responseStream.Current;
                    CarList.Add(car);
                }
            }
        }

        public void Create(object sender, RoutedEventArgs e)
        {
            if (_sessionId is null)
            {
                Error("Unauthorized access.");
                return;
            }

            PopupWindow_CreateCar pwcg = new(_sessionId, _hasznaltAutoClient);
            Success("Car creation window opened.");
            pwcg.Show();
            Default();
        }

        public async void Update(object sender, RoutedEventArgs e)
        {
            if (_sessionId is null)
            {
                Error("Unauthorized access.");
                return;
            }

            if (CarsGrid.SelectedItem is not Car selectedCar)
            {
                Error("Selected item is not a car.");
                return;
            }

            Car response = await _hasznaltAutoClient.GetCarAsync(new GetCarRequest { CarId = selectedCar.Id });
            if (response is null)
            {
                Error("Car not found.");
                return;
            }

            PopupWindow_UpdateCar pwuc = new PopupWindow_UpdateCar(_sessionId, response, _hasznaltAutoClient);
            Success("Car update window opened.");
            pwuc.Show();
            Default();
        }

        public async void Delete(object sender, RoutedEventArgs e)
        {
            if (_sessionId is null)
            {
                Error("Unauthorized access.");
                return;
            }

            if (CarsGrid.SelectedItem is not Car selectedCar)
            {
                Error("Selected item is not a car.");
                return;
            }

            var response = await _hasznaltAutoClient.DeleteCarAsync(new DeleteCarRequest
            {
                CarId = selectedCar.Id,
                CurrentUser = _currentUser,
                SessionId = _sessionId
            });

            if (response.Success)
            {
                Success(response.Message);
                return;
            }
            else
            {
                Error(response.Message);
                return;
            }
        }

        public async void Buy(object sender, RoutedEventArgs e)
        {
            if (_sessionId is null)
            {
                Error("Unauthorized access.");
                return;
            }

            if (CarsGrid.SelectedItem is not Car selectedCar)
            {
                Error("Selected item is not a car.");
                return;
            }

            var response = await _hasznaltAutoClient.BuyCarAsync(new BuyCarRequest
            {
                CarId = selectedCar.Id,
                CurrentUser = _currentUser,
                SessionId = _sessionId
            });

            if (response.Success)
            {
                Success(response.Message);
                return;
            }
            else
            {
                Error(response.Message);
                return;
            }
        }

        private async void Get(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CarsGrid.SelectedItem is not Car selectedCar)
            {
                Error("Selected item is not a car.");
                return;
            }

            Car response = await _hasznaltAutoClient.GetCarAsync(new GetCarRequest { CarId = selectedCar.Id });
            if (response is null)
            {
                Error("Car not found.");
                return;
            }

            Success("Car found, opening car details window...");
            await Task.Delay(1000);
            PopupWindow_GetCar pwgc = new(response);
            pwgc.Show();
            Default();
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

        void Default()
        {
            resultMessage.Foreground = Brushes.Gray;
            resultMessage.Content = "Messages regarding the outcome of above operations will be displayed here.";
        }
    }
}
