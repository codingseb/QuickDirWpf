using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

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
            string findTextPattern = ".*" + Regex.Replace(text, ".", delegate (Match match)
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

        private static List<QDCommand> commandsList = (new QDCommand[]
        {
            new QDCommand("Set Close on Escape" , delegate()
            {
                Config.Instance.CloseOnEscape = true;
                MessageBox.Show("A press on [Escape] when the field is empty will now close the application.");
            }),
            new QDCommand("Set Minimize on Escape" , delegate()
            {
                Config.Instance.CloseOnEscape = false;
                MessageBox.Show("A press on [Escape] when the field is empty will now minimize the application.");
            }),
            new QDCommand("Close all explorer windows", delegate(){

                EnumWindows(delegate(IntPtr hwnd, IntPtr lParam) {

                    if(GetClassName(hwnd).Equals("CabinetWClass"))
                    {
                        SendMessage(hwnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                    }

                    return true;
                }
                , IntPtr.Zero);

            })
        }).ToList();


    }
}
