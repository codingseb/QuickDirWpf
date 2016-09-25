using System;
using System.Collections.Generic;
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
            commandsList.Find(command => command.Text.Equals(commandText))
                .Action();

            QDCommandFinished(commandText, new EventArgs());
        }

        public static List<string> FindCommands(string text)
        {
            string findTextPattern = ".*" + Regex.Replace(text.Split(':')[0], ".", delegate (Match match)
            {
                return match.Value + ".*";
            });

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

            internal Action Action
            {
                get;
                set;
            }

            internal QDCommand(string text, Action action)
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

        private static List<QDCommand> commandsList = (new QDCommand[]
        {
            new QDCommand("Set Close on Escape key press" , delegate()
            {
                Config.Instance.CloseOnEscape = true;
                MessageBox.Show("A press on [Escape] when the field is empty will now close the application.");
            }),
            new QDCommand("Set Minimize on Escape key press" , delegate()
            {
                Config.Instance.CloseOnEscape = false;
                MessageBox.Show("A press on [Escape] when the field is empty will now minimize the application.");
            }),
            new QDCommand("Close all explorer windows", delegate()
            {
                CloseAllWindowsWithClassName("CabinetWClass");
            }),
            new QDCommand("Close all cmd terminal windows", delegate()
            {
                CloseAllWindowsWithClassName("ConsoleWindowClass");
            }),
            new QDCommand("Set Field Width", delegate()
            {
                TextBox txtField = MainWindow.Instance.txtDirRequest;
                string[] commandsArgsArray = MainWindow.Instance.txtDirRequest.Text.Split(':');

                if(commandsArgsArray.Length > 0)
                {

                }
                else
                {
                    string currentWidth = Convert.ToInt32(Math.Floor(MainWindow.Instance.Width)).ToString();

                    txtField.Text += ":" + currentWidth;
                    txtField.Select(txtField.Text.Length - currentWidth.Length, currentWidth.Length);

                }
            })
        })
        .OrderBy(command => command.Text)
        .ToList();


    }
}
