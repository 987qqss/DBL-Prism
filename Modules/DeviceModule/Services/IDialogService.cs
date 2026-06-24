using Core.Interfaces;
using Core.Models;

namespace DeviceModule.Services
{
    public interface IDialogService
    {
        ProductionLineModel? ShowProductionLineDialog(ProductionLineModel? model, bool isEditMode);
        DeviceModel? ShowDeviceDialog(DeviceModel? device, bool isEditMode);
        DeviceCommand? ShowCommandDialog(DeviceCommand? cmd, bool isEditMode);
        IProtocolConfig? ShowProtocolConfigDialog(ProtocolType protocolType, IProtocolConfig? existingConfig);
    }
}