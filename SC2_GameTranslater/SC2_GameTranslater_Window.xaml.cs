﻿using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fluent;
using Fluent.Localization;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Threading;
using SC2_GameTranslater.Source;
using System.Data;

namespace SC2_GameTranslater
{
    using Globals = Source.Class_Globals;
    using Preference = Source.Class_Preference;
    using Log = Source.Class_Log;
    using EnumLanguage = Source.EnumLanguage;

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SC2_GameTranslater_Window : RibbonWindow
    {
        #region 声明

        /// <summary>
        /// 进度条函数委托
        /// </summary>
        /// <param name="count">当前计数</param>
        /// <param name="max">最大计数</param>
        public delegate void Delegate_ProgressEvent(double count, double max);

        #endregion

        #region 命令

        /// <summary>
        /// 打开命令依赖项
        /// </summary>
        public static DependencyProperty CommandOpenProperty = DependencyProperty.Register(nameof(CommandOpen), typeof(RoutedUICommand), typeof(SC2_GameTranslater_Window), new PropertyMetadata(new RoutedUICommand()));

        /// <summary>
        /// 打开命令依赖项属性
        /// </summary>
        public RoutedUICommand CommandOpen { set => SetValue(CommandOpenProperty, value); get => (RoutedUICommand)GetValue(CommandOpenProperty); }
                
        /// <summary>
        /// 保存命令依赖项
        /// </summary>
        public static DependencyProperty CommandSaveProperty = DependencyProperty.Register(nameof(CommandSave), typeof(RoutedUICommand), typeof(SC2_GameTranslater_Window), new PropertyMetadata(new RoutedUICommand()));

        /// <summary>
        /// 保存命令依赖项属性
        /// </summary>
        public RoutedUICommand CommandSave { set => SetValue(CommandSaveProperty, value); get => (RoutedUICommand)GetValue(CommandSaveProperty); }

        /// <summary>
        /// 另存为命令依赖项
        /// </summary>
        public static DependencyProperty CommandSaveAsProperty = DependencyProperty.Register(nameof(CommandSaveAs), typeof(RoutedUICommand), typeof(SC2_GameTranslater_Window), new PropertyMetadata(new RoutedUICommand()));

        /// <summary>
        /// 另存为命令依赖项属性
        /// </summary>
        public RoutedUICommand CommandSaveAs { set => SetValue(CommandSaveAsProperty, value); get => (RoutedUICommand)GetValue(CommandSaveAsProperty); }
        
        /// <summary>
        /// 应用命令依赖项
        /// </summary>
        public static DependencyProperty CommandAcceptProperty = DependencyProperty.Register(nameof(CommandAccept), typeof(RoutedUICommand), typeof(SC2_GameTranslater_Window), new PropertyMetadata(new RoutedUICommand()));

        /// <summary>
        /// 应用命令依赖项属性
        /// </summary>
        public RoutedUICommand CommandAccept { set => SetValue(CommandAcceptProperty, value); get => (RoutedUICommand)GetValue(CommandAcceptProperty); }

        /// <summary>
        /// 关闭命令依赖项
        /// </summary>
        public static DependencyProperty CommandCloseProperty = DependencyProperty.Register(nameof(CommandClose), typeof(RoutedUICommand), typeof(SC2_GameTranslater_Window), new PropertyMetadata(new RoutedUICommand()));

        /// <summary>
        /// 关闭命令依赖项属性
        /// </summary>
        public RoutedUICommand CommandClose { set => SetValue(CommandCloseProperty, value); get => (RoutedUICommand)GetValue(CommandCloseProperty); }

        /// <summary>
        /// 选择Mod/Map命令依赖项
        /// </summary>
        public static DependencyProperty CommandModPathProperty = DependencyProperty.Register(nameof(CommandModPath), typeof(RoutedUICommand), typeof(SC2_GameTranslater_Window), new PropertyMetadata(new RoutedUICommand()));

        /// <summary>
        /// 选择Mod/Map依赖项属性
        /// </summary>
        public RoutedUICommand CommandModPath { set => SetValue(CommandModPathProperty, value); get => (RoutedUICommand)GetValue(CommandModPathProperty); }

        #endregion

        #region 字段属性

        #region 属性

        /// <summary>
        /// 语言依赖项属性
        /// </summary>
        public static DependencyProperty EnumCurrentLanguageProperty = DependencyProperty.Register(nameof(EnumCurrentLanguage), typeof(EnumLanguage), typeof(SC2_GameTranslater_Window));

        /// <summary>
        /// 当前语言依赖项
        /// </summary>
        public EnumLanguage EnumCurrentLanguage
        {
            set
            {
                SetValue(EnumCurrentLanguageProperty, value);
                ResourceDictionary_WindowLanguage.MergedDictionaries.Clear();
                Globals.CurrentLanguage = Globals.DictUILanguages[value];
                ResourceDictionary_WindowLanguage.MergedDictionaries.Add(Globals.CurrentLanguage);
                RibbonLocalization.Current.Localization = Globals.FluentLocalizationMap[value];
                Title = Globals.CurrentLanguage["TEXT_WindowTitleText"].ToString() + " V" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            get
            {
                return ((EnumLanguage)GetValue(EnumCurrentLanguageProperty));
            }
        }

        /// <summary>
        /// 翻译语言
        /// </summary>
        public Dictionary<EnumLanguage, ToggleButton> TranslateLanguage { set; get; } = NewTranslateLanguageButton();


        #endregion

        #region 字段

        private List<ToggleButton> m_GalaxyButtons = new List<ToggleButton>();

        #endregion

        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public SC2_GameTranslater_Window()
        {
            Globals.MainWindow = this;
            InitializeComponent();
            
            #region 多语言配置
            bool useDefault = true;
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (EnumLanguage select in Enum.GetValues(typeof(EnumLanguage)))
            {
                string TEXT_LanguageName = Enum.GetName(typeof(EnumLanguage), select);
                string fileName = "Language/" + TEXT_LanguageName + ".xaml";
                FileInfo file = new FileInfo(fileName);
                ResourceDictionary language = new ResourceDictionary();
                if (file.Exists)
                {
                    language.Source = new Uri(file.FullName);
                }
                else
                {
                    switch (select)
                    {
                        case EnumLanguage.zhCN:
                            language.Source = new Uri("pack://application:,,,/" + fileName);
                            break;
                        case EnumLanguage.enUS:
                            language.Source = new Uri("pack://application:,,,/" + fileName);
                            break;
                        default:
                            continue;
                    }
                }
                string itemName = language["TEXT_LanguageName"] as string;
                Globals.DictComboBoxItemLanguage.Add(itemName, select);
                Globals.DictUILanguages.Add(select, language);
                Globals.FluentLocalizationMap[select] = assembly.CreateInstance("SC2_GameTranslater.Source.RibbonLanguage_" + Enum.GetName(typeof(EnumLanguage), select)) as RibbonLocalizationBase;
                ComboBox_Language.Items.Add(itemName);
                if (CultureInfo.CurrentCulture.LCID == (int)select)
                {
                    ComboBox_Language.SelectedItem = itemName;
                    useDefault = false;
                }
            }
            if (useDefault)
            {
                ComboBox_Language.SelectedItem = Globals.DictUILanguages[EnumLanguage.enUS]["TEXT_LanguageName"];
            }
            #endregion

            #region 指令配置

            CommandBinding binding;
            binding = new CommandBinding(CommandOpen, Executed_Open, CanExecuted_Open);
            Globals.MainWindow.CommandBindings.Add(binding);
            binding = new CommandBinding(CommandSave, Executed_Save, CanExecuted_Save);
            Globals.MainWindow.CommandBindings.Add(binding);
            binding = new CommandBinding(CommandSaveAs, Executed_SaveAs, CanExecuted_SaveAs);
            Globals.MainWindow.CommandBindings.Add(binding);
            binding = new CommandBinding(CommandAccept, Executed_Accept, CanExecuted_Accept);
            Globals.MainWindow.CommandBindings.Add(binding);
            binding = new CommandBinding(CommandClose, Executed_Close, CanExecuted_Close);
            Globals.MainWindow.CommandBindings.Add(binding);
            binding = new CommandBinding(CommandModPath, Executed_ModPath, CanExecuted_ModPath);
            Globals.MainWindow.CommandBindings.Add(binding);

            #endregion

            #region 其他
            Globals.Preference.LoadPreference();
            Globals.EventProjectChange += OnProjectChangeRefresh;
            OnProjectChangeRefresh(null, null);

            #endregion
        }

        #endregion

        #region 方法

        #region 通用

        /// <summary>
        /// 打开文件浏览器选择文件
        /// </summary>
        /// <param name="pTextBox">设置文件地址的TextBox控件</param>
        /// <param name="pFilter">文件类型筛选字符串</param>
        /// <param name="pTitle">标题</param>
        /// <returns></returns>
        private FileInfo OpenFileDialogGetOpenFile(System.Windows.Controls.TextBox pTextBox, string pFilter, string pTitle)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            if (Directory.Exists(pTextBox.Text))
            {
                fileDialog.InitialDirectory = pTextBox.Text;
            }
            else
            {
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            fileDialog.Filter = pFilter;
            fileDialog.Multiselect = false;
            fileDialog.RestoreDirectory = true;
            fileDialog.Title = pTitle;
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pTextBox.Text = fileDialog.FileName;
                return new FileInfo(fileDialog.FileName);
            }
            else
            {
                pTextBox.Text = "";
                return null;
            }
        }

        #endregion

        #region 进度条

        /// <summary>
        /// 初始化进度条状态
        /// </summary>
        /// <param name="max">最大值</param>
        /// <param name="msg">初始消息</param>
        /// <param name="func">调用委托</param>
        public void ProgressBarInit(int max, string msg, Delegate_ProgressEvent func)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    Grid_Main.IsEnabled = false;
                    ProgressBar_Loading.Visibility = Visibility.Visible;
                    TextBlock_ProgressMsg.Visibility = Visibility.Visible;
                    ProgressBar_Loading.Maximum = max;
                    TextBlock_ProgressMsg.Text = msg;
                    func?.Invoke(0, max);
                });
        }

        /// <summary>
        /// 更新加载进度条
        /// </summary>
        /// <param name="count">计数</param>
        /// <param name="msg">消息</param>
        public void ProgressBarUpadte(int count, string msg, Delegate_ProgressEvent func)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    ProgressBar_Loading.Value += count;
                    TextBlock_ProgressMsg.Text = string.Format("({0}/{1}) {2} {3}", ProgressBar_Loading.Value, ProgressBar_Loading.Maximum, Globals.CurrentLanguage["UI_TextBlock_ProgressMsg_Text"], msg);
                    func?.Invoke(ProgressBar_Loading.Value, ProgressBar_Loading.Maximum);
                });
        }

        /// <summary>
        /// 清理进度条状态
        /// </summary>
        public void ProgressBarClean(Delegate_ProgressEvent func)
        {
            Dispatcher.BeginInvoke(priority: System.Windows.Threading.DispatcherPriority.Normal,
                method: (ThreadStart)delegate ()
                {
                    ProgressBar_Loading.Visibility = Visibility.Hidden;
                    TextBlock_ProgressMsg.Visibility = Visibility.Hidden;
                    ProgressBar_Loading.Value =0 ;
                    TextBlock_ProgressMsg.Text = "";
                    Grid_Main.IsEnabled = true;
                    func?.Invoke(ProgressBar_Loading.Value, ProgressBar_Loading.Maximum);
                });
        }

        #endregion

        #region 命令

        /// <summary>
        /// 打开项目命令执行函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void Executed_Open(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            if (Directory.Exists(Globals.Preference.LastFolderPath))
            {
                fileDialog.InitialDirectory = Globals.Preference.LastFolderPath;
            }
            else
            {
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
#if true

            fileDialog.Filter = Globals.CurrentLanguage["TEXT_ProjectFile"] as string + "|*" + Globals.Extension_SC2GameTran + "|" + Globals.CurrentLanguage["TEXT_SC2File"] as string + "|" + Globals.FileName_SC2Components;
#else            
            fileDialog.Filter = Globals.CurrentLanguage["TEXT_ProjectFile"] as string + "|*" + Globals.Extension_SC2GameTran + "|" + Globals.CurrentLanguage["TEXT_SC2File"] as string + "|*" + Globals.Extension_SC2Map + ";*" + Globals.Extension_SC2Mod + ";" + Globals.FileName_SC2Components;
#endif
            fileDialog.Multiselect = false;
            fileDialog.RestoreDirectory = true;
            fileDialog.Title = Globals.CurrentLanguage["UI_OpenFileDialog_Open_Title"] as string;
#if DEBUG
            fileDialog.FilterIndex = 2;
#endif
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo file = new FileInfo(fileDialog.FileName);
                Globals.Preference.LastFolderPath = file.DirectoryName;
                if (fileDialog.FilterIndex == 1)
                {
                    Globals.MainWindow.ProjectOpen(file);
                }
                else
                {
                    Globals.MainWindow.ProjectNew(file);
                }
            }
            
            e.Handled = true;
        }

        /// <summary>
        /// 打开项目命令判断函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void CanExecuted_Open(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        ///接受项目命令执行函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void Executed_Save(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Save");
            e.Handled = true;
        }

        /// <summary>
        /// 保存项目命令判断函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void CanExecuted_Save(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Globals.MainWindow.CheckCurrentProjectExist();
            e.Handled = true;
        }
        
        /// <summary>
        ///另存为项目命令执行函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void Executed_SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("SaveAs");
            e.Handled = true;
        }

        /// <summary>
        /// 另存为项目命令判断函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void CanExecuted_SaveAs(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Globals.MainWindow.CheckCurrentProjectExist();
            e.Handled = true;
        }
        
        /// <summary>
        /// 应用项目命令执行函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void Executed_Accept(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Accept");
            e.Handled = true;
        }

        /// <summary>
        /// 应用项目命令判断函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void CanExecuted_Accept(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Globals.ModPathValid;
            e.Handled = true;
        }

        /// <summary>
        ///关闭项目命令执行函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            Globals.MainWindow.ProjectClose();
            e.Handled = true;
        }

        /// <summary>
        /// 关闭项目命令判断函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void CanExecuted_Close(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Globals.MainWindow.CheckCurrentProjectExist();
            e.Handled = true;
        }

        /// <summary>
        /// 浏览Mod/Map执行函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void Executed_ModPath(object sender, ExecutedRoutedEventArgs e)
        {
            if (Globals.MainWindow != null)
            {
                FileInfo mod = Globals.MainWindow.OpenFileDialogGetOpenFile(Globals.MainWindow.TextBox_ModPath, Globals.CurrentLanguage["TEXT_SC2File"] as string + "|" + Globals.FileName_SC2Components, Globals.CurrentLanguage["UI_OpenFileDialog_Open_Title"] as string);
                if (mod != null)
                {
                    Globals.CurrentProject.ComponentsPath = mod.DirectoryName;
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// 浏览Mod/Map判断函数
        /// </summary>
        /// <param name="sender">命令来源</param>
        /// <param name="e">路由事件参数</param>
        public static void CanExecuted_ModPath(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Globals.MainWindow.CheckCurrentProjectExist();
            e.Handled = true;
        }

        #endregion

        #region 翻译选项

        /// <summary>
        /// 刷新翻译选项
        /// </summary>
        /// <param name="project">项目</param>
        public void RefreshTranslateLanguageButtons(Data_GameText project)
        {
            if (project == null)
            {
                RefreshNoTranslateLanguageButtons();
            }
            else
            {
                List<EnumLanguage> list = project.GetLanguageList();
                if (list.Count == 0)
                {
                    RefreshNoTranslateLanguageButtons();
                    return;
                }
                InRibbonGallery_TranslateLanguage.Items.Clear();
                ToggleButton selectButton = null;
                foreach (KeyValuePair<EnumLanguage, ToggleButton> select in TranslateLanguage)
                {
                    if (list.Contains(select.Key))
                    {
                        InRibbonGallery_TranslateLanguage.Items.Add(select.Value);
                        if (selectButton == null || (int)select.Key == CultureInfo.CurrentCulture.LCID)
                        {
                            selectButton = select.Value;
                        }
                    }
                }
                selectButton.IsChecked = true;
            }
        }

        /// <summary>
        /// 没有可用的翻译语言
        /// </summary>
        private void RefreshNoTranslateLanguageButtons()
        {
            InRibbonGallery_TranslateLanguage.Items.Clear();
            InRibbonGallery_TranslateLanguage.Items.Add(TranslateLanguage[0]);
            TranslateLanguage[0].IsChecked = true;
        }

        /// <summary>
        /// 新建语言切换按钮
        /// </summary>
        /// <returns>按钮</returns>
        private static Dictionary<EnumLanguage, ToggleButton> NewTranslateLanguageButton()
        {
            Dictionary<EnumLanguage, ToggleButton> list = new Dictionary<EnumLanguage, ToggleButton>();

            ToggleButton button = new ToggleButton();
            button.SetResourceReference(ToggleButton.HeaderProperty, "TEXT_Null");
            button.SetResourceReference(ToggleButton.IconProperty, "IMAGE_Null");
            button.SetResourceReference(ToggleButton.LargeIconProperty, "IMAGE_Null");
            button.Tag = null;
            button.IsEnabled = false;
            list.Add(0, button);

            EnumLanguage[] array = Enum.GetValues(typeof(EnumLanguage)).Cast<EnumLanguage>().ToArray();
            Array.Sort(array, (p1, p2) => Enum.GetName(typeof(EnumLanguage), p1).CompareTo(Enum.GetName(typeof(EnumLanguage), p2)));
            foreach (EnumLanguage lang in array)
            {
                list.Add(lang, NewTranslateLanguageButton(lang));
            }
            return list;
        }

        /// <summary>
        /// 新建语言切换按钮
        /// </summary>
        /// <param name="language">对应语言</param>
        /// <returns>按钮</returns>
        private static ToggleButton NewTranslateLanguageButton(EnumLanguage language)
        {
            string lang = Enum.GetName(language.GetType(), language);
            ToggleButton button = new ToggleButton
            {
                Tag = language,
                GroupName = "TranslateLanguage",
            };
            button.SetResourceReference(ToggleButton.IconProperty, string.Format("IMAGE_{0}", lang));
            button.SetResourceReference(ToggleButton.LargeIconProperty, string.Format("IMAGE_{0}", lang));
            button.SetResourceReference(ToggleButton.HeaderProperty, string.Format("TEXT_{0}", lang));
            button.Checked += Button_TranslateLanguage_Checked;
            return button;
        }

        #endregion

        #region Galaxy选项


        /// <summary>
        /// 新建语言切换按钮
        /// </summary>
        /// <returns>按钮列表</returns>
        private void RefreshGalaxyFileFilterButton(Data_GameText project)
        {
            foreach (ToggleButton button in m_GalaxyButtons)
            {
                InRibbonGallery_GalaxyFilter.Items.Remove(button);
            }
            ToggleButton_FilterGalaxyFileNone.IsChecked = true;
            m_GalaxyButtons.Clear();
            if (project != null)
            {
                InRibbonGallery_GalaxyFilter.Items.Add(ToggleButton_FilterGalaxyFileNone);
                ToggleButton button;
                foreach (DataRow row in project.Tables[Data_GameText.TN_GalaxyFile].Rows)
                {
                    button = NewGalaxyFileFilterButton(row);
                    m_GalaxyButtons.Add(NewGalaxyFileFilterButton(row));
                    InRibbonGallery_GalaxyFilter.Items.Insert(InRibbonGallery_GalaxyFilter.Items.Count - 1, button);
                }
            }
            else
            {
                InRibbonGallery_GalaxyFilter.Items.Remove(ToggleButton_FilterGalaxyFileNone);
            }
            InRibbonGallery_GalaxyFilter.SelectedItem = null;
        }

        /// <summary>
        /// 新建Galaxy文件筛选按钮
        /// </summary>
        /// <param name="language">对应语言</param>
        /// <returns>按钮</returns>
        private ToggleButton NewGalaxyFileFilterButton(DataRow fileRow)
        {
            ToggleButton button = new ToggleButton
            {
                Tag = fileRow,
                Header = fileRow[Data_GameText.RN_GalaxyFile_Name],
                ToolTip = fileRow[Data_GameText.RN_GalaxyFile_Path],
                SizeDefinition = new RibbonControlSizeDefinition(RibbonControlSize.Middle, RibbonControlSize.Middle, RibbonControlSize.Middle),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            button.SetResourceReference(ToggleButton.IconProperty, "IMAGE_GalaxyFile");
            button.SetResourceReference(ToggleButton.LargeIconProperty, "IMAGE_GalaxyFile");
            return button;
        }

        #endregion

        #region DataGrid

        /// <summary>
        /// 加载数据表
        /// </summary>
        /// <param name="table"></param>
        public void DataGrid_LoadData(DataTable table)
        {
            if (table == null)
            {
                DataGrid_Excel.ItemsSource = null;
            }
            else
            {
                DataGrid_Excel.ItemsSource = new DataView(table);
            }
        }

        /// <summary>
        /// 设置当前翻译语言
        /// </summary>
        /// <param name="lang">翻译语言</param>
        public void SetCurrentTranslateLanguage(EnumLanguage lang)
        {
            Binding binding;
            string langName = Enum.GetName(lang.GetType(), lang);
            binding = new Binding(Data_GameText.GetGameTextNameForLanguage(lang, Data_GameText.RN_GameText_Status))
            {
                Mode = BindingMode.TwoWay,
            };
            DataGridColumn_Status.Binding = binding;
            binding = new Binding(Data_GameText.GetGameTextNameForLanguage(lang, Data_GameText.RN_GameText_Text))
            {
                Mode = BindingMode.TwoWay,
            };
            DataGridColumn_Text.Binding = binding;
            binding = new Binding(Data_GameText.GetGameTextNameForLanguage(lang, Data_GameText.RN_GameText_Edited))
            {
                Mode = BindingMode.TwoWay,
            };
            DataGridColumn_Edited.Binding = binding;
        }

        /// <summary>
        /// 清理当前翻译语言
        /// </summary>
        public void CleanCurrentTranslateLanguage()
        {
            DataGridColumn_Status.Binding = null;
            DataGridColumn_Text.Binding = null;
            DataGridColumn_Edited.Binding = null;
        }

        #endregion

        #region 功能

        /// <summary>
        /// 打开项目
        /// </summary>
        /// <param name="file">文件路径</param>
        public void ProjectOpen(FileInfo file)
        {
            ProjectClose();
        }

        /// <summary>
        /// 新建项目
        /// </summary>
        /// <param name="file">文件路径</param>
        public void ProjectNew(FileInfo file)
        {
            ProjectClose();
            Globals.InitProjectData(file);
            RibbonGroupBox_File.IsEnabled = true;
        }
        
        /// <summary>
        /// 关闭文件
        /// </summary>
        public void ProjectClose()
        {
            ///To Do Save
            if (Globals.CurrentProject != null) Globals.CurrentProject = null;
        }

        /// <summary>
        /// 检测当前项目存在
        /// </summary>
        /// <returns></returns>
        public bool CheckCurrentProjectExist()
        {
            return Globals.CurrentProject != null;
        }

        /// <summary>
        /// 当前项目数据切换刷新
        /// </summary>
        /// <param name="oldPro">旧项目</param>
        /// <param name="newPro">新项目</param>
        private void OnProjectChangeRefresh(Data_GameText oldPro, Data_GameText newPro)
        {
            RefreshTranslateLanguageButtons(newPro);
            RefreshGalaxyFileFilterButton(newPro);
            DataGrid_LoadData(newPro?.Tables[Data_GameText.TN_GameText]);
        }
        #endregion

        #endregion

        #region 控件事件

        /// <summary>
        /// 语言选择下拉列表选择事件
        /// </summary>
        /// <param name="sender">事件控件</param>
        /// <param name="e">响应参数</param>
        private void ComboBox_Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string itemName = ComboBox_Language.SelectedItem as string;
            EnumCurrentLanguage = Globals.DictComboBoxItemLanguage[itemName];
            e.Handled = true;
        }

        /// <summary>
        /// 翻译语言按钮点击
        /// </summary>
        /// <param name="sender">事件控件</param>
        /// <param name="e">响应参数</param>
        private static void Button_TranslateLanguage_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                Globals.MainWindow.InRibbonGallery_TranslateLanguage.SelectedItem = button;
                if (button.Tag != null)
                {
                    Globals.MainWindow.SetCurrentTranslateLanguage((EnumLanguage)button.Tag);
                }
                else
                {
                    Globals.MainWindow.CleanCurrentTranslateLanguage();
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="sender">事件控件</param>
        /// <param name="e">响应参数</param>
        private void RibbonWindow_Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Globals.Preference.SavePreference();
        }

        /// <summary>
        /// Galaxy文件筛选全选
        /// </summary>
        /// <param name="sender">事件控件</param>
        /// <param name="e">响应参数</param>
        private void MenuItem_GalaxyFilterSelectAll_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton_FilterGalaxyFileNone.IsChecked = true;
            foreach (ToggleButton button in m_GalaxyButtons)
            {
                button.IsChecked = true;
            }
        }
        
        /// <summary>
        /// Galaxy文件筛选全不选
        /// </summary>
        /// <param name="sender">事件控件</param>
        /// <param name="e">响应参数</param>
        private void MenuItem_GalaxyFilterSelectNone_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton_FilterGalaxyFileNone.IsChecked = false;
            foreach (ToggleButton button in m_GalaxyButtons)
            {
                button.IsChecked = false;
            }

        }

        #endregion

    }
}
