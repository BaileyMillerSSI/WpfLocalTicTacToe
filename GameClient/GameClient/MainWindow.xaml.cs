using GameLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static GameSearcher Searcher = new GameSearcher("Bailey Miller");
        
        public MainWindow()
        {
            InitializeComponent();

            Searcher.OnGameFound += NewGameClientFound;
            Searcher.OnException += ExceptionThrown;
            Searcher.OnGameRequest += GameRequestStarted;
            
        }

        private void GameRequestStarted(object sender, GameInstance instance)
        {
            StartGame(instance);
        }

        private void ExceptionThrown(object sender, Exception ex)
        {
            //MessageBox.Show($"Exception Caught: {ex.Message}");
        }

        private void NewGameClientFound(object sender, AdvertiseGame AvailableGame)
        {
            Dispatcher.Invoke(()=> 
            {
                AvailableGames.Children.Clear();

                foreach (var client in Searcher.AvailableGames)
                {
                    var btn = new Button()
                    {
                        Content = client.Username,
                        Tag = client.UniqueMatchMakingIdentifer
                    };

                    btn.Click += RequestMatchClicked;

                    AvailableGames.Children.Add(btn);
                }
            });
        }

        private void RequestMatchClicked(object sender, RoutedEventArgs e)
        {
            Searcher.RequestGame((Guid)(sender as Button).Tag, StartGame);
        }

        private void StartGame(object state)
        {
            var gameInstance = (GameInstance)state;
            Dispatcher.Invoke(()=> 
            {
                Title = "Game Started!";
                if (!gameInstance.IsHost)
                {
                    Title += $" you ({gameInstance.LocalClient.Username}) vs {gameInstance.RemoteClient.Body.Username}";
                }
                Searcher.StopSearching();
                AvailableGames.Children.Clear();
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Searcher.StopSearching();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;

            Searcher.SetUsername(UsernameField.Text);
            Searcher.StartSearching();
        }
    }
}
