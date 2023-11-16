using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.IO;
using System.Net.Http;

namespace WpfAppfile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlTextBox.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Введите URL");
                return;
            }

            try
            {
                using (HttpClient httpClient = new HttpClient())
                using (HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    long? contentLength = response.Content.Headers.ContentLength;
                    string fileName = GetFileNameFromContentDisposition(response) ?? System.IO.Path.GetFileName(url);
                    string mimeType = GetMimeType(System.IO.Path.GetExtension(fileName));
                    string uniqueFileName = $"{Guid.NewGuid():N}{System.IO.Path.GetExtension(fileName)}";
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string filePath = System.IO.Path.Combine(desktopPath, uniqueFileName);

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    using (FileStream streamToWriteTo = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        long totalBytesRead = 0;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await streamToWriteTo.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                            UpdateProgressBar((double)totalBytesRead / contentLength.Value * 100);
                        }
                    }

                    MessageBox.Show($"Файл успешно загружен.\nСохранено по пути: {filePath}\nMIME-тип: {mimeType}", "Успех");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateProgressBar(double value)
        {
            Dispatcher.Invoke(() => ProgressBar.Value = value);
        }

        private string GetMimeType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".txt": return "text/plain";
                case ".mp3": return "audio/mpeg";
                default: return "application/octet-stream";
            }
        }

        private string GetFileNameFromContentDisposition(HttpResponseMessage response) =>
            response.Content.Headers.ContentDisposition?.FileName;
    }
}