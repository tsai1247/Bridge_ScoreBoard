using Bridge_ScoreBoard.Static;
using Bridge_ScoreBoard.Views;
using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bridge_ScoreBoard.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public Command StartRecord { get; }
        public AboutViewModel()
        {
            Title = "新增";
            StartRecord = new Command(OnStartClicked);
        }

        private async void OnStartClicked(object obj)
        {
            //Variables.RoundNum = int.Parse(RoundNum);
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            await Shell.Current.GoToAsync($"//{nameof(ItemsPage)}");
        }

    }
}