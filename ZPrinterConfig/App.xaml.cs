using ControlzEx.Theming;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ZPrinterConfig
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Version;
        public static Databases.SimpleDatabase Settings { get; private set; }

        //#if DEBUG
        public static string WorkingDir { get; set; } = System.IO.Directory.GetCurrentDirectory();
        //#else        
        //        public static string WorkingDir { get; set; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TDD\\ZPrinterConfig\\";
        //#endif

        public static string UserDataDirectory => $"{WorkingDir}\\UserData";
        public static string MapDatabaseSettingsFile => "ApplicationSettings";
        public static string MapDatabaseExtension => ".sqlite";

        public App()
        {
            //new GetCommandData();

            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (!Directory.Exists(UserDataDirectory))
            {
                _ = Directory.CreateDirectory(UserDataDirectory);
            }

            Settings = new Databases.SimpleDatabase().Init(Path.Combine(UserDataDirectory, $"{MapDatabaseSettingsFile}{MapDatabaseExtension}"), false);

            if (Settings == null)
            {
                return;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set the application theme to Dark.Green
            _ = ThemeManager.Current.ChangeTheme(this, Settings.GetValue("App.Theme", "Light.Steel"));

            ThemeManager.Current.ThemeChanged += Current_ThemeChanged;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
        }
        private void Current_ThemeChanged(object sender, ThemeChangedEventArgs e) => App.Settings.SetValue("App.Theme", e.NewTheme.Name);

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Settings.Dispose();
        }

        //public class GetCommandData
        //{

        //    public Dictionary<string, string> Commands { get; private set; } = new Dictionary<string, string>();
        //    public GetCommandData()
        //    {
        //        using (StreamReader file = new StreamReader("Assets\\RAW Command List.txt"))
        //        {
        //            StringBuilder sb = new StringBuilder();

        //            bool build = false;
        //            string ln;
        //            string temp = "";
        //            string command = "";
        //            while ((ln = file.ReadLine()) != null)
        //            {
        //                if (Regex.IsMatch(ln, @"^!"))
        //                {
        //                    if (Regex.IsMatch(ln, @"^! U1 getvar"))
        //                    {


        //                        if (Regex.IsMatch(ln, "\" \""))
        //                            continue;
        //                        else
        //                            command = ln;
        //                    }
        //                    else if (Regex.IsMatch(ln, @"^! U1 setvar"))
        //                    {
        //                        if (!ln.EndsWith("\""))
        //                        {
        //                            build = true;
        //                            temp = ln;
        //                            continue;
        //                        }
        //                        else
        //                        {
        //                            int index = ln.IndexOf("\" \"");
        //                            if (index != -1)
        //                                command = ln.Substring(0, index);
        //                            else
        //                            {
        //                                command = ln;
        //                            }

        //                        }

        //                    }
        //                    else
        //                    {
        //                        int index = ln.IndexOf("\" \"");
        //                        if (index != -1)
        //                            command = ln.Substring(0, index);
        //                        else
        //                        {
        //                            command = ln;
        //                        }
        //                    }

        //                    if (!Commands.ContainsKey(command))
        //                        Commands.Add(command, ln.Trim('\r', '\n'));

        //                    continue;
        //                }

        //                if (build)
        //                {
        //                    temp += ln;

        //                    command = temp.Substring(0, temp.IndexOf("\" \""));
        //                    if (!Commands.ContainsKey(command))
        //                        Commands.Add(command, temp.Trim(new char[] { '\r', '\n' }));
        //                    build = false;
        //                    //    if (ln.StartsWith("Ex"))
        //                    //        continue;

        //                    //    if (ln.StartsWith("Con"))
        //                    //        continue;

        //                    //    sb.Append(ln.Trim(new char[] { '\r', '\n' }));

        //                    //    if (ln.Contains(")"))
        //                    //    {
        //                    //        build = false;
        //                    //        Commands.Add(sb.ToString());
        //                    //        sb.Clear();
        //                    //    }
        //                }

        //                //if (ln.StartsWith("syntax", StringComparison.OrdinalIgnoreCase))
        //                //    build = true;
        //            }
        //        }

        //        var sortedDict = from entry in Commands orderby entry.Value ascending select entry;

        //        using (StreamWriter file1 = new StreamWriter("Assets\\Command List.txt"))
        //        {
        //            foreach (var kv in sortedDict)
        //            {
        //                file1.WriteLine($"{{ \"{kv.Value.Replace("! U1 ", "").Replace("\"", "")}\" }},");
        //            }
        //        }
        //        //File.WriteAllLines(, Commands.ToArray());
        //    }
        //}
    }
}
