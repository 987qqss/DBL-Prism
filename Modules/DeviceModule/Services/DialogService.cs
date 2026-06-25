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
    //这个类专门用于设备模块里打开的弹窗服务，比如添加或修改产线、设备、设备命令弹窗，配置协议弹窗
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

        //传入一个设备命令类和是否编辑参数，根据是否编辑判断打开的窗口是否保留传进来的设备命令属性
        //并且返回窗口修改后的设备命令类
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

        //这个方法通过传入的协议配置类型来打开对应的协议配置窗口，然后把传入的协议配置对象传入给窗口
        //并且返回窗口最后的协议配置类，这样就实现了配置协议的时候根据协议类型多态打开配置协议窗口
        public Core.Interfaces.IProtocolConfig? ShowProtocolConfigDialog(Core.Interfaces.ProtocolType protocolType, Core.Interfaces.IProtocolConfig? existingConfig)
        {
            var (view, viewModel) = ResolveProtocolConfigView(protocolType);
            if (view == null || viewModel == null)
                return null;

            viewModel.Initialize(existingConfig);//将传入的配置类传入给对应的viewModel让它初始化界面

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
            return dialogResult == true ? viewModel.GetConfig() : null;//返回窗口更改后的配置
        }

        //通过传入的协议类型返回对应的View和ViewModel
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