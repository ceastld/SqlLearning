using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FreeSql;
using FreeSql.DataAnnotations;
using PropertyChanged;

namespace SqlInWPF
{
    using static Database;
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = ViewModel;
        }

        public MainWindowViewModel ViewModel = new MainWindowViewModel();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button btn && btn.DataContext is Topic tp)
            {
                tp.Clicks++;
            }
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class MainWindowViewModel : DependencyObject
    {
        public MainWindowViewModel()
        {
            var list = fsql.Select<Topic>()
                .Where(x => true)
                .ToList();

            TopicList = new ObservableCollection<Topic>(list);

            var tp = list.First();
            if (tp == null)
            {
                tp = new Topic() { Clicks = 10 };
                tp.Save();
            }

            SelectedItem = list.First();
            //next 添加了过后，立马保存
        }

        public ObservableCollection<Topic> TopicList { get; set; }


        public Topic SelectedItem
        {
            get { return (Topic)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(Topic), typeof(MainWindowViewModel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(SelectionChangedCallback)));

        private static void SelectionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (e.OldValue as Topic)?.Close();
            (e.NewValue as Topic)?.Open();
        }


    }
    public static class Database
    {
        public static readonly IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, "data source=123.db")
                .UseAutoSyncStructure(true) //自动同步实体结构到数据库
                .Build();

        public static void CloseDatabase()
        {
            fsql.Dispose();
        }
    }

    public class Topic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public int Clicks { get; set; }
        public int CCCC => Clicks + 10;
        public string Title { get; set; }
        public DateTime CreateTime { get; set; }

        public static IBaseRepository<Topic> Repo = fsql.GetRepository<Topic>();

        public void Delete() => Repo.Delete(this);
        public void Save() => Repo.InsertOrUpdate(this);

        private int _opencount = 0;
        public void Open()
        {
            if (_opencount == 0)
            {
                Repo.Attach(this);
                this.PropertyChanged += Topic_PropertyChanged;
            }
            _opencount++;
        }
        public void Close()
        {
            _opencount = Math.Max(0, _opencount - 1);
            if (_opencount == 0)
            {
                PropertyChanged -= Topic_PropertyChanged;
            }
        }

        private void Topic_PropertyChanged(object sender, PropertyChangedEventArgs e) => Repo.Update(this);
    }
}
