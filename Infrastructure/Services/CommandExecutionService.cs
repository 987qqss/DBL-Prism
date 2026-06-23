using Core.Interfaces;
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
                    case CommandType.Read:
                        if (device.ProtocolType == ProtocolType.S7)
                        {
                            result = await ExecuteS7ReadCommand(command, device);
                            formattedResult = FormatS7Result(result, command);
                        }
                        else
                        {
                            result = await ExecuteModbusReadCommand(command, device);
                            formattedResult = FormatModbusResult(result, command);
                        }
                        break;
                    case CommandType.Write:
                        if (device.ProtocolType == ProtocolType.S7)
                        {
                            result = await ExecuteS7WriteCommand(command, device);
                            formattedResult = "写入成功";
                        }
                        else
                        {
                            result = await ExecuteModbusWriteCommand(command, device);
                            formattedResult = "写入成功";
                        }
                        break;
                    case CommandType.Custom:
                        result = await ExecuteTcpRequestCommand(command, device);
                        formattedResult = result?.ToString();
                        break;
                }
                
                stopwatch.Stop();
                
                return new CommandExecutionResult
                {
                    Success = true,
                    Data = result,
                    FormattedResult = formattedResult,
                    ExecutionTime = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new CommandExecutionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionTime = stopwatch.ElapsedMilliseconds
                };
            }
        }

        private async Task<object> ExecuteModbusReadCommand(DeviceCommand command, DeviceModel device)
        {
            var service = GetOrCreateModbusService(device);
            var config = device.Config as ModbusTCPModel ?? new ModbusTCPModel();
            
            if (!service.IsConnected)
                throw new InvalidOperationException("设备未连接");

            switch (command.OperationCode)
            {
                case 0x01: // ReadCoils
                    return await service.ReadCoilsAsync(config.SlaveId, command.Address, command.Length);
                case 0x02: // ReadDiscreteInputs
                    return await service.ReadDiscreteInputsAsync(config.SlaveId, command.Address, command.Length);
                case 0x03: // ReadHoldingRegisters
                    return await service.ReadHoldingRegistersAsync(config.SlaveId, command.Address, command.Length);
                case 0x04: // ReadInputRegisters
                    return await service.ReadInputRegistersAsync(config.SlaveId, command.Address, command.Length);
                default:
                    throw new NotSupportedException($"不支持的功能码: {command.OperationCode}");
            }
        }

        private async Task<object> ExecuteModbusWriteCommand(DeviceCommand command, DeviceModel device)
        {
            var service = GetOrCreateModbusService(device);
            var config = device.Config as ModbusTCPModel ?? new ModbusTCPModel();
            
            if (!service.IsConnected)
                throw new InvalidOperationException("设备未连接");

            switch (command.OperationCode)
            {
                case 0x05: // WriteSingleCoil
                    await service.WriteSingleCoilAsync(config.SlaveId, command.Address, true);
                    return true;
                case 0x06: // WriteSingleRegister
                    await service.WriteSingleRegisterAsync(config.SlaveId, command.Address, command.Address);
                    return true;
                case 0x0F: // WriteMultipleCoils
                    await service.WriteMultipleCoilsAsync(config.SlaveId, command.Address, new bool[command.Length]);
                    return true;
                case 0x10: // WriteMultipleRegisters
                    await service.WriteMultipleRegistersAsync(config.SlaveId, command.Address, new ushort[command.Length]);
                    return true;
                default:
                    throw new NotSupportedException($"不支持的功能码: {command.OperationCode}");
            }
        }

        private async Task<object> ExecuteS7ReadCommand(DeviceCommand command, DeviceModel device)
        {
            var service = GetOrCreateS7Service(device);
            
            if (!service.IsConnected)
                throw new InvalidOperationException("设备未连接");

            // 从命令参数中获取 S7 DB 块信息（使用 Parameters 或直接用 Address/Length）
            var dbNumber = GetS7DbNumber(command);
            var startOffset = (int)command.Address;
            var length = (int)command.Length;

            return await service.ReadDataAsync(dbNumber, startOffset, length);
        }

        private async Task<object> ExecuteS7WriteCommand(DeviceCommand command, DeviceModel device)
        {
            var service = GetOrCreateS7Service(device);
            
            if (!service.IsConnected)
                throw new InvalidOperationException("设备未连接");

            var dbNumber = GetS7DbNumber(command);
            var startOffset = (int)command.Address;
            byte[] data = new byte[command.Length];
            await service.WriteDataAsync(dbNumber, startOffset, data);
            return true;
        }

        private async Task<object> ExecuteTcpRequestCommand(DeviceCommand command, DeviceModel device)
        {
            var config = device.Config as TCPIPModel ?? new TCPIPModel();
            using var tcpClient = new System.Net.Sockets.TcpClient();
            var result = tcpClient.BeginConnect(config.IpAddress, config.Port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(config.Timeout);
            
            if (!success || !tcpClient.Connected)
                throw new InvalidOperationException("连接失败");

            using var stream = tcpClient.GetStream();
            byte[] requestData = System.Text.Encoding.ASCII.GetBytes(command.RequestData);
            await stream.WriteAsync(requestData, 0, requestData.Length);

            byte[] responseBuffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
            
            return System.Text.Encoding.ASCII.GetString(responseBuffer, 0, bytesRead);
        }

        private int GetS7DbNumber(DeviceCommand command)
        {
            // 从命令参数中查找 DB 块号
            var dbParam = command.Parameters.FirstOrDefault(p => p.Name == "DBNumber");
            if (dbParam != null && int.TryParse(dbParam.Value, out var dbNumber))
                return dbNumber;
            
            // 默认使用 Address 作为 DB 块号
            return command.Address > 0 ? command.Address : 1;
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
                        var config = device.Config as ModbusRTUModel ?? new ModbusRTUModel();
                        ((ModbusRtuService)service).Connect(config.SerialPortName, config.BaudRate, 
                            MapParity(config.Parity), config.DataBits, MapStopBits(config.StopBits));
                    }
                    else
                    {
                        var config = device.Config as ModbusTCPModel ?? new ModbusTCPModel();
                        service.Connect(config.IpAddress, config.Port);
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
                    var config = device.Config as S7Model ?? new S7Model();
                    service.Connect(config.IpAddress, config.Rack, config.Slot);
                    _s7Services[device.Id] = service;
                }
                return service;
            }
        }

        private static System.IO.Ports.Parity MapParity(SerialParity parity) => parity switch
        {
            SerialParity.None => System.IO.Ports.Parity.None,
            SerialParity.Odd => System.IO.Ports.Parity.Odd,
            SerialParity.Even => System.IO.Ports.Parity.Even,
            SerialParity.Mark => System.IO.Ports.Parity.Mark,
            SerialParity.Space => System.IO.Ports.Parity.Space,
            _ => System.IO.Ports.Parity.None
        };

        private static System.IO.Ports.StopBits MapStopBits(SerialStopBits stopBits) => stopBits switch
        {
            SerialStopBits.One => System.IO.Ports.StopBits.One,
            SerialStopBits.OnePointFive => System.IO.Ports.StopBits.OnePointFive,
            SerialStopBits.Two => System.IO.Ports.StopBits.Two,
            _ => System.IO.Ports.StopBits.One
        };

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
                switch (command.DataFormat)
                {
                    case DataFormat.Int16:
                        return BitConverter.ToInt16(byteArray, 0).ToString();
                    case DataFormat.Int32:
                        return BitConverter.ToInt32(byteArray, 0).ToString();
                    case DataFormat.Float:
                        return BitConverter.ToSingle(byteArray, 0).ToString();
                    case DataFormat.String:
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
