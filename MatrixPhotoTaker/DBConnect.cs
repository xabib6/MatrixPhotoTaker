using Antilatency.Factory.DatabaseAccessor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace MatrixPhotoTaker
{
    internal class DBConnect
    {
        static List<Device> MatrixList;

        private string _username;
        private string _password;
        private string _IP;
        public DBConnect(string username, string password, string IP)
        {
            _username= username;
            _password= password;
            _IP= IP;
        }

        public static void Init()
        {
            DatabaseAccessTool databaseAccessTool = new DatabaseAccessTool("postgres", "admin", "192.168.222.104");
            MatrixList = databaseAccessTool.Devices.Where(d => d.DeviceType == (int)DeviceTypeNames.MatrixM240HW01_1_8).ToList();
        }

        public bool SerialNumberExsist(string serialNumber)
        {
            var Matrix = MatrixList.FirstOrDefault(d => d.SerialNumber == serialNumber);
            return Matrix != null;
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
            try
            {
                databaseAccessTool.SendReport(serialNumber, DeviceTypeNames.MatrixM240HW01_1_8, ReportTypes.Test, data.ToString(), "photo");
                databaseAccessTool.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);                
            }
                       
        }

       
    }
}
