using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using QRCoder;
using QRCoder.Xaml;

namespace kodyQRwpfKB4D
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            loadListboxData();
        }

        private string lastGeneratedText;
        private void GenerateQRcodeBtn_OnClick(object sender, RoutedEventArgs e)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(TextBoxInput.Text, QRCodeGenerator.ECCLevel.Q);
            XamlQRCode qrCode = new XamlQRCode(qrCodeData);
            DrawingImage qrCodeAsXaml = qrCode.GetGraphic(20);
            lastGeneratedText = TextBoxInput.Text;

            QRimage.Source = qrCodeAsXaml;
        }
        private void UploadQRcodeToDatabase_OnClick(object sender, RoutedEventArgs e)
        {
            if (QRimage.Source != null && lastGeneratedText != null)
            {
                string constring = "SERVER=localhost; DATABASE=baza_do_zadania; UID=root; pwd=;";
                MySqlConnection con = new MySqlConnection(constring);

                String Query = String.Format("INSERT INTO kodyQR(codeID,codeText) VALUES (DEFAULT,'{0}')",lastGeneratedText);
                MySqlCommand zapytanie = new MySqlCommand(Query, con);
                MySqlDataReader MyReader;

                con.Open();
                MyReader = zapytanie.ExecuteReader();
            }
            else
            {
                MessageBox.Show("Generate the QR Code first", "Error");
            }
        }

        private void EditRecord_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Print_OnClick(object sender, RoutedEventArgs e)
        {
            if (QRimage.Source != null)
            {
                PrintDialog myPrintDialog = new PrintDialog();  
                if (myPrintDialog.ShowDialog() == true)
                {
                    myPrintDialog.PrintVisual(QRimage, "Image Print");
                }   
            }
            else
            {
                MessageBox.Show("Generate the QR Code first", "Error");
            }
        }

        private void loadListboxData()
        {
            MySqlConnection con = new MySqlConnection("SERVER=localhost; DATABASE=baza_do_zadania; UID=root; pwd=;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select * from kodyQR",con);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "kodyQR");

            IList<kodyQR> listCod = new List<kodyQR>();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                listCod.Add(
                    new kodyQR
                    {
                        id = Convert.ToInt32(dr[0].ToString()),
                        name = dr[1].ToString()
                    }
                );
            }

            viewDatabaseRecords.ItemsSource = listCod;
        }
        private void DatabaseManipulationButton_OnClick(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection("SERVER=localhost; DATABASE=baza_do_zadania; UID=root; pwd=;"))
            {
                qrImageShown.Visibility = Visibility.Hidden;
                databaseManipulationForm.Visibility = Visibility.Visible;
                
                loadListboxData();
            }
        }

        public class kodyQR
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        private void DatabaseManipulationFormChangeButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string idInput = databaseManipulationFormID.Text;
                string stringInput = databaseManipulationFormString.Text;

                if (idInput != "")
                {
                    string constring = "SERVER=localhost; DATABASE=baza_do_zadania; UID=root; pwd=;";
                    MySqlConnection con = new MySqlConnection(constring);

                    String Query;
                    
                    if (stringInput == "")
                        Query = String.Format("delete from kodyQR where codeID = '{0}';", idInput);
                    else 
                        Query = String.Format("update kodyQR set codeText = '{0}' where codeID = '{1}';", stringInput, idInput);
                    MySqlCommand zapytanie = new MySqlCommand(Query, con);
                    MySqlDataReader MyReader;

                    con.Open();
                    MyReader = zapytanie.ExecuteReader();

                    MessageBox.Show("Record was updated", "Positive action");
                
                    qrImageShown.Visibility = Visibility.Visible;
                    databaseManipulationForm.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Bruh: "+exception);
            }
        }

        private void DatabaseManipulationFormCancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            qrImageShown.Visibility = Visibility.Visible;
            databaseManipulationForm.Visibility = Visibility.Hidden;
        }
    }
}