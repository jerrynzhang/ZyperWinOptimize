using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;

namespace ZyperWin__
{
    /// <summary>
    /// Windows Defender 管理页面：检测 Defender 服务状态并提供启用/禁用的操作入口。
    /// 显示各项服务状态（存在性、是否允许运行、是否正在运行）并设置对应的界面指示。
    /// </summary>
    public partial class Defender : UserControl
    {
        /// <summary>
        /// 构造函数：初始化组件并刷新 Defender 状态显示。
        /// </summary>
        public Defender()
        {
            InitializeComponent();
            RefreshDefenderStatus();
        }

        /// <summary>
        /// 刷新 Defender 状态（入口），会触发对 Defender 服务与安全中心状态的检测。
        /// </summary>
        private void RefreshDefenderStatus()
        {
            CheckWinDefendExistence();
        }

        /// <summary>
        /// 检测系统中是否安装存在 WinDefend 服务，并根据检测结果更新界面与按钮状态。
        /// </summary>
        private void CheckWinDefendExistence()
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                bool winDefendExists = services.Any(service => service.ServiceName.Equals("WinDefend", StringComparison.OrdinalIgnoreCase));

                if (winDefendExists)
                {
                    label2.Text = " 系统已存在Defender 服务";
                    label2.ForeColor = Color.FromArgb(76, 175, 80);
                    label2.PrefixColor = Color.FromArgb(76, 175, 80);
                    label2.PrefixSvg = "CheckCircleOutlined";

                    // Defender存在，继续检测其他状态
                    CheckWinDefendServiceStatus();
                    CheckSecurityHealthServiceStatus();

                    // 启用按钮
                    button1.Enabled = true;
                    button2.Enabled = true;

                    // 调用UpdateIconState来更新状态
                    UpdateIconState();
                }
                else
                {
                    label2.Text = " 系统未存在Defender 服务";
                    label2.ForeColor = Color.FromArgb(244, 67, 54);
                    label2.PrefixColor = Color.FromArgb(244, 67, 54);
                    label2.PrefixSvg = "CloseCircleOutlined";

                    // Defender不存在，清空其他标签并冻结按钮
                    ClearOtherLabels();
                    FreezeButtons();

                    // Defender不存在时直接设置为Error状态
                    iconState1.State = AntdUI.TType.Error;
                }
            }
            catch
            {
                label2.Text = " 检测服务存在性时出错";
                label2.ForeColor = Color.FromArgb(244, 67, 54);
                label2.PrefixColor = Color.FromArgb(244, 67, 54);
                label2.PrefixSvg = "CloseCircleOutlined";

                // 出错时也清空其他标签
                ClearOtherLabels();
                FreezeButtons();

                // 出错时也设置为Error状态
                iconState1.State = AntdUI.TType.Error;
            }
        }

        private void ClearOtherLabels()
        {
            // 清空label3、4、7的内容
            label3.Text = " ";
            label4.Text = " ";
            label7.Text = " ";

            // 重置颜色和图标为默认状态
            label3.ForeColor = SystemColors.ControlText;
            label4.ForeColor = SystemColors.ControlText;
            label7.ForeColor = SystemColors.ControlText;

            // 如果有PrefixColor和PrefixSvg属性，也重置
            try
            {
                label3.PrefixColor = SystemColors.ControlText;
                label4.PrefixColor = SystemColors.ControlText;
                label7.PrefixColor = SystemColors.ControlText;

                // 设置为空图标或默认图标
                label3.PrefixSvg = "";
                label4.PrefixSvg = "";
                label7.PrefixSvg = "";
            }
            catch
            {
                // 如果设置失败，忽略
            }
        }

        private void FreezeButtons()
        {
            // 冻结按钮
            button1.Enabled = false;
            button2.Enabled = false;
        }

        /// <summary>
        /// 检查 WinDefend 服务的运行许可与运行状态，并在界面上给予可视化提示。
        /// </summary>
        private void CheckWinDefendServiceStatus()
        {
            try
            {
                ServiceController winDefendService = ServiceController.GetServices()
                    .FirstOrDefault(service => service.ServiceName.Equals("WinDefend", StringComparison.OrdinalIgnoreCase));

                if (winDefendService == null)
                {
                    label3.Text = " Defender 服务未允许运行";
                    label3.ForeColor = Color.FromArgb(244, 67, 54);
                    label3.PrefixColor = Color.FromArgb(244, 67, 54);
                    label3.PrefixSvg = "CloseCircleOutlined";

                    label4.Text = " Defender 服务未在运行";
                    label4.ForeColor = Color.FromArgb(244, 67, 54);
                    label4.PrefixColor = Color.FromArgb(244, 67, 54);
                    label4.PrefixSvg = "CloseCircleOutlined";
                    return;
                }

                bool isDisabled = IsServiceDisabled(winDefendService.ServiceName);

                if (isDisabled)
                {
                    label3.Text = " Defender 服务未允许运行";
                    label3.ForeColor = Color.FromArgb(244, 67, 54);
                    label3.PrefixColor = Color.FromArgb(244, 67, 54);
                    label3.PrefixSvg = "CloseCircleOutlined";
                }
                else
                {
                    label3.Text = " Defender 服务已允许运行";
                    label3.ForeColor = Color.FromArgb(76, 175, 80);
                    label3.PrefixColor = Color.FromArgb(76, 175, 80);
                    label3.PrefixSvg = "CheckCircleOutlined";
                }

                if (winDefendService.Status == ServiceControllerStatus.Running)
                {
                    label4.Text = " Defender 服务正在运行";
                    label4.ForeColor = Color.FromArgb(76, 175, 80);
                    label4.PrefixColor = Color.FromArgb(76, 175, 80);
                    label4.PrefixSvg = "CheckCircleOutlined";
                }
                else
                {
                    label4.Text = " Defender 服务未在运行";
                    label4.ForeColor = Color.FromArgb(244, 67, 54);
                    label4.PrefixColor = Color.FromArgb(244, 67, 54);
                    label4.PrefixSvg = "CloseCircleOutlined";
                }
            }
            catch
            {
                label3.Text = " 检测服务状态时出错";
                label3.ForeColor = Color.FromArgb(244, 67, 54);
                label3.PrefixColor = Color.FromArgb(244, 67, 54);
                label3.PrefixSvg = "CloseCircleOutlined";

                label4.Text = " 检测服务运行时出错";
                label4.ForeColor = Color.FromArgb(244, 67, 54);
                label4.PrefixColor = Color.FromArgb(244, 67, 54);
                label4.PrefixSvg = "CloseCircleOutlined";
            }
        }

        /// <summary>
        /// 检查 SecurityHealthService（安全中心）的状态并更新相应标签。
        /// </summary>
        private void CheckSecurityHealthServiceStatus()
        {
            try
            {
                ServiceController securityHealthService = ServiceController.GetServices()
                    .FirstOrDefault(service => service.ServiceName.Equals("SecurityHealthService", StringComparison.OrdinalIgnoreCase));

                if (securityHealthService == null)
                {
                    label7.Text = " 已禁用安全中心";
                    label7.ForeColor = Color.FromArgb(244, 67, 54);
                    label7.PrefixColor = Color.FromArgb(244, 67, 54);
                    label7.PrefixSvg = "CloseCircleOutlined";
                    return;
                }

                bool isDisabled = IsServiceDisabled(securityHealthService.ServiceName);

                if (isDisabled)
                {
                    label7.Text = " 已禁用安全中心";
                    label7.ForeColor = Color.FromArgb(244, 67, 54);
                    label7.PrefixColor = Color.FromArgb(244, 67, 54);
                    label7.PrefixSvg = "CloseCircleOutlined";
                }
                else
                {
                    label7.Text = " 已启用安全中心";
                    label7.ForeColor = Color.FromArgb(76, 175, 80);
                    label7.PrefixColor = Color.FromArgb(76, 175, 80);
                    label7.PrefixSvg = "CheckCircleOutlined";
                }
            }
            catch
            {
                label7.Text = " 检测安全中心时出错";
                label7.ForeColor = Color.FromArgb(244, 67, 54);
                label7.PrefixColor = Color.FromArgb(244, 67, 54);
                label7.PrefixSvg = "CloseCircleOutlined";
            }
        }

        /// <summary>
        /// 根据各检测项的显示状态计算总体图标状态（Success/Warn/Error）。
        /// </summary>
        private void UpdateIconState()
        {
            int disabledCount = 0;

            // 检查label3 - 只有当有内容时才计数
            if (!string.IsNullOrWhiteSpace(label3.Text) &&
                (label3.Text.Contains("未允许运行") || label3.ForeColor == Color.FromArgb(244, 67, 54)))
            {
                disabledCount++;
            }

            // 检查label4 - 只有当有内容时才计数
            if (!string.IsNullOrWhiteSpace(label4.Text) &&
                (label4.Text.Contains("未在运行") || label4.ForeColor == Color.FromArgb(244, 67, 54)))
            {
                disabledCount++;
            }

            // 检查label7 - 只有当有内容时才计数
            if (!string.IsNullOrWhiteSpace(label7.Text) &&
                (label7.Text.Contains("已禁用") || label7.ForeColor == Color.FromArgb(244, 67, 54)))
            {
                disabledCount++;
            }

            // 设置iconState1的状态
            if (disabledCount == 3)
            {
                iconState1.State = AntdUI.TType.Error;
            }
            else if (disabledCount > 0)
            {
                iconState1.State = AntdUI.TType.Warn;
            }
            else
            {
                iconState1.State = AntdUI.TType.Success;
            }
        }

        /// <summary>
        /// 检查指定服务的启动类型是否为禁用（Start == 4）。
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <returns>禁用返回 true，否则 false</returns>
        private bool IsServiceDisabled(string serviceName)
        {
            try
            {
                using (var serviceKey = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}"))
                {
                    if (serviceKey != null)
                    {
                        var startValue = serviceKey.GetValue("Start");
                        if (startValue != null && startValue is int start)
                        {
                            return start == 4;
                        }
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// 响应“禁用 Defender”按钮：调用外部脚本禁用 Defender，并刷新状态。
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            var MainWindow = this.ParentForm as MainWindow;
            if (MainWindow == null) return;
            button1.Enabled = false;
            button2.Enabled = false;
            MainWindow.menu1.Enabled = false;
            DialogResult result = MessageBox.Show("是否禁用Windows Defender？", "ZyperWin++", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = @".\Bin\NSudoLG.exe",
                        Arguments = @"-U:T -P:E -ShowWindowMode:Hide Defender\DisableWD.bat",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    System.Diagnostics.Process.Start(psi)?.WaitForExit();
                    Process.Start(".\\Bin\\Defender\\NoticeOFF.bat");

                    MessageBox.Show("已禁用完成，请立即重启系统以生效。", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshDefenderStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"禁用失败: {ex.Message}", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                button1.Enabled = true;
                button2.Enabled = true;
                MainWindow.menu1.Enabled = true;
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;
                MainWindow.menu1.Enabled = true;
            }
        }

        /// <summary>
        /// 响应“启用 Defender”按钮：调用外部脚本启用 Defender，并刷新状态。
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            var MainWindow = this.ParentForm as MainWindow;
            if (MainWindow == null) return;
            button1.Enabled = false;
            button2.Enabled = false;
            MainWindow.menu1.Enabled = false;

            DialogResult result = MessageBox.Show("是否启用Windows Defender？", "ZyperWin++", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = @".\Bin\NSudoLG.exe",
                        Arguments = @"-U:T -P:E -ShowWindowMode:Hide Defender\EnableWD.bat",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    System.Diagnostics.Process.Start(psi)?.WaitForExit();
                    Process.Start(".\\Bin\\Defender\\NoticeON.bat");

                    MessageBox.Show("已启用完成，需要重启系统以生效。", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshDefenderStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"启用失败: {ex.Message}", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                button1.Enabled = true;
                button2.Enabled = true;
                MainWindow.menu1.Enabled = true;
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;
                MainWindow.menu1.Enabled = true;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.123912.com/s/PJv7Vv-BxGr");
        }
    }
}