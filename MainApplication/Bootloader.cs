using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FTD2XX_NET;
using System.Globalization;
using System.Windows.Forms;
using System.Threading;

namespace MainApplication
{
    public sealed class Bootloader
    {
        private static readonly Bootloader instance = new Bootloader();
        private string hexFileLocation; // Location of hex file

        public static Bootloader Instance
        {
            get
            {
                return instance;
            }
        }

        public string HexFileLocation
        {
            get
            {
                return hexFileLocation;
            }
            set
            {
                hexFileLocation = value;
            }
        }

        public void DisplayPageData(int start_address, ref byte[] data)
        // Desc: Display one page data
        {
            int i;

            Console.WriteLine("Displaying data\n");
            for (i = 0; i < 64 * 8 * 3; i = i + 12)
            {
                Console.Write("0x{0:x6}: {1:x2}{2:x2}{3:x2} {4:x2}{5:x2}{6:x2} {7:x2}{8:x2}{9:x2} {10:x2}{11:x2}{12:x2}\n",
                    start_address + i*2/3
                    , data[i], data[i+1], data[i+2], data[i+3], data[i+4], data[i+5], data[i+6], data[i+7],
                    data[i+8], data[i+9], data[i+10], data[i+11]);
            }

        }

        public void ReadPM(int start_address, byte[] page_read)
        // Read one page from program memory
        // Input: start_address: start address for read
        // Output: page_read: Data read
        {
            byte[] data_to_write = new byte[4];

            // Program read command = 2
            data_to_write[0] = 2;
            // Starting location for read
            data_to_write[1] = (byte)start_address; // LSByte
            data_to_write[2] = (byte)(start_address >> 8);
            data_to_write[3] = (byte)(start_address >> 16); //MSByte
            // Send request for read
            FtdiWrapper.Instance.Write(data_to_write);
            // read page
            ReceivePage(page_read);            
        }

        public void ReceivePage(byte[] page_read)
        // Desc: Receive one page data from USB buffer
        // Output: page_read: Data received
        {
            uint num_bytes_read = 0;
            uint num_bytes_available = 0;

            // Wait till one page data is available
            while (num_bytes_available < 64 * 8 * 3)
            {
                FtdiWrapper.Instance.GetRxBytesAvailable(ref num_bytes_available);
            }
            // Read data
            FtdiWrapper.Instance.Read(page_read, 64 * 8 * 3, ref num_bytes_read);
        }

        public bool ReceiveAck()
        // Desc: Attempt to receive "acknoldge"
        {
            uint num_bytes_read = 0;
            uint num_bytes_available = 0;
            byte[] ack = new byte[1];
            int sleep_count = 0;
            uint num_bytes_write_queue = 0;

            // While no data is available
            while (num_bytes_available == 0)
            {
                // Check number of available bytes
                FtdiWrapper.Instance.GetRxBytesAvailable(ref num_bytes_available);
                // Wait a bit
                Thread.Sleep(1);
                // Increment count
                sleep_count++;
                // Check timeout
                if (sleep_count > 1000)
                {
                    // Error
                    Console.WriteLine("Acknowledge timeout");
                    return false;
                }
            }
            // Make sure that all data were read by dsPIC
            FtdiWrapper.Instance.GetTxBytesWaiting(ref num_bytes_write_queue);
            if (num_bytes_write_queue > 0)
            {
                Console.WriteLine("Not all bytes were read by dsPIC");
            }
            // Make sure that only one byte is available
            if (num_bytes_available != 1)
            {
                // Error
                Console.WriteLine("Incorrect number of bytes for acknowledge");
                return false;
            }
            // Check if data is "ACK"
            FtdiWrapper.Instance.Read(ack, 1, ref num_bytes_read);
            if (ack[0] == Const.COMMAND_ACK)
            {
                // Success
                return true;
            }
            else
            {
                // Error
                Console.WriteLine("Incorrect acknowledge");
                return false;
            }
        }

        public bool CheckReadyness()
        // Desc: Check if bootloader is ready
        {
            byte[] data_to_write = new byte[1];
            bool success;

            // Empty buffer
            FtdiWrapper.Instance.EmptyReceiveBuffer();
            // Send request for Check ready
            data_to_write[0] = 4;
            FtdiWrapper.Instance.Write(data_to_write);
            // Attempt to receive ACK
            success = ReceiveAck(); 
            // Empty buffer
            FtdiWrapper.Instance.EmptyReceiveBuffer();
            return success;
        }

        private void GetStartAddress(byte[] page_data, out int start_address)
        // Desc: Get start address from the initial page in PM
        {
            start_address = (int)page_data[2] + ((int)(page_data[1]) << 8) + ((int)(page_data[0]) << 16);
        }
        
        private bool CreateBuffers(int start_address, byte[] code_buf, byte[] config_buf)
        // Desc: Create buffer to send for programing with bootloader
        // Input: start_address: Start address of bootloader program (should not be overwritten)
        // Output: code_buf: Data for user application
        //         config_buf: Data for configuration register
        //         bool: true = success, false = fail
        {
            StreamReader sr;
            LineInfo li = new LineInfo();
            int i;
            int upper_address = 0;
            bool eof = false;

            // Initialize code_buf
            for (i = 0; i < code_buf.Length; i++)
            {
                code_buf[i] = 0xff;
            }
            code_buf[0] = (byte)start_address; // LSB              
            code_buf[1] = (byte)(start_address >> 8);
            code_buf[2] = (byte)(start_address >> 16); // MSB          
            code_buf[3] = 0x00;
            code_buf[4] = 0x00;
            code_buf[5] = 0x00;
            code_buf[0x0C00 * 3 / 2] = 0x00; // Delay value for bootloader
            // Initialize config_buf
            for (i = 0; i < config_buf.Length; i++)
            {
                config_buf[i] = 0xff;
            }
            // Open hex file
            sr = File.OpenText(hexFileLocation);
            while (eof == false)
            {
                // Decode next line(s)
                if (DecodeLine(sr, ref upper_address, ref eof, ref li) == true)
                {
                    // Add initialization location (start of user application) to code_buf
                    if (li.address == 0)
                    {
                        for (i = 0; i < li.data.Length; i++)
                        {
                            code_buf[0x0C02 * 3 / 2 + i] = li.data[i];
                        }
                    }
                    // Add to code_buff
                    else if (((0x4 <= li.address) && (li.address < 0x200) || (0xC00 <= li.address) && (li.address < 0x15800)))
                    {
                        for (i = 0; i < li.data.Length; i++)
                        {
                            code_buf[li.address * 3 / 2 + i] = li.data[i];
                        }
                    }
                    // Add to config_buf
                    else if ((li.address >= 0xf80000) && (li.address < 0xf80010))
                    {
                        config_buf[(li.address - 0xf80000) * 3 / 2] = 0x00;
                        config_buf[(li.address - 0xf80000) * 3 / 2 + 1] = li.data[0];  // LSB
                        config_buf[(li.address - 0xf80000) * 3 / 2 + 2] = li.data[1];  // MSB
                    }
                    else
                    {
                        MessageBox.Show("Invalid memory region to write");
                        return false;
                    }
                }
            }
            sr.Close();
            return true;
        }

        private bool WritePM(int start_address, byte[] write_page_buf)
        // Desc: Write one page to program memory
        // Input: start_address: Start address of the page
        //        write_page_buf: Data to write
        // Output: bool: true = success, false = fail
        {
            byte[] data_to_write = new byte[4];         

            // Send Program command = 3
            data_to_write[0] = 3;
            // Starting location for program
            data_to_write[1] = (byte)start_address; // LSByte
            data_to_write[2] = (byte)(start_address >> 8);
            data_to_write[3] = (byte)(start_address >> 16); //MSByte
            // Send write command
            FtdiWrapper.Instance.Write(data_to_write);
            // Send page 
            FtdiWrapper.Instance.Write(write_page_buf);
            // Receive Acknoledge
            return ReceiveAck();
        }

        private bool WriteConfig(byte[] config_data)
        // Desc: Write to configuration register
        // Input: config_data: data to write
        // Output: bool: true = success, false = fail
        {
            byte[] data_to_write = new byte[1];

            // Program command = 7
            data_to_write[0] = 7;
            // Send write command
            FtdiWrapper.Instance.Write(data_to_write);
            // Send config data 
            FtdiWrapper.Instance.Write(config_data);
            // Receive Acknoledge
            return ReceiveAck();
        }

        private bool WriteBuffers(byte[] code_buf, byte[] config_buf)
        // Desc: Write user application and configuration data to dsPIC
        // Input: code_buf: User application data
        //        config_buf: Configuration register data
        // Output: bool: true = success, false = fail
        {
            int i, j;
            byte[] temp_buf = new byte[64 * 8 * 3];

            //MessageBox.Show("p6");

            // Write code_buf
            for(i=0;i < 86;i++)
            {
                //i = 9;
                // Skip page 1 and 2 (bootloader resides here)
                if ((i != 1) && (i != 2))
                {
                    // Copy one page to temporary storage
                    for (j = 0; j < 64 * 8 * 3; j++)
                    {
                        temp_buf[j] = code_buf[i * 64 * 8 * 3 + j];
                    }
                    // Write the page
                    if (WritePM(0x0400 * i, temp_buf) == false)
                    {
                        return false;
                    }
                }
            }
            // Write config_buf
            return WriteConfig(config_buf);
        }

        public bool Program()
        // Desc: Program dsPIC with bootloader
        // Output: bool: true = success, false = fail
        {            
            byte[] read_page_buf = new byte[64 * 8 * 3];
            byte[] config_buf = new byte[8*3];
            int start_address;
            byte[] code_buf = new byte[64*8*3*86];

            // Read PM page at 0x000000 (to get the "goto" address)
            Console.WriteLine("Reading PM at 0x000000");
            ReadPM(0x0000, read_page_buf);
            // Get start address
            GetStartAddress(read_page_buf, out start_address);
            // Create buffers to send
            Console.WriteLine("Creating buffers");
            if (CreateBuffers(start_address, code_buf, config_buf) == true)
            {
                // Write buffers
                Console.WriteLine("Writing buffers");
                return WriteBuffers(code_buf, config_buf);
            }
            else
            {
                return false;
            }      
        }
                
        private bool DecodeLine(StreamReader sr, ref int upper_address, ref bool eof, ref LineInfo li)
        // Desc: Decode one line in hex file
        // Input: sr: Stream for hex file
        //        upper_address: Upper address
        //        eof: End of file indicator
        // Output: li: Line inforamtion
        {
            byte[] data;
            int i;
            string s;
            string line;

            // Read one line
            line = sr.ReadLine();
            // Get record type
            s = line.Substring(7, 2);
            li.recordType = int.Parse(s, NumberStyles.AllowHexSpecifier);
            // Deal with extended address
            while (li.recordType == 04)
            {
                s = line.Substring(9, 4);
                upper_address = int.Parse(s, NumberStyles.AllowHexSpecifier);
                line = sr.ReadLine();
                s = line.Substring(7, 2);
                li.recordType = int.Parse(s, NumberStyles.AllowHexSpecifier);
            }
            // Check if last line
            if (li.recordType == 1)
            {
                eof = true;
                return false;
            }
            // Get line inforamtion
            s = (line.Substring(1, 2));
            li.byteLen = int.Parse(s, NumberStyles.AllowHexSpecifier);
            s = line.Substring(3, 4);
            li.address = (int.Parse(s, NumberStyles.AllowHexSpecifier) + (upper_address << 16))/2;
            // Store data in li.data
            if (li.byteLen >= 4)
            {
                data = new byte[li.byteLen*3/4];
                for (i = 0; i < li.byteLen; i = i + 4)
                {
                    s = line.Substring(9 + 2 * i, 2); // LSB
                    data[i * 3 / 4 + 0] = (byte)int.Parse(s, NumberStyles.AllowHexSpecifier);                     
                    s = line.Substring(11 + 2*i, 2);
                    data[i*3/4 + 1] = (byte)int.Parse(s, NumberStyles.AllowHexSpecifier);
                    s = line.Substring(13 + 2 * i, 2); // MSB
                    data[i * 3 / 4 + 2] = (byte)int.Parse(s, NumberStyles.AllowHexSpecifier);                    
                }
                li.data = data;
            }
            return true;
        }

        public void GoToUserApplication()
        {
            byte[] data_to_write = new byte[1];

            // Send request to go to user application
            data_to_write[0] = 8;
            FtdiWrapper.Instance.Write(data_to_write);
        }
    }

    struct LineInfo
    {
        public int byteLen;
        public int address;
        public int recordType;
        public byte[] data;
    }
}
