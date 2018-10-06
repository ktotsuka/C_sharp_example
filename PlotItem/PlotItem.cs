using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Collections;

namespace PlotItemSpace
{
    public partial class PlotItem : Control
    {     
        public PlotDataSets yPlotDataSets = new PlotDataSets();
        public PlotDataSet xPlotDataSet;
        private Rectangle legendWindow;
        private Rectangle plotWindow;
        private Rectangle xLabelWindow;
        private DBGraphics memGraphics;
        private Point pendingPoint;
        private Rectangle zoomRect;
        private bool drawGrid;
        private States state;
        private List<Pen> LegendPens = new List<Pen>();
        private Color[] myColors = new Color[7];
        private DesignStates designState;
        private ActionStates actionState;
        private ZoomStates zoomState;
        private bool drawSymbol;
        private bool holdPlot;
        private float plotWindowMinX;
        private float plotWindowMaxX;
        private float plotWindowMinY;
        private float plotWindowMaxY;
        private float validPlotWindowMinX;
        private float validPlotWindowMaxX;
        private float validPlotWindowMinY;
        private float validPlotWindowMaxY;
        private Pen zoomPen;
        private string xLabel;
        private List<string> legendLabels = new List<string>();
        public delegate void PlotItemMouseDownEventHandler(object sender, EventArgs e);
        public delegate void PlotItemMouseMoveEventHandler(object sender, EventArgs e);
        public delegate void PlotItemMouseUpEventHandler(object sender, EventArgs e);
        public delegate void PlotItemDeleteEventHandler(object sender, EventArgs e);
        public delegate void PlotItemExtraDataProcessUpdateHandler(); 
        public event PlotItemMouseDownEventHandler PlotItemMouseDown;
        public event PlotItemMouseMoveEventHandler PlotItemMouseMove;
        public event PlotItemMouseUpEventHandler PlotItemMouseUp;
        public event PlotItemDeleteEventHandler PlotItemDelete;

        public PlotItem(Point location, Size size)
        {
            ContextMenu mnu_context = new ContextMenu();
            MenuItem mnu_delete = new MenuItem();
            MenuItem mnu_zoom_in = new MenuItem();
            MenuItem mnu_zoom_out = new MenuItem();
            MenuItem mnu_pan = new MenuItem();
            MenuItem mnu_pinpoint = new MenuItem();
            MenuItem mnu_grid = new MenuItem();
            MenuItem mnu_symbol = new MenuItem();
            MenuItem mnu_hold = new MenuItem();

            InitializeComponent();
            // Initialize
            Location = location;
            Size = size;
            designState = DesignStates.Normal;
            actionState = ActionStates.PlotReady;
            zoomState = ZoomStates.ZoomInReady;
            MinimumSize = new Size(200, 100);
            drawGrid = false;
            drawSymbol = false;
            holdPlot = false;
            // Initialize graphics
            memGraphics = new DBGraphics();
            memGraphics.CreateDoubleBuffer(this.CreateGraphics(), this.ClientRectangle.Width, this.ClientRectangle.Height);
            zoomPen = new Pen(Color.Black);
            zoomPen.DashStyle = DashStyle.Dash;
            myColors[0] = Color.Black;
            myColors[1] = Color.Red;
            myColors[2] = Color.Green;
            myColors[3] = Color.Blue;
            myColors[4] = Color.Cyan;
            myColors[5] = Color.Magenta;
            myColors[6] = Color.Yellow;
            // Create menus for context menue
            mnu_delete.Text = "Delete";
            mnu_delete.Name = mnu_delete.Text;
            mnu_delete.Click += new System.EventHandler(this.mnuDelete_Click);
            mnu_zoom_in.Text = "Zoom in";
            mnu_zoom_in.Name = mnu_zoom_in.Text;
            mnu_zoom_in.Click += new System.EventHandler(this.mnuZoomIn_Click);
            mnu_zoom_out.Text = "Zoom out";
            mnu_zoom_out.Name = mnu_zoom_out.Text;
            mnu_zoom_out.Click += new System.EventHandler(this.mnuZoomOut_Click);
            mnu_pan.Text = "Pan";
            mnu_pan.Name = mnu_pan.Text;
            mnu_pan.Click += new System.EventHandler(this.mnuPan_Click);
            mnu_pinpoint.Text = "Pinpoint";
            mnu_pinpoint.Name = mnu_pinpoint.Text;
            mnu_pinpoint.Click += new System.EventHandler(this.mnuPinpoint_Click);
            mnu_grid.Text = "Grid";
            mnu_grid.Name = mnu_grid.Text;
            mnu_grid.Click += new System.EventHandler(this.mnuGrid_Click);
            mnu_symbol.Text = "Symbol";
            mnu_symbol.Name = mnu_symbol.Text;
            mnu_symbol.Click += new System.EventHandler(this.mnuSymbol_Click);
            mnu_hold.Text = "Hold";
            mnu_hold.Name = mnu_hold.Text;
            mnu_hold.Click += new System.EventHandler(this.mnuHold_Click);
            // Add context menu
            mnu_context.MenuItems.Add(mnu_delete);
            mnu_context.MenuItems.Add("-");
            mnu_context.MenuItems.Add(mnu_zoom_in);
            mnu_context.MenuItems.Add(mnu_zoom_out);
            mnu_context.MenuItems.Add(mnu_pan);
            mnu_context.MenuItems.Add(mnu_pinpoint);
            mnu_context.MenuItems.Add("-");
            mnu_context.MenuItems.Add(mnu_grid);
            mnu_context.MenuItems.Add(mnu_symbol);
            mnu_context.MenuItems.Add(mnu_hold);
            mnu_context.MenuItems.Add("-");
            mnu_context.MenuItems["Zoom in"].Checked = true;
            this.ContextMenu = mnu_context;

        }

        public enum States
        {
            Design,
            Action
        }

        public enum DesignStates
        {
            Normal,
            ResizeNWSEHover,
            ResizeNSHover,
            ResizeWEHover, 
            ResizeNWSE,
            ResizeNS,
            ResizeWE,
            Move
        }

        public enum ActionStates
        {
            PlotReady,
            PlotRunning,
        }

        public enum ZoomStates
        {
            ZoomInReady,
            ZoomInActive,
            ZoomOut,
            PanReady,
            PanActive,
            Pinpoint
        }        

        public bool IsAssigned()
        {
            // Check if x and y variables are assigned
            if (XLabel != null && legendLabels.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void EnterPlotState()
        {
            //memGraphics.CreateDoubleBuffer(this.CreateGraphics(), ClientRectangle.Width, ClientRectangle.Height);
        }

        public bool ContainLegendLabel(string label)
        {
            return legendLabels.Contains(label);
        }

        public void AddLegendLabel(string label)
        {
            MenuItem menu_y_var = new MenuItem();
            Pen new_pen;

            new_pen = new Pen(myColors[(legendLabels.Count) % myColors.Length]);
            legendLabels.Add(label);
            LegendPens.Add(new_pen);
            // Update context menu
            menu_y_var.Text = "Delete y variable: " + label;
            menu_y_var.Name = label;
            menu_y_var.Click += new System.EventHandler(this.mnuDeleteYVar_Click);
            ContextMenu.MenuItems.Add(menu_y_var);
        }

        public int CountLegendLabel()
        {
            return legendLabels.Count;
        }

        public string XLabel
        {
            get
            {
                return xLabel;
            }
            set
            {
                xLabel = value;
            }
        }

        public bool DrawSymbol
        {
            get
            {
                return drawSymbol;
            }
            set
            {
                drawSymbol = value;
            }
        }

        public bool HoldPlot
        {
            get
            {
                return holdPlot;
            }
            set
            {
                holdPlot = value;
            }
        }

        public bool DrawGrid
        {
            get
            {
                return drawGrid;
            }
            set
            {
                drawGrid = value;
            }
        }

        public void ZoomOutAll()
        {
            // Get the max and min value of the window
            plotWindowMinX = xPlotDataSet.Min - 0.1f*(xPlotDataSet.Max - xPlotDataSet.Min);
            plotWindowMaxX = xPlotDataSet.Max + 0.1f * (xPlotDataSet.Max - xPlotDataSet.Min);
            plotWindowMinY = yPlotDataSets.Min - 0.1f * (yPlotDataSets.Max - yPlotDataSets.Min);
            plotWindowMaxY = yPlotDataSets.Max + 0.1f * (yPlotDataSets.Max - yPlotDataSets.Min); 
            // Update display
            Invalidate();
        }

        private bool DrawPlot(Graphics g, Rectangle window)
        {
            // g: Graphics to be used
            // window: Window to draw plot on
            // Output: True if plot success, false otherwise
            // 
            // 

            GraphicsPath gp;
            int i, j;
            Point start_pt;
            Point end_pt;
            int end_pt_y;
            int[] x_locations;
            float temp;
            int num_data_pts;        
            Rectangle active_window = new Rectangle(0, 0, plotWindow.Width, plotWindow.Height);

            // Capture number of x data points in case it changes during plotting (extra data process function could be called)
            num_data_pts = xPlotDataSet.Count;
            // Get Start position for data x and y
            xPlotDataSet.GetReset();
            for (j = 0; j < yPlotDataSets.Count; j++)
            {
                yPlotDataSets[j].GetReset();
            }
            // Handel one value case
            if (xPlotDataSet.Min == xPlotDataSet.Max)
            {
                plotWindowMinX = (float)Math.Floor((double)xPlotDataSet.Min - 1);
                plotWindowMaxX = (float)Math.Floor((double)xPlotDataSet.Max + 1);
            }
            if (yPlotDataSets.Min == yPlotDataSets.Max)
            {
                plotWindowMinY = (float)Math.Floor((double)yPlotDataSets.Min - 1);
                plotWindowMaxY = (float)Math.Floor((double)yPlotDataSets.Max + 1);
            }
            // Get x points
            x_locations = new int[num_data_pts];
            for (i = 0; i < num_data_pts; i++)
            {
                temp = xPlotDataSet.Get();
                x_locations[i] = (int)((float)(plotWindow.Width) * (temp - plotWindowMinX) / (plotWindowMaxX - plotWindowMinX));
            }
            // For each y variable
            for (j = 0; j < yPlotDataSets.Count; j++)
            {
                // Start new path
                gp = new GraphicsPath();
                // Calculate initial x y position
                // Calculate current position
                temp = yPlotDataSets[j].Get();
                end_pt_y = (int)((float)(plotWindow.Height) * (plotWindowMaxY - temp) / (plotWindowMaxY - plotWindowMinY));
                end_pt = new Point(x_locations[0], end_pt_y);
                // Check if it is inside active window
                if (active_window.Contains(end_pt))
                {
                    // Check if symbol should be drawn
                    if (drawSymbol == true)
                    {
                        // Draw symbol
                        gp.AddEllipse(end_pt.X - 2, end_pt.Y - 2, 4, 4);
                    }          
                }
                // For each y data point following the initial point
                for (i = 1; i < num_data_pts; i++)
                {
                    // Get starting point of line
                    start_pt = end_pt;
                    // Get next point
                    end_pt_y = (int)((float)(plotWindow.Height) * (plotWindowMaxY - yPlotDataSets[j].Get()) / (plotWindowMaxY - plotWindowMinY));
                    end_pt = new Point(x_locations[i], end_pt_y);
                    // Check if line should be drawn
                    if (active_window.Contains(start_pt) || active_window.Contains(end_pt))
                    {
                        if (start_pt.X != end_pt.X || start_pt.Y != end_pt.Y)
                        {
                            // Check if symbol should be drawn
                            if (drawSymbol == true)
                            {
                                // Draw symbol
                                gp.AddEllipse(end_pt.X - 2, end_pt.Y - 2, 4, 4);
                            }
                            // Add line between two points
                            gp.AddLine(start_pt, end_pt);
                        }
                    }
                }
                g.SetClip(window); 
                g.TranslateTransform(window.Location.X, window.Location.Y);
                try
                {
                    g.DrawPath(LegendPens[j], gp);
                }
                catch
                {
                    Console.WriteLine("Exception raised");
                    g.ResetClip();
                    g.TranslateTransform(-window.Location.X, -window.Location.Y);
                    return false;
                }
                g.ResetClip();
                g.TranslateTransform(-window.Location.X, -window.Location.Y);                
            }
            // Draw x and y ticks
            if (DrawTicks(g, true, plotWindowMinX, plotWindowMaxX, plotWindow) == false)
            {
                return false;
            }
            if (DrawTicks(g, false, plotWindowMinY, plotWindowMaxY, plotWindow) == false)
            {
                return false;
            }                    
            return true;
        }

        private bool DrawTicks(Graphics g, bool xy, float min, float max, Rectangle window)
        {
            int i;
            Point tick_point_start;
            Point tick_point_end;
            Point grid_point_start;
            Point grid_point_end;
            int interp;
            float adj_tick_value_now;
            Size label_size;
            float tick_interval;
            float tick_value_now;
            float adj_tick_interval;
            string format_string;
            string tick_label;
            int adj_exp;
            float adj_mul;
            float abs_max;
            PlotDataSet ticks;
            Point label_point;
            Pen dash_pen = new Pen(Color.Black, 1);        

            // Initialize dash pen
            dash_pen.DashStyle = DashStyle.Dash;
            // Get tick values
            ticks = PlotDataSet.GetTickIntervals(min, max);
            // Check if tick is invalid
            if (ticks == null)
            {
                // Error
                return false;
            }
            // Get tick interval
            tick_interval = (ticks.Max - ticks.Min) / ((float)(ticks.Count) - 1f);
            // Get abs max of all tick values
            abs_max = Math.Max(Math.Abs(ticks.Min), ticks.Max); 
            // Check if above 10000
            if (10000 <= abs_max)
            {
                // Get power to be adjusted
                adj_exp = GetExp(abs_max);
            }
            else if (tick_interval < 0.0001)
            {
                adj_exp = GetExp(abs_max);
            }
            else
            {
                // No need to adjust power
                adj_exp = 0;
            }
            // Check if power is adjusted
            if (adj_exp != 0) 
            {
                // Check if this is for x axis
                if (xy == true)
                {
                    // Display power adjustment for x axis
                    g.DrawString("x 10",Font, Brushes.Black, new Point(window.Right - 35, window.Bottom + 25));
                    g.DrawString((adj_exp).ToString(), new Font("Times New Roman", 8), Brushes.Black, new Point(window.Right - 16, window.Bottom + 15));
                }
                else 
                {
                    // Display power adjustment for y axis
                    g.DrawString("x 10", Font, Brushes.Black, new Point(window.Left - 35, window.Top - 15));
                    g.DrawString((adj_exp).ToString(), new Font("Times New Roman", 8), Brushes.Black, new Point(window.Left - 16, window.Top - 25));
                }
            }
            // Adjust tick interval
            adj_mul = (float)(Math.Pow(10, -adj_exp));
            adj_tick_interval = tick_interval * adj_mul; 
            // Determin number of digit to display
            if (1 <= adj_tick_interval)
            {
                format_string = "####";

            }
            else if (0.1 <= adj_tick_interval && adj_tick_interval < 1)
            {
                format_string = "####.#";
            }
            else if (0.01 <= adj_tick_interval && adj_tick_interval < 0.1)
            {
                format_string = "####.##";
            }
            else if (0.001 <= adj_tick_interval && adj_tick_interval < 0.01)
            {
                format_string = "####.###";
            }
            else if (0.0001 <= adj_tick_interval && adj_tick_interval < 0.001)
            {
                format_string = "####.####";
            }
            else if (0.00001 <= adj_tick_interval && adj_tick_interval < 0.0001)
            {
                format_string = "####.#####";
            }
            else if (0.000001 <= adj_tick_interval && adj_tick_interval < 0.00001)
            {
                format_string = "####.######";
            }
            else if (0.0000001 <= adj_tick_interval && adj_tick_interval < 0.000001)
            {
                format_string = "####.#######";
            }
            else if (0.00000001 <= adj_tick_interval && adj_tick_interval < 0.0000001)
            {
                format_string = "####.########";
            }
            else 
            {
                format_string = "####";
            }
            // For each tick
            for (i = 0; i < ticks.Count; i++)
            {
                // Get adjusted tick value
                tick_value_now = ticks.Get();
                adj_tick_value_now = tick_value_now * adj_mul;
                // Check if for x axis
                if (xy == true)
                {
                    // Get tick start and end point
                    interp = (int)((float)(window.Width) * (tick_value_now - min) / (max - min));
                    tick_point_start = new Point(interp + window.Location.X, window.Location.Y + window.Height);
                    tick_point_end = new Point(tick_point_start.X, tick_point_start.Y - 5);
                    // Get grid start and end point
                    grid_point_start = tick_point_end;
                    grid_point_end = new Point(grid_point_start.X, window.Location.Y);                    
                }
                else
                {
                    // Get tick start and end point
                    interp = (int)((float)(window.Height) * (tick_value_now - plotWindowMinY) / (plotWindowMaxY - plotWindowMinY));
                    tick_point_start = new Point(window.Location.X, window.Location.Y + window.Height - interp);
                    tick_point_end = new Point(tick_point_start.X + 5, tick_point_start.Y);
                    // Get grid start and end point
                    grid_point_start = tick_point_end;
                    grid_point_end = new Point(window.Location.X + window.Width, grid_point_start.Y);    
                }
                // Draw tick line
                g.DrawLine(Pens.Black, tick_point_start, tick_point_end);
                // Draw grid
                if (drawGrid == true)
                {
                    g.DrawLine(dash_pen, grid_point_start, grid_point_end);
                }                
                // Check for zero
                if (adj_tick_value_now == 0)
                {
                    tick_label = "0";
                }
                else
                {
                    // Determine string
                    tick_label = adj_tick_value_now.ToString(format_string);
                }
                label_size = (g.MeasureString(tick_label, this.Font)).ToSize();
                // Check if for x axis
                if (xy == true)
                {
                    // Calculate tick label position
                    label_point = new Point(tick_point_start.X - label_size.Width / 2, tick_point_start.Y + 5);
                }
                else
                {
                    // Calculate tick label position
                    label_point = new Point(tick_point_start.X - label_size.Width - 5, tick_point_start.Y - label_size.Height / 2);
                }
                // Draw tick label
                g.DrawString(tick_label, this.Font, Brushes.Black, label_point);
            }
            return true;
        }

        private int GetExp(float num)
        {
            int exp = 0;

            // Get absolute number
            if (num < 0)
            {
                num = -num;
            }
            // Check if number is 0
            if (num == 0)
            {
                // Return exp = 0
                return 0;
            }
            // Check if number is less than 1
            if (num < 1)
            {
                // Get power needed to be > 10
                while (num < 10)
                {
                    num = num * 10;
                    exp--;
                }
            }
            else
            {
                // Get power needed to be <= 10
                while (10 <= num)
                {
                    num = num / 10;
                    exp++;
                }
            }
            return exp;
        }

        public void AddPlotDataY(PlotDataSet set)
        {
            yPlotDataSets.Add(set);
        }

        public void AssignPlotDataX(PlotDataSet set)
        {
            xPlotDataSet = set;
        }

        private void DeleteDataY(int index)
        {            
            yPlotDataSets.RemoveAt(index);
        }

        public void DeleteAllDataY()
        {
            yPlotDataSets.Clear();
        }

        public void EmptyAllData()
        {
            int i;

            // Empty x data
            xPlotDataSet.Empty();
            // For each y variables
            for (i = 0; i < yPlotDataSets.Count; i++)
            {
                // Empty data
                yPlotDataSets[i].Empty();
            }
        }

        public Rectangle LegendWindow 
        {
            get
            {
                return legendWindow;
            }
            set
            {
                legendWindow = value;
            }
        }

        public Rectangle XLabelWindow
        {
            get
            {
                return xLabelWindow;
            }
            set
            {
                xLabelWindow = value;

            }
        }

        public DesignStates DesignState
        {
            get
            {
                return designState;
            }
            set
            {
                designState = value;
            }
        }

        public ActionStates ActionState
        {
            get
            {
                return actionState;
            }
            set
            {
                actionState = value;
            }
        }

        public ZoomStates ZoomState
        {
            get
            {
                return zoomState;
            }
            set
            {
                zoomState = value;
            }
        }


        public States State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //Console.WriteLine("step 2\n");
        }

        private void PlotItem_Paint(object sender, PaintEventArgs e)
        {
            int i;
            Point legend_point;
            StringFormat sf = new StringFormat();

            if (memGraphics.CanDoubleBuffer())
            {           
                //Console.WriteLine("PlotItem_Paint\n");
                // Draw background
                memGraphics.g.FillRectangle(Brushes.LightGray, ClientRectangle);
                memGraphics.g.DrawRectangle(Pens.Black, new Rectangle(ClientRectangle.X,
                ClientRectangle.Y, ClientRectangle.Width - 1,
                ClientRectangle.Height - 1));
                // Draw legend window
                if (legendLabels.Count == 0)
                {
                    legendWindow = new Rectangle(Const.LeftSpace, 0, ClientRectangle.Width - Const.RightSpace - Const.LeftSpace, Font.Height + 2);
                    memGraphics.g.FillRectangle(Brushes.Red, legendWindow);
                }
                else
                {
                    legendWindow = new Rectangle(Const.LeftSpace, 0, ClientRectangle.Width - Const.RightSpace - Const.LeftSpace, legendLabels.Count * Font.Height + 2);
                    memGraphics.g.FillRectangle(Brushes.White, legendWindow);
                }                
                memGraphics.g.DrawRectangle(Pens.Black, legendWindow);
                // Draw x label
                sf.Alignment = StringAlignment.Center;
                xLabelWindow = new Rectangle(legendWindow.X, ClientRectangle.Height - 15, legendWindow.Width, 15);
                if (xLabel != null)
                {
                    memGraphics.g.DrawString(xLabel, this.Font, Brushes.Black, xLabelWindow, sf);
                }
                else
                {
                    memGraphics.g.DrawString("drop x variable here", this.Font, Brushes.Red, xLabelWindow, sf);
                }
                // Draw plot window
                plotWindow = new Rectangle(legendWindow.X, legendWindow.Height + 20, legendWindow.Width, ClientRectangle.Height - legendWindow.Height - 20 - Const.BottomSpace);
                memGraphics.g.FillRectangle(Brushes.White, plotWindow);
                memGraphics.g.DrawRectangle(Pens.Black, plotWindow);
                // Check if y data exist
                if (legendLabels.Count == 0)
                {
                    // Draw generic text
                    memGraphics.g.DrawString("drop y variables here", this.Font, Brushes.Black, legendWindow.Location);
                }
                else 
                {
                    // For each y variable
                    for (i = 0; i < legendLabels.Count; i++)
                    {
                        // Draw legend line
                        legend_point = legendWindow.Location;
                        legend_point.Offset(0, Font.Height * i);
                        memGraphics.g.DrawLine(LegendPens[i], legend_point.X + 5, legend_point.Y + Font.Height / 2, legend_point.X + 45, legend_point.Y + Font.Height / 2);                    
                        // Draw legend text                        
                        legend_point.Offset(50, 0);                        
                        memGraphics.g.DrawString(legendLabels[i], this.Font, Brushes.Black, legend_point);
                    }
                }    
                // Check if plot should be drawn
                if (State == States.Action && xPlotDataSet.Count != 0)
                {
                    // Draw plot                    
                    if (DrawPlot(memGraphics.g, plotWindow) == false)
                    {
                        // Invalid plot setting, so bring back to known good state                        
                        plotWindowMinX = validPlotWindowMinX;
                        plotWindowMaxX = validPlotWindowMaxX;
                        plotWindowMinY = validPlotWindowMinY;
                        plotWindowMaxY = validPlotWindowMaxY;
                        Invalidate();
                        MessageBox.Show("Can't zoom that much!");
                        return;
                    }
                    else
                    {
                        // Everything is valid, so update PlotWindowMinMax
                        validPlotWindowMinX = plotWindowMinX;
                        validPlotWindowMaxX = plotWindowMaxX;
                        validPlotWindowMinY = plotWindowMinY;
                        validPlotWindowMaxY = plotWindowMaxY;
                    }
                    // Check if zoom rectangle should be drawn
                    if (State == States.Action && (actionState == ActionStates.PlotReady || holdPlot == true) && zoomState == ZoomStates.ZoomInActive)
                    {
                        // Draw Zoom rectangle
                        memGraphics.g.DrawRectangle(zoomPen, zoomRect);
                    }
                    // Check if pinpoint data should be drawn
                    if (State == States.Action && (actionState == ActionStates.PlotReady || holdPlot == true) && zoomState == ZoomStates.Pinpoint)
                    {
                        // Draw pinpoint data 
                        DisplayPinpointData(memGraphics.g);
                    }
                }
                // Render to the form
                memGraphics.Render(e.Graphics);
            }

            //Diagnose.StopTimer();
        }

        private void PlotItem_MouseDown(object sender, MouseEventArgs e)
        {
            int legend_index;
            ColorDialog color_dlg;
            int i;            

            // Check if left click
            if (e.Button == MouseButtons.Left)
            {
                if (State == States.Design)
                {
                    // Check if inside legend window
                    if (legendWindow.Contains(e.Location))
                    {
                        // Find out which legend lable was selected
                        legend_index = (e.Location.Y - legendWindow.Location.Y) / Font.Height;
                        // Open color chooser
                        color_dlg = new ColorDialog();
                        if (color_dlg.ShowDialog() != DialogResult.Cancel)
                        {
                            LegendPens[legend_index].Color = color_dlg.Color;
                            Invalidate();
                        }                        
                    }
                    else
                    {
                        if (DesignState == DesignStates.Normal)
                        {
                            DesignState = DesignStates.Move;
                            if (PlotItemMouseDown != null)
                            {
                                PlotItemMouseDown(this, e);
                            }
                        }
                        else if (DesignState == DesignStates.ResizeNWSEHover)
                        {
                            DesignState = DesignStates.ResizeNWSE;
                        }
                        else if (DesignState == DesignStates.ResizeNSHover)
                        {
                            DesignState = DesignStates.ResizeNS;
                        }
                        else if (DesignState == DesignStates.ResizeWEHover)
                        {
                            DesignState = DesignStates.ResizeWE;
                        }
                    }
                }
                else if (State == States.Action)
                {
                    // Check if plot ready or plot-held
                    if (actionState == ActionStates.PlotReady || holdPlot == true)
                    {
                        // Check if within plot window
                        if (plotWindow.Contains(e.X, e.Y))
                        {                            
                            // Check if zoom-in ready
                            if (zoomState == ZoomStates.ZoomInReady)
                            {
                                // Refresh pending point
                                pendingPoint = new Point(e.X, e.Y);
                                // Set to zoom in active
                                zoomState = ZoomStates.ZoomInActive;
                            }
                            else if (zoomState == ZoomStates.ZoomOut)
                            {
                                // Zoom out
                                plotWindowMinX = Math.Max(plotWindowMinX * 0.5f, xPlotDataSet.Min);
                                plotWindowMaxX = Math.Min(plotWindowMaxX * 1.5f, xPlotDataSet.Max);
                                plotWindowMinY = Math.Max(plotWindowMinY * 0.5f, yPlotDataSets.Min);
                                plotWindowMaxY = Math.Min(plotWindowMaxY * 1.5f, yPlotDataSets.Max);
                                Invalidate();
                            }
                            else if (zoomState == ZoomStates.PanReady)
                            {
                                // Refresh pending point
                                pendingPoint = new Point(e.X, e.Y);
                                // Set to pan active
                                zoomState = ZoomStates.PanActive;
                            }
                            else if (zoomState == ZoomStates.Pinpoint)
                            {
                                // Refresh pending point
                                pendingPoint = new Point(e.X, e.Y);
                                // Update display
                                Invalidate();                                
                            }
                        }
                    }
                }
            }
            else
            {
                // Check if design state
                if (state == States.Design)
                {
                    // Set Enable/Disable 
                    ContextMenu.MenuItems["Delete"].Enabled = true;
                    ContextMenu.MenuItems["Zoom in"].Enabled = false;
                    ContextMenu.MenuItems["Zoom out"].Enabled = false;
                    ContextMenu.MenuItems["Pan"].Enabled = false;
                    ContextMenu.MenuItems["Pinpoint"].Enabled = false;
                    ContextMenu.MenuItems["Hold"].Enabled = false;
                    // For each y var
                    for (i = 0; i < legendLabels.Count; i++)
                    {
                        // Enable y var delete
                        ContextMenu.MenuItems[legendLabels[i]].Enabled = true;                       
                    }
                }
                else
                {
                    // Set Enable/Disable  
                    ContextMenu.MenuItems["Delete"].Enabled = false;
                    ContextMenu.MenuItems["Hold"].Enabled = true;
                    // Check if plot ready
                    if (actionState == ActionStates.PlotReady || holdPlot == true)
                    {
                        // Set Enable/Disable 
                        ContextMenu.MenuItems["Zoom in"].Enabled = true;
                        ContextMenu.MenuItems["Zoom out"].Enabled = true;
                        ContextMenu.MenuItems["Pan"].Enabled = true;
                        ContextMenu.MenuItems["Pinpoint"].Enabled = true;  
                    }
                    else
                    {
                        // Set Enable/Disable  
                        ContextMenu.MenuItems["Zoom in"].Enabled = false;
                        ContextMenu.MenuItems["Zoom out"].Enabled = false;
                        ContextMenu.MenuItems["Pan"].Enabled = false;
                        ContextMenu.MenuItems["Pinpoint"].Enabled = false;  
                    }
                    // For each y var
                    for (i = 0; i < legendLabels.Count; i++)
                    {
                        // Disable y var delete
                        ContextMenu.MenuItems[legendLabels[i]].Enabled = false;
                    }
                }    
                // Set check state
                if (zoomState == ZoomStates.ZoomInReady)
                {
                    ContextMenu.MenuItems["Zoom in"].Checked = true;
                }
                else
                {
                    ContextMenu.MenuItems["Zoom in"].Checked = false;
                }
                if (zoomState == ZoomStates.ZoomOut)
                {
                    ContextMenu.MenuItems["Zoom out"].Checked = true;
                }
                else
                {
                    ContextMenu.MenuItems["Zoom out"].Checked = false;
                }
                if (zoomState == ZoomStates.PanReady)
                {
                    ContextMenu.MenuItems["Pan"].Checked = true;
                }
                else
                {
                    ContextMenu.MenuItems["Pan"].Checked = false;
                }
                if (zoomState == ZoomStates.Pinpoint)
                {
                    ContextMenu.MenuItems["Pinpoint"].Checked = true;
                }
                else
                {
                    ContextMenu.MenuItems["Pinpoint"].Checked = false;
                }
                if (drawGrid == true)
                {
                    ContextMenu.MenuItems["Grid"].Checked = true;
                }
                else
                {
                    ContextMenu.MenuItems["Grid"].Checked = false;
                }
                if (drawSymbol == true)
                {
                    ContextMenu.MenuItems["Symbol"].Checked = true;
                }
                else
                {
                    ContextMenu.MenuItems["Symbol"].Checked = false;
                }
                if (holdPlot == true)
                {
                    ContextMenu.MenuItems["Hold"].Checked = true;
                }
                else
                {
                    ContextMenu.MenuItems["Hold"].Checked = false;
                }
            }
        }

        private void DisplayPinpointData(Graphics g)
        {
            int i, j;
            Point pt;
            int pt_y;
            int[] x_locations;
            float dist;
            int num_data_pts;
            int min_y_var_index;
            float[] x_values;
            float y_value;
            float min_dist;
            float min_x_value;
            float min_y_value;
            Point min_pt;
            string str;
            Point translated_pending_point;
            Rectangle active_window = new Rectangle(0, 0, plotWindow.Width, plotWindow.Height);

            // Make sure pending point exist
            if (pendingPoint.IsEmpty == true)
            {
                return;
            }
            // Initialize
            min_dist = 0;
            min_x_value = 0;
            min_y_value = 0;
            min_y_var_index = 0;
            min_pt = new Point();
            translated_pending_point = new Point(pendingPoint.X - plotWindow.X, pendingPoint.Y - plotWindow.Y);
            // Capture number of x data points 
            num_data_pts = xPlotDataSet.Count;
            // Get Start position for data x and y
            xPlotDataSet.GetReset();
            for (j = 0; j < yPlotDataSets.Count; j++)
            {
                yPlotDataSets[j].GetReset();
            }
            // Get x points
            x_locations = new int[num_data_pts];
            x_values = new float[num_data_pts];
            for (i = 0; i < num_data_pts; i++)
            {
                x_values[i] = xPlotDataSet.Get();
                x_locations[i] = (int)((float)(plotWindow.Width) * (x_values[i] - plotWindowMinX) / (plotWindowMaxX - plotWindowMinX));
            }
            // For each y variable
            for (j = 0; j < yPlotDataSets.Count; j++)
            {
                // For each y data point 
                for (i = 0; i < num_data_pts; i++)
                {
                    // Get next point
                    y_value = yPlotDataSets[j].Get();
                    pt_y = (int)((float)(plotWindow.Height) * (plotWindowMaxY - y_value) / (plotWindowMaxY - plotWindowMinY));
                    pt = new Point(x_locations[i], pt_y);
                    // Check if point is within active window
                    if (active_window.Contains(pt))
                    {
                        // Calculate distance
                        dist = (pt.X - translated_pending_point.X) * (pt.X - translated_pending_point.X) + (pt.Y - translated_pending_point.Y) * (pt.Y - translated_pending_point.Y);
                        // Check if dist is minimum
                        if (dist < min_dist || min_dist == 0)
                        {
                            // Update min info
                            min_dist = dist;
                            min_y_var_index = j;
                            min_x_value = x_values[i];
                            min_y_value = y_value;
                            min_pt = pt;
                        }
                    }
                }
            }
            // Check if min point is identified
            if (min_dist != 0)
            {
                // Draw big circle around it
                g.DrawEllipse(Pens.Black, plotWindow.X + min_pt.X - 5, plotWindow.Y + min_pt.Y - 5, 10, 10);
                // Display values
                str = legendLabels[min_y_var_index] + ": x = " + min_x_value + ", y = " + min_y_value;
                g.DrawString(str,this.Font, Brushes.Black, new Point(pendingPoint.X, pendingPoint.Y));
            }
        }

        public string GetLegendLabelName(int index)
        {
            return legendLabels[index];
        }

        public string[] GetLegendLabelNames()
        {
            string[] names;
            int i;

            // Instantiate
            names = new string[legendLabels.Count];
            // For each name
            for (i = 0; i < legendLabels.Count; i++)
            {
                // Get name
                names[i] = legendLabels[i];
            }
            return names;
        }

        private void PlotItem_MouseMove(object sender, MouseEventArgs e)
        {         
            Point p = new Point(e.X, e.Y);
            Rectangle spot_NWSE;
            Rectangle spot_NS;
            Rectangle spot_WE;
            Rectangle r;
            Point zoom_start_pt;
            Size s;
            float ratio;
            float offset;

            if (State == States.Design)
            {
                if (DesignState == DesignStates.Normal || DesignState == DesignStates.ResizeNWSEHover ||
                    DesignState == DesignStates.ResizeNSHover || DesignState == DesignStates.ResizeWEHover)
                {
                    spot_NWSE = new Rectangle(this.Width - 10, this.Height - 10, 10, 10);
                    spot_NS = new Rectangle(0, this.Height - 10, this.Width - 10, 10);
                    spot_WE = new Rectangle(this.Width - 10, 0, 10, this.Height - 10);

                    if (spot_NWSE.Contains(p))
                    {
                        this.Cursor = Cursors.SizeNWSE;
                        DesignState = DesignStates.ResizeNWSEHover;
                    }
                    else if (spot_NS.Contains(p))
                    {
                        this.Cursor = Cursors.SizeNS;
                        DesignState = DesignStates.ResizeNSHover;
                    }
                    else if (spot_WE.Contains(p))
                    {
                        this.Cursor = Cursors.SizeWE;
                        DesignState = DesignStates.ResizeWEHover;
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;
                        DesignState = DesignStates.Normal;
                    }
                }
                else if (DesignState == DesignStates.Move)
                {
                    if (PlotItemMouseMove != null)
                    {
                        PlotItemMouseMove(this, e);
                    }
                }
                else if (DesignState == DesignStates.ResizeNWSE)
                {
                    this.Size = new Size(e.X, e.Y);
                    Console.WriteLine("ResizeNWSE\n");
                    memGraphics.CreateDoubleBuffer(this.CreateGraphics(), this.ClientRectangle.Width, this.ClientRectangle.Height);
                    Invalidate();
                }
                else if (DesignState == DesignStates.ResizeNS)
                {
                    this.Size = new Size(this.Size.Width, e.Y);
                    memGraphics.CreateDoubleBuffer(this.CreateGraphics(), this.ClientRectangle.Width, this.ClientRectangle.Height);
                    Invalidate();
                }
                else if (DesignState == DesignStates.ResizeWE)
                {
                    this.Size = new Size(e.X, this.Size.Height);
                    memGraphics.CreateDoubleBuffer(this.CreateGraphics(), this.ClientRectangle.Width, this.ClientRectangle.Height);
                    Invalidate();
                }
            }
            else if (State == States.Action)
            {
                // Make sure cursor is normal
                Cursor = Cursors.Arrow;
                // Check if plot ready or plot-held
                if (actionState == ActionStates.PlotReady || holdPlot == true)
                {
                    // Check if zoom-in is active
                    if (zoomState == ZoomStates.ZoomInActive)
                    {
                        // Create zoom rectangle
                        zoom_start_pt = new Point(Math.Min(p.X, pendingPoint.X), Math.Min(p.Y, pendingPoint.Y));
                        s = new Size(Math.Abs(p.X - pendingPoint.X), Math.Abs(p.Y - pendingPoint.Y));
                        zoomRect = new Rectangle(zoom_start_pt, s);
                        r = zoomRect;
                        r.Inflate(new Size(2, 2));
                        // Update display
                        Invalidate();
                    }
                    else if (zoomState == ZoomStates.PanActive)
                    {
                        // Calculate how much to move for x
                        ratio = (float)(p.X - pendingPoint.X) / (float)plotWindow.Width;
                        offset = (plotWindowMaxX - plotWindowMinX) * ratio;
                        plotWindowMinX -= offset;
                        plotWindowMaxX -= offset;
                        // Calculate how much to move for y
                        ratio = (float)(p.Y - pendingPoint.Y) / (float)plotWindow.Height;
                        offset = (plotWindowMaxY - plotWindowMinY) * ratio;
                        plotWindowMinY += offset;
                        plotWindowMaxY += offset;
                        // Update pending point
                        pendingPoint = p;
                        // Update display
                        Invalidate();
                    }
                }
            }
        }

        private void PlotItem_MouseUp(object sender, MouseEventArgs e)
        {
            float interp;
            float zoom_ratio;
            float tempMinX;
            float tempMaxX;
            float tempMinY;
            float tempMaxY;

            if (State == States.Design)
            {
                if (DesignState == DesignStates.Move)
                {
                    DesignState = DesignStates.Normal;
                    if (PlotItemMouseUp != null)
                    {
                        PlotItemMouseUp(this, e);
                    }
                }
                else if (DesignState == DesignStates.ResizeNWSE || DesignState == DesignStates.ResizeNS || DesignState == DesignStates.ResizeWE)
                {
                    DesignState = DesignStates.Normal;
                }
            }
            else if (State == States.Action)
            {
                if (actionState == ActionStates.PlotReady || holdPlot == true)
                {
                    // Check if zoom in active
                    if (zoomState == ZoomStates.ZoomInActive)
                    {
                        // Set to ZoomInReady state
                        zoomState = ZoomStates.ZoomInReady;
                        // Calculate zoom in points (x and y)
                        interp = (float)(Math.Min(e.X, pendingPoint.X) - plotWindow.Location.X) / (float)plotWindow.Width;
                        tempMinX = plotWindowMinX + interp * (plotWindowMaxX - plotWindowMinX);
                        interp = (float)(Math.Max(e.X, pendingPoint.X) - plotWindow.Location.X) / (float)plotWindow.Width;
                        tempMaxX = plotWindowMinX + interp * (plotWindowMaxX - plotWindowMinX);
                        interp = (float)(Math.Max(e.Y, pendingPoint.Y) - plotWindow.Location.Y) / (float)plotWindow.Height;
                        tempMinY = plotWindowMaxY - interp * (plotWindowMaxY - plotWindowMinY);
                        interp = (float)(Math.Min(e.Y, pendingPoint.Y) - plotWindow.Location.Y) / (float)plotWindow.Height;
                        tempMaxY = plotWindowMaxY - interp * (plotWindowMaxY - plotWindowMinY);
                        // Calculate zoom ratio for x
                        zoom_ratio = xPlotDataSet.ZoomRatio(tempMinX, tempMaxX);
                        // Make sure that zoom is valid
                        if (zoom_ratio > 0)
                        {
                            plotWindowMinX = tempMinX;
                            plotWindowMaxX = tempMaxX;
                        }
                        // Calculate zoom ratio for x
                        zoom_ratio = yPlotDataSets.ZoomRatio(tempMinY, tempMaxY);
                        // Make sure that zoom is valid
                        if (zoom_ratio > 0)
                        {
                            plotWindowMinY = tempMinY;
                            plotWindowMaxY = tempMaxY;
                        }
                        // Update display
                        Invalidate();                        
                    }
                    else if (zoomState == ZoomStates.PanActive)
                    {
                        zoomState = ZoomStates.PanReady;
                    }
                }
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e)
        {
            if (PlotItemDelete != null)
            {
                PlotItemDelete(this, e);
            }
        }

        private void mnuZoomIn_Click(object sender, EventArgs e)
        {
            // Set to zoom-in state
            zoomState = ZoomStates.ZoomInReady;
            // Update display
            Invalidate();
        }

        private void mnuZoomOut_Click(object sender, EventArgs e)
        {
            // Set to zoom-out state
            zoomState = ZoomStates.ZoomOut;
            // Update display
            Invalidate();
        }

        private void mnuPan_Click(object sender, EventArgs e)
        {
            // Set to pan state
            zoomState = ZoomStates.PanReady;
            // Update display
            Invalidate();
        }

        private void mnuPinpoint_Click(object sender, EventArgs e)
        {
            // Set to pinpoint state
            zoomState = ZoomStates.Pinpoint;
            // Set pendingpoint empty
            pendingPoint = new Point();
            // Update display
            Invalidate();
        }

        private void mnuGrid_Click(object sender, EventArgs e)
        {
            // Check if grid is on
            if (drawGrid == true)
            {
                // Turn off grid
                drawGrid = false;
            }
            else
            {
                drawGrid = true;
            }
            // Update display
            Invalidate();
        }

        private void mnuSymbol_Click(object sender, EventArgs e)
        {
            // Check if symbol is on
            if (drawSymbol == true)
            {
                // Turn off symbol
                drawSymbol = false;
            }
            else
            {
                // Turn on symbol
                drawSymbol = true;
            }
            // Update display
            Invalidate();
        }

        private void mnuHold_Click(object sender, EventArgs e)
        {
            // Check if plot is held
            if (holdPlot == true)
            {
                // Release hold
                holdPlot = false;
            }
            else
            {
                // Turn on hold
                holdPlot = true;
            }
            // Update display
            Invalidate();
        }

        private void mnuDeleteYVar_Click(object sender, EventArgs e)
        {
            string y_var_name;            

            // Get y var name
            y_var_name = ((MenuItem)sender).Name;
            // Delete y var
            UnAssignYVar(y_var_name);
            // Update display
            Invalidate();
        }

        public void UnAssignYVar(string var_name)
        {
            int index;

            // Delete y var legend
            index = legendLabels.IndexOf(var_name);
            legendLabels.Remove(legendLabels[index]);
            LegendPens.Remove(LegendPens[index]);
            // Remove y var menu
            ContextMenu.MenuItems.Remove(ContextMenu.MenuItems[var_name]);
        }

        private void PlotItem_DoubleClick(object sender, EventArgs e)
        {
            // Check if action state
            if (state == States.Action)
            {
                // Check if plot running or held
                if (actionState == ActionStates.PlotReady || holdPlot == true)
                {
                    // Zoom out all
                    ZoomOutAll();
                }
            }
        }
    }
}