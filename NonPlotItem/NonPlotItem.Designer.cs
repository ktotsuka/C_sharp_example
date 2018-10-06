namespace NonPlotItemSpace
{
    partial class NonPlotItem
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBoxBool = new System.Windows.Forms.CheckBox();
            this.textBoxLongFloat = new System.Windows.Forms.TextBox();
            this.labelVarName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // checkBoxBool
            // 
            this.checkBoxBool.AutoSize = true;
            this.checkBoxBool.Location = new System.Drawing.Point(0, 0);
            this.checkBoxBool.Name = "checkBoxBool";
            this.checkBoxBool.Size = new System.Drawing.Size(54, 17);
            this.checkBoxBool.TabIndex = 2;
            this.checkBoxBool.Text = "Name";
            this.checkBoxBool.UseVisualStyleBackColor = true;
            this.checkBoxBool.Visible = false;
            this.checkBoxBool.Click += new System.EventHandler(this.checkBoxName_Click);
            this.checkBoxBool.MouseDown += new System.Windows.Forms.MouseEventHandler(this.checkBoxName_MouseDown);
            // 
            // textBoxLongFloat
            // 
            this.textBoxLongFloat.Location = new System.Drawing.Point(3, 56);
            this.textBoxLongFloat.Name = "textBoxLongFloat";
            this.textBoxLongFloat.Size = new System.Drawing.Size(100, 20);
            this.textBoxLongFloat.TabIndex = 3;
            this.textBoxLongFloat.Visible = false;
            this.textBoxLongFloat.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            this.textBoxLongFloat.Leave += new System.EventHandler(this.textBox1_Leave);
            this.textBoxLongFloat.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBox1_MouseDown);
            // 
            // labelVarName
            // 
            this.labelVarName.AutoSize = true;
            this.labelVarName.Location = new System.Drawing.Point(3, 29);
            this.labelVarName.Name = "labelVarName";
            this.labelVarName.Size = new System.Drawing.Size(74, 13);
            this.labelVarName.TabIndex = 4;
            this.labelVarName.Text = "Variable name";
            this.labelVarName.Visible = false;
            // 
            // NonPlotItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelVarName);
            this.Controls.Add(this.textBoxLongFloat);
            this.Controls.Add(this.checkBoxBool);
            this.Name = "NonPlotItem";
            this.Size = new System.Drawing.Size(175, 137);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.NonPlotItem_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.NonPlotItem_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.NonPlotItem_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.NonPlotItem_MouseUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxBool;
        private System.Windows.Forms.TextBox textBoxLongFloat;
        private System.Windows.Forms.Label labelVarName;
    }
}
