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
            scanPanel = new Panel();
            scanResultGridView = new DataGridView();
            scanHeaderPanel = new Panel();
            scanIntButton = new Button();
            nextScanButton = new Button();
            unknownInitialValueCheckBox = new CheckBox();
            privateReadWriteOnlyCheckBox = new CheckBox();
            scanComparisonComboBox = new ComboBox();
            scanComparisonLabel = new Label();
            scanValueTextBox = new TextBox();
            scanValueLabel = new Label();
            scanValueTypeComboBox = new ComboBox();
            scanTypeLabel = new Label();
            scanResultLabel = new Label();
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
            scanPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)scanResultGridView).BeginInit();
            scanHeaderPanel.SuspendLayout();
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
            memoryPanel.Controls.Add(scanPanel);
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
            memoryRegionGridView.Size = new Size(744, 332);
            memoryRegionGridView.TabIndex = 1;
            // 
            // scanPanel
            // 
            scanPanel.Controls.Add(scanResultGridView);
            scanPanel.Controls.Add(scanHeaderPanel);
            scanPanel.Dock = DockStyle.Bottom;
            scanPanel.Location = new Point(8, 371);
            scanPanel.Name = "scanPanel";
            scanPanel.Size = new Size(744, 260);
            scanPanel.TabIndex = 2;
            // 
            // scanResultGridView
            // 
            scanResultGridView.AllowUserToAddRows = false;
            scanResultGridView.AllowUserToDeleteRows = false;
            scanResultGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            scanResultGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            scanResultGridView.Dock = DockStyle.Fill;
            scanResultGridView.Location = new Point(0, 68);
            scanResultGridView.Name = "scanResultGridView";
            scanResultGridView.ReadOnly = true;
            scanResultGridView.RowHeadersVisible = false;
            scanResultGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            scanResultGridView.Size = new Size(744, 192);
            scanResultGridView.TabIndex = 1;
            // 
            // scanHeaderPanel
            // 
            scanHeaderPanel.Controls.Add(scanIntButton);
            scanHeaderPanel.Controls.Add(nextScanButton);
            scanHeaderPanel.Controls.Add(unknownInitialValueCheckBox);
            scanHeaderPanel.Controls.Add(privateReadWriteOnlyCheckBox);
            scanHeaderPanel.Controls.Add(scanComparisonComboBox);
            scanHeaderPanel.Controls.Add(scanComparisonLabel);
            scanHeaderPanel.Controls.Add(scanValueTextBox);
            scanHeaderPanel.Controls.Add(scanValueLabel);
            scanHeaderPanel.Controls.Add(scanValueTypeComboBox);
            scanHeaderPanel.Controls.Add(scanTypeLabel);
            scanHeaderPanel.Controls.Add(scanResultLabel);
            scanHeaderPanel.Dock = DockStyle.Top;
            scanHeaderPanel.Location = new Point(0, 0);
            scanHeaderPanel.Name = "scanHeaderPanel";
            scanHeaderPanel.Size = new Size(744, 68);
            scanHeaderPanel.TabIndex = 0;
            // 
            // scanIntButton
            // 
            scanIntButton.Location = new Point(398, 7);
            scanIntButton.Name = "scanIntButton";
            scanIntButton.Size = new Size(88, 25);
            scanIntButton.TabIndex = 3;
            scanIntButton.Text = "首次扫描";
            scanIntButton.UseVisualStyleBackColor = true;
            scanIntButton.Click += scanIntButton_Click;
            // 
            // nextScanButton
            // 
            nextScanButton.Location = new Point(492, 7);
            nextScanButton.Name = "nextScanButton";
            nextScanButton.Size = new Size(88, 25);
            nextScanButton.TabIndex = 4;
            nextScanButton.Text = "再次扫描";
            nextScanButton.UseVisualStyleBackColor = true;
            nextScanButton.Click += nextScanButton_Click;
            // 
            // unknownInitialValueCheckBox
            // 
            unknownInitialValueCheckBox.AutoSize = true;
            unknownInitialValueCheckBox.Location = new Point(586, 10);
            unknownInitialValueCheckBox.Name = "unknownInitialValueCheckBox";
            unknownInitialValueCheckBox.Size = new Size(87, 21);
            unknownInitialValueCheckBox.TabIndex = 9;
            unknownInitialValueCheckBox.Text = "未知初始值";
            unknownInitialValueCheckBox.UseVisualStyleBackColor = true;
            // 
            // privateReadWriteOnlyCheckBox
            // 
            privateReadWriteOnlyCheckBox.AutoSize = true;
            privateReadWriteOnlyCheckBox.Checked = true;
            privateReadWriteOnlyCheckBox.CheckState = CheckState.Checked;
            privateReadWriteOnlyCheckBox.Location = new Point(271, 40);
            privateReadWriteOnlyCheckBox.Name = "privateReadWriteOnlyCheckBox";
            privateReadWriteOnlyCheckBox.Size = new Size(123, 21);
            privateReadWriteOnlyCheckBox.TabIndex = 10;
            privateReadWriteOnlyCheckBox.Text = "只扫私有读写内存";
            privateReadWriteOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanComparisonComboBox
            // 
            scanComparisonComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            scanComparisonComboBox.FormattingEnabled = true;
            scanComparisonComboBox.Location = new Point(134, 38);
            scanComparisonComboBox.Name = "scanComparisonComboBox";
            scanComparisonComboBox.Size = new Size(131, 25);
            scanComparisonComboBox.TabIndex = 8;
            // 
            // scanComparisonLabel
            // 
            scanComparisonLabel.AutoSize = true;
            scanComparisonLabel.Location = new Point(62, 42);
            scanComparisonLabel.Name = "scanComparisonLabel";
            scanComparisonLabel.Size = new Size(56, 17);
            scanComparisonLabel.TabIndex = 7;
            scanComparisonLabel.Text = "再次条件";
            // 
            // scanValueTextBox
            // 
            scanValueTextBox.Location = new Point(271, 8);
            scanValueTextBox.Name = "scanValueTextBox";
            scanValueTextBox.Size = new Size(121, 23);
            scanValueTextBox.TabIndex = 2;
            // 
            // scanValueLabel
            // 
            scanValueLabel.AutoSize = true;
            scanValueLabel.Location = new Point(235, 11);
            scanValueLabel.Name = "scanValueLabel";
            scanValueLabel.Size = new Size(32, 17);
            scanValueLabel.TabIndex = 1;
            scanValueLabel.Text = "数值";
            // 
            // scanValueTypeComboBox
            // 
            scanValueTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            scanValueTypeComboBox.FormattingEnabled = true;
            scanValueTypeComboBox.Location = new Point(100, 8);
            scanValueTypeComboBox.Name = "scanValueTypeComboBox";
            scanValueTypeComboBox.Size = new Size(129, 25);
            scanValueTypeComboBox.TabIndex = 6;
            // 
            // scanTypeLabel
            // 
            scanTypeLabel.AutoSize = true;
            scanTypeLabel.Location = new Point(62, 11);
            scanTypeLabel.Name = "scanTypeLabel";
            scanTypeLabel.Size = new Size(32, 17);
            scanTypeLabel.TabIndex = 5;
            scanTypeLabel.Text = "类型";
            // 
            // scanResultLabel
            // 
            scanResultLabel.AutoSize = true;
            scanResultLabel.Location = new Point(0, 11);
            scanResultLabel.Name = "scanResultLabel";
            scanResultLabel.Size = new Size(56, 17);
            scanResultLabel.TabIndex = 0;
            scanResultLabel.Text = "数值扫描";
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
            scanPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)scanResultGridView).EndInit();
            scanHeaderPanel.ResumeLayout(false);
            scanHeaderPanel.PerformLayout();
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
        private Panel scanPanel;
        private DataGridView scanResultGridView;
        private Panel scanHeaderPanel;
        private Button scanIntButton;
        private Button nextScanButton;
        private CheckBox unknownInitialValueCheckBox;
        private CheckBox privateReadWriteOnlyCheckBox;
        private ComboBox scanComparisonComboBox;
        private Label scanComparisonLabel;
        private TextBox scanValueTextBox;
        private Label scanValueLabel;
        private ComboBox scanValueTypeComboBox;
        private Label scanTypeLabel;
        private Label scanResultLabel;
        private Label memoryTitleLabel;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
    }
}
