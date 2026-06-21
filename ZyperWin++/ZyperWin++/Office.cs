using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ZyperWin__
{
    /// <summary>
    /// Office 管理页面：提供在线安装不同版本 Office 的入口，并支持调用卸载脚本卸载 Office。
    /// </summary>
    public partial class Office : UserControl
    {
        public Office()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 从 AntdUI.Select 控件中获取选中的文本或值，按多种属性顺序尝试读取。
        /// </summary>
        private string GetSelectText(AntdUI.Select selectControl)
        {
            if (selectControl.SelectedIndex >= 0 && selectControl.SelectedIndex < selectControl.Items.Count)
            {
                var item = selectControl.Items[selectControl.SelectedIndex];
                var textProp = item.GetType().GetProperty("Text");
                if (textProp != null)
                    return textProp.GetValue(item)?.ToString();

                var valueProp = item.GetType().GetProperty("Value");
                if (valueProp != null)
                    return valueProp.GetValue(item)?.ToString();

                var nameProp = item.GetType().GetProperty("Name");
                if (nameProp != null)
                    return nameProp.GetValue(item)?.ToString();

                return item.ToString();
            }
            return selectControl.Text;
        }

        /// <summary>
        /// 响应安装按钮：根据用户选择构建模板 URL，并通过 PowerShell 执行在线安装脚本。
        /// </summary>
        private void ButtonInstall_Click(object sender, EventArgs e)
        {
            ButtonInstall.Enabled = false;
            ButtonInstall.Text = "执行中...";

            string version = GetSelectText(select1);
            string architecture = GetSelectText(select2);
            string type = GetSelectText(select3);

            // 调试输出，查看实际获取的值
            Console.WriteLine($"Version: {version}, Architecture: {architecture}, Type: {type}");

            if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(architecture) || string.IsNullOrEmpty(type))
            {
                MessageBox.Show("请确保所有选项都已选择！");
                ButtonInstall.Enabled = true;
                ButtonInstall.Text = "开始安装";
                return;
            }

            // 根据选择映射productCode
            string productCode = "";
            switch (version)
            {
                case "Office365":
                    productCode = "O365ProPlusRetail";
                    break;
                case "Office2024":
                    productCode = "ProPlus2024Retail";
                    break;
                case "Office2021":
                    productCode = "ProPlus2021Retail";
                    break;
                case "Office2019":
                    productCode = "ProPlus2019Retail";
                    break;
                default:
                    MessageBox.Show("未知版本");
                    ButtonInstall.Enabled = true;
                    ButtonInstall.Text = "开始安装";
                    return;
            }

            // 架构映射
            string arch = architecture == "64位" ? "64" : "32";

            // 类型映射
            string excludeApps = "";
            if (type == "常用三件套")
            {
                excludeApps = "&exclude_apps=" + productCode + ":Access,Bing,Groove,Lync,Outlook,OneNote,Publisher,Teams";
            }

            // 构建最终的模板URL
            string template = $"https://www.coolhub.top/get/?prod_to_add={productCode}_zh-cn{excludeApps}&arch={arch}";

            // 确认对话框
            var result = MessageBox.Show($"即将安装：{version} ({arch}位)\n类型：{type}\n再检查一遍是否安装此版本？", "都准备好了吗？", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "powershell.exe";
                    psi.Arguments = $"-NoExit -Command \"irm '{template}' | iex\"";
                    psi.UseShellExecute = true;
                    psi.CreateNoWindow = false;
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("启动 PowerShell 失败：" + ex.Message);
                }
            }

            // 恢复按钮状态
            ButtonInstall.Enabled = true;
            ButtonInstall.Text = "开始安装";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 获取主窗口引用
            var MainWindow = this.ParentForm as MainWindow;
            if (MainWindow == null) return;

            // 确认对话框
            DialogResult result = MessageBox.Show("是否卸载Office？", "ZyperWin++", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // 冻结所有控件
                    SetControlsEnabled(false);

                    // 执行卸载脚本
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "wscript.exe",
                        Arguments = @".\Bin\UnInstallC2R.vbs",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process process = new Process();
                    process.StartInfo = psi;
                    process.EnableRaisingEvents = true;
                    process.Exited += (s, args) =>
                    {
                        // 在UI线程中恢复控件状态
                        this.Invoke(new Action(() =>
                        {
                            SetControlsEnabled(true);
                            MessageBox.Show("Office卸载完成！", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }));
                    };

                    process.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"卸载失败: {ex.Message}", "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // 发生异常时恢复控件状态
                    SetControlsEnabled(true);
                }
            }
        }

        /// <summary>
        /// 设置所有控件的启用状态
        /// </summary>
        private void SetControlsEnabled(bool enabled)
        {
            // 获取主窗口引用
            var MainWindow = this.ParentForm as MainWindow;
            
            // 冻结/解冻选择框和按钮
            select1.Enabled = enabled;
            select2.Enabled = enabled;
            select3.Enabled = enabled;
            ButtonInstall.Enabled = enabled;
            button1.Enabled = enabled;

            // 冻结/解冻主窗口菜单
            if (MainWindow != null && MainWindow.menu1 != null)
            {
                MainWindow.menu1.Enabled = enabled;
            }

            // 更新按钮文本
            if (enabled)
            {
                ButtonInstall.Text = "开始安装";
                button1.Text = "卸载Office";
            }
            else
            {
                ButtonInstall.Text = "执行中...";
                button1.Text = "卸载中...";
            }
        }
    }
}