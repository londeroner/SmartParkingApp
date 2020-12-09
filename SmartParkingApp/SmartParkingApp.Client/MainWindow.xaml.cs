﻿using SmartParkingApp.ClassLibrary;
using SmartParkingApp.Client.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System;
using System.Reflection;

namespace SmartParkingApp.Client
{
    public partial class MainWindow : Window
    {
        private readonly string DataPath;
        private ParkingManager _pkManager;

        private Action navigateToRegister;
        private Action navigateToLogin;
        private Action logoutAction;
        private Action navigateToMenue;

        public MainWindow()
        {
            InitializeComponent();
            DataPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SmartParkingApp\\Data";
            PrepareFiles();
            _pkManager = new ParkingManager();

            // Disable navigation bar in frame
            main_frame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;


            navigateToLogin = new Action(NavigateToLogin);
            navigateToRegister = new Action(NavigateToRegister);
            navigateToMenue = new Action(NavigateToMenue);
            logoutAction = new Action(Logout);

            main_frame.Content = new LoginPage(_pkManager, navigateToMenue, navigateToRegister);
        }

        private async void Logout()
        {
            await _pkManager.Logout();
            NavigateToLogin();
        }


        // Navigates to Register page
        private void NavigateToRegister()
        {
            main_frame.Content = new RegistrationPage(_pkManager, navigateToLogin);
        }
        
        // Navigates to Login page
        private void NavigateToLogin()
        {
            main_frame.Content = new LoginPage(_pkManager, navigateToMenue, navigateToRegister);
        }


        // Navigates to Menue Page
        private void NavigateToMenue()
        {
            main_frame.Content = new ClientMenuePage(logoutAction, _pkManager);
        }

        // Prepares json files
        private void PrepareFiles()
        {
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }

            if (!File.Exists(DataPath + "\\Users.json"))
            {
                File.Create(DataPath + "\\Users.json").Close();
            }

            if (!File.Exists(DataPath + "\\ParkingData.json"))
            {

                using (Stream _res = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("SmartParkingApp.Client.Default.ParkingData.json"))
                {
                    using (FileStream file = new FileStream(DataPath + "\\ParkingData.json", FileMode.Create))
                    {
                        _res.CopyTo(file);
                    }
                }

            }

            if(!File.Exists(DataPath + "\\Tariffs.json"))
            {
                using (Stream _res = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("SmartParkingApp.Client.Default.Tariffs.json"))
                {
                    using (FileStream file = new FileStream(DataPath + "\\Tariffs.json", FileMode.Create))
                    {
                        _res.CopyTo(file);
                    }
                }
            }
        }


        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragEnter += (s, t) =>
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
            };
            if (e.ClickCount == 1)
            {
                DragMove();
            }
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                    WindowMaximize.Content = Resources["WMaximize"];
                }
                else
                {
                    WindowState = WindowState.Maximized;
                    WindowMaximize.Content = Resources["WRestore"];
                }
            }
            
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void WindowMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                (sender as Button).Content = Resources["WRestore"];
                
            }
            else
            {
                WindowState = WindowState.Normal;
                (sender as Button).Content = Resources["WMaximize"];
            }
        }

        private void WindowClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
