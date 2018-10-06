using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using PlotItemSpace;
using NonPlotItemSpace;
using System.IO;
using System.Drawing;


namespace MainApplication
{
    public sealed class DesignManager
    {
        private static readonly DesignManager instance = new DesignManager();

        public static DesignManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void SaveDesign(ref string file_name, Point display_location)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            StreamWriter sw;

            // Check if design file doesn't exist
            if (file_name == null)
            {
                // Set dialog filter
                dlg.Filter = "Design file (*.des)|*.des";
                // Open dialog
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // Update designFile
                    file_name = dlg.FileName; 
                    // Specify the file location to write
                    sw = new StreamWriter(dlg.FileName);                    
                }
                else
                {
                    return;
                }
            }
            else 
            {
                sw = new StreamWriter(file_name);
            }
            // Update design file location for display
            FileManager.Instance.UpdateDesignFile(file_name);          
            // Save file locations
            SaveFileLocations(sw);
            // Save NonPlotItems
            SaveNonPlotItems(sw, display_location);
            // Save PlotItems
            SavePlotItems(sw, display_location);
            // Save catputre setting
            SaveCaptureSetting(sw);
            // Close stream
            sw.Close();             
        }

        public void LoadDesign(string file_name)
        {
            StreamReader sr;
            string[] file_names = new string[1];

            // Load design file
            file_names[0] = file_name;
            FileManager.Instance.ReceiveFiles(file_names);
            // Open file stream
            sr = new StreamReader(file_name);
            // Load file locations and receive files
            LoadFileLocations(sr);
            // Load NonPlotItems
            LoadNonPlotItems(sr);
            // Load PlotItems
            LoadPlotItems(sr);
            // Load catputre setting
            LoadCaptureSetting(sr);
            // Close stream
            sr.Close();
        }

        private void SaveFileLocations(StreamWriter sw)
        {
            string[] file_names;
            int i;

            // Get file names
            file_names = FileManager.Instance.GetFiles();
            // Write number of files
            sw.WriteLine("// Number of files");
            sw.WriteLine(file_names.Length);
            // For each file
            for (i = 0; i < file_names.Length; i++)
            {
                // Write to file
                sw.WriteLine(file_names[i]);
            }
        }

        private void LoadFileLocations(StreamReader sr)
        {
            string[] file_names;
            int i;
            int num_files;
            string temp_name;
            string comment;

            // Get number of files
            comment = sr.ReadLine();
            num_files = int.Parse(sr.ReadLine());
            // Instantiate
            file_names = new string[num_files];
            // For each file
            for (i = 0; i < num_files; i++)
            {
                // Get name
                temp_name = sr.ReadLine();
                // If file exist
                if (File.Exists(temp_name))
                {
                    // Store file name
                    file_names[i] = temp_name;
                }
                else
                {
                    // Give warning
                    MessageBox.Show("Can not find file " + temp_name + ". Edit the design file so that file locations are correct.");
                }
            }
            // Recieve files
            FileManager.Instance.ReceiveFiles(file_names);            
        }

        private void SaveNonPlotItems(StreamWriter sw, Point display_location)
        {
            int i;

            // Write number of items
            sw.WriteLine("// Number of non-plot items");
            sw.WriteLine(NonPlotItems.Instance.Count);
            // For each item
            for (i = 0; i < NonPlotItems.Instance.Count; i++)
            {
                // Write location
                sw.WriteLine("// Location");
                sw.WriteLine(NonPlotItems.Instance[i].Location.X - display_location.X);
                sw.WriteLine(NonPlotItems.Instance[i].Location.Y - display_location.Y);
                // Write item type
                sw.WriteLine("// Item type");
                sw.WriteLine((int)NonPlotItems.Instance[i].Type);
                // Write variable name
                sw.WriteLine("// Variable name");
                sw.WriteLine(NonPlotItems.Instance[i].VariableName);
            }
        }

        private void LoadNonPlotItems(StreamReader sr)
        {
            int i;
            int num_items;
            Point pt;
            NonPlotItemType item_type;
            string var_name;
            VariableInfo vi;
            string comment;

            // Receive number of item
            comment = sr.ReadLine();
            num_items = int.Parse(sr.ReadLine());
            // For each item
            for (i = 0; i < num_items; i++)
            {
                // Get location
                comment = sr.ReadLine();
                pt = new Point(int.Parse(sr.ReadLine()), int.Parse(sr.ReadLine()));
                // Get item type
                comment = sr.ReadLine();
                item_type = (NonPlotItemType)int.Parse(sr.ReadLine());
                // Add item
                NonPlotItems.Instance.Add(new NonPlotItem(pt, item_type));
                // Attach event handler
                NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemValueChanged += new NonPlotItem.NonPlotItemValueChangedEventHandler(USBDataManager.Instance.NonPlotItemChanged);
                // Get variable name
                comment = sr.ReadLine();
                var_name = sr.ReadLine();
                // Get variable info
                vi = VariableManager.Instance.GetVariable(var_name);
                // Check if item is assigned
                if (vi != null)
                {
                    // Make sure variable is valid type
                    if (vi.type != VariableType.Unknown)
                    {
                        // Assign variable to item
                        NonPlotItems.Instance[i].Assign(var_name, vi.data);
                    }
                }
                // Update display
                NonPlotItems.Instance[i].Invalidate();
            }
        }

        private void SavePlotItems(StreamWriter sw, Point display_location)
        {
            int i, j;
            string[] legend_names;

            // Write number of items
            sw.WriteLine("// Number of plot items");
            sw.WriteLine(PlotItems.Instance.Count);
            // For each item
            for (i = 0; i < PlotItems.Instance.Count; i++)
            {
                // Write location
                sw.WriteLine("// Location");
                sw.WriteLine(PlotItems.Instance[i].Location.X - display_location.X);
                sw.WriteLine(PlotItems.Instance[i].Location.Y - display_location.Y);
                // Write size
                sw.WriteLine("// Size");
                sw.WriteLine(PlotItems.Instance[i].Size.Width);
                sw.WriteLine(PlotItems.Instance[i].Size.Height);
                // Write x label
                sw.WriteLine("// X label");
                sw.WriteLine(PlotItems.Instance[i].XLabel);
                // Get legend names
                legend_names = PlotItems.Instance[i].GetLegendLabelNames();
                // Write legend name length
                sw.WriteLine("// Legend name length");
                sw.WriteLine(legend_names.Length);
                // For each y label
                for (j = 0; j < legend_names.Length; j++)
                {
                    // Write y label
                    sw.WriteLine(legend_names[j]);
                }
                // Write grid state
                sw.WriteLine("// Grid state");
                sw.WriteLine(PlotItems.Instance[i].DrawGrid);
                // Write symbol state
                sw.WriteLine("// Symbol state");
                sw.WriteLine(PlotItems.Instance[i].DrawSymbol);
            }
        }

        private void LoadPlotItems(StreamReader sr)
        {
            int i, j;
            string[] legend_names;
            int num_items;
            int num_legends;
            Point pt;
            Size size;
            string comment;

            // Receive number of item
            comment = sr.ReadLine();
            num_items = int.Parse(sr.ReadLine());
            // For each item
            for (i = 0; i < num_items; i++)
            {
                // Read location
                comment = sr.ReadLine();
                pt = new Point(int.Parse(sr.ReadLine()), int.Parse(sr.ReadLine()));
                // Read size
                comment = sr.ReadLine();
                size = new Size(int.Parse(sr.ReadLine()), int.Parse(sr.ReadLine()));
                // Add item
                PlotItems.Instance.Add(new PlotItem(pt, size));
                // Read x label
                comment = sr.ReadLine();
                PlotItems.Instance[i].XLabel = sr.ReadLine();
                // Check if x label is empty
                if (PlotItems.Instance[i].XLabel == "")
                {
                    // Make it null
                    PlotItems.Instance[i].XLabel = null;
                }
                // Read number of legends
                comment = sr.ReadLine();
                num_legends = int.Parse(sr.ReadLine());
                // Instantiate
                legend_names = new string[num_legends];
                // For each legend
                for (j = 0; j < num_legends; j++)
                {
                    // Assign legend to plot
                    PlotItems.Instance[i].AddLegendLabel(sr.ReadLine());
                    // Update display
                    PlotItems.Instance[i].Invalidate();
                }
                // Read grid state
                comment = sr.ReadLine();
                PlotItems.Instance[i].DrawGrid = bool.Parse(sr.ReadLine());
                // Read symbol state
                comment = sr.ReadLine();
                PlotItems.Instance[i].DrawSymbol = bool.Parse(sr.ReadLine());
            }
        }

        private void SaveCaptureSetting(StreamWriter sw)
        {
            DataGridViewRowCollection rows;
            int i;

            // Get rows
            rows = CaptureSettingManager.Instance.Grid.Rows;
            // Write number of rows
            sw.WriteLine("// Number of grid rows");
            sw.WriteLine(rows.Count);
            // For each row
            for (i = 0; i < rows.Count; i++)
            {
                // Write capture period setting
                sw.WriteLine("// Capture period setting");
                sw.WriteLine((string)(rows[i].Cells[Const.CAPTURE_PERIOD_NUM].Value));
                // Write # data points
                sw.WriteLine("// Number of data points");
                sw.WriteLine((string)(rows[i].Cells[Const.NUM_DATA_PTS].Value));
                // Write auto repeat
                sw.WriteLine("// Auto repeat state");
                sw.WriteLine((bool)rows[i].Cells[Const.AUTO_REPEAT].Value);
                // Write start-at-zero
                sw.WriteLine("// Start at zero state");
                sw.WriteLine((bool)rows[i].Cells[Const.START_AT_ZERO].Value);
            }            
        }

        private void LoadCaptureSetting(StreamReader sr)
        {
            DataGridViewRowCollection rows;
            int i;
            int num_rows;
            string comment;

            // Refresh capture setting
            CaptureSettingManager.Instance.UpdateDisplay();
            // Read rows
            rows = CaptureSettingManager.Instance.Grid.Rows;
            // Read number of rows
            comment = sr.ReadLine();
            num_rows = int.Parse(sr.ReadLine());
            // For each rows
            for (i = 0; i < num_rows; i++)
            {
                // Read capture period number
                comment = sr.ReadLine();
                rows[i].Cells[Const.CAPTURE_PERIOD_NUM].Value = sr.ReadLine();
                // Read # data points
                comment = sr.ReadLine();
                rows[i].Cells[Const.NUM_DATA_PTS].Value = sr.ReadLine();
                // Read auto repeat
                comment = sr.ReadLine();
                rows[i].Cells[Const.AUTO_REPEAT].Value = bool.Parse(sr.ReadLine());
                // Read auto repeat
                comment = sr.ReadLine();
                rows[i].Cells[Const.START_AT_ZERO].Value = bool.Parse(sr.ReadLine());
            }
        }

        public void ClearDesign()
        {
            // Clear 
            PlotItems.Instance.Clear();
            NonPlotItems.Instance.Clear();
            CaptureSettingManager.Instance.Clear();
            FileManager.Instance.Clear();
            VariableManager.Instance.Clear();   
        }

        public void SaveDesignAs(ref string design_file_name, Point display_location)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            // Set dialog filter
            dlg.Filter = "Design file (*.des)|*.des";
            // Open dialog
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                design_file_name = dlg.FileName;
                SaveDesign(ref design_file_name, display_location);
            }
            else
            {
                return;
            }
        }
    }
}
