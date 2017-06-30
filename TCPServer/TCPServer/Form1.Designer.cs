namespace TCPServer
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tbConsoleOutput = new System.Windows.Forms.TextBox();
            this.tbIPAddress = new System.Windows.Forms.TextBox();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStartListening = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.tbPayload = new System.Windows.Forms.TextBox();
            this.btnFindIPv4IP = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lbClients = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbConsoleOutput
            // 
            this.tbConsoleOutput.Location = new System.Drawing.Point(12, 12);
            this.tbConsoleOutput.Multiline = true;
            this.tbConsoleOutput.Name = "tbConsoleOutput";
            this.tbConsoleOutput.Size = new System.Drawing.Size(590, 260);
            this.tbConsoleOutput.TabIndex = 0;
            // 
            // tbIPAddress
            // 
            this.tbIPAddress.Location = new System.Drawing.Point(55, 278);
            this.tbIPAddress.Name = "tbIPAddress";
            this.tbIPAddress.Size = new System.Drawing.Size(156, 22);
            this.tbIPAddress.TabIndex = 1;
            this.tbIPAddress.Text = "192.168.0.14";
            // 
            // tbPort
            // 
            this.tbPort.Location = new System.Drawing.Point(217, 278);
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(100, 22);
            this.tbPort.TabIndex = 2;
            this.tbPort.Text = "23000";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 282);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "IP/Port";
            // 
            // btnStartListening
            // 
            this.btnStartListening.Location = new System.Drawing.Point(55, 306);
            this.btnStartListening.Name = "btnStartListening";
            this.btnStartListening.Size = new System.Drawing.Size(156, 23);
            this.btnStartListening.TabIndex = 4;
            this.btnStartListening.Text = "Start Listening";
            this.btnStartListening.UseVisualStyleBackColor = true;
            this.btnStartListening.Click += new System.EventHandler(this.btnStartListening_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(446, 308);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(156, 23);
            this.btnSend.TabIndex = 5;
            this.btnSend.Text = "&Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // tbPayload
            // 
            this.tbPayload.Location = new System.Drawing.Point(446, 278);
            this.tbPayload.Name = "tbPayload";
            this.tbPayload.Size = new System.Drawing.Size(156, 22);
            this.tbPayload.TabIndex = 6;
            // 
            // btnFindIPv4IP
            // 
            this.btnFindIPv4IP.Location = new System.Drawing.Point(217, 306);
            this.btnFindIPv4IP.Name = "btnFindIPv4IP";
            this.btnFindIPv4IP.Size = new System.Drawing.Size(100, 23);
            this.btnFindIPv4IP.TabIndex = 7;
            this.btnFindIPv4IP.Text = "Find IP";
            this.btnFindIPv4IP.UseVisualStyleBackColor = true;
            this.btnFindIPv4IP.Click += new System.EventHandler(this.btnFindIPv4IP_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(608, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "Clients:";
            // 
            // lbClients
            // 
            this.lbClients.FormattingEnabled = true;
            this.lbClients.ItemHeight = 12;
            this.lbClients.Location = new System.Drawing.Point(608, 27);
            this.lbClients.Name = "lbClients";
            this.lbClients.Size = new System.Drawing.Size(120, 244);
            this.lbClients.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(394, 282);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "PayLoad";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 343);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbClients);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnFindIPv4IP);
            this.Controls.Add(this.tbPayload);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnStartListening);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbPort);
            this.Controls.Add(this.tbIPAddress);
            this.Controls.Add(this.tbConsoleOutput);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbConsoleOutput;
        private System.Windows.Forms.TextBox tbIPAddress;
        private System.Windows.Forms.TextBox tbPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStartListening;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox tbPayload;
        private System.Windows.Forms.Button btnFindIPv4IP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbClients;
        private System.Windows.Forms.Label label3;
    }
}

