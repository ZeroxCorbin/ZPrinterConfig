using ControlzEx.Theming;
using Logging.lib;
using System.IO;
using System.Reflection;
using System.Windows;

namespace ZPrinterConfig;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string Version;

    public static string AppBasePath =>
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        ?? Directory.GetCurrentDirectory();
    public static string UserDataDirectory => $"{AppBasePath}\\UserData";
    public static string ApplicationSettingsFile => "ApplicationSettings";
    public static string ApplicationSettingsExtension => ".sqlite";

    public static SimpleDatabase.lib.SettingsService Settings { get; private set; }

    public App()
    { 
        Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        
        Logger.InitializeNLog(UserDataDirectory, "ZPrinterConfig");
        Logger.Info($"Starting: {Version}");

        if (!Directory.Exists(UserDataDirectory))
        {
            _ = Directory.CreateDirectory(UserDataDirectory);
        }

        Settings = new SimpleDatabase.lib.SettingsService(Path.Combine(UserDataDirectory, $"{ApplicationSettingsFile}{ApplicationSettingsExtension}"));
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _ = ThemeManager.Current.ChangeTheme(this, Settings.GetValue("App.Theme", "Light.Steel"));

        ThemeManager.Current.ThemeChanged += Current_ThemeChanged;
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
