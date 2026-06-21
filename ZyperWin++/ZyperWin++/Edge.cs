using AntdUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ZyperWin__
{
    /// <summary>
    /// Edge 组件管理窗口：检测 Edge、Edge WebView、Edge Core 的安装状态，提供卸载与控制更新服务的功能。
    /// 使用外部批处理和 NSudo 提权工具执行卸载/启停操作，并在界面上反馈状态。
    /// </summary>
    public partial class Edge : AntdUI.Window
    {
        private string edgePath = "";
        private string edgeWebViewPath = "";
        private string edgeCorePath = "";
        private bool isProcessingCheckbox = false; // 添加这个字段
        /// <summary>
        /// 构造函数：初始化组件、冻结操作控件并触发状态检测流程。
        /// </summary>
        public Edge()
        {
            InitializeComponent();

            // 初始化时先冻结所有按钮和复选框
            SetAllOperationControlsEnabled(false);

            // 检测状态
            DetectAllStatus();
        }

        /// <summary>
        /// 检测所有相关状态（安装、服务等），并在控制台输出检测进度。
        /// </summary>
        private void DetectAllStatus()
        {
            Console.WriteLine("开始检测所有状态...");

            // 检测安装状态
            DetectEdgeInstallation();

            // 检测服务状态
            CheckUpdateServiceStatus();

            Console.WriteLine("所有状态检测完成");
        }

        /// <summary>
        /// 检测 Edge 及其相关组件的安装状态，确定安装路径与版本，并更新界面标签与按钮。
        /// </summary>
        private void DetectEdgeInstallation()
        {
            Console.WriteLine("开始检测Edge安装状态...");

            // 强制刷新文件系统缓存
            Microsoft.Win32.SystemEvents.InvokeOnEventsThread(new Action(() => { }));

            // 检测系统位数并设置基础路径
            string programFilesPath = Environment.Is64BitOperatingSystem ?
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) :
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            Console.WriteLine($"ProgramFiles路径: {programFilesPath}");

            // 检测Edge安装状态
            string edgeBasePath = Path.Combine(programFilesPath, "Microsoft", "Edge", "Application");
            Console.WriteLine($"Edge检测路径: {edgeBasePath}, 存在: {Directory.Exists(edgeBasePath)}");
            DetectAndSetEdgeStatus(edgeBasePath, label2, label3, ref edgePath, "Edge", button1);

            // 检测EdgeWebView安装状态
            string edgeWebViewBasePath = Path.Combine(programFilesPath, "Microsoft", "EdgeWebView", "Application");
            Console.WriteLine($"EdgeWebView检测路径: {edgeWebViewBasePath}, 存在: {Directory.Exists(edgeWebViewBasePath)}");
            DetectAndSetEdgeStatus(edgeWebViewBasePath, label4, label6, ref edgeWebViewPath, "Edge WebView", button2);

            // 检测EdgeCore安装状态
            string edgeCoreBasePath = Path.Combine(programFilesPath, "Microsoft", "EdgeCore");
            Console.WriteLine($"EdgeCore检测路径: {edgeCoreBasePath}, 存在: {Directory.Exists(edgeCoreBasePath)}");
            DetectAndSetEdgeStatus(edgeCoreBasePath, label8, label9, ref edgeCorePath, "Edge Core", button3);

            // 检测是否需要启用"卸载全部"按钮
            RefreshUninstallAllButton();

            Console.WriteLine("Edge安装状态检测完成");
        }

        /// <summary>
        /// 根据给定的基路径检测指定组件是否安装，并设置对应的状态标签、版本信息与操作按钮状态。
        /// </summary>
        private void DetectAndSetEdgeStatus(string basePath, AntdUI.Label statusLabel, AntdUI.Label versionLabel, ref string targetPath, string appName, AntdUI.Button correspondingButton)
        {
            bool isInstalled = false;

            if (Directory.Exists(basePath))
            {
                // 获取所有版本号文件夹，过滤掉非版本文件夹
                var versionDirs = Directory.GetDirectories(basePath)
                    .Where(dir => IsVersionDirectory(Path.GetFileName(dir)))
                    .ToArray();

                Console.WriteLine($"{appName} 找到 {versionDirs.Length} 个版本目录");

                if (versionDirs.Length > 0)
                {
                    // 选择版本号最高的文件夹
                    string latestVersionDir = GetLatestVersionDirectory(versionDirs);
                    string version = Path.GetFileName(latestVersionDir);

                    // 检查Installer文件夹和setup.exe是否存在
                    string installerPath = Path.Combine(latestVersionDir, "Installer");
                    string setupExePath = Path.Combine(installerPath, "setup.exe");

                    Console.WriteLine($"{appName} 安装器路径: {setupExePath}, 存在: {File.Exists(setupExePath)}");

                    if (Directory.Exists(installerPath) && File.Exists(setupExePath))
                    {
                        // 设置状态
                        statusLabel.Text = "已安装";
                        statusLabel.ForeColor = Color.FromArgb(76, 175, 80);
                        versionLabel.Text = $"版本：{version}";
                        targetPath = setupExePath;
                        isInstalled = true;

                        Console.WriteLine($"{appName} 检测到最新版本: {version}");
                    }
                    else
                    {
                        Console.WriteLine($"{appName} 找到版本目录但缺少Installer或setup.exe");
                        SetNotInstalled(statusLabel, versionLabel, ref targetPath, appName);
                    }
                }
                else
                {
                    Console.WriteLine($"{appName} 未找到有效的版本目录");
                    SetNotInstalled(statusLabel, versionLabel, ref targetPath, appName);
                }
            }
            else
            {
                Console.WriteLine($"{appName} 基础路径不存在");
                SetNotInstalled(statusLabel, versionLabel, ref targetPath, appName);
            }

            // 根据安装状态设置按钮
            SetButtonEnabled(correspondingButton, isInstalled, appName);
        }

        /// <summary>
        /// 在 UI 线程上设置按钮是否可用，并打印日志。
        /// </summary>
        private void SetButtonEnabled(AntdUI.Button button, bool enabled, string appName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<AntdUI.Button, bool, string>(SetButtonEnabled), button, enabled, appName);
                return;
            }

            button.Enabled = enabled;
            Console.WriteLine($"{appName} 按钮状态: {(enabled ? "启用" : "禁用")}");
        }

        /// <summary>
        /// 根据当前系统中组件的安装情况决定是否启用“卸载全部”按钮。
        /// </summary>
        private void RefreshUninstallAllButton()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(RefreshUninstallAllButton));
                return;
            }

            // 检测系统位数并设置基础路径
            string programFilesPath = Environment.Is64BitOperatingSystem ?
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) :
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            // 检查是否有任何组件已安装
            bool edgeInstalled = CheckComponentInstalled(Path.Combine(programFilesPath, "Microsoft", "Edge", "Application"));
            bool webViewInstalled = CheckComponentInstalled(Path.Combine(programFilesPath, "Microsoft", "EdgeWebView", "Application"));
            bool coreInstalled = CheckComponentInstalled(Path.Combine(programFilesPath, "Microsoft", "EdgeCore"));

            bool anyComponentInstalled = edgeInstalled || webViewInstalled || coreInstalled;
            button4.Enabled = anyComponentInstalled;

            Console.WriteLine($"卸载全部按钮状态: Edge[{edgeInstalled}] WebView[{webViewInstalled}] Core[{coreInstalled}] => {anyComponentInstalled}");
        }

        /// <summary>
        /// 检查指定路径下是否存在有效的版本目录以及安装器（setup.exe）。
        /// </summary>
        private bool CheckComponentInstalled(string basePath)
        {
            if (!Directory.Exists(basePath))
                return false;

            var versionDirs = Directory.GetDirectories(basePath)
                .Where(dir => IsVersionDirectory(Path.GetFileName(dir)))
                .ToArray();

            if (versionDirs.Length == 0)
                return false;

            // 检查是否有有效的安装器
            string latestVersionDir = GetLatestVersionDirectory(versionDirs);
            string installerPath = Path.Combine(latestVersionDir, "Installer");
            string setupExePath = Path.Combine(installerPath, "setup.exe");

            return Directory.Exists(installerPath) && File.Exists(setupExePath);
        }

        // 检查是否为版本目录
        /// <summary>
        /// 判断目录名是否符合版本号格式（例如 x.x.x.x）。
        /// </summary>
        private bool IsVersionDirectory(string dirName)
        {
            // 版本目录应该是 x.x.x.x 格式的数字
            return System.Text.RegularExpressions.Regex.IsMatch(dirName, @"^\d+\.\d+\.\d+\.\d+$");
        }

        // 获取最新版本目录
        /// <summary>
        /// 在多个版本目录中选取版本号最大的目录并返回其路径。
        /// </summary>
        private string GetLatestVersionDirectory(string[] versionDirs)
        {
            var versions = new List<Version>();
            var versionMap = new Dictionary<string, string>();

            foreach (string dir in versionDirs)
            {
                string versionStr = Path.GetFileName(dir);
                if (Version.TryParse(versionStr, out Version version))
                {
                    versions.Add(version);
                    versionMap[versionStr] = dir;
                }
            }

            // 按版本号降序排序，取最高版本
            versions.Sort((a, b) => b.CompareTo(a));
            string latestVersionStr = versions[0].ToString();

            // 查找对应的目录路径
            foreach (var kvp in versionMap)
            {
                if (Version.TryParse(kvp.Key, out Version dirVersion) && dirVersion.Equals(versions[0]))
                {
                    return kvp.Value;
                }
            }

            return versionDirs[0];
        }

        /// <summary>
        /// 在界面上标记指定组件为“未安装”并重置路径与版本显示。
        /// </summary>
        private void SetNotInstalled(AntdUI.Label statusLabel, AntdUI.Label versionLabel, ref string targetPath, string appName)
        {
            statusLabel.Text = "未安装";
            statusLabel.ForeColor = Color.FromArgb(244, 67, 54);
            versionLabel.Text = "版本：无";
            targetPath = "";
            Console.WriteLine($"{appName} 未安装");
        }

        // 卸载Edge
        private void button1_Click(object sender, EventArgs e)
        {
            ExecuteBatchWithNSudo(@"Edge\Edge.bat", "Edge");
        }

        // 卸载Edge WebView
        private void button2_Click(object sender, EventArgs e)
        {
            ExecuteBatchWithNSudo(@"Edge\EdgeWebView2.bat", "Edge WebView");
        }

        // 卸载Edge Core
        private void button3_Click(object sender, EventArgs e)
        {
            ExecuteBatchWithNSudo(@"Edge\EdgeCore.bat", "Edge Core");
        }

        // 卸载全部
        private void button4_Click(object sender, EventArgs e)
        {
            ExecuteBatchWithNSudo(@"Edge\All.bat", "所有Edge组件");
        }

        // 使用NSudo执行批处理
        /// <summary>
        /// 使用 NSudo 提权工具执行指定的批处理文件（例如卸载脚本），并在完成后刷新状态。
        /// </summary>
        private void ExecuteBatchWithNSudo(string batchFile, string operationName)
        {
            string batchPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bin", batchFile);

            Console.WriteLine($"查找批处理文件: {batchPath}");

            if (!File.Exists(batchPath))
            {
                batchPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, batchFile);
                Console.WriteLine($"尝试备用路径: {batchPath}");

                if (!File.Exists(batchPath))
                {
                    MessageBox.Show($"找不到批处理文件: {batchFile}\n查找路径: {batchPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            DialogResult result = MessageBox.Show($"确定要执行 {operationName} 操作吗？", "确认操作",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // 立即冻结所有操作控件
            SetAllOperationControlsEnabled(false);

            int exitCode = 0;
            bool success = false;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Bin\NSudoLG.exe"),
                    Arguments = $@"-U:T -P:E -ShowWindowMode:Hide ""{batchPath}""",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(batchPath)
                };

                Console.WriteLine($"执行命令: {startInfo.FileName} {startInfo.Arguments}");

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit(60000);
                    exitCode = process.ExitCode;

                    Console.WriteLine($"批处理执行完成，退出代码: {exitCode}");

                    // 添加延迟，确保系统完成卸载
                    System.Threading.Thread.Sleep(1000);
                }

                success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行过程中出现错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"异常详情: {ex}");
            }
            finally
            {
                // 添加更长的延迟确保系统完全处理卸载
                System.Threading.Thread.Sleep(1000);

                // 多次刷新状态确保检测准确
                for (int i = 0; i < 1; i++)
                {
                    ForceRefreshStatus();
                    System.Threading.Thread.Sleep(1000);
                }

                // 显示完成提示
                if (success)
                {
                    if (exitCode == 0)
                    {
                        MessageBox.Show($"{operationName} 操作完成！", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"{operationName} 操作完成！\n退出代码: {exitCode}", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // 最后再刷新一次状态
                ForceRefreshStatus();
            }
        }

        // 设置所有操作控件的启用状态
        /// <summary>
        /// 设置所有操作相关控件（按钮、复选框）是否可用，线程安全地在 UI 线程上执行。
        /// </summary>
        private void SetAllOperationControlsEnabled(bool enabled)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(SetAllOperationControlsEnabled), enabled);
                return;
            }

            button1.Enabled = enabled;
            button2.Enabled = enabled;
            button3.Enabled = enabled;
            button4.Enabled = enabled;
            checkbox1.Enabled = enabled;

            Console.WriteLine($"所有操作控件: {(enabled ? "启用" : "禁用")}");
        }

        // 检查服务状态
        /// <summary>
        /// 检查 Edge 更新服务（edgeupdate）的状态并根据结果设置复选框状态。
        /// </summary>
        private void CheckUpdateServiceStatus()
        {
            try
            {
                string serviceName = "edgeupdate";

                // 直接检查服务是否存在
                bool serviceExists = CheckServiceExists(serviceName);

                if (!serviceExists)
                {
                    // 服务不存在，禁用复选框
                    checkbox1.Enabled = false;
                    checkbox1.Checked = false;
                    Console.WriteLine("Edge更新服务不存在，禁用checkbox1");
                    return;
                }

                // 服务存在，启用复选框
                checkbox1.Enabled = true;

                // 临时取消事件绑定，避免递归
                checkbox1.CheckedChanged -= checkbox1_CheckedChanged;

                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = $"qc {serviceName}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(1000);

                    // 检查服务启动类型
                    if (output.Contains("DISABLED"))
                    {
                        checkbox1.Checked = true; // 禁用状态，勾选复选框
                        Console.WriteLine("Edge更新服务已禁用");
                    }
                    else
                    {
                        checkbox1.Checked = false; // 自动状态，不勾选
                        Console.WriteLine("Edge更新服务已启用");
                    }
                }

                // 重新绑定事件
                checkbox1.CheckedChanged += checkbox1_CheckedChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查更新服务状态失败: {ex.Message}");
                checkbox1.Enabled = false;
                checkbox1.Checked = false;
            }
        }

        /// <summary>
        /// 调用 sc 命令检查指定服务是否存在（通过返回码判断）。
        /// </summary>
        private bool CheckServiceExists(string serviceName)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = $"query {serviceName}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(1000);

                    // 如果服务存在，sc query会返回成功(退出代码0)
                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查服务存在性失败 {serviceName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 强制刷新所有检测状态：重置变量、清空标签并重新触发检测流程。
        /// </summary>
        private void ForceRefreshStatus()
        {
            try
            {
                // 重置路径变量
                edgePath = "";
                edgeWebViewPath = "";
                edgeCorePath = "";

                // 清除所有标签状态
                ResetAllLabels();

                // 强制垃圾回收
                GC.Collect();
                GC.WaitForPendingFinalizers();

                System.Threading.Thread.Sleep(500);

                // 检测所有状态
                DetectAllStatus();

                // 特别刷新服务状态
                CheckUpdateServiceStatus();

                this.Invalidate();
                Application.DoEvents();

                Console.WriteLine("状态强制刷新完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"强制刷新状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 将所有状态标签重置为“检测中...”初始状态并刷新界面。
        /// </summary>
        private void ResetAllLabels()
        {
            // Edge
            label2.Text = "检测中...";
            label2.ForeColor = Color.Gray;
            label3.Text = "版本：检测中...";

            // Edge WebView
            label4.Text = "检测中...";
            label4.ForeColor = Color.Gray;
            label6.Text = "版本：检测中...";

            // Edge Core
            label8.Text = "检测中...";
            label8.ForeColor = Color.Gray;
            label9.Text = "版本：检测中...";

            // 强制界面更新
            this.Refresh();
            Application.DoEvents();
        }

        /// <summary>
        /// 窗口加载事件：再次触发状态检测以确保显示信息最新。
        /// </summary>
        private void Edge_Load(object sender, EventArgs e)
        {
            // 窗口加载时再次检测确保状态正确
            DetectAllStatus();
        }

        /// <summary>
        /// 复选框切换事件：根据勾选状态执行启用/禁用更新服务的批处理，带并发保护。
        /// </summary>
        private void checkbox1_CheckedChanged(object sender, AntdUI.BoolEventArgs e)
        {
            // 如果复选框被禁用，直接返回
            if (!checkbox1.Enabled)
            {
                Console.WriteLine("复选框被禁用，跳过操作");
                return;
            }

            // 防止重复点击
            if (isProcessingCheckbox) return;

            isProcessingCheckbox = true;

            try
            {
                // 立即冻结所有控件
                SetAllOperationControlsEnabled(false);

                string batchFile = checkbox1.Checked ? @".\Bin\Edge\UpdateStop.bat" : @".\Bin\Edge\UpdateStart.bat";
                string operationName = checkbox1.Checked ? "禁用Edge更新" : "启用Edge更新";

                ExecuteBatchForCheckbox(batchFile, operationName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 恢复原始状态
                checkbox1.Checked = !checkbox1.Checked;
            }
            finally
            {
                isProcessingCheckbox = false;
            }
        }

        /// <summary>
        /// 为复选框操作执行指定批处理（通过 NSudo），并在完成后刷新服务状态与界面控件。
        /// </summary>
        private void ExecuteBatchForCheckbox(string batchFile, string operationName)
        {
            string batchPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bin", batchFile);

            Console.WriteLine($"查找批处理文件: {batchPath}");

            if (!File.Exists(batchPath))
            {
                batchPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, batchFile);
                Console.WriteLine($"尝试备用路径: {batchPath}");

                if (!File.Exists(batchPath))
                {
                    MessageBox.Show($"找不到批处理文件: {batchFile}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            int exitCode = 0;
            bool success = false;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Bin\NSudoLG.exe"),
                    Arguments = $@"-U:T -P:E -ShowWindowMode:Hide ""{batchPath}""",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(batchPath)
                };

                Console.WriteLine($"执行命令: {startInfo.FileName} {startInfo.Arguments}");

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit(30000); // 30秒超时
                    exitCode = process.ExitCode;

                    Console.WriteLine($"批处理执行完成，退出代码: {exitCode}");
                }

                success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行过程中出现错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"异常详情: {ex}");
            }
            finally
            {
                // 等待服务状态稳定
                System.Threading.Thread.Sleep(1000);

                // 刷新服务状态（专门为复选框刷新）
                CheckUpdateServiceStatus();

                // 重新启用控件
                SetAllOperationControlsEnabled(true);

                if (success)
                {
                    string status = checkbox1.Checked ? "已禁用" : "已启用";
                    MessageBox.Show($"Edge更新服务{status}！", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}