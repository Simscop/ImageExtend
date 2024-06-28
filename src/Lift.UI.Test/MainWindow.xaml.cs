using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using Lift.UI.V2.Controls.PropertyGrid;

namespace Lift.UI.Test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        var vm = new MainViewModel();



        LiftUiPropertyGrid.SelectedObject = new PropertyItem();
        //LiftUiPropertyGrid.FilterFunc = m => !m.Name.Contains("<") && !m.Name.Contains("_");

        //LiftUiPropertyGrid.AddEditor(PropertyGrid.EditorDictKeys.ReadOnlyWithTextBox, typeof(ReadOnlyWithTextBoxEditor));
        //PdPropertyGrid.AddEditor(PropertyGrid.EditorDictKeys.ReadOnlyWithTextBox, typeof(ReadOnlyWithTextBoxEditor));
    }
}

public static class PropertyDescriptorExtension
{
    public static T? GetAttribute<T>(this PropertyDescriptor pd)
        => (T?) (object?) pd.Attributes.OfType<Attribute>().FirstOrDefault(x => x.GetType() == typeof(T));
}


public partial class MainViewModel : ObservableObject
{
    [PropertyGrid]
    [ObservableProperty]
    private int _age = 1;

    [PropertyGrid(Alias = "Hello")]
    [ObservableProperty]
    private string _name = "kitty";

    [PropertyGrid(Editor = PropertyGrid.EditorDictKeys.ReadOnlyWithTextBlock)]
    [ObservableProperty]
    private bool _male = true;

    [Category("H")]
    [PropertyGrid(GroupName = "AK")]
    public bool IsInit { get; init; }

    [PropertyGrid(Alias = "中文", Tips = "中文不是英语")]
    public int Chinese1 { get; set; }

    [PropertyGrid(Alias = "英语", Tips = "英语不是中文")]
    public int Chinese2 { get; set; }

    [PropertyGrid(Ignore = true)]
    public string Demo { get; set; } = "hellos";
}

public class CoreVm
{

}

