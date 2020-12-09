﻿using SmartParkingApp.ClassLibrary;
using SmartParkingApp.ClassLibrary.Models;
using SmartParkingApp.Client.Commands;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace SmartParkingApp.Client.ViewModels
{
    class RegisterViewModel : INotifyPropertyChanged
    {

        // Properties for DataBinding
        /************************************************************************************/
        // Color for border relative to username field
        public Color UsernameColor
        {
            get { return _usernameColor; }
            private set
            {
                _usernameColor = value;
                OnPropertyChanged("UsernameColor");
            }
        }
        private Color _usernameColor = Colors.Chocolate;



        // Color for border relative to password field
        public Color PasswordColor
        {
            get { return _passwordColor; }
            private set
            {
                _passwordColor = value;
                OnPropertyChanged("PasswordColor");
            }
        }
        private Color _passwordColor = Colors.Chocolate;



        // Color for border relative to CarPlateNumber field
        public Color CarPlateNumberColor
        {
            get { return _carPlateNumberColor; }
            private set
            {
                _carPlateNumberColor = value;
                OnPropertyChanged("CarPlateNumber");
            }
        }
        private Color _carPlateNumberColor = Colors.Chocolate;



        // Color for border relative to PhoneNumber
        public Color PhoneNumberColor
        {
            get { return _phoneNumberColor; }
            private set
            {
                _phoneNumberColor = value;
                OnPropertyChanged("PhoneNumberColor");
            }
        }
        private Color _phoneNumberColor = Colors.Chocolate;



        // Property for button enable
        public bool IsBtnRegisterEnabled
        {
            get { return _isBtnRegisterEnabled; }
            private set
            {
                _isBtnRegisterEnabled = value;
                OnPropertyChanged("IsBtnRegisterEnabled");
            }
        }
        private bool _isBtnRegisterEnabled = false;


        // Property for Name field
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
                Match m = Regex.Match(value, "^[A-Za-z]*[A-Za-z]+[A-Za-z0-9_]*$");
                if (m.Success)
                {
                    UsernameColor = Colors.Chocolate;
                    _nameReady = true;
                }
                else
                {
                    UsernameColor = Colors.Red;
                    _nameReady = false;
                }
                EnableRegisterButton();
            }
        }
        private string _userName = "Name";


        // Property for password
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                Match m = Regex.Match(value, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,35}$");
                if (m.Success)
                {
                    PasswordColor = Colors.Chocolate;
                    _passwordReady = true;
                }
                else
                {
                    PasswordColor = Colors.Red;
                    _passwordReady = false;
                }
                EnableRegisterButton();
            }
        }
        private string _password = "";



        // Property for CarPlateNumber
        public string CarPlateNumber
        {
            get { return _carPlateNumber; }
            set
            {
                _carPlateNumber = value;
                OnPropertyChanged("CarPlateNumber");
                if (value.Length == 0)
                {
                    _carPlateNumberColor = Colors.Red;
                    _carPlateNumberReady = false;
                }
                else
                {
                    _carPlateNumberColor = Colors.Chocolate;
                    _carPlateNumberReady = true;
                }
                EnableRegisterButton();
            }
        }
        private string _carPlateNumber = "CarPlateNumber";




        // Property for PhoneNumber
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set
            {
                _phoneNumber = value;
                OnPropertyChanged("PhoneNumber");
                Match m = Regex.Match(value, @"^((\+[0-9])+([0-9]){10,16})$");

                if (m.Success)
                {
                    PhoneNumberColor = Colors.Chocolate;
                    _phoneNumberReady = true;
                }
                else
                {
                    PhoneNumberColor = Colors.Red;
                    _phoneNumberReady = false;
                }
                EnableRegisterButton();
            }
        }
        private string _phoneNumber = "PhoneNumber";


        private bool _nameReady = false;
        private bool _passwordReady = false;
        private bool _carPlateNumberReady = true;
        private bool _phoneNumberReady = false;
        private void EnableRegisterButton()
        {
            if (_nameReady && _passwordReady && _carPlateNumberReady && _phoneNumberReady)
            {
                IsBtnRegisterEnabled = true;
            }
            else
            {
                IsBtnRegisterEnabled = false;
            }
        }

        // Register Command
        public RegisterCommand RegisterUserCommand { get; private set; }
        public RegisterCommand NavigateToLogin { get; private set; }

        /************************************************************************************/









        private ParkingManager _pkManager;
        public RegisterViewModel(string userRole, ParkingManager pkm, Action navigateToLogin)
        {
            _pkManager = pkm;
            RegisterUserCommand = new RegisterCommand(Register);
            NavigateToLogin = new RegisterCommand(navigateToLogin);
        }


        private async void Register()
        {
            // Create new user object
            User usr = new User
            {
                Username = UserName,
                CarPlateNumber = CarPlateNumber,
                Phone = PhoneNumber,
                Password = Password
            };


            // Try to add user to the json database
            ResponseModel response = await _pkManager.RegisterUser(usr);

            if (response == null)
            {
                IssueWindow.ShowMessage("An error occurred while trying to register");
                return;
            }


            if (!response.Succeded)
            {
                IssueWindow.ShowMessage(response.Message);
            }
            else
            {
                NavigateToLogin.Execute(null);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
