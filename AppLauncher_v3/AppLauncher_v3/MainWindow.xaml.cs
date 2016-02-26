using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Shell;
using Hardcodet.Wpf.TaskbarNotification;

namespace AppLauncher_v3
{
    public enum ITEM_TYPE { file, folder, none }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ContextMenu menuTabControl = new ContextMenu();

            MenuItem menuItemInsert = new MenuItem();
            menuItemInsert.Header = "追加";
            menuItemInsert.Click += (object sender, RoutedEventArgs e) => { addTabItem(); };

            MenuItem menuItemDelete = new MenuItem();
            menuItemDelete.Header = "削除";
            menuItemDelete.Click += (object sender, RoutedEventArgs e) =>
            {
                TabItem item = tabControl.SelectedItem as TabItem;

                MessageBoxResult rs = MessageBox.Show(item.Header + "を本当に削除しますか？", "削除", MessageBoxButton.OKCancel);
                if (rs == MessageBoxResult.OK)
                    tabControl.Items.Remove(tabControl.SelectedItem);
            };

            MenuItem menuItemHeaderEdit = new MenuItem();
            menuItemHeaderEdit.Header = "編集";
            menuItemHeaderEdit.Click += (object sender, RoutedEventArgs e) =>
            {
                TabItem item = tabControl.SelectedItem as TabItem;
                item.Header = checkTabItemHeader(openEditDialog(item.Header.ToString()));
            };


            menuTabControl.Items.Add(menuItemInsert);
            menuTabControl.Items.Add(menuItemDelete);
            menuTabControl.Items.Add(menuItemHeaderEdit);
            tabControl.ContextMenu = menuTabControl;


            readConfig();

            //MyNotifyIcon.ShowBalloonTip("メール通知", "やりたいなあ！", BalloonIcon.Info);
            
            
        }
        private void saveConfig()
        {
            if(tabControl.Items.Count > 0)
            {
                using (FileStream fs = new FileStream(@".\save.config", FileMode.OpenOrCreate))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    BinaryFormatter bf = new BinaryFormatter();

                    Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
                    foreach(TabItem item in tabControl.Items)
                    {
                        String key = item.Header.ToString();
                        ListBox lb = item.Content as ListBox;
                        List<string> list = new List<string>();

                        foreach(ImageList imageItem in lb.Items)
                            list.Add(imageItem.fullPath);
                        dict.Add(key, list.ToArray());
                        
                    }

                    SaveConfig con = new SaveConfig(dict);
                    bf.Serialize(fs, con);
                }
                
            }
        }
        private void readConfig()
        {
            using(FileStream fs = new FileStream(@".\save.config", FileMode.OpenOrCreate))
            {
                
                BinaryFormatter bf = new BinaryFormatter();
                
                if (fs.Length > 0)
                {
                    SaveConfig con = (SaveConfig)bf.Deserialize(fs);
                    Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
                    dict = con.dict;
                    foreach(string item in dict.Keys)
                    {
                        TabItem tItem = returnTabItem(item);
                        ListBox lb = tItem.Content as ListBox;
                        foreach (string path in dict[item])
                        {
                            ImageList iList = returnImageList(path);
                            if(iList.type != ITEM_TYPE.none)
                                lb.Items.Add(iList);
                        }
                        tabControl.Items.Add(tItem);
                    }
                    
                }
            }
        }
        private string openEditDialog(String dText)
        {
            EditWindow ew = new EditWindow(dText);
            ew.ShowDialog();
            if (!string.IsNullOrWhiteSpace(ew.AfterText))
                return ew.AfterText;
            else return ew.BeforeText;
        }

        private TabItem returnTabItem(string header)
        {
            TabItem item = new TabItem();
            item.Header = header;
            item.Content = returnListBox();
            item.AllowDrop = true;
            return item;
        }
        private void addTabItem()
        {
            TabItem item = returnTabItem(checkTabItemHeader("default"));
            tabControl.Items.Add(item);
        }
        private WriteableBitmap returnIcon(String imgPath)
        {
            //MessageBox.Show(imgPath);
            BitmapImage folIcon = new BitmapImage();
            MemoryStream data = new MemoryStream(File.ReadAllBytes(imgPath));
            WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));

            
            
            data.Close();

            return wbmp;
        }
        private string checkTabItemHeader(String name)
        {
            //MessageBox.Show(name);
            List<String> hList = new List<string>();
            String ans="";

            foreach (TabItem item in tabControl.Items)
                hList.Add(item.Header.ToString());

            String ansName = name;
            for (int i = 0; ; i++)
            {
                if (!hList.Contains(ansName))
                {
                    ans = ansName;
                    break;
                }
                ansName = name + i.ToString();
            }
            return ansName;
        }
        private ListBox returnListBox()
        {

            FrameworkElementFactory spFactory = new FrameworkElementFactory(typeof(StackPanel));
            spFactory.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

            FrameworkElementFactory imgFactory = new FrameworkElementFactory(typeof(Image));
            imgFactory.SetValue(Image.WidthProperty, 30.0);
            imgFactory.SetValue(Image.HeightProperty, 30.0);
            imgFactory.SetBinding(Image.SourceProperty, new Binding("MyImage"));
            spFactory.AppendChild(imgFactory);

            FrameworkElementFactory labelFactory = new FrameworkElementFactory(typeof(Label));
            labelFactory.SetBinding(Label.ContentProperty, new Binding("MyImageName"));
            spFactory.AppendChild(labelFactory);

            DataTemplate tmp = new DataTemplate();
            tmp.VisualTree = spFactory;

            ListBox lb = new ListBox();

            lb.AllowDrop = true;
            lb.ItemTemplate = tmp;
            lb.MouseDoubleClick += SelectList_MouseDoubleClick;
            lb.MouseDown += SelectList_MouseDown;
            lb.Drop += SelectList_Drop;
            lb.DragEnter += SelectList_DragEnter;
            lb.PreviewDragOver += SelectList_PreviewDragOver;

            return lb;
        }
        private ImageList returnImageList(string item)
        {
            Image img = new Image();
            ITEM_TYPE type;
            if (File.Exists(item))
            {
                System.Drawing.Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(item);

                img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    appIcon.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                type = ITEM_TYPE.file;
            }
            else if(Directory.Exists(item))
            {
                
                img.Source = returnIcon(@"img/folder.png");
                type = ITEM_TYPE.folder;
            }
            else
            {
                MessageBox.Show(item+"\nは存在しません");
                type = ITEM_TYPE.none;
            }
            return new ImageList(item, img.Source, type);
        }
        

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
        
        //itemで指定したフルパスの先にあるファイルorフォルダのフルパスと名前と対応するアイコンを追加する。
        

        //参照元１：http://dobon.net/vb/dotnet/control/draganddrop.html
        //参照元２：https://code.msdn.microsoft.com/windowsdesktop/XAMLCVB-WPF-Windows-WPF-a1c048ae
        //参照元３：http://www.atmarkit.co.jp/fdotnet/csharptips/003dragdrop/003dragdrop.html
        private void SelectList_MouseDown(object sender, MouseEventArgs e)
        {
            DragDrop.DoDragDrop((ListBox)sender, Content.ToString(), DragDropEffects.Copy);
        }

        private void SelectList_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);

            if (e.Handled) e.Effects = DragDropEffects.Move;
            else e.Effects = DragDropEffects.None;
        }

        private void SelectList_Drop(object sender, DragEventArgs e)
        {
            
            ListBox lb = sender as ListBox;

            //()を使った場合はもしその型にキャスト出来なかった場合はInvalidCastExceptionが投げられる。
            //as を使用した場合は変換できなかった時にnullが返ってくる。
            //正直Nullの方がいいので今後はasを多様していく。
            if ((string[])e.Data.GetData(DataFormats.FileDrop) != null)
            {
                
                //itemにはフルパスが入る。
                foreach (string item in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    ImageList iList = returnImageList(item);
                    if(iList.type != ITEM_TYPE.none)
                        lb.Items.Add(iList);
                }
                    
            }
            
        }
        private void ImageListAdd(IList<ImageList> iList)
        {

        }

        private void SelectList_PreviewDragOver(object sender, DragEventArgs e)
        {
            // ファイルをドロップされた場合のみ e.Handled を True にする
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }
        //一応ダブルクリックでエクスプローラで確認可能にする。　右クリックのアイテムでも可能にはしたい
        private void SelectList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;
            foreach(ImageList item in lb.SelectedItems)
            {
                System.Diagnostics.Process.Start("EXPLORER.EXE", item.fullPath);
                MessageBox.Show(item.fullPath);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tabControl.SelectionChanged += TabControl_SelectionChanged;
           
        }

        private void button_add_Click(object sender, RoutedEventArgs e)
        {
            addTabItem();
        }

        private void tabControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TabItem item = tabControl.SelectedItem as TabItem;
            item.Header = openEditDialog(item.Header.ToString());
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            saveConfig();
        }

        private void button_read_Click(object sender, RoutedEventArgs e)
        {
            tabControl.Items.Clear();
            readConfig();
        }
    }
}
