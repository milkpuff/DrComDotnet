using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

namespace configer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public JsonOptionsModel jsonOptions {get; set;}

        public MainWindow()
        {
            jsonOptions = new JsonOptionsModel();
            InitializeComponent();
        }

        public void VerifyButton_Clicked(object sender, RoutedEventArgs e)
        {
            Button self = sender as Button;

            self.Content = "保存中...";

            jsonOptions.user.name     = UserNameBox.Text;
            jsonOptions.user.password = UserPassWordBox.Text;

            try 
            {
                string   basePath = AppDomain.CurrentDomain.BaseDirectory;
                string jsonString = JsonSerializer.Serialize<JsonOptionsModel>(jsonOptions);
                MessageBox.Show(jsonString);
                File.WriteAllText($"{basePath}userOptions.json", jsonString);
            }
            catch(Exception err)
            {
                MessageBox.Show(err.ToString());
            }

            self.Content = "已保存";
        }
    }

}
