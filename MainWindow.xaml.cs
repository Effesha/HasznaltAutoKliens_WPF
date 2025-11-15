using Grpc.Core;
using HasznaltAuto.API;
using HasznaltAuto.API.DTOs;
using HasznaltAuto.API.REST.Mappers;
using HasznaltAuto.Desktop.Helpers;
using HasznaltAuto.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static HasznaltAuto.API.CarGrpc;
using static HasznaltAuto.API.HasznaltAutoGrpc;
using static HasznaltAuto.API.UserGrpc;

namespace HasznaltAutoKliens
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HasznaltAutoGrpcClient _hasznaltAutoGrpcClient;
        private readonly UserGrpcClient _userGrpClient;
        private readonly CarGrpcClient _carGrpClient;

        private static ObservableCollection<CarType> CarList = [];
        private static ObservableCollection<ModelType> Models = [];
        private static ObservableCollection<MakeType> Makes = [];
        private static ObservableCollection<HasznaltAuto.API.FuelType> FuelTypes = [];
        private static ObservableCollection<VehicleRegistrationType> VehicleRegistrationTypes = [];
        private static ObservableCollection<ListUsersResponse> Users = [];

        public List<CarDto> CarDtos { get; set; } = [];

        string _sessionId = string.Empty;
        int _currentUser = 0;
        bool _isGuest = false;

        public MainWindow(string sessionId, int currentuser)
        {
            InitializeComponent();

            _hasznaltAutoGrpcClient = App.GrpcService.HasznaltAutoGrpcClient;
            _userGrpClient = App.GrpcService.UserGrpcClient;
            _carGrpClient = App.GrpcService.CarGrpcClient;

            _sessionId = sessionId;
            _currentUser = currentuser;
            Title += " - " + sessionId;
            MessageHelper.Default(ref resultMessage);

            Loaded += OnLoad;

            AssignEventHandlers();
        }

        public MainWindow(bool isGuest)
        {
            InitializeComponent();

            _hasznaltAutoGrpcClient = App.GrpcService.HasznaltAutoGrpcClient;
            _userGrpClient = App.GrpcService.UserGrpcClient;
            _carGrpClient = App.GrpcService.CarGrpcClient;

            _isGuest = isGuest;
            HideButtons();
            Title += "Logged in as a guest";
            MessageHelper.Default(ref resultMessage);

            Loaded += OnLoad;

            AssignEventHandlers();
        }

        private void HideButtons()
        {
            createButton.Visibility = Visibility.Hidden;
            updateButton.Visibility = Visibility.Hidden;
            deleteButton.Visibility = Visibility.Hidden;
            buyButton.Visibility = Visibility.Hidden;
        }

        public async void Logout(object sender, RoutedEventArgs e)
        {
            if (_isGuest)
            {
                LoginWindow lw = new();
                Close();
                lw.Show();
            }

            if (_sessionId is null)
            {
                MessageHelper.Error(ref resultMessage, "Unauthorized access.");
                return;
            }

            var response = await _userGrpClient.LogoutAsync(new LogoutRequest { SessionId = _sessionId }, null);
            if (response.Success)
            {
                MessageHelper.Success(ref resultMessage, response.Message);
                _sessionId = string.Empty;
                _currentUser = 0;
                LoginWindow lw = new();
                Close();
                lw.Show();
            }
            else
            {
                MessageHelper.Error(ref resultMessage, response.Message);
                return;
            }
        }

        public void Refresh(object sender, RoutedEventArgs e)
        {
            OnLoad(sender, e);
        }

        public void Create(object sender, RoutedEventArgs e)
        {
            if (_sessionId is null || _isGuest)
            {
                MessageHelper.Error(ref resultMessage, "Unauthorized access.");
                return;
            }

            PopupWindow_CreateCar pwcg = new(this, _sessionId, _currentUser, Models.ToList(), Makes.ToList(), FuelTypes.ToList());
            MessageHelper.Success(ref resultMessage, "Car creation window opened.");
            pwcg.Show();
            MessageHelper.Default(ref resultMessage);
        }

        public async void Update(object sender, RoutedEventArgs e)
        {
            if (_sessionId is null || _isGuest)
            {
                MessageHelper.Error(ref resultMessage, "Unauthorized access.");
                return;
            }

            if (CarsGrid.SelectedItem is not CarDto selectedCar)
            {
                MessageHelper.Error(ref resultMessage, "Selected item is not a car.");
                return;
            }

            CarType carResponse = await _hasznaltAutoGrpcClient.GetCarAsync(new GetCarRequest { CarId = selectedCar.Id });
            if (carResponse is null)
            {
                MessageHelper.Error(ref resultMessage, "Car not found.");
                return;
            }

            if (carResponse.CurrentOwner != _currentUser)
            {
                MessageHelper.Error(ref resultMessage, "Unauthorized: this car does not belong to you.");
                return;
            }

            string? licensePlate = VehicleRegistrationTypes
                .ToList()
                .FirstOrDefault(vr => vr.Id == carResponse.VehicleRegistrationId)
                ?.LicensePlate;

            var viewModel = new PopupVm()
            {
                CarDto = selectedCar,
                FuelTypes = FuelTypes.ToList(),
                Makes = Makes.ToList(),
                Models = Models.ToList()
            };

            PopupWindow_UpdateCar pwuc = new PopupWindow_UpdateCar(this, viewModel, _sessionId, _currentUser);
            MessageHelper.Success(ref resultMessage, "Car update window opened.");
            pwuc.Show();
            MessageHelper.Default(ref resultMessage);
        }

        public async void Delete(object sender, RoutedEventArgs e)
        {
            if (_sessionId is null || _isGuest)
            {
                MessageHelper.Error(ref resultMessage, "Unauthorized access.");
                return;
            }

            if (CarsGrid.SelectedItem is not CarDto selectedCar)
            {
                MessageHelper.Error(ref resultMessage, "Selected item is not a car.");
                return;
            }

            if (selectedCar.CurrentOwner != _currentUser)
            {
                MessageHelper.Error(ref resultMessage, "Unauthorized: this car does not belong to you.");
                return;
            }

            MessageBoxResult deletePromptResult = MessageBox.Show("Are you sure you want to delete this car?", $"Deleting {selectedCar.VehicleRegistrationDto.LicensePlate}", MessageBoxButton.YesNoCancel);

            if (deletePromptResult == MessageBoxResult.Yes)
            {
                var response = await _hasznaltAutoGrpcClient.DeleteCarAsync(new DeleteCarRequest
                {
                    CarId = selectedCar.Id,
                    CurrentUser = _currentUser,
                    SessionId = _sessionId
                });

                if (response.Success)
                {
                    MessageHelper.Success(ref resultMessage, response.Message);
                    return;
                }
                else
                {
                    MessageHelper.Error(ref resultMessage, response.Message);
                    return;
                }
            }

            MessageHelper.Error(ref resultMessage, "Delete process was canceled by the user.");
        }

        public async void Buy(object sender, RoutedEventArgs e)
        {
            if (_sessionId is null || _isGuest)
            {
                MessageHelper.Error(ref resultMessage, "Unauthorized access.");
                return;
            }

            if (CarsGrid.SelectedItem is not CarDto selectedCar)
            {
                MessageHelper.Error(ref resultMessage, "Selected item is not a car.");
                return;
            }

            var response = await _hasznaltAutoGrpcClient.BuyCarAsync(new BuyCarRequest
            {
                CarId = selectedCar.Id,
                CurrentUser = _currentUser,
                SessionId = _sessionId
            });

            if (response.Success)
            {
                MessageHelper.Success(ref resultMessage, response.Message);
                return;
            }
            else
            {
                MessageHelper.Error(ref resultMessage, response.Message);
                return;
            }
        }

        public async void Get(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CarsGrid.SelectedItem is not CarDto selectedCar)
            {
                MessageHelper.Error(ref resultMessage, "Selected item is not a car.");
                return;
            }

            MessageHelper.Success(ref resultMessage, "Car found, opening car details window...");
            await Task.Delay(1000);
            PopupWindow_GetCar pwgc = new(selectedCar);
            pwgc.Show();
            MessageHelper.Default(ref resultMessage);
        }

        #region Private methods
        private async void OnLoad(object sender, RoutedEventArgs e)
        {
            try
            {
                await GetDataFromDb();
                CarsGrid.ItemsSource = CarDtos;
            }
            catch (Exception)
            {
                MessageHelper.Error(ref resultMessage, "Unexpected error occured.");
            }
        }

        private async Task GetDataFromDb()
        {
            await PopulateDropdowns();
            await PopulateCarList();
        }

        private async Task PopulateCarList()
        {
            var carList = new ObservableCollection<CarType>();
            using var listCarsCall = _hasznaltAutoGrpcClient.ListCars(new Empty());
            var listCarsResponse = listCarsCall.ResponseStream;
            while (await listCarsResponse.MoveNext())
            {
                CarType car = listCarsResponse.Current;
                carList.Add(car);
            }

            CarList = carList;

            var vehRegsList = new ObservableCollection<VehicleRegistrationType>();
            using var listVehRegsCall = _carGrpClient.ListVehicleRegistrations(new Empty());
            var listVehRegsResponse = listVehRegsCall.ResponseStream;
            while (await listVehRegsResponse.MoveNext())
            {
                vehRegsList.Add(listVehRegsResponse.Current);
            }

            VehicleRegistrationTypes = vehRegsList;

            var usersList = new ObservableCollection<ListUsersResponse>();
            using var usersCall = _userGrpClient.ListUsers(new Empty());
            var usersResponse = usersCall.ResponseStream;
            while (await usersResponse.MoveNext())
            {
                usersList.Add(usersResponse.Current);
            }

            Users = usersList;

            foreach (var carType in CarList)
            {
                CarDtos.Add(carType.MapToCarDto(Users.ToList(), FuelTypes.ToList(), Makes.ToList(), Models.ToList(), VehicleRegistrationTypes.ToList()));
            }
        }

        private async Task PopulateDropdowns()
        {
            var modelsList = new ObservableCollection<ModelType>();
            using var modelsCall = _hasznaltAutoGrpcClient.ListModels(new Empty());
            var modelsResponse = modelsCall.ResponseStream;
            while (await modelsResponse.MoveNext())
            {
                modelsList.Add(modelsResponse.Current);
            }

            Models = modelsList;

            var makesList = new ObservableCollection<MakeType>();
            using var makesCall = _hasznaltAutoGrpcClient.ListMakes(new Empty());
            var makesResponse = makesCall.ResponseStream;
            while (await makesResponse.MoveNext())
            {
                makesList.Add(makesResponse.Current);
            }

            Makes = makesList;

            var fuelTypesList = new ObservableCollection<FuelType>();
            using var fuelTypesCall = _hasznaltAutoGrpcClient.ListFuelTypes(new Empty());
            var fuelTypesResponse = fuelTypesCall.ResponseStream;
            while (await fuelTypesResponse.MoveNext())
            {
                fuelTypesList.Add(fuelTypesResponse.Current);
            }

            FuelTypes = fuelTypesList;
        }

        private void AssignEventHandlers()
        {
            createButton.Click += new RoutedEventHandler(Create);
            updateButton.Click += new RoutedEventHandler(Update);
            deleteButton.Click += new RoutedEventHandler(Delete);
            buyButton.Click += new RoutedEventHandler(Buy);
            logoutButton.Click += new RoutedEventHandler(Logout);
            refreshButton.Click += new RoutedEventHandler(Refresh);
        }
        #endregion
    }
}
