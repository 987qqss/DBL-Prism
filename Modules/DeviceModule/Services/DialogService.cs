using Core.Interfaces;
using Core.Models;
using DeviceModule.Views;
using DeviceModule.Views.Dialog.ProtocolConfig;
using DeviceModule.ViewModels;
using DeviceModule.ViewModels.ProtocolConfig;
using Prism.Ioc;
using System.Windows;

namespace DeviceModule.Services
{
    public class DialogService : IDialogService
    {
        private readonly IContainerProvider _containerProvider;

        public DialogService(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
        }

        public ProductionLineModel? ShowProductionLineDialog(ProductionLineModel? model, bool isEditMode)
        {
            var view = _containerProvider.Resolve<ProductionLineDialogView>();
            var viewModel = _containerProvider.Resolve<ProductionLineDialogViewModel>();
            view.DataContext = viewModel;
            viewModel.Initialize(model ?? new ProductionLineModel(), isEditMode);

            var window = new Window
            {
                Title = viewModel.Title,
                Content = view,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow,
                WindowStyle = WindowStyle.ToolWindow
            };

            viewModel.CloseAction = r =>
            {
                window.DialogResult = r;
                window.Close();
            };

            var dialogResult = window.ShowDialog();
            return dialogResult == true ? viewModel.GetResult() : null;
        }

        public DeviceModel? ShowDeviceDialog(DeviceModel? device, bool isEditMode)
        {
            var view = _containerProvider.Resolve<DeviceDialogView>();
            var viewModel = _containerProvider.Resolve<DeviceDialogViewModel>();
            view.DataContext = viewModel;
            viewModel.Initialize(device ?? new DeviceModel(), isEditMode);

            var window = new Window
            {
                Title = viewModel.Title,
                Content = view,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow,
                WindowStyle = WindowStyle.ToolWindow
            };

            viewModel.CloseAction = r =>
            {
                window.DialogResult = r;
                window.Close();
            };

            var dialogResult = window.ShowDialog();
            return dialogResult == true ? viewModel.GetResult() : null;
        }

        public DeviceCommand? ShowCommandDialog(DeviceCommand? cmd, bool isEditMode)
        {
            var view = _containerProvider.Resolve<CommandDialogView>();
            var viewModel = _containerProvider.Resolve<CommandDialogViewModel>();
            view.DataContext = viewModel;
            viewModel.Initialize(cmd ?? new DeviceCommand(), isEditMode);

            var window = new Window
            {
                Title = viewModel.Title,
                Content = view,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow,
                WindowStyle = WindowStyle.ToolWindow
            };

            viewModel.CloseAction = r =>
            {
                window.DialogResult = r;
                window.Close();
            };

            var dialogResult = window.ShowDialog();
            return dialogResult == true ? viewModel.GetResult() : null;
        }

        public Core.Interfaces.IProtocolConfig? ShowProtocolConfigDialog(Core.Interfaces.ProtocolType protocolType, Core.Interfaces.IProtocolConfig? existingConfig)
        {
            var (view, viewModel) = ResolveProtocolConfigView(protocolType);
            if (view == null || viewModel == null)
                return null;

            viewModel.Initialize(existingConfig);

            var window = new Window
            {
                Title = viewModel.Title,
                Content = view,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow,
                WindowStyle = WindowStyle.ToolWindow
            };

            viewModel.CloseAction = r =>
            {
                window.DialogResult = r;
                window.Close();
            };

            var dialogResult = window.ShowDialog();
            return dialogResult == true ? viewModel.GetConfig() : null;
        }

        private (FrameworkElement? View, IProtocolConfigDialogViewModel? ViewModel) 
            ResolveProtocolConfigView(Core.Interfaces.ProtocolType protocolType)
        {
            switch (protocolType)
            {
                case Core.Interfaces.ProtocolType.ModbusTcp:
                    var tcpView = _containerProvider.Resolve<ModbusTCPConfigView>();
                    var tcpViewModel = _containerProvider.Resolve<ModbusTCPConfigViewModel>();
                    tcpView.DataContext = tcpViewModel;
                    return (tcpView, tcpViewModel);

                case Core.Interfaces.ProtocolType.ModbusRtu:
                    var rtuView = _containerProvider.Resolve<ModbusRTUConfigView>();
                    var rtuViewModel = _containerProvider.Resolve<ModbusRTUConfigViewModel>();
                    rtuView.DataContext = rtuViewModel;
                    return (rtuView, rtuViewModel);

                default:
                    return (null, null);
            }
        }
    }
}