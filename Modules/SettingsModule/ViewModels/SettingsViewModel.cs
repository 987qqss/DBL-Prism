using System.Collections.ObjectModel;
using Core;
using Prism.Commands;
using Prism.Mvvm;

namespace SettingsModule.ViewModels
{
    /// <summary>
    /// 系统设置 ViewModel —— 展示所有配置文件路径。
    /// 每个路径可一键在资源管理器中打开。
    /// </summary>
    public class SettingsViewModel : BindableBase
    {
        /// <summary>路径显示项</summary>
        public class PathItem
        {
            public string Name { get; set; } = "";
            public string Path { get; set; } = "";
            public string Description { get; set; } = "";
        }

        public ObservableCollection<PathItem> PathItems { get; } = new();

        public string RootDir => AppPaths.RootDir;

        public DelegateCommand<PathItem> OpenFolderCommand { get; }
        public DelegateCommand OpenRootCommand { get; }

        public SettingsViewModel()
        {
            OpenFolderCommand = new DelegateCommand<PathItem>(OpenFolder);
            OpenRootCommand = new DelegateCommand(() => OpenDirectory(AppPaths.RootDir));

            // 收集所有配置路径
            PathItems.Add(new PathItem { Name = "数据点配置", Path = AppPaths.DataPointsFile, Description = "监测点配置 JSON" });
            PathItems.Add(new PathItem { Name = "设备树配置", Path = AppPaths.DeviceConfigFile, Description = "产线/设备/命令配置 JSON" });
            PathItems.Add(new PathItem { Name = "用户数据", Path = AppPaths.UsersFile, Description = "登录用户 JSON" });
            PathItems.Add(new PathItem { Name = "系统设置", Path = AppPaths.SystemSettingsFile, Description = "系统参数 JSON" });
            PathItems.Add(new PathItem { Name = "日志目录", Path = AppPaths.LogDir, Description = "NLog 运行日志" });
            PathItems.Add(new PathItem { Name = "报表目录", Path = AppPaths.ReportDir, Description = "测试记录报表" });
            PathItems.Add(new PathItem { Name = "驱动目录", Path = AppPaths.DriverDir, Description = "外部设备驱动 DLL" });
            PathItems.Add(new PathItem { Name = "导出目录", Path = AppPaths.ExportDir, Description = "配置/报表导出" });
        }

        private void OpenFolder(PathItem? item)
        {
            if (item == null) return;
            var dir = System.IO.Directory.Exists(item.Path)
                ? item.Path
                : System.IO.Path.GetDirectoryName(item.Path);
            OpenDirectory(dir);
        }

        private static void OpenDirectory(string? dir)
        {
            if (string.IsNullOrEmpty(dir) || !System.IO.Directory.Exists(dir))
            {
                if (!string.IsNullOrEmpty(dir))
                    System.IO.Directory.CreateDirectory(dir);
            }
            if (!string.IsNullOrEmpty(dir))
                System.Diagnostics.Process.Start("explorer.exe", dir);
        }
    }
}
