namespace Shell.Views
{
    /// <summary>
    /// 侧栏容器视图 —— 仅提供标题/版本号外壳和 SidebarTreeRegion。
    /// 树形内容由 DeviceModule 的 DeviceCommandTreeView 注入。
    /// 遵循 Prism 模块化原则：Shell 负责容器，Module 负责内容。
    /// </summary>
    public partial class SidebarView
    {
        public SidebarView()
        {
            InitializeComponent();
        }
    }
}
