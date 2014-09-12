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
using System.Windows.Shapes;
using System.IO;

namespace CustomAnnotation
{

    public partial class Settings : Window
    {
        private string mSettingsFilePath = "C:\\WOZAnnotationLogs\\SavedSettings.txt";
        
        public Settings()
        {
            InitializeComponent();

            System.IO.Directory.CreateDirectory("C:\\WOZAnnotationLogs");

            if (File.Exists(mSettingsFilePath))
            {
                var reader = new StreamReader(File.OpenRead(mSettingsFilePath));
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    ObjectiveCriteria.Text = values[1];
                    CoderName.Text = values[0];

                }
            }
            else
            {
                Byte[] info = new UTF8Encoding(true).GetBytes("John Doe,How much is the child paying attention and concentrating on the robot/task/overall interaction?");
                FileStream fs = File.Create(mSettingsFilePath);
                fs.Write(info, 0, info.Length);
                fs.Dispose();
                
            }




        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainWindow mainWindow = new MainWindow(ObjectiveCriteria.Text, CoderName.Text);

            Byte[] info = new UTF8Encoding(true).GetBytes(CoderName.Text + "," + ObjectiveCriteria.Text);
            FileStream fs = File.Create(mSettingsFilePath);
            fs.Write(info, 0, info.Length);
            fs.Dispose();

            mainWindow.Show();
        }
    }
}
