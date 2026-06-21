using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZyperWin__
{
    /// <summary>
    /// 激活页面：提供调用本地批处理脚本（MAS_AIO_CN.cmd）进行 Office/Windows 激活的入口。
    /// 包含脚本执行的异步封装与用户提示。
    /// </summary>
    public partial class Activate : UserControl
    {
        public Activate()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/cmontage/mas-cn");
        }

        /// <summary>
        /// 响应“开始激活”按钮：检查批处理文件是否存在并异步执行，完成后提示用户。
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button1.Text = "执行中...";

            try
            {
                // 检查文件是否存在
                string cmdFilePath = Path.Combine(Application.StartupPath, "Bin", "MAS_AIO_CN.cmd");

                if (!File.Exists(cmdFilePath))
                {
                    MessageBox.Show("未找到 MAS_AIO_CN.cmd 文件！\n请确保文件位于 .\\Bin\\ 目录下。",
                                  "文件未找到",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    return;
                }

                await ExecuteCmdFile(cmdFilePath);

                MessageBox.Show("MAS激活执行完成！", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行失败：\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复按钮状态
                button1.Enabled = true;
                button1.Text = "开始激活";
            }
        }

        /// <summary>
        /// 异步执行指定的 CMD 文件，等待其完成并在失败时抛出异常。
        /// </summary>
        /// <param name="filePath">CMD 文件的完整路径</param>
        private async Task ExecuteCmdFile(string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.Arguments = $"/c \"{filePath}\"";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = false; // 设置为false可以看到CMD窗口
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(filePath);

                        process.Start();

                        // 读取输出（可选）
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"CMD执行失败，退出码: {process.ExitCode}\n错误信息: {error}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"执行CMD文件时出错: {ex.Message}");
                }
            });
        }
    }
}
