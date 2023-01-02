using HasznaltAuto;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static HasznaltAuto.HasznaltAuto;

namespace HasznaltAutoKliens
{
    /// <summary>
    /// Interaction logic for PopupWindow_UpdateCar.xaml
    /// </summary>
    public partial class PopupWindow_UpdateCar : Window
    {
        Car Car;
        string _sessionId;
        HasznaltAutoClient _hasznaltAutoClient;

        public PopupWindow_UpdateCar(string sessionId, Car car, HasznaltAutoClient hasznaltAutoClient)
        {
            InitializeComponent();
            _sessionId = sessionId;
            _hasznaltAutoClient = hasznaltAutoClient;
            Car = car;
            DataContext = Car;
            updateButton.Click += new RoutedEventHandler(Update);
            resultMessage.Content = "Input format requirements on input hover";
        }

        private void Validate()
        {
            bool validationError = false;
            string validationMessage = string.Empty;
            if (Car.LicensePlate.Length == 7)
            {
                if (Car.LicensePlate.Contains('-'))
                {
                    Car.LicensePlate = Car.LicensePlate.ToUpper();
                    var licensePlate = Car.LicensePlate.Split('-');
                    if (licensePlate[0] is not string)
                    {
                        validationError = true;
                        validationMessage += "License plate invalid: first part only letters allowed.";
                    }
                    if (licensePlate[1].All(char.IsDigit) is false)
                    {
                        validationError = true;
                        validationMessage += "License plate invalid: second part only numbers allowed.";
                    }
                }
                else
                {
                    validationError = true;
                    validationMessage += "License plate invalid: invalid format.";
                }
            }
            else
            {
                validationError = true;
                validationMessage += "License plate invalid: invalid length.";
            }

            if (Car.Registration.Length == 7 || Car.Registration.Length == 6)
            {
                if (Car.Registration.Contains('/'))
                {
                    var registration = Car.Registration.Split('/');
                    if (int.TryParse(registration[0], out int year) is false || year <= 0 || year > DateTime.Now.Year + 1)
                    {
                        validationError = true;
                        validationMessage += "Registraton invalid: year invalid.";
                    }
                    if (int.TryParse(registration[1], out int month) is false || month < 0 || month > 12)
                    {
                        validationError = true;
                        validationMessage += "Registraton invalid: month invalid.";
                    }
                }
                else
                {
                    validationError = true;
                }
            }
            else
            {
                validationError = true;
                validationMessage += "Registration invalid: invalid length.";
            }

            if (Car.FuelType != FuelType.Gasoline && Car.FuelType != FuelType.Diesel)
            {
                validationError = true;
                validationMessage += "Fuel type invalid: only 'Gasoline' or 'Diesel' is accepted";
            }

            if (Car.CurrentOwner < 0)
            {
                validationError = true;
                validationMessage += "Owner id invalid: must be higher than 0.";
            }

            if (validationError)
            {
                Error(validationMessage);
                return;
            }
        }

        public async void Update(object sender, EventArgs e)
        {
            if (Car is null)
            {
                Error("Car object is empty.");
                return;
            }

            try
            {
                Validate();
            }
            catch (Exception ex)
            {
                resultMessage.Content = ex.Message;
            }

            var response = await _hasznaltAutoClient.UpdateCarAsync(new UpdateCarRequest
            {
                Car = Car,
                SessionId = _sessionId
            });

            if (response.Success)
            {
                Success(response.Message);
                resultMessage.Content += ", redirecting...";
                await Task.Delay(1000);
                await MainWindow.ReloadList();
                Close();
            }
            else
            {
                Error(response.Message);
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
