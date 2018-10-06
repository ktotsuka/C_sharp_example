using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace NonPlotItemSpace
{
    public partial class NonPlotItem : UserControl
    {
        private string variableName = "drop variable here";
        private bool waitState = false;
        private NonPlotData data;
        private NonPlotItemType type;
        private States state;
        private DesignStates designState;
        public delegate void NonPlotItemValueChangedEventHandler(object sender, byte[] data_in_bytes);  
        public event NonPlotItemValueChangedEventHandler NonPlotItemValueChanged;
        public delegate void NonPlotItemDeleteEventHandler(object sender, EventArgs e);
        public delegate void NonPlotItemMouseDownEventHandler(object sender, EventArgs e);
        public delegate void NonPlotItemMouseMoveEventHandler(object sender, EventArgs e);
        public delegate void NonPlotItemMouseUpEventHandler(object sender, EventArgs e);
        public event NonPlotItemMouseDownEventHandler NonPlotItemMouseDown;
        public event NonPlotItemMouseMoveEventHandler NonPlotItemMouseMove;
        public event NonPlotItemMouseUpEventHandler NonPlotItemMouseUp; 
        public event NonPlotItemDeleteEventHandler NonPlotItemDelete;

        public NonPlotItem()
        {

        }

        public NonPlotItem(Point location, NonPlotItemType new_type)
        {
            ContextMenu mnu_context = new ContextMenu();
            MenuItem mnu_delete = new MenuItem();

            // Instantiate 
            InitializeComponent();
            Location = location;
            designState = DesignStates.Normal;
            // Set type
            type = new_type;
            // Handle appearance
            if (type == NonPlotItemType.BooleanItem)
            {
                // Show check box
                checkBoxBool.Visible = true;
                checkBoxBool.Location = new Point(0, 0);
            }
            else
            {
                // Show text box
                labelVarName.Visible = true;
                textBoxLongFloat.Visible = true;
                labelVarName.Location = new Point(0, 0);
                textBoxLongFloat.Location = new Point(0, labelVarName.Height);
            }
            // Create menus for context menue
            mnu_delete.Index = 0;
            mnu_delete.Text = "Delete";
            mnu_delete.Click += new System.EventHandler(this.mnuDelete_Click);
            // Add context menu
            mnu_context.MenuItems.Add(mnu_delete);
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
            Move
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

        private void mnuDelete_Click(object sender, EventArgs e)
        {
            if (NonPlotItemDelete != null)
            {
                NonPlotItemDelete(this, e);
            }

        }

        public bool IsAssigned()
        {
            // Check if variable is assigned
            if (VariableName != "drop variable here")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Assign(string name, NonPlotData new_data)
        {
            variableName = name;
            data = new_data;
        }

        public void UnAssign()
        {
            variableName = "drop variable here";
            data = null;
        }

        public void UpdateDisplay()
        {
            if (type == NonPlotItemType.BooleanItem)
            {
                checkBoxBool.Checked = Convert.ToBoolean(data.data);
            }
            else if (type == NonPlotItemType.FloatItem)
            {
                // Update if not on focus
                if (textBoxLongFloat.Focused == false)
                {
                    textBoxLongFloat.Text = Convert.ToString(data.data);
                }
            }
            else if (type == NonPlotItemType.LongItem)
            {
                // Update if not on focus
                if (textBoxLongFloat.Focused == false)
                {
                    textBoxLongFloat.Text = Convert.ToString(data.data);
                }
            }
            Update();
        }

        private void NonPlotItem_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int larger_width;

            // Set to appropriate size
            if (type == NonPlotItemType.BooleanItem)
            {
                // Set to appropriate size
                this.Size = checkBoxBool.Size;
                // Display name
                checkBoxBool.Text = variableName;
                // Indicate if variable is assigned
                if (IsAssigned() == true)
                {
                    this.BackColor = Color.Transparent;
                }
                else
                {
                    this.BackColor = Color.Red;
                }
            }
            else
            {                
                // Display name
                labelVarName.Text = variableName;
                // Set to appropriate size
                labelVarName.Size = (g.MeasureString(labelVarName.Text, this.Font)).ToSize();
                larger_width = Math.Max(labelVarName.Width, textBoxLongFloat.Width);
                this.Size = new Size(larger_width, labelVarName.Height + textBoxLongFloat.Height);
                // Indicate if variable is assigned
                if (IsAssigned() == true)
                {
                    textBoxLongFloat.BackColor = Color.White;
                }
                else
                {
                    textBoxLongFloat.BackColor = Color.Red;
                }
            }
            // Check if design state
            if (state == States.Design)
            {
                // Disable controls
                checkBoxBool.Enabled = false;
                textBoxLongFloat.Enabled = false;
                labelVarName.Enabled = false;
            }
            else
            {
                // Enable controls
                checkBoxBool.Enabled = true;
                textBoxLongFloat.Enabled = true;
                labelVarName.Enabled = true;
            }
        }

        public CheckBox GetCheckBox
        {
            get
            {
                return checkBoxBool;
            }
        }

        public TextBox GetTextBox
        {
            get
            {
                return textBoxLongFloat;
            }
        }

        public bool WaitState
        {
            get
            {
                return waitState;
            }
            set
            {
                waitState = value;
            }
        }

        public string VariableName
        {
            get
            {
                return variableName;
            }
        }

        public NonPlotItemType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        public float Data
        {
            get
            {
                return data.data;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if enter is pressed
            
            if (e.KeyCode == Keys.Enter)
            {
                textBox1_Leave(sender, new EventArgs());
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            int int_number;
            float float_number;
            byte[] data_in_byte = new byte[4];

            Console.WriteLine("leave");
            if (type == NonPlotItemType.FloatItem)
            {
                // Get number that is typed
                try
                {
                    float_number = Convert.ToSingle(textBoxLongFloat.Text);
                }
                catch
                {
                    // Invalid entry
                    MessageBox.Show("Invalid entry");
                    return;
                }
                // Check if value has changed
                if (float_number != data.data)
                {
                    // Get four bytes for float value
                    data_in_byte = BitConverter.GetBytes(float_number);
                    NonPlotItemValueChanged(this, data_in_byte);
                }
            }
            else if (type == NonPlotItemType.LongItem)
            {
                // Get number that is typed
                try
                {
                    int_number = Convert.ToInt32(textBoxLongFloat.Text);
                }
                catch
                {
                    // Invalid entry
                    MessageBox.Show("Invalid entry");
                    return;
                }
                // Check if value has changed
                if (int_number != (int)data.data)
                {
                    //Console.WriteLine("hey");
                    // Get four bytes for float value
                    data_in_byte = BitConverter.GetBytes(int_number);
                    NonPlotItemValueChanged(this, data_in_byte);
                }
            }
        }

        private void checkBoxName_Click(object sender, EventArgs e)
        {
            byte[] data_in_byte = new byte[4];

            // Check if it is checked or unchecked
            if (checkBoxBool.Checked == true)
            {
                data_in_byte[0] = 0x01;
            }
            else
            {
                data_in_byte[0] = 0x00;
            }
            // Send data
            data_in_byte[1] = 0x00;
            data_in_byte[2] = 0x00;
            data_in_byte[3] = 0x00;
            NonPlotItemValueChanged(this, data_in_byte);
        }

        private void checkBoxName_MouseDown(object sender, MouseEventArgs e)
        {
            // Forward to parent
            NonPlotItem_MouseDown(sender, e);
        }

        private void NonPlotItem_MouseDown(object sender, MouseEventArgs e)
        {
            // Check if left click
            if (e.Button == MouseButtons.Left)
            {
                // Check if design state
                if (state == States.Design)
                {
                    // Set to move state
                    designState = DesignStates.Move;
                    // Send event
                    NonPlotItemMouseDown(this, e);
                }
            }
            else
            {
                Console.WriteLine("NonPlotItem_MouseDown - Right");
                // Check if design state
                if (state == States.Design)
                {
                    // Enable "Delete"
                    ContextMenu.MenuItems[Constants.CONTEXT_MENU_DELETE].Enabled = true;
                }
                else
                {
                    // Disalbe "Delete"
                    ContextMenu.MenuItems[Constants.CONTEXT_MENU_DELETE].Enabled = false;
                }
            }
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Forward to parent
            NonPlotItem_MouseDown(sender, e);
        }

        private void NonPlotItem_MouseMove(object sender, MouseEventArgs e)
        {
            // Check if move state
            if (designState == DesignStates.Move)
            {
                // Send event
                NonPlotItemMouseMove(this, e);
            }
        }

        private void NonPlotItem_MouseUp(object sender, MouseEventArgs e)
        {
            // Check if design  state
            if (state == States.Design && designState == DesignStates.Move)
            {
                // Check if move state
                if (designState == DesignStates.Move)
                {
                    // Set to normal state
                    designState = DesignStates.Normal;
                    // Send event
                    NonPlotItemMouseUp(this, e);
                }
            }
        }
    }

    public class NonPlotData
    {
        public float data;
    }

    public enum NonPlotItemType
    {
        BooleanItem,
        LongItem,
        FloatItem
    }
}