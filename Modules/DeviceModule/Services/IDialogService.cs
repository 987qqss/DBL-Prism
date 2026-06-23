using Core.Models;

namespace DeviceModule.Services
{
    /// <summary>
    /// 对话框服务接口 - 用于管理模态对话框的显示和交互
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 显示产线编辑对话框
        /// </summary>
        /// <param name="productionLine">要编辑的产线对象（如果是新增则为新实例）</param>
        /// <param name="isEditMode">是否为编辑模式，true=编辑，false=新增</param>
        /// <returns>用户点击确定返回true，取消返回false</returns>
        bool? ShowProductionLineDialog(ref ProductionLineModel productionLine, bool isEditMode);

        /// <summary>
        /// 显示设备编辑对话框
        /// </summary>
        /// <param name="device">要编辑的设备对象（如果是新增则为新实例）</param>
        /// <param name="isEditMode">是否为编辑模式</param>
        /// <returns>用户点击确定返回true，取消返回false</returns>
        bool? ShowDeviceDialog(ref DeviceModel device, bool isEditMode);

        /// <summary>
        /// 显示命令编辑对话框
        /// </summary>
        /// <param name="command">要编辑的命令对象（如果是新增则为新实例）</param>
        /// <param name="isEditMode">是否为编辑模式</param>
        /// <returns>用户点击确定返回true，取消返回false</returns>
        bool? ShowCommandDialog(ref DeviceCommand command, bool isEditMode);
    }
}
