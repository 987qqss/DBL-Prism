using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Core.Models
{
   
    public partial class DataPoint : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string PointType { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Length { get; set; } = 1;
        public DataFormat DataFormat { get; set; } = DataFormat.Float;
        public float Scale { get; set; } = 1.0f;
        public float Offset { get; set; } = 0.0f;
        public string Unit { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [ObservableProperty]
        private object? _rawValue;

        [ObservableProperty]
        private object? _convertedValue;

        [ObservableProperty]
        private bool _isAlarm;

        [ObservableProperty]
        private bool _isValid;

        [ObservableProperty]
        private string _quality = "Good";

        public DateTime LastUpdateTime { get; set; } = DateTime.MinValue;
        public Func<object?, object?>? ValueConverter { get; set; }

        public void UpdateValue(object? rawValue)
        {
            RawValue = rawValue;
            if (ValueConverter != null)
            {
                try
                {
                    ConvertedValue = ValueConverter(rawValue);
                    IsValid = true;
                    Quality = "Good";
                }
                catch
                {
                    ConvertedValue = rawValue;
                    IsValid = false;
                    Quality = "Bad";
                }
            }
            else
            {
                ConvertedValue = ApplyScaleAndOffset(rawValue);
                IsValid = true;
                Quality = "Good";
            }
            LastUpdateTime = DateTime.Now;
        }

        private object? ApplyScaleAndOffset(object? value)
        {
            if (value is IConvertible convertible)
            {
                double numValue = convertible.ToDouble(null);
                return numValue * Scale + Offset;
            }
            return value;
        }
    }

    public partial class ModbusDataPoint : DataPoint
    {
        public byte SlaveId { get; set; } = 1;
        public ushort RegisterAddress { get; set; }
        public ModbusFunctionCode ModbusType { get; set; }
        public bool UseCustomOffset { get; set; }
    }

    public partial class SCPIDataPoint : DataPoint
    {
        public string QueryCommand { get; set; } = string.Empty;
        public string WriteCommand { get; set; } = string.Empty;
        public string ResponsePattern { get; set; } = string.Empty;
        public bool RequiresTerminator { get; set; } = true;
    }
}
