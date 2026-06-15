using Prism.Mvvm;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace DataCollectionModule.ViewModels
{
    public class DataCollectionViewModel : BindableBase
    {
        private bool _isCollecting;
        private string _collectFrequency = "1000ms";

        public bool IsCollecting
        {
            get => _isCollecting;
            set => SetProperty(ref _isCollecting, value);
        }

        public string CollectFrequency
        {
            get => _collectFrequency;
            set => SetProperty(ref _collectFrequency, value);
        }

        public ObservableCollection<DataPoint> DataPoints { get; } = new();

        public DelegateCommand StartCollectCommand { get; }
        public DelegateCommand StopCollectCommand { get; }
        public DelegateCommand ExportDataCommand { get; }

        public DataCollectionViewModel()
        {
            StartCollectCommand = new DelegateCommand(StartCollect);
            StopCollectCommand = new DelegateCommand(StopCollect);
            ExportDataCommand = new DelegateCommand(ExportData);

            InitializeMockData();
        }

        private void InitializeMockData()
        {
            DataPoints.Add(new DataPoint { Name = "温度1", Value = "25.3", Unit = "℃", Time = "10:30:00" });
            DataPoints.Add(new DataPoint { Name = "温度2", Value = "26.1", Unit = "℃", Time = "10:30:00" });
            DataPoints.Add(new DataPoint { Name = "湿度", Value = "65", Unit = "%RH", Time = "10:30:00" });
            DataPoints.Add(new DataPoint { Name = "电压", Value = "24.5", Unit = "V", Time = "10:30:00" });
            DataPoints.Add(new DataPoint { Name = "电流", Value = "1.2", Unit = "A", Time = "10:30:00" });
            DataPoints.Add(new DataPoint { Name = "功率", Value = "29.4", Unit = "W", Time = "10:30:00" });
        }

        private void StartCollect()
        {
            IsCollecting = true;
        }

        private void StopCollect()
        {
            IsCollecting = false;
        }

        private void ExportData()
        {
        }
    }

    public class DataPoint
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}