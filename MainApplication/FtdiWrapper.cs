using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using FTD2XX_NET;
using System.Threading;

namespace MainApplication
{
    public sealed class FtdiWrapper
    {
        private static readonly FtdiWrapper instance = new FtdiWrapper();
        private FTDI myFtdiDevice = new FTDI();
        private FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;
        private bool initialized; // Indicate if the USB communication is initialized
        private bool error = false;

        public static FtdiWrapper Instance
        {
            get
            {
                return instance;
            }
        }

        public bool Initialize(out string message)
        // Desc: Initilize USB communication
        // Output: message: Message
        // bool: true = success, false = fail
        {
            uint ftdiDeviceCount = 0;
            uint i;
            uint myDeviceNum = 0;

            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList;
            FTDI.FT2232H_EEPROM_STRUCTURE myEEData = new FTDI.FT2232H_EEPROM_STRUCTURE();

            // Determine the number of FTDI devices connected to the machine
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                message = "FTDI GetNumberOfDevices Failed";
                return false;
            }
            if (ftdiDeviceCount == 0)
            {
                message = "No device found";
                return false;
            }
            // Get device info
            ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];
            ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                message = "FTDI GetDeviceList Failed";
                return false;
            }
            if (ftdiDeviceList[0] == null)
            {
                message = "FTDI GetDeviceList Failed";
                return false;
            }
            // Determine which device to use
            for (i = 0; i < ftdiDeviceCount; i++)
            {
                if (ftdiDeviceList[i].Description == "TTL232R")
                {
                    myDeviceNum = i;
                    break;
                }
            }
            if (i == ftdiDeviceCount)
            {
                message = "FTDI devices doesn't fit the description";
                return false;
            }
            // Open the selected device
            ftStatus = myFtdiDevice.OpenBySerialNumber(ftdiDeviceList[myDeviceNum].SerialNumber);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                message = "FTDI OpenBySerialNumber Failed";
                return false;
            }
            // Setup baud rate
            ftStatus = myFtdiDevice.SetBaudRate(1250000); // 9600
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                message = "FTDI SetBaudRate Failed";
                return false;
            }
            // Set data characteristics - Data bits, Stop bits, Parity
            ftStatus = myFtdiDevice.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                message = "FTDI SetDataCharacteristics Failed";
                return false;
            }
            // Set flow control - set RTS/CTS flow control
            ftStatus = myFtdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x00, 0x00);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                message = "FTDI SetFlowControl Failed";
                return false;
            }
            // Set read timeout, write timeout 
            ftStatus = myFtdiDevice.SetTimeouts(3000, 3000);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                message = "FTDI SetTimeouts Failed";
                return false;
            }          

            // Show result
            message = "Initialization complete";
            initialized = true;
            return true;                
        }

        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        public bool Error
        {
            get
            {
                return error;
            }
        }

        public bool EmptyReceiveBuffer()
        // Desc: Empty receive buffer
        {
            uint num_bytes_available = 0;
            uint num_bytes_read = 0;
            byte[] data_read;

            // Get number of available bytes to read
            ftStatus = myFtdiDevice.GetRxBytesAvailable(ref num_bytes_available);
            if (num_bytes_available == 0)
            {
                return true;
            }
            data_read = new byte[num_bytes_available];
            // Read available bytes
            ftStatus = myFtdiDevice.Read(data_read, num_bytes_available, ref num_bytes_read);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void GetTxBytesWaiting(ref uint num_bytes_write_queue)
        {
            myFtdiDevice.GetTxBytesWaiting(ref num_bytes_write_queue);
        }

        public bool Write(byte[] data_to_write)
        // Desc: Write to USB chip
        {
            uint num_bytes_written = 0;

            ftStatus = myFtdiDevice.Write(data_to_write, data_to_write.Length, ref num_bytes_written);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                //
                HandleError("FTDI write failed");
            }
            return true;
        }

        public bool Read(byte[] data_read, uint num_byte_to_read, ref uint num_byte_read)
        // Desc: Read from USB chip
        {
            ftStatus = myFtdiDevice.Read(data_read, num_byte_to_read, ref num_byte_read);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                HandleError("FTDI read failed");
            }
            return true;
        }

        public bool GetRxBytesAvailable(ref uint num_bytes_available)
        // Desc: Get number of available byte from USB chip
        {
            ftStatus = myFtdiDevice.GetRxBytesAvailable(ref num_bytes_available);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                //
                HandleError("FTDI GetRxBytesAvailable failed");
            }
            return true;
        }

        private void HandleError(string str)
        {
            // Indicate error
            error = true;
            // Display error message
            //MessageBox.Show(str);
            // Exit application
            Application.Exit();
        }
    }
}
