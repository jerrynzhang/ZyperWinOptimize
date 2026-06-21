using System;
using System.Windows.Forms;

namespace ZyperWin__
{
    /// <summary>
    /// ZyperWin++ 应用程序入口类
    /// 一款Windows系统优化工具，提供系统优化、垃圾清理、Defender管理、
    /// Office安装、系统激活、Appx管理等功能
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// 启用视觉样式并启动主窗口 MainWindow
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // 启用Windows视觉样式（让控件使用现代Windows外观）
            Application.EnableVisualStyles();
            // 设置文本渲染兼容性（使用GDI+而非GDI渲染）
            Application.SetCompatibleTextRenderingDefault(false);
            // 启动主窗口（基于AntdUI的现代化界面）
            Application.Run(new MainWindow());
        }
    }
}