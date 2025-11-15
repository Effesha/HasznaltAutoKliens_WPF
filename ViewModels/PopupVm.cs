using HasznaltAuto.API;
using HasznaltAuto.API.DTOs;
using System.Collections.Generic;

namespace HasznaltAuto.Desktop.ViewModels
{
    public class PopupVm
    {
        public CarDto CarDto { get; set; } = new();
        public List<MakeType> Makes { get; set; } = [];
        public List<ModelType> Models { get; set; } = [];
        public List<FuelType> FuelTypes { get; set; } = [];
    }
}
