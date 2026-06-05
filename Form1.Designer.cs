namespace LCE
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer = new SplitContainer();
            processPanel = new Panel();
            processGridView = new DataGridView();
            refreshProcessesButton = new Button();
            processTitleLabel = new Label();
            memoryPanel = new Panel();
            memoryRegionGridView = new DataGridView();
            memoryTitleLabel = new Label();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            processPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)processGridView).BeginInit();
            memoryPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)memoryRegionGridView).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(processPanel);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(memoryPanel);
            splitContainer.Size = new Size(1184, 639);
            splitContainer.SplitterDistance = 420;
            splitContainer.TabIndex = 0;
            // 
            // processPanel
            // 
            processPanel.Controls.Add(processGridView);
            processPanel.Controls.Add(refreshProcessesButton);
            processPanel.Controls.Add(processTitleLabel);
            processPanel.Dock = DockStyle.Fill;
            processPanel.Location = new Point(0, 0);
            processPanel.Name = "processPanel";
            processPanel.Padding = new Padding(8);
            processPanel.Size = new Size(420, 639);
            processPanel.TabIndex = 0;
            // 
            // processGridView
            // 
            processGridView.AllowUserToAddRows = false;
            processGridView.AllowUserToDeleteRows = false;
            processGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            processGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            processGridView.Dock = DockStyle.Fill;
            processGridView.Location = new Point(8, 39);
            processGridView.MultiSelect = false;
            processGridView.Name = "processGridView";
            processGridView.ReadOnly = true;
            processGridView.RowHeadersVisible = false;
            processGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            processGridView.Size = new Size(404, 592);
            processGridView.TabIndex = 2;
            processGridView.SelectionChanged += processGridView_SelectionChanged;
            // 
            // refreshProcessesButton
            // 
            refreshProcessesButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refreshProcessesButton.Location = new Point(337, 8);
            refreshProcessesButton.Name = "refreshProcessesButton";
            refreshProcessesButton.Size = new Size(75, 25);
            refreshProcessesButton.TabIndex = 1;
            refreshProcessesButton.Text = "刷新";
            refreshProcessesButton.UseVisualStyleBackColor = true;
            refreshProcessesButton.Click += refreshProcessesButton_Click;
            // 
            // processTitleLabel
            // 
            processTitleLabel.AutoSize = true;
            processTitleLabel.Location = new Point(8, 13);
            processTitleLabel.Name = "processTitleLabel";
            processTitleLabel.Size = new Size(56, 17);
            processTitleLabel.TabIndex = 0;
            processTitleLabel.Text = "进程列表";
            // 
            // memoryPanel
            // 
            memoryPanel.Controls.Add(memoryRegionGridView);
            memoryPanel.Controls.Add(memoryTitleLabel);
            memoryPanel.Dock = DockStyle.Fill;
            memoryPanel.Location = new Point(0, 0);
            memoryPanel.Name = "memoryPanel";
            memoryPanel.Padding = new Padding(8);
            memoryPanel.Size = new Size(760, 639);
            memoryPanel.TabIndex = 0;
            // 
            // memoryRegionGridView
            // 
            memoryRegionGridView.AllowUserToAddRows = false;
            memoryRegionGridView.AllowUserToDeleteRows = false;
            memoryRegionGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            memoryRegionGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            memoryRegionGridView.Dock = DockStyle.Fill;
            memoryRegionGridView.Location = new Point(8, 39);
            memoryRegionGridView.Name = "memoryRegionGridView";
            memoryRegionGridView.ReadOnly = true;
            memoryRegionGridView.RowHeadersVisible = false;
            memoryRegionGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            memoryRegionGridView.Size = new Size(744, 592);
            memoryRegionGridView.TabIndex = 1;
            // 
            // memoryTitleLabel
            // 
            memoryTitleLabel.AutoSize = true;
            memoryTitleLabel.Location = new Point(8, 13);
            memoryTitleLabel.Name = "memoryTitleLabel";
            memoryTitleLabel.Size = new Size(80, 17);
            memoryTitleLabel.TabIndex = 0;
            memoryTitleLabel.Text = "内存区域列表";
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 639);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1184, 22);
            statusStrip.TabIndex = 1;
            statusStrip.Text = "statusStrip";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(32, 17);
            statusLabel.Text = "就绪";
            // 
            // Form1
            // 
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 661);
            Controls.Add(splitContainer);
            Controls.Add(statusStrip);
            MinimumSize = new Size(900, 500);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LCE - 进程与内存区域查看器";
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            processPanel.ResumeLayout(false);
            processPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)processGridView).EndInit();
            memoryPanel.ResumeLayout(false);
            memoryPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)memoryRegionGridView).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SplitContainer splitContainer;
        private Panel processPanel;
        private DataGridView processGridView;
        private Button refreshProcessesButton;
        private Label processTitleLabel;
        private Panel memoryPanel;
        private DataGridView memoryRegionGridView;
        private Label memoryTitleLabel;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
    }
}
