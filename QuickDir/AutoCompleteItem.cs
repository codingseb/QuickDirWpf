using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickDir
{
    public class AutoCompleteItem : INotifyPropertyChanged
    {
        public AutoCompleteItem(string autoComplete)
        {
            this.AutoComplete = autoComplete;
            this.MainText = autoComplete;
        }

        public AutoCompleteItem(string mainText, string autoComplete)
        {
            this.AutoComplete = autoComplete;
            this.MainText = mainText;
        }

        public AutoCompleteItem(string mainText, string infoText, string autoComplete)
        {
            this.AutoComplete = autoComplete;
            this.MainText = mainText;
            this.InfoText = infoText;
        }

        private string mainText = "";

        public string MainText
        {
            get { return mainText; }
            set 
            {
                mainText = value;
                NotifyPropertyChanged();
            }
        }

        private string infoText = "";

        public string InfoText
        {
            get { return infoText; }
            set 
            {
                infoText = value;
                NotifyPropertyChanged();
            }
        }

        private string autoComplete = "";

        public string AutoComplete
        {
            get { return autoComplete; }
            set 
            { 
                autoComplete = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
