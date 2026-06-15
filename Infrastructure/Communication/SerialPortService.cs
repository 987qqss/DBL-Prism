using System.IO.Ports;

namespace Infrastructure.Communication
{
    public class SerialPortService : IDisposable
    {
        private SerialPort? _serialPort;
        private readonly object _lockObj = new object();
        private bool _disposed;

        public bool IsOpen => _serialPort?.IsOpen ?? false;

        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public bool Open(string portName, int baudRate = 9600, Parity parity = Parity.None, 
                        int dataBits = 8, StopBits stopBits = StopBits.One, 
                        Handshake handshake = Handshake.None)
        {
            lock (_lockObj)
            {
                try
                {
                    Close();
                    
                    _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
                    {
                        Handshake = handshake,
                        ReadTimeout = 1000,
                        WriteTimeout = 1000,
                        DtrEnable = true,
                        RtsEnable = true
                    };
                    
                    _serialPort.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Close()
        {
            lock (_lockObj)
            {
                try
                {
                    _serialPort?.Close();
                }
                catch
                {
                    // 忽略关闭异常
                }
                finally
                {
                    _serialPort?.Dispose();
                    _serialPort = null;
                }
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            lock (_lockObj)
            {
                if (!IsOpen)
                    throw new InvalidOperationException("串口未打开");
                
                return _serialPort!.Read(buffer, offset, count);
            }
        }

        public int ReadByte()
        {
            lock (_lockObj)
            {
                if (!IsOpen)
                    throw new InvalidOperationException("串口未打开");
                
                return _serialPort!.ReadByte();
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            lock (_lockObj)
            {
                if (!IsOpen)
                    throw new InvalidOperationException("串口未打开");
                
                _serialPort!.Write(buffer, offset, count);
            }
        }

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public int ReadExisting(byte[] buffer)
        {
            lock (_lockObj)
            {
                if (!IsOpen)
                    throw new InvalidOperationException("串口未打开");
                
                var available = _serialPort!.BytesToRead;
                if (available == 0)
                    return 0;
                
                var count = Math.Min(available, buffer.Length);
                return _serialPort.Read(buffer, 0, count);
            }
        }

        public int BytesToRead => _serialPort?.BytesToRead ?? 0;

        public void DiscardInBuffer()
        {
            _serialPort?.DiscardInBuffer();
        }

        public void DiscardOutBuffer()
        {
            _serialPort?.DiscardOutBuffer();
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
                Close();
            }
            
            _disposed = true;
        }

        ~SerialPortService()
        {
            Dispose(false);
        }
    }
}
