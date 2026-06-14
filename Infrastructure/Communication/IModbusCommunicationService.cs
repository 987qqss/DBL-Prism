using Core.Models;

namespace Infrastructure.Communication
{
    public interface IModbusCommunicationService
    {
        void Start(CancellationToken appShutdownToken);
        void Stop();
        Task ReadProducer(CancellationToken ct);
        Task ReadConsumer(CancellationToken ct);
        Task WriteProducer(ModbusWriteCommand command, CancellationToken ct);
        Task WriteConsumer(CancellationToken ct);
    }
}