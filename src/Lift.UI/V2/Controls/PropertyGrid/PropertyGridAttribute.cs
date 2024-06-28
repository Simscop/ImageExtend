using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lift.UI.V2.Controls.PropertyGrid;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PropertyGridAttribute : Attribute
{
    #region Base prop

    /// <summary>
    /// 组名
    /// </summary>
    public string? GroupName { get; set; } = "DEFAULT";

    /// <summary>
    /// 别名
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// 变量名称用的Tips
    /// </summary>
    public string? Tips { get; set; } = null;

    /// <summary>
    /// Tips单行最长的长度
    ///
    /// -1 代表不分割
    /// </summary>
    public int MaxTipsLength { get; set; } = 50;

    /// <summary>
    /// 是否变量只读
    /// </summary>
    public bool ReadOnly { get; set; } = false;

    /// <summary>
    /// 对应的控件类型，使用string是为了方便扩展
    /// </summary>
    public string? Editor { get; set; }

    /// <summary>
    /// 是否跳过该数据对象
    /// </summary>
    public bool Ignore { get; set; } = false;

    /// <summary>
    /// 整体排序顺序
    /// </summary>
    public int Index { get; set; } = 0;

    /// <summary>
    /// 
    /// </summary>
    public int Delay { get; set; } = 0;

    #endregion

    // todo 附属属性
    // note 附属属性和控件强相关，所以应该放到自定义控件那一部分
}
