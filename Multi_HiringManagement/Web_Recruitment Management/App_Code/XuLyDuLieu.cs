using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using Microsoft.AnalysisServices.AdomdClient;

namespace Web_Recruitment_Management.App_Code
{
    public partial class XuLyDuLieu
    {
        OleDbConnection CON;
        AdomdConnection myconnect;

        public XuLyDuLieu()
        {
            CON = new OleDbConnection();
            CON.ConnectionString = @"Provider=SQLNCLI11;Data Source=PHUOCNGUYEN;Integrated Security=SSPI;Initial Catalog=recruitment";

            string adomdConnStr = @"provider=olap;initial catalog=Multi_HiringManagement;datasource=localhost\SQL2016";
            myconnect = new AdomdConnection(adomdConnStr);
        }

        public void Open()
        {
            if (this.CON.State == ConnectionState.Closed)
                this.CON.Open();
        }

        public void Close()
        {
            if (this.CON.State == ConnectionState.Open)
                this.CON.Close();
        }
        public DataTable getTable(String SQL)
        {
            this.Open();
            DataTable tb = new DataTable();
            OleDbDataAdapter adp = new OleDbDataAdapter(SQL, this.CON);
            adp.Fill(tb);
            this.Close();
            return tb;
        }
        public DataTable getPredicted(String SQL)
        {
            DataTable tb = new DataTable();
            using (AdomdDataAdapter mycommand = new AdomdDataAdapter(SQL, myconnect))
            {
                try
                {
                    mycommand.Fill(tb);
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi truy vấn SSAS: " + ex.Message);
                }
            }
            return tb;
        }

       
    }
}