using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lift.UI.V2;

/// <summary>
/// 
/// </summary>
public static class ControlCommands
{
    /// <summary>
    /// 搜索
    /// </summary>
    public static RoutedCommand Search { get; } = new(nameof(Search), typeof(ControlCommands));

    /// <summary>
    /// 按照类别排序
    /// </summary>
    public static RoutedCommand SortByCategory { get; } = new(nameof(SortByCategory), typeof(ControlCommands));

    /// <summary>
    /// 按照名称排序
    /// </summary>
    public static RoutedCommand SortByName { get; } = new(nameof(SortByName), typeof(ControlCommands));
}
