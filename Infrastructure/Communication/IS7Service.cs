namespace Infrastructure.Communication
{
    public interface IS7Service
    {
        bool Connect(string ipAddress, int rack, int slot);
        void Disconnect();
        bool IsConnected { get; }
        Task<byte[]> ReadDataAsync(int dbNumber, int startOffset, int length);
        Task WriteDataAsync(int dbNumber, int startOffset, byte[] data);
    }
}