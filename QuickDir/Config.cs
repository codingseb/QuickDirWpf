using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuickDir
{
    public class Config : INotifyPropertyChanged
    {
        private static Config instance = null;

        private string fileName = Path.Combine(EApplication.StartupPath, "config.json");
        private dynamic config = null;

        private dynamic defaultConfig = null;

        public static Config Instance
        {
            get 
            {
                if(instance == null)
                {
                    instance = new Config();
                }

                return instance;
            }
        }

        private Config()
        {
            try
            {
                defaultConfig = JsonConvert.DeserializeObject(Resources.DefaultConfig);
            }
            catch { }

            if(File.Exists(fileName))
            {
                config = JsonConvert.DeserializeObject(File.ReadAllText(fileName));
            }
            else
            {
                try
                {
                    config = JsonConvert.DeserializeObject(Resources.DefaultConfig);
                    Save();
                }
                catch { }
            }
        }

        public int MaxHeight
        {
            get
            {
                try
                {
                    return config.MaxHeight;
                }
                catch
                {
                    try
                    {
                        config.MaxHeight = defaultConfig.MaxHeight;
                        Save();
                    }
                    catch { }
                    return defaultConfig.MaxHeight;
                }
            }

            set 
            {
                try
                {
                    config.MaxHeight = value;
                    Save();
                }
                catch { }

                NotifyPropertyChanged();
            }
        }

        public bool CloseOnEscape
        {
            get
            {
                try
                {
                    return config.CloseOnEscape;
                }
                catch
                {
                    try
                    {
                        config.CloseOnEscape = defaultConfig.CloseOnEscape;
                        Save();
                    }
                    catch { }
                    return defaultConfig.CloseOnEscape;
                }
            }

            set
            {
                try
                {
                    config.CloseOnEscape = value;
                    Save();
                }
                catch { }

                NotifyPropertyChanged();
            }
        }

        public bool SetFav(string key, string path)
        {
            bool result = false;

            try
            {
                config.Favs[key] = path;
                Save();
                result = true;
            }
            catch { }

            return result;
        }

        public string GetFav(string key)
        {
            string result = "";
            try
            {
                result = config.Favs[key];
            }
            catch { }

            return result;
        }

        private void Save()
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}