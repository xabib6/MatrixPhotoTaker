using FluentFTP;
using System.Windows;

namespace MatrixPhotoTaker
{
    internal class ServerConnection
    {
        private string _IP;
        private string _username;
        private string _password;
        private int _Port;
        private static ServerConnection _connection;
        public ServerConnection(string IP, string username, string password, int Port)
        {
            _IP = IP;
            _username = username;
            _password = password;
            _Port = Port;
        }

        public string SendImageToServer(string FilePath, string SerialNumber)
        {
            if (_connection != null)
            {
                MessageBox.Show("Попытка подключения уже создана");
                return null;
            }
            _connection = this;
            byte[] imageBytes = new byte[0];
            var client = new FtpClient(_IP, new System.Net.NetworkCredential(_username, _password), _Port);
            var result = client.AutoConnect();
            if (result == null)
            {
                MessageBox.Show("Не удалось подключиться к серверу.");
                return null;
            }
            int photoNumber = 1;
            foreach (FtpListItem item in client.GetListing("/"))
            {
                if (item.Type == FtpObjectType.File)
                {
                    var OnlySerial = item.Name.Substring(0, item.Name.LastIndexOf('-'));
                    if (SerialNumber == OnlySerial) 
                    {
                        photoNumber++;
                    }
                }
            }
            FtpStatus res = FtpStatus.Skipped;
            try
            {
                res = client.UploadFile(FilePath, $"/{SerialNumber}"+$"-{photoNumber}.png");
            }
            catch(System.Exception e)
            {
                MessageBox.Show(e.InnerException.Message);
                
            }
            if (res == FtpStatus.Failed)
            {
                MessageBox.Show("Не удалось");
                _connection = null;
                return null;
            }
            else if (res == FtpStatus.Success)
            {
                MessageBox.Show("Фото отправлено");
            }
            SerialNumber += $"-{photoNumber}.png";
            _connection = null;
            MessageBox.Show(SerialNumber);
            return SerialNumber;

        }

    }
}
