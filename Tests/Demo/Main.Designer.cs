namespace Tests.Demo
{
    partial class Main
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
            this.tc = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cbGPIO = new System.Windows.Forms.ComboBox();
            this.btnOnOff = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.lblTemperature = new System.Windows.Forms.Label();
            this.btnStartOW = new System.Windows.Forms.Button();
            this.lbSystem = new System.Windows.Forms.ListBox();
            this.tc.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tc
            // 
            this.tc.Controls.Add(this.tabPage1);
            this.tc.Controls.Add(this.tabPage2);
            this.tc.Controls.Add(this.tabPage3);
            this.tc.Location = new System.Drawing.Point(1, 117);
            this.tc.Name = "tc";
            this.tc.SelectedIndex = 0;
            this.tc.Size = new System.Drawing.Size(564, 262);
            this.tc.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cbGPIO);
            this.tabPage1.Controls.Add(this.btnOnOff);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(556, 236);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "GPIO";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // cbGPIO
            // 
            this.cbGPIO.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGPIO.FormattingEnabled = true;
            this.cbGPIO.Location = new System.Drawing.Point(7, 6);
            this.cbGPIO.Name = "cbGPIO";
            this.cbGPIO.Size = new System.Drawing.Size(140, 21);
            this.cbGPIO.TabIndex = 1;
            // 
            // btnOnOff
            // 
            this.btnOnOff.Location = new System.Drawing.Point(170, 4);
            this.btnOnOff.Name = "btnOnOff";
            this.btnOnOff.Size = new System.Drawing.Size(94, 23);
            this.btnOnOff.TabIndex = 0;
            this.btnOnOff.Text = "Toogle GPIO";
            this.btnOnOff.UseVisualStyleBackColor = true;
            this.btnOnOff.Click += new System.EventHandler(this.BtnOnOff_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(556, 236);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "I2C";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.lblTemperature);
            this.tabPage3.Controls.Add(this.btnStartOW);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(556, 236);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "OneWire";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // lblTemperature
            // 
            this.lblTemperature.AutoSize = true;
            this.lblTemperature.Location = new System.Drawing.Point(185, 24);
            this.lblTemperature.Name = "lblTemperature";
            this.lblTemperature.Size = new System.Drawing.Size(0, 13);
            this.lblTemperature.TabIndex = 1;
            // 
            // btnStartOW
            // 
            this.btnStartOW.Location = new System.Drawing.Point(33, 19);
            this.btnStartOW.Name = "btnStartOW";
            this.btnStartOW.Size = new System.Drawing.Size(75, 23);
            this.btnStartOW.TabIndex = 0;
            this.btnStartOW.Text = "Start";
            this.btnStartOW.UseVisualStyleBackColor = true;
            this.btnStartOW.Click += new System.EventHandler(this.BtnStartOW_Click);
            // 
            // lbSystem
            // 
            this.lbSystem.FormattingEnabled = true;
            this.lbSystem.Location = new System.Drawing.Point(5, 5);
            this.lbSystem.Name = "lbSystem";
            this.lbSystem.Size = new System.Drawing.Size(556, 108);
            this.lbSystem.TabIndex = 1;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 378);
            this.Controls.Add(this.lbSystem);
            this.Controls.Add(this.tc);
            this.Name = "Main";
            this.Text = "RaspberrySharp Demo";
            this.Load += new System.EventHandler(this.Main_Load);
            this.tc.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tc;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ComboBox cbGPIO;
        private System.Windows.Forms.Button btnOnOff;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnStartOW;
        private System.Windows.Forms.Label lblTemperature;
        private System.Windows.Forms.ListBox lbSystem;
    }
}

