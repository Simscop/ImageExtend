using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Lift.UI.Tools;

namespace Lift.UI.Test
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            //ConfigHelper.Instance.SetLang("");
            //var jump = Lift.UI.Properties.Langs.LangProvider.GetLang("Jump");

            //var windows = new Window1();
            //windows.Show();
        }
    }
}
