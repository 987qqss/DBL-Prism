using System.Net;
using System.Net.Sockets;

namespace Infrastructure.Communication
{
    public class ModbusTcpService : IModbusService, IDisposable
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private int _transactionId;
        private readonly object _lockObj = new object();
        private bool _disposed;

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public bool Connect(string ipAddress, int port = 502)
        {
            lock (_lockObj)
            {
                try
                {
                    Disconnect();
                    
                    _tcpClient = new TcpClient();
                    var result = _tcpClient.BeginConnect(ipAddress, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(3000);
                    
                    if (!success || !_tcpClient.Connected)
                    {
                        _tcpClient.Close();
                        return false;
                    }
                    
                    _networkStream = _tcpClient.GetStream();
                    _networkStream.ReadTimeout = 3000;
                    _networkStream.WriteTimeout = 3000;
                    _transactionId = 0;
                    
                    return true;
                }
                catch
                {
                    Disconnect();
                    return false;
                }
            }
        }

        public void Disconnect()
        {
            lock (_lockObj)
            {
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
            }
        }

        public async Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort count)
        {
            var request = BuildRequest(slaveId, 0x04, startAddress, count);
            var response = await SendRequestAsync(request, 7 + count * 2);
            return ParseRegisterResponse(response, count);
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort count)
        {
            var request = BuildRequest(slaveId, 0x03, startAddress, count);
            var response = await SendRequestAsync(request, 7 + count * 2);
            return ParseRegisterResponse(response, count);
        }

        public async Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort count)
        {
            var request = BuildRequest(slaveId, 0x01, startAddress, count);
            var response = await SendRequestAsync(request, 7 + (count + 7) / 8);
            return ParseCoilResponse(response, count);
        }

        public async Task<bool[]> ReadDiscreteInputsAsync(byte slaveId, ushort startAddress, ushort count)
        {
            var request = BuildRequest(slaveId, 0x02, startAddress, count);
            var response = await SendRequestAsync(request, 7 + (count + 7) / 8);
            return ParseCoilResponse(response, count);
        }

        public async Task WriteSingleCoilAsync(byte slaveId, ushort address, bool value)
        {
            var request = BuildWriteSingleCoilRequest(slaveId, address, value);
            await SendRequestAsync(request, 12);
        }

        public async Task WriteSingleRegisterAsync(byte slaveId, ushort address, ushort value)
        {
            var request = BuildWriteSingleRegisterRequest(slaveId, address, value);
            await SendRequestAsync(request, 12);
        }

        public async Task WriteMultipleCoilsAsync(byte slaveId, ushort startAddress, bool[] values)
        {
            var request = BuildWriteMultipleCoilsRequest(slaveId, startAddress, values);
            await SendRequestAsync(request, 12);
        }

        public async Task WriteMultipleRegistersAsync(byte slaveId, ushort startAddress, ushort[] values)
        {
            var request = BuildWriteMultipleRegistersRequest(slaveId, startAddress, values);
            await SendRequestAsync(request, 12);
        }

        private byte[] BuildRequest(byte slaveId, byte functionCode, ushort startAddress, ushort count)
        {
            var request = new byte[12];
            
            _transactionId = (_transactionId + 1) % 65536;
            request[0] = (byte)(_transactionId >> 8);
            request[1] = (byte)_transactionId;
            request[2] = 0x00;
            request[3] = 0x00;
            request[4] = 0x00;
            request[5] = 0x06;
            request[6] = slaveId;
            request[7] = functionCode;
            request[8] = (byte)(startAddress >> 8);
            request[9] = (byte)startAddress;
            request[10] = (byte)(count >> 8);
            request[11] = (byte)count;
            
            return request;
        }

        private byte[] BuildWriteSingleCoilRequest(byte slaveId, ushort address, bool value)
        {
            var request = new byte[12];
            
            _transactionId = (_transactionId + 1) % 65536;
            request[0] = (byte)(_transactionId >> 8);
            request[1] = (byte)_transactionId;
            request[2] = 0x00;
            request[3] = 0x00;
            request[4] = 0x00;
            request[5] = 0x06;
            request[6] = slaveId;
            request[7] = 0x05;
            request[8] = (byte)(address >> 8);
            request[9] = (byte)address;
            request[10] = value ? (byte)0xFF : (byte)0x00;
            request[11] = 0x00;
            
            return request;
        }

        private byte[] BuildWriteSingleRegisterRequest(byte slaveId, ushort address, ushort value)
        {
            var request = new byte[12];
            
            _transactionId = (_transactionId + 1) % 65536;
            request[0] = (byte)(_transactionId >> 8);
            request[1] = (byte)_transactionId;
            request[2] = 0x00;
            request[3] = 0x00;
            request[4] = 0x00;
            request[5] = 0x06;
            request[6] = slaveId;
            request[7] = 0x06;
            request[8] = (byte)(address >> 8);
            request[9] = (byte)address;
            request[10] = (byte)(value >> 8);
            request[11] = (byte)value;
            
            return request;
        }

        private byte[] BuildWriteMultipleCoilsRequest(byte slaveId, ushort startAddress, bool[] values)
        {
            var byteCount = (values.Length + 7) / 8;
            var request = new byte[13 + byteCount];
            
            _transactionId = (_transactionId + 1) % 65536;
            request[0] = (byte)(_transactionId >> 8);
            request[1] = (byte)_transactionId;
            request[2] = 0x00;
            request[3] = 0x00;
            request[4] = (byte)((7 + byteCount) >> 8);
            request[5] = (byte)(7 + byteCount);
            request[6] = slaveId;
            request[7] = 0x0F;
            request[8] = (byte)(startAddress >> 8);
            request[9] = (byte)startAddress;
            request[10] = (byte)(values.Length >> 8);
            request[11] = (byte)values.Length;
            request[12] = (byte)byteCount;
            
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    request[13 + i / 8] |= (byte)(1 << (i % 8));
                }
            }
            
            return request;
        }

        private byte[] BuildWriteMultipleRegistersRequest(byte slaveId, ushort startAddress, ushort[] values)
        {
            var request = new byte[11 + values.Length * 2];
            
            _transactionId = (_transactionId + 1) % 65536;
            request[0] = (byte)(_transactionId >> 8);
            request[1] = (byte)_transactionId;
            request[2] = 0x00;
            request[3] = 0x00;
            request[4] = (byte)((5 + values.Length * 2) >> 8);
            request[5] = (byte)(5 + values.Length * 2);
            request[6] = slaveId;
            request[7] = 0x10;
            request[8] = (byte)(startAddress >> 8);
            request[9] = (byte)startAddress;
            request[10] = (byte)(values.Length >> 8);
            request[11] = (byte)values.Length;
            request[12] = (byte)(values.Length * 2);
            
            for (int i = 0; i < values.Length; i++)
            {
                request[13 + i * 2] = (byte)(values[i] >> 8);
                request[14 + i * 2] = (byte)values[i];
            }
            
            return request;
        }

        private async Task<byte[]> SendRequestAsync(byte[] request, int expectedLength)
        {
            if (!IsConnected)
                throw new InvalidOperationException("未连接到设备");

            _networkStream!.Write(request, 0, request.Length);

            var headerBuffer = new byte[6];
            await _networkStream.ReadAsync(headerBuffer, 0, 6);
            
            var length = (headerBuffer[4] << 8) | headerBuffer[5];
            var totalLength = 6 + length;
            
            var response = new byte[totalLength];
            Array.Copy(headerBuffer, response, 6);
            
            if (length > 0)
            {
                await _networkStream.ReadAsync(response, 6, length);
            }

            ValidateResponse(request, response);

            return response;
        }

        private void ValidateResponse(byte[] request, byte[] response)
        {
            if (response.Length < 9)
                throw new InvalidOperationException("响应数据不完整");

            var requestTransactionId = (request[0] << 8) | request[1];
            var responseTransactionId = (response[0] << 8) | response[1];
            
            if (requestTransactionId != responseTransactionId)
                throw new InvalidOperationException("事务ID不匹配");

            if (response[7] != request[7])
            {
                if ((response[7] & 0x80) != 0)
                {
                    var exceptionCode = response[8];
                    throw new InvalidOperationException($"Modbus异常: {GetExceptionMessage(exceptionCode)}");
                }
                throw new InvalidOperationException("功能码不匹配");
            }
        }

        private ushort[] ParseRegisterResponse(byte[] response, ushort count)
        {
            var registers = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                registers[i] = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);
            }
            return registers;
        }

        private bool[] ParseCoilResponse(byte[] response, ushort count)
        {
            var coils = new bool[count];
            for (int i = 0; i < count; i++)
            {
                coils[i] = (response[9 + i / 8] & (1 << (i % 8))) != 0;
            }
            return coils;
        }

        private string GetExceptionMessage(byte exceptionCode)
        {
            return exceptionCode switch
            {
                0x01 => "非法功能码",
                0x02 => "非法数据地址",
                0x03 => "非法数据值",
                0x04 => "从站设备故障",
                0x05 => "确认",
                0x06 => "从站设备忙",
                _ => $"未知异常 (0x{exceptionCode:X2})"
            };
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

        ~ModbusTcpService()
        {
            Dispose(false);
        }
    }
}
