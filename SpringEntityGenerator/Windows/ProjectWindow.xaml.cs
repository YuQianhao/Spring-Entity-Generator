using Microsoft.Win32;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
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
    /// ProjectSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectWindow : Window
    {

        private readonly Project project = new();

        // 是否是编辑模式
        private bool editMode = false;

        // 创建项目的状态
        private bool createProjectStatus = false;

        // 保存时的返回值
        public Project? Result;

        public ProjectWindow(Project? editProject=null)
        {
            InitializeComponent();
            if(editProject!=null)
            {
                project=editProject;
                editMode = true;
            }
            MainPanel.DataContext = project;
        }

        /// <summary>
        /// 检查设置是否正确
        /// </summary>
        private bool CheckSetting()
        {
            if (string.IsNullOrEmpty(project.Name))
            {
                ShowWarninngDialog("请设置项目名称。");
                return false;
            }
            if (string.IsNullOrEmpty(project.Path))
            {
                ShowWarninngDialog("请选择项目路径。");
                return false;
            }
            if (string.IsNullOrEmpty(project.PackageName))
            {
                ShowWarninngDialog("请设置项目包名。");
                return false;
            }
            if (string.IsNullOrEmpty(project.MySql.Host))
            {
                ShowWarninngDialog("请设置数据库Host。");
                return false;
            }
            if (string.IsNullOrEmpty(project.MySql.Port.ToString()))
            {
                ShowWarninngDialog("请设置数据库端口号。");
                return false;
            }
            if (string.IsNullOrEmpty(project.MySql.User))
            {
                ShowWarninngDialog("请设置数据库用户名。");
                return false;
            }
            if (string.IsNullOrEmpty(project.MySql.Password))
            {
                ShowWarninngDialog("请设置数据库密码。");
                return false;
            }
            if (string.IsNullOrEmpty(project.MySql.Databases))
            {
                ShowWarninngDialog("请设置数据库名称。");
                return false;
            }
            if (string.IsNullOrEmpty(project.PageFieldName))
            {
                ShowWarninngDialog("请设置分页页码字段。");
                return false;
            }
            if (string.IsNullOrEmpty(project.PageSizeFieldName))
            {
                ShowWarninngDialog("请设置分页数据长度字段。");
                return false;
            }
            return true;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if(!CheckSetting())
            {
                return;
            }
            // 创建项目目录
            if (!Directory.Exists(project.SrcPath))
            {
                Directory.CreateDirectory(project.SrcPath);
            }
            if (!Directory.Exists(project.DocumentPath))
            {
                Directory.CreateDirectory(project.DocumentPath);
            }
            var projectConfigFilePath = project.GeneratorConfigPath + "\\" + Project.ProjectFileName;
            if(File.Exists(projectConfigFilePath) && !editMode)
            {
                MessageBox.Show("这个Spring Boot工程已经存在一个Spring Entity Gneerator项目了，不能再次创建。","新建项目",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            File.WriteAllText(projectConfigFilePath, Json.Serialize(project));
            if(!editMode)
            {
                createProjectStatus = true;
                // 打开编辑窗口
                MainWindow mainWindow = new MainWindow(project);
                GetWindow(this).Close();
                mainWindow.Show();
            }
            else
            {
                Result = project;
                GetWindow(this).Close();
            }
        }

        private void ButtonSelectPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Maven项目|pom.xml|Gradle项目|build.gradle";
            openFileDialog.Title = "选择Java项目POM、build.gradle文件";
            if (openFileDialog.ShowDialog() == true)
            {
                var selectPath = new FileInfo(openFileDialog.FileName).DirectoryName;
                if (selectPath != null)
                {
                    project.Path = selectPath;
                    project.GeneratorConfigPath = selectPath + "\\src\\main\\generator\\SpringEntityGenerator\\";
                    project.SrcPath = selectPath + "\\src\\main\\generator\\SpringEntityGenerator\\src\\";
                    project.DocumentPath = selectPath + "\\src\\main\\generator\\SpringEntityGenerator\\document\\";
                }
            }
        }

        private void ShowWarninngDialog(string message)
        {
            MessageBox.Show(message, "SpringEntityGenerator", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ButtonTestDataBase_Click(object sender, RoutedEventArgs e)
        {
            if(!CheckSetting())
            {
                return;
            }
            try 
            {
                var mySqlConnection = new MySqlConnection(
                    $"server = {project.MySql.Host}; port = {project.MySql.Port}; database = {project.MySql.Databases}; user = {project.MySql.User}; password = {project.MySql.Password}");
                mySqlConnection.Open();
                mySqlConnection.Close();
                MessageBox.Show("数据库连接成功。", "Spring Entity Generator", MessageBoxButton.OK);
            }catch(Exception error)
            {
                MessageBox.Show(error.Message, "数据库连接测试", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!editMode && !createProjectStatus)
            {
                WelcomeWindow welcomeWindow = new WelcomeWindow();
                welcomeWindow.Show();
            }
        }
    }
}
