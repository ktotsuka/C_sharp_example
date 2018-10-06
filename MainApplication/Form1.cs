using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using NonPlotItemSpace;  
using PlotItemSpace;

namespace MainApplication
{
    public partial class MiniDspace : Form
    {
        private States myState; 
        private DesignStates myDesignStates = DesignStates.Normal;
        private DropOffInfo myDropOffInfo;
        private System.Windows.Forms.Timer myUpdateItemDisplayTimer = new System.Windows.Forms.Timer();
        private int myIndex; // Index of item to be moved
        private TimerCallback myTimerCallback; // Delagate for incomingDataProcessTimer
        System.Threading.Timer incomingDataProcessTimer; // Timer for processing incomming data (use separate thread for accurate timing)
        private string designFileName; 

        public MiniDspace()
        {
            InitializeComponent();
            // Attach parts
            FileManager.Instance.Grid = dataGridViewFiles;
            CaptureSettingManager.Instance.Grid = dataGridViewCaptureSetting;
            VariableManager.Instance.Grid = dataGridViewVariables;
            MessageManager.Instance.MessageTextBox = textBoxMessage;
            // Attach functions
            myUpdateItemDisplayTimer.Tick += new EventHandler(UpdateItemDisplay);
            myTimerCallback = new TimerCallback(USBDataManager.Instance.HandleIncommingData);
            // Initialize 
            MessageManager.Instance.Initialize();
            designFileName = null;   
            myState = States.Design;               
        }

        public enum States
        {
            Design,
            Action
        }

        public enum DesignStates
        {
            Normal,
            PlaceReadyBooleanItem,
            PlaceReadyLongFloatItem,
            PlaceReadyPlotItem,
            VariableDragged,
            MoveNonPlotItem,
            MovePlotItem
        }

        private void selectFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] data_to_write = new byte[4];
            OpenFileDialog dlg = new OpenFileDialog();

            // Ask for files
            dlg.Filter = "Applicable files (*.map;*.hex;*.c;*.h)|*.map;*.hex;*.c;*.h";
            dlg.Multiselect = true;
            // Check if file selected
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Give file(s) to file manager
                FileManager.Instance.ReceiveFiles(dlg.FileNames);
            }
        }

        private void dataGridViewFiles_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            MenuItem mnu_remove_file;
            MenuItem mnu_remove_all_files;
            ContextMenu mnu_context = new ContextMenu();
            Rectangle cell_rect;

            // Check if right click
            if (e.Button == MouseButtons.Right)
            {
                // Check if design state
                if (myState == States.Design)
                {
                    Console.WriteLine("x = " + e.X + ", Y = " + e.Y);
                    Console.WriteLine("cell x = " + dataGridViewFiles.CurrentCellAddress.X + ", cell y = " + dataGridViewFiles.CurrentCellAddress.Y);

                    if (-1 < e.RowIndex && e.RowIndex < FileManager.Instance.Count)
                    {
                        // Select that item
                        dataGridViewFiles.CurrentCell = dataGridViewFiles.Rows[e.RowIndex].Cells[0];
                        // Create remove file menu
                        mnu_remove_file = new MenuItem();
                        mnu_remove_file.Tag = dataGridViewFiles.Rows[e.RowIndex].Cells[1].Value;
                        mnu_remove_file.Index = 0;
                        mnu_remove_file.Click += new System.EventHandler(this.mnuRemoveFile_Click);
                        mnu_remove_file.Text = "remove this file";
                        // Create remove all files menu
                        mnu_remove_all_files = new MenuItem();
                        mnu_remove_file.Index = 1;
                        mnu_remove_all_files.Click += new System.EventHandler(this.mnuRemoveAllFiles_Click);
                        mnu_remove_all_files.Text = "remove all files";
                        // Add to menu popup
                        mnu_context.MenuItems.Add(mnu_remove_file);
                        mnu_context.MenuItems.Add(mnu_remove_all_files);
                        // Attach to menu context
                        dataGridViewFiles.ContextMenu = mnu_context;
                        // Display menu popup
                        cell_rect = dataGridViewFiles.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                        dataGridViewFiles.ContextMenu.Show(dataGridViewFiles, new Point(cell_rect.X + e.X, cell_rect.Y + e.Y));
                    }
                }
            }
        }

        private void mnuRemoveFile_Click(object sender, EventArgs e)
        {
            string file_name;

            // Get file name
            file_name = (string)((MenuItem)sender).Tag;
            // Remove the selected file            
            FileManager.Instance.RemoveFile(file_name);
            // Check if design file
            if (file_name.EndsWith(".des"))
            {
                // Reset design file 
                designFileName = null;
            }
        }

        private void mnuRemoveAllFiles_Click(object sender, EventArgs e)
        {
            // Remove all files
            FileManager.Instance.RemoveAllFiles();
            // Reset design file 
            designFileName = null;
        }        

        private void booleanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set to appropriate state
            myDesignStates = DesignStates.PlaceReadyBooleanItem;
        }

        private void splitContainerMain_Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            Point p;
            Size size;
            Point offset;

            //Console.WriteLine("Mouse down - splitContainerMain_Panel1");
            // Check left click
            if (e.Button == MouseButtons.Left)
            {                
                // Check if design state
                if (myState == States.Design)
                {       
                    // Check Design states
                    switch (myDesignStates)
                    {
                        case DesignStates.Normal:
                            // Calculate offset
                            offset = new Point(0, 0);                        
                            // Calculate point for searching
                            p = e.Location;
                            p.Offset(offset);
                            // Check if it is inside an item
                            if (NonPlotItems.Instance.Contain(p, out myIndex) == true)
                            {
                                // Get ready for moving that item
                                myDesignStates = DesignStates.MoveNonPlotItem;
                            }
                        break;
                        case DesignStates.PlaceReadyBooleanItem:
                            // Place NonPlotItem (Boolean)
                            p = new Point(e.X, e.Y);
                            // Add new item
                            NonPlotItems.Instance.Add(new NonPlotItem(p, NonPlotItemType.BooleanItem));
                            // Subscribe event handler
                            NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemMouseDown += new NonPlotItem.NonPlotItemMouseDownEventHandler(this.Form1_MouseDownFromNonPlotItem);
                            NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemMouseMove += new NonPlotItem.NonPlotItemMouseMoveEventHandler(this.Form1_MouseMoveFromNonPlotItem);
                            NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemMouseUp += new NonPlotItem.NonPlotItemMouseUpEventHandler(this.Form1_MouseUpFromNonPlotItem);                            
                            NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemDelete += new NonPlotItem.NonPlotItemDeleteEventHandler(this.Form1_DeleteFromNonPlotItem);
                            NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemValueChanged += new NonPlotItem.NonPlotItemValueChangedEventHandler(USBDataManager.Instance.NonPlotItemChanged);
                            // Add to container                           
                            splitContainerMain.Panel1.Controls.Add(NonPlotItems.Instance[NonPlotItems.Instance.Count-1]);
                            myDesignStates = DesignStates.Normal; 
                        break;
                        case DesignStates.PlaceReadyLongFloatItem:
                            // Place NonPlotItem (Long or Float)
                            p = new Point(e.X, e.Y);
                            // Add new item
                            NonPlotItems.Instance.Add(new NonPlotItem(p, NonPlotItemType.LongItem)); // Doesn't matter Long or Float at this point
                            // Subscribe event handler
                            NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemMouseDown += new NonPlotItem.NonPlotItemMouseDownEventHandler(this.Form1_MouseDownFromNonPlotItem);
                            NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemMouseMove += new NonPlotItem.NonPlotItemMouseMoveEventHandler(this.Form1_MouseMoveFromNonPlotItem);
                            NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemMouseUp += new NonPlotItem.NonPlotItemMouseUpEventHandler(this.Form1_MouseUpFromNonPlotItem);
                            NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemDelete += new NonPlotItem.NonPlotItemDeleteEventHandler(this.Form1_DeleteFromNonPlotItem);                            
                            NonPlotItems.Instance[NonPlotItems.Instance.Count - 1].NonPlotItemValueChanged += new NonPlotItem.NonPlotItemValueChangedEventHandler(USBDataManager.Instance.NonPlotItemChanged);
                            // Add to container
                            splitContainerMain.Panel1.Controls.Add(NonPlotItems.Instance[NonPlotItems.Instance.Count - 1]);
                            myDesignStates = DesignStates.Normal;
                        break;
                        case DesignStates.PlaceReadyPlotItem:
                            // Place PlotItem
                            p = new Point(e.X, e.Y);
                            size = new Size(800, 400);
                            PlotItems.Instance.Add(new PlotItem(p, size));
                            // Subscribe event handlers
                            PlotItems.Instance[PlotItems.Instance.Count - 1].PlotItemMouseDown += new PlotItem.PlotItemMouseDownEventHandler(this.Form1_MouseDownFromPlotItem);
                            PlotItems.Instance[PlotItems.Instance.Count - 1].PlotItemMouseMove += new PlotItem.PlotItemMouseMoveEventHandler(this.Form1_MouseMoveFromPlotItem);
                            PlotItems.Instance[PlotItems.Instance.Count - 1].PlotItemMouseUp += new PlotItem.PlotItemMouseUpEventHandler(this.Form1_MouseUpFromPlotItem);
                            PlotItems.Instance[PlotItems.Instance.Count - 1].PlotItemDelete += new PlotItem.PlotItemDeleteEventHandler(this.Form1_DeleteFromPlotItem);
                            // Add to container
                            splitContainerMain.Panel1.Controls.Add(PlotItems.Instance[PlotItems.Instance.Count - 1]);
                            // Back to normal design state
                            myDesignStates = DesignStates.Normal;
                        break;
                    }
                }
            }
        }

        private void dataGridViewVariables_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            Console.WriteLine("Mouse down - gridVariableCell");
            // Check if left click
            if (e.Button == MouseButtons.Left)
            {
                // Check if design state
                if (myState == States.Design)
                {
                    // Make sure it's valid range
                    if (-1 < e.RowIndex)
                    {
                        // Get dropoff info
                        VariableManager.Instance.GetDropoffInfo(e.RowIndex, ref myDropOffInfo);
                        // Check if variable type is known
                        if (myDropOffInfo.variableType != VariableType.Unknown)
                        {
                            // Select that item
                            dataGridViewVariables.CurrentCell = dataGridViewVariables.Rows[e.RowIndex].Cells[0];
                            // Prepare for dragging
                            myDesignStates = DesignStates.VariableDragged;
                            myDropOffInfo.region = RegionTypes.Invalid;
                        }
                        else
                        {
                            // Warning
                            MessageBox.Show("Can't use variable of unknown type");
                        }                        
                    }
                }
            }
        }

        private void splitContainerMain_Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            // Check for proper states
            if (myState == States.Design)
            {
                if (myDesignStates == DesignStates.MoveNonPlotItem)
                {
                    // Move to new location
                    NonPlotItems.Instance[myIndex].Location = e.Location;
                }
            }
        }

        private void splitContainerMain_Panel2_MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("mouse move split container panel 2");
            if (myState == States.Design)
            {
                if (myDesignStates == DesignStates.VariableDragged)
                {
                    this.Cursor = Cursors.No;
                }
            }
        }

        private void dataGridViewVariables_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt;
            int i = 0;
            Point offset;
            Rectangle temp_rect_legend;
            Rectangle temp_rect_x_label;
 
            //Console.WriteLine("mouse move variable view");
            if (myState == States.Design)
            {
                if (myDesignStates == DesignStates.VariableDragged)
                {
                    // Initialize to invalid
                    myDropOffInfo.region = RegionTypes.Invalid;
                    // Calculate offset
                    offset = new Point(tabControlMain.DisplayRectangle.X + dataGridViewVariables.Location.X,
                    splitContainerMain.Panel2.Top + tabControlMain.DisplayRectangle.Y + dataGridViewVariables.Location.Y); 
                    // Check NonPlotItem regions
                    while (myDropOffInfo.region == RegionTypes.Invalid && i < NonPlotItems.Instance.Count)
                    {                        
                        // Get translated point
                        pt = e.Location;                        
                        pt.Offset(offset);
                        //Console.WriteLine("pt.X = " + pt.X + ", " + "pt.Y = " + pt.Y);
                        if (NonPlotItems.Instance[i].Bounds.Contains(pt))
                        {
                            myDropOffInfo.region = RegionTypes.NonPlotItemRegion;
                            myDropOffInfo.itemIndex = i;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    i = 0;
                    // Check PlotItem regions
                    while (myDropOffInfo.region == RegionTypes.Invalid && i < PlotItems.Instance.Count)
                    {                        
                        // Get translated point
                        pt = e.Location;
                        pt.Offset(offset);
                        //Console.WriteLine("pt.X = " + pt.X + ", " + "pt.Y = " + pt.Y);
                        temp_rect_legend = PlotItems.Instance[i].LegendWindow;
                        temp_rect_legend.Offset(PlotItems.Instance[i].Location);
                        temp_rect_x_label = PlotItems.Instance[i].XLabelWindow;
                        temp_rect_x_label.Offset(PlotItems.Instance[i].Location);
                        if (temp_rect_legend.Contains(pt))
                        {
                            myDropOffInfo.region = RegionTypes.LegendRegion;
                            myDropOffInfo.itemIndex = i;
                        }
                        else if (temp_rect_x_label.Contains(pt))
                        {
                            myDropOffInfo.region = RegionTypes.XLabelRegion;
                            myDropOffInfo.itemIndex = i;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    if (myDropOffInfo.region != RegionTypes.Invalid)
                    {
                        this.Cursor = Cursors.Cross;
                    }
                    else
                    {
                        this.Cursor = Cursors.No;
                    }
                }
            }
        }

        private void dataGridViewVariables_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            Console.WriteLine("Mouse up - gridVariableCell");
        }

        private void splitContainerMain_Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse up - splitContainer_Panel1");

            if (myState == States.Design)
            {
                if (myDesignStates == DesignStates.MoveNonPlotItem)
                {
                    myDesignStates = DesignStates.Normal;
                }
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse up - Form1");
        }

        private void splitContainerMain_MouseUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse up - splitContainerMain");     
        }

        private void dataGridViewVariables_MouseUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse up - dataGridViewVariables");
            VariableInfo vi;

            // Check if left
            if (e.Button == MouseButtons.Left)
            {
                // Check if Variable was dragged
                if (myDesignStates == DesignStates.VariableDragged)
                {
                    //Check if valid range                    
                    if (myDropOffInfo.region == RegionTypes.NonPlotItemRegion)
                    {
                        // NonPlotItem region is selected
                        // Assign variable to non plot item
                        NonPlotItems.Instance[myDropOffInfo.itemIndex].Assign(myDropOffInfo.variableName, myDropOffInfo.data);
                        // Assign float or long type
                        if (NonPlotItems.Instance[myDropOffInfo.itemIndex].Type != NonPlotItemType.BooleanItem)
                        {
                            vi = VariableManager.Instance.GetVariable(myDropOffInfo.variableName);
                            if (vi.type == VariableType.Float)
                            {
                                NonPlotItems.Instance[myDropOffInfo.itemIndex].Type = NonPlotItemType.FloatItem;
                            }
                            else
                            {
                                NonPlotItems.Instance[myDropOffInfo.itemIndex].Type = NonPlotItemType.LongItem;
                            }
                        }
                        // Update display
                        NonPlotItems.Instance[myDropOffInfo.itemIndex].Invalidate();
                    }
                    else if (myDropOffInfo.region == RegionTypes.LegendRegion)
                    {
                        // Legend region selected
                        // Check if the selected item exists
                        if (PlotItems.Instance[myDropOffInfo.itemIndex].ContainLegendLabel(myDropOffInfo.variableName) == false)
                        {
                            // Doesn't exist
                            // Add variable to legend
                            PlotItems.Instance[myDropOffInfo.itemIndex].AddLegendLabel(myDropOffInfo.variableName);
                            // Update plot
                            PlotItems.Instance[myDropOffInfo.itemIndex].Invalidate();
                        }                        
                        else
                        {
                            MessageBox.Show("The selected variable already exists");
                        }       
                    }
                    else if (myDropOffInfo.region == RegionTypes.XLabelRegion)
                    {
                        // X label region selected
                        // Assign x label to plot
                        PlotItems.Instance[myDropOffInfo.itemIndex].XLabel = myDropOffInfo.variableName;
                        // Refresh grid
                        CaptureSettingManager.Instance.UpdateDisplay();
                        // Update plot
                        PlotItems.Instance[myDropOffInfo.itemIndex].Invalidate(); 
                    }
                    // Return to normal state
                    myDesignStates = DesignStates.Normal;
                    // Return to normal cursor
                    Cursor = Cursors.Arrow;
                    dataGridViewVariables.Cursor = Cursors.Arrow; // Not sure why, but sometimes I need this
                }

            }
        }

        private void tabPageVariableBrowser_MouseUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse up - tabPageVariableBrowser");    
        }

        private void tabControlMain_MouseUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse up - tabControlMain"); 
        }

        private void splitContainerMain_Panel2_MouseUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse up - splitContainerMain_Panel2"); 
        }

        private void dataGridViewVariables_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse down - dataGridViewVariables");
        }

        private void tabPageVariableBrowser_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse down - tabPageVariableBrowser");
        }

        private void tabControlMain_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse down - tabControlMain");
        }

        private void splitContainerMain_Panel2_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse down - splitContainerMain_Panel2");
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse down - Form1");
        }

        private void splitContainerMain_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse down - splitContainerMain");
        }

        private void actionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if some items are not assigned
            if (NonPlotItems.Instance.AllAssigned() == false || PlotItems.Instance.AllAssigned() == false)
            {
                // Warn and return
                MessageBox.Show("All items need to have their variables assigned");
                return;
            }
            // Check if capture period num are unique
            if (CaptureSettingManager.Instance.CapturePeriodNumIsValid() == false)
            {
                // Warn and return
                MessageBox.Show("Use unique capture period num starting 0 through 3");
                return;
            }
            // Get ready for action
            USBDataManager.Instance.GetReadyForAction1();
            GetReadyForAction();
            SubscriptionTableManager.Instance.GetReadyForAction();
            VariableManager.Instance.GetReadyForAction();
            USBDataManager.Instance.GetReadyForAction2();
            NonPlotItems.Instance.GetReadyForAction();
            PlotItems.Instance.GetReadyForAction();
            CaptureSettingManager.Instance.GetReadyForAction();
        }

        private void GetReadyForAction()
        {
            // Handle appearance
            actionToolStripMenuItem.Checked = true;
            designToolStripMenuItem.Checked = false;
            splitContainerMain.Panel1.BackColor = Color.Gold;
            // Enable/disable commands
            insertToolStripMenuItem.Enabled = false;
            toolsToolStripMenuItem.Enabled = false;
            ColumnStartStop.Visible = true;
            ColumnSave.Visible = true;
            ColumnCapturePeriodNum.ReadOnly = true;
            designToolStripMenuItem.Enabled = true;
            actionToolStripMenuItem.Enabled = false;
            newToolStripMenuItem.Enabled = false;
            loadToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            // Set state
            myState = States.Action;
            // Start update item display timer
            myUpdateItemDisplayTimer.Start();
            myUpdateItemDisplayTimer.Interval = 250;
            // Start incoming data process timer
            incomingDataProcessTimer = new System.Threading.Timer(myTimerCallback, null, 0, 100);
            // Command to go to user application (For now, tell the micro to start running)
            Bootloader.Instance.GoToUserApplication();
            // Pad empty strings
            MessageManager.Instance.EnQueueMessage("\n\n");
            // Add delay for application to stabilize
            Thread.Sleep(250);
        }

        public void MessageFocus()
        {
            tabControlMain.Focus();
        }

        private void designToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get ready for design
            GetReadyForDesign();
            NonPlotItems.Instance.GetReadyForDesign();
            PlotItems.Instance.GetReadyForDesign();
            USBDataManager.Instance.GetReadyForDesign();                  
        }

        private void GetReadyForDesign()
        {
            // Handle Appearance
            actionToolStripMenuItem.Checked = false;
            designToolStripMenuItem.Checked = true;
            splitContainerMain.Panel1.BackColor = Color.Transparent;
            // Set state
            myState = States.Design;
            // Enable/disable commands
            insertToolStripMenuItem.Enabled = true;
            toolsToolStripMenuItem.Enabled = true;
            ColumnStartStop.Visible = false;
            ColumnSave.Visible = false;
            ColumnCapturePeriodNum.ReadOnly = false;
            designToolStripMenuItem.Enabled = false;
            actionToolStripMenuItem.Enabled = true;
            newToolStripMenuItem.Enabled = true;
            loadToolStripMenuItem.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            // Stop update item display timer
            myUpdateItemDisplayTimer.Stop();
            // Check if data process timer exist
            if (incomingDataProcessTimer != null)
            {
                // Stop incoming data process timer
                incomingDataProcessTimer.Dispose();
            }
        }

        private void programToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Programing function not available");

            //byte[] data_to_write = new byte[4];
            //OpenFileDialog dlg = new OpenFileDialog();
            //string hex_file_name;

            //// Check readyness
            //if (Bootloader.Instance.CheckReadyness())
            //{
            //    // Assign hex file to myBootloader
            //    FileManager.Instance.GetFile("Hex", out hex_file_name);
            //    if (hex_file_name == "")
            //    {
            //        MessageBox.Show("No hex file found");
            //        return;
            //    }
            //    Bootloader.Instance.HexFileLocation = hex_file_name;
            //    // Program dsPIC
            //    MessageBox.Show("Program should take about 1 ~ 15 sec");
            //    if (Bootloader.Instance.Program() == true)
            //    {
            //        // Bring the window to front (for some reason it gets lost during programing)
            //        this.BringToFront();
            //        this.Focus();
            //        this.Activate();
            //        // Display message  
            //        MessageBox.Show("Program finished successfully");
            //        // Reload files
            //        FileManager.Instance.HandleFileChange();
            //    }
            //    else
            //    {
            //        // Fail
            //        MessageBox.Show("Program failed!");
            //    }
            //}
            //else
            //{
            //    // Conmmunication fail
            //    MessageBox.Show("Connection to bootloader failed! Program can be performed only in the design mode");
            //}            
        }

        private void InitializeCommunicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string str;

            // Initialize communication to USB card
            FtdiWrapper.Instance.Initialize(out str);
            MessageBox.Show(str);
            if (FtdiWrapper.Instance.Initialized == true)
            {
                // Success
                // Enable/Disable options
                InitializeCommunicationToolStripMenuItem.Enabled = false;
                fileToolStripMenuItem.Enabled = true;
                modeToolStripMenuItem.Enabled = true;
                importProgrammingFilesToolStripMenuItem.Enabled = true;
                programToolStripMenuItem.Enabled = true;
                // Get ready for design
                designToolStripMenuItem_Click(this, new EventArgs());
                // Make sure to be in bootloader to start with
                //if (myBootloader.CheckReadyness() == false)
                //{
                //    USBDataManager.Instance.GoToBootloader();
                //}  
            }
            else
            {
                // Fail
                MessageBox.Show("Make sure that the device is powered and USB driver is installed");
            }            
        }

        private void UpdateItemDisplay(Object myObject, EventArgs myEventArgs)
        {
            int i;

            // Update message item
            MessageManager.Instance.UpdateDisplay();
            // Update non plot items
            NonPlotItems.Instance.UpdateDisplay();
            // For each plot item
            for (i = 0; i < PlotItems.Instance.Count; i++)
            {
                // Check if it is action state
                if (PlotItems.Instance[i].State == PlotItem.States.Action)
                {
                    // Check if plot running
                    if (PlotItems.Instance[i].ActionState == PlotItem.ActionStates.PlotRunning)
                    {
                        // Check if plot is not held
                        if (PlotItems.Instance[i].HoldPlot == false)
                        {
                            // Update display
                            PlotItems.Instance[i].ZoomOutAll();
                        }
                    }
                }
            }  
            // Check capture limit
            CaptureSettingManager.Instance.CheckCaptureLimit();            
        }

        private void Int32FloatItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set to appropriate state
            myDesignStates = DesignStates.PlaceReadyLongFloatItem;
        }

        private void dataGridViewVariables_KeyDown(object sender, KeyEventArgs e)
        {
            char c;
            bool found = false;
            int i = 0;
            string var_name;
            int row_count;

            // Check what key was pressed
            c = Convert.ToChar(e.KeyCode);
            // Check if it is a valid character
            if (c < 91 && 65 > c)
            {
                return;
            }
            // Find first occurance of variable starts with the character
            row_count = dataGridViewVariables.Rows.Count;
            while ((found == false) && (i < row_count))
            {
                var_name = (string)(dataGridViewVariables.Rows[i].Cells[0].Value);
                if (c == var_name[0])
                {
                    found = true;
                }
                i++;
            }
            if (found == true)
            {
                //Select that item
                dataGridViewVariables.CurrentCell = dataGridViewVariables.Rows[i-1].Cells[0];

            }
        }

        private void plotItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set to appropriate state
            myDesignStates = DesignStates.PlaceReadyPlotItem;
        }

        private void Form1_MouseDownFromPlotItem(object sender, EventArgs e)
        {
        }

        private void Form1_MouseDownFromNonPlotItem(object sender, EventArgs e)
        {
        }

        private void Form1_MouseMoveFromPlotItem(object sender, EventArgs e)
        {
            Point pt;

            // Check if move state
            if (((PlotItem)sender).DesignState == PlotItem.DesignStates.Move)
            {
                // Get position of cursor in client coordinate
                pt = PointToClient(Cursor.Position);
                // Translate the point to position in split container
                pt.Offset(splitContainerMain.Location.X, -splitContainerMain.Location.Y);
                // Point needs to be non-negative
                if (pt.X < 0)
                {
                    pt.X = 0;
                }
                if (pt.Y < 0)
                {
                    pt.Y = 0;
                }
                // Set location of item
                ((PlotItem)sender).Location = pt;
            }
        }

        private void Form1_MouseMoveFromNonPlotItem(object sender, EventArgs e)
        {
            Point pt;

            // Check if move state
            if (((NonPlotItem)sender).DesignState == NonPlotItem.DesignStates.Move)
            {
                // Get position of cursor in client coordinate
                pt = PointToClient(Cursor.Position);
                // Translate the point to position in split container
                pt.Offset(splitContainerMain.Location.X, -splitContainerMain.Location.Y);
                // Point needs to be non-negative
                if (pt.X < 0)
                {
                    pt.X = 0;
                }
                if (pt.Y < 0)
                {
                    pt.Y = 0;
                }
                // Set location of item
                ((NonPlotItem)sender).Location = pt;
            }
        }

        private void Form1_MouseUpFromPlotItem(object sender, EventArgs e)
        {
            
        }

        private void Form1_MouseUpFromNonPlotItem(object sender, EventArgs e)
        {

        }

        private void Form1_DeleteFromPlotItem(object sender, EventArgs e)
        {
            // Remove plot from panel
            splitContainerMain.Panel1.Controls.Remove((PlotItem)sender);
            // Remove plot from PlotItems
            PlotItems.Instance.Remove((PlotItem)sender);
            // Refresh capture setting
            CaptureSettingManager.Instance.UpdateDisplay();
        }

        private void Form1_DeleteFromNonPlotItem(object sender, EventArgs e)
        {
            // Remove plot from panel
            splitContainerMain.Panel1.Controls.Remove((NonPlotItem)sender);
            // Remove plot from NonPlotItems
            NonPlotItems.Instance.Remove((NonPlotItem)sender);
        }

        private void dataGridViewCaptureSetting_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            Console.WriteLine("dataGridViewCaptureSetting_CellContentClick");
            CaptureSettingManager.Instance.HandleCellContentClick(e, dataGridViewCaptureSetting);            
        }

        private void dataGridViewCaptureSetting_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            //Console.WriteLine("aa");
        }

        private void dataGridViewCaptureSetting_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            Console.WriteLine("dataGridViewCaptureSetting_CellEndEdit");
            CaptureSettingManager.Instance.HandleCellEndEdit(e);
        }

        private void dataGridViewCaptureSetting_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            Console.WriteLine("RowValidating");
        }

        private void dataGridViewCaptureSetting_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            Console.WriteLine("dataGridViewCaptureSetting_CellValidating");
            CaptureSettingManager.Instance.HandleCellValidating(e);
        }

        private void splitContainerMain_Scroll(object sender, ScrollEventArgs e)
        {
            Console.WriteLine("splitContainerMain_Pan");
        }

        private void splitContainerMain_Panel1_Scroll(object sender, ScrollEventArgs e)
        {
            Console.WriteLine("splitContainerMain_Panel1_Scroll");
        }

        private void splitContainerMain_Panel1_MouseClick(object sender, MouseEventArgs e)
        {
            // Get focus
            splitContainerMain.Panel1.Focus();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save design
            DesignManager.Instance.SaveDesign(ref designFileName, splitContainerMain.Panel1.DisplayRectangle.Location);
        }

        private void ClearDesign()
        {
            int i;

            // For each plot item
            for (i = 0; i < PlotItems.Instance.Count; i++)
            {
                // Detatch from split container
                splitContainerMain.Panel1.Controls.Remove(PlotItems.Instance[i]);
            }
            // For each non plot item
            for (i = 0; i < NonPlotItems.Instance.Count; i++)
            {
                // Detach from split container
                splitContainerMain.Panel1.Controls.Remove(NonPlotItems.Instance[i]);
            }
            // Clear design
            DesignManager.Instance.ClearDesign();
        }

        private void LoadDesign(string file_name)
        {
            int i;

            // Set current design file
            designFileName = file_name;
            // Load design
            DesignManager.Instance.LoadDesign(file_name);
            // For each plot item
            for (i = 0; i < PlotItems.Instance.Count; i++)
            {
                // Attach event handler
                PlotItems.Instance[i].PlotItemMouseDown += new PlotItem.PlotItemMouseDownEventHandler(Form1_MouseDownFromPlotItem);
                PlotItems.Instance[i].PlotItemMouseMove += new PlotItem.PlotItemMouseMoveEventHandler(Form1_MouseMoveFromPlotItem);
                PlotItems.Instance[i].PlotItemMouseUp += new PlotItem.PlotItemMouseUpEventHandler(Form1_MouseUpFromPlotItem);
                PlotItems.Instance[i].PlotItemDelete += new PlotItem.PlotItemDeleteEventHandler(Form1_DeleteFromPlotItem);
                // Attach to split container
                splitContainerMain.Panel1.Controls.Add(PlotItems.Instance[i]);
            }
            // For each non plot item
            for (i = 0; i < NonPlotItems.Instance.Count; i++)
            {
                // Attach event handler
                NonPlotItems.Instance[i].NonPlotItemMouseDown += new NonPlotItem.NonPlotItemMouseDownEventHandler(Form1_MouseDownFromNonPlotItem);
                NonPlotItems.Instance[i].NonPlotItemMouseMove += new NonPlotItem.NonPlotItemMouseMoveEventHandler(Form1_MouseMoveFromNonPlotItem);
                NonPlotItems.Instance[i].NonPlotItemMouseUp += new NonPlotItem.NonPlotItemMouseUpEventHandler(Form1_MouseUpFromNonPlotItem);
                NonPlotItems.Instance[i].NonPlotItemDelete += new NonPlotItem.NonPlotItemDeleteEventHandler(Form1_DeleteFromNonPlotItem);
                // Attach to split container
                splitContainerMain.Panel1.Controls.Add(NonPlotItems.Instance[i]);
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Clear design
            ClearDesign();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            // Set dialog filter
            dlg.Filter = "Design file (*.des)|*.des";
            // Open dialog
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ClearDesign();
                LoadDesign(dlg.FileName);
            }
            else
            {
                return;
            }   
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save design
            DesignManager.Instance.SaveDesignAs(ref designFileName, splitContainerMain.Panel1.DisplayRectangle.Location);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Mini-dSPACE V3.0");
        }

        private void userManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message;

            // Open user manual
            message = "StartSampe: start sampling\n";
            message = "SamplingFrequencyHz: Sampling frequency in Hz.  Valid values are 1, 10, 100, 1000, 83333.34, 262820.5, 2733333, 8200000Hz\n";
            message = "Sample mode: 0 = Interleave, 1 = DMA, 2 = Timer, 3 = DMA with trigger, 4 = Timer with trigger\n";
            message = "Number ofSamples: Number of samples to take\n";
            message = "DurationSec: Duration of the sampling in second\n";
            message = "EnableChannel1~3: Enable channel 1.  The disabled channel will read 0\n";
            message = "Ch1~3Probe10x: Check it if using 10x probe.  Uncheck if using 1x probe\n";
            message = "RisingEdgeTriggerEnable: Enable trigger on rising edge\n";
            message = "FallingEdgeTriggerEnable: Enable trigger on falling edge\n";
            message = "Number of sample to take before the trigger\n";
            message = "TriggerVoltageLevel: Trigger level in volt\n";
            message = "StartFrequencyGeneration: Generate a test squarewave\n";
            message = "FreqGenerationPeriod:  The period of the squarewave.  499 = 1kHz, 49 = 10kHz, 4 = 100kHz, 1 = 250kHz (1MHz/((FreqGenerationPeriod + 1)*2)) \n";
            //MessageBox.Show(message);
            Process.Start("scope_manual.txt");            
        }
    }

    public enum RegionTypes
    {
        Invalid,
        NonPlotItemRegion,
        LegendRegion,
        XLabelRegion
    }

    public struct DropOffInfo
    {
        public RegionTypes region;
        public int itemIndex;
        public string variableName;
        public VariableType variableType;
        public uint variableAddress;
        public NonPlotData data;
    }  
}
