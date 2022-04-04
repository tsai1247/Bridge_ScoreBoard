using Android.Widget;
using Bridge_ScoreBoard.Static;
using Bridge_ScoreBoard.ViewModels;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bridge_ScoreBoard.Views
{
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel _viewModel;

        View[,] labels = new View[Variables.局數, 9];
        Label[] resultlabel = new Label[2];
        public ItemsPage()
        {
            InitializeComponent();
            
            BindingContext = _viewModel = new ItemsViewModel();


            string[] Dealer = { "N", "E", "S", "W" };
            string[] Vulnerability = { "None", "N-S", "E-W", "Both" };
            for (int i = 0; i < labels.GetLength(0); i++)
            {

                for (int j = 0; j < labels.GetLength(1); j++)
                {
                    if (j == 3)
                        labels[i, j] = new Entry { MaxLength = 6, TextTransform = TextTransform.Uppercase, Keyboard = null };
                    else if (j == 4)
                        labels[i, j] = new Entry { MaxLength = 1, TextTransform = TextTransform.Uppercase, Keyboard = null };
                    else if (j == 5)
                        labels[i, j] = new Entry { MaxLength = 3, TextTransform = TextTransform.Uppercase, Keyboard = null };
                    else if (j == 6)
                        labels[i, j] = new Entry { MaxLength = 3, Keyboard = null, TextTransform = TextTransform.Uppercase };
                    else
                        labels[i, j] = new Label();

                    if (j < 3)
                        labels[i, j].Background = new SolidColorBrush(Xamarin.Forms.Color.Default);
                    else if (j < 7)
                        labels[i, j].Background = new SolidColorBrush(j % 2 == 0 ? Xamarin.Forms.Color.AliceBlue : Xamarin.Forms.Color.LightYellow);
                    else
                        labels[i, j].Background = new SolidColorBrush(j % 2 == 0 ? Xamarin.Forms.Color.FromHex("F1E1FF") : Xamarin.Forms.Color.FromHex("DEFFAC"));

                    if (j >= 3 && j <= 6)
                    {
                        (labels[i, j] as Entry).TextColor = Xamarin.Forms.Color.Black;
                        (labels[i, j] as Entry).HorizontalTextAlignment = TextAlignment.Center;
                        (labels[i, j] as Entry).TextChanged += Entry_TextChanged;
                        (labels[i, j] as Entry).Focused += ShowKeyboard;
                    }
                    else
                    {
                        if (j == 1)
                        {
                            (labels[i, j] as Label).Text = Dealer[i % 4];
                        }
                        else if (j == 2)
                        {
                            (labels[i, j] as Label).Text = Vulnerability[(i % 4 + i / 4)%4];
                        }
                        (labels[i, j] as Label).TextColor = Xamarin.Forms.Color.Black;
                        (labels[i, j] as Label).HorizontalTextAlignment = TextAlignment.Center;
                        (labels[i, j] as Label).VerticalTextAlignment = TextAlignment.Center;
                    }
                    Grid.SetColumn(labels[i, j], j);
                }
            }

            for (int i = 0; i < labels.GetLength(0); i++)
            {
                for (int j = 0; j < labels.GetLength(1); j++)
                {
                    Grid.SetColumn(labels[i, j], j);
                    Grid.SetRow(labels[i, j], i + 2);

                    (labels[i, 0] as Label).Text = (i + 1).ToString();

                    dataGrid.Children.Add(labels[i, j]);
                }
            }
            for (int i = 0; i < resultlabel.Length; i++)
            {
                resultlabel[i] = new Label();

                resultlabel[i].Text = "0";
                resultlabel[i].HorizontalTextAlignment = TextAlignment.Center;
                resultlabel[i].VerticalTextAlignment = TextAlignment.Center;
                resultlabel[i].TextColor = Xamarin.Forms.Color.Black;

                Grid.SetColumn(resultlabel[i], i+7);
                Grid.SetRow(resultlabel[i], Variables.局數+2);
                dataGrid.Children.Add(resultlabel[i]);
            }
        }
        private void HideKeyboard(object sender, FocusEventArgs e)
        {
            keyboard_content.IsVisible = false;
            keyboard_declarer.IsVisible = false;
            keyboard_firstCard.IsVisible = false;
            keyboard_result.IsVisible = false;
            if (currentFocusEntry != null)
            {
                currentFocusEntry.Background = currentColor;
                currentFocusEntry = null;
            }
            keyboardRowDef.Height = new GridLength(1, GridUnitType.Absolute);
        }
        private Entry currentFocusEntry = null;
        private Brush currentColor = null;
        private void ShowKeyboard(object sender, FocusEventArgs e)
        {
            HideKeyboard(sender, e);
            keyboardRowDef.Height = new GridLength(3.5, GridUnitType.Star);
            int col = Grid.GetColumn((sender as Entry)) - 3;
            (sender as Entry).Unfocus();
            currentFocusEntry = (sender as Entry);
            currentColor = currentFocusEntry.Background;

            (sender as Entry).Background = new SolidColorBrush(Xamarin.Forms.Color.FromHex("FFD0FF"));
            switch (col)
            {
                case 0: // 3NT
                    keyboard_content.IsVisible = true;
                    keyboard_content.Focus();
                    break;
                case 1: // N
                    keyboard_declarer.IsVisible = true;
                    keyboard_content.Focus();
                    break;
                case 2: // D4
                    keyboard_firstCard.IsVisible = true;
                    keyboard_content.Focus();
                    break;
                case 3: // +1
                    keyboard_result.IsVisible = true;
                    keyboard_content.Focus();
                    break;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        enum Trump
        {
            High,
            Low,
            NoTrump,
        }
        enum Game
        {
            Game,
            Part_Game,
            Slam,
            Grand_Slam,
        }
        enum Double
        {
            None,
            Double,
            ReDouble,
        }


        private void Entry_TextChanged(object sender, EventArgs e)
        {
            int row = Grid.GetRow(sender as Entry)-2;
            string content;
            int doublenum;
            string trump;

            string banker;
            string res;
            string worth;

            bool 身價;
            Trump 花色;
            int 線位;
            Game 成局;
            int 超蹬;
            Double 賭倍;

            try
            {
                content = (labels[row, 3] as Entry).Text; // 7NTXX
                if (content != null && content != "")
                    doublenum = content.Split('X').Length - 1;
                else
                    throw new Exception();
                trump = content.Substring(1).Split('X')[0].Trim();

                banker = (labels[row, 4] as Entry).Text; // N/E/S/W
                res = (labels[row, 6] as Entry).Text; // +1
                worth = (labels[row, 2] as Label).Text; // None

                身價 = Have(worth, banker);
                花色 = GetTrump(trump);
                線位 = int.Parse(content[0].ToString());
                成局 = GetGame(線位, 花色);
                超蹬 = Get蹬數(res);
                賭倍 = (Double)doublenum;


                int ret;
                if (超蹬 >= 0)
                {
                    int 牌蹬分數 = Get牌蹬分數(賭倍, 線位, 花色);
                    int 線位獎分 = Get線位獎分(身價, 成局);
                    int 賭倍獎分 = Get賭倍獎分(賭倍);
                    int 超蹬分數 = Get超蹬分數(賭倍, 花色, 超蹬);

                    ret = 牌蹬分數 + 線位獎分 + 賭倍獎分 + 超蹬分數;
                }
                else
                {
                    int 罰分 = Get罰分(身價, 賭倍, 超蹬);
                    ret = 罰分;
                }

                switch (banker.ToLower())
                {
                    case "n":
                    case "s":
                        (labels[row, 7] as Label).Text = ret.ToString();
                        (labels[row, 8] as Label).Text = "";
                        break;

                    case "e":
                    case "w":
                        (labels[row, 7] as Label).Text = "";
                        (labels[row, 8] as Label).Text = ret.ToString();
                        break;
                }

                for (int i = 0; i < resultlabel.Length; i++)
                {
                    int totalScore = 0;
                    for (int j = 0; j < labels.GetLength(0); j++)
                    {
                        string curScore = (labels[j, i + 7] as Label).Text;
                        totalScore += (curScore == null || curScore == "") ? 0 : int.Parse(curScore);
                    }
                    resultlabel[i].Text = totalScore.ToString();
                }
            }
            catch (Exception)
            {
                (labels[row, 7] as Label).Text = "";
                (labels[row, 8] as Label).Text = "";
                return;
            }
        }

        private int Get罰分(bool 身價, Double 賭倍, int 超蹬)
        {
            int ret = 0;
            if (身價)
            {
                switch (賭倍)
                {
                    case Double.None:
                        ret = 超蹬 * 100;
                        break;
                    case Double.Double:
                        ret = 超蹬 * 200 + ((超蹬 < -1) ? (超蹬 + 1) * 100 : 0);
                        break;
                    case Double.ReDouble:
                        ret = 超蹬 * 400 + ((超蹬 < -1) ? (超蹬 + 1) * 200 : 0);
                        break;
                }
            }
            else
            {
                switch (賭倍)
                {
                    case Double.None:
                        ret = 超蹬 * 50;
                        break;
                    case Double.Double:
                        ret = 超蹬 * 100 + ((超蹬 < -1) ? (超蹬 + 1) * 100 : 0) + ((超蹬 < -3) ? (超蹬 + 3) * 100 : 0);
                        break;
                    case Double.ReDouble:
                        ret = 超蹬 * 200 + ((超蹬 < -1) ? (超蹬 + 1) * 200 : 0) + ((超蹬 < -3) ? (超蹬 + 3) * 200 : 0);
                        break;
                }
            }
            return ret;
        }

        private int Get超蹬分數(Double 賭倍, Trump 花色, int 超蹬)
        {
            int eachScore = 0;
            switch (賭倍)
            {
                case Double.None:
                    eachScore =  花色 == Trump.Low ? 20 : 30;
                    break;
                case Double.Double:
                    eachScore = 花色 == Trump.Low ? 100 : 200;
                    break;
                case Double.ReDouble:
                    eachScore = 花色 == Trump.Low ? 200 : 400;
                    break;
            }
            return eachScore * 超蹬;
        }

        private int Get賭倍獎分(Double 賭倍)
        {
            return 50 * (int)賭倍;
        }

        private int Get線位獎分(bool 身價,  Game 成局)
        {
            switch(成局)
            {
                case Game.Part_Game:
                    return 50;
                case Game.Game:
                    return 身價 ? 500 : 300;
                case Game.Slam:
                    return 身價 ? 1250 : 800;
                case Game.Grand_Slam:
                    return 身價 ? 2000 : 1300;
                default:
                    throw new Exception();
            }
        }

        private int Get牌蹬分數(Double 賭倍, int 線位, Trump 花色)
        {
            int ret = 0;

            switch(花色)
            {
                case Trump.NoTrump:
                    ret += 線位 * 30 + 10;
                    break;
                case Trump.High:
                    ret += 線位 * 30;
                    break;
                case Trump.Low:
                    ret += 線位 * 20;
                    break;
            }

            switch (賭倍)
            {
                case Double.None:
                    break;
                case Double.Double:
                    ret *= 2;
                    break;
                case Double.ReDouble:
                    ret *= 4;
                    break;
            }
            return ret;
        }

        private int Get蹬數(string res)
        {

            if (res.Contains('='))
                res = "0";
            else
                res = res.Remove(1, 1);

            int result = int.Parse(res.Trim());
            return result;
        }

        private Game GetGame(int line, Trump 花色)
        {
            if (line == 6)
                return Game.Slam;
            else if (line == 7)
                return Game.Grand_Slam;

            switch (花色)
            {
                case Trump.NoTrump:
                    return line >= 3 ? Game.Game : Game.Part_Game;
                case Trump.High:
                    return line >= 4 ? Game.Game : Game.Part_Game;
                case Trump.Low:
                    return line >= 5 ? Game.Game : Game.Part_Game;
                default:
                    throw new Exception();
            }
        }


        private Trump GetTrump(string trump)
        {
            switch (trump)
            {
                case "♠":
                case "♥":
                    return Trump.High;
                case "♦":
                case "♣":
                    return Trump.Low;
                case "NT":
                    return Trump.NoTrump;
                default:
                    throw new Exception();
            }
        }

        private bool Have(string worth, string banker)
        {
            if(worth == "Both")
            {
                return true;
            }
            else if(worth == "None")
            {
                return false;
            }
            else
            {
                return worth.Contains(banker);
            }
        }

        private static string GetLines(string text)
        {
            for (char c = '1'; c <= '7'; c++)
            {
                if (text.Contains(c))
                    return c.ToString();
            }
            return "1";
        }
        private static string GetSign(string text)
        {
            return text.Contains("-") ? "-" : "+";
        }
        private static string GetShapes(string text)
        {
            string[] shapeArr = { "♠", "♥", "♦", "♣", "NT"};
            foreach (string s in shapeArr)
            {
                if (text.Contains(s))
                    return s;
            }
            return shapeArr[0];
        }
        private static string GetDouble(string text)
        {
            string ret = "";
            for(int i=0; i < text.Split('X').Length-1; i++)
            {
                ret += "X";
            }
            return ret;
        }

        private static string GetCardNum(string text)
        {
            if (text.Contains("10"))
                return "10";

            for (char c = '2'; c <= '9'; c++)
            {
                if (text.Contains(c))
                    return c.ToString();
            }

            foreach (char c in "AKQJ")
            {
                if (text.Contains(c))
                    return c.ToString();
            }

            return "A";
        }

        private void Shape_Clicked(object sender, EventArgs e)
        {
            string origin = currentFocusEntry.Text;
            if (origin == null)
                origin = "";

            currentFocusEntry.Text = GetLines(origin) + (sender as Xamarin.Forms.Button).Text + " " + GetDouble(origin);
        }
        private void Lines_Clicked(object sender, EventArgs e)
        {
            string origin = currentFocusEntry.Text;
            if (origin == null)
                origin = "";
            currentFocusEntry.Text = (sender as Xamarin.Forms.Button).Text + GetShapes(origin) + " " + GetDouble(origin);

        }
        private void Double_Clicked(object sender, EventArgs e)
        {
            string origin = currentFocusEntry.Text;
            if (origin == null)
                origin = "";
            currentFocusEntry.Text = GetLines(origin) + GetShapes(origin) + " " + (sender as Xamarin.Forms.Button).Text;

        }

        private void Enter_Clicked(object sender, EventArgs e)
        {
            HideKeyboard(null, null);

        }

        private void Declarer_Clicked(object sender, EventArgs e)
        {
            currentFocusEntry.Text = (sender as Xamarin.Forms.Button).Text;
        }

        private void FirstShape_Clicked(object sender, EventArgs e)
        {
            string origin = currentFocusEntry.Text;
            if (origin == null)
                origin = "";
            currentFocusEntry.Text = (sender as Xamarin.Forms.Button).Text + GetCardNum(origin);
        }
        private void FirstNum_Clicked(object sender, EventArgs e)
        {
            string origin = currentFocusEntry.Text;
            if (origin == null)
                origin = "";
            currentFocusEntry.Text = GetShapes(origin) + (sender as Xamarin.Forms.Button).Text;
        }

        private void ResultNum_Clicked(object sender, EventArgs e)
        {
            if((sender as Xamarin.Forms.Button).Text.Contains("="))
            {
                currentFocusEntry.Text = "+ 0";
            }
            else
            {
                string origin = currentFocusEntry.Text;
                if (origin == null)
                    origin = "";
                currentFocusEntry.Text = GetSign(origin) + " " + (sender as Xamarin.Forms.Button).Text;
            }
        }
        private void ResultSign_Clicked(object sender, EventArgs e)
        {
            string origin = currentFocusEntry.Text;
            if (origin == null)
                origin = "";
            currentFocusEntry.Text = (sender as Xamarin.Forms.Button).Text + " " + GetLines(origin);
        }


        int[] colMinWidth = { 40, 50, 50, 110, 50, 60, 70, 100, 100 };
        private void Title_Clicked(object sender, EventArgs e)
        {
            var col = Grid.GetColumn(sender as View);
            var width = dataGrid.ColumnDefinitions.ElementAt(col).Width.Value;
            if(width == 10)
            {
                dataGrid.ColumnDefinitions.ElementAt(col).Width = colMinWidth[col];
            }
            else
            {
                dataGrid.ColumnDefinitions.ElementAt(col).Width = 10;
                
            }
        }

        
        private void Toast(string message, ToastLength toastLength = ToastLength.Long)
        {
            try
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    Android.Widget.Toast.MakeText(Android.App.Application.Context, message, toastLength).Show();
                }
            }
            catch
            {

            }
            
        }

        private string AppFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string fileName => DateTime.Now.ToString("BridgeScoreBoard_MMdd_HHmmss") + ".xlsx";
        private static DocumentFormat.OpenXml.Spreadsheet.Cell ConstructCell(string value, CellValues dataTypes) =>new DocumentFormat.OpenXml.Spreadsheet.Cell()
        {
            CellValue = new CellValue(value),
            DataType = new EnumValue<CellValues>(dataTypes)
        };



        private void Export_Clicked(object sender, EventArgs e)
        {
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.Favorites));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            //Toast(Environment.GetFolderPath(Environment.SpecialFolder.Resources));
            //return;


            Environment.SetEnvironmentVariable("MONO_URI_DOTNETRELATIVEORABSOLUTE", "true");

            // Creating the SpreadsheetDocument in the indicated FilePath
            var filePath = Path.Combine(AppFolder, fileName);
            var document = SpreadsheetDocument.Create(Path.Combine(AppFolder, fileName), SpreadsheetDocumentType.Workbook);

            var wbPart = document.AddWorkbookPart();
            wbPart.Workbook = new Workbook(); 

            var part = wbPart.AddNewPart<WorksheetPart>();
            part.Worksheet = new Worksheet(new SheetData());

            //  Here are created the sheets, you can add all the child sheets that you need.
            var sheets = wbPart.Workbook.AppendChild
                (
                   new Sheets(
                            new Sheet()
                            {
                                Id = wbPart.GetIdOfPart(part),
                                SheetId = 1,
                                Name = "Contacts"
                            }
                        )
                );
            ////////////

            var sheetData = part.Worksheet.Elements<SheetData>().First();

            var row = sheetData.AppendChild(new Row());


            for (int i=0; i<labels.GetLength(1); i++)
            {
                row.Append(
                    ConstructCell(
                        (dataGrid.Children.ToList()[i] as Label).Text, 
                        CellValues.String
                    )
                );
            }
            Row dataRow;
            for (int i=0; i<labels.GetLength(0); i++)
            {
                dataRow = sheetData.AppendChild(new Row());

                for(int j=0; j<labels.GetLength(1); j++)
                {
                    dataRow.Append(ConstructCell(labels[i, j] is Label ? (labels[i, j] as Label).Text : (labels[i, j] as Entry).Text, CellValues.String));
                }
            }

            dataRow = sheetData.AppendChild(new Row());
            dataRow.Append(ConstructCell("總分", CellValues.String));

            for (int j = 0; j < labels.GetLength(1) - 3; j++)
            {
                dataRow.Append(ConstructCell("", CellValues.String));
            }

            for (int j = 0; j < resultlabel.Length; j++)
            {
                dataRow.Append(ConstructCell(resultlabel[j].Text, CellValues.String));
            }


            wbPart.Workbook.Save();
            document.Close();

            Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
        }


    }


}
