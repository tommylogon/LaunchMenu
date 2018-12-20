using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Path = System.IO.Path;

namespace LaunchMenu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentDirectory = Environment.CurrentDirectory + @"\..\..\shortcuts";
        public List<Button> Buttons { get; set; } = new List<Button>();

        public MainWindow()
        {
            InitializeComponent();

            ReadShortcuts();
        }

        private void ReadShortcuts()
        {
            foreach (string file in Directory.GetFiles(currentDirectory))
            {
                try
                {
                    GetShortcutInfo(Path.GetFullPath(file), out string name, out string path, out string descr, out string workdir, out string args);

                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(path);

                    Bitmap bitmap = icon.ToBitmap();
                    IntPtr hBitmap = bitmap.GetHbitmap();

                    img.Source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                    Button button = new Button();
                    button.Content = img;
                    button.Width = 50;
                    button.CommandParameter = path;
                    button.Click += RunApplication;
                    button.ToolTip = name;

                    wp_Content.Children.Add(button);
                }
                catch
                {
                }
            }
        }

        private void RunApplication(object sender, RoutedEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = (string)((Button)sender).CommandParameter;
            proc.Start();
        }

        private void RunAllApps(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (string file in Directory.GetFiles(currentDirectory))
                {
                    Console.WriteLine(file);
                    Process proc = new Process();
                    proc.StartInfo.FileName = file;
                    proc.Start();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private string GetShortcutInfo(string full_name, out string name, out string path, out string descr, out string working_dir, out string args)
        {
            name = "";
            path = "";
            descr = "";
            working_dir = "";
            args = "";
            try
            {
                // Make a Shell object.
                Shell32.Shell shell = new Shell32.Shell();

                // Get the shortcut's folder and name.
                string shortcut_path = full_name.Substring(0, full_name.LastIndexOf("\\"));
                string shortcut_name = full_name.Substring(full_name.LastIndexOf("\\") + 1);
                if (!shortcut_name.EndsWith(".lnk")) shortcut_name += ".lnk";

                // Get the shortcut's folder.
                Shell32.Folder shortcut_folder = shell.NameSpace(shortcut_path);

                // Get the shortcut's file.
                Shell32.FolderItem folder_item = shortcut_folder.Items().Item(shortcut_name);

                if (folder_item == null)
                    return "Cannot find shortcut file '" + full_name + "'";
                if (!folder_item.IsLink)
                    return "File '" + full_name + "' isn't a shortcut.";

                // Display the shortcut's information.
                Shell32.ShellLinkObject lnk =
                    (Shell32.ShellLinkObject)folder_item.GetLink;
                name = folder_item.Name;
                descr = lnk.Description;
                path = lnk.Path;
                working_dir = lnk.WorkingDirectory;
                args = lnk.Arguments;
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}