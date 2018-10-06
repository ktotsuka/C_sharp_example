using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using NonPlotItemSpace;
using PlotItemSpace;

namespace MainApplication
{
    public sealed class SubscriptionTableManager
    {
        private static readonly SubscriptionTableManager instance = new SubscriptionTableManager();
        private VariableInfo[] nonPlotDataSubscriptionTable;
        private PlotDataSets[] plotDataSubscriptionTables;
        private float[] iniX;
        private List<string> orderedXVar;
        private List<string>[] orderedYVar;

        public static SubscriptionTableManager Instance
        {
            get
            {
                return instance;
            }
        }

        public int GetNonPlotSubTableLength()
        {
            return nonPlotDataSubscriptionTable.Length;
        }

        public int GetPlotSubTableLength()
        {
            // Check if it is null
            if (plotDataSubscriptionTables == null)
            {
                return 0;
            }
            // Table exist, so return the length
            return plotDataSubscriptionTables.Length;
        }

        public int GetPlotSubTableLength(int capture_period_num)
        {
            return plotDataSubscriptionTables[capture_period_num].Count;
        }

        public int GetPlotDataNumPoints(int capture_period_num)
        {
            // Get number of points
            return plotDataSubscriptionTables[capture_period_num][0].Count;
        }

        public void SavePlotData(int capture_period_num, StreamWriter sw)
        {
            int i;
            int j;

            // For each variable
            for (i = 0; i < GetPlotSubTableLength(capture_period_num); i++)
            {
                // Get start position for each variable
                plotDataSubscriptionTables[capture_period_num][i].GetReset();
                // Write name to stream
                sw.Write(plotDataSubscriptionTables[capture_period_num][i].VarName);
                sw.Write(",");
            }
            // New line
            sw.Write("\r\n");
            
            // For each data point
            for (j = 0; j < GetPlotDataNumPoints(capture_period_num); j++)
            {
                // For each variable
                for (i = 0; i < GetPlotSubTableLength(capture_period_num); i++)
                {
                    // Write data to stream
                    sw.Write(plotDataSubscriptionTables[capture_period_num][i].Get());
                    // Comma
                    sw.Write(",");
                }
                // New line
                sw.Write("\r\n");
            }
        }

        public void EmptyPlotData(int capture_period_num)
        {
            int i;

            // For each y variables
            for (i = 0; i < plotDataSubscriptionTables[capture_period_num].Count; i++)
            {
                // Empty data
                plotDataSubscriptionTables[capture_period_num][i].Empty();
            }
        }

        public void GetPlotDataAddresses(out List<uint>[] plot_data_addresses)
        {
            int i;
            int j;
            string var_name;
            VariableInfo vi;
            uint var_address;

            // Instantiate
            plot_data_addresses = new List<uint>[plotDataSubscriptionTables.Length];
            // For each capture capture time
            for (i = 0; i < plot_data_addresses.Length; i++)
            {
                // Instantiate
                plot_data_addresses[i] = new List<uint>();
                // For each variable
                for (j = 0; j < plotDataSubscriptionTables[i].Count; j++)
                {
                    // Get variable name
                    var_name = plotDataSubscriptionTables[i][j].VarName;
                    // Get address
                    vi = VariableManager.Instance.GetVariable(var_name);
                    var_address = vi.address;
                    // Insert address
                    plot_data_addresses[i].Add(var_address);
                }
            }
        }

        public void CreateNonPlotDataSubscriptionTable()
        {
            int i;
            VariableInfo vi;

            // Exit if no non plot item exist
            if (NonPlotItems.Instance.Count == 0)
            {
                return;
            }
            // Create space for table
            nonPlotDataSubscriptionTable = new VariableInfo[NonPlotItems.Instance.Count];
            // For nonPlotItems
            for (i = 0; i < NonPlotItems.Instance.Count; i++)
            {
                // Instatiate data for that variable
                vi = VariableManager.Instance.GetVariable(NonPlotItems.Instance[i].VariableName);
                // Make entry for table
                nonPlotDataSubscriptionTable[i] = vi;
            }
        }

        public void CreatePlotDataSubscriptionTables()
        {
            int i, j;
            PlotDataSet new_data_set;
            VariableInfo vi;

            // Exit if no plot item exist
            if (PlotItems.Instance.Count == 0)
            {
                return;
            }
            // Get ordered x variable names
            CaptureSettingManager.Instance.GetOrderedXVarNames(out orderedXVar);
            // Get ordered y variable names
            PlotItems.Instance.GetYVarNames(orderedXVar, out orderedYVar);
            // Instantiate tables
            plotDataSubscriptionTables = new PlotDataSets[orderedXVar.Count];
            // For each capture period setting
            for (i = 0; i < orderedXVar.Count; i++)
            {
                // Instantiate a table
                plotDataSubscriptionTables[i] = new PlotDataSets();
                // Create x variable data set
                vi = VariableManager.Instance.GetVariable(orderedXVar[i]);
                new_data_set = new PlotDataSet(orderedXVar[i], vi.type);
                // Add x variable
                plotDataSubscriptionTables[i].Add(new_data_set);
                // Add y variable
                for (j = 0; j < orderedYVar[i].Count; j++)
                {
                    // Create y variable data set
                    vi = VariableManager.Instance.GetVariable(orderedYVar[i][j]);
                    new_data_set = new PlotDataSet(orderedYVar[i][j], vi.type);
                    plotDataSubscriptionTables[i].Add(new_data_set);
                }
            }
        }

        public void GetPlotDataType(int capture_period_num, out VariableType[] var_types)
        {
            int i;

            // Instantiate
            var_types = new VariableType[plotDataSubscriptionTables[capture_period_num].Count];
            // For each variable
            for (i = 0; i < var_types.Length; i++)
            {
                // Get variable types
                var_types[i] = plotDataSubscriptionTables[capture_period_num][i].VarType;
            }
        }

        public void UpdatePlotDataSubTable(int capture_period_num, float[] data)
        {
            int i;

            // Check if plot is running
            if (CaptureSettingManager.Instance.IsCaptureActive(capture_period_num))
            {
                // Check if number of data is within limit
                if (plotDataSubscriptionTables[capture_period_num][0].Count <= CaptureSettingManager.Instance.GetCaptureLimit(capture_period_num))
                {
                    // Update each variables
                    for (i = 0; i < plotDataSubscriptionTables[capture_period_num].Count; i++)
                    {
                        // Check if x variable
                        if (i == 0)
                        {

                            // Check if 1st value
                            if (plotDataSubscriptionTables[capture_period_num][0].Count == 0)
                            {
                                // Record first value
                                iniX[capture_period_num] = data[0];                                
                            }
                            // Check if x value starts at zero
                            if (CaptureSettingManager.Instance.GetStartAtZero(capture_period_num) == true)
                            {
                                // Give offset to make x zero
                                data[0] -= iniX[capture_period_num];
                            }
                        }
                        // Add value to table
                        plotDataSubscriptionTables[capture_period_num][i].Add(data[i]);
                    }
                }
            }
        }

        public void AssignPlotVariables()
        {
            int i; // Iterator for plots
            int j; // Iterator for y variables
            int x_pos;
            int y_pos;

            // Exit if no plot item exist
            if (PlotItems.Instance.Count == 0)
            {
                return;
            }
            // For each plot
            for (i = 0; i < PlotItems.Instance.Count; i++)
            {
                // Clear y variables
                PlotItems.Instance[i].DeleteAllDataY();
                // Find out which x variable is used
                x_pos = orderedXVar.IndexOf(PlotItems.Instance[i].XLabel);
                // Assign x variable
                PlotItems.Instance[i].AssignPlotDataX(plotDataSubscriptionTables[x_pos][0]);
                // For each y variables
                for (j = 0; j < PlotItems.Instance[i].CountLegendLabel(); j++)
                {
                    // Find out which y variable is used
                    y_pos = orderedYVar[x_pos].IndexOf(PlotItems.Instance[i].GetLegendLabelName(j));
                    // Assign y variable
                    PlotItems.Instance[i].AddPlotDataY(plotDataSubscriptionTables[x_pos][y_pos + 1]);
                }
            }
        }

        private void GetOrderedXYVariable()
        {
            int i,j;

            // Instantiate
            orderedXVar = new List<string>();
            orderedYVar = new List<string>[plotDataSubscriptionTables.Length];
            // For each capture period
            for (i = 0; i < plotDataSubscriptionTables.Length; i++)
            {
                // Add X variable
                orderedXVar.Add(plotDataSubscriptionTables[i][0].VarName);
                // Instantiate
                orderedYVar[i] = new List<string>();
                // For each y variable
                for (j = 1; j < plotDataSubscriptionTables[i].Count; j++)
                {
                    // Add Y variable
                    orderedYVar[i].Add(plotDataSubscriptionTables[i][j].VarName);
                }
            }
        }

        public VariableInfo LookUpnonPlotDataSubscriptionTable(int num)
        {
            return nonPlotDataSubscriptionTable[num];
        }

        public void GetReadyForAction()
        {
            // Create nonplot variable subscription table
            SubscriptionTableManager.Instance.CreateNonPlotDataSubscriptionTable();
            // Create plot data subscription table
            SubscriptionTableManager.Instance.CreatePlotDataSubscriptionTables();
            // Assign plot variables
            SubscriptionTableManager.Instance.AssignPlotVariables();
            // Instantiate Xini
            iniX = new float[GetPlotSubTableLength()];
        }
    }
}
