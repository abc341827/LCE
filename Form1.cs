using System.ComponentModel;
using System.Diagnostics;
using LCE.Models;
using LCE.Services;

namespace LCE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
                memoryRegionGridView.DataSource = null;
                statusLabel.Text = "请选择一个进程。";
                return;
            }

            LoadMemoryRegions(process);
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
    }
}
