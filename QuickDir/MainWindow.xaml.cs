using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace QuickDir
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Regex parentDirRegex = new Regex(@"[^\\/]*$");

        public MainWindow()
        {
            InitializeComponent();
            txtDirRequest.Focus();
            FillCompletion();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape)
                {
                    if (txtDirRequest.Text.Equals(string.Empty))
                    {
                        this.WindowState = System.Windows.WindowState.Minimized;
                    }
                    else
                    {
                        txtDirRequest.Text = string.Empty;
                    }
                }
                else if (e.Key == Key.Enter)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        ShowDir(e);
                    }
                    else
                    {
                        if(lbCompletion.Items.Count > 0 && lbCompletion.SelectedItem != null)
                        {
                            txtDirRequest.Text = lbCompletion.SelectedItem.ToString();
                            txtDirRequest.Select(txtDirRequest.Text.Length, 0);
                            e.Handled = true;
                        }
                    }
                }
                else if(e.Key == Key.F5)
                {
                    ShowDir(e);
                }
                else if(e.Key == Key.Up && lbCompletion.Items.Count > 0)
                {
                    if(lbCompletion.SelectedIndex > 0)
                    {
                        lbCompletion.SelectedIndex--;
                    }
                    else
                    {
                        lbCompletion.SelectedIndex = lbCompletion.Items.Count - 1;
                    }
                }
                else if(e.Key == Key.Down && lbCompletion.Items.Count > 0)
                {
                    if (lbCompletion.SelectedIndex < lbCompletion.Items.Count - 1)
                    {
                        lbCompletion.SelectedIndex++;
                    }
                    else
                    {
                        lbCompletion.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }

        private void ShowDir(KeyEventArgs e = null)
        {
            if (Directory.Exists(txtDirRequest.Text))
            {
                Process.Start(txtDirRequest.Text);
                if(e != null)
                    e.Handled = true;
            }
        }

        private void txtDirRequest_TextChanged(object sender, TextChangedEventArgs e)
        {
            FillCompletion();
        }

        private void FillCompletion()
        {
            List<string> levels = txtDirRequest.Text.Split('\\', '/').ToList();

            if(levels.Count == 1)
            {
                lbCompletion.ItemsSource = Directory.GetLogicalDrives()
                    .ToList()
                    .FindAll(drive => drive.ToLower().StartsWith(levels[0].ToLower()));
            }
            else
            {
                string parentDir = parentDirRegex.Replace(txtDirRequest.Text, "");
                string searchSubDir = levels.Last();

                lbCompletion.ItemsSource = Directory.GetDirectories(parentDir, searchSubDir + "*")
                    .ToList()
                    .ConvertAll(p => p + @"\");
            }

            if(lbCompletion.Items.Count > 0)
            {
                lbCompletion.SelectedIndex = 0;
            }
        }
    }
}
