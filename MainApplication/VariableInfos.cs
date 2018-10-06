using System;
using System.Collections.Generic;
using System.Text;
using PlotItemSpace;
using NonPlotItemSpace;

namespace MainApplication
{
    public class VariableInfos : SortedDictionary<string, VariableInfo>
    {
    }

    public class VariableInfo
    {
        public VariableType type;
        public uint address;
        public NonPlotData data;
    }
}
