using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using Uno.Extensions.Specialized;
using Windows.Graphics.Printing.PrintSupport;

namespace DCSSTV.Models.ViewModels
{
    public class MainViewModel : DispatchedBindableBase
    {
        // Insert variables below here
        private bool _isBusy;
        private int _seekbarMaxValue = 0;
        private int _seekbarValue = 0;
        private string _timeRemaining = string.Empty;
        private string _searchTerm = string.Empty;
        //private ObservableCollection<Breed> _searchResults = new ObservableCollection<Breed>();
        //private BreedSearchApi _breedSearchApi = new BreedSearchApi();

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public int SeekbarMaxValue
        {
            get => _seekbarMaxValue;
            set => SetProperty(ref _seekbarMaxValue, value);
        }

        public int SeekbarValue
        {
            get => _seekbarValue;
            set => SetProperty(ref _seekbarValue, value);
        }

        public string RemainingTime
        {
            get => _timeRemaining;
            set => SetProperty(ref _timeRemaining, value);
        }

        //public string SearchTerm
        //{
        //    get => _searchTerm;
        //    set => SetProperty(ref _searchTerm, value);
        //}

        //public ObservableCollection<Breed> SearchResults
        //{
        //    get => _searchResults;
        //    set => SetProperty(ref _searchResults, value);
        //}

        public MainViewModel()
        {

        }

        //public async Task SearchBreeds()
        //{
        //    if (!string.IsNullOrWhiteSpace(SearchTerm))
        //    {
        //        try
        //        {
        //            IsBusy = true;
        //            var result = await _breedSearchApi.Search(SearchTerm).ConfigureAwait(false);
        //            if (result.Any())
        //            {
        //                SearchResults = new ObservableCollection<Breed>(result);
        //            }
        //        }
        //        finally
        //        {
        //            IsBusy = false;
        //        }
        //    }
        //}

        // Insert Favorites below here
    }
}