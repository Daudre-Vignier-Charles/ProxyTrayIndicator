﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ProxyTrayIndicator
{
    public partial class MainWindow : Window
    {
        private Proxies proxies = new Proxies();
        private bool close = false;
        private Proxy userDefinedProxyServer;
        private bool userDefinedProxyState = false;
        private static System.Threading.Mutex mutex = new System.Threading.Mutex(false, "6b6bacb3-4b87-4516-876c-55eff887dad7");
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenu mainMenu = new System.Windows.Forms.ContextMenu();
        private System.Windows.Forms.MenuItem force;
        private System.Windows.Forms.MenuItem proxyMenu = new System.Windows.Forms.MenuItem() { Text = "Set proxy" };
        private static System.Timers.Timer timer;
        private System.Diagnostics.Process InternetSettings = new System.Diagnostics.Process()
        {
            //  Microsoft Windows Internet Settings window
            StartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + "inetcpl.cpl ,4")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += EventClosing;
            this.Closed += EventClosed;
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                System.Windows.Forms.MessageBox.Show("only one instance at a time");
                this.Close();
                return;
            }
            notifyIcon = new System.Windows.Forms.NotifyIcon()
            {
                Text = proxies.CurrentProxyServer.ToString(),
                Visible = true
            };
            notifyIcon.Click += IconClick;
            BuildMenu();
            SetTimer();
            userDefinedProxyState = proxies.CurrentProxyState;
            userDefinedProxyServer = proxies.CurrentProxyServer;
            dgProxy.ItemsSource = proxies.proxies;
        }

        private void IconClick(Object sender, EventArgs e)
        {
            if (((System.Windows.Forms.MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (proxies.CurrentProxyState)
                {
                    proxies.CurrentProxyState = false;
                    userDefinedProxyState = false;
                }
                else
                {
                    proxies.CurrentProxyState = true;
                    userDefinedProxyState = true;
                }
            }
        }

        private void SetTimer()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        /// <summary>
        /// Check proxy state and update tray icon
        /// </summary>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            notifyIcon.Text = proxies.CurrentProxyServer.ToString();
            if (force.Checked)
            {
                if (notifyIcon.Text != userDefinedProxyServer.ToString())
                    proxies.CurrentProxyServer = userDefinedProxyServer;
                if (proxies.CurrentProxyState != userDefinedProxyState)
                    proxies.CurrentProxyState = userDefinedProxyState;
            }
            if (proxies.CurrentProxyState)
                notifyIcon.Icon = Resource.on;
            else
                notifyIcon.Icon = Resource.off;
        }

        private void ForceSwitch(object sender, EventArgs e) =>
            force.Checked = force.Checked ? false : true;

        private void ExitClick(object sender, EventArgs e)
        {
            this.close = true;
            this.Close();
        }

        private void ShowCopyright(object sender, EventArgs e) =>
            System.Windows.Forms.MessageBox.Show(Resource.License, "Copyrights and licenses");

        private void LaunchIEParamClick(object sender, EventArgs e) =>
            InternetSettings.Start();

        private void EventClosing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (! this.close)
            {
                UpdateProxyMenu();
                this.Hide();
                this.ShowInTaskbar = false;
                e.Cancel = true;
            }
        }
        private void EventClosed(Object sender, EventArgs e)
        {
            proxies.SaveProxies();
            mutex.ReleaseMutex();
            notifyIcon.Dispose();
            return;
        }

        private void BuildMenu()
        {
            mainMenu.MenuItems.Add("Exit", new EventHandler(ExitClick));
            mainMenu.MenuItems.Add("Copyright and sources", new EventHandler(ShowCopyright));
            mainMenu.MenuItems.Add("Show IE settings", new EventHandler(LaunchIEParamClick));
            mainMenu.MenuItems.Add("Clear proxy", new EventHandler(proxies.ClearProxyServer));
            force = new System.Windows.Forms.MenuItem("Force mode", ForceSwitch) { Checked = false };
            mainMenu.MenuItems.Add(3, force);
            mainMenu.MenuItems.Add("Edit proxys", new EventHandler(OpenEditProxiesWindow));
            UpdateProxyMenu();
            notifyIcon.ContextMenu = mainMenu;
        }

        private void UpdateProxyMenu()
        {
            proxyMenu.MenuItems.Clear();
            proxyMenu.MenuItems.AddRange(GetProxyList());
            mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { proxyMenu });
        }

        private System.Windows.Forms.MenuItem[] GetProxyList()
        {
            List<System.Windows.Forms.MenuItem> menu = new List<System.Windows.Forms.MenuItem>();
            foreach (Proxy proxy in proxies.GetValidatedProxies())
                menu.Add(new System.Windows.Forms.MenuItem(proxy.Name, SetProxy) { Tag = proxy });
            return menu.ToArray();
        }

        private void SetProxy(Object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem item = sender as System.Windows.Forms.MenuItem;
            if (item != null)
            {
                Proxy proxy = item.Tag as Proxy;
                if (proxy != null)
                {
                    proxies.CurrentProxyServer = proxy;
                    userDefinedProxyServer = proxy;
                }
            }
        }

        private void OpenEditProxiesWindow(Object sender, EventArgs e)
        {
            CenterWindowOnScreen();
            this.Show();
            this.ShowInTaskbar = true;
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = (screenHeight - windowHeight) / 2;
        }
    }
}
