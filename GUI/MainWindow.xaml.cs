using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageControl = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;

namespace ImageSearch
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Comparator Comparator;

        private readonly Dictionary<string, ImageControl> _imageCache = new Dictionary<string, ImageControl>();

        private void ChooseDir_Click(object sender, RoutedEventArgs e)
        {
            string path;
            using (var dialog = new FolderBrowserDialog {ShowNewFolderButton = false})
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                path = dialog.SelectedPath;
            }
            if (path == Comparator?.Folder)
            {
                return;
            }
            DirectoryPath.Text = path;
            Comparator = ComparatorFactory.NewComparator(path);
            _imageCache.Clear();
        }

        private static ImageSource ImageSourceFromPath(string path) => new BitmapImage(new Uri(path));

        private void ChoosePic_Click(object sender, RoutedEventArgs e)
        {
            string path;
            using (var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Image Files|*.jpg;*.jpeg;*.bmp;*.gif;*.tiff"
            })
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                path = dialog.FileNames[0];
            }
            if (path == PicturePath.Text)
            {
                return;
            }

            PicturePath.Text = path;
            Preview.Source = ImageSourceFromPath(path);
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            var path = PicturePath.Text;
            var task = Task.Run(() =>
            {
                using (var bitmap = new Bitmap(Image.FromFile(path)))
                {
                    return Comparator.SearchFilenames(bitmap);
                }
            });

            if (!int.TryParse(SearchCount.Text, out var count))
            {
                MessageBox.Show(this, $"Invalid count: {SearchCount.Text}");
                return;
            }

            var filenames = await task;
            if (filenames is null) return;
            ImagePanel.Children.Clear();
            foreach (var filename in filenames.Take(count))
            {
                if (!_imageCache.TryGetValue(filename, out var image))
                {
                    image = new ImageControl
                    {
                        Source = ImageSourceFromPath(filename),
                        Margin = ImagePanel.ItemMargin,
                        MaxHeight = 150
                    };
                    _imageCache.Add(filename, image);
                }
                ImagePanel.Children.Add(image);
            }
        }
    }
}
