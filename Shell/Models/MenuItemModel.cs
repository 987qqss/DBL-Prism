using Prism.Commands;

namespace Shell.Models
{
    /// <summary>
    /// 菜单项数据模型（用于菜单下拉项）
    /// </summary>
    public class MenuItemModel
    {
        /// <summary>菜单项标题</summary>
        public string Header { get; set; } = string.Empty;

        /// <summary>Material Design 图标（Kind 名称）</summary>
        public string IconKind { get; set; } = string.Empty;

        /// <summary>点击触发的命令</summary>
        public DelegateCommand Command { get; set; } = null!;

        /// <summary>快捷键提示文本</summary>
        public string InputGestureText { get; set; } = string.Empty;

        /// <summary>是否分隔线</summary>
        public bool IsSeparator { get; set; }

        /// <summary>子项（用于多级菜单，可选）</summary>
        public System.Collections.Generic.List<MenuItemModel> Children { get; set; } = new();
    }
}
