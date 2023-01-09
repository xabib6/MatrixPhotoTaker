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

        public string? SendFile(string FileName, string SerialNumber, out string Result, string fileFormat = ".png")
        {     
            byte[] imageBytes = new byte[0];
            var client = new FtpClient(_IP, new System.Net.NetworkCredential(_username, _password), _port);
            var result = client.AutoConnect();
            if (result == null)
            {
                Result = "Failed to connect to server.";
                return null;
            }
            int fileNumber = 1;
            foreach (FtpListItem item in client.GetListing("/"))
            {
                if (item.Type == FtpObjectType.File)
                {
                    var OnlySerial = item.Name.Substring(0, item.Name.LastIndexOf('-'));
                    if (SerialNumber == OnlySerial) 
                    {
                        fileNumber++;
                    }
                }
            }
            FtpStatus res = FtpStatus.Skipped;
            try
            {
                res = client.UploadFile(FileName, $"/{SerialNumber}"+$"-{fileNumber}{fileFormat}");
            }
            catch(System.Exception e)
            {
                Result =  e.InnerException?.Message;
                return null;
            }

            if (res == FtpStatus.Failed)
            {
                Result = "Failed to upload file";
                return null;
            }
            
            SerialNumber += $"-{fileNumber}{fileFormat}";
            Result = "Success";
            return SerialNumber;

        }

    }
}
