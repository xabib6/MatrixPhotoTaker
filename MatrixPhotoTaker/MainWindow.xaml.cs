using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using EOSDigital.API;
using EOSDigital.SDK;
using Microsoft.Win32;

namespace MatrixPhotoTaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string ImageSaveDirectory;
        static string MatrixSerialNumber = "ww";
        static ManualResetEvent WaitEvent = new ManualResetEvent(false);
        static CanonAPI APIHandler;
        static Camera MainCamera;
        public MainWindow()
        {
            InitializeComponent();
            Start();

        }

        static void Start()
        {
            ChangeDirectory();
            APIHandler = new CanonAPI();
            MainCamera = APIHandler.GetCameraList().FirstOrDefault();
            if (MainCamera == null )
            {
                MessageBox.Show("Камера не подключена");
            }
            MainCamera.DownloadReady += MainCamera_DownloadReady;
            MainCamera.OpenSession();
            MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);

            
        }

        private static void MainCamera_DownloadReady(Camera sender, DownloadInfo Info)
        {
            sender.DownloadFile(Info, ImageSaveDirectory);
        }

        static void ChangeDirectory()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Folders|\n";
            string folder = dialog.FileName;
            if (folder != null)
            {
                ImageSaveDirectory = Path.Combine(folder, MatrixSerialNumber);
            }
            else
            {
                ImageSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), MatrixSerialNumber);
            }
        }

        static void TakePhoto()
        {
            MainCamera.TakePhoto();

            WaitEvent.WaitOne();
        }

    }
}
