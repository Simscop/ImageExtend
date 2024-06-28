using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lift.UI.V2.Controls.PropertyGrid;

/// <summary>
/// 专门针对ViewModel的反射帮助类
/// note: 后期可能有其他MVVM框架，也是同样在这里做修改
/// </summary>
internal static class ViewModelReflectionHelper
{
    /// <summary>
    /// 
    /// </summary>
    private const string MvvmToolkitsClassName = "ObservableObject";

    /// <summary>
    /// 配合CommunicateMvvmToolkits
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string Field2Prop(this string name)
        => $"{char.ToUpper(name.Replace("_", "")[0])}{name.Replace("_", "")[1..]}";

    /// <summary>
    /// 判断有没有继承ObservableObject
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsObservableObject(this object obj)
        => obj.GetType().BaseType?.Name == MvvmToolkitsClassName;

    /// <summary>
    /// 新的GetProperties方式，这里面
    /// </summary>
    /// <returns></returns>
    public static MemberInfo[] GetMembers(this object obj)
        => obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(member => member.MemberType is MemberTypes.Field or MemberTypes.Property)
            .Where(member => !member.IsNeedSkip(obj)).ToArray();

    /// <summary>
    /// 当前这个成员是否需要跳过不看
    /// 尤其是自动生成这一部分
    /// </summary>
    /// <param name="info"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsNeedSkip(this MemberInfo info, object obj)
        => obj.IsObservableObject() && (info.IsGeneratedCode() || info.IsDebuggerBrowsable());

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static PropertyGridAttribute? GetPropertyGridAttribute(this MemberInfo info)
        => info.GetCustomAttribute<PropertyGridAttribute>();

    /// <summary>
    /// 判断是不是GeneratedCodeAttribute数据对象
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsGeneratedCode(this MemberInfo info)
        => info.GetCustomAttribute<System.CodeDom.Compiler.GeneratedCodeAttribute>() is not null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsDebuggerBrowsable(this MemberInfo info)
        => info.GetCustomAttribute<System.Diagnostics.DebuggerBrowsableAttribute>() is not null;

    /// <summary>
    /// 获取绑定使用的名称
    /// </summary>
    /// <param name="info"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetBindingName(this MemberInfo info, object obj)
        => obj.IsObservableObject() ? info.Name.Field2Prop() : info.Name;

    public static Type GetValueType(this MemberInfo info) => info switch
    {
        PropertyInfo prop => prop.PropertyType,
        FieldInfo field => field.FieldType,
        _ => throw new Exception("The member not defined.")
    };
}
