﻿using System;
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
namespace QuickDir
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Regex parentDirRegex = new Regex(@"[^\\/]*$");

        private static MainWindow instance = null;

        public static MainWindow Instance
        {
            get
            {
                return instance;
            }
        }

        internal MainWindow()
        {
            InitializeComponent();
            instance = this;
            QDCommands.QDCommandFinished += QDCommands_QDCommandFinished;
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
                    if (Popup.IsActive)
                        Popup.Close();
                    else if (txtDirRequest.Text.Equals(string.Empty))
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

        void QDCommands_QDCommandFinished(object sender, EventArgs e)
        {
            txtDirRequest.Text = string.Empty;
        }

        private bool IsCommand()
        {
            return txtDirRequest.Text.StartsWith(">");
        }

        private void ShowDir(KeyEventArgs e = null)
        {
            if (Directory.Exists(txtDirRequest.Text))
            {
                Process.Start(txtDirRequest.Text);
                OnDirAction();
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
                OnDirAction();
                if (e != null)
                    e.Handled = true;
            }
        }

        private void OnDirAction()
        {
            if(Config.Instance.EmptyOnValidate)
                txtDirRequest.Text = string.Empty;
            if (Config.Instance.MinimizeOnValidate)
                this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void txtDirRequest_TextChanged(object sender, TextChangedEventArgs e)
        {
            FillCompletion();
        }

        private void FillCompletion()
        {
            List<AutoCompleteItem> completeList = new List<AutoCompleteItem>();
            Popup.Close();

            try
            {
                if (IsCommand())
                {
                    completeList = GetCommands();
                }
                else
                {
                    string path = txtDirRequest.Text.Replace("/", @"\");
                    List<string> levels = path.Split('\\').ToList();
                    string fav = Config.Instance.GetFav(txtDirRequest.Text);
                    string findTextPattern = SmartSearch.GetRegexSmartSearchPattern(txtDirRequest.Text);

                    if (levels.Count == 1)
                    {
                        completeList = Directory.GetLogicalDrives()
                            .ToList()
                            .FindAll(drive => Config.Instance.IsManaged(drive) 
                                && drive.ToLower().StartsWith(levels[0].ToLower()))
                            .ConvertAll(drive => new AutoCompleteItem(drive));

                        if (Config.Instance.SearchFavByKeys)
                        {
                            completeList.AddRange(Config.Instance.FavsList
                                .FindAll(e => e.ToLower().StartsWith(levels[0].ToLower()))
                                .OrderBy(e => e)
                                .ToList()
                                .ConvertAll(f => new AutoCompleteItem(f, Config.Instance.GetFav(f), Config.Instance.GetFav(f))));
                        }
                    }
                    else
                    {
                        if(!levels[0].Contains(":") && Directory.GetLogicalDrives().Contains(levels[0].ToUpper() + @":\"))
                        {
                            path = Regex.Replace(path, @"^[^\\]+", levels[0].ToUpper() + ":");
                        }

                        string parentDir = parentDirRegex.Replace(path, "");
                        string searchSubDir = levels.Last();

                        Config.Instance.SetHistoryEntry(parentDir);

                        completeList = Directory.GetDirectories(parentDir, searchSubDir + "*")
                            .ToList()
                            .FindAll(p => Config.Instance.IsManaged(p))
                            .ConvertAll(p => new AutoCompleteItem(p + @"\"));

                        if (Config.Instance.SmartSearchOnDirectories)
                        {
                            completeList.AddRange(Directory.GetDirectories(parentDir)
                                .ToList()
                                .FindAll(e => Regex.IsMatch(e.ToLower(), findTextPattern.ToLower())
                                    && completeList.Find(f => f.AutoComplete.ToLower().Equals(e.ToLower() + @"\")) == null)
                                .OrderBy(e => e)
                                .ToList()
                                .ConvertAll(d => new AutoCompleteItem(d + @"\")));
                        }
                    }

                    if(Config.Instance.SmartSearchOnFavs)
                    {
                        completeList.AddRange(Config.Instance.FavsList
                            .FindAll(e => 
                                Regex.IsMatch(Config.Instance.GetFav(e).ToLower(), findTextPattern.ToLower())
                                && completeList.Find(a => a.MainText.ToLower().Equals(e.ToLower())) == null
                                && !Config.Instance.GetFav(e).ToLower().ToLower().Equals(txtDirRequest.Text.ToLower()))
                            .OrderBy(e => e)
                            .ToList()
                            .ConvertAll(f => new AutoCompleteItem(f, Config.Instance.GetFav(f), Config.Instance.GetFav(f))));
                    }

                    if (Config.Instance.SmartHistory)
                    {
                        completeList.AddRange(Config.Instance.History
                            .FindAll(e =>
                                    Regex.IsMatch(e.ToLower(), findTextPattern.ToLower())
                                        && !e.ToLower().Equals(txtDirRequest.Text.ToLower())
                                        && completeList.Find(f => f.AutoComplete.ToLower().Equals(e.ToLower())) == null
                                )
                            .OrderBy(e => e)
                            .ToList()
                            .ConvertAll(h => new AutoCompleteItem(h)));
                    }
                }
            }
            catch(DirectoryNotFoundException ex)
            {
                Popup.Show(ex.Message, true);
            }
            catch(Exception ex)
            {
                Popup.Show(ex.Message);
            }

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
            return QDCommands.FindCommands(txtDirRequest.Text.TrimStart('>').Trim())
                .ConvertAll(command => new AutoCompleteItem(">", command, command));
        }

        private void Validate(KeyEventArgs e = null)
        {
            string[] favEqArray = txtDirRequest.Text.Split('=');

            if (favEqArray.Length == 2
                && (Directory.Exists(favEqArray[1])
                    || favEqArray[1].Equals(string.Empty)))
            {
                if (Config.Instance.SetFav(favEqArray[0], favEqArray[1]))
                    SetFieldValue(favEqArray[0]);
            }
            else if (favEqArray.Length == 2
                && (Directory.Exists(favEqArray[0])
                    || favEqArray[0].Equals(string.Empty)))
            {
                if (Config.Instance.SetFav(favEqArray[1], favEqArray[0]))
                    SetFieldValue(favEqArray[1]);
            }
            else if (lbCompletion.Items.Count > 0)
            {
                ValidateAutoComplete();

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
            ValidateAutoComplete();
        }

        private void ValidateAutoComplete()
        {
            try
            {
                if (lbCompletion.SelectedItem != null)
                {
                    if (IsCommand() && lbCompletion.SelectedItem != null)
                    {
                        try
                        {
                            QDCommands.Execute((lbCompletion.SelectedItem as AutoCompleteItem).AutoComplete);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Une erreur est survenue : " + ex.Message);
                        }
                    }
                    else
                    {
                        SetFieldValue((lbCompletion.SelectedItem as AutoCompleteItem).AutoComplete);
                    }
                }
            }
            catch { }
        }

        private void btnPopupClose_Click(object sender, RoutedEventArgs e)
        {
            Popup.Close();            
        }

        private void btnCreateDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(txtDirRequest.Text);
            }
            catch { }
            FillCompletion();
        }

    }
}
