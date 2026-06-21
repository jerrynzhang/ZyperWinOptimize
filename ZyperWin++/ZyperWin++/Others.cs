using System;
using System.Diagnostics;

namespace ZyperWin__
{
    /// <summary>
    /// 其他工具窗口：提供一些附加工具的快速入口（例如资源管理器相关工具、更新工具等）。
    /// </summary>
    public partial class Others : AntdUI.Window
    {
        /// <summary>
        /// 构造函数：初始化组件。
        /// </summary>
        public Others()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start(".\\Bin\\Explorer11\\右键");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start(".\\Bin\\Explorer11\\资源管理器");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start(".\\Bin\\Update");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Process.Start(".\\Bin\\Explorer11\\Win11DisableOrRestoreRoundedCorners.exe");
        }
    }
}
