using AntdUI;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ZyperWin__
{
    /// <summary>
    /// 备份/还原配置页面：用于列出存放于 Config 文件夹中的 INI 配置文件，便于管理和还原。
    /// </summary>
    public partial class Backup : UserControl
    {
        /// <summary>
        /// 构造函数：初始化界面并加载配置文件列表到树视图。
        /// </summary>
        public Backup()
        {
            InitializeComponent();
            LoadIniFileListToTree();
        }
        /// <summary>
        /// 扫描 `Config` 文件夹，加载所有 INI 文件并按时间排序显示到 tree1。
        /// 若目录或文件不存在，会在界面中给出提示信息。
        /// </summary>
        private void LoadIniFileListToTree()
        {
            try
            {
                tree1.Items.Clear(); // 清空现有项

                // 确保Config文件夹存在
                string ConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
                if (!Directory.Exists(ConfigFolder))
                {
                    Directory.CreateDirectory(ConfigFolder);
                    tree1.Items.Add(new TreeItem("配置文件文件夹为空"));
                    return;
                }

                // 获取所有INI文件
                var iniFiles = Directory.GetFiles(ConfigFolder, "*.ini");

                if (iniFiles.Length == 0)
                {
                    tree1.Items.Add(new TreeItem("没有找到INI配置文件"));
                    return;
                }

                // 创建根节点
                var ConfigNode = new TreeItem("配置文件列表") { Tag = "Config_root" };

                // 按修改时间倒序排列
                var sortedFiles = iniFiles.OrderByDescending(f => File.GetLastWriteTime(f)).ToArray();

                // 添加每个INI文件作为子节点
                foreach (var filePath in sortedFiles)
                {
                    string fileName = Path.GetFileName(filePath);
                    string fileTime = File.GetLastWriteTime(filePath).ToString("yyyy-MM-dd HH:mm:ss");
                    long fileSize = new FileInfo(filePath).Length;

                    // 创建文件节点，显示文件名、大小和修改时间
                    var fileNode = new TreeItem($"{fileName} ({FormatFileSize(fileSize)}) - {fileTime}")
                    {
                        Tag = filePath, // 存储完整路径
                        Checked = false
                    };

                    ConfigNode.Sub.Add(fileNode);
                }

                tree1.Items.Add(ConfigNode);
                tree1.ExpandAll(); // 展开所有节点
            }
            catch (Exception ex)
            {
                tree1.Items.Add(new TreeItem($"加载文件列表失败: {ex.Message}"));
            }
        }

        // 格式化文件大小显示
        /// <summary>
        /// 将字节数格式化为可读的字符串（B/KB/MB/GB）。
        /// </summary>
        /// <param name="bytes">文件大小（字节）</param>
        /// <returns>格式化后的大小字符串</returns>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }
}
