using Core.Models;
using DeviceModule.ViewModels;
using DeviceModule.Views;
using Prism.Ioc;
using System.Windows;

namespace DeviceModule.Services
{
    /// <summary>
    /// 对话框服务实现 - 使用 Prism 容器解析对话框视图和视图模型
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly IContainerProvider _containerProvider;

        public DialogService(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
        }

        public bool? ShowProductionLineDialog(ref ProductionLineModel productionLine, bool isEditMode)
        {
            var dialogView = _containerProvider.Resolve<ProductionLineDialogView>();
            var viewModel = (ProductionLineDialogViewModel)dialogView.DataContext;

            viewModel.Initialize(productionLine, isEditMode);

            var window = new Window
            {
                Title = isEditMode ? "编辑产线" : "新增产线",
                Content = dialogView,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow,
                WindowStyle = WindowStyle.ToolWindow
            };

            var result = window.ShowDialog();
            
            // 使用 GetResult() 的返回值
            if (result == true)
            {
                productionLine = viewModel.GetResult();
            }
            
            return result;
        }

        public bool? ShowDeviceDialog(ref DeviceModel device, bool isEditMode)
        {
            var dialogView = _containerProvider.Resolve<DeviceDialogView>();
            var viewModel = (DeviceDialogViewModel)dialogView.DataContext;
            viewModel.Initialize(device, isEditMode);

            var window = new Window
            {
                Title = isEditMode ? "编辑设备" : "新增设备",
                Content = dialogView,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow,
                WindowStyle = WindowStyle.ToolWindow
            };

            var result = window.ShowDialog();
            
            // 使用 GetResult() 的返回值
            if (result == true)
            {
                device = viewModel.GetResult();
            }
            
            return result;
        }

        public bool? ShowCommandDialog(ref DeviceCommand command, bool isEditMode)
        {
            var dialogView = _containerProvider.Resolve<CommandDialogView>();
            var viewModel = (CommandDialogViewModel)dialogView.DataContext;
            viewModel.Initialize(command, isEditMode);

            var window = new Window
            {
                Title = isEditMode ? "编辑命令" : "新增命令",
                Content = dialogView,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow,
                WindowStyle = WindowStyle.ToolWindow
            };

            var result = window.ShowDialog();
            
            // 使用 GetResult() 的返回值
            if (result == true)
            {
                command = viewModel.GetResult();
            }
            
            return result;
        }
    }
}
