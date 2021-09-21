using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Dapper;

namespace _12M.StokEkstresi.Desktop
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        public MainForm()
        {
            InitializeComponent(); 


            // fill combo box items
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["cn"].ConnectionString))
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();
                string query = "select MalKodu + '#' + MalAdi from STK";

                var result = db.Query<string>(query, CommandType.Text);

                cbxProductCodeOrName.Properties.Items.AddRange(result.ToArray());
                db.Close();
            }
        }

        // generate report
        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            StokEkstreReport stokEkstreReport = new StokEkstreReport();

            try
            {
                // set the parameters for the stored procedure
                stokEkstreReport.RequestParameters = true;
                stokEkstreReport.Parameters[0].Value = cbxProductCodeOrName.EditValue.ToString().Split('#')[0];
                stokEkstreReport.Parameters[1].Value = Convert.ToInt32(dateFrom.DateTime.ToOADate());
                stokEkstreReport.Parameters[2].Value = Convert.ToInt32(dateTo.DateTime.ToOADate());

                // set the output path of the file
                var outputFilePath = $"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")} {cbxProductCodeOrName.EditValue.ToString().Split('#')[0]} Stok Ekstresi.pdf";
                stokEkstreReport.ExportToPdf(outputFilePath);

                // show message box
                var dialogResult = MessageBox.Show($"{outputFilePath} Oluşturuldu. Açılsın mı?", "Başarılı", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(Application.StartupPath + "/" + outputFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Hata!");
            }
        }

        // execute stored procedure and fill grid
        private void btnLoad_Click(object sender, EventArgs e)
        {
            // create and dispose new database instance
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["cn"].ConnectionString))
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                // stored procedure query with parameters
                string query = $@"
                    EXEC dbo.sp_stok_ekstresi_olustur
                        @MalKodu = '{cbxProductCodeOrName.EditValue.ToString().Split('#')[0]}',
                    	@BaslangicTarihi = {Convert.ToInt32(dateFrom.DateTime.ToOADate())},
                    	@BitisTarihi = {Convert.ToInt32(dateTo.DateTime.ToOADate())}";

                // cast stored procedure results as enumerable
                var result = db.Query<StokEkstresi>(query, CommandType.Text);

                gridStokEkstresi.DataSource = result;
                db.Close();
            }
        }

        // print the report
        private void btnPrint_Click(object sender, EventArgs e)
        {
            StokEkstreReport stokEkstreReport = new StokEkstreReport();

            try
            {
                stokEkstreReport.RequestParameters = true;
                stokEkstreReport.Parameters[0].Value = cbxProductCodeOrName.EditValue.ToString().Split('#')[0];
                stokEkstreReport.Parameters[1].Value = Convert.ToInt32(dateFrom.DateTime.ToOADate());
                stokEkstreReport.Parameters[2].Value = Convert.ToInt32(dateTo.DateTime.ToOADate());

                // set the output path of the file
                var outputFilePath = $"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")} {cbxProductCodeOrName.EditValue.ToString().Split('#')[0]} Stok Ekstresi.pdf";
                stokEkstreReport.Print();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Hata!");
            }
        }
    }
}