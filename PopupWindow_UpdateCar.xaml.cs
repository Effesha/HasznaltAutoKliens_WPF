using HasznaltAuto.API;
using HasznaltAuto.API.DTOs;
using HasznaltAuto.Desktop.Helpers;
using HasznaltAuto.Desktop.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using static HasznaltAuto.API.CarGrpc;
using static HasznaltAuto.API.HasznaltAutoGrpc;

namespace HasznaltAutoKliens
{
    /// <summary>
    /// Interaction logic for PopupWindow_UpdateCar.xaml
    /// </summary>
    public partial class PopupWindow_UpdateCar : Window
    {
        private readonly HasznaltAutoGrpcClient _hasznaltAutoGrpcClient;
        private readonly CarGrpcClient _carGrpcClient;

        private readonly MainWindow _mainWindow;
        private readonly string _sessionId;
        private readonly int _currentUser;

        public PopupVm ViewModel { get; set; }

        public PopupWindow_UpdateCar(MainWindow mainWindow, PopupVm viewModel, string sessionId, int currentUser)
        {
            InitializeComponent();

            _hasznaltAutoGrpcClient = App.GrpcService.HasznaltAutoGrpcClient;
            _carGrpcClient = App.GrpcService.CarGrpcClient;

            _mainWindow = mainWindow;
            ViewModel = viewModel;
            _sessionId = sessionId;
            _currentUser = currentUser;
        }

        public async void Update(object sender, RoutedEventArgs e)
        {
            bool isCarValid = CarValidator.Validate(ViewModel.CarDto, out string errorMessage);
            if (!isCarValid)
            {
                MessageHelper.Error(ref resultMessage, errorMessage);
                return;
            }

            var response = await _hasznaltAutoGrpcClient.UpdateCarAsync(new UpdateCarRequest
            {
                Car = ViewModel.CarDto.MapToCarType(_currentUser),
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
