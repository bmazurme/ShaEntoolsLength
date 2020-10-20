using Entools.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Entools.ViewModels
{
    class ViewModel : INotifyPropertyChanged
    {
        #region FIELDS
        private ObservableCollection<PipeDetail> listSelection = new ObservableCollection<PipeDetail>();

        public ObservableCollection<PipeDetail> ListSelection
        {
            get => listSelection;
            set
            {
                listSelection = value;
                OnPropertyChange(nameof(ListSelection));
            }
        }
        #endregion

        public ViewModel()
        {
            #region INTERFACE

            CultureInfo ui = Thread.CurrentThread.CurrentUICulture;

            if (ui.Name == "ru-RU") System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru-RU");
            else System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            #endregion


            for (int i = 0; i < Transfers.Diameter.Count(); i++)
            {
                var data = new PipeDetail
                {
                    Diameter = Transfers.Diameter[i] * 304.8, // Units ft -> mm
                    Length = Math.Round(Transfers.Length[i], 0),
                    Area = Math.Round(Transfers.Area[i], 2),
                    Count = Transfers.Count[i],
                };

                ListSelection.Add(data);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Action CloseAction { get; set; }

        protected void OnPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}