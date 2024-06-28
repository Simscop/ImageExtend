using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Lift.UI.Core.Extensions;

public static class VisualTreeExtensions
{
    public static T? FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
        var parentObject = VisualTreeHelper.GetParent(child);

        return parentObject switch
        {
            null => null,
            T parent => parent,
            _ => FindParent<T>(parentObject)
        };
    }
}
