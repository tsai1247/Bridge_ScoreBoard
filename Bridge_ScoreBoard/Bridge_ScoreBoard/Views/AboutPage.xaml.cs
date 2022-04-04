using Bridge_ScoreBoard.Static;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bridge_ScoreBoard.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            RoundNum.Text = Variables.局數.ToString();
        }

        private void RoundNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            RoundNum.Text = Variables.局數.ToString();
        }
    }
}