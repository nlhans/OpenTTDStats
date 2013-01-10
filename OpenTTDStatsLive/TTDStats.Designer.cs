namespace OpenTTDStatsLive
{
    partial class TTDStats
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
            this.lbl_SampleSpeed = new System.Windows.Forms.Label();
            this.buffer = new System.Windows.Forms.ProgressBar();
            this.tb_sampleSpeed = new System.Windows.Forms.TrackBar();
            this.lbl_buffer = new System.Windows.Forms.Label();
            this.lbl_average = new System.Windows.Forms.Label();
            this.tb_samplePeriod = new System.Windows.Forms.TrackBar();
            this.lbl_status = new System.Windows.Forms.Label();
            this.split = new System.Windows.Forms.SplitContainer();
            this.cb_Camera = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.tb_sampleSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_samplePeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.split)).BeginInit();
            this.split.Panel1.SuspendLayout();
            this.split.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_SampleSpeed
            // 
            this.lbl_SampleSpeed.AutoSize = true;
            this.lbl_SampleSpeed.Location = new System.Drawing.Point(12, 20);
            this.lbl_SampleSpeed.Name = "lbl_SampleSpeed";
            this.lbl_SampleSpeed.Size = new System.Drawing.Size(108, 13);
            this.lbl_SampleSpeed.TabIndex = 0;
            this.lbl_SampleSpeed.Text = "Sample Speed: 25fps";
            // 
            // buffer
            // 
            this.buffer.Location = new System.Drawing.Point(770, 13);
            this.buffer.Name = "buffer";
            this.buffer.Size = new System.Drawing.Size(159, 23);
            this.buffer.TabIndex = 1;
            // 
            // tb_sampleSpeed
            // 
            this.tb_sampleSpeed.Location = new System.Drawing.Point(136, 9);
            this.tb_sampleSpeed.Maximum = 100;
            this.tb_sampleSpeed.Minimum = 1;
            this.tb_sampleSpeed.Name = "tb_sampleSpeed";
            this.tb_sampleSpeed.Size = new System.Drawing.Size(129, 32);
            this.tb_sampleSpeed.TabIndex = 2;
            this.tb_sampleSpeed.TickFrequency = 5;
            this.tb_sampleSpeed.Value = 1;
            this.tb_sampleSpeed.ValueChanged += new System.EventHandler(this.tb_sampleSpeed_ValueChanged);
            // 
            // lbl_buffer
            // 
            this.lbl_buffer.AutoSize = true;
            this.lbl_buffer.Location = new System.Drawing.Point(726, 20);
            this.lbl_buffer.Name = "lbl_buffer";
            this.lbl_buffer.Size = new System.Drawing.Size(38, 13);
            this.lbl_buffer.TabIndex = 3;
            this.lbl_buffer.Text = "Buffer:";
            // 
            // lbl_average
            // 
            this.lbl_average.AutoSize = true;
            this.lbl_average.Location = new System.Drawing.Point(271, 20);
            this.lbl_average.Name = "lbl_average";
            this.lbl_average.Size = new System.Drawing.Size(148, 13);
            this.lbl_average.TabIndex = 4;
            this.lbl_average.Text = "Averaging period: 15 seconds";
            // 
            // tb_samplePeriod
            // 
            this.tb_samplePeriod.Location = new System.Drawing.Point(425, 9);
            this.tb_samplePeriod.Maximum = 360;
            this.tb_samplePeriod.Minimum = 15;
            this.tb_samplePeriod.Name = "tb_samplePeriod";
            this.tb_samplePeriod.Size = new System.Drawing.Size(159, 32);
            this.tb_samplePeriod.TabIndex = 5;
            this.tb_samplePeriod.TickFrequency = 15;
            this.tb_samplePeriod.Value = 15;
            this.tb_samplePeriod.ValueChanged += new System.EventHandler(this.tb_period_ValueChanged);
            // 
            // lbl_status
            // 
            this.lbl_status.AutoSize = true;
            this.lbl_status.Location = new System.Drawing.Point(590, 29);
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(35, 13);
            this.lbl_status.TabIndex = 6;
            this.lbl_status.Text = "label1";
            // 
            // split
            // 
            this.split.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.split.IsSplitterFixed = true;
            this.split.Location = new System.Drawing.Point(0, 0);
            this.split.Name = "split";
            this.split.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // split.Panel1
            // 
            this.split.Panel1.Controls.Add(this.cb_Camera);
            this.split.Panel1.Controls.Add(this.lbl_SampleSpeed);
            this.split.Panel1.Controls.Add(this.lbl_status);
            this.split.Panel1.Controls.Add(this.buffer);
            this.split.Panel1.Controls.Add(this.tb_samplePeriod);
            this.split.Panel1.Controls.Add(this.tb_sampleSpeed);
            this.split.Panel1.Controls.Add(this.lbl_average);
            this.split.Panel1.Controls.Add(this.lbl_buffer);
            this.split.Size = new System.Drawing.Size(938, 691);
            this.split.SplitterIncrement = 50;
            this.split.TabIndex = 7;
            // 
            // cb_Camera
            // 
            this.cb_Camera.AutoSize = true;
            this.cb_Camera.Location = new System.Drawing.Point(593, 9);
            this.cb_Camera.Name = "cb_Camera";
            this.cb_Camera.Size = new System.Drawing.Size(89, 17);
            this.cb_Camera.TabIndex = 7;
            this.cb_Camera.Text = "Sync Camera";
            this.cb_Camera.UseVisualStyleBackColor = true;
            this.cb_Camera.CheckedChanged += new System.EventHandler(this.cb_Camera_CheckedChanged);
            // 
            // TTDStats
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(938, 691);
            this.Controls.Add(this.split);
            this.Name = "TTDStats";
            this.Text = "TTD Stats";
            ((System.ComponentModel.ISupportInitialize)(this.tb_sampleSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_samplePeriod)).EndInit();
            this.split.Panel1.ResumeLayout(false);
            this.split.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split)).EndInit();
            this.split.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbl_SampleSpeed;
        private System.Windows.Forms.ProgressBar buffer;
        private System.Windows.Forms.TrackBar tb_sampleSpeed;
        private System.Windows.Forms.Label lbl_buffer;
        private System.Windows.Forms.Label lbl_average;
        private System.Windows.Forms.TrackBar tb_samplePeriod;
        private System.Windows.Forms.Label lbl_status;
        private System.Windows.Forms.SplitContainer split;
        private System.Windows.Forms.CheckBox cb_Camera;
    }
}

