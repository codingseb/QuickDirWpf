using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace QuickDir
{
    public class QDCommands
    {
        public static event EventHandler QDCommandFinished;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        protected delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        protected static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;

        public static void Execute(string commandText)
        {
            if(commandsList.Find(command => command.Text.Equals(commandText))
                .Action(commandText))
            {
                QDCommandFinished(commandText, new EventArgs());
            }
        }

        public static List<string> FindCommands(string text)
        {
            string findTextPattern = SmartSearch.GetRegexSmartSearchPattern(text.Split(':')[0]);

            return commandsList
                .FindAll(command => Regex.IsMatch(command.Text.ToLower(), findTextPattern.ToLower()))
                .ConvertAll(command => command.Text);
        }

        private class QDCommand
        {
            internal string Text
            {
                get;
                set;
            }

            internal Func<string, bool> Action
            {
                get;
                set;
            }

            internal QDCommand(string text, Func<string, bool> action)
            {
                this.Text = text;
                this.Action = action;
            }
        }

        private static string GetClassName(IntPtr hwnd)
        {
            int nRet;
            // Pre-allocate 256 characters, since this is the maximum class name length.
            StringBuilder className = new StringBuilder(100);
            //Get the window class name
            nRet = GetClassName(hwnd, className, className.Capacity);
            if (nRet != 0)
            {
                return className.ToString();
            }
            else
            {
                return "";
            }
        }

        private static void CloseAllWindowsWithClassName(string className)
        {
            EnumWindows(delegate (IntPtr hwnd, IntPtr lParam) {

                if (GetClassName(hwnd).Equals(className))
                {
                    SendMessage(hwnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                }

                return true;
            }
            , IntPtr.Zero);
        }

        private static bool GetBoolFromString(string stringValue)
        {
            string sResult = stringValue.Trim(' ', '\t');
            bool result = sResult.Equals("1")
                || sResult.ToLower().Equals("yes")
                || sResult.ToLower().Equals("on")
                || sResult.ToLower().Equals("true") ? true :
                (sResult.Equals("0")
                || sResult.ToLower().Equals("no")
                || sResult.ToLower().Equals("off")
                || sResult.ToLower().Equals("false") ? false : bool.Parse(sResult));

            return result;
        }

        private static void SetBoolConfig(string configName, bool value)
        {
            Config.Instance.GetType().GetProperty(configName)
                .SetValue(Config.Instance, value);
        }

        private static bool GetBoolConfig(string configName)
        {
            return (bool)Config.Instance.GetType().GetProperty(configName)
                .GetValue(Config.Instance);
        }

        private static bool SetBoolCommand(string name, string configName)
        {
                TextBox txtField = MainWindow.Instance.txtDirRequest;
                string[] commandsArgsArray = MainWindow.Instance.txtDirRequest.Text.Split(':');

                if(commandsArgsArray.Length > 1)
                {
                    try
                    {
                        SetBoolConfig(configName, GetBoolFromString(commandsArgsArray[1]));
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Impossible to attribute the " + name + " value. " + ex.Message);
                    }

                    return true;
                }
                else
                {
                    string current = GetBoolConfig(configName).ToString();

                    txtField.Text = "> " + name + ":" + current;
                    txtField.Select(txtField.Text.Length - current.Length, current.Length);
                    return false;
                }
        }

        private static void SetIntConfig(string configName, int value)
        {
            Config.Instance.GetType().GetProperty(configName)
                .SetValue(Config.Instance, value);
        }

        private static int GetIntConfig(string configName)
        {
            return (int)Config.Instance.GetType().GetProperty(configName)
                .GetValue(Config.Instance);
        }

        private static bool SetIntCommand(string name, string configName)
        {
            TextBox txtField = MainWindow.Instance.txtDirRequest;
            string[] commandsArgsArray = MainWindow.Instance.txtDirRequest.Text.Split(':');

            if (commandsArgsArray.Length > 1)
            {
                try
                {
                    int width = int.Parse(commandsArgsArray[1].Trim(' ', '\t'));

                    SetIntConfig(configName, width);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Impossible to set the width. " + ex.Message);
                }

                return true;
            }
            else
            {
                string currentWidth = GetIntConfig(configName).ToString();

                txtField.Text = "> " + name + ":" + currentWidth;
                txtField.Select(txtField.Text.Length - currentWidth.Length, currentWidth.Length);
                return false;
            }
        }

        private static List<QDCommand> commandsList = (new QDCommand[]
        {
            new QDCommand("Set Close on Escape key press" , delegate(string name)
            {
                Config.Instance.CloseOnEscape = true;
                MessageBox.Show("A press on [Escape] when the field is empty will now close the application.");
                return true;
            }),
            new QDCommand("Set Minimize on Escape key press" , delegate(string name)
            {
                Config.Instance.CloseOnEscape = false;
                MessageBox.Show("A press on [Escape] when the field is empty will now minimize the application.");
                return true;
            }),
            new QDCommand("Close all explorer windows", delegate(string name)
            {
                CloseAllWindowsWithClassName("CabinetWClass");
                return true;
            }),
            new QDCommand("Close all cmd terminal windows", delegate(string name)
            {
                CloseAllWindowsWithClassName("ConsoleWindowClass");
                return true;
            }),
            new QDCommand("Set Field Width", delegate(string name)
            {
                return SetIntCommand(name, "Width");
            }),
            new QDCommand("Set Max Height", delegate(string name)
            {
                return SetIntCommand(name, "MaxHeight");
            }),
            new QDCommand("Activate Search Favs by Keys", delegate(string name)
            {
                return SetBoolCommand(name, "SearchFavByKeys");
            }),
            new QDCommand("Activate SmartSearch on Favs", delegate(string name)
            {
                return SetBoolCommand(name, "SmartSearchOnFavs");
            }),
            new QDCommand("Activate Smart History", delegate(string name)
            {
                return SetBoolCommand(name, "SmartHistory");
            }),
            new QDCommand("Clear Smart History", delegate(string name)
            {
                if(MessageBox.Show("It will empty the history. Do you validate ?", "Clear", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    Config.Instance.ClearHistory();

                return true;
            }),
            new QDCommand("Activate SmartSearch on Directories", delegate(string name)
            {
                return SetBoolCommand(name, "SmartSearchOnDirectories");
            }),
            new QDCommand("Minimize On Validation", delegate(string name)
            {
                return SetBoolCommand(name, "MinimizeOnValidate");
            }),            
            new QDCommand("Empty Field On Validation", delegate(string name)
            {
                return SetBoolCommand(name, "EmptyOnValidate");
            }),
            new QDCommand("Exit", delegate(string name){
                MainWindow.Instance.Close();
                return true;
            }),
        })
        .OrderBy(command => command.Text)
        .ToList();

        private static void GetSubDir(string current, List<string> index)
        {
            if (Config.Instance.IsManaged(current))
            {
                try
                {
                    if (Directory.GetDirectories(current).Length > 0)
                    {
                        Directory.GetDirectories(current).ToList()
                            .ForEach(delegate(string dir)
                            {
                                GetSubDir(dir, index);
                            });
                    }
                    else
                    {
                        index.Add(current);
                    }
                }
                catch
                {
                    index.Add(current);
                }
            }
        }
    }
}
