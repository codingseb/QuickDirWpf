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
            this.DataContext = Config.Instance;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape)
                {
                    if (txtDirRequest.Text.Equals(string.Empty))
                    {
                        if (Config.Instance.CloseOnEscape)
                            this.Close();
                        else
                            this.WindowState = System.Windows.WindowState.Minimized;
                    }
                    else
                    {
                        txtDirRequest.Text = string.Empty;
                    }
                }
                else if (e.Key == Key.System && e.SystemKey == Key.Enter)
                {
                    CmdOn(e);
                }
                else if (e.Key == Key.Enter)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        ShowDir(e);
                    }
                    else
                    {
                        Validate();
                    }
                }
                else if(e.Key == Key.F5)
                {
                    ShowDir(e);
                }
                else if(e.Key == Key.F6)
                {
                    CmdOn(e);
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
                else if(e.Key == Key.PageDown && lbCompletion.Items.Count > 0)
                {
                    lbCompletion.Focus();
                }
                else if (e.Key == Key.PageUp && lbCompletion.Items.Count > 0)
                {
                    lbCompletion.Focus();
                }
                else
                {
                    if(!txtDirRequest.IsFocused)
                    {
                        txtDirRequest.Focus();
                        txtDirRequest.Select(txtDirRequest.Text.Length, 0);
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

        private void CmdOn(KeyEventArgs e = null)
        {
            if (Directory.Exists(txtDirRequest.Text))
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd");
                psi.WorkingDirectory = txtDirRequest.Text;
                Process.Start(psi);
                if (e != null)
                    e.Handled = true;
            }
        }

        private void txtDirRequest_TextChanged(object sender, TextChangedEventArgs e)
        {
            FillCompletion();
        }

        private void FillCompletion()
        {
            List<AutoCompleteItem> completeList = new List<AutoCompleteItem>();

            try
            {
                if (txtDirRequest.Text.StartsWith(">"))
                {
                    completeList = GetCommands();
                }
                else
                {
                    List<string> levels = txtDirRequest.Text.Split('\\', '/').ToList();
                    string fav = Config.Instance.GetFav(txtDirRequest.Text);

                    if (levels.Count == 1)
                    {
                        completeList = Directory.GetLogicalDrives()
                            .ToList()
                            .FindAll(drive => drive.ToLower().StartsWith(levels[0].ToLower()))
                            .ConvertAll(drive => new AutoCompleteItem(drive));

                        completeList.AddRange(Config.Instance.FavsList
                            .FindAll(e => e.ToLower().StartsWith(levels[0].ToLower()))
                            .OrderBy(e => e)
                            .ToList()
                            .ConvertAll(f => new AutoCompleteItem(f, Config.Instance.GetFav(f), Config.Instance.GetFav(f))));
                    }
                    else
                    {
                        string parentDir = parentDirRegex.Replace(txtDirRequest.Text, "");
                        string searchSubDir = levels.Last();

                        completeList = Directory.GetDirectories(parentDir, searchSubDir + "*")
                            .ToList()
                            .ConvertAll(p => new AutoCompleteItem(p + @"\"));
                    }

                }
            }
            catch
            {}

            try
            {
                lbCompletion.ItemsSource = completeList;
            }
            catch
            {
                lbCompletion.ItemsSource = null;
            }

            if(lbCompletion.Items.Count > 0)
            {
                lbCompletion.SelectedIndex = 0;
            }
        }

        private List<AutoCompleteItem> GetCommands()
        {
            List<AutoCompleteItem> commands = new List<AutoCompleteItem>();

            return commands;
        }

        private void Validate(KeyEventArgs e = null)
        {
            string[] favEqArray = txtDirRequest.Text.Split('=');

            if(favEqArray.Length == 2
                && Directory.Exists(favEqArray[1])
                && !favEqArray[0].Equals(string.Empty))
            {
                if (Config.Instance.SetFav(favEqArray[0], favEqArray[1]))
                    SetFieldValue(favEqArray[0]);
            }
            else if (favEqArray.Length == 2
                && Directory.Exists(favEqArray[0])
                && !favEqArray[1].Equals(string.Empty))
            {
                if (Config.Instance.SetFav(favEqArray[1], favEqArray[0]))
                    SetFieldValue(favEqArray[1]);
            }
            else if (lbCompletion.Items.Count > 0 && lbCompletion.SelectedItem != null)
            {
                SetFieldValue(((AutoCompleteItem)lbCompletion.SelectedItem).AutoComplete);

                if (e != null)
                    e.Handled = true;
            }
        }

        private void SetFieldValue(string value)
        {
            txtDirRequest.Text = value;
            txtDirRequest.Select(value.Length, 0);
            txtDirRequest.Focus();
        }

        private void lbCompletion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbCompletion.ScrollIntoView(lbCompletion.SelectedItem);
            
            if (!txtDirRequest.IsFocused)
            {
                txtDirRequest.Focus();
                txtDirRequest.Select(txtDirRequest.Text.Length, 0);
            }
        }

        private void lbCompletion_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(lbCompletion.SelectedItem != null)
            {
                SetFieldValue(lbCompletion.SelectedItem.ToString());
            }
        }
    }
}
