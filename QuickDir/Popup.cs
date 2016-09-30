using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDir
{
    public class Popup
    {
        public static void Show(string message, bool withCreateButton=false)
        {
            MainWindow.Instance.lblStatus.Content = message;
            MainWindow.Instance.lblStatus.Visibility = System.Windows.Visibility.Visible;
            MainWindow.Instance.btnPopupClose.Visibility = System.Windows.Visibility.Visible;
            MainWindow.Instance.btnCreateDir.Visibility = withCreateButton ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public static void Close()
        {
            MainWindow.Instance.lblStatus.Visibility = System.Windows.Visibility.Collapsed;
            MainWindow.Instance.btnPopupClose.Visibility = System.Windows.Visibility.Collapsed;
            MainWindow.Instance.btnCreateDir.Visibility = System.Windows.Visibility.Collapsed;
        }

        public static bool IsActive
        {
            get
            {
                return MainWindow.Instance.lblStatus.Visibility == System.Windows.Visibility.Visible;
            }
        }
    }
}
