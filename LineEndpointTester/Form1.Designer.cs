namespace LineEndpointTester
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
            this.clearButton = new System.Windows.Forms.Button();
            this.recalcButton = new System.Windows.Forms.Button();
            this.plusTenButton = new System.Windows.Forms.Button();
            this.clipButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(12, 12);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 23);
            this.clearButton.TabIndex = 0;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // recalcButton
            // 
            this.recalcButton.Location = new System.Drawing.Point(93, 12);
            this.recalcButton.Name = "recalcButton";
            this.recalcButton.Size = new System.Drawing.Size(75, 23);
            this.recalcButton.TabIndex = 1;
            this.recalcButton.Text = "Recalc";
            this.recalcButton.UseVisualStyleBackColor = true;
            this.recalcButton.Click += new System.EventHandler(this.recalcButton_Click);
            // 
            // plusTenButton
            // 
            this.plusTenButton.Location = new System.Drawing.Point(174, 12);
            this.plusTenButton.Name = "plusTenButton";
            this.plusTenButton.Size = new System.Drawing.Size(75, 23);
            this.plusTenButton.TabIndex = 2;
            this.plusTenButton.Text = "+10";
            this.plusTenButton.UseVisualStyleBackColor = true;
            this.plusTenButton.Click += new System.EventHandler(this.plusTenButton_Click);
            // 
            // clipButton
            // 
            this.clipButton.Location = new System.Drawing.Point(255, 12);
            this.clipButton.Name = "clipButton";
            this.clipButton.Size = new System.Drawing.Size(75, 23);
            this.clipButton.TabIndex = 3;
            this.clipButton.Text = "-> Clipboard";
            this.clipButton.UseVisualStyleBackColor = true;
            this.clipButton.Click += new System.EventHandler(this.clipButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 516);
            this.Controls.Add(this.clipButton);
            this.Controls.Add(this.plusTenButton);
            this.Controls.Add(this.recalcButton);
            this.Controls.Add(this.clearButton);
            this.Name = "Form1";
            this.Text = "Endpoint Tester";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button recalcButton;
        private System.Windows.Forms.Button plusTenButton;
        private System.Windows.Forms.Button clipButton;
    }
}

