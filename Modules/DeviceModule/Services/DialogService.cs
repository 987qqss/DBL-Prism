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
    //�����ר�������豸ģ����򿪵ĵ������񣬱������ӻ��޸Ĳ��ߡ��豸���豸�����������Э�鵯��
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

        //����һ���豸��������Ƿ�༭�����������Ƿ�༭�жϴ򿪵Ĵ����Ƿ������������豸��������
        //���ҷ��ش����޸ĺ���豸������
        public DeviceCommand? ShowCommandDialog(DeviceCommand? cmd, bool isEditMode, ProtocolType protocolType)
        {
            var view = _containerProvider.Resolve<CommandDialogView>();
            var viewModel = _containerProvider.Resolve<CommandDialogViewModel>();
            view.DataContext = viewModel;
            viewModel.Initialize(cmd ?? new DeviceCommand(), isEditMode, protocolType);

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

        //�������ͨ�������Э�������������򿪶�Ӧ��Э�����ô��ڣ�Ȼ��Ѵ����Э�����ö����������
        //���ҷ��ش�������Э�������࣬������ʵ��������Э���ʱ�����Э�����Ͷ�̬������Э�鴰��
        public Core.Interfaces.IProtocolConfig? ShowProtocolConfigDialog(Core.Interfaces.ProtocolType protocolType, Core.Interfaces.IProtocolConfig? existingConfig)
        {
            var (view, viewModel) = ResolveProtocolConfigView(protocolType);
            if (view == null || viewModel == null)
                return null;

            viewModel.Initialize(existingConfig);//������������ഫ�����Ӧ��viewModel������ʼ������

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
            return dialogResult == true ? viewModel.GetConfig() : null;//���ش��ڸ��ĺ������
        }

        //ͨ�������Э�����ͷ��ض�Ӧ��View��ViewModel
        /// <summary>显示报警配置对话框，返回是否已配置</summary>
        public bool ShowAlarmConfigDialog(DeviceCommand command)
        {
            var view = _containerProvider.Resolve<AlarmConfigDialogView>();
            var viewModel = _containerProvider.Resolve<AlarmConfigDialogViewModel>();
            view.DataContext = viewModel;
            viewModel.Initialize(command);

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
            return dialogResult == true;
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