using Microsoft.Extensions.Logging;
using RestClient.Net;
using RestClient.Net.Abstractions;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UnoCrossPlatform.Models;
using Windows.System;

namespace UnoCrossPlatform.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Fields
        private bool _isLoading = true;
        private CatFact? _catFact;
        private readonly IClient _client;
        private readonly ILogger _logger;
        #endregion

        #region Public Properties
        public ICommand Next { get; }
        public ICommand GoToGithub { get; }
        public CatFact? CatFact
        {
            get => _catFact;
            private set
            {
                _catFact = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CatFact)));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }
        #endregion

        #region Constructor
        public MainPageViewModel(
            IClient client,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MainPageViewModel>();

            _client = client;
            _ = LoadFactAsync();

            Next = new AsyncCommand(async (a) =>
            {
                await LoadFactAsync();
            }, loggerFactory);

            GoToGithub = new AsyncCommand(async (a) =>
            {
                _ = await Launcher.LaunchUriAsync(new Uri("https://github.com/MelbourneDeveloper/Samples/tree/master/UnoCrossPlatformTemplate"));
            }, loggerFactory);
        }

        private async Task LoadFactAsync()
        {
            try
            {
                CatFact = null;

                IsLoading = true;

                CatFact = await _client.GetAsync<CatFact>("facts/random");

                Console.WriteLine(CatFact.text);
            }
#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CS0168 // Variable is declared but never used
            {
                _logger.LogError(ex, ex.Message);
                CatFact = new CatFact { text = ex.Message };
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
    }
}