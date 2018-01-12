namespace DST_CLIENT
{
    partial class Client
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Client));
            this.btn_upload = new System.Windows.Forms.Button();
            this.btn_Dowm = new System.Windows.Forms.Button();
            this.btn_exit = new System.Windows.Forms.Button();
            this.rTB = new System.Windows.Forms.RichTextBox();
            this.grb_choose = new System.Windows.Forms.GroupBox();
            this.rbn_manual = new System.Windows.Forms.RadioButton();
            this.rbn_auto = new System.Windows.Forms.RadioButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.grb_choose.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_upload
            // 
            this.btn_upload.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_upload.Location = new System.Drawing.Point(12, 433);
            this.btn_upload.Name = "btn_upload";
            this.btn_upload.Size = new System.Drawing.Size(150, 61);
            this.btn_upload.TabIndex = 1;
            this.btn_upload.Text = "上傳數據";
            this.btn_upload.UseVisualStyleBackColor = true;
            this.btn_upload.Click += new System.EventHandler(this.btn_upload_Click);
            // 
            // btn_Dowm
            // 
            this.btn_Dowm.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Dowm.Location = new System.Drawing.Point(195, 433);
            this.btn_Dowm.Name = "btn_Dowm";
            this.btn_Dowm.Size = new System.Drawing.Size(150, 61);
            this.btn_Dowm.TabIndex = 2;
            this.btn_Dowm.Text = "下載數據";
            this.btn_Dowm.UseVisualStyleBackColor = true;
            this.btn_Dowm.Click += new System.EventHandler(this.btn_Dowm_Click);
            // 
            // btn_exit
            // 
            this.btn_exit.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_exit.Location = new System.Drawing.Point(431, 433);
            this.btn_exit.Name = "btn_exit";
            this.btn_exit.Size = new System.Drawing.Size(150, 61);
            this.btn_exit.TabIndex = 5;
            this.btn_exit.Text = "退出";
            this.btn_exit.UseVisualStyleBackColor = true;
            this.btn_exit.Click += new System.EventHandler(this.btn_exit_Click);
            // 
            // rTB
            // 
            this.rTB.BackColor = System.Drawing.Color.White;
            this.rTB.Location = new System.Drawing.Point(12, 12);
            this.rTB.Name = "rTB";
            this.rTB.ReadOnly = true;
            this.rTB.Size = new System.Drawing.Size(583, 415);
            this.rTB.TabIndex = 6;
            this.rTB.Text = "";
            // 
            // grb_choose
            // 
            this.grb_choose.Controls.Add(this.rbn_manual);
            this.grb_choose.Controls.Add(this.rbn_auto);
            this.grb_choose.Location = new System.Drawing.Point(12, 299);
            this.grb_choose.Name = "grb_choose";
            this.grb_choose.Size = new System.Drawing.Size(200, 128);
            this.grb_choose.TabIndex = 7;
            this.grb_choose.TabStop = false;
            this.grb_choose.Text = "Choose Mode";
            this.grb_choose.Visible = false;
            // 
            // rbn_manual
            // 
            this.rbn_manual.AutoSize = true;
            this.rbn_manual.Location = new System.Drawing.Point(32, 77);
            this.rbn_manual.Name = "rbn_manual";
            this.rbn_manual.Size = new System.Drawing.Size(59, 16);
            this.rbn_manual.TabIndex = 1;
            this.rbn_manual.TabStop = true;
            this.rbn_manual.Text = "manual";
            this.rbn_manual.UseVisualStyleBackColor = true;
            this.rbn_manual.CheckedChanged += new System.EventHandler(this.rbn_manual_CheckedChanged);
            // 
            // rbn_auto
            // 
            this.rbn_auto.AutoSize = true;
            this.rbn_auto.Location = new System.Drawing.Point(32, 34);
            this.rbn_auto.Name = "rbn_auto";
            this.rbn_auto.Size = new System.Drawing.Size(77, 16);
            this.rbn_auto.TabIndex = 0;
            this.rbn_auto.TabStop = true;
            this.rbn_auto.Text = "automatic";
            this.rbn_auto.UseVisualStyleBackColor = true;
            this.rbn_auto.CheckedChanged += new System.EventHandler(this.rbn_auto_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 6000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "DTS CLIENT";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // timer2
            // 
            this.timer2.Enabled = true;
            this.timer2.Interval = 7200000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // Client
            // 
            this.AcceptButton = this.btn_Dowm;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.NavajoWhite;
            this.ClientSize = new System.Drawing.Size(604, 511);
            this.Controls.Add(this.grb_choose);
            this.Controls.Add(this.rTB);
            this.Controls.Add(this.btn_exit);
            this.Controls.Add(this.btn_Dowm);
            this.Controls.Add(this.btn_upload);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Client";
            this.ShowInTaskbar = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Client_FormClosing);
            this.Load += new System.EventHandler(this.Client_Load);
            this.SizeChanged += new System.EventHandler(this.Client_SizeChanged);
            this.grb_choose.ResumeLayout(false);
            this.grb_choose.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_upload;
        private System.Windows.Forms.Button btn_Dowm;
        private System.Windows.Forms.Button btn_exit;
        private System.Windows.Forms.RichTextBox rTB;
        private System.Windows.Forms.GroupBox grb_choose;
        private System.Windows.Forms.RadioButton rbn_manual;
        private System.Windows.Forms.RadioButton rbn_auto;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Timer timer2;
    }
}

