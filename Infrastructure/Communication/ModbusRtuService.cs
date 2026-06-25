using System.IO.Ports;
using System.Text;

namespace Infrastructure.Communication
{
    public class ModbusRtuService : IDisposable
    {
        private readonly SerialPortService _serialPortService;
        private bool _disposed;

        public bool IsConnected => _serialPortService.IsOpen;

        public ModbusRtuService()
        {
            _serialPortService = new SerialPortService();
        }

        public bool Connect(string portName, int baudRate = 9600, Parity parity = Parity.None,
                          int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            return _serialPortService.Open(portName, baudRate, parity, dataBits, stopBits);
        }

        public void Disconnect()
        {
            _serialPortService.Close();
        }

        public async Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort count)
        {
            var request = BuildRequest(slaveId, 0x04, startAddress, count);
            var response = await SendRequestAsync(request, 5 + count * 2);
            return ParseRegisterResponse(response, count);
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort count)
        {
            var request = BuildRequest(slaveId, 0x03, startAddress, count);
            var response = await SendRequestAsync(request, 5 + count * 2);
            return ParseRegisterResponse(response, count);
        }

        public async Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort count)
        {
            var request = BuildRequest(slaveId, 0x01, startAddress, count);
            var response = await SendRequestAsync(request, 5 + (count + 7) / 8);
            return ParseCoilResponse(response, count);
        }

        public async Task<bool[]> ReadDiscreteInputsAsync(byte slaveId, ushort startAddress, ushort count)
        {
            var request = BuildRequest(slaveId, 0x02, startAddress, count);
            var response = await SendRequestAsync(request, 5 + (count + 7) / 8);
            return ParseCoilResponse(response, count);
        }

        public async Task WriteSingleCoilAsync(byte slaveId, ushort address, bool value)
        {
            var request = BuildWriteSingleCoilRequest(slaveId, address, value);
            await SendRequestAsync(request, 8);
        }

        public async Task WriteSingleRegisterAsync(byte slaveId, ushort address, ushort value)
        {
            var request = BuildWriteSingleRegisterRequest(slaveId, address, value);
            await SendRequestAsync(request, 8);
        }

        public async Task WriteMultipleCoilsAsync(byte slaveId, ushort startAddress, bool[] values)
        {
            var request = BuildWriteMultipleCoilsRequest(slaveId, startAddress, values);
            await SendRequestAsync(request, 8);
        }

        public async Task WriteMultipleRegistersAsync(byte slaveId, ushort startAddress, ushort[] values)
        {
            var request = BuildWriteMultipleRegistersRequest(slaveId, startAddress, values);
            await SendRequestAsync(request, 8);
        }

        private byte[] BuildRequest(byte slaveId, byte functionCode, ushort startAddress, ushort count)
        {
            var request = new byte[8];
            request[0] = slaveId;
            request[1] = functionCode;
            request[2] = (byte)(startAddress >> 8);
            request[3] = (byte)startAddress;
            request[4] = (byte)(count >> 8);
            request[5] = (byte)count;
            
            var crc = CalculateCrc(request, 6);
            request[6] = (byte)(crc >> 8);
            request[7] = (byte)crc;
            
            return request;
        }

        private byte[] BuildWriteSingleCoilRequest(byte slaveId, ushort address, bool value)
        {
            var request = new byte[8];
            request[0] = slaveId;
            request[1] = 0x05;
            request[2] = (byte)(address >> 8);
            request[3] = (byte)address;
            request[4] = value ? (byte)0xFF : (byte)0x00;
            request[5] = 0x00;
            
            var crc = CalculateCrc(request, 6);
            request[6] = (byte)(crc >> 8);
            request[7] = (byte)crc;
            
            return request;
        }

        private byte[] BuildWriteSingleRegisterRequest(byte slaveId, ushort address, ushort value)
        {
            var request = new byte[8];
            request[0] = slaveId;
            request[1] = 0x06;
            request[2] = (byte)(address >> 8);
            request[3] = (byte)address;
            request[4] = (byte)(value >> 8);
            request[5] = (byte)value;
            
            var crc = CalculateCrc(request, 6);
            request[6] = (byte)(crc >> 8);
            request[7] = (byte)crc;
            
            return request;
        }

        private byte[] BuildWriteMultipleCoilsRequest(byte slaveId, ushort startAddress, bool[] values)
        {
            var byteCount = (values.Length + 7) / 8;
            var request = new byte[9 + byteCount];
            request[0] = slaveId;
            request[1] = 0x0F;
            request[2] = (byte)(startAddress >> 8);
            request[3] = (byte)startAddress;
            request[4] = (byte)(values.Length >> 8);
            request[5] = (byte)values.Length;
            request[6] = (byte)byteCount;
            
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    request[7 + i / 8] |= (byte)(1 << (i % 8));
                }
            }
            
            var crc = CalculateCrc(request, 7 + byteCount);
            request[7 + byteCount] = (byte)(crc >> 8);
            request[8 + byteCount] = (byte)crc;
            
            return request;
        }

        private byte[] BuildWriteMultipleRegistersRequest(byte slaveId, ushort startAddress, ushort[] values)
        {
            var request = new byte[7 + values.Length * 2 + 2];
            request[0] = slaveId;
            request[1] = 0x10;
            request[2] = (byte)(startAddress >> 8);
            request[3] = (byte)startAddress;
            request[4] = (byte)(values.Length >> 8);
            request[5] = (byte)values.Length;
            request[6] = (byte)(values.Length * 2);
            
            for (int i = 0; i < values.Length; i++)
            {
                request[7 + i * 2] = (byte)(values[i] >> 8);
                request[8 + i * 2] = (byte)values[i];
            }
            
            var crc = CalculateCrc(request, 7 + values.Length * 2);
            request[7 + values.Length * 2] = (byte)(crc >> 8);
            request[8 + values.Length * 2] = (byte)crc;
            
            return request;
        }

        private async Task<byte[]> SendRequestAsync(byte[] request, int expectedLength)
        {
            if (!IsConnected)
                throw new InvalidOperationException("未连接到设备");

            _serialPortService.DiscardInBuffer();
            _serialPortService.Write(request);

            await Task.Delay(50);

            var buffer = new byte[256];
            var totalBytes = 0;
            var maxAttempts = 10;
            var attempts = 0;

            while (totalBytes < expectedLength && attempts < maxAttempts)
            {
                if (_serialPortService.BytesToRead > 0)
                {
                    var bytesRead = _serialPortService.Read(buffer, totalBytes, buffer.Length - totalBytes);
                    totalBytes += bytesRead;
                }
                else
                {
                    await Task.Delay(50);
                    attempts++;
                }
            }

            if (totalBytes < expectedLength)
                throw new TimeoutException("读取超时");

            var response = new byte[totalBytes];
            Array.Copy(buffer, response, totalBytes);

            ValidateResponse(response);

            return response;
        }

        private void ValidateResponse(byte[] response)
        {
            if (response.Length < 5)
                throw new InvalidOperationException("响应数据不完整");

            var expectedCrc = CalculateCrc(response, response.Length - 2);
            var actualCrc = (ushort)((response[response.Length - 2] << 8) | response[response.Length - 1]);

            if (expectedCrc != actualCrc)
                throw new InvalidOperationException("CRC校验失败");

            if ((response[1] & 0x80) != 0)
            {
                var exceptionCode = response[2];
                throw new InvalidOperationException($"Modbus异常: {GetExceptionMessage(exceptionCode)}");
            }
        }

        private ushort CalculateCrc(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 0x0001) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                }
            }
            
            return crc;
        }

        private ushort[] ParseRegisterResponse(byte[] response, ushort count)
        {
            var registers = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                registers[i] = (ushort)((response[3 + i * 2] << 8) | response[4 + i * 2]);
            }
            return registers;
        }

        private bool[] ParseCoilResponse(byte[] response, ushort count)
        {
            var coils = new bool[count];
            for (int i = 0; i < count; i++)
            {
                coils[i] = (response[3 + i / 8] & (1 << (i % 8))) != 0;
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
                _serialPortService.Dispose();
            }
            
            _disposed = true;
        }

        ~ModbusRtuService()
        {
            Dispose(false);
        }
    }
}
