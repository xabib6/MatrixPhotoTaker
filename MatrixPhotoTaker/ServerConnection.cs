using FluentFTP;
using System.Windows;

namespace MatrixPhotoTaker
{
    internal class ServerConnection
    {
        private string _IP;
        private string _username;
        private string _password;
        private int _port;
        public ServerConnection(string IP, string username, string password, int port)
        {
            _IP = IP;
            _username = username;
            _password = password;
            _port = port;
        }

        public string? SendImageToServer(string FileName, string SerialNumber)
        {     
            byte[] imageBytes = new byte[0];
            var client = new FtpClient(_IP, new System.Net.NetworkCredential(_username, _password), _port);
            var result = client.AutoConnect();
            if (result == null)
            {
                MessageBox.Show("Failed to connect to server.");
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
                res = client.UploadFile(FileName, $"/{SerialNumber}"+$"-{photoNumber}.png");
            }
            catch(System.Exception e)
            {
                MessageBox.Show(e.InnerException.Message);
            }

            if (res == FtpStatus.Failed)
            {
                MessageBox.Show("Failed");
                return null;
            }
            else if (res == FtpStatus.Success)
            {
                MessageBox.Show("Photo sent");
            }
            SerialNumber += $"-{photoNumber}.png";
            return SerialNumber;

        }

    }
}
