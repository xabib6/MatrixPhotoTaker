using Antilatency.Factory.DatabaseAccessor;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Windows;

namespace MatrixPhotoTaker
{
    internal class DBConnect
    {
        private string _username;
        private string _password;
        private string _IP;
        public DBConnect(string username, string password, string IP)
        {
            _username= username;
            _password= password;
            _IP= IP;
        }


        public void AddReport(string FilePath, string serialNumber)
        {
            if (FilePath == null)
            {
                return;
            }
            DatabaseAccessTool databaseAccessTool = new DatabaseAccessTool(_username, _password, _IP);
            var data = new JObject();
            data["url"] = FilePath;
            databaseAccessTool.SendReport(serialNumber, DeviceTypeNames.MatrixM240HW01_1_8, ReportTypes.Test, data.ToString(), "photo");            
            databaseAccessTool.SaveChanges();            
        }

       public void Test()
        {
            MessageBox.Show("Enter");
            DatabaseAccessTool databaseAccessTool = new DatabaseAccessTool(_username, _password, _IP);

            var urlReport = databaseAccessTool.DatabaseReports;
            var Reports = urlReport.Where(d => d.ReportType == (int)ReportTypes.Test && d.ReportedDevice == 1000500405).ToList();

            foreach (var report in Reports)
            {
                MessageBox.Show(report.ReportData);
            }
        }
    }
}
