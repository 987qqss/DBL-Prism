using Core.Interfaces;
using Prism.Mvvm;
using System.ComponentModel;

namespace DeviceModule.ViewModels.ProtocolConfig
{
    public interface IProtocolConfigDialogViewModel : INotifyPropertyChanged
    {
        string Title { get; }
        IProtocolConfig GetConfig();
        void Initialize(IProtocolConfig? existingConfig);
        bool CanConfirm();
        System.Action<bool>? CloseAction { get; set; }
    }
}