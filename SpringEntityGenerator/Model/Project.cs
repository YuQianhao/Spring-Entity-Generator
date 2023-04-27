using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpringEntityGenerator.Model
{
    /// <summary>
    /// 生成项目
    /// </summary>
    public class Project :INotifyPropertyChanged
    {

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

        private string _path = "";

        /// <summary>
        /// 项目路径
        /// </summary>
        public string Path
        {
            get => _path;
            set { _path = value;OnPropertyChanged("Path"); }
        }

        /// <summary>
        /// 自动备份旧的文件
        /// </summary>
        public bool AutoBackup { get; set; } = true;

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

        /// <summary>
        /// 包名
        /// </summary>
        public string PackageName { get; set; } = "";

        /// <summary>
        /// 数据表结构
        /// </summary>
        public EntityTable Table { get; set; } = new();

        /// <summary>
        /// MySql连接
        /// </summary>
        public MySqlConnection MySql { get; set; } = new();

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
