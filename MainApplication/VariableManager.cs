using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using NonPlotItemSpace;
using PlotItemSpace;
using System.Text.RegularExpressions;

namespace MainApplication
{
    public sealed class VariableManager
    {
        private static readonly VariableManager instance = new VariableManager();
        private VariableInfos variables = new VariableInfos();
        private DataGridView grid;

        public static VariableManager Instance
        {
            get
            {
                return instance;
            }
        }

        public DataGridView Grid
        {
            set
            {
                grid = value;
            }
        }        

        public void ProcessMapFile()
        {
            StreamReader sr;
            string line = "";
            bool eol = false;
            string var_name;
            uint var_address;
            int index;
            string map_file;
            string[] ch_files;

            // Empty variables
            variables.Clear();
            // Make sure that map file exist
            index = FileManager.Instance.GetFile("Map", out map_file);
            if (index == -1)
            {
                // No map file, so do nothing
                return;
            }
            // Get c and header files
            FileManager.Instance.GetCHFiles(out ch_files);
            // Open map file
            sr = File.OpenText(map_file);
            // Read until data or bss fields are found
            while (((line.Contains(".bss") == false) && (line.Contains(".data") == false)) || (line.Contains("main.o") == false))
            {
                line = sr.ReadLine();
            }
            if (line.Contains(".data"))
            {
                // Move one more to get to the variable lines
                line = sr.ReadLine();
                // Keep adding variables until end of list
                while (eol == false)
                {
                    // Grab name and address of variable
                    GetVariableInfoFromLine(line, out var_name, out var_address, ref eol);
                    // check if end of list
                    if (eol == false)
                    {
                        // Add variable
                        AddVariable(var_name, var_address, ch_files);
                        // Read next line
                        line = sr.ReadLine();
                    }
                }
                eol = false;
                // Read until bss fields are found
                while ((line.Contains(".bss") == false) || (line.Contains("main.o") == false))
                {
                    line = sr.ReadLine();
                }
            }
            // Move one more to get to the variable lines
            line = sr.ReadLine();
            // Keep adding variables until end of list
            while (eol == false)
            {
                // Grab name and address of variable
                GetVariableInfoFromLine(line, out var_name, out var_address, ref eol);
                // check if end of line
                if (eol == false)
                {
                    // Add variable
                    AddVariable(var_name, var_address, ch_files);
                    // Read next line
                    line = sr.ReadLine();
                }
            }
            sr.Close();
        }

        private bool GetVariableInfoFromLine(string line, out string var_name, out uint var_address, ref bool eol)
        {
            int index;
            string address;

            // Check for eol
            if (line.Contains("(COMMON)") || line == "" || line.Contains(".o"))
            {
                eol = true;
                var_name = "";
                var_address = 0;
                return true;
            }
            else
            {                
                // Grab variable address
                index = line.IndexOf("0x");
                address = line.Substring(index + 2, 8);
                var_address = Convert.ToUInt32(address, 16);
                // Move to end of address
                index += 10;
                // Move to beginning of name
                while (line.Substring(index, 1) == " ")
                {
                    index++;
                }
                // Grab variable name
                var_name = line.Substring(index);
                return true;
            }
        }

        private bool FindVariableFromLine(string line, string var_name)
        {
            Regex exp = new Regex("[ ,]" + var_name + "[ ,;]");
            return exp.IsMatch(line);
        }

        private bool AddVariable(string var_name, uint var_address, string[] ch_files)
        {
            int i;
            StreamReader sr;
            string line = "";
            VariableType var_type;
            bool eof;
            VariableInfo vi = new VariableInfo();
            
            // Serch for each file
            for (i = 0; i < ch_files.Length; i++)
            {
                // Open file
                sr = File.OpenText(ch_files[i]);
                // Set eof = false
                eof = false;
                // Read line by line
                while (eof == false)
                {
                    line = sr.ReadLine();
                    // Check if end of line
                    if (line == null)
                    {
                        // End of line reached
                        eof = true;
                    }
                    else
                    {
                        // See if variable is in the line
                        if (FindVariableFromLine(line, var_name) == true)
                        {
                            // Get variable type
                            if (GetVariableTypeFromLine(line, out var_type) == true)
                            {
                                // Found type, so add the variable
                                vi.type = var_type;
                                vi.address = var_address;
                                vi.data = new NonPlotData();
                                variables.Add(var_name, vi);
                                // Close file
                                sr.Close();
                                return true;
                            }
                        }
                    }
                }
                // Close file
                sr.Close();
            }
            // Couldn't find variable
            vi.type = VariableType.Unknown;
            vi.address = 0;
            variables.Add(var_name, vi);
            return false;
        }

        private bool GetVariableTypeFromLine(string line, out VariableType var_type)
        {
            // Trim white spaces
            line.Trim();
            // Check if it starts with float or int32_t
            if (line.StartsWith("float")) 
            {
                // float type
                var_type =  VariableType.Float;
                return true;
            }
            else if (line.StartsWith("int32_t"))
            {
                // long type
                var_type = VariableType.Int32;
                return true;
            }
            else
            {
                // invalid type
                var_type = VariableType.Unknown;
                return false;
            }
        }

        public void UpdateDisplay()
        {
            int row_num = 0;
            DataGridViewCellStyle cell_style = new DataGridViewCellStyle();

            DataGridViewRow grid_row = new DataGridViewRow();

            // Delete all rows
            grid.Rows.Clear();
            // Add each row
            //for (i = 0; i < variables.Count; i++)
            foreach (KeyValuePair<string, VariableInfo> kvp in variables)
            {
                // Add empty row
                grid.Rows.Add();
                // Assign name
                grid.Rows[row_num].Cells[0].Value = (string)kvp.Key;
                // Assign type
                grid.Rows[row_num].Cells[1].Value = ((VariableInfo)kvp.Value).type;
                // check if invalid entry
                if (((VariableInfo)kvp.Value).type == VariableType.Unknown)
                {
                    // Highlight in red
                    cell_style.BackColor = Color.Red;
                    grid.Rows[row_num].DefaultCellStyle = cell_style;
                }
                row_num++;
            }
        }

        public void RefreshVariableAssignments()
        {
            int i;
            int j;
            string[] legend_names;
            VariableInfo vi;

            // For each nonplot item
            for (i = 0; i < NonPlotItems.Instance.Count; i++)
            {
                // Get variable info
                vi = GetVariable(NonPlotItems.Instance[i].VariableName);
                // Check if variable exist
                if (vi != null)
                {
                    // Check if variable is unknown
                    if (vi.type == VariableType.Unknown)
                    {
                        // Unassign
                        NonPlotItems.Instance[i].UnAssign();
                    }
                    else
                    {
                        // Refresh assignment just in case location of "data" changed
                        NonPlotItems.Instance[i].Assign(NonPlotItems.Instance[i].VariableName, vi.data);
                        // Assign float or long type
                        if (NonPlotItems.Instance[i].Type != NonPlotItemType.BooleanItem)
                        {
                            if (vi.type == VariableType.Float)
                            {
                                NonPlotItems.Instance[i].Type = NonPlotItemType.FloatItem;
                            }
                            else
                            {
                                NonPlotItems.Instance[i].Type = NonPlotItemType.LongItem;
                            }
                        }
                    }
                }
                else
                {
                    // Unassign
                    NonPlotItems.Instance[i].UnAssign();
                }
                // Update display
                NonPlotItems.Instance[i].Invalidate();
            }
            // For each plot item
            for (i = 0; i < PlotItems.Instance.Count; i++)
            {
                // Check if x variable is assigned
                if (PlotItems.Instance[i].XLabel != null)
                {
                    // Get x variable info
                    vi = GetVariable(PlotItems.Instance[i].XLabel);
                    // Check if variable exist
                    if (vi != null)
                    {
                        // Check if variable is unknown
                        if (vi.type == VariableType.Unknown)
                        {
                            // Unassign
                            PlotItems.Instance[i].XLabel = null;
                        }
                    }
                    else
                    {
                        // Unassign
                        PlotItems.Instance[i].XLabel = null;
                    }
                }
                // Get legend names
                legend_names = PlotItems.Instance[i].GetLegendLabelNames();
                // For each y variable
                for (j = 0; j < legend_names.Length; j++)
                {
                    // Get y variable info
                    vi = GetVariable(legend_names[j]);
                    // Check if variable exist
                    if (vi != null)
                    {
                        // Check if variable is unknown
                        if (vi.type == VariableType.Unknown)
                        {
                            // Unassign
                            PlotItems.Instance[i].UnAssignYVar(legend_names[j]);
                        }
                    }
                    else
                    {
                        // Unassign
                        PlotItems.Instance[i].UnAssignYVar(legend_names[j]);
                    }
                }
                // Update display
                PlotItems.Instance[i].Invalidate();
            }
        }

        public void GetDropoffInfo(int row_index, ref DropOffInfo info)
        {
            info.variableName = (string)grid.Rows[row_index].Cells[Const.VAR_NAME].Value;
            info.variableAddress = variables[info.variableName].address;
            info.variableType = variables[info.variableName].type;
            info.data = variables[info.variableName].data;
        }

        public VariableInfo GetVariable(string var_name)
        {
            // Check if variable exist
            if (variables.ContainsKey(var_name) == true)
            {
                // Return variable info
                return variables[var_name];
            }
            else
            {
                // Return null
                return null;
            }
        }

        public string GetVariableName(uint address)
        {   
            // For each variable  
            foreach (KeyValuePair<string, VariableInfo> kvp in variables)
            {
                // Check for match
                if (address == kvp.Value.address)
                {
                    // Return variable name
                    return (string)kvp.Key;
                }
            }
            // Didn't find
            return null;
        }

        public void Clear()
        {
            // Clear
            variables.Clear();
            grid.Rows.Clear();
        }

        public void GetReadyForAction()
        {
            // Disalbe grid
            //grid.Visible = false;
        }
    }   
}
