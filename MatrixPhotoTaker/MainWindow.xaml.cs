using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EOSDigital.API;
using EOSDigital.SDK;

namespace MatrixPhotoTaker
{
    public partial class MainWindow : Window
    {

        private static ManualResetEvent WaitEvent = new ManualResetEvent(false);
        private static CanonAPI APIHandler;
        private static Camera MainCamera;
        private static string SerialNumber;

        private static string _fileName;
        private static bool _isSessionOpen;
        private static float _delay = 5f;
        private string _MachineID;
        private DBConnect _dbConnection;
        private static string TempPhotoFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto\\");

        public MainWindow()
        {
            InitializeComponent();



            CurrentDelay.Text = _delay.ToString();

            _dbConnection = new DBConnect("tester", "user", "192.168.222.58");
            _dbConnection.Init();

            _MachineID = MachineID.GetId();
            
            if (_MachineID == null)
            {
                MessageBox.Show("Set ID in MatrixTest program");
                RefreshMachineID.IsEnabled = true;
            }
            else
            {
                MessageBox.Show(_MachineID);
                ConnectToCameraButton.IsEnabled = true;
                GetLastMatrix.IsEnabled= true;
            }

            if (File.Exists(TempPhotoFolder))
            {
                File.Delete(TempPhotoFolder);
            }
        }

        

        private void OpenSession_Click(object sender, RoutedEventArgs e)
        {
            APIHandler = new CanonAPI();
            if (MainCamera == null)
            {
                MainCamera = APIHandler.GetCameraList().FirstOrDefault();
            }
            if (MainCamera == null)
            {
                MessageBox.Show("Camera doesn't connected");
                return;
            }
           
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
            ConnectToCameraButton.IsEnabled= false;
        }


        private async void TakePhoto_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < TakingPhotoDelay.Maximum; i++)
            {
                TakingPhotoDelay.Value++;
                await Task.Delay((int)(_delay/TakingPhotoDelay.Maximum*1000));
            }

            MainCamera.TakePhotoAsync();
            TakingPhotoDelay.Value = 0;
            SendPhoto.IsEnabled = true;
            MatrixImage.ImageSource = null;
            Thread.Sleep(2000);
            ChangePreviewPhoto();
        }

        private void ChangeSerialNumber_Click(object sender, RoutedEventArgs e)
        {
            SendPhoto.IsEnabled = false;
            if (SerialNumberBox.Text == string.Empty)
            {
                MessageBox.Show("Serial number can't be empty");
                return;
            }

            if (_dbConnection.SerialNumberExsist(SerialNumberBox.Text) == false)
            {
                MessageBox.Show("This matrix doesn't exist");
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
            _fileName = TempPhotoFolder + SerialNumber + ".png";
            SerialNumberBox.Text = string.Empty;
        }

        private void SendPhoto_Click(object sender, RoutedEventArgs e)
        {
            ServerConnection connection = new ServerConnection("192.168.222.250", "admin", "admin", 22);
            string FilePathOnServer = connection.SendImageToServer(_fileName, SerialNumber);
            _dbConnection.AddReport(FilePathOnServer, SerialNumber);
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

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SerialNumberBox.IsFocused == true)
                {
                    ChangeSerialNumber_Click(sender, e);
                }
                else if (ChangeDelayBox.IsFocused == true)
                {
                    ChangeDelayButton_Click(sender, e);
                }                
            }
        }

        private void ChangeDelayButton_Click(object sender, RoutedEventArgs e)
        {

            if (float.TryParse(ChangeDelayBox.Text, System.Globalization.NumberStyles.Currency, null, out _delay) == true)
            {
                CurrentDelay.Text = ChangeDelayBox.Text;
                ChangeDelayBox.Text = string.Empty;
            }
            else
            {
                MessageBox.Show("Uncorrect delay input");
                ChangeDelayBox.Text = string.Empty;
            }
        }

        private void GetLastMatrix_Click(object sender, RoutedEventArgs e)
        {
            if (_MachineID == null)
            {
                MessageBox.Show("No ID for this PC");
                return;
            }
            _dbConnection.Init();
            string lastMatrix = _dbConnection.GetLast(_MachineID);
            if (lastMatrix == null)
            {
                MessageBox.Show("There is no reviewed matrix on this PC");
                return;
            }
            SerialNumberBox.Text = lastMatrix;
            ChangeSerialNumber_Click(sender, e);
        }

        private void RefreshMachineID_Click(object sender, RoutedEventArgs e)
        {
            _MachineID = MachineID.GetId();
            if (_MachineID != null)
            {
                RefreshMachineID.IsEnabled = false;
                ConnectToCameraButton.IsEnabled = true;
                GetLastMatrix.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("No ID set for this PC");
            }
        }
    }
}
