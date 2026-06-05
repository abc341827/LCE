using System.ComponentModel;
using System.Diagnostics;
using LCE.Models;
using LCE.Services;

namespace LCE
{
    public partial class Form1 : Form
    {
        private ProcessSnapshot? selectedProcess;
        private MemoryScanSession? scanSession;

        public Form1()
        {
            InitializeComponent();
            InitializeScanOptions();
            RefreshProcesses();
        }

        private void refreshProcessesButton_Click(object sender, EventArgs e)
        {
            RefreshProcesses();
        }

        private void processGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (processGridView.CurrentRow?.DataBoundItem is not ProcessSnapshot process)
            {
                selectedProcess = null;
                scanSession = null;
                memoryRegionGridView.DataSource = null;
                scanResultGridView.DataSource = null;
                statusLabel.Text = "请选择一个进程。";
                return;
            }

            selectedProcess = process;
            scanSession = null;
            scanResultGridView.DataSource = null;
            LoadMemoryRegions(process);
        }

        private void scanIntButton_Click(object sender, EventArgs e)
        {
            if (selectedProcess is null)
            {
                statusLabel.Text = "请先选择一个进程。";
                return;
            }

            FirstScan(selectedProcess);
        }

        private void nextScanButton_Click(object sender, EventArgs e)
        {
            if (selectedProcess is null)
            {
                statusLabel.Text = "请先选择一个进程。";
                return;
            }

            if (scanSession is null || scanSession.ProcessId != selectedProcess.Id)
            {
                statusLabel.Text = "请先完成首次扫描。";
                return;
            }

            NextScan(selectedProcess);
        }

        private void RefreshProcesses()
        {
            var processes = Process.GetProcesses()
                .Select(process => new ProcessSnapshot(process))
                .OrderBy(process => process.Name)
                .ThenBy(process => process.Id)
                .ToList();

            processGridView.DataSource = new BindingList<ProcessSnapshot>(processes);
            statusLabel.Text = $"已加载 {processes.Count} 个进程。选择一个进程查看内存区域。";
        }

        private void LoadMemoryRegions(ProcessSnapshot process)
        {
            try
            {
                var regions = MemoryInspector.EnumerateMemoryRegions(process.Id);
                memoryRegionGridView.DataSource = new BindingList<MemoryRegionInfo>(regions.ToList());
                statusLabel.Text = $"{process.Name} ({process.Id})：已枚举 {regions.Count} 个内存区域。";
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                memoryRegionGridView.DataSource = null;
                statusLabel.Text = $"无法打开 {process.Name} ({process.Id})：{ex.Message}";
            }
        }

        private void InitializeScanOptions()
        {
            scanValueTypeComboBox.Items.AddRange(new object[]
            {
                "Int32",
                "Float",
                "Double",
                "String UTF-8",
                "String UTF-16"
            });
            scanValueTypeComboBox.SelectedIndex = 0;

            scanComparisonComboBox.Items.AddRange(new object[]
            {
                "等于输入值",
                "变大",
                "变小",
                "未变"
            });
            scanComparisonComboBox.SelectedIndex = 0;
        }

        private void FirstScan(ProcessSnapshot process)
        {
            try
            {
                SetScanButtonsEnabled(false);
                var valueType = GetSelectedValueType();
                statusLabel.Text = $"正在首次扫描 {process.Name} ({process.Id})...";
                Application.DoEvents();

                scanSession = MemoryInspector.FirstScan(process.Id, valueType, scanValueTextBox.Text);
                BindScanResults(scanSession);
                statusLabel.Text = $"首次扫描完成：找到 {scanSession.Candidates.Count} 个结果。修改目标程序数值后可进行再次扫描。";
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or OverflowException)
            {
                scanSession = null;
                scanResultGridView.DataSource = null;
                statusLabel.Text = $"首次扫描失败：{ex.Message}";
            }
            finally
            {
                SetScanButtonsEnabled(true);
            }
        }

        private void NextScan(ProcessSnapshot process)
        {
            try
            {
                SetScanButtonsEnabled(false);
                var comparison = GetSelectedComparison();
                statusLabel.Text = $"正在对 {process.Name} ({process.Id}) 再次扫描...";
                Application.DoEvents();

                scanSession = MemoryInspector.NextScan(scanSession!, comparison, scanValueTextBox.Text);
                BindScanResults(scanSession);
                statusLabel.Text = $"再次扫描完成：剩余 {scanSession.Candidates.Count} 个结果。";
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or OverflowException)
            {
                scanResultGridView.DataSource = null;
                statusLabel.Text = $"再次扫描失败：{ex.Message}";
            }
            finally
            {
                SetScanButtonsEnabled(true);
            }
        }

        private void BindScanResults(MemoryScanSession session)
        {
            var results = MemoryInspector.ToResults(session);
            scanResultGridView.DataSource = new BindingList<MemoryScanResult>(results.ToList());
        }

        private ScanValueType GetSelectedValueType()
        {
            return scanValueTypeComboBox.SelectedIndex switch
            {
                0 => ScanValueType.Int32,
                1 => ScanValueType.Float,
                2 => ScanValueType.Double,
                3 => ScanValueType.StringUtf8,
                4 => ScanValueType.StringUtf16,
                _ => ScanValueType.Int32
            };
        }

        private ScanComparison GetSelectedComparison()
        {
            return scanComparisonComboBox.SelectedIndex switch
            {
                0 => ScanComparison.EqualToValue,
                1 => ScanComparison.Increased,
                2 => ScanComparison.Decreased,
                3 => ScanComparison.Unchanged,
                _ => ScanComparison.EqualToValue
            };
        }

        private void SetScanButtonsEnabled(bool enabled)
        {
            scanIntButton.Enabled = enabled;
            nextScanButton.Enabled = enabled;
        }
    }
}
