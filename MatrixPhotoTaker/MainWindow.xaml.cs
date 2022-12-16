using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EOSDigital.API;
using EOSDigital.SDK;

namespace MatrixPhotoTaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
              
        static ManualResetEvent WaitEvent = new ManualResetEvent(false);
        static CanonAPI APIHandler;
        static Camera MainCamera;
        static string SerialNumber;
        static string _fileName;
        static bool _isSessionOpen;
        static ImageBrush imageBrush;
        public MainWindow()
        {
            InitializeComponent();
            Start();
            DBConnect.Init();
        }

        static void Start()
        {            
            APIHandler = new CanonAPI();
            MainCamera = APIHandler.GetCameraList().FirstOrDefault();
            if (MainCamera == null )
            {
                MessageBox.Show("Камера не подключена");
                return;
            }
        }

        private void OpenSession_Click(object sender, RoutedEventArgs e)
        {
            if (MainCamera == null)
            {
                MainCamera = APIHandler.GetCameraList().FirstOrDefault();
            }
            if (MainCamera == null)
            {
                MessageBox.Show("Камера не подключена");
                return;
            }
            imageBrush = MatrixImage;
            if (SerialNumber != null)
            {
                TakePhoto.IsEnabled = true;
            }
            OpenSession();
        }

        private void OpenSession()
        {
            _isSessionOpen = true;
            MainCamera.OpenSession();
            MainCamera.DownloadReady += MainCamera_DownloadReady;
            MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
            MainCamera.SetCapacity(4096, int.MaxValue);
            CameraValue[] AvList;
            CameraValue[] TvList;
            CameraValue[] ISOList;
            AvList = MainCamera.GetSettingsList(PropertyID.Av);
            TvList = MainCamera.GetSettingsList(PropertyID.Tv);
            ISOList = MainCamera.GetSettingsList(PropertyID.ISO);
            foreach (var Av in AvList) AvCoBox.Items.Add(Av.StringValue);
            foreach (var Tv in TvList) TvCoBox.Items.Add(Tv.StringValue);
            foreach (var ISO in ISOList) ISOCoBox.Items.Add(ISO.StringValue);
            AvCoBox.SelectedIndex = AvCoBox.Items.IndexOf(AvValues.GetValue(MainCamera.GetInt32Setting(PropertyID.Av)).StringValue);
            TvCoBox.SelectedIndex = TvCoBox.Items.IndexOf(TvValues.GetValue(MainCamera.GetInt32Setting(PropertyID.Tv)).StringValue);
            ISOCoBox.SelectedIndex = ISOCoBox.Items.IndexOf(ISOValues.GetValue(MainCamera.GetInt32Setting(PropertyID.ISO)).StringValue);
            AvSettings.Text = AvCoBox.SelectedItem.ToString();
            TvSettings.Text = TvCoBox.SelectedItem.ToString();
            ISOSettings.Text = ISOCoBox.SelectedItem.ToString();
        }


        private void TakePhoto_Click(object sender, RoutedEventArgs e)
        {
            MainCamera.TakePhotoAsync();
            SendPhoto.IsEnabled = true;
            imageBrush.ImageSource= null;
            Thread.Sleep(2000);
            ChangePreviewPhoto();
        }

        private void ChangeSerialNumber_Click(object sender, RoutedEventArgs e)
        {
            SendPhoto.IsEnabled = false;
            if (SerialNumberBox.Text == string.Empty)
            {
                MessageBox.Show("Серийный номер не может быть пустым");
                return;
            }

            DBConnect dbConnection = new DBConnect("postgres", "admin", "192.168.222.104");
            if (dbConnection.SerialNumberExsist(SerialNumberBox.Text) == false)
            {
                MessageBox.Show("Матрица с таким серийным номером не существует");
                SerialNumberBox.Text = string.Empty;
                return;
            }

            MatrixImage.ImageSource = null;
            if (_isSessionOpen == true)
            {
                TakePhoto.IsEnabled = true;
            }

            SerialNumberText.Text = SerialNumberBox.Text;
            SerialNumber = SerialNumberBox.Text;
            DeleteTempPhoto();
            _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto\\") + SerialNumber + ".png";
            SerialNumberBox.Text = string.Empty;
        }

        private void SendPhoto_Click(object sender, RoutedEventArgs e)
        {
            ServerConnection connection = new ServerConnection("192.168.222.250", "admin", "admin", 22);
            string FilePathOnServer = connection.SendImageToServer(_fileName, SerialNumber);
            DBConnect dbConnection=  new DBConnect( "postgres", "admin","192.168.222.104");
            dbConnection.AddReport(FilePathOnServer, SerialNumber);
            SendPhoto.IsEnabled = false;
        }    

        private void ChangeSettings_Click(object sender, RoutedEventArgs e)
        {
            AvCoBox.IsEnabled = true;
            ISOCoBox.IsEnabled = true;
            TvCoBox.IsEnabled = true;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            MainCamera.SetSetting(PropertyID.Av, AvValues.GetValue((string)AvCoBox.SelectedItem).IntValue);
            MainCamera.SetSetting(PropertyID.Tv, TvValues.GetValue((string)TvCoBox.SelectedItem).IntValue);
            MainCamera.SetSetting(PropertyID.ISO, ISOValues.GetValue((string)ISOCoBox.SelectedItem).IntValue);
            
            AvCoBox.IsEnabled = false;
            ISOCoBox.IsEnabled = false;
            TvCoBox.IsEnabled = false;            
        }


        private System.Windows.Media.ImageSource GetCopy()
        {
            Thread.Sleep(1000);
            byte[] imgBytes = File.ReadAllBytes(_fileName);
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imgBytes);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            System.Windows.Media.ImageSource imgSrc = biImg as System.Windows.Media.ImageSource;

            return imgSrc;
        }

        private static void DeleteTempPhoto()
        {
            if (File.Exists(_fileName) == true)
            {
                File.Delete(_fileName);
            }
        }

        private void ChangePreviewPhoto()
        {
            if (File.Exists(_fileName))
            {
                MatrixImage.ImageSource = GetCopy();
            }
        }

        private static void MainCamera_DownloadReady(Camera sender, DownloadInfo Info)
        {
            DeleteTempPhoto();
            Info.FileName = SerialNumber + ".png";
            var ImageSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto");
            sender.DownloadFile(Info, ImageSaveDirectory);
            MainCamera.SetCapacity(4096, int.MaxValue);
        }

        private void Grid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
        }
    }
}
