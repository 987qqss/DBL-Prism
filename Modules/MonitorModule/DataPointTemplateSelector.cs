using System.Windows;
using System.Windows.Controls;
using Core.Models;
using MonitorModule.ViewModels;

namespace MonitorModule;

/// <summary>
/// 数据点模板选择器 —— 按展示类型自动选择模板。
/// 曲线模板（数值型）/ 指示灯模板（布尔型）/ 文本模板（字符串）。
/// </summary>
public class DataPointTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TrendTemplate { get; set; }
    public DataTemplate? IndicatorTemplate { get; set; }
    public DataTemplate? TextTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is DataPointDisplayModel vm)
        {
            return vm.DisplayType switch
            {
                DataPointDisplayType.TrendChart => TrendTemplate,
                DataPointDisplayType.Indicator => IndicatorTemplate,
                _ => TextTemplate
            };
        }
        return base.SelectTemplate(item, container);
    }
}
