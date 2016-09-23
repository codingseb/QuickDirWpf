using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace QuickDir
{
    public class QDCommands
    {
        public static event EventHandler QDCommandFinished;

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
                Process.GetProcesses()
                    .ToList()
                    .FindAll(p => p.GetClassName().Contains("CabinetWClass"))
                    .ForEach(p => p.Kill());
            })
        }).ToList();
    }
}
