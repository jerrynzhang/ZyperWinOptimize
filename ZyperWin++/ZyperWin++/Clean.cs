using AntdUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZyperWin__
{
    /// <summary>
    /// 垃圾清理页面：定义多个清理命令，允许用户选择要执行的清理项并以异步方式执行这些清理操作。
    /// 支持取消、进度显示与超时保护。
    /// </summary>
    public partial class Clean : UserControl
    {
        /// <summary>
        /// 清理命令字典：键为命令名称，值为包含命令字符串的CleanupCommand对象。
        /// </summary>
        private Dictionary<string, CleanupCommand> cleanupCommands = new Dictionary<string, CleanupCommand>();
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 清理命令函数
        /// </summary>
        private class CleanupCommand
        {
            public string Command { get; set; }
        }

        /// <summary>
        /// 构造函数：初始化组件、命令字典以及界面树视图。
        /// </summary>
        public Clean()
        {
            InitializeComponent();
            InitializeCommands();
            InitializeTreeView();
        }

        /// <summary>
        /// 初始化可用的清理命令集合（cleanupCommands），每项包含要执行的命令字符串。
        /// </summary>
        private void InitializeCommands()
        {
            cleanupCommands = new Dictionary<string, CleanupCommand>
            {
                {"Terminal Server Client缓存", new CleanupCommand { Command = "del /f /s /q \"%LocalAppData%\\Microsoft\\Terminal Server Client\\Cache\\*\" >nul 2>&1" }},
                {"Windows更新缓存", new CleanupCommand { Command = "del /f /s /q \"%SystemRoot%\\SoftwareDistribution\\Download\\*\" >nul 2>&1" }},
                {"网页缓存", new CleanupCommand { Command = "del /f /s /q \"%LocalAppData%\\Microsoft\\Windows\\INetCache\\*\" >nul 2>&1" }},
                {"Cookies", new CleanupCommand { Command = "del /f /s /q \"%LocalAppData%\\Microsoft\\Windows\\INetCookies\\*\" >nul 2>&1" }},
                {"缩略图缓存", new CleanupCommand { Command = "del /f /s /q \"%LocalAppData%\\Microsoft\\Windows\\Explorer\\thumbcache_*.db\" >nul 2>&1" }},
                {"D3D着色器缓存", new CleanupCommand { Command = "del /f /s /q \"%LocalAppData%\\Local\\D3DSCache\\*\" >nul 2>&1" }},
                {".NET程序集缓存", new CleanupCommand { Command = "rd /s /q \"%WinDir%\\assembly\\NativeImages_v4.0.30319_32\" >nul 2>&1 & rd /s /q \"%WinDir%\\assembly\\NativeImages_v4.0.30319_64\" >nul 2>&1" }},
                {"传递优化缓存", new CleanupCommand { Command = "del /f /s /q \"%SystemRoot%\\SoftwareDistribution\\DeliveryOptimization\\*\" >nul 2>&1" }},
                {"过时的WinSxS文件", new CleanupCommand { Command = "dism /Online /Cleanup-Image /StartComponentCleanup /ResetBase >nul 2>&1" }},
                {"错误应用包", new CleanupCommand { Command = "powershell -Command \"Get-AppxPackage -AllUsers | Where-Object {$_.Status -eq 'Error'} | Remove-AppxPackage -ErrorAction SilentlyContinue\" >nul" }},
                {"Windows日志", new CleanupCommand { Command = "del /f /s /q \"%SystemRoot%\\Logs\\*\" >nul 2>&1" }},
                {"Windows错误报告", new CleanupCommand { Command = "del /f /s /q \"%ProgramData%\\Microsoft\\Windows\\WER\\ReportQueue\\*\" >nul 2>&1" }},
                {"诊断数据", new CleanupCommand { Command = "del /f /s /q \"%ProgramData%\\Microsoft\\Diagnosis\\*\" >nul 2>&1" }},
                {"崩溃dmp文件", new CleanupCommand { Command = "del /f /s /q \"%SystemRoot%\\Minidump\\*.dmp\" >nul 2>&1 & del /f /q \"%SystemRoot%\\memory.dmp\" >nul 2>&1" }},
                {"Windows Defender扫描", new CleanupCommand { Command = "del /f /s /q \"%ProgramData%\\Microsoft\\Windows Defender\\Scans\\*\" >nul 2>&1" }},
                {"WinSxS临时文件", new CleanupCommand { Command = "del /f /s /q \"%SystemRoot%\\WinSxS\\Temp\\*\" >nul 2>&1" }},
                {"系统临时文件", new CleanupCommand { Command = "del /f /s /q \"%SystemRoot%\\Temp\\*\" >nul 2>&1" }},
                {"系统dmp文件", new CleanupCommand { Command = "del /f /s /q \"%SystemDrive%\\*.dmp\" >nul 2>&1" }},
                {"回收站", new CleanupCommand { Command = "rd /s /q \"%SystemDrive%\\$Recycle.bin\" >nul 2>&1" }},
                {"所有临时文件", new CleanupCommand { Command = "del /f /s /q \"%SystemDrive%\\Windows\\Temp\\*\" >nul 2>&1 & del /f /s /q \"%SystemRoot%\\Temp\\*\" >nul 2>&1 & del /f /s /q \"%TEMP%\\*\" >nul 2>&1" }},
                {"预读取文件", new CleanupCommand { Command = "del /f /s /q \"%SystemRoot%\\Prefetch\\*\" >nul 2>&1" }}
            };
        }

        /// <summary>
        /// 构建树视图，按分类列出所有清理选项并默认勾选。
        /// </summary>
        private void InitializeTreeView()
        {
            tree1.Items.Clear();

            // 添加分类节点
            var cacheNode = new TreeItem("缓存文件") { Checked = true, Tag = "cache" };
            var systemNode = new TreeItem("系统文件") { Checked = true, Tag = "system" };
            var tempNode = new TreeItem("临时文件") { Checked = true, Tag = "temp" };

            // 缓存文件 (1-8)
            AddTreeItem(cacheNode, "Terminal Server Client缓存", "Terminal Server Client缓存");
            AddTreeItem(cacheNode, "Windows更新缓存", "Windows更新缓存");
            AddTreeItem(cacheNode, "网页缓存", "网页缓存");
            AddTreeItem(cacheNode, "Cookies", "Cookies");
            AddTreeItem(cacheNode, "缩略图缓存", "缩略图缓存");
            AddTreeItem(cacheNode, "D3D着色器缓存", "D3D着色器缓存");
            AddTreeItem(cacheNode, ".NET程序集缓存", ".NET程序集缓存");
            AddTreeItem(cacheNode, "传递优化缓存", "传递优化缓存");

            // 系统文件 (9-14)
            AddTreeItem(systemNode, "过时的WinSxS文件", "过时的WinSxS文件");
            AddTreeItem(systemNode, "错误应用包", "错误应用包");
            AddTreeItem(systemNode, "Windows日志", "Windows日志");
            AddTreeItem(systemNode, "Windows错误报告", "Windows错误报告");
            AddTreeItem(systemNode, "诊断数据", "诊断数据");
            AddTreeItem(systemNode, "崩溃dmp文件", "崩溃dmp文件");

            // 临时文件 (15-21)
            AddTreeItem(tempNode, "Windows Defender扫描", "Windows Defender扫描");
            AddTreeItem(tempNode, "WinSxS临时文件", "WinSxS临时文件");
            AddTreeItem(tempNode, "系统临时文件", "系统临时文件");
            AddTreeItem(tempNode, "系统dmp文件", "系统dmp文件");
            AddTreeItem(tempNode, "回收站", "回收站");
            AddTreeItem(tempNode, "所有临时文件", "所有临时文件");
            AddTreeItem(tempNode, "预读取文件", "预读取文件");

            tree1.Items.Add(cacheNode);
            tree1.Items.Add(systemNode);
            tree1.Items.Add(tempNode);

            tree1.ExpandAll();
        }

        /// <summary>
        /// 向父节点添加子项，并设置其 Tag 为命令键。
        /// </summary>
        private void AddTreeItem(TreeItem parent, string text, string commandKey)
        {
            var item = new TreeItem(text) { Checked = true, Tag = commandKey };
            parent.Sub.Add(item);
        }

        /// <summary>
        /// 响应“开始清理”按钮：收集选择的项目，确认后以异步方式逐项执行清理命令并显示进度。
        /// 支持取消操作并在完成或取消后恢复 UI 状态。
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            var MainWindow = this.ParentForm as MainWindow;
            if (MainWindow == null) return;
            button1.Enabled = false;
            tree1.Enabled = false;
            MainWindow.menu1.Enabled = false;
            button1.Text = "清理中...";
            _cancellationTokenSource = new CancellationTokenSource();

            var selectedItems = GetSelectedItems();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("请至少选择一个清理项目！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                button1.Enabled = true;
                MainWindow.menu1.Enabled = true;
                tree1.Enabled = true;
                button1.Text = "开始清理";
                return;
            }

            string message = $"即将清理以下 {selectedItems.Count} 个项目：\n\n";
            foreach (var item in selectedItems.Take(8))
            {
                message += $"{GetDisplayName(item)}\n";
            }
            if (selectedItems.Count > 8)
            {
                message += $"以及另外 {selectedItems.Count - 8} 个项目\n";
            }
            message += "\n确定要清理吗？";

            DialogResult result = MessageBox.Show(message, "ZyperWin++", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int cleanedCount = 0;
                int totalCount = selectedItems.Count;
                var cancellationToken = _cancellationTokenSource.Token;

                try
                {
                    // 重置按钮文本
                    this.Invoke((MethodInvoker)delegate
                    {
                        button1.Text = $"清理中... ({cleanedCount}/{totalCount})";
                    });

                    // 遍历并执行每个选中的项目
                    for (int i = 0; i < selectedItems.Count; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        string currentItem = selectedItems[i];

                        // 执行清理命令
                        bool cleaned = await ExecuteCleanupCommandAsync(currentItem, cancellationToken);

                        if (cleaned) cleanedCount++;

                        // 更新按钮文本显示进度
                        this.Invoke((MethodInvoker)delegate
                        {
                            button1.Text = $"清理中... ({i + 1}/{totalCount})";
                        });
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        MessageBox.Show($"清理完成！\n成功清理了 {totalCount} 个项目",
                            "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"清理已取消。\n成功清理了 {cleanedCount} 个项目）",
                            "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("清理任务已被取消。", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                finally
                {
                    button1.Text = "开始清理";
                    button1.Enabled = true;
                    MainWindow.menu1.Enabled = true;
                    tree1.Enabled = true;
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
            }
            else
            {
                button1.Enabled = true;
                tree1.Enabled = true;
                MainWindow.menu1.Enabled = true;
                button1.Text = "开始清理";
            }
        }

        // 获取显示名称的方法
        /// <summary>
        /// 根据命令键返回用于显示的中文名称（用于构建确认提示）。
        /// </summary>
        private string GetDisplayName(string commandKey)
        {
            switch (commandKey)
            {
                case "Terminal Server Client缓存": return "Terminal Server Client缓存";
                case "Windows更新缓存": return "Windows更新缓存";
                case "网页缓存": return "网页缓存";
                case "Cookies": return "Cookies";
                case "缩略图缓存": return "缩略图缓存";
                case "D3D着色器缓存": return "D3D着色器缓存";
                case ".NET程序集缓存": return ".NET程序集缓存";
                case "传递优化缓存": return "传递优化缓存";
                case "过时的WinSxS文件": return "过时的WinSxS文件";
                case "错误应用包": return "错误应用包";
                case "Windows日志": return "Windows日志";
                case "Windows错误报告": return "Windows错误报告";
                case "诊断数据": return "诊断数据";
                case "崩溃dmp文件": return "崩溃dmp文件";
                case "Windows Defender扫描": return "Windows Defender扫描";
                case "WinSxS临时文件": return "WinSxS临时文件";
                case "系统临时文件": return "系统临时文件";
                case "系统dmp文件": return "系统dmp文件";
                case "回收站": return "回收站";
                case "所有临时文件": return "所有临时文件";
                case "预读取文件": return "预读取文件";
                default: return commandKey;
            }
        }

        // 异步执行清理命令
        /// <summary>
        /// 异步执行指定的清理命令，启动外部 cmd 进程并等待完成或超时，返回是否成功。
        /// </summary>
        /// <param name="commandKey">清理命令键</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>成功返回 true，否则 false</returns>
        private async Task<bool> ExecuteCleanupCommandAsync(string commandKey, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (cleanupCommands.ContainsKey(commandKey))
                    {
                        string command = cleanupCommands[commandKey].Command;

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c \"{command}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };

                        using (Process process = new Process { StartInfo = psi })
                        {
                            process.Start();
                            // 等待进程退出或任务被取消，或者超时 (例如60秒)
                            bool processExited = process.WaitForExit(60000);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                if (!process.HasExited)
                                {
                                    try { process.Kill(); } catch { /* 忽略kill异常 */ }
                                }
                                return false; // 被取消
                            }

                            if (processExited)
                            {
                                return process.ExitCode == 0;
                            }
                            else
                            {
                                // 超时
                                try { process.Kill(); } catch { /* 忽略kill异常 */ }
                                return false;
                            }
                        }
                    }
                    return false;
                }
                catch (OperationCanceledException)
                {
                    // 任务被取消
                    return false;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// 从树视图中收集被勾选的清理项，返回其对应的命令键列表。
        /// </summary>
        private List<string> GetSelectedItems()
        {
            var selectedItems = new List<string>();
            foreach (var category in tree1.Items)
            {
                foreach (var item in category.Sub)
                {
                    if (item.Checked && item.Tag is string commandKey)
                    {
                        selectedItems.Add(commandKey);
                    }
                }
            }
            return selectedItems;
        }
    }
}