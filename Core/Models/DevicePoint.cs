using CommunityToolkit.Mvvm.ComponentModel;

namespace Core.Models
{
    public partial class DevicePoint<T> : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public byte SlaveId { get; set; }
        public ushort Address { get; set; }
        public ModbusFunctionCode ModbusType { get; set; }

        [ObservableProperty]
        private T _value;

        [ObservableProperty]
        private bool _isAlarm;

        public string Unit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}