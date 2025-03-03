using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AppFolderPro.Databases;

namespace AppFolderPro;

public partial class FolderWindow : Window
{
    private int count = 0;
    public FolderWindow(int folderId)
    {
        InitializeComponent();
        Width = 100;

        Height = 100;
        
       
        Deactivated += OtherWindow_Deactivated!;
        var appIcons = new List<AppIcon>();
       

        // 아이콘 리스트를 ItemsControl에 바인딩
        

        var manager = new FolderDatabaseManager();
        var folder = manager.GetFolderById(folderId);

        if (folder != null)
        {
            Console.WriteLine($"Folder: {folder.Name}");
            foreach (var file in folder.Files)
            {
                appIcons.Add(new AppIcon(file.Id, file.Name, file.Path, file.Icon));
                // MessageBox.Show($"  File: {file.Name}, Path: {file.Path}");
                Console.WriteLine($"  File: {file.Name}, Path: {file.Path}");
            }
        }

        count = appIcons.Count;
        AppFolderPanel.ItemsSource = appIcons;
        AnimateWindowSize();
    }
    private void AnimateWindowSize()
    {
        DoubleAnimation widthAnimation = new DoubleAnimation
        {
            To = count == 0 ? 100 : count * 100,                // 목표 Width
            Duration = new Duration(TimeSpan.FromSeconds(2))
        };
        this.BeginAnimation(Window.WidthProperty, widthAnimation);
    }
    private void OtherWindow_Deactivated(object sender, EventArgs e)
    {
        // 조건을 추가해 Deactivated가 창을 바로 닫지 않도록 함
        if (!IsMouseOver)
        {
            Close(); // 창 닫기
        }
    }

    public class AppIcon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LnkPath { get; set; }
        public BitmapImage Icon { get; set; }

        public AppIcon(int id, string name, string lnkPath, string absoluteIconPath)
        {
            Name = name;
            Id = id;
            // 절대 경로로 아이콘 로드
            Icon = new BitmapImage(new Uri(absoluteIconPath, UriKind.Absolute));
            LnkPath = lnkPath;
        }
    }
    private void Icon_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // 이벤트가 상위로 전파되지 않도록 방지
        e.Handled = true;

        if (sender is Border border && border.DataContext is AppIcon icon)
        {
            ShellExecute(IntPtr.Zero, "open", icon.LnkPath, "", "", 1);
            Close();
        }
    }
    
    [DllImport("Shell32.dll")]
    private static extern int ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

}