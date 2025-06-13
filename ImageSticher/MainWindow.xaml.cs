using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageStitcher
{
    public partial class MainWindow : Window
    {
        private string? _image1Path;
        private string? _image2Path;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Image Loading and Selection

        private void SelectImage1Button_Click(object sender, RoutedEventArgs e)
        {
            string? selectedPath = SelectImageFile();
            if (selectedPath != null)
            {
                LoadImage(selectedPath, 1);
            }
        }

        private void SelectImage2Button_Click(object sender, RoutedEventArgs e)
        {
            string? selectedPath = SelectImageFile();
            if (selectedPath != null)
            {
                LoadImage(selectedPath, 2);
            }
        }

        private void LoadImage(string filePath, int slot)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; 
                bitmap.EndInit();
                bitmap.Freeze(); 

                if (slot == 1)
                {
                    _image1Path = filePath;
                    Image1Preview.Source = bitmap;
                    StatusText.Text = "First image loaded. Please select or drop the second image.";
                }
                else if (slot == 2)
                {
                    _image2Path = filePath;
                    Image2Preview.Source = bitmap;
                    StatusText.Text = "Second image loaded. Ready to stitch.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load the image file.\n\nError: {ex.Message}", "Invalid Image", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string? SelectImageFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select an Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        #endregion

        #region UI Interaction Handlers

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            Image1Preview.Source = null;
            Image2Preview.Source = null;
            _image1Path = null;
            _image2Path = null;
            StatusText.Text = "Cleared. Please select two images.";
        }

        private void SwapButton_Click(object sender, RoutedEventArgs e)
        {
            (_image1Path, _image2Path) = (_image2Path, _image1Path);
            (Image1Preview.Source, Image2Preview.Source) = (Image2Preview.Source, Image1Preview.Source);

            StatusText.Text = "Images swapped.";
        }

        #endregion

        #region Drag and Drop Handlers

        private void Image_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                if (sender is Border border)
                {
                    border.BorderBrush = Brushes.DodgerBlue;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Image_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xcc, 0xcc, 0xcc)); 
            }
        }

        private void Image1_Drop(object sender, DragEventArgs e)
        {
            Image_DragLeave(sender, e); 
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                LoadImage(files[0], 1);
            }
        }

        private void Image2_Drop(object sender, DragEventArgs e)
        {
            Image_DragLeave(sender, e);
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                LoadImage(files[0], 2);
            }
        }

        #endregion

        #region Image Stitching Logic

        private async void StitchImagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (Image1Preview.Source == null || Image2Preview.Source == null)
            {
                MessageBox.Show("Please select two images before stitching.", "Missing Images", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                Title = "Save Stitched Image",
                FileName = $"stitched_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            bool isHorizontal = HorizontalRadio.IsChecked == true;
            bool isCenterAlign = AlignCenterRadio.IsChecked == true;
            bool isBottomRightAlign = AlignBottomRightRadio.IsChecked == true;
            BitmapSource bmp1 = (BitmapSource)Image1Preview.Source;
            BitmapSource bmp2 = (BitmapSource)Image2Preview.Source;
            string fileName = saveFileDialog.FileName;
            int filterIndex = saveFileDialog.FilterIndex;

            ActionPanel.IsEnabled = false;
            StatusText.Text = "Working... please wait.";

            try
            {
                await Task.Run(() =>
                {
                    var drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        int outputWidth = isHorizontal ? bmp1.PixelWidth + bmp2.PixelWidth : Math.Max(bmp1.PixelWidth, bmp2.PixelWidth);
                        int outputHeight = isHorizontal ? Math.Max(bmp1.PixelHeight, bmp2.PixelHeight) : bmp1.PixelHeight + bmp2.PixelHeight;

                        drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, outputWidth, outputHeight));

                        Rect rect1, rect2;
                        if (isHorizontal)
                        {
                            double y1 = CalculateAlignment(outputHeight, bmp1.PixelHeight, isCenterAlign, isBottomRightAlign);
                            double y2 = CalculateAlignment(outputHeight, bmp2.PixelHeight, isCenterAlign, isBottomRightAlign);
                            rect1 = new Rect(0, y1, bmp1.PixelWidth, bmp1.PixelHeight);
                            rect2 = new Rect(bmp1.PixelWidth, y2, bmp2.PixelWidth, bmp2.PixelHeight);
                        }
                        else 
                        {
                            double x1 = CalculateAlignment(outputWidth, bmp1.PixelWidth, isCenterAlign, isBottomRightAlign);
                            double x2 = CalculateAlignment(outputWidth, bmp2.PixelWidth, isCenterAlign, isBottomRightAlign);
                            rect1 = new Rect(x1, 0, bmp1.PixelWidth, bmp1.PixelHeight);
                            rect2 = new Rect(x2, bmp1.PixelHeight, bmp2.PixelWidth, bmp2.PixelHeight);
                        }

                        drawingContext.DrawImage(bmp1, rect1);
                        drawingContext.DrawImage(bmp2, rect2);
                    }

                    int finalWidth = isHorizontal ? bmp1.PixelWidth + bmp2.PixelWidth : Math.Max(bmp1.PixelWidth, bmp2.PixelWidth);
                    int finalHeight = isHorizontal ? Math.Max(bmp1.PixelHeight, bmp2.PixelHeight) : bmp1.PixelHeight + bmp2.PixelHeight;
                    var stitchedBitmap = new RenderTargetBitmap(finalWidth, finalHeight, 96, 96, PixelFormats.Pbgra32);
                    stitchedBitmap.Render(drawingVisual);
                    stitchedBitmap.Freeze();

                    BitmapEncoder encoder = filterIndex switch
                    {
                        2 => new JpegBitmapEncoder { QualityLevel = 90 },
                        3 => new BmpBitmapEncoder(),
                        _ => new PngBitmapEncoder(),
                    };

                    encoder.Frames.Add(BitmapFrame.Create(stitchedBitmap));

                    using (var fileStream = new FileStream(fileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                });

                StatusText.Text = $"Successfully saved to: {Path.GetFileName(fileName)}";
                MessageBox.Show($"Image saved successfully!\n\nPath: {fileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusText.Text = "An error occurred during stitching.";
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ActionPanel.IsEnabled = true;
            }
        }

        private double CalculateAlignment(double containerSize, double elementSize, bool isCenter, bool isBottomRight)
        {
            if (isCenter)
            {
                return (containerSize - elementSize) / 2;
            }
            if (isBottomRight)
            {
                return containerSize - elementSize;
            }
            return 0;
        }

        #endregion
    }
}
