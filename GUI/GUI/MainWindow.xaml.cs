using System;
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

namespace GUI
{

    class Chat
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            OutputChats();
        }

        private void OutputChats()
        {
            List<Chat> chats = new List<Chat> { new Chat { Id = "1", Title = "Foo" }, new Chat { Id = "2", Title="Bar" } };
            chats.ForEach(chat =>
            {
                ListViewItem listViewItem = new ListViewItem() { Height = 50 };
                StackPanel stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                TextBlock textBlock = new TextBlock()
                {
                    Text = chat.Title,
                    FontSize = 17,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 20,0)
                };

                stackPanel.Children.Add(textBlock);
                listViewItem.Content = stackPanel;
                ChatsList.Items.Add(listViewItem);
            });
        }

        private void MoveDrag(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
