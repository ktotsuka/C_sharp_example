using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using PlotItemSpace;
using System.IO;

namespace MainApplication
{
    public sealed class CaptureSettingManager
    {
        private static readonly CaptureSettingManager instance = new CaptureSettingManager();
        private DataGridView grid;        

        public static CaptureSettingManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void Clear()
        {
            // Clear grid
            grid.Rows.Clear();
        }

        private int GetRowIndex(int capture_period_num)
        {
            int i;
            string temp_str;

            // For each row
            for (i = 0; i < grid.Rows.Count; i++)
            {
                // Check capture_period_num
                temp_str = (string)(grid.Rows[i].Cells[Const.CAPTURE_PERIOD_NUM].Value);
                if (capture_period_num == Int32.Parse(temp_str))
                {
                    return i;
                }
            }
            // error
            return -1;
        }

        public int GetCaptureLimit(int capture_period_num)
        {
            int row_index;
            string limit_str;

            // Get row index
            row_index = GetRowIndex(capture_period_num);
            // Get limit in string
            limit_str = (string)(grid.Rows[row_index].Cells[Const.NUM_DATA_PTS].Value);
            // Return limit
            return Int32.Parse(limit_str);
        }

        public bool CapturePeriodNumIsValid()
        {
            int num_rows;
            int i;
            int row_index = 0;
            bool found = false;
            int capture_period_num;

            // Get number of capture period
            num_rows = grid.Rows.Count;
            // Limit number of period
            if (num_rows > 4)
            {
                // Too many rows (or x variables)
                return false;
            }
            // For each capture period
            for (i = 0; i < num_rows; i++)
            {
                // Search for the capture period
                while (found == false)
                {
                    // Get capture period num
                    capture_period_num = Int32.Parse((string)(grid.Rows[row_index].Cells[Const.CAPTURE_PERIOD_NUM].Value));
                    // Check if it is the one I'm looking for
                    if (capture_period_num == i)
                    {
                        // Found
                        found = true;
                    }
                    else
                    {
                        // Go to next row
                        row_index++;
                        // Check if end of rows
                        if (row_index >= num_rows)
                        {
                            // Didn't find
                            return false;
                        }
                    }                    
                }
                // Reset
                found = false;
                row_index = 0;
            }
            // All found
            return true;
        }

        public void UpdateDisplay()
        {
            List<string> x_var_names;
            int original_row_count;
            int row_index;
            int i;

            // Get original row count
            original_row_count = grid.Rows.Count;
            // Get variable names
            PlotItems.Instance.GetXVarNames(out x_var_names);
            // For each x variable
            for (i = 0; i < x_var_names.Count; i++)
            {
                // Get row index of x variable
                row_index = GetRowIndex(x_var_names[i]);
                // Check if corresponding row already exist
                if (row_index != -1)
                {
                    // Copy the row
                    grid.Rows.Add();
                    grid.Rows[grid.Rows.Count - 1].Cells[Const.CAPTURE_PERIOD_NUM].Value = grid.Rows[row_index].Cells[Const.CAPTURE_PERIOD_NUM].Value;
                    grid.Rows[grid.Rows.Count-1].Cells[Const.X_VAR_NAME].Value = grid.Rows[row_index].Cells[Const.X_VAR_NAME].Value;
                    grid.Rows[grid.Rows.Count-1].Cells[Const.NUM_DATA_PTS].Value = grid.Rows[row_index].Cells[Const.NUM_DATA_PTS].Value;
                    grid.Rows[grid.Rows.Count-1].Cells[Const.START_STOP].Value = grid.Rows[row_index].Cells[Const.START_STOP].Value;
                    grid.Rows[grid.Rows.Count-1].Cells[Const.AUTO_REPEAT].Value = grid.Rows[row_index].Cells[Const.AUTO_REPEAT].Value;
                    grid.Rows[grid.Rows.Count - 1].Cells[Const.START_AT_ZERO].Value = grid.Rows[row_index].Cells[Const.START_AT_ZERO].Value; 
                    grid.Rows[grid.Rows.Count - 1].Cells[Const.SAVE].Value = grid.Rows[row_index].Cells[Const.SAVE].Value;
                }
                else
                {
                    // Add a clean copy
                    grid.Rows.Add();
                    // Assign default values
                    grid.Rows[grid.Rows.Count - 1].Cells[Const.CAPTURE_PERIOD_NUM].Value = "0";
                    grid.Rows[grid.Rows.Count - 1].Cells[Const.X_VAR_NAME].Value = x_var_names[i];
                    grid.Rows[grid.Rows.Count - 1].Cells[Const.NUM_DATA_PTS].Value = "10000";
                    grid.Rows[grid.Rows.Count - 1].Cells[Const.START_STOP].Value = "Start";
                    grid.Rows[grid.Rows.Count - 1].Cells[Const.AUTO_REPEAT].Value = false;
                    grid.Rows[grid.Rows.Count - 1].Cells[Const.START_AT_ZERO].Value = false;
                    grid.Rows[grid.Rows.Count - 1].Cells[Const.SAVE].Value = "Save";
                }           
            }
            // Remove old entries
            for (i = 0; i < original_row_count; i++)
            {
                // Remove row
                grid.Rows.RemoveAt(0);
            }
        }

        public void GetOrderedXVarNames(out List<string> x_var_names)
        {
            int i;

            // Instantiate
            x_var_names = new List<string>();
            // For each capture period
            for (i = 0; i < grid.Rows.Count; i++)
            {
                // Add x variable
                x_var_names.Add(GetXVarName(i));
            }
        }

        private int GetRowIndex(string x_var_name)
        {
            int i;

            // For each row
            for (i = 0; i < grid.Rows.Count; i++)
            {
                // See if x_var_name matches
                if (x_var_name == (string)(grid.Rows[i].Cells[Const.X_VAR_NAME].Value))
                {
                    // Return index
                    return i;
                }
            }
            // No match, so return -1
            return -1;
        }

        public void HandleCellContentClick(DataGridViewCellEventArgs e, DataGridView grid)
        {
            int capture_period_num;
            string temp_str;

            // Check what was clicked
            if (e.ColumnIndex == Const.AUTO_REPEAT)
            {
                // End edit immediately becasue it's a checkbox
                grid.EndEdit();
            }
            else if (e.ColumnIndex == Const.START_AT_ZERO)
            {
                // End edit immediately becasue it's a checkbox
                grid.EndEdit();
            }
            else if (e.ColumnIndex == Const.START_STOP)
            {
                // Start/Stop clicked
                HandleStartStop(e.RowIndex);
            }
            else if (e.ColumnIndex == Const.SAVE)
            {
                // Get capture period selected
                temp_str = (string)(grid.Rows[e.RowIndex].Cells[Const.CAPTURE_PERIOD_NUM].Value);
                capture_period_num = Int32.Parse(temp_str);
                // Save data
                SavePlotData(capture_period_num);
                }
        }

        public bool GetStartAtZero(int capture_period_num)
        {
            int row_index;

            row_index = GetRowIndex(capture_period_num);
            return (bool)(grid.Rows[row_index].Cells[Const.START_AT_ZERO].Value);
        }

        private void SavePlotData(int capture_period_num)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            StreamWriter sw;

            // Set dialog filter
            dlg.Filter = "Comma separated values (*.csv)|*.csv";
            // Open dialog
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using (sw = new StreamWriter(dlg.FileName))
                {
                    // Write plot data to stream
                    SubscriptionTableManager.Instance.SavePlotData(capture_period_num, sw);
                }                
            }
        }

        public void HandleCellValidating(DataGridViewCellValidatingEventArgs e)
        {
            string new_str;
            int test_num;
                        
            // Check if num data points is selected
            if (e.ColumnIndex == Const.NUM_DATA_PTS)
            {
                // Get new string
                new_str = (string)(e.FormattedValue);
                // Check if valid
                if (Int32.TryParse(new_str, out test_num))
                {
                    if (test_num > 0 && test_num < 500001)
                    {
                        // Valid
                        grid.Rows[e.RowIndex].Cells[Const.NUM_DATA_PTS].Tag = test_num;
                        return;
                    }                    
                }
                // Invalid
                grid.Rows[e.RowIndex].ErrorText = "Enter 1 ~ 500000";
                e.Cancel = true;
            }            
        }

        public void HandleCellEndEdit(DataGridViewCellEventArgs e)
        {
            // Clear error text
            grid.Rows[e.RowIndex].ErrorText = String.Empty;
        }


        private int GetCapturePeriodNum(int row_num)
        {
            string temp_str;

            temp_str = (string)(grid.Rows[row_num].Cells[Const.CAPTURE_PERIOD_NUM].Value);
            return Int32.Parse(temp_str);
        }

        private void HandleStartStop(int row_num)
        {
            string x_var_name;

            // Get x variable name
            x_var_name = (string)(grid.Rows[row_num].Cells[Const.X_VAR_NAME].Value);
            // Check if start
            if ((string)(grid.Rows[row_num].Cells[Const.START_STOP].Value) == "Start")
            {
                // Update text
                grid.Rows[row_num].Cells[Const.START_STOP].Value = "Stop";
                // Disable "start at zero"
                grid.Rows[row_num].Cells[Const.START_AT_ZERO].ReadOnly = true;
                // Empty data
                SubscriptionTableManager.Instance.EmptyPlotData(GetCapturePeriodNum(row_num));
                // Start plots                
                PlotItems.Instance.StartPlots(x_var_name);
            }
            else
            {
                // Stop
                // Update text
                grid.Rows[row_num].Cells[Const.START_STOP].Value = "Start";
                // Enable "start at zero"
                grid.Rows[row_num].Cells[Const.START_AT_ZERO].ReadOnly = false;
                // Stop plots
                PlotItems.Instance.StopPlots(x_var_name);
            }
        }

        public void StartAllCapture()
        {
            int i;
            string x_var_name;

            // For each capture period
            for (i = 0; i < grid.Rows.Count; i++)
            {
                // Get x variable name
                x_var_name = (string)(grid.Rows[i].Cells[Const.X_VAR_NAME].Value);
                // Update text
                grid.Rows[i].Cells[Const.START_STOP].Value = "Stop";
                // Disable "start at zero"
                grid.Rows[i].Cells[Const.START_AT_ZERO].ReadOnly = true;
                // Empty data
                SubscriptionTableManager.Instance.EmptyPlotData(i);
                // Start plots                
                PlotItems.Instance.StartPlots(x_var_name);
            }            
        }

        public void CheckCaptureLimit()
        {
            int i;

            // For each capture period
            for (i = 0; i < SubscriptionTableManager.Instance.GetPlotSubTableLength(); i++)
            {
                // Check if capture is active
                if (IsCaptureActive(i) == true)
                {
                    // Check if limit is reached
                    if (SubscriptionTableManager.Instance.GetPlotDataNumPoints(i) >= GetCaptureLimit(i))
                    {
                        // Update start/stop button
                        grid.Rows[GetRowIndex(i)].Cells[Const.START_STOP].Value = "Start";
                        // Enable "start at zero"
                        grid.Rows[GetRowIndex(i)].Cells[Const.START_AT_ZERO].ReadOnly = false;
                        // Stop plot
                        PlotItems.Instance.StopPlots((string)(grid.Rows[i].Cells[Const.X_VAR_NAME].Value));
                        // Check for restart
                        if ((bool)(grid.Rows[i].Cells[Const.AUTO_REPEAT].Value) == true)
                        {
                            // Restart
                            // Update start/stop button
                            grid.Rows[i].Cells[Const.START_STOP].Value = "Stop";
                            // Disable "start at zero"
                            grid.Rows[i].Cells[Const.START_AT_ZERO].ReadOnly = true;
                            // Empty data
                            SubscriptionTableManager.Instance.EmptyPlotData(i);
                            // Start plots                
                            PlotItems.Instance.StartPlots((string)(grid.Rows[i].Cells[Const.X_VAR_NAME].Value));
                        }
                    }
                }
            }
        }

        public DataGridView Grid
        {
            set
            {
                grid = value;
            }
            get
            {
                return grid;
            }
        }

        public string GetXVarName(int capture_period_num)
        {
            int i;
            string temp_str;

            // For each row
            for (i = 0; i < grid.Rows.Count; i++)
            {
                // Check capture_period_num
                temp_str = (string)(grid.Rows[i].Cells[Const.CAPTURE_PERIOD_NUM].Value);
                if (capture_period_num == Int32.Parse(temp_str))
                {
                    return (string)(grid.Rows[i].Cells[Const.X_VAR_NAME].Value);
                }
            }
            // error
            return null;
        }

        public bool IsCaptureActive(int capture_period_num)
        {
            int row_num;

            // Get row number
            row_num = GetRowIndex(capture_period_num);
            // Check state
            if ((string)(grid.Rows[row_num].Cells[Const.START_STOP].Value) == "Stop")
            {
                // Active
                return true;
            }
            else
            {
                // Inactive
                return false;
            }
        }

        public void GetReadyForAction()
        {
            // Start all capture
            StartAllCapture();
        }
    }
}
