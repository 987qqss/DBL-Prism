using Core.Models;
using Infrastructure.Communication;

namespace Infrastructure.Services
{
    public class CommandExecutionService
    {
        private readonly Dictionary<string, IModbusService> _modbusServices = new();
        private readonly Dictionary<string, IS7Service> _s7Services = new();
        private readonly Dictionary<string, object> _protocolServices = new();
        private readonly object _lockObj = new object();

        public async Task<CommandExecutionResult> ExecuteCommandAsync(DeviceCommand command, DeviceModel device)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                object? result = null;
                string? formattedResult = null;
                
                switch (command.CommandType)
                {
                    case CommandType.ModbusRead:
                        result = await ExecuteModbusReadCommand(command, device);
                        formattedResult = FormatModbusResult(result, command);
                        break;
                    case CommandType.ModbusWrite:
                        result = await ExecuteModbusWriteCommand(command, device);
                        formattedResult = "写入成功";
                        break;
                    case CommandType.S7Read:
                        result = await ExecuteS7ReadCommand(command, device);
                        formattedResult = FormatS7Result(result, command);
                        break;
                    case CommandType.S7Write:
                        result = await ExecuteS7WriteCommand(command, device);
                        formattedResult = "写入成功";
                        break;
                    case CommandType.TcpRequest:
                        result = await ExecuteTcpRequestCommand(command, device);
                        formattedResult = result?.ToString();
                        break;
                }
                
                stopwatch.Stop();
                
                return new CommandExecutionResult
                {
                    CommandId = command.Id,
                    DeviceId = device.Id,
                    Success = true,
                    Result = result,
                    FormattedResult = formattedResult,
                    ExecutionTime = DateTime.Now,
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new CommandExecutionResult
                {
                    CommandId = command.Id,
                    DeviceId = device.Id,
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionTime = DateTime.Now,
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        private async Task<object> ExecuteModbusReadCommand(DeviceCommand command, DeviceModel device)
        {
            var service = GetOrCreateModbusService(device);
            
            if (!service.IsConnected)
                throw new InvalidOperationException("设备未连接");

            switch (command.FunctionCode)
            {
                case ModbusFunctionCode.ReadCoils:
                    return await service.ReadCoilsAsync(command.SlaveId, command.StartAddress, command.Quantity);
                case ModbusFunctionCode.ReadDiscreteInputs:
                    return await service.ReadDiscreteInputsAsync(command.SlaveId, command.StartAddress, command.Quantity);
                case ModbusFunctionCode.ReadHoldingRegisters:
                    return await service.ReadHoldingRegistersAsync(command.SlaveId, command.StartAddress, command.Quantity);
                case ModbusFunctionCode.ReadInputRegisters:
                    return await service.ReadInputRegistersAsync(command.SlaveId, command.StartAddress, command.Quantity);
                default:
                    throw new NotSupportedException($"不支持的功能码: {command.FunctionCode}");
            }
        }

        private async Task<object> ExecuteModbusWriteCommand(DeviceCommand command, DeviceModel device)
        {
            var service = GetOrCreateModbusService(device);
            
            if (!service.IsConnected)
                throw new InvalidOperationException("设备未连接");

            switch (command.FunctionCode)
            {
                case ModbusFunctionCode.WriteSingleCoil:
                    await service.WriteSingleCoilAsync(command.SlaveId, command.StartAddress, true);
                    return true;
                case ModbusFunctionCode.WriteSingleRegister:
                    await service.WriteSingleRegisterAsync(command.SlaveId, command.StartAddress, command.StartAddress);
                    return true;
                case ModbusFunctionCode.WriteMultipleCoils:
                    await service.WriteMultipleCoilsAsync(command.SlaveId, command.StartAddress, new bool[command.Quantity]);
                    return true;
                case ModbusFunctionCode.WriteMultipleRegisters:
                    await service.WriteMultipleRegistersAsync(command.SlaveId, command.StartAddress, new ushort[command.Quantity]);
                    return true;
                default:
                    throw new NotSupportedException($"不支持的功能码: {command.FunctionCode}");
            }
        }

        private async Task<object> ExecuteS7ReadCommand(DeviceCommand command, DeviceModel device)
        {
            var service = GetOrCreateS7Service(device);
            
            if (!service.IsConnected)
                throw new InvalidOperationException("设备未连接");

            return await service.ReadDataAsync(command.S7DbNumber, command.S7StartOffset, command.S7Length);
        }

        private async Task<object> ExecuteS7WriteCommand(DeviceCommand command, DeviceModel device)
        {
            var service = GetOrCreateS7Service(device);
            
            if (!service.IsConnected)
                throw new InvalidOperationException("设备未连接");

            byte[] data = new byte[command.S7Length];
            await service.WriteDataAsync(command.S7DbNumber, command.S7StartOffset, data);
            return true;
        }

        private async Task<object> ExecuteTcpRequestCommand(DeviceCommand command, DeviceModel device)
        {
            using var tcpClient = new System.Net.Sockets.TcpClient();
            var result = tcpClient.BeginConnect(device.IpAddress, device.Port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(device.Timeout);
            
            if (!success || !tcpClient.Connected)
                throw new InvalidOperationException("连接失败");

            using var stream = tcpClient.GetStream();
            byte[] requestData = System.Text.Encoding.ASCII.GetBytes(command.TcpCommand);
            await stream.WriteAsync(requestData, 0, requestData.Length);

            byte[] responseBuffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
            
            return System.Text.Encoding.ASCII.GetString(responseBuffer, 0, bytesRead);
        }

        private IModbusService GetOrCreateModbusService(DeviceModel device)
        {
            lock (_lockObj)
            {
                if (!_modbusServices.TryGetValue(device.Id, out var service))
                {
                    service = device.ProtocolType == ProtocolType.ModbusRtu 
                        ? new ModbusRtuService() as IModbusService
                        : new ModbusTcpService();
                    
                    if (device.ProtocolType == ProtocolType.ModbusRtu)
                    {
                        ((ModbusRtuService)service).Connect(device.SerialPortName, device.BaudRate, 
                            ParseParity(device.Parity), device.DataBits, ParseStopBits(device.StopBits));
                    }
                    else
                    {
                        service.Connect(device.IpAddress, device.Port);
                    }
                    
                    _modbusServices[device.Id] = service;
                }
                return service;
            }
        }

        private IS7Service GetOrCreateS7Service(DeviceModel device)
        {
            lock (_lockObj)
            {
                if (!_s7Services.TryGetValue(device.Id, out var service))
                {
                    service = new S7Service();
                    service.Connect(device.IpAddress, device.S7Rack, device.S7Slot);
                    _s7Services[device.Id] = service;
                }
                return service;
            }
        }

        private System.IO.Ports.Parity ParseParity(string parity)
        {
            return Enum.TryParse<System.IO.Ports.Parity>(parity, out var result) ? result : System.IO.Ports.Parity.None;
        }

        private System.IO.Ports.StopBits ParseStopBits(string stopBits)
        {
            return Enum.TryParse<System.IO.Ports.StopBits>(stopBits, out var result) ? result : System.IO.Ports.StopBits.One;
        }

        private string FormatModbusResult(object? result, DeviceCommand command)
        {
            if (result is bool[] boolArray)
            {
                return string.Join(", ", boolArray.Select(b => b ? "1" : "0"));
            }
            if (result is ushort[] ushortArray)
            {
                return string.Join(", ", ushortArray.Select(v => ApplyFormatting(v, command)));
            }
            return result?.ToString() ?? string.Empty;
        }

        private string FormatS7Result(object? result, DeviceCommand command)
        {
            if (result is byte[] byteArray)
            {
                switch (command.S7DataType)
                {
                    case S7DataType.Int:
                        return BitConverter.ToInt16(byteArray, 0).ToString();
                    case S7DataType.DInt:
                        return BitConverter.ToInt32(byteArray, 0).ToString();
                    case S7DataType.Real:
                        return BitConverter.ToSingle(byteArray, 0).ToString();
                    case S7DataType.String:
                        return System.Text.Encoding.ASCII.GetString(byteArray).Trim('\0');
                    default:
                        return BitConverter.ToString(byteArray);
                }
            }
            return result?.ToString() ?? string.Empty;
        }

        private string ApplyFormatting(ushort value, DeviceCommand command)
        {
            float result = value;
            result = result * command.Scale + command.Offset;
            
            return $"{result}{command.Unit}";
        }

        public void DisconnectDevice(string deviceId)
        {
            lock (_lockObj)
            {
                if (_modbusServices.TryGetValue(deviceId, out var modbusService))
                {
                    modbusService.Disconnect();
                    _modbusServices.Remove(deviceId);
                }
                if (_s7Services.TryGetValue(deviceId, out var s7Service))
                {
                    s7Service.Disconnect();
                    _s7Services.Remove(deviceId);
                }
            }
        }

        public void DisconnectAll()
        {
            lock (_lockObj)
            {
                foreach (var service in _modbusServices.Values)
                {
                    service.Disconnect();
                }
                foreach (var service in _s7Services.Values)
                {
                    service.Disconnect();
                }
                _modbusServices.Clear();
                _s7Services.Clear();
            }
        }
    }
}
