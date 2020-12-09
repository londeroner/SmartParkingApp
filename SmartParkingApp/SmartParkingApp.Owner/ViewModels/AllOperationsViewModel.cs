using SmartParkingApp.ClassLibrary;
using SmartParkingApp.ClassLibrary.Models;
using SmartParkingApp.Owner.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace SmartParkingApp.Owner.ViewModels
{
    class AllOperationsViewModel : INotifyPropertyChanged
    {
        // Properties for DataBinding
        /************************************************************************************/


        // Collection to display
        public ObservableCollection<ParkingSession> Sessions { get; set; }
        public ICommand RenewCommand { get; set; }



        // SelectedItemValue
        public string Selected
        {
            get { return _Selected; }
            set
            {
                _Selected = value;
                SetNewListMthod();
            }
        }
        private string _Selected;


        private List<ParkingSession> PastSessions;
        private List<ParkingSession> ActiveSessions;
        private ObservableCollection<ParkingSession> AllSessions;
        /************************************************************************************/





        private ParkingManager _pk;
        private int _userId;
        public AllOperationsViewModel(int userId, ParkingManager pk)
        {
            _pk = pk;
            _userId = userId;

            RenewCommand = new AllOperationsCommand(GetSessions);
            GetSessions();
        }

        private async void GetSessions()
        {
            ResponseModel responsePast = await _pk.GetPastSesstionsForOwner(_userId);
            ResponseModel responseActive = await _pk.GetActiveSesstionsForOwner(_userId);

            ActiveSessions = new List<ParkingSession>();
            PastSessions = new List<ParkingSession>();

            if (responseActive == null && responsePast == null)
            {
                return;
            }

            if (responseActive != null && responseActive.Succeded)
                ActiveSessions = (List<ParkingSession>)responseActive.Data;

            if (responsePast != null && responsePast.Succeded)
                PastSessions = (List<ParkingSession>)responsePast.Data;

            List<ParkingSession> tmp = new List<ParkingSession>(PastSessions);
            tmp.AddRange(ActiveSessions);
            AllSessions = new ObservableCollection<ParkingSession>(tmp);
            Sessions = new ObservableCollection<ParkingSession>(tmp);
            SetNewListMthod();
        }


        private void SetNewListMthod()
        {
            switch (Selected)
            {
                case "PastSessions":
                    {
                        Sessions = new ObservableCollection<ParkingSession>(PastSessions);
                        break;
                    }
                case "ActiveSessions":
                    {
                        Sessions = new ObservableCollection<ParkingSession>(ActiveSessions);
                        break;
                    }
                case "All":
                    {
                        Sessions = AllSessions ?? new ObservableCollection<ParkingSession>();
                        break;
                    }
                default:
                    break;
            }
            OnPropertyChanged("Sessions");
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
