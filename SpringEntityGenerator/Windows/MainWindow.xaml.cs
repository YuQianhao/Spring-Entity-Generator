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
using System.Windows.Input;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using SpringEntityGenerator.Generators;
using SpringEntityGenerator.Models;
using SpringEntityGenerator.Utils;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SpringEntityGenerator.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// 当前项目
        /// </summary>
        private Project project;

        // 上一次保存的项目结构
        private Project lastSaveProject = new();

        public MainWindow(Project project)
        {
            this.project = project;
            InitializeComponent();
            KeyDown += MainWindow_KeyDown;
            ReloadConfigs();
        }

        /// <summary>
        /// 检查是否需要保存
        /// </summary>
        private bool CheckNeedSave()
        {
            return !Json.Serialize(lastSaveProject).Equals(Json.Serialize(project));
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Key key = e.Key;
            ModifierKeys modifiers = Keyboard.Modifiers;

            if (modifiers == ModifierKeys.Control && key == Key.N)
            {
                CreateNewTable();
            }
            else if (modifiers == ModifierKeys.Control && key == Key.O)
            {
                ReloadConfigs();
            }
            else if (modifiers == ModifierKeys.Control && key == Key.S)
            {
                SaveCurrent();
            }
            else if (modifiers == ModifierKeys.Control && key == Key.P)
            {
                SaveCurrent();
            }
            else if (key == Key.F8)
            {
                StartGeneratorCode();
            }
        }

        /// <summary>
        /// 查询Table对象
        /// </summary>
        /// <returns>Table对象</returns>
        private Project GetProject()
        {
            return project;
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataGridColumn.Height = e.NewSize.Height - 135;
        }

        // <summary>
        /// 将当前的Table更新为空的表格
        /// </summary>
        private void RefreshTable()
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
                                Comment = "主键ID",
                                AllowSetField = false
                            }
                        }
            };
        }

        private void MainWindow_OnInitialized(object? sender, EventArgs e)
        {
            RefreshTable();
            ProjectConfigPanel.DataContext = project;
        }

        /// <summary>
        /// 保存当前正在编辑的生成器配置
        /// </summary>
        private bool SaveCurrent()
        {
            if (project.Table.Name.Length == 0)
            {
                MessageBox.Show("请先设置表名。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            SetStatusMessage("正在保存");
            File.WriteAllText(project.SrcPath + "//" + GetProject().Table.Name + ".seg", Json.Serialize(GetProject().Table));
            if (!project.TableNameFiles.Contains(project.Table.Name))
            {
                project.TableNameFiles.Add(GetProject().Table.Name);
            }
            ResetLastSaveProject();
            SetStatusMessage("保存完成");
            return true;
        }

        /// <summary>
        /// 重新设置上一次保存的项目对象
        /// </summary>
        private void ResetLastSaveProject()
        {
            // 深拷贝给lastSaveProject
            var deepCopyProject = Json.Deserialize<Project>(Json.Serialize(project));
            if (deepCopyProject != null)
            {
                lastSaveProject = deepCopyProject;
            }
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveCurrent())
            {
                MessageBox.Show("保存成功", "Spring Entity Generator", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// 开始生成代码
        /// </summary>
        private void StartGeneratorCode()
        {
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
            SetStatusMessage("正在编译生成代码，请稍后");
            MenuItemGenerator.IsEnabled = false;
            MenuItemGenerator.Header = "生成中，请稍后";
            string generatorInfo = "";
            try
            {
                if (MenuItemSettingMySql.IsChecked)
                {
                    new MySqlGenerator().Generator(project);
                    generatorInfo += "MySQL数据库，";
                }
                if (MenuItemSettingController.IsChecked)
                {
                    new JavaControllerGenerator().Generator(project);
                    generatorInfo += "Java控制器，";
                }
                if (MenuItemSettingEntity.IsChecked)
                {
                    new JavaEntityGenerator().Generator(project);
                    generatorInfo += "JavaEntity，";
                }
                if (MenuItemSettingMapper.IsChecked)
                {
                    new JavaMapperGenerator().Generator(project);
                    generatorInfo += "JavaMapper，";
                }
                if (MenuItemSettingService.IsChecked)
                {
                    new JavaServiceGenerator().Generator(project);
                    generatorInfo += "JavaService，";
                }
                if (MenuItemSettingDocument.IsChecked)
                {
                    new DocumentGenerator().Generator(project);
                    generatorInfo += "数据库字典，";
                }
                // 写入本地临时文件
                if (!Directory.Exists("./tmp"))
                {
                    Directory.CreateDirectory("./tmp");
                }
                File.WriteAllText(project.SrcPath + "\\" + GetProject().Table.Name + ".seg", Json.Serialize(GetProject().Table));
                MessageBox.Show($"生成{generatorInfo[..^1]}成功", "Spring Entity Generator", MessageBoxButton.OK);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            MenuItemGenerator.IsEnabled = true;
            MenuItemGenerator.Header = "生成";
            SetStatusMessage($"生成{generatorInfo[..^1]}成功");
        }

        private void MenuItemGenerator_Click(object sender, RoutedEventArgs e)
        {
            StartGeneratorCode();
        }

        /// <summary>
        /// 创建一个新的Table表
        /// </summary>
        private void CreateNewTable()
        {
            if (CheckNeedSave())
            {
                if (MessageBox.Show("确定要新建一个表结构吗？新建表结构会清空当前的表格。。", "Spring Entity Generator", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                MessageBoxResult.Yes)
                {
                    RefreshTable();
                }
            }
            else
            {
                RefreshTable();
            }
        }

        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTable();
        }

        /// <summary>
        /// 加载指定路径的Table
        /// </summary>
        /// <param name="path"></param>
        private void LoadTableConfig(string path)
        {
            var table = Json.Deserialize<EntityTable>(File.ReadAllText(path));
            if (table == null || project == null)
            {
                MessageBox.Show("打开失败。这个文件不是Spring Entity Generator的表结构配置文件。", "Spring Entity Generator", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                project.Table = table;
                ResetLastSaveProject();
            }
        }

        private void ListBoxTable_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!CheckNeedSave())
            {
                // 打开双击项
                var tableName = ListBoxTable.SelectedItem;
                LoadTableConfig(project.SrcPath + "\\" + tableName + ".seg");
                return;
            }
            var result = MessageBox.Show("是否需要保存当前的更改？", "Spring Entity Generator", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result != MessageBoxResult.Cancel)
            {
                if (result == MessageBoxResult.Yes)
                {
                    // 保存
                    SaveCurrent();
                }
                // 打开双击的项
                var tableName = ListBoxTable.SelectedItem;
                LoadTableConfig(project.SrcPath + "\\" + tableName + ".seg");
            }
        }

        /// <summary>
        /// 从本地src路径中重新加载所有的配置
        /// </summary>
        private void ReloadConfigs()
        {
            SetStatusMessage("正在重新加载");
            string[] files = Directory.GetFiles(project.SrcPath, "*.seg");
            project.TableNameFiles.Clear();
            foreach (var filePath in files)
            {
                project.TableNameFiles.Add(new FileInfo(filePath).Name.Split(".")[0]);
            }
            SetStatusMessage("加载表结构配置完成。");
            // 加载完成后，如果存在配置文件，就加载第一个，如果不存在，就创建新的配置文件
            if (project.TableNameFiles.Count > 0)
            {
                LoadTableConfig(project.SrcPath + "\\" + project.TableNameFiles[0] + ".seg");
            }
            else
            {
                RefreshTable();
            }
        }

        private void MenuItemReload_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckNeedSave())
            {
                // 重新加载表结构
                ReloadConfigs();
                return;
            }
            var result = MessageBox.Show("是否需要保存当前的更改？", "Spring Entity Generator", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result != MessageBoxResult.Cancel)
            {
                if (result == MessageBoxResult.Yes)
                {
                    // 保存
                    if(!SaveCurrent())
                    {
                        return;
                    }
                }
                // 重新加载表结构
                ReloadConfigs();
            }
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 修改状态栏的信息
        /// </summary>
        private void SetStatusMessage(string message)
        {
            StatusItemMessage.Content = message;
            StatusItemDate.Content = DateTime.Now.ToString("G");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CheckNeedSave() && MessageBox.Show("确定要退出吗？退出后未保存的更改不会主动保存。", "Spring Entity Generator", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void ButtonDeleteTable_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(project.Table.Name))
            {
                MessageBox.Show("没有要删除的表结构名称。", "删除表结构", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"确定要删除表结构{project.Table.Name}吗？删除表结构只会删除本地的配置文件，不会影响已经生成的代码和字典。", "删除表结构", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                SetStatusMessage("正在删除表结构");
                File.Delete(project.SrcPath + "\\" + project.Table.Name + ".seg");
                SetStatusMessage("删除表结构成功");
                ReloadConfigs();
            }
        }

        private void MenuItemEditProjectSetting_Click(object sender, RoutedEventArgs e)
        {
            if(!CheckNeedSave())
            {
                ProjectWindow projectWindow = new ProjectWindow(project);
                projectWindow.ShowDialog();
                if(projectWindow.Result!=null)
                {
                    project=projectWindow.Result;
                    ResetLastSaveProject();
                }
            }
            else
            {
                MessageBox.Show("您有未保存的工作，请保存后再尝试调整项目设置。", "修改项目设置", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
