using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpringEntityGenerator.Models
{
    /// <summary>
    /// 生成项目
    /// </summary>
    public class Project :INotifyPropertyChanged
    {

        /// <summary>
        /// 项目文件名称
        /// </summary>
        public const string ProjectFileName = "project.segp";

        public class MySqlConnection:INotifyPropertyChanged
        {

            public string Host { get; set; } = "localhost";

            public int Port { get; set; } = 3306;

            public string User { get; set; } = "root";

            public string Password { get; set; } = "";

            public string Databases { get; set; } = "";

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }

        /// <summary>
        /// 版本名称
        /// </summary>
        public int VersionCode { get; } = 1;

        private string _name = "";
        /// <summary>
        /// 项目名称
        /// </summary>
        public string Name 
        {
            get => _name;
            set { _name = value; OnPropertyChanged("Name"); }
        }

        private string _path = "";
        /// <summary>
        /// 项目路径
        /// </summary>
        public string Path
        {
            get => _path;
            set { _path = value;OnPropertyChanged("Path"); }
        }

        private bool _autoBackup = true;
        /// <summary>
        /// 自动备份旧的文件
        /// </summary>
        public bool AutoBackup 
        {
            get => _autoBackup;
            set { _autoBackup = value; OnPropertyChanged("AutoBackup"); }
        }

        private bool _uppercase = true;
        /// <summary>
        /// 字段名大写
        /// </summary>
        public bool Uppercase
        {
            get => _uppercase;
            set { _uppercase = value; OnPropertyChanged("Uppercase"); }
        }

        private string _prefix = "";
        /// <summary>
        /// 数据库和字段名前缀
        /// </summary>
        public string Prefix
        {
            get => _prefix;
            set { _prefix = value; OnPropertyChanged("Prefix"); }
        }

        private string _pageFieldName="page";
        /// <summary>
        /// 分页页码字段
        /// </summary>
        public string PageFieldName
        {
            get => _pageFieldName;
            set { _pageFieldName = value;OnPropertyChanged("_pageFieldName"); }
        }

        private string _pageSizeFieldName = "pageSize";
        /// <summary>
        /// 分页数据长度字段
        /// </summary>
        public string PageSizeFieldName
        {
            get => _pageSizeFieldName;
            set { _pageSizeFieldName = value; OnPropertyChanged("_pageSizeFieldName"); }
        }

        private string _documentPath="";
        /// <summary>
        /// 文档路径
        /// </summary>
        public string DocumentPath
        {
            get => _documentPath;
            set { _documentPath = value;OnPropertyChanged("DocumentPath"); }
        }

        private string _srcPath = "";
        /// <summary>
        /// 配置源文件存放路径
        /// </summary>
        public string SrcPath
        {
            get => _srcPath;
            set { _srcPath = value; OnPropertyChanged("SrcPath"); }
        }

        /// <summary>
        /// 生成器配置文件路径
        /// </summary>
        public string GeneratorConfigPath { get; set; } = "";

        /// <summary>
        /// 包名
        /// </summary>
        public string PackageName { get; set; } = "";


        private EntityTable _table=new EntityTable();
        /// <summary>
        /// 数据表结构
        /// </summary>
        public EntityTable Table
        {
            get => _table;
            set { _table = value; OnPropertyChanged("Table"); }
        }

        /// <summary>
        /// MySql连接
        /// </summary>
        public MySqlConnection MySql { get; set; } = new();

        /// <summary>
        /// 关联的表结构文件名
        /// </summary>
        public ObservableCollection<string> TableNameFiles { get; set; } = new ObservableCollection<string>();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
