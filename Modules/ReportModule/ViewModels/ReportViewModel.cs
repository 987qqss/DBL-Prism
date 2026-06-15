using Prism.Mvvm;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace ReportModule.ViewModels
{
    public class ReportViewModel : BindableBase
    {
        private string _startDate = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
        private string _endDate = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
        private string _reportType = "日报";
        private string _maxTemp = "35℃";
        private string _minTemp = "18℃";
        private string _avgTemp = "25.5℃";
        private string _alarmCount = "3次";

        public string StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public string EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public string ReportType
        {
            get => _reportType;
            set => SetProperty(ref _reportType, value);
        }

        public string MaxTemp
        {
            get => _maxTemp;
            set => SetProperty(ref _maxTemp, value);
        }

        public string MinTemp
        {
            get => _minTemp;
            set => SetProperty(ref _minTemp, value);
        }

        public string AvgTemp
        {
            get => _avgTemp;
            set => SetProperty(ref _avgTemp, value);
        }

        public string AlarmCount
        {
            get => _alarmCount;
            set => SetProperty(ref _alarmCount, value);
        }

        public ObservableCollection<ReportItem> ReportItems { get; } = new();

        public DelegateCommand GenerateReportCommand { get; }
        public DelegateCommand ExportReportCommand { get; }

        public ReportViewModel()
        {
            GenerateReportCommand = new DelegateCommand(GenerateReport);
            ExportReportCommand = new DelegateCommand(ExportReport);

            InitializeMockData();
        }

        private void InitializeMockData()
        {
            ReportItems.Add(new ReportItem { Time = "08:00", Temperature = "22.1", Humidity = "58", Voltage = "24.2" });
            ReportItems.Add(new ReportItem { Time = "09:00", Temperature = "23.5", Humidity = "60", Voltage = "24.3" });
            ReportItems.Add(new ReportItem { Time = "10:00", Temperature = "25.3", Humidity = "62", Voltage = "24.5" });
            ReportItems.Add(new ReportItem { Time = "11:00", Temperature = "27.8", Humidity = "65", Voltage = "24.6" });
            ReportItems.Add(new ReportItem { Time = "12:00", Temperature = "30.2", Humidity = "68", Voltage = "24.8" });
            ReportItems.Add(new ReportItem { Time = "13:00", Temperature = "32.1", Humidity = "70", Voltage = "24.9" });
        }

        private void GenerateReport()
        {
        }

        private void ExportReport()
        {
        }
    }

    public class ReportItem
    {
        public string Time { get; set; } = string.Empty;
        public string Temperature { get; set; } = string.Empty;
        public string Humidity { get; set; } = string.Empty;
        public string Voltage { get; set; } = string.Empty;
    }
}