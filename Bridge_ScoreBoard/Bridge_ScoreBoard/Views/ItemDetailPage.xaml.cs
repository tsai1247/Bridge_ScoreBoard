using Bridge_ScoreBoard.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Bridge_ScoreBoard.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}