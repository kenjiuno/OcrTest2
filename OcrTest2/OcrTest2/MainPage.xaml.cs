using Microsoft.ProjectOxford.Vision;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.ProjectOxford.Vision.Contract;
using SkiaSharp;
using System.Reflection;

namespace OcrTest2 {
    public partial class MainPage : TabbedPage {
        private OcrResults results;

        public MainPage() {
            InitializeComponent();

            // http://qiita.com/wraith13/items/2ed51964d463c5dddee7#%E5%9F%8B%E3%82%81%E8%BE%BC%E3%81%BF%E3%83%95%E3%82%A9%E3%83%B3%E3%83%88%E3%81%AE%E5%8F%96%E5%BE%97
            // http://qiita.com/nowri/items/1c69b9b25f2958bd9f97
            // https://www.google.com/get/noto/#sans-jpan
        }

        private async void cameraButton_Clicked(object sender, EventArgs e) {
            // https://github.com/xamarin/XamarinComponents
            // https://github.com/jamesmontemagno/MediaPlugin
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable) {
                await DisplayAlert("カメラがない!", "カメラを接続してな!", "閉じる");
                return;
            }
            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { });
            consume(file);
        }

        private async void consume(MediaFile file) {
            // https://forums.xamarin.com/discussion/comment/62875/#Comment_62875
            previewImage.Source = ImageSource.FromFile(file?.Path);

            var client = new VisionServiceClient(
                subscriptionKey: Secrets.ComputerVisionServiceApiKey,
                apiRoot: Consts.ComputerVisionApiEndPoint
                );

            CurrentPage = Children[1];
            results = null;
            resultsHere.Text = "・・・Azure Computer Vision API OCR で解析中・・・";
            textOnly.Text = "・・・Azure Computer Vision API OCR で解析中・・・";
            progress.IsVisible = true;
            results = await client.RecognizeTextAsync(file?.GetStream(), "ja");
            progress.IsVisible = false;
            resultsHere.Text = JsonConvert.SerializeObject(results, Formatting.Indented);
            uiPanel.InvalidateSurface();

            textOnly.Text = String.Join(" ", results.Regions
                .SelectMany(region => region.Lines)
                .SelectMany(line => line.Words.Concat(new Word[] { new Word { Text = "\n" } }))
                .Select(word => word.Text)
                );
        }

        private async void selectButton_Clicked(object sender, EventArgs e) {
            // https://github.com/xamarin/XamarinComponents
            // https://github.com/jamesmontemagno/MediaPlugin
            await CrossMedia.Current.Initialize();

            var file = await CrossMedia.Current.PickPhotoAsync();
            consume(file);
        }

        SKPaint redPen = new SKPaint {
            StrokeWidth = 1,
            Color = SKColors.Red,
            IsStroke = true,
        };
        SKPaint greenPen = new SKPaint {
            StrokeWidth = 1,
            Color = SKColors.Lime,
            IsStroke = true,
        };
        SKPaint bluePen = new SKPaint {
            StrokeWidth = 1,
            Color = SKColors.Violet,
            IsStroke = true,
        };
        SKPaint blackPen = new SKPaint {
            StrokeWidth = 1,
            Color = SKColors.Black,
        };

        private void uiPanel_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e) {
            // http://zawapro.com/?p=2000
            var canvas = e.Surface.Canvas;
            if (results == null) {
                return;
            }
            blackPen.Typeface = SKTypeface.FromStream(
                new SKManagedStream(
                    typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("OcrTest2.Fonts.NotoSansCJKjp-Regular.otf")
                    )
                );
            foreach (var region in results.Regions) {
                canvas.DrawRect(region.Rectangle.ToSKRect(), redPen);
                foreach (var line in region.Lines) {
                    canvas.DrawRect(line.Rectangle.ToSKRect(), greenPen);
                    foreach (var word in line.Words) {
                        canvas.DrawRect(word.Rectangle.ToSKRect(), bluePen);
                        canvas.DrawText(word.Text, word.Rectangle.Left, word.Rectangle.Top, blackPen);
                    }
                }
            }
        }
    }
}
