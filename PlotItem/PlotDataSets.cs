using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace PlotItemSpace
{
    public class PlotDataSets : CollectionBase
    {
        public void Add(PlotDataSet newPlotDataSet)
        {
            List.Add(newPlotDataSet);
        }

        public void Remove(PlotDataSet newPlotDataSet)
        {
            List.Remove(newPlotDataSet);
        }

        public PlotDataSets()
        {
        }

        public PlotDataSet this[int plotDataSetIndex]
        {
            get
            {
                return (PlotDataSet)List[plotDataSetIndex];
            }
            set
            {
                List[plotDataSetIndex] = value;
            }
        }

        public PlotDataSet this[string varName]
        {
            get
            {
                int i;

                for (i = 0; i < Count; i++)
                {
                    if (this[i].VarName == varName)
                    {
                        return (PlotDataSet)List[i];
                    }
                }
                return null;
            }
        }

        public float Min
        {
            get
            {
                float min;
                int i;

                min = ((PlotDataSet)List[0]).Min;
                for (i = 1; i < Count; i++)
                {
                    if (((PlotDataSet)List[i]).Min < min)
                    {
                        min = ((PlotDataSet)List[i]).Min;
                    }
                }
                return min;
            }
        }

        public float Max
        {
            get
            {
                float max;
                int i;

                max = ((PlotDataSet)List[0]).Max;
                for (i = 1; i < Count; i++)
                {
                    if (max < ((PlotDataSet)List[i]).Max)
                    {
                        max = ((PlotDataSet)List[i]).Max;
                    }
                }
                return max;
            }
        }

        public float ZoomRatio(float min_in, float max_in)
        {
            float ratio;

            // Check if Max and Min is same
            if (Max == Min)
            {
                // Return -1 (zoom invalid)
                return -1;
            }
            // Calculate zoom ratio
            ratio = (max_in - min_in) / (Max - Min);
            return ratio;
        }
    }
}
