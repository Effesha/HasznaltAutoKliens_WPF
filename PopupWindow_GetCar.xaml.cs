using HasznaltAuto.API.DTOs;
using System.Windows;

namespace HasznaltAutoKliens
{
    /// <summary>
    /// Interaction logic for PopupWindow_GetCar.xaml
    /// </summary>
    public partial class PopupWindow_GetCar : Window
    {
        private readonly CarDto _car;

        public PopupWindow_GetCar(CarDto car)
        {
            InitializeComponent();
            _car = car;
            Title += " - " + car.VehicleRegistrationDto.LicensePlate;
            DataContext = car;
        }
    }
}
