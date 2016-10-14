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
using System.Dynamic;
using System.Reflection;
using Newtonsoft.Json.Linq;

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

        public int Width
        {
            get
            {
                try
                {
                    return config.Width;
                }
                catch
                {
                    try
                    {
                        config.Width = defaultConfig.Width;
                        Save();
                    }
                    catch { }
                    return defaultConfig.Width;
                }
            }

            set
            {
                try
                {
                    config.Width = value;
                    Save();
                }
                catch { }

                NotifyPropertyChanged();
            }
        }

        public List<string> NotManagedDirectories
        {
            get
            {
                List<string> list = null;

                try
                {
                    list = (config.NotManagedDirectories as JArray).Select(x => x.Value<string>()).ToList();
                }
                catch
                { }

                if(list == null)
                {
                    config.NotManagedDirectories = defaultConfig.NotManagedDirectories;
                    Save();
                    list = (defaultConfig.NotManagedDirectories as JArray).Select(x => x.Value<string>()).ToList();
                }

                return list;
            }
        }

        public bool IsManaged(string directory)
        {
            return !(NotManagedDirectories.Contains(directory)
                || NotManagedDirectories.Contains(Path.GetFileName(directory)));
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

        public bool SearchFavByKeys
        {
            get
            {
                try
                {
                    return config.SearchFavByKeys;
                }
                catch
                {
                    try
                    {
                        config.SearchFavByKeys = defaultConfig.SearchFavByKeys;
                        Save();
                    }
                    catch { }
                    return defaultConfig.SearchFavByKeys;
                }
            }

            set
            {
                try
                {
                    config.SearchFavByKeys = value;
                    Save();
                }
                catch { }

                NotifyPropertyChanged();
            }
        }

        public bool SmartSearchOnFavs
        {
            get
            {
                try
                {
                    return config.SmartSearchOnFavs;
                }
                catch
                {
                    try
                    {
                        config.SmartSearchOnFavs = defaultConfig.SmartSearchOnFavs;
                        Save();
                    }
                    catch { }
                    return defaultConfig.SmartSearchOnFavs;
                }
            }

            set
            {
                try
                {
                    config.SmartSearchOnFavs = value;
                    Save();
                }
                catch { }

                NotifyPropertyChanged();
            }
        }

        public bool SmartHistory
        {
            get
            {
                try
                {
                    return config.SmartHistory;
                }
                catch
                {
                    try
                    {
                        config.SmartHistory = defaultConfig.SmartHistory;
                        Save();
                    }
                    catch { }
                    return defaultConfig.SmartHistory;
                }
            }

            set
            {
                try
                {
                    config.SmartHistory = value;
                    Save();
                }
                catch { }

                NotifyPropertyChanged();
            }
        }

        public bool SmartSearchOnDirectories
        {
            get
            {
                try
                {
                    return config.SmartSearchOnDirectories;
                }
                catch
                {
                    try
                    {
                        config.SmartSearchOnDirectories = defaultConfig.SmartSearchOnDirectories;
                        Save();
                    }
                    catch { }
                    return defaultConfig.SmartSearchOnDirectories;
                }
            }

            set
            {
                try
                {
                    config.SmartSearchOnDirectories = value;
                    Save();
                }
                catch { }

                NotifyPropertyChanged();
            }
        }

        public bool EmptyOnValidate
        {
            get
            {
                try
                {
                    return config.EmptyOnValidate;
                }
                catch
                {
                    try
                    {
                        config.EmptyOnValidate = defaultConfig.EmptyOnValidate;
                        Save();
                    }
                    catch { }
                    return defaultConfig.EmptyOnValidate;
                }
            }

            set
            {
                try
                {
                    config.EmptyOnValidate = value;
                    Save();
                }
                catch { }

                NotifyPropertyChanged();
            }
        }

        public bool MinimizeOnValidate
        {
            get
            {
                try
                {
                    return config.MinimizeOnValidate;
                }
                catch
                {
                    try
                    {
                        config.MinimizeOnValidate = defaultConfig.MinimizeOnValidate;
                        Save();
                    }
                    catch { }
                    return defaultConfig.MinimizeOnValidate;
                }
            }

            set
            {
                try
                {
                    config.MinimizeOnValidate = value;
                    Save();
                }
                catch { }

                NotifyPropertyChanged();
            }
        }

        public List<string> FavsList
        {
            get
            {
                List<string> result = new List<string>();

                try
                {
                    string json = JsonConvert.SerializeObject(config);
                    JObject parsed = JObject.Parse(json);
                    Dictionary<string, string> favsDict = parsed["Favs"].ToObject<Dictionary<string, string>>();

                    result = favsDict.Keys.ToList();
                }
                catch { }

                return result;
            }
        }

        private void EnsureHistoryPropertyExist()
        {
            if (config["History"] == null)
            {
                config.Add("History", new JArray());
                Save();
            }
        }

        public List<string> History
        {
            get
            {
                EnsureHistoryPropertyExist();

                return (config.History as JArray).ToList()
                    .ConvertAll(token => token.ToString());
            }
        }

        public bool SetHistoryEntry(string path)
        {
            bool result = false;

            try
            {
                EnsureHistoryPropertyExist();
                JArray arr = config.History as JArray;
                if (SmartHistory
                    && Directory.Exists(path)
                    && !Path.GetPathRoot(path).ToLower().Equals(path.ToLower())
                    && arr.ToList().Find(token => token.ToString().ToLower().Equals(path.ToLower())) == null)
                {
                    arr.Add(path + (path.EndsWith(@"\") ? "" : @"\"));
                    Save();
                }

                result = true;
            }
            catch { }

            return result;
        }



        public void ClearHistory()
        {
            config["History"] = new JArray();
        }

        public bool SetFav(string key, string path)
        {
            bool result = false;

            try
            {
                if (path.Equals(string.Empty))
                {
                    config.Favs.Remove(key);
                }
                else
                {
                    config.Favs[key] = path;
                }
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