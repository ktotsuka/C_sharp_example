using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace NonPlotItemSpace
{
    public sealed class NonPlotItems : CollectionBase
    {
        private static readonly NonPlotItems instance = new NonPlotItems();

        public static NonPlotItems Instance
        {
            get
            {
                return instance;
            }
        }

        public void GetReadyForDesign()
        {
            int i;

            for (i = 0; i < Count; i++)
            {
                this[i].State = NonPlotItem.States.Design;
                this[i].Invalidate();
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

        public void Add(NonPlotItem new_item)
        {
            List.Add(new_item);
        }

        public void Remove(NonPlotItem old_item)
        {
            List.Remove(old_item);
        }

        
        public NonPlotItem this[int index]
        {
            get
            {
                return (NonPlotItem)List[index];
            }
        }

        public void UpdateDisplay()
        {
            int i;

            for (i = 0; i < this.Count; i++)
            {
                // Update display
                ((NonPlotItem)List[i]).UpdateDisplay();
            }
        }

        public bool Contain(Point pt, out int index)
        {
            int i = 0;

            // Keep searching 
            for (i = 0; i < Count; i++)
            {
                // Check if it is inside bound
                if (((NonPlotItem)List[i]).Bounds.Contains(pt))
                {
                    // Found
                    index = i;
                    return true;
                }
            }
            index = 0;
            return false;
        }

        public void EndWaitState(string var_name)
        {
            NonPlotItem item;

            // Get non plot item
            item = GetItem(var_name);
            // End wait state
            item.WaitState = false;
        }

        private NonPlotItem GetItem(string var_name)
        {
            int i;

            // For each item
            for (i = 0; i < Count; i++)
            {
                // Check for match
                if (var_name == this[i].VariableName)
                {
                    // Return the item
                    return this[i];
                }
            }
            // Didn't find
            return null;
        }

        public void GetReadyForAction()
        {
            int i;

            // Enable Boolean items
            for (i = 0; i < Count; i++)
            {
                this[i].State = NonPlotItem.States.Action;
                this[i].Invalidate();
            }
        }
    }
}