
namespace UoM_Server
{
    partial class ServerGUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.rbOn = new System.Windows.Forms.RadioButton();
            this.rbOFF = new System.Windows.Forms.RadioButton();
            this.lbClients = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server Status";
            // 
            // rbOn
            // 
            this.rbOn.AutoSize = true;
            this.rbOn.Location = new System.Drawing.Point(183, 17);
            this.rbOn.Name = "rbOn";
            this.rbOn.Size = new System.Drawing.Size(41, 17);
            this.rbOn.TabIndex = 1;
            this.rbOn.TabStop = true;
            this.rbOn.Text = "ON";
            this.rbOn.UseVisualStyleBackColor = true;
            this.rbOn.CheckedChanged += new System.EventHandler(this.rbOn_CheckedChanged);
            // 
            // rbOFF
            // 
            this.rbOFF.AutoSize = true;
            this.rbOFF.Location = new System.Drawing.Point(295, 17);
            this.rbOFF.Name = "rbOFF";
            this.rbOFF.Size = new System.Drawing.Size(45, 17);
            this.rbOFF.TabIndex = 2;
            this.rbOFF.TabStop = true;
            this.rbOFF.Text = "OFF";
            this.rbOFF.UseVisualStyleBackColor = true;
            this.rbOFF.CheckedChanged += new System.EventHandler(this.rbOFF_CheckedChanged);
            // 
            // lbClients
            // 
            this.lbClients.FormattingEnabled = true;
            this.lbClients.Location = new System.Drawing.Point(18, 74);
            this.lbClients.Name = "lbClients";
            this.lbClients.Size = new System.Drawing.Size(322, 147);
            this.lbClients.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Client List";
            // 
            // ServerGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 450);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbClients);
            this.Controls.Add(this.rbOFF);
            this.Controls.Add(this.rbOn);
            this.Controls.Add(this.label1);
            this.Name = "ServerGUI";
            this.Text = "Server GUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rbOn;
        private System.Windows.Forms.RadioButton rbOFF;
        private System.Windows.Forms.ListBox lbClients;
        private System.Windows.Forms.Label label2;
    }
}

