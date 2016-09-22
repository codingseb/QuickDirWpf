using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace QuickDir
{
    public class QDCommands
    {
        private static List<QDCommand> commandsList = (new QDCommand[]
        {
            new QDCommand("Set MaxHeight" , delegate()
            {
                MessageBox.Show("Set MaxHeight");
            }),
        }).ToList();

        public static void Execute(string commandText)
        {
            commandsList.Find(command => command.Text.Equals(commandText))
                .Action();
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
    }
}
