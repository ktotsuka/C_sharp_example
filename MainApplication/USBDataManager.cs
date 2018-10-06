using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FTD2XX_NET;
using System.Windows.Forms;
using NonPlotItemSpace;
using PlotItemSpace;
using System.Diagnostics;

namespace MainApplication
{  
    public sealed class USBDataManager
    {
        private static readonly USBDataManager instance = new USBDataManager();
        private Byte[] leftover;

        public static USBDataManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void GoToBootloader()
        {
            to_dspic_header header;
            byte[] data_to_write = new byte[4];

            // Make sure that bootloader program is not running
            //if (Bootloader.Instance.CheckReadyness() == false)
            //{
                //MessageBox.Show("Forcing to go to Bootloader");

                // Put start marker
                data_to_write[0] = Const.PACKET_START;
                // Put number of remaining bytes
                data_to_write[1] = 2; // 1: header, 1: Packet_end
                // Create header
                header = to_dspic_header.bootloader;
                data_to_write[2] = (byte)header;
                // Put end marker
                data_to_write[3] = Const.PACKET_END;
                // Send data
                FtdiWrapper.Instance.Write(data_to_write);
            //}            
        }

        public void EmptyReceiveBuffer()
        {
            // Empty receive buffer
            FtdiWrapper.Instance.EmptyReceiveBuffer();
            // Remove leftover
            leftover = null;
        }

        public void SendNonPlotDataAssignments()
        {
            to_dspic_header header;
            byte[] data_to_write = new byte[5 + Const.VAR_SIZE * NonPlotItems.Instance.Count]; // 1: start, 1, num_rem_bytes, 1: header, 1: number of var, 4*num var: address, 1: end
            int i;
            VariableInfo vi;

            // Exit if there are no non plot item
            if (NonPlotItems.Instance.Count == 0)
            {
                return;
            }
            // Put start marker
            data_to_write[0] = Const.PACKET_START;
            // Put number of remaining bytes
            data_to_write[1] = (byte)(3 + (Const.VAR_SIZE * NonPlotItems.Instance.Count)); // 1: header, 1: Num of variable, 1: Packet_end, ...
            // Create header
            header = to_dspic_header.assign_non_plot;
            data_to_write[2] = (byte)header;
            // Put number of non-plot variables
            data_to_write[3] = (byte)(NonPlotItems.Instance.Count);
            // Put addresses
            for (i = 0; i < NonPlotItems.Instance.Count; i++)
            {
                vi = VariableManager.Instance.GetVariable(NonPlotItems.Instance[i].VariableName);
                data_to_write[4 + Const.VAR_SIZE * i] = (byte)(vi.address);
                data_to_write[5 + Const.VAR_SIZE * i] = (byte)(vi.address >> 8);
                data_to_write[6 + Const.VAR_SIZE * i] = (byte)(vi.address >> 16);
                data_to_write[7 + Const.VAR_SIZE * i] = (byte)(vi.address >> 24);
            }
            // Put end marker
            data_to_write[4 + Const.VAR_SIZE * NonPlotItems.Instance.Count] = Const.PACKET_END;
            // Send data
            FtdiWrapper.Instance.Write(data_to_write);
        }

        public void SendPlotDataAssignments()
        {
            int i;
            int j;
            List<uint>[] plot_data_addresses;
            byte[] data_to_write;
            to_dspic_header header;

            // Exit if there are no items
            if (PlotItems.Instance.Count == 0)
            {
                return;
            }
            // Get plot data addresses
            SubscriptionTableManager.Instance.GetPlotDataAddresses(out plot_data_addresses);
            // For each capture period
            for (i = 0; i < plot_data_addresses.Length; i++)
            {
                // Instantiate
                data_to_write = new byte[Const.VAR_SIZE * plot_data_addresses[i].Count + 6]; // 1: start, 1: num_rem_bytes, 1: header, 1: capture_period_num, 1: number of var, 4*num var: addresses, 1: end
                // Put start marker
                data_to_write[0] = Const.PACKET_START;
                // Put number of remaining bytes
                data_to_write[1] = (byte)(data_to_write.Length - 2); // 1: start, 1: num_rem_bytes
                // Create header
                header = to_dspic_header.assign_plot;
                data_to_write[2] = (byte)header;
                // Put capture period number
                data_to_write[3] = (byte)i;
                // Put number of plot variables
                data_to_write[4] = (byte)(plot_data_addresses[i].Count);
                // Put addresses
                for (j = 0; j < plot_data_addresses[i].Count; j++)
                {
                    data_to_write[5 + Const.VAR_SIZE * j] = (byte)(plot_data_addresses[i][j]);
                    data_to_write[6 + Const.VAR_SIZE * j] = (byte)(plot_data_addresses[i][j] >> 8);
                    data_to_write[7 + Const.VAR_SIZE * j] = (byte)(plot_data_addresses[i][j] >> 16);
                    data_to_write[8 + Const.VAR_SIZE * j] = (byte)(plot_data_addresses[i][j] >> 24);
                }
                // Put end marker
                data_to_write[5 + Const.VAR_SIZE * plot_data_addresses[i].Count] = Const.PACKET_END;
                // Send data
                FtdiWrapper.Instance.Write(data_to_write);
            }
        }

        public void NonPlotItemChanged(object sender, byte[] data_in_bytes)
        {
            to_dspic_header header;
            byte[] data_to_write = new byte[12];
            string var_name;
            VariableInfo vi;
            NonPlotItem item;

            // Get non plot item
            item = (NonPlotItem)sender;
            // Put start marker
            data_to_write[0] = Const.PACKET_START;
            // Put remaining # of bytes
            data_to_write[1] = 10; // 1: header, 4: address, 4: data, 1: end mark
            // Create header
            header = to_dspic_header.data_changed;
            data_to_write[2] = (byte)header;
            // Put address
            var_name = item.VariableName;
            vi = VariableManager.Instance.GetVariable(var_name);
            data_to_write[3] = (byte)vi.address;
            data_to_write[4] = (byte)(vi.address >> 8);
            data_to_write[5] = (byte)(vi.address >> 16);
            data_to_write[6] = (byte)(vi.address >> 24);
            data_to_write[7] = data_in_bytes[0];
            data_to_write[8] = data_in_bytes[1];
            data_to_write[9] = data_in_bytes[2];
            data_to_write[10] = data_in_bytes[3];
            // Put end marker
            data_to_write[11] = Const.PACKET_END;
            // Send data
            FtdiWrapper.Instance.Write(data_to_write);
        }        

        public void HandleIncommingData(Object obj)
        {
            uint num_bytes_read = 0;
            uint num_bytes_available = 0;
            byte[] data_read;

            //Diagnose.StopTimer();
            //Diagnose.StartTimer();

            // Check for communication error 
            if (FtdiWrapper.Instance.Error == true)
            {
                // Do nothing
                return;
            }
            // Check if data is available
            FtdiWrapper.Instance.GetRxBytesAvailable(ref num_bytes_available);
            if (num_bytes_available == 0)
            {
                //Diagnose.StopTimer();
                //Diagnose.StartTimer();
                return;
            }
            else
            {
                Console.WriteLine("bytes available = " + num_bytes_available); 
            }
            // Read data
            data_read = new byte[num_bytes_available];
            FtdiWrapper.Instance.Read(data_read, num_bytes_available, ref num_bytes_read);

            ProcessData(data_read);
        }

        public void ProcessData(byte[] data_read)
        {
            byte[] concat_array;
            int i_packet = 0;
            int i;       
            int num_rem_bytes;
            from_dspic_header header;                
            VariableInfo vi;
            float float_data = 0;
            int int_data;
            int capture_period_num;
            float[] plot_data;
            VariableType[] var_types;
            uint[] temp = new uint[4];
            uint var_address;
            string var_name;
            char[] message_chars;
            int message_len = 0;

            // Concatnate arrays
            if (leftover != null)
            {
                concat_array = new byte[data_read.Length + leftover.Length];
                System.Buffer.BlockCopy(leftover, 0, concat_array, 0, leftover.Length);
                System.Buffer.BlockCopy(data_read, 0, concat_array, leftover.Length, data_read.Length);
            }
            else 
            {
                concat_array = data_read;
            }
            // i_packet points to PACKET_START
            while (concat_array[i_packet] != Const.PACKET_START)
            {
                Console.WriteLine("Abnormal data = " + concat_array[i_packet].ToString()); 
                i_packet++;
                if (i_packet >= concat_array.Length)
                {
                    leftover = null;
                    return;
                }
            }
            // i_packet points to num_rem_bytes
            i_packet++;
            if (i_packet >= concat_array.Length)
            {
                leftover = null;
                return;
            }            
            //process data
            while (true)
            {
                // Get number of remaining bytes
                num_rem_bytes = concat_array[i_packet];

                // Check if array contains enough data
                if (i_packet + num_rem_bytes >= concat_array.Length)
                {
                    // Make i_packet points to start
                    i_packet--;
                    break;
                }
                // Check end mark
                if (concat_array[i_packet + num_rem_bytes] != Const.PACKET_END)
                {
                    // Data lost
                    MessageManager.Instance.EnQueueMessage("WARNING: Some data was lost!\n");
                    Console.WriteLine("Data was lost");
                    // Find next PACKET_START
                    i_packet += num_rem_bytes;
                    while (concat_array[i_packet] != Const.PACKET_START)
                    {
                        i_packet++;
                        if (i_packet >= concat_array.Length)
                        {
                            leftover = null;
                            return;
                        }
                    }
                    // i_packet points to num_rem_bytes
                    i_packet++;
                    if (i_packet >= concat_array.Length)
                    {
                        leftover = null;
                        return;
                    }
                }
                // Get header
                i_packet++;
                header = (from_dspic_header)concat_array[i_packet];
                // Process data
                if (header == from_dspic_header.data_non_plot)
                {
                    // Get data
                    for (i = 0; i < SubscriptionTableManager.Instance.GetNonPlotSubTableLength(); i++)
                    {
                        // Get variable type
                        vi = SubscriptionTableManager.Instance.LookUpnonPlotDataSubscriptionTable(i);
                        // Check variable type
                        if (vi.type == VariableType.Int32)
                        {
                            // Get one variable data
                            temp[0] = (uint)((concat_array[i_packet + 1]) & 0x000000FF);
                            temp[1] = (uint)((concat_array[i_packet + 2] << 8) & 0x0000FF00);
                            temp[2] = (uint)((concat_array[i_packet + 3] << 16) & 0x00FF0000);
                            temp[3] = (uint)((concat_array[i_packet + 4] << 24) & 0xFF000000);
                            i_packet += 4;
                            // Convert to float
                            int_data = (int)(temp[0] + temp[1] + temp[2] + temp[3]);
                            float_data = (float)int_data;
                        }
                        else if (vi.type == VariableType.Float)
                        {
                            // Get float data
                            float_data = BitConverter.ToSingle(concat_array,++i_packet);
                            i_packet += 3;
                        }   
                        // Update data
                        vi.data.data = float_data;
                    }
                    // Point to next num_rem_bytes
                    i_packet += 3; // last byte of data, end byte, start byte
                    // Check if array contains enough data
                    if (i_packet >= concat_array.Length)
                    {
                        // Make i_packet points to start of last packet
                        i_packet--;
                        if (i_packet >= concat_array.Length)
                        {
                            // The data stream ends at the end of packet
                            leftover = null;
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else if (header == from_dspic_header.data_plot)
                {
                    // Get capture period number
                    capture_period_num = concat_array[++i_packet];

                    if (capture_period_num == 1)
                    {
                        Console.WriteLine("capture period = 1");
                    }
                    // Get variable types
                    SubscriptionTableManager.Instance.GetPlotDataType(capture_period_num, out var_types);  
                    // Instantiate
                    plot_data = new float[var_types.Length];
                    // For each variable
                    for (i = 0; i < plot_data.Length; i++)
                    {
                        // Check variable type
                        if (var_types[i] == VariableType.Int32)
                        {
                            // Get long data
                            temp[0] = (uint)((concat_array[++i_packet]) & 0x000000FF);
                            temp[1] = (uint)((concat_array[++i_packet] << 8) & 0x0000FF00);
                            temp[2] = (uint)((concat_array[++i_packet] << 16) & 0x00FF0000);
                            temp[3] = (uint)((concat_array[++i_packet] << 24) & 0xFF000000);
                            plot_data[i] = (float)((int)(temp[0] + temp[1] + temp[2] + temp[3]));
                        }
                        else if (var_types[i] == VariableType.Float)
                        {
                            // Get float data
                            plot_data[i] = BitConverter.ToSingle(concat_array,++i_packet);
                            i_packet += 3;
                        }      
                    }
                    // Update subscription table
                    SubscriptionTableManager.Instance.UpdatePlotDataSubTable(capture_period_num, plot_data);
                    // i_packet points to num_rem_bytes
                    i_packet += 3;                    
                    // Check if array contains enough data
                    if (i_packet >= concat_array.Length)
                    {
                        // Make i_packet points to start of last packet
                        i_packet--;
                        if (i_packet >= concat_array.Length)
                        {
                            // The data stream ends at the end of packet
                            leftover = null;
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }                    
                }
                else if (header == from_dspic_header.acknowledge)
                {
                    // Get address
                    var_address = (uint)((uint)concat_array[i_packet + 1] + (uint)((concat_array[i_packet + 2] << 8)) + (uint)((concat_array[i_packet + 3] << 16)) + (uint)((concat_array[i_packet + 4] << 24)));
                    // Get var name
                    var_name = VariableManager.Instance.GetVariableName(var_address);

                    Console.WriteLine("acknowledge " + var_name);
                    
                    // End wait state
                    NonPlotItems.Instance.EndWaitState(var_name);
                    // i_packet points to num_rem_bytes
                    i_packet += 7;
                    // Check if array contains enough data
                    if (i_packet >= concat_array.Length)
                    {
                        // Make i_packet points to start of last packet
                        i_packet--;
                        if (i_packet >= concat_array.Length)
                        {
                            // The data stream ends at the end of packet
                            leftover = null;
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }  
                }
                else if (header == from_dspic_header.message)
                {
                    string message_string;

                    // Get message
                    // Instantiate message
                    message_len = (int)(concat_array[i_packet - 1] - 6);
                    message_chars = new char[message_len];
                    // Point to beginning of message
                    i_packet++;
                    // For each charactor
                    for (i = 0; i < message_len; i++)
                    {
                        // Grab charactor
                        message_chars[i] = (char)concat_array[i_packet++];                        
                    }
                    // Get value
                    temp[0] = (uint)((concat_array[i_packet + 0]) & 0x000000FF);
                    temp[1] = (uint)((concat_array[i_packet + 1] << 8) & 0x0000FF00);
                    temp[2] = (uint)((concat_array[i_packet + 2] << 16) & 0x00FF0000);
                    temp[3] = (uint)((concat_array[i_packet + 3] << 24) & 0xFF000000);
                    i_packet += 4;
                    int_data = (int)(temp[0] + temp[1] + temp[2] + temp[3]);
                    // Make string
                    message_string = new string(message_chars);
                    message_string += ", ";
                    message_string += int_data.ToString();
                    // Add to message queue
                    MessageManager.Instance.EnQueueMessage(message_string);
                    // i_packet points to num_rem_bytes
                    i_packet += 2;
                    // Check if array contains enough data
                    if (i_packet >= concat_array.Length)
                    {
                        // Make i_packet points to start of last packet
                        i_packet--;
                        if (i_packet >= concat_array.Length)
                        {
                            // The data stream ends at the end of packet
                            leftover = null;
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }  
                }                
            }
            // Get leftover. i_packet should be pointing at start of last packet
            leftover = new byte[concat_array.Length - i_packet];
            System.Buffer.BlockCopy(concat_array,i_packet, leftover,0, concat_array.Length - i_packet);
        }

        public void GetReadyForDesign()
        {
            // Go to bootloader
            USBDataManager.Instance.GoToBootloader();  
        }

        public void GetReadyForAction1()
        {
            // Empty buffer
            EmptyReceiveBuffer();
        }

        public void GetReadyForAction2()
        {
            // Send variable assignments to micro
            SendNonPlotDataAssignments();
            // Add delay so micro has enough time to process them.  If using UART, it may help.  
            Thread.Sleep(10);

            SendPlotDataAssignments();
        }
    }

    [Flags]
    public enum to_dspic_header
    {
        data_changed = 1,
        bootloader = 2,
        assign_plot = 4,
        assign_non_plot = 8,
    }

    [Flags]
    public enum from_dspic_header
    {
        message = 1,
        data_plot = 2,
        data_non_plot = 4,
        acknowledge = 8,
    }  
}
