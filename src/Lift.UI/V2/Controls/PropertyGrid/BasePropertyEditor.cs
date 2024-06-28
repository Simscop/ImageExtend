using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lift.UI.Controls;
using System.Windows.Data;
using System.Windows;

namespace Lift.UI.V2.Controls.PropertyGrid;

/// <summary>
/// 基础的编辑对象
/// </summary>
public abstract class BasePropertyEditor : DependencyObject
{
    /// <summary>
    /// 使用PropertyItem去创建一个Framework
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <returns></returns>
    public abstract FrameworkElement CreateElement(PropertyItem propertyItem);

    /// <summary>
    /// 手动绑定属性
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <param name="element"></param>
    public virtual void CreateBinding(PropertyItem propertyItem, DependencyObject element) =>
        BindingOperations.SetBinding(element, GetDependencyProperty(),
            new Binding($"{propertyItem.PropertyName}")
            {
                Source = propertyItem.Value,
                Mode = GetBindingMode(propertyItem),
                UpdateSourceTrigger = GetUpdateSourceTrigger(propertyItem),
                Converter = GetConverter(propertyItem),
                Delay = propertyItem.PropertyGridAttribute.Delay
            });

    /// <summary>
    /// 用来自定义该对象使用什么控件来操作
    /// </summary>
    /// <returns></returns>
    public abstract DependencyProperty GetDependencyProperty();

    /// <summary>
    /// 确定绑定方式
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <returns></returns>
    public virtual BindingMode GetBindingMode(PropertyItem propertyItem) =>
        propertyItem.PropertyGridAttribute?.ReadOnly is false ? BindingMode.TwoWay : BindingMode.OneWay;

    /// <summary>
    /// 默认当属性改变的时候更新数据源
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <returns></returns>
    public virtual UpdateSourceTrigger GetUpdateSourceTrigger(PropertyItem propertyItem) =>
        UpdateSourceTrigger.PropertyChanged;

    /// <summary>
    /// 默认不进行数据转换
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <returns></returns>
    protected virtual IValueConverter? GetConverter(PropertyItem propertyItem)
        => null;
}
