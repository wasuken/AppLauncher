using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppLauncher_v3
{
    /// <summary>
    /// EditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditWindow : Window
    {
        public string BeforeText { get; set; }
        public string AfterText { get; set; }
        public EditWindow()
        {
            InitializeComponent();
            

            textBox.KeyDown += (object sender, KeyEventArgs e) => 
            {
                if (e.Key == Key.Enter)
                    decisionResult();
                switch (e.Key)
                {
                    case Key.Enter:
                        var provider = new ButtonAutomationPeer(button_OK) as IInvokeProvider;
                        provider.Invoke();
                        //decisionResult();
                        break;
                    case Key.Escape:
                        Close();
                        DialogResult = false;
                        break;
                    default: break;
                }
            };
            
        }
        public EditWindow(String dText) : this()
        {
            textBox.Text = dText;
            BeforeText = dText;
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            decisionResult();
        }
        private void decisionResult()
        {
            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                AfterText = textBox.Text;
                
                Close();
            }
            else {
                MessageBox.Show("テキストボックスに値が入っていません");
                
            }
        }

        private void button_CANCEL_Click(object sender, RoutedEventArgs e)
        {
            Close();
            
        }
    }
}
