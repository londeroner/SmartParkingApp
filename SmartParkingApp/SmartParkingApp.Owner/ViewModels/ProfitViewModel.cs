using SmartParkingApp.ClassLibrary;
using SmartParkingApp.ClassLibrary.Models;
using SmartParkingApp.Owner.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SmartParkingApp.Owner.ViewModels
{
    class ProfitViewModel : INotifyPropertyChanged
    {
        // Properties for DataBinding
        /************************************************************************************/

        // Property for CalculatedProfit
        public decimal CalculatedProfit
        {
            get { return _CalculatedProfit; }
            private set
            {
                _CalculatedProfit = value;
                OnPropertyChanged("CalculatedProfit");
            }
        }
        private decimal _CalculatedProfit;




        // Property for date since
        public DateTime Since { get; set; } = new DateTime(2020, 1, 1);


        // Property for date until
        public DateTime Until { get; set; } = new DateTime(2020, 12, 31);



        // Command for calculation
        public ProfitCommand CalculateCommand { get; set; }

        /************************************************************************************/





        private int _userId;
        private ParkingManager _pk;
        public ProfitViewModel(int userId, ParkingManager pk)
        {
            _userId = userId;
            _pk = pk;
            CalculateCommand = new ProfitCommand(new Action(Calculate));
        }


        private async void Calculate()
        {
            if ((Until - Since).TotalMilliseconds < 0)
                return;

            ResponseModel response = await _pk.GetPayedSessionsInPeriod(Since, Until);

            if (response == null)
            {
                IssueWindow.ShowMessage("An error occurred while calculating total margin");
                return;
            }

            if (!response.Succeded)
            {
                IssueWindow.ShowMessage(response.Message);
                return;
            }

            IEnumerable<ParkingSession> result = (IEnumerable<ParkingSession>)response.Data;
            
            decimal? count = 0;
            foreach(ParkingSession ps in result)
            {
                count += ps.TotalPayment;
            }

            CalculatedProfit = count.Value;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
