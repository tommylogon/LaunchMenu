using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Interop;
using Path = System.IO.Path;
using Image = System.Windows.Controls.Image;
using Brushes = System.Windows.Media.Brush;

namespace LaunchMenu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string shortcutDirectory = Environment.CurrentDirectory + @"\Shortcuts";

        private List<Button> buttons = new List<Button>();

        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(shortcutDirectory))
            {
                Directory.CreateDirectory(shortcutDirectory);
            }

            ReadShortcuts();
        }

        private void AddBorder()
        {
            Border border = new Border();
            border.Height = 5;
            border.Width = wp_Content.Width;
            //border.Background = new SolidBrush();
            wp_Content.Children.Add(border);
        }

        private void AddTitle(string directory)
        {
            Label groupTitle = new Label();
            groupTitle.Content = Path.GetFileName(directory);

            wp_Content.Children.Add(groupTitle);
            AddBorder();
        }

        private void ReadShortcuts()
        {
            foreach (string directory in Directory.GetDirectories(shortcutDirectory))
            {
                AddTitle(directory);

                foreach (string file in Directory.GetFiles(directory))
                {
                    AddButtonsToWrapPanel(file);
                }
                AddBorder();
            }
            AddTitle("Andre");
            foreach (string file in Directory.GetFiles(shortcutDirectory))
            {
                try
                {
                    AddButtonsToWrapPanel(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void AddButtonsToWrapPanel(string file)
        {
            GetShortcutInfo(file, out string name, out string path, out string descr, out string workdir, out string args);
            Button button = new Button
            {
                Content = GetImageFromIcon(path),
                Width = 50,
                CommandParameter = path
            };
            button.Click += RunApplication;
            button.ToolTip = name;
            button.Margin = new Thickness()
            {
                Left = 2,
                Right = 2,
                Top = 2,
                Bottom = 2
            };
            wp_Content.Children.Add(button);
        }

        private Image GetImageFromIcon(string path)
        {
            Image img = new Image();

            Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(path);

            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            img.Source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return img;
        }

        private void RunApplication(object sender, RoutedEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = (string)((Button)sender).CommandParameter;
            proc.Start();
        }

        private void OpenShortcutFolder(object sender, RoutedEventArgs e)
        {
            Process.Start(shortcutDirectory);
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
                if (File.Exists(lnk.Path))
                {
                    path = lnk.Path;
                }
                else
                {
                    if (File.Exists(lnk.Path.Replace("(x86)", "")))
                    {
                        path = lnk.Path.Replace("(x86)", "");
                    }
                }
                working_dir = lnk.WorkingDirectory;
                args = lnk.Arguments;
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void Update_Clicked(object sender, RoutedEventArgs e)
        {
            wp_Content.Children.Clear();
            ReadShortcuts();
        }
    }
}