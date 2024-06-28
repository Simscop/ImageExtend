using System.Windows.Markup;

[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Controls")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Tools")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Tools.Converter")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Tools.Extension")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Interactivity")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Expression.Shapes")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Expression.Media")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Media.Animation")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Media.Effects")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Data")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Properties.Langs")]
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Lift.UI.Themes")]

#if NET40
[assembly: XmlnsDefinition("https://github.com/pchuan98/Lift.UI", "Microsoft.Windows.Shell")]
#endif
[assembly: XmlnsPrefix("https://github.com/pchuan98/Lift.UI", "liftui")]
