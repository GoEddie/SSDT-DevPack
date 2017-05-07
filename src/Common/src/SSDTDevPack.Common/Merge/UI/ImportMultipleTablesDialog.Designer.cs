namespace SSDTDevPack.Merge.UI
{
    partial class ImportMultipleTablesDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportMultipleTablesDialog));
            this.connectionString = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buildButton = new System.Windows.Forms.Button();
            this.import = new System.Windows.Forms.Button();
            this.tableListDropDown = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // connectionString
            // 
            this.connectionString.Location = new System.Drawing.Point(145, 41);
            this.connectionString.Name = "connectionString";
            this.connectionString.Size = new System.Drawing.Size(450, 20);
            this.connectionString.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connection String:";
            // 
            // buildButton
            // 
            this.buildButton.Location = new System.Drawing.Point(602, 37);
            this.buildButton.Name = "buildButton";
            this.buildButton.Size = new System.Drawing.Size(75, 23);
            this.buildButton.TabIndex = 2;
            this.buildButton.Text = "&Build";
            this.buildButton.UseVisualStyleBackColor = true;
            this.buildButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // import
            // 
            this.import.Location = new System.Drawing.Point(602, 87);
            this.import.Name = "import";
            this.import.Size = new System.Drawing.Size(75, 23);
            this.import.TabIndex = 4;
            this.import.Text = "&Import";
            this.import.UseVisualStyleBackColor = true;
            this.import.Click += new System.EventHandler(this.import_Click);
            // 
            // tableListDropDown
            // 
            this.tableListDropDown.CheckOnClick = true;
            this.tableListDropDown.FormattingEnabled = true;
            this.tableListDropDown.Location = new System.Drawing.Point(145, 87);
            this.tableListDropDown.Name = "tableListDropDown";
            this.tableListDropDown.Size = new System.Drawing.Size(450, 244);
            this.tableListDropDown.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(53, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Tables to Import:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(145, 337);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Check All";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ImportMultipleTablesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 376);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tableListDropDown);
            this.Controls.Add(this.import);
            this.Controls.Add(this.buildButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.connectionString);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ImportMultipleTablesDialog";
            this.Text = "Import Multiple Tables";
            this.Load += new System.EventHandler(this.ImportSingleTable_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox connectionString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buildButton;
        private System.Windows.Forms.Button import;
        private System.Windows.Forms.CheckedListBox tableListDropDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
    }
}