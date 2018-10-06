using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PlotItemSpace
{
    public sealed class PlotItems : CollectionBase
    {
        private static readonly PlotItems instance = new PlotItems();
     
        public static PlotItems Instance
        {
            get
            {
                return instance;
            }
        }

        public void Add(PlotItem newPlotItem)
        {
            List.Add(newPlotItem);
        }

        public void Remove(PlotItem newPlotItem)
        {
            List.Remove(newPlotItem);
        }

        public PlotItems()
        {
        }

        public void StartPlots(string x_var_name)
        {
            int i;

            // For each plot
            for (i = 0; i < Count; i++)
            {
                // Check x variable name
                if (((PlotItem)List[i]).XLabel == x_var_name)
                {
                    // Name matches
                    // Start plot
                    ((PlotItem)List[i]).ActionState = PlotItem.ActionStates.PlotRunning;
                }
            }
        }

        public void GetReadyForDesign()
        {
            int i;

            for (i = 0; i < Count; i++)
            {
                this[i].State = PlotItem.States.Design;
                this[i].Invalidate();
            }
        }

        public void StopPlots(string x_var_name)
        {
            int i;

            // For each plot
            for (i = 0; i < Count; i++)
            {
                // Check x variable name
                if (((PlotItem)List[i]).XLabel == x_var_name)
                {
                    // Name matches
                    // Stop plot
                    ((PlotItem)List[i]).ActionState = PlotItem.ActionStates.PlotReady;
                }
            }
        }

        public bool AllAssigned()
        {
            int i;

            // For each item
            for (i = 0; i < Count; i++)
            {
                // Check if not assigned
                if (this[i].IsAssigned() != true)
                {
                    return false;
                }
            }
            // All assigned
            return true;
        }

        public PlotItem this[int plotItemIndex]
        {
            get
            {
                return (PlotItem)List[plotItemIndex];
            }
            set
            {
                List[plotItemIndex] = value;
            }
        }

        public void GetXVarNames(out List<string> x_var_names)
        {
            int i;
            string var_name;

            // Instantiate
            x_var_names = new List<string>();
            // For each plot
            for (i = 0; i < Count; i++)
            {
                // Get x variables
                var_name = ((PlotItem)List[i]).XLabel;
                // Make sure the name is new and not null
                if ((x_var_names.Contains(var_name) == false) && (var_name != null))
                {
                    // Add variable name
                    x_var_names.Add(var_name);
                }
            }            
        }

        public void GetYVarNames(List<string> x_var_names, out List<string>[] y_var_names)
        {
            int i, j;
            string var_name;
            int capture_period_num;

            // Instantiate array of list
            y_var_names = new List<string>[x_var_names.Count];
            // For each x variable name
            for (i = 0; i < x_var_names.Count; i++)
            {
                // Instantiate list
                y_var_names[i] = new List<string>();
            }
            // Get y variables
            // for each plot
            for (i = 0; i < Count; i++)
            {
                // Select x variable (capture period number)
                capture_period_num = x_var_names.IndexOf(this[i].XLabel);
                // For each y data set
                for (j = 0; j < ((PlotItem)List[i]).CountLegendLabel(); j++)
                {
                    // Get y variable name
                    var_name = ((PlotItem)List[i]).GetLegendLabelName(j);
                    // Make sure the name is new and not null
                    if ((y_var_names[capture_period_num].Contains(var_name) == false) && (var_name != null))
                    {
                        // Add to y_var names
                        y_var_names[capture_period_num].Add(var_name);
                    }
                }
            }
        }

        public void GetReadyForAction()
        {
            int i;

            // For each item
            for (i = 0; i < Count; i++)
            {
                // Set to action state
                this[i].State = PlotItem.States.Action;
            }
        }
    }
}
