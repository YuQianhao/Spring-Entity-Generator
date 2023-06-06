using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SpringEntityGenerator.generator;
using SpringEntityGenerator.Model;
using SpringEntityGenerator.Utils;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SpringEntityGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 查询Table对象
        /// </summary>
        /// <returns>Table对象</returns>
        private Project GetProject()
        {
            return (Project)ProjectConfigPanel.DataContext;
        }

        private void UpdateProjectConfig(Project project)
        {
            ProjectConfigPanel.DataContext = project;
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataGridColumn.Height = e.NewSize.Height - 740 + 320;
        }

        private void MainWindow_OnInitialized(object? sender, EventArgs e)
        {
            var project = new Project()
            {
                Table = new EntityTable()
                {
                    Columns = new List<Column>
                {
                    new()
                    {
                        Name = "id",
                        Key = true,
                        AutoIncrease = true,
                        CnName = "主键ID",
                        Comment = "主键ID"
                    }
                }
                }
            };
            ProjectConfigPanel.DataContext = project;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var project = GetProject();
            if (project.Table.Name.Length == 0)
            {
                MessageBox.Show("请先设置表名。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "选择文档存放路径";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(folderBrowserDialog.SelectedPath + "//" + GetProject().Table.Name + ".seg", Json.Serialize(GetProject().Table));
                MessageBox.Show("保存成功", "Spring Entity Generator", MessageBoxButton.OK);
            }
        }

        private void ButtonGenerator_Click(object sender, RoutedEventArgs e)
        {
            var project = GetProject();
            if (string.IsNullOrEmpty(project.Path))
            {
                MessageBox.Show("项目路径不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(project.PackageName))
            {
                MessageBox.Show("项目包名不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(project.DocumentPath))
            {
                MessageBox.Show("数据库字典路径不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(project.MySql.Host))
            {
                MessageBox.Show("数据库Host不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (project.MySql.Port < 0)
            {
                MessageBox.Show("数据库端口号格式不正确。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(project.MySql.User))
            {
                MessageBox.Show("数据库用户不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(project.MySql.Password))
            {
                MessageBox.Show("数据库密码不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(project.MySql.Databases))
            {
                MessageBox.Show("数据库名称不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(project.Table.Name))
            {
                MessageBox.Show("Entity名称不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(project.Table.CnName))
            {
                MessageBox.Show("Entity中文名称不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (project.Table.Columns.Count == 0)
            {
                MessageBox.Show("至少得有一个字段。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                if (project.Table.Columns.Find(item => item.Name.Equals("id") && item is { Key: true, AutoIncrease: true }) == null)
                {
                    MessageBox.Show("表结构中至少需要一个名为id的自增主键。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            if (project.Table.Columns.Find(item => string.IsNullOrEmpty(item.Name)) != null)
            {
                MessageBox.Show("表结构中不能有空名称的字段。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (project.Table.Columns.Find(item => item.SelectEqual && item.SelectRange) != null)
            {
                MessageBox.Show("表结构的字段存在既能用于相等匹配和范围匹配的字段。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ButtonGenerator.IsEnabled = false;
            ButtonGenerator.Content = "生成中，请稍后";
            try
            {
                string generatorInfo = "";
                switch (ComboBoxGeneratorType.SelectedIndex)
                {
                    case 0:
                        {
                            new MySqlGenerator().Generator(project);
                            new JavaEntityGenerator().Generator(project);
                            new JavaMapperGenerator().Generator(project);
                            new JavaServiceGenerator().Generator(project);
                            new JavaControllerGenerator().Generator(project);
                            new DocumentGenerator().Generator(project);
                            generatorInfo = "数据库，JavaEntity，Java控制器，JavaMapper，JavaService，Document";
                        }
                        break;
                    case 1:
                        {
                            new MySqlGenerator().Generator(project);
                            generatorInfo = "数据库";
                        }
                        break;
                    case 2:
                        {
                            new JavaControllerGenerator().Generator(project);
                            generatorInfo = "Java控制器";
                        }
                        break;
                    case 3:
                        {
                            new JavaEntityGenerator().Generator(project);
                            generatorInfo = "JavaEntity";
                        }
                        break;
                    case 4:
                        {
                            new JavaMapperGenerator().Generator(project);
                            generatorInfo = "JavaMapper";
                        }
                        break;
                    case 5:
                        {
                            new JavaServiceGenerator().Generator(project);
                            generatorInfo = "JavaService";
                        }
                        break;
                    case 6:
                        {
                            new DocumentGenerator().Generator(project);
                            generatorInfo = "Document";
                        }
                        break;
                }
                // 写入本地临时文件
                if (!Directory.Exists("./tmp"))
                {
                    Directory.CreateDirectory("./tmp");
                }
                File.WriteAllText("./tmp/" + GetProject().Table.Name+$".{Guid.NewGuid().ToString()}" + ".seg", Json.Serialize(GetProject().Table));
                MessageBox.Show($"生成{generatorInfo}成功", "Spring Entity Generator", MessageBoxButton.OK);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ButtonGenerator.IsEnabled = true;
            ButtonGenerator.Content = "生成";
        }

        private void ButtonOpenProject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Maven项目|pom.xml|Gradle项目|build.gradle";
            openFileDialog.Title = "选择Java项目POM、build.gradle文件";
            if (openFileDialog.ShowDialog() == true)
            {
                var selectPath = new FileInfo(openFileDialog.FileName).DirectoryName;
                if (selectPath != null)
                {
                    GetProject().Path = selectPath;
                }
            }
        }

        private void ButtonSelectDocumentPath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "选择文档存放路径";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                GetProject().DocumentPath = folderBrowserDialog.SelectedPath;
            }
        }

        private void ButtonTestMySql_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var project = GetProject();
                if (string.IsNullOrEmpty(project.MySql.Host))
                {
                    MessageBox.Show("数据库Host不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (project.MySql.Port < 0)
                {
                    MessageBox.Show("数据库端口号格式不正确。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(project.MySql.User))
                {
                    MessageBox.Show("数据库用户不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(project.MySql.Password))
                {
                    MessageBox.Show("数据库密码不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(project.MySql.Databases))
                {
                    MessageBox.Show("数据库名称不能是空的。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var mySqlConnection = new MySqlConnection(
                    $"server = {project.MySql.Host}; port = {project.MySql.Port}; database = {project.MySql.Databases}; user = {project.MySql.User}; password = {project.MySql.Password}");
                mySqlConnection.Open();
                mySqlConnection.Close();
                MessageBox.Show("数据库连接成功。", "Spring Entity Generator", MessageBoxButton.OK);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要新建一个吗？新建将会清空当前的内容。", "Spring Entity Generator", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                MessageBoxResult.Yes)
            {
                var project = Json.Deserialize<Project>(Json.Serialize(GetProject()));
                if (project != null)
                {
                    project.Table = new EntityTable()
                    {
                        Columns = new List<Column>
                        {
                            new()
                            {
                                Name = "id",
                                Key = true,
                                AutoIncrease = true,
                                CnName = "主键ID",
                                Comment = "主键ID"
                            }
                        }
                    };
                    ProjectConfigPanel.DataContext = project;
                }
            }
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("打开一个新的配置脚本将会丢失当前的工作进度。确定要打开新的吗？", "Spring Entity Generator", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "seg文件|*.seg",
                    Multiselect = false,
                    Title = "打开Spring Entity Generator文件"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    var project = Json.Deserialize<Project>(Json.Serialize(GetProject()));
                    var table = Json.Deserialize<EntityTable>(File.ReadAllText(openFileDialog.FileName));
                    if (table == null || project == null)
                    {
                        MessageBox.Show("打开失败。这个文件不是Spring Entity Generator的表结构配置文件。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        project.Table = table;
                        ProjectConfigPanel.DataContext = project;
                    }
                }
            }
        }
    }
}
