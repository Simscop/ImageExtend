using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Lift.UI.V2.Controls.PropertyGrid;

/// <summary>
/// 这个应该和Attribute相关联
/// </summary>
public class PropertyItem : ListBoxItem
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(object), typeof(PropertyItem), new PropertyMetadata(default(object)));

    /// <summary>
    /// ViewModel原始对象
    /// </summary>
    public object Value
    {
        get => (object) GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
        nameof(PropertyName), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    /// <summary>
    /// 我当前需要关注的属性名称
    /// </summary>
    public string PropertyName
    {
        get => (string) GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
        nameof(DisplayName), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string DisplayName
    {
        get => (string) GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public static readonly DependencyProperty EditorProperty = DependencyProperty.Register(
        nameof(Editor), typeof(BasePropertyEditor), typeof(PropertyItem), new PropertyMetadata(null));

    /// <summary>
    /// 数据类型对应使用的编辑窗口类型
    /// </summary>
    public BasePropertyEditor? Editor
    {
        get => (BasePropertyEditor?) GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    public static readonly DependencyProperty EditorElementProperty = DependencyProperty.Register(
        nameof(EditorElement), typeof(FrameworkElement), typeof(PropertyItem), new PropertyMetadata(default(FrameworkElement)));

    /// <summary>
    /// 有编辑窗口衍生的实际WPF控件元素
    /// </summary>
    public FrameworkElement EditorElement
    {
        get => (FrameworkElement) GetValue(EditorElementProperty);
        set => SetValue(EditorElementProperty, value);
    }

    public static readonly DependencyProperty PropertyGridAttributeProperty = DependencyProperty.Register(
        nameof(PropertyGridAttribute), typeof(PropertyGridAttribute), typeof(PropertyItem), new PropertyMetadata(default(PropertyGridAttribute)));

    public static readonly DependencyProperty TipsProperty = DependencyProperty.Register(
        nameof(Tips), typeof(string), typeof(PropertyItem), new PropertyMetadata(null));

    public string? Tips
    {
        get => (string?) GetValue(TipsProperty);
        set => SetValue(TipsProperty, value);
    }

    /// <summary>
    /// 属性的外围修饰
    /// </summary>
    public PropertyGridAttribute PropertyGridAttribute
    {
        get => (PropertyGridAttribute) GetValue(PropertyGridAttributeProperty);
        set => SetValue(PropertyGridAttributeProperty, value);
    }

    public static readonly DependencyProperty BindingTypeProperty = DependencyProperty.Register(
        nameof(BindingType), typeof(Type), typeof(PropertyItem), new PropertyMetadata(default(Type)));

    /// <summary>
    /// 需要绑定的数据原始类型，用来做默认数据处理
    /// </summary>
    public Type BindingType
    {
        get => (Type) GetValue(BindingTypeProperty);
        set => SetValue(BindingTypeProperty, value);
    }

    /// <summary>
    /// 初始化显示使用的控件元素
    /// </summary>
    public virtual void InitElement()
    {
        if (Editor == null) return;
        EditorElement = Editor.CreateElement(this);
        Editor.CreateBinding(this, EditorElement);

        Tips = SplitString(PropertyGridAttribute?.Tips, PropertyGridAttribute?.MaxTipsLength ?? -1);
    }

    private string? SplitString(string? tips, int lenght)
        => lenght < 0 || tips is null
            ? tips
            : string.Join(Environment.NewLine,
                Enumerable.Range(0, (int) Math.Ceiling((double) tips.Length / lenght))
                .Select(i => tips.Substring(i * lenght, Math.Min(lenght, tips.Length - i * lenght)))
                .ToArray());

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="vm"></param>
    /// <returns></returns>
    public static PropertyItem FromMemberInfo(MemberInfo info, object vm) => new PropertyItem()
    {
        Value = vm,
        PropertyName = info.GetBindingName(vm),
        PropertyGridAttribute = info.GetPropertyGridAttribute() ?? new PropertyGridAttribute(),
        Editor = null,
        DisplayName = info.GetPropertyGridAttribute()?.Alias ?? info.GetBindingName(vm),
        BindingType = info.GetValueType()
    };
}
