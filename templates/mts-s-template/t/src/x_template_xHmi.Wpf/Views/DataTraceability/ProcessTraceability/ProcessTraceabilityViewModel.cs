using x_template_xPlc;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TcoData;
using TcoInspectors;
using Vortex.Presentation;

namespace x_template_xHmi.Wpf.Views.Data.ProcessTraceability
{
    public class ProcessTraceabilityViewModel : BindableBase
    {


        public ProcessTraceabilityViewModel()
        {
            _processData = new TcoDataExchangeDisplayViewModel()
            {
                Model = App.x_template_xPlc.MAIN._technology._processTraceability
            };

            var records = (ObservableCollection<object>)_processData.DataViewModel.ObservableRecords;




            FilteredRecords = CollectionViewSource.GetDefaultView(records);
            FilteredRecords.Filter = ApplyCustomFilter;

            _availableResults = GetAvailableResults();
            _availableStations = GetAvailableStations();

            SelectedResult = _availableResults.First(); ;
            SelectedStation = _availableStations.First(); ;

            records.CollectionChanged += (s, e) =>
            {
                var currentResult = SelectedResult?.Value;
                var currentStation = SelectedStation?.Value;

                _availableResults = GetAvailableResults();
                _availableStations = GetAvailableStations();


                OnPropertyChanged(nameof(AvailableResults));
                OnPropertyChanged(nameof(AvailableStations));

                //// keep selection valid
                SelectedResult = _availableResults.FirstOrDefault(x => x.Value == currentResult) ?? _availableResults.First();

                SelectedStation = _availableStations.FirstOrDefault(x => x.Value == currentStation) ?? _availableStations.First();
            };

        }

        #region PUBLIC PROPERTIES
        public IEnumerable<ResultFilterItem> AvailableResults => _availableResults;

        public IEnumerable<StationFilterItem> AvailableStations => _availableStations;


        public ICollectionView FilteredRecords { get; }

        public TcoDataExchangeDisplayViewModel ProcessData => _processData;

        public ResultFilterItem SelectedResult
        {
            get => _selectedResult;
            set
            {
                SetProperty(ref _selectedResult, value);
                FilteredRecords.Refresh();
            }
        }


        public StationFilterItem SelectedStation
        {
            get => _selectedStation;
            set
            {
                SetProperty(ref _selectedStation, value);
                FilteredRecords.Refresh();
            }
        }
        #endregion

        #region PRIVATE METHODS

        private bool ApplyCustomFilter(object obj)
        {
            if (obj is null || !(obj is PlainProcessData item)) return false;


            bool resultMatch = SelectedResult?.Value == null || (eOverallResult)item.EntityHeader.Results.Result == SelectedResult.Value;

            bool stationMatch = SelectedStation?.Value == null || (eStations)item.EntityHeader.LastStation == SelectedStation.Value;

            return resultMatch && stationMatch;
        }

        private List<ResultFilterItem> GetAvailableResults()
        {
            var allResults = new ResultFilterItem { Value = null, Display = "All" };

            return new[] { allResults }
                                .Concat(
                                    Records
                                        .Select(x => (eOverallResult?)x.EntityHeader.Results.Result)
                                        .Distinct()
                                        .OrderBy(x => x)
                                        .Select(x => new ResultFilterItem
                                        {
                                            Value = x,
                                            Display = x.ToString()
                                        })
                                )
                                .ToList();
        }


        private List<StationFilterItem> GetAvailableStations()
        {

            var allStations = new StationFilterItem { Value = null, Display = "All" };

            return new[] { allStations }
                                .Concat(
                                        Records
                                            .Select(x => (eStations?)x.EntityHeader.LastStation)
                                            .Distinct()
                                            .OrderBy(x => x)
                                            .Select(x => new StationFilterItem
                                            {
                                                Value = x,
                                                Display = x.ToString()
                                            })
                                    )
                                    .ToList();
        }

        #endregion

        #region PRIVATE FIELDS AND PROPERTIES
        private List<ResultFilterItem> _availableResults;
        private List<StationFilterItem> _availableStations;
        private StationFilterItem _selectedStation;
        private ResultFilterItem _selectedResult;
        private readonly TcoDataExchangeDisplayViewModel _processData;

        private IEnumerable<PlainProcessData> Records => ((IEnumerable<object>)_processData.DataViewModel.ObservableRecords).Cast<PlainProcessData>();
        #endregion

        #region HELPER CLASSES
        public class ResultFilterItem
        {
            public eOverallResult? Value { get; set; }
            public string Display { get; set; }
        }

        public class StationFilterItem
        {
            public eStations? Value { get; set; }
            public string Display { get; set; }
        }
        #endregion
    }
}
