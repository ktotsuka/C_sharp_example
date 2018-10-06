using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MainApplication
{    
    public sealed class FileManager
    {
        private static readonly FileManager instance = new FileManager();
        private FileInfos files = new FileInfos();
        private DataGridView grid;

        public static FileManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void Clear()
        {
            // Clear
            files.Clear();
            grid.Rows.Clear();
        }

        public DataGridView Grid
        {
            set
            {
                grid = value;
            }
        }

        public int Count
        {
            get
            {
                return files.Count;
            }
        }
            
        public void RemoveFile(string name)
        {
            int index;

            index = LocateFile(name);
            files.RemoveAt(index);
            // Hanlde file change
            HandleFileChange();
        }

        public void RemoveAllFiles()
        {
            files.Clear();
            // Hanlde file change
            HandleFileChange();
        }

        public void ReceiveFiles(string[] file_names)
        {
            int i;
            FileInfo file;
            int index;
            string temp_name;

            // For each file
            for (i = 0; i < file_names.Length; i++)
            {
                // Make sure the file doesn't exist already
                if (LocateFile(file_names[i]) == -1)
                {
                    // Map file
                    if (file_names[i].EndsWith(".map") == true)
                    {
                        // Check location of map file
                        index = GetFile("Map", out temp_name);
                        // Check if map file exist
                        if (index != -1)
                        {
                            // File already exist so remove it
                            files.RemoveAt(index);
                        }
                        file.type = "Map";
                    }
                    // Hex file
                    else if (file_names[i].EndsWith(".hex") == true)
                    {
                        // Check location of hex file
                        index = GetFile("Hex", out temp_name);
                        // Check if hex file exist
                        if (index != -1)
                        {
                            // File already exist so remove it
                            files.RemoveAt(index);
                        }
                        file.type = "Hex";
                    }
                    // Design file
                    else if (file_names[i].EndsWith(".des") == true)
                    {
                        // Check location of design file
                        index = GetFile("Design", out temp_name);
                        // Check if design file exist
                        if (index != -1)
                        {
                            // File already exist so remove it
                            files.RemoveAt(index);
                        }
                        file.type = "Design";
                    }
                    // C file
                    else if (file_names[i].EndsWith(".c") == true)
                    {
                        // Check if file already exist
                        index = LocateFile(file_names[i]);
                        if (index != -1)
                        {
                            // File already exist so break
                            break;
                        }
                        file.type = "C";
                    }
                    // Header file
                    else if (file_names[i].EndsWith(".h") == true)
                    {
                        // Check if file already exist
                        index = LocateFile(file_names[i]);
                        if (index != -1)
                        {
                            // File already exist so break
                            break;
                        }
                        file.type = "Header";
                    }
                    else
                    {
                        // Invalid case
                        file.type = "Invalid";
                    }
                    // Assign name
                    file.name = file_names[i];
                    // Add file
                    files.Add(file);
                }
            }
            // Handle file change
            HandleFileChange();
            return;
        }

        public int GetFile(string type, out string name)
        {
            int i = 0;

            // Search through files
            while (i < files.Count)
            {
                // Check if type matches
                if (files[i].type == type)
                {
                    // File found
                    name = files[i].name;
                    return i;
                }
                i++;
            }
            // File not found
            name = "";
            return -1;
        }

        public string[] GetFiles()
        {
            string[] file_names;
            int i;

            file_names = new string[files.Count];
            // for each file
            for (i = 0; i < file_names.Length; i++)
            {
                // get file name
                file_names[i] = files[i].name;
            }
            return file_names;
        }

        public void GetCHFiles(out string[] ch_files)
        {
            int num;
            int i = 0;
            int index = 0;

            // Count c and header files
            num = CountCHFiles();
            // Create array to hold c-header files
            ch_files = new string[num];
            // Insert c-header files
            for (i = 0; i < files.Count; i++)
            {
                if (files[i].type == "C" || files[i].type == "Header")
                {
                    ch_files[index] = files[i].name;
                    index++;
                }
            }
        }

        private int CountCHFiles()
        {
            int i;
            int count = 0;

            for (i = 0; i < files.Count; i++)
            {
                if (files[i].type == "C" || files[i].type == "Header")
                {
                    count++;
                }
            }
            return count;
        }

        private int LocateFile(string name)
        {
            int i = 0;

            // Serch through files
            while (i < files.Count)
            {
                // Check if name matches
                if (files[i].name == name)
                {
                    // File found
                    return i;
                }
                i++;
            }
            // File not found
            return -1;
        }

        public void HandleFileChange()
        {
            // Process map file
            VariableManager.Instance.ProcessMapFile();
            // Refresh variable assignments
            VariableManager.Instance.RefreshVariableAssignments();
            // UpdateDisplay
            UpdateDisplay();
            VariableManager.Instance.UpdateDisplay();
            CaptureSettingManager.Instance.UpdateDisplay();
        }

        public void UpdateDesignFile(string design_file)
        {
            int index;
            string str;
            FileInfo fi;

            // Get index for design file
            index = GetFile("Design", out str);
            // See if design file exist
            if (index != -1)
            {
                //Remove old design file
                files.RemoveAt(index);
                // Create new entry
                fi.name = design_file;
                fi.type = "Design";
                files.Add(fi);
            }
            else
            {
                // Create new entry
                fi.name = design_file;
                fi.type = "Design";
                files.Add(fi);
            }    
            // Update grid display
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            int i;

            DataGridViewRow grid_row = new DataGridViewRow();
            // Delete all rows
            grid.Rows.Clear();
            // Add each row
            for (i = 0; i < files.Count; i++)
            {
                // Add empty row
                grid.Rows.Add();
                // Assign Type
                grid.Rows[i].Cells[1].Value = files[i].name;
                // Assign Name
                grid.Rows[i].Cells[0].Value = files[i].type;
            }
        }
    }    
}


