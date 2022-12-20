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
            _username = username;
            _password = password;
            _IP = IP;
        }

        public static void Init()
        {
            DatabaseAccessTool databaseAccessTool = new DatabaseAccessTool(MainWindow.DBConnectionData[0], MainWindow.DBConnectionData[1], MainWindow.DBConnectionData[2]);
            MatrixList = databaseAccessTool.Devices.Where(d => d.DeviceType == (int)DeviceTypeNames.MatrixM240HW01_1_8).ToList();
        }
        public static string GetLast(string PCID)
        {
            DatabaseAccessTool databaseAccessTool = new DatabaseAccessTool(MainWindow.DBConnectionData[0], MainWindow.DBConnectionData[1], MainWindow.DBConnectionData[2]);
            var reports = databaseAccessTool.DatabaseReports.Where(d => d.Name == "matrixOnly").ToList();
            reports.Reverse();
            DatabaseReport lastReport = null;
            foreach (var report in reports)
            {
                var data = JObject.Parse(report.ReportData);
                if (data["PCID"] != null)
                {
                    var pcid = data["PCID"].ToString();
                    if (pcid == PCID)
                    {
                        lastReport = report;
                        break;
                    }
                }

                else
                {
                    MessageBox.Show("Для данного пк не найдено отчёта");
                    return null;
                }

            }


            var matrix = databaseAccessTool.Devices.First(d => d.Id == lastReport.ReportedDevice).SerialNumber;

            return matrix;
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
