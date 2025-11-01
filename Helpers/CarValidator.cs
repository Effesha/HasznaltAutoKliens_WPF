using HasznaltAuto.API.DTOs;
using System;
using System.Globalization;
using System.Linq;

namespace HasznaltAuto.Desktop.Helpers
{
    public static class CarValidator
    {
        public static bool Validate(CarDto carDto, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(carDto.VehicleRegistrationDto.LicensePlate))
            {
                errorMessage += "License plate invalid: empty value.";
                return false;
            }

            if (carDto.VehicleRegistrationDto.LicensePlate.Length != 7)
            {
                errorMessage += "License plate invalid: invalid length.";
                return false;
            }

            if (!carDto.VehicleRegistrationDto.LicensePlate.Contains('-'))
            {
                errorMessage += "License plate invalid: invalid format.";
                return false;
            }

            carDto.VehicleRegistrationDto.LicensePlate = carDto.VehicleRegistrationDto.LicensePlate.ToUpper();
            var licensePlate = carDto.VehicleRegistrationDto.LicensePlate.Split('-');
            if (!licensePlate[0].All(char.IsLetter))
            {
                errorMessage += "License plate invalid: first part only letters allowed.";
                return false;
            }

            if (!licensePlate[1].All(char.IsDigit))
            {
                errorMessage += "License plate invalid: second part only numbers allowed.";
                return false;
            }

            if (string.IsNullOrEmpty(carDto.ProductionDate))
            {
                errorMessage += "Production date invalid: missing data";
                return false;
            }

            if (!DateTime.TryParseExact(carDto.ProductionDate, ["yyyy/MM", "yyyy/M"], CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                errorMessage += "Production date invalid: invalid format.";
                return false;
            }

            if (carDto.Mileage < 0)
            {
                errorMessage += "Mileage invalid: mileage can not be a negative number.";
                return false;
            }

            if (carDto.Price < 0)
            {
                errorMessage += "Price invalid: price can not be a negative number.";
                return false;
            }

            return true;
        }

    }
}
