using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZyperWin__
{
    /// <summary>
    /// Appx 管理页面：列出已安装的可卸载 UWP/应用包，支持选择并批量卸载。
    /// 使用 PowerShell 获取包列表并调用 Remove-AppxPackage 执行卸载。
    /// </summary>
    public partial class Appx : UserControl
    {
        public Appx()
        {
            InitializeComponent();
            this.Load += new System.EventHandler(this.Appx_Load);
        }

        /// <summary>
        /// 页面加载事件：异步加载应用列表并初始化交互事件与进度条。
        /// </summary>
        private async void Appx_Load(object sender, EventArgs e)
        {
            await LoadAppListAsync();
            checkedListBox1.MouseUp += CheckedListBox1_MouseUp;

            // 初始化进度条 - 使用相同的方法
            SetProgressValue(0);
        }

        private void CheckedListBox1_MouseUp(object sender, MouseEventArgs e)
        {
            int index = checkedListBox1.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches && index >= 0)
            {
                bool isChecked = checkedListBox1.GetItemChecked(index);
                checkedListBox1.SetItemChecked(index, !isChecked);
            }
        }

        /// <summary>
        /// 异步调用 PowerShell 获取当前可卸载应用的完整包名列表，并填充到界面列表中。
        /// </summary>
        private async Task LoadAppListAsync()
        {
            try
            {
                checkedListBox1.Invoke((MethodInvoker)delegate
                {
                    checkedListBox1.Items.Clear();
                    checkedListBox1.Items.Add("正在加载应用，请稍候...");
                    // 显示加载状态
                    SetProgressStateLoading(true);
                });

                List<string> packages = new List<string>();

                await Task.Run(() =>
                {
                    try
                    {
                        string psScript = "Get-AppxPackage | Where-Object { !$_.IsFramework -and !$_.NonRemovable } | ForEach-Object { $_.PackageFullName }";

                        using (Process p = new Process())
                        {
                            p.StartInfo.FileName = "powershell.exe";
                            p.StartInfo.Arguments = $"-Command \"{psScript}\"";
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.StartInfo.CreateNoWindow = true;

                            p.Start();

                            string output = p.StandardOutput.ReadToEnd();
                            string error = p.StandardError.ReadToEnd();
                            p.WaitForExit();

                            if (p.ExitCode == 0)
                            {
                                packages = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(s => s.Trim())
                                                 .Where(s => !string.IsNullOrEmpty(s))
                                                 .ToList();
                            }
                            else
                            {
                                throw new Exception("PowerShell 错误: " + error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        packages.Clear();
                        packages.Add("加载失败: " + ex.Message);
                    }
                });

                checkedListBox1.Invoke((MethodInvoker)delegate
                {
                    checkedListBox1.Items.Clear();
                    if (packages.Count > 0 && !packages[0].StartsWith("加载失败"))
                    {
                        foreach (string pkg in packages)
                        {
                            checkedListBox1.Items.Add(pkg, false);
                        }
                    }
                    else
                    {
                        checkedListBox1.Items.AddRange(packages.ToArray());
                    }
                });
            }
            catch (Exception ex)
            {
                checkedListBox1.Invoke((MethodInvoker)delegate
                {
                    checkedListBox1.Items.Clear();
                    checkedListBox1.Items.Add("异常: " + ex.Message);
                    SetProgressStateLoading(false);
                });
            }
        }

        /// <summary>
        /// 响应“开始卸载”按钮：收集用户勾选的包并依次调用 PowerShell 卸载，显示进度与结果。
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            List<string> selectedPackages = new List<string>();
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    string item = checkedListBox1.Items[i].ToString();
                    if (!item.StartsWith("正在加载") && !item.StartsWith("没有") && !item.StartsWith("加载失败"))
                    {
                        selectedPackages.Add(item);
                    }
                }
            }

            if (selectedPackages.Count == 0)
            {
                MessageBox.Show("请至少勾选一个要卸载的应用！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string msg = "即将卸载以下应用：\n\n" + string.Join("\n", selectedPackages.Take(10))
                       + (selectedPackages.Count > 10 ? $"\n\n（还有 {selectedPackages.Count - 10} 个）" : "");

            DialogResult dr = MessageBox.Show(msg + "\n\n确定要继续吗？", "ZyperWin++", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr != DialogResult.Yes) return;

            // 设置进度条
            SetProgressMax(selectedPackages.Count);
            SetProgressValue(0);

            var MainWindow = this.ParentForm as MainWindow;
            if (MainWindow == null) return;
            MainWindow.menu1.Enabled = false;
            checkedListBox1.Enabled = false;
            button1.Enabled = false;
            button1.Text = "卸载中...";

            int successCount = 0;
            int failedCount = 0;
            List<string> failedPackages = new List<string>();

            await Task.Run(() =>
            {
                for (int i = 0; i < selectedPackages.Count; i++)
                {
                    string pkg = selectedPackages[i];
                    bool success = false;

                    try
                    {
                        using (Process p = new Process())
                        {
                            p.StartInfo.FileName = "powershell.exe";
                            p.StartInfo.Arguments = $"-Command \"Remove-AppxPackage -Package '{pkg}'\"";
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            p.Start();
                            p.WaitForExit();

                            success = p.ExitCode == 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        failedPackages.Add($"{pkg} (错误: {ex.Message})");
                        success = false;
                    }

                    if (success)
                        successCount++;
                    else
                        failedCount++;

                    // 更新进度条
                    this.Invoke((MethodInvoker)delegate
                    {
                        SetProgressValue(i + 1);
                    });
                }
            });

            await LoadAppListAsync();
            MainWindow.menu1.Enabled = true;
            checkedListBox1.Enabled = true;
            button1.Enabled = true;
            button1.Text = "开始卸载";

            string resultMessage = $"卸载完成！\n成功: {successCount} 个\n失败: {failedCount} 个";

            if (failedCount > 0)
            {
                resultMessage += "\n\n失败的应用：";
                if (failedPackages.Count <= 5)
                {
                    resultMessage += "\n" + string.Join("\n", failedPackages);
                }
                else
                {
                    resultMessage += $"\n{string.Join("\n", failedPackages.Take(3))}";
                    resultMessage += $"\n... 还有 {failedPackages.Count - 3} 个失败应用";
                }
                resultMessage += "\n\n失败的应用可能需要管理员权限或正在运行中。";
            }

            MessageBox.Show(resultMessage, "ZyperWin++", MessageBoxButtons.OK,
                failedCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

            SetProgressValue(0);
        }

        // 使用相同的进度条设置方法
        /// <summary>
        /// 设置进度条的当前值（兼容不同控件实现）。
        /// </summary>
        private void SetProgressValue(int value)
        {
            try
            {
                var valueProp = progress1.GetType().GetProperty("Value");
                if (valueProp != null && valueProp.CanWrite)
                {
                    valueProp.SetValue(progress1, value);
                }
            }
            catch { }
        }

        /// <summary>
        /// 设置进度条的最大值（兼容不同控件属性名）。
        /// </summary>
        private void SetProgressMax(int max)
        {
            try
            {
                var maxProp = progress1.GetType().GetProperty("Max");
                if (maxProp != null && maxProp.CanWrite)
                {
                    maxProp.SetValue(progress1, max);
                    return;
                }

                var maximumProp = progress1.GetType().GetProperty("Maximum");
                if (maximumProp != null && maximumProp.CanWrite)
                {
                    maximumProp.SetValue(progress1, max);
                }
            }
            catch { }
        }

        // 添加加载状态设置方法
        /// <summary>
        /// 以“加载”状态显示或隐藏进度控件，兼容 AntdUI 的状态枚举或回退为 Visible。
        /// </summary>
        private void SetProgressStateLoading(bool isLoading)
        {
            try
            {
                if (isLoading)
                {
                    // 尝试设置加载状态
                    var stateProp = progress1.GetType().GetProperty("State");
                    if (stateProp != null && stateProp.CanWrite)
                    {
                        // 尝试使用Ant Design UI的状态枚举
                        var loadingState = Enum.Parse(stateProp.PropertyType, "加载");
                        stateProp.SetValue(progress1, loadingState);
                    }
                    else
                    {
                        // 备用方案：显示进度条
                        progress1.Visible = true;
                    }
                }
                else
                {
                    // 恢复默认状态
                    var stateProp = progress1.GetType().GetProperty("State");
                    if (stateProp != null && stateProp.CanWrite)
                    {
                        var defaultState = Enum.Parse(stateProp.PropertyType, "默认");
                        stateProp.SetValue(progress1, defaultState);
                    }
                    else
                    {
                        progress1.Visible = false;
                    }
                }
            }
            catch
            {
                // 如果设置状态失败，简单显示/隐藏
                progress1.Visible = isLoading;
            }
        }
    }
}