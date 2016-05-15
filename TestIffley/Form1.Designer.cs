namespace ASCOM.Iffley
{
    partial class Form1
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
            this.buttonChoose = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelDriverId = new System.Windows.Forms.Label();
            this.buttonAction = new System.Windows.Forms.Button();
            this.Move0 = new System.Windows.Forms.Button();
            this.Move100 = new System.Windows.Forms.Button();
            this.Move200 = new System.Windows.Forms.Button();
            this.Position = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonChoose
            // 
            this.buttonChoose.Location = new System.Drawing.Point(309, 10);
            this.buttonChoose.Name = "buttonChoose";
            this.buttonChoose.Size = new System.Drawing.Size(72, 23);
            this.buttonChoose.TabIndex = 0;
            this.buttonChoose.Text = "Choose";
            this.buttonChoose.UseVisualStyleBackColor = true;
            this.buttonChoose.Click += new System.EventHandler(this.buttonChoose_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(309, 39);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(72, 23);
            this.buttonConnect.TabIndex = 1;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // labelDriverId
            // 
            this.labelDriverId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelDriverId.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::ASCOM.Iffley.Properties.Settings.Default, "DriverId", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.labelDriverId.Location = new System.Drawing.Point(12, 40);
            this.labelDriverId.Name = "labelDriverId";
            this.labelDriverId.Size = new System.Drawing.Size(291, 21);
            this.labelDriverId.TabIndex = 2;
            this.labelDriverId.Text = global::ASCOM.Iffley.Properties.Settings.Default.DriverId;
            this.labelDriverId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonAction
            // 
            this.buttonAction.Location = new System.Drawing.Point(306, 68);
            this.buttonAction.Name = "buttonAction";
            this.buttonAction.Size = new System.Drawing.Size(75, 23);
            this.buttonAction.TabIndex = 3;
            this.buttonAction.Text = "RTZ";
            this.buttonAction.UseVisualStyleBackColor = true;
            this.buttonAction.Click += new System.EventHandler(this.buttonAction_Click);
            // 
            // Move0
            // 
            this.Move0.Location = new System.Drawing.Point(13, 122);
            this.Move0.Name = "Move0";
            this.Move0.Size = new System.Drawing.Size(75, 23);
            this.Move0.TabIndex = 4;
            this.Move0.Text = "GoTo 0";
            this.Move0.UseVisualStyleBackColor = true;
            this.Move0.Click += new System.EventHandler(this.Move0_Click);
            // 
            // Move100
            // 
            this.Move100.Location = new System.Drawing.Point(115, 121);
            this.Move100.Name = "Move100";
            this.Move100.Size = new System.Drawing.Size(75, 23);
            this.Move100.TabIndex = 5;
            this.Move100.Text = "GoTo 100";
            this.Move100.UseVisualStyleBackColor = true;
            this.Move100.Click += new System.EventHandler(this.Move100_Click);
            // 
            // Move200
            // 
            this.Move200.Location = new System.Drawing.Point(211, 121);
            this.Move200.Name = "Move200";
            this.Move200.Size = new System.Drawing.Size(75, 23);
            this.Move200.TabIndex = 6;
            this.Move200.Text = "Goto 2000";
            this.Move200.UseVisualStyleBackColor = true;
            this.Move200.Click += new System.EventHandler(this.button3_Click);
            // 
            // Position
            // 
            this.Position.AutoSize = true;
            this.Position.Location = new System.Drawing.Point(318, 121);
            this.Position.Name = "Position";
            this.Position.Size = new System.Drawing.Size(16, 13);
            this.Position.TabIndex = 7;
            this.Position.Text = "---";
            this.Position.Click += new System.EventHandler(this.label1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 262);
            this.Controls.Add(this.Position);
            this.Controls.Add(this.Move200);
            this.Controls.Add(this.Move100);
            this.Controls.Add(this.Move0);
            this.Controls.Add(this.buttonAction);
            this.Controls.Add(this.labelDriverId);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.buttonChoose);
            this.Name = "Form1";
            this.Text = "Focuser Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonChoose;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label labelDriverId;
        private System.Windows.Forms.Button buttonAction;
        private System.Windows.Forms.Button Move0;
        private System.Windows.Forms.Button Move100;
        private System.Windows.Forms.Button Move200;
        private System.Windows.Forms.Label Position;
    }
}

