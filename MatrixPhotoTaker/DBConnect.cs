using Antilatency.Factory.DatabaseAccessor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatrixPhotoTaker
{
    internal class DBConnect
    {
        private List<Device> MatrixList;

        private DeviceTypeNames _deviceType = DeviceTypeNames.MatrixM240HW01_1_8;

        private DatabaseAccessTool _databaseAccessTool;
        public DBConnect(string username, string password, string IP)
        {
            _databaseAccessTool = new DatabaseAccessTool(username, password, IP);
        }

        public void Init()
        {            
            if (_databaseAccessTool.IsConnectoinAvailable == false)
            {
                return;
            }
            MatrixList = _databaseAccessTool.Devices.Where(d => d.DeviceType == Convert.ToInt32(_deviceType)).ToList();
        }
        public string? GetLast(string MachineID)
        {            
            var reports = _databaseAccessTool.DatabaseReports.Where(d => d.Name == "matrixOnly").OrderByDescending(d => d.Date).ToList();
            DatabaseReport? lastReport = null;
            foreach (var report in reports)
            {
                var data = JObject.Parse(report.ReportData);

                var machineID = data["MachineID"]?.ToString();
                if (machineID == MachineID)
                {
                    lastReport = report;
                    break;
                }

                else
                {
                    return null;
                }

            }
            if (lastReport == null)
            {
                return null;
            }

            var matrix = _databaseAccessTool.Devices.First(d => d.Id == lastReport.ReportedDevice).SerialNumber;

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
            var data = new JObject();
            data["url"] = FilePath;
            try
            {
                _databaseAccessTool.SendReport(serialNumber, _deviceType, ReportTypes.Test, data.ToString(), "photo");
                _databaseAccessTool.SaveChanges();
            }
            catch(Exception ex)
            {

            }

        }


    }
}
