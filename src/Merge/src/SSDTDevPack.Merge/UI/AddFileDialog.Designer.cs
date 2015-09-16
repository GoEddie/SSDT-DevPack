namespace SSDTDevPack.Merge.UI
{
    partial class AddFileDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableListDropDown = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tableListDropDown
            // 
            this.tableListDropDown.FormattingEnabled = true;
            this.tableListDropDown.Location = new System.Drawing.Point(101, 32);
            this.tableListDropDown.Name = "tableListDropDown";
            this.tableListDropDown.Size = new System.Drawing.Size(339, 21);
            this.tableListDropDown.TabIndex = 0;
            this.tableListDropDown.SelectedIndexChanged += new System.EventHandler(this.tableListDropDown_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(447, 32);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "&Add";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AddFileDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 96);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tableListDropDown);
            this.Name = "AddFileDialog";
            this.Text = "AddFileDialog";
            this.Load += new System.EventHandler(this.AddFileDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox tableListDropDown;
        private System.Windows.Forms.Button button1;
    }
}