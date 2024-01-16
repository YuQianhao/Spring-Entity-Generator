using Microsoft.Win32;
using SpringEntityGenerator.Models;
using SpringEntityGenerator.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpringEntityGenerator.Windows
{
    /// <summary>
    /// WelcomeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();
        }

        private void ButtonCreateNewProject_Click(object sender, RoutedEventArgs e)
        {
            // 创建新项目
            ProjectWindow projectWindow = new ProjectWindow();
            GetWindow(this).Close();
            projectWindow.Show();
        }

        private void ButtonOpenProject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "segp|*.segp";
            openFileDialog.Title = "选择Spring Entity Generator工程文件";
            if (openFileDialog.ShowDialog() == true)
            {
                Project? project = Json.Deserialize<Project>(File.ReadAllText(openFileDialog.FileName));
                if (project == null)
                {
                    MessageBox.Show("工程文件格式解析失败，无法正常使用。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if(project.VersionCode>new Project().VersionCode)
                    {
                        MessageBox.Show("无法打开更改版本的工程文件。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    MainWindow mainWindow = new MainWindow(project);
                    GetWindow(this).Close();
                    mainWindow.Show();
                }
            }
        }
    }
}
