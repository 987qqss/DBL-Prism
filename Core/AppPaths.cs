namespace Core;

/// <summary>
/// 应用路径统一管理 —— 所有配置文件/日志统一存放在 D:/DBL 下。
/// 集中定义路径，避免散落各处硬编码。
/// </summary>
public static class AppPaths
{
    /// <summary>系统根目录（所有配置文件的根）</summary>
    public static string RootDir => @"D:\DBL";

    // ─── 子目录 ───

    /// <summary>配置文件目录（数据点配置等 JSON）</summary>
    public static string ConfigDir => Path.Combine(RootDir, "Config");

    /// <summary>日志目录</summary>
    public static string LogDir => Path.Combine(RootDir, "Logs");

    /// <summary>报表目录</summary>
    public static string ReportDir => Path.Combine(RootDir, "Reports");

    /// <summary>外部驱动 DLL 目录（插件）</summary>
    public static string DriverDir => Path.Combine(RootDir, "Drivers");

    /// <summary>导出目录</summary>
    public static string ExportDir => Path.Combine(RootDir, "Export");

    // ─── 具体文件路径 ───

    /// <summary>数据点配置 JSON</summary>
    public static string DataPointsFile => Path.Combine(ConfigDir, "datapoints.json");

    /// <summary>设备树配置 JSON（自动保存的全局配置）</summary>
    public static string DeviceConfigFile => Path.Combine(ConfigDir, "deviceconfig.json");

    /// <summary>用户数据 JSON</summary>
    public static string UsersFile => Path.Combine(ConfigDir, "users.json");

    /// <summary>系统设置 JSON</summary>
    public static string SystemSettingsFile => Path.Combine(ConfigDir, "system.json");

    /// <summary>NLog 日志文件前缀（完整路径由 NLog 拼接日期）</summary>
    public static string NLogFilePrefix => Path.Combine(LogDir, "Log_");

    /// <summary>确保所有目录存在（启动时调用）</summary>
    public static void EnsureDirectories()
    {
        foreach (var dir in new[] { ConfigDir, LogDir, ReportDir, DriverDir, ExportDir })
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
