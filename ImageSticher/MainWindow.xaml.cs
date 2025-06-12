using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageStitcher
{
    public partial class MainWindow : Window
    {
        // Store the paths of the selected images
        private string? _image1Path;
        private string? _image2Path;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectImage1Button_Click(object sender, RoutedEventArgs e)
        {
            _image1Path = SelectImageFile();
            if (_image1Path != null)
            {
                // Display a preview of the selected image
                Image1Preview.Source = new BitmapImage(new Uri(_image1Path));
                StatusText.Text = "First image selected. Please select the second image.";
            }
        }

        private void SelectImage2Button_Click(object sender, RoutedEventArgs e)
        {
            _image2Path = SelectImageFile();
            if (_image2Path != null)
            {
                // Display a preview of the selected image
                Image2Preview.Source = new BitmapImage(new Uri(_image2Path));
                StatusText.Text = "Second image selected. Ready to stitch.";
            }
        }

        private void StitchImagesButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure both images have been selected before proceeding
            if (Image1Preview.Source == null || Image2Preview.Source == null || string.IsNullOrEmpty(_image1Path))
            {
                MessageBox.Show("Please select two images before stitching.", "Missing Images", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                StatusText.Text = "Stitching images...";

                // Get the BitmapSource objects from the image controls
                BitmapSource bmp1 = (BitmapSource)Image1Preview.Source;
                BitmapSource bmp2 = (BitmapSource)Image2Preview.Source;

                // Calculate the dimensions of the new, combined image
                int outputWidth = bmp1.PixelWidth + bmp2.PixelWidth;
                int outputHeight = Math.Max(bmp1.PixelHeight, bmp2.PixelHeight);

                // A DrawingVisual is a lightweight drawing class used to render shapes, images, or text.
                var drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    // Draw a white background rectangle to fill any space if images have different heights
                    drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, outputWidth, outputHeight));

                    // Draw the first image on the left
                    drawingContext.DrawImage(bmp1, new Rect(0, 0, bmp1.PixelWidth, bmp1.PixelHeight));

                    // Draw the second image to the right of the first one
                    drawingContext.DrawImage(bmp2, new Rect(bmp1.PixelWidth, 0, bmp2.PixelWidth, bmp2.PixelHeight));
                }

                // A RenderTargetBitmap can render a visual object (like our DrawingVisual) into a bitmap.
                var stitchedBitmap = new RenderTargetBitmap(outputWidth, outputHeight, 96, 96, PixelFormats.Pbgra32);
                stitchedBitmap.Render(drawingVisual);

                // Use a PngBitmapEncoder to save the file
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(stitchedBitmap));

                // Determine the output path
                string? directory = Path.GetDirectoryName(_image1Path);
                if (directory == null)
                {
                    MessageBox.Show("Could not determine the directory of the first image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string outputFileName = $"stitched_{timestamp}.png";
                string outputPath = Path.Combine(directory, outputFileName);

                // Save the stitched image to a file
                using (var fileStream = new FileStream(outputPath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }

                StatusText.Text = $"Successfully saved to: {outputPath}";
                MessageBox.Show($"Image saved successfully!\n\nPath: {outputPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusText.Text = "An error occurred during stitching.";
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Opens a file dialog to allow the user to select an image file.
        /// </summary>
        /// <returns>The file path of the selected image, or null if canceled.</returns>
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
    }
}
