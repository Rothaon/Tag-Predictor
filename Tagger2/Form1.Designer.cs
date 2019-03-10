namespace TaggerPred
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
            this.pb_main = new System.Windows.Forms.PictureBox();
            this.lb_Score = new System.Windows.Forms.ListBox();
            this.lbl_prog = new System.Windows.Forms.Label();
            this.lbl_buffer = new System.Windows.Forms.Label();
            this.lbl_reminder = new System.Windows.Forms.Label();
            this.lbl_agro = new System.Windows.Forms.Label();
            this.lbl_date = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pb_main)).BeginInit();
            this.SuspendLayout();
            // 
            // pb_main
            // 
            this.pb_main.Location = new System.Drawing.Point(0, 0);
            this.pb_main.Name = "pb_main";
            this.pb_main.Size = new System.Drawing.Size(1704, 1041);
            this.pb_main.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_main.TabIndex = 0;
            this.pb_main.TabStop = false;
            // 
            // lb_Score
            // 
            this.lb_Score.Dock = System.Windows.Forms.DockStyle.Right;
            this.lb_Score.FormattingEnabled = true;
            this.lb_Score.Location = new System.Drawing.Point(1701, 0);
            this.lb_Score.Name = "lb_Score";
            this.lb_Score.Size = new System.Drawing.Size(203, 1041);
            this.lb_Score.TabIndex = 1;
            // 
            // lbl_prog
            // 
            this.lbl_prog.AutoSize = true;
            this.lbl_prog.Location = new System.Drawing.Point(1676, 21);
            this.lbl_prog.Name = "lbl_prog";
            this.lbl_prog.Size = new System.Drawing.Size(19, 13);
            this.lbl_prog.TabIndex = 2;
            this.lbl_prog.Text = "¿?";
            // 
            // lbl_buffer
            // 
            this.lbl_buffer.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lbl_buffer.AutoSize = true;
            this.lbl_buffer.Location = new System.Drawing.Point(1660, 48);
            this.lbl_buffer.Name = "lbl_buffer";
            this.lbl_buffer.Size = new System.Drawing.Size(19, 13);
            this.lbl_buffer.TabIndex = 3;
            this.lbl_buffer.Text = "10";
            // 
            // lbl_reminder
            // 
            this.lbl_reminder.AutoSize = true;
            this.lbl_reminder.Location = new System.Drawing.Point(1656, 75);
            this.lbl_reminder.Name = "lbl_reminder";
            this.lbl_reminder.Size = new System.Drawing.Size(40, 52);
            this.lbl_reminder.TabIndex = 4;
            this.lbl_reminder.Text = "JKL\r\n+=-\r\nH Skip\r\nS save\r\n";
            // 
            // lbl_agro
            // 
            this.lbl_agro.AutoSize = true;
            this.lbl_agro.Location = new System.Drawing.Point(1659, 131);
            this.lbl_agro.Name = "lbl_agro";
            this.lbl_agro.Size = new System.Drawing.Size(35, 13);
            this.lbl_agro.TabIndex = 5;
            this.lbl_agro.Text = "label1";
            this.lbl_agro.Click += new System.EventHandler(this.label1_Click);
            // 
            // lbl_date
            // 
            this.lbl_date.AutoSize = true;
            this.lbl_date.Location = new System.Drawing.Point(1656, 155);
            this.lbl_date.Name = "lbl_date";
            this.lbl_date.Size = new System.Drawing.Size(29, 13);
            this.lbl_date.TabIndex = 6;
            this.lbl_date.Text = "fsdfs";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.lbl_date);
            this.Controls.Add(this.lbl_agro);
            this.Controls.Add(this.lbl_reminder);
            this.Controls.Add(this.lbl_buffer);
            this.Controls.Add(this.lbl_prog);
            this.Controls.Add(this.lb_Score);
            this.Controls.Add(this.pb_main);
            this.KeyPreview = true;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pb_main)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pb_main;
        private System.Windows.Forms.ListBox lb_Score;
        private System.Windows.Forms.Label lbl_prog;
        private System.Windows.Forms.Label lbl_buffer;
        private System.Windows.Forms.Label lbl_reminder;
        private System.Windows.Forms.Label lbl_agro;
        private System.Windows.Forms.Label lbl_date;
    }
}

