using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ZyperWin__
{
    /// <summary>
    /// 主窗口类，负责程序主界面、菜单切换与单实例控制。
    /// 负责显示不同功能页面（如系统优化、垃圾清理、Office 管理等）。
    /// </summary>
    public partial class MainWindow : AntdUI.Window
    {
        private static Mutex mutex;
        protected override CreateParams CreateParams
        {
            get
            {
                if (DesignMode)
                {
                    return base.CreateParams;
                }
                CreateParams paras = base.CreateParams;
                paras.ExStyle |= 0x02000000;
                return paras;
            }
        }
        /// <summary>
        /// 启用或禁用左侧菜单（线程安全）。
        /// 在非 UI 线程调用时会通过 Invoke 切换到 UI 线程执行。
        /// </summary>
        /// <param name="enabled">是否启用菜单</param>
        public void SetMenuEnabled(bool enabled)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(SetMenuEnabled), enabled);
            }
            else
            {
                menu1.Enabled = enabled;
            }
        }
        /// <summary>
        /// 构造函数：检查单实例，初始化 DPI 缩放与组件，并显示主页。
        /// </summary>
        public MainWindow()
        {
            // 单实例检查
            if (!IsSingleInstance())
            {
                return;
            }

            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            InitializeComponent();

            // 显示主页
            ShowForm(() => new MainMenu());
        }

        /// <summary>
        /// 检查并阻止程序重复启动（通过全局 Mutex）。
        /// 如果检测到已有实例，会弹出提示并尝试关闭当前窗口。
        /// </summary>
        /// <returns>方法尝试创建互斥体，若已有其他实例存在则显示提示并触发关闭逻辑。</returns>
        private bool IsSingleInstance()
        {
            bool createdNew;
            mutex = new Mutex(true, "ZyperWin++_SingleInstance_Mutex", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("ZyperWin++ 已经在运行中！\n请不要重复启动程序。",
                    "ZyperWin++", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }
            return true;
        }

        private void menu1_SelectChanged(object sender, EventArgs e)
        {
            // 首先查看事件参数的实际类型和属性
            Console.WriteLine($"事件参数类型: {e.GetType().Name}");

            // 查看事件参数的所有属性
            foreach (var prop in e.GetType().GetProperties())
            {
                Console.WriteLine($"- {prop.Name}: {prop.PropertyType.Name}");
            }

            // 尝试常见的属性名
            TryCommonProperties(e);
        }

        private void TryCommonProperties(EventArgs e)
        {
            // 尝试不同的可能属性名
            string[] possibleProperties = { "Item", "MenuItem", "Value", "SelectedValue", "SelectedMenuItem", "Index" };

            foreach (var propName in possibleProperties)
            {
                var prop = e.GetType().GetProperty(propName);
                if (prop != null)
                {
                    var value = prop.GetValue(e);
                    if (value != null)
                    {
                        HandleMenuSelectionByValue(value, propName);
                        return;
                    }
                }
            }

            // 如果上面没找到，尝试通过索引获取
            TryGetByIndex(e);
        }

        private void HandleMenuSelectionByValue(object value, string propName)
        {
            if (value is int index)
            {
                // 如果是索引
                if (index >= 0 && index < menu1.Items.Count)
                {
                    string menuText = menu1.Items[index].Text;
                    HandleMenuSelection(menuText);
                }
            }
            else
            {
                // 如果是菜单项对象
                var textProp = value.GetType().GetProperty("Text");
                if (textProp != null)
                {
                    string menuText = textProp.GetValue(value).ToString();
                    HandleMenuSelection(menuText);
                }
            }
        }

        private void TryGetByIndex(EventArgs e)
        {
            // 尝试获取索引属性
            var indexProp = e.GetType().GetProperty("Index");
            if (indexProp != null)
            {
                int index = (int)indexProp.GetValue(e);
                if (index >= 0 && index < menu1.Items.Count)
                {
                    string menuText = menu1.Items[index].Text;
                    HandleMenuSelection(menuText);
                    return;
                }
            }

            MessageBox.Show("无法获取选中项，请查看输出窗口的属性信息");
        }

        private void HandleMenuSelection(string menuText)
        {
            switch (menuText)
            {
                case "主页":
                    ShowForm(() => new MainMenu());
                    break;

                case "系统优化":
                    ShowForm(() => new Optimize());
                    break;

                case "还原选项":
                    ShowForm(() => new Backup());
                    break;

                case "垃圾清理":
                    ShowForm(() => new Clean());
                    break;

                case "Office管理":
                    ShowForm(() => new Office());
                    break;

                case "系统激活":
                    ShowForm(() => new Activate());
                    break;

                case "Appx管理":
                    ShowForm(() => new Appx());
                    break;

                case "关于软件":
                    ShowForm(() => new About());
                    break;

                case "安全中心":
                    ShowForm(() => new Defender());
                    break;

                case "疑难解答":
                    ShowForm(() => new question());
                    break;
            }
        }

        /// <summary>
        /// 在主窗口的右侧面板中显示指定的控件（通常是一个功能页面）。
        /// 会清空面板并将新的控件以 DockStyle.Fill 添加进去。
        /// </summary>
        /// <param name="createControl">用于创建要显示控件的工厂方法</param>
        private void ShowForm(Func<Control> createControl)
        {
            panel1.SuspendLayout();
            panel1.Controls.Clear();

            // 创建新的控件实例
            Control formControl = createControl();

            // 设置控件属性
            formControl.Dock = DockStyle.Fill;
            panel1.Controls.Add(formControl);
            panel1.ResumeLayout();
        }
    }
}