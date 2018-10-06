using System;
using System.Collections.Generic;
using System.Text;

namespace PlotItemSpace
{  
    public class PlotDataSet
    {
        private PlotData start;
        private PlotData end;
        private PlotData now;
        private float min;
        private float max;
        private int count;
        private string varName;
        private VariableType varType;

        public PlotDataSet(string varName, VariableType varType)
        {
            this.varName = varName;
            this.varType = varType;
        }

        public void Add(float new_data)
        {
            PlotData tmp_data = new PlotData();

	        tmp_data.data = new_data;
            if (start == null)
            {
                start = tmp_data;
                min = new_data;
                max = new_data;
                now = tmp_data;
            }
            else
            {
                end.next = tmp_data;
                if (new_data < min) 
                {
                    min = new_data;
                }
                if (max < new_data) 
                {
                    max = new_data;
                }
            }
            end = tmp_data;
            count++;
        }

        public float Get()
        {
            PlotData dummy;

            dummy = now;
            now = now.next;
            return dummy.data;
        }

        public void GetReset()
        {
            if (Count == 0)
            {
                now = null;
            }
            else
            {
                now = start;
            }
        }

        public string VarName
        {
            get
            {
                return varName;
            }
            set
            {
                varName = value;
            }
        }

        public VariableType VarType
        {
            get
            {
                return varType;
            }
        }

        public void Empty()
        {
            start = null;
            end = null;
            count = 0;
        }

        public float Min
        {
            get
            {
                return min;
            }
            set
            {
                min = value;
            }
        }

        public float Max
        {
            get
            {
                return max;
            }
            set
            {
                max = value;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
            }
        }

        public static PlotDataSet GetTickIntervals(float min, float max)
        {
            float tick_size_ref;
            float tick_size = 1;
            float tick_value_now;
            float temp;
            int temp_i;
            PlotDataSet tick_values = new PlotDataSet("tick values", VariableType.Float);

            // Get range            
            tick_size_ref = (max - min) / 5;
            // Determin tick size
            if (tick_size_ref >= tick_size) 
            {
                while (tick_size_ref >= tick_size)
                {
                    tick_size = tick_size * 10;
                }
                tick_size = tick_size / 10;
            }
            else 
            {
                while (tick_size_ref < tick_size)
                {
                    tick_size = tick_size / 10;
                }
            }
            if (tick_size * 5 <= tick_size_ref) 
            {
                tick_size = tick_size * 5f;
            }
            else if (tick_size * 2 <= tick_size_ref) 
            {
                tick_size = tick_size * 2f;
            }      
            // Determin min tick value
            temp_i = (int)(min / tick_size) - 1;
            tick_value_now = tick_size * (float)temp_i;
            while (tick_value_now < min)
            {
                temp = tick_value_now;
                tick_value_now += tick_size;
                if (tick_value_now == temp)
                {
                    // Reached limit of float
                    return null;
                }
            }
            // Get tick intervals
            while (tick_value_now < max)
            {
                tick_values.Add(tick_value_now);
                temp = tick_value_now;
                tick_value_now += tick_size;
                if (tick_value_now == temp)
                {
                    // Reached limit of float
                    return null;
                }
            }
            return tick_values;
        }

        public float ZoomRatio(float min_in, float max_in)
        {
            float ratio;

            // Check if only one value
            if (max == min)
            {
                // Return -1 (zoom invalid)
                return -1;
            }
            // Calculate zoom ratio
            ratio = (max_in - min_in) / (max - min);
            return ratio;
        }
    }    

    public class PlotData
    {
        public float data;
        public PlotData next;
    }

    public enum VariableType
    {
        Float,
        Int32,
        Unknown
    }
}
