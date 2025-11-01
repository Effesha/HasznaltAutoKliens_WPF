using HasznaltAuto.API;
using HasznaltAuto.API.DTOs;
using HasznaltAuto.Desktop.Helpers;
using HasznaltAuto.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using static HasznaltAuto.API.CarGrpc;
using static HasznaltAuto.API.HasznaltAutoGrpc;

namespace HasznaltAutoKliens
{
    /// <summary>
    /// Interaction logic for PopupWindow_CreateCar.xaml
    /// </summary>
    public partial class PopupWindow_CreateCar : Window
    {
        private readonly HasznaltAutoGrpcClient _hasznaltAutoGrpcClient;
        private readonly CarGrpcClient _carGrpcClient;

        private readonly MainWindow _mainWindow;
        private readonly string _sessionId;
        private readonly int _currentUser;

        public PopupVm ViewModel { get; set; } = new();

        public PopupWindow_CreateCar(MainWindow mainWindow, string sessionId, int currentUser, List<ModelType> models, List<MakeType> makes, List<FuelType> fuels)
        {
            InitializeComponent();

            _hasznaltAutoGrpcClient = App.GrpcService.HasznaltAutoGrpcClient;
            _carGrpcClient = App.GrpcService.CarGrpcClient;

            _mainWindow = mainWindow;
            _sessionId = sessionId;
            _currentUser = currentUser;

            ViewModel.Models = models;
            ViewModel.Makes = makes;
            ViewModel.FuelTypes = fuels;

            DataContext = ViewModel;

            createButton.Click += new RoutedEventHandler(Create);
            resultMessage.Content = "Input format requirements on input hover";
        }

        public async void Create(object sender, RoutedEventArgs e)
        {
            bool isCarValid = CarValidator.Validate(ViewModel.CarDto, out string errorMessage);
            if (!isCarValid)
            {
                MessageHelper.Error(ref resultMessage, errorMessage);
                return;
            }

            var vehRegResponse = await _carGrpcClient.CreateVehicleRegistrationAsync(new VehicleRegistrationRequest
            {
                LicensePlate = ViewModel.CarDto.VehicleRegistrationDto.LicensePlate,
                LicensePlateTypeTypeId = 1,
                SessionId = _sessionId
            });

            if (!vehRegResponse.Success)
            {
                MessageHelper.Error(ref resultMessage, $"Vehicle registration error: {vehRegResponse.Message}");
                return;
            }

            var response = await _hasznaltAutoGrpcClient.CreateCarAsync(new CreateCarRequest
            {
                Car = ViewModel.CarDto.MapToCarType(_currentUser, vehRegResponse.EntityId),
                SessionId = _sessionId
            });

            if (response.Success)
            {
                MessageHelper.Success(ref resultMessage, response.Message);
                resultMessage.Content += ", redirecting...";
                await Task.Delay(1000);
                _mainWindow.Refresh(sender, e);
                Close();
            }
            else
            {
                MessageHelper.Error(ref resultMessage, response.Message);
            }
        }
    }
}
