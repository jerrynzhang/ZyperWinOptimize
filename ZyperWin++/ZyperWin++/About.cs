using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ZyperWin__
{
    /// <summary>
    /// 关于页面：展示作者与项目链接，提供跳转到相关网页的按钮。
    /// </summary>
    public partial class About : UserControl
    {
        /// <summary>
        /// 构造函数：初始化界面组件。
        /// </summary>
        public About()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.bilibili.com/opus/1054761358514454535");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("https://space.bilibili.com/1645147838");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/ZyperWave/ZyperWinOptimize");
        }
    }
}
