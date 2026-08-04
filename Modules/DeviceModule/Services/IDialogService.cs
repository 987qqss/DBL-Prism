using Core.Interfaces;
using Core.Models;

namespace DeviceModule.Services
{
    public interface IDialogService
    {
        ProductionLineModel? ShowProductionLineDialog(ProductionLineModel? model, bool isEditMode);
        DeviceModel? ShowDeviceDialog(DeviceModel? device, bool isEditMode);
        DeviceCommand? ShowCommandDialog(DeviceCommand? cmd, bool isEditMode, ProtocolType protocolType);
        IProtocolConfig? ShowProtocolConfigDialog(ProtocolType protocolType, IProtocolConfig? existingConfig);

        /// <summary>
        /// 显示报警配置对话框，为用户设置命令的报警上下限。
        /// 返回 true 表示已配置（命令的 IsAlarmEnabled/AlarmUpperLimit/AlarmLowerLimit 已更新）。
        /// </summary>
        bool ShowAlarmConfigDialog(DeviceCommand command);
    }
}