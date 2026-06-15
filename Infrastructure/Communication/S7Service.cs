using System.Net;
using System.Net.Sockets;

namespace Infrastructure.Communication
{
    public class S7Service : IS7Service, IDisposable
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private int _requestId;
        private byte[]? _sessionParams;
        private readonly object _lockObj = new object();
        private bool _disposed;

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public bool Connect(string ipAddress, int rack = 0, int slot = 2)
        {
            lock (_lockObj)
            {
                try
                {
                    Disconnect();
                    
                    _tcpClient = new TcpClient();
                    var result = _tcpClient.BeginConnect(ipAddress, 102, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(3000);
                    
                    if (!success || !_tcpClient.Connected)
                    {
                        _tcpClient.Close();
                        return false;
                    }
                    
                    _networkStream = _tcpClient.GetStream();
                    _networkStream.ReadTimeout = 3000;
                    _networkStream.WriteTimeout = 3000;
                    
                    if (!EstablishConnection(rack, slot))
                    {
                        Disconnect();
                        return false;
                    }
                    
                    return true;
                }
                catch
                {
                    Disconnect();
                    return false;
                }
            }
        }

        private bool EstablishConnection(int rack, int slot)
        {
            try
            {
                var connectRequest = BuildConnectRequest();
                _networkStream!.Write(connectRequest, 0, connectRequest.Length);
                
                var response = new byte[1024];
                var bytesRead = _networkStream.Read(response, 0, response.Length);
                
                if (bytesRead < 10 || response[2] != 0x02)
                    return false;

                _sessionParams = new byte[16];
                Array.Copy(response, 10, _sessionParams, 0, 16);
                
                var setupRequest = BuildSetupRequest(rack, slot);
                _networkStream.Write(setupRequest, 0, setupRequest.Length);
                
                bytesRead = _networkStream.Read(response, 0, response.Length);
                
                return bytesRead >= 24 && response[2] == 0x02 && response[10] == 0x00;
            }
            catch
            {
                return false;
            }
        }

        private byte[] BuildConnectRequest()
        {
            return new byte[]
            {
                0x03, 0x00, 0x00, 0x11,
                0x0E, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0xC1, 0x02, 0x01, 0x00,
                0xC2, 0x02, 0x01, 0x00
            };
        }

        private byte[] BuildSetupRequest(int rack, int slot)
        {
            var request = new byte[28];
            request[0] = 0x03;
            request[1] = 0x00;
            request[2] = 0x00;
            request[3] = 0x1C;
            request[4] = 0x02;
            request[5] = 0x00;
            request[6] = 0x00;
            request[7] = 0x00;
            
            Array.Copy(_sessionParams!, 0, request, 8, 16);
            
            request[24] = (byte)rack;
            request[25] = (byte)slot;
            request[26] = 0x00;
            request[27] = 0x00;
            
            return request;
        }

        public void Disconnect()
        {
            lock (_lockObj)
            {
                try
                {
                    if (_sessionParams != null && IsConnected)
                    {
                        var disconnectRequest = BuildDisconnectRequest();
                        _networkStream?.Write(disconnectRequest, 0, disconnectRequest.Length);
                    }
                }
                catch
                {
                    // 忽略关闭异常
                }
                
                try
                {
                    _networkStream?.Close();
                }
                catch
                {
                    // 忽略关闭异常
                }
                
                try
                {
                    _tcpClient?.Close();
                }
                catch
                {
                    // 忽略关闭异常
                }
                
                _networkStream = null;
                _tcpClient = null;
                _sessionParams = null;
            }
        }

        private byte[] BuildDisconnectRequest()
        {
            var request = new byte[20];
            request[0] = 0x03;
            request[1] = 0x00;
            request[2] = 0x00;
            request[3] = 0x14;
            request[4] = 0x04;
            request[5] = 0x00;
            request[6] = 0x00;
            request[7] = 0x00;
            
            Array.Copy(_sessionParams!, 0, request, 8, 12);
            
            return request;
        }

        public async Task<byte[]> ReadDataAsync(int dbNumber, int startOffset, int length)
        {
            if (!IsConnected || _sessionParams == null)
                throw new InvalidOperationException("未连接到设备");

            var request = BuildReadRequest(dbNumber, startOffset, length);
            
            _networkStream!.Write(request, 0, request.Length);
            
            var response = new byte[1024];
            var bytesRead = await _networkStream.ReadAsync(response, 0, response.Length);
            
            if (bytesRead < 28)
                throw new InvalidOperationException("响应数据不完整");

            var dataLength = (response[26] << 8) | response[27];
            var result = new byte[dataLength];
            Array.Copy(response, 28, result, 0, dataLength);
            
            return result;
        }

        public async Task WriteDataAsync(int dbNumber, int startOffset, byte[] data)
        {
            if (!IsConnected || _sessionParams == null)
                throw new InvalidOperationException("未连接到设备");

            var request = BuildWriteRequest(dbNumber, startOffset, data);
            
            _networkStream!.Write(request, 0, request.Length);
            
            var response = new byte[1024];
            var bytesRead = await _networkStream.ReadAsync(response, 0, response.Length);
            
            if (bytesRead < 24 || response[22] != 0x00)
                throw new InvalidOperationException("写入失败");
        }

        private byte[] BuildReadRequest(int dbNumber, int startOffset, int length)
        {
            var request = new byte[44];
            
            request[0] = 0x03;
            request[1] = 0x00;
            request[2] = 0x00;
            request[3] = 0x2C;
            request[4] = 0x02;
            request[5] = 0x00;
            
            _requestId = (_requestId + 1) % 65536;
            request[6] = (byte)(_requestId >> 8);
            request[7] = (byte)_requestId;
            
            Array.Copy(_sessionParams!, 0, request, 8, 16);
            
            request[24] = 0x01;
            request[25] = 0x00;
            request[26] = 0x00;
            request[27] = 0x01;
            request[28] = 0x00;
            request[29] = 0x00;
            request[30] = 0x04;
            request[31] = 0x01;
            
            request[32] = 0x84;
            request[33] = (byte)((dbNumber >> 8) & 0xFF);
            request[34] = (byte)(dbNumber & 0xFF);
            request[35] = 0x00;
            
            request[36] = (byte)((startOffset >> 24) & 0xFF);
            request[37] = (byte)((startOffset >> 16) & 0xFF);
            request[38] = (byte)((startOffset >> 8) & 0xFF);
            request[39] = (byte)(startOffset & 0xFF);
            
            request[40] = 0x00;
            request[41] = 0x00;
            request[42] = (byte)((length >> 8) & 0xFF);
            request[43] = (byte)(length & 0xFF);
            
            return request;
        }

        private byte[] BuildWriteRequest(int dbNumber, int startOffset, byte[] data)
        {
            var request = new byte[44 + data.Length];
            
            request[0] = 0x03;
            request[1] = 0x00;
            request[2] = 0x00;
            request[3] = (byte)(0x2C + data.Length);
            request[4] = 0x02;
            request[5] = 0x00;
            
            _requestId = (_requestId + 1) % 65536;
            request[6] = (byte)(_requestId >> 8);
            request[7] = (byte)_requestId;
            
            Array.Copy(_sessionParams!, 0, request, 8, 16);
            
            request[24] = 0x01;
            request[25] = 0x00;
            request[26] = 0x00;
            request[27] = 0x01;
            request[28] = 0x00;
            request[29] = 0x00;
            request[30] = (byte)(0x05 + data.Length);
            request[31] = 0x02;
            
            request[32] = 0x84;
            request[33] = (byte)((dbNumber >> 8) & 0xFF);
            request[34] = (byte)(dbNumber & 0xFF);
            request[35] = 0x00;
            
            request[36] = (byte)((startOffset >> 24) & 0xFF);
            request[37] = (byte)((startOffset >> 16) & 0xFF);
            request[38] = (byte)((startOffset >> 8) & 0xFF);
            request[39] = (byte)(startOffset & 0xFF);
            
            request[40] = 0x00;
            request[41] = 0x00;
            request[42] = (byte)((data.Length >> 8) & 0xFF);
            request[43] = (byte)(data.Length & 0xFF);
            
            Array.Copy(data, 0, request, 44, data.Length);
            
            return request;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            
            if (disposing)
            {
                Disconnect();
            }
            
            _disposed = true;
        }

        ~S7Service()
        {
            Dispose(false);
        }
    }
}
