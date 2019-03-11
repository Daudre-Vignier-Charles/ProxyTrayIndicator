﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProxyTrayIndicator
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Mutex is used to allow only one instance of ProxyTrayIndicator
        static System.Threading.Mutex mutex = new System.Threading.Mutex(true, "{8F6E0AC4-B9A1-45fd-AC6F-73F04E6BDE8F}");
        private static string key = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\";
        private static string name = "ProxyEnable";
        private bool proxySet = true;
        System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon()
        {
            Text = (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "Aucune"),
            Visible = true
        };
        System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
        private static System.Timers.Timer timer;
        /// <summary>
        ///  Microsoft Windows Internet Settings
        /// </summary>
        System.Diagnostics.Process proc = new System.Diagnostics.Process()
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + "inetcpl.cpl ,4")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };

        public   MainWindow()
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("only one instance at a time");
                this.Close();
                return;
            }
            Closing += OnClose;
            InitializeComponent();
            menu.MenuItems.Add("Show IE settings", new EventHandler(LaunchIEParamClick));
            menu.MenuItems.Add("Copyright", new EventHandler(ShowC));
            menu.MenuItems.Add("Exit", new EventHandler(ExitClick));
            notifyIcon.ContextMenu = menu;
            notifyIcon.Click += Click;
            SetTimer();
        }

        /// <summary>
        /// Switch proxy state on tray icon click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Click(Object sender, EventArgs e)
        {
            if (((System.Windows.Forms.MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (proxySet)
                    Microsoft.Win32.Registry.SetValue(key, name, 0, Microsoft.Win32.RegistryValueKind.DWord);
                else
                    Microsoft.Win32.Registry.SetValue(key, name, 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Every one second, Timer_Elapsed is called
        /// </summary>
        private void SetTimer()
        {
            timer = new System.Timers.Timer(1000); //Timer goes off every 1 seconds
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        /// <summary>
        /// Check proxy state and update tray icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if ((int)Microsoft.Win32.Registry.GetValue(key, name, 8) == 1)
            {
                proxySet = true;
                notifyIcon.Icon = Resource.on;
            }
            else
            {
                proxySet = false;
                notifyIcon.Icon = Resource.off;
            }
            notifyIcon.Text = (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "no proxy server");
        }

        /// <summary>
        /// Clean and exit app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitClick(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Show copyright notice
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowC(object sender, EventArgs e)
        {
            MessageBox.Show(Resource.License, "Copyrights and licenses");
        }

        /// <summary>
        /// Open Microsoft Windows Internet Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchIEParamClick(object sender, EventArgs e)
        {
            proc.Start();
        }

        void OnClose(Object sender, EventArgs e)
        {
            mutex.ReleaseMutex();
            notifyIcon.Dispose();
            return;
        }
    }
}
