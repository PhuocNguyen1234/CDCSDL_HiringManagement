using System;
using Web_Recruitment_Management.App_Code;
using System.Data;
namespace Web_Recruitment_Management
{
    public partial class FinalResultList : System.Web.UI.Page
    {
        XuLyDuLieu _xuly = new XuLyDuLieu();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadFinalResults();
            }
        }
        private void LoadFinalResults()
        {
            string sql = @"
                SELECT f.ApplicationID, c.FullName, c.Stream, f.AI_Cluster_Group, f.Final_Result
                FROM Fact_Applications f
                INNER JOIN Dim_Candidates c ON f.CandidateID = c.CandidateID
                WHERE f.Final_Result IS NOT NULL
                ORDER BY f.ApplicationID DESC";

            DataTable dt = _xuly.getTable(sql);
            gvFinal.DataSource = dt;
            gvFinal.DataBind();

            if (gvFinal.Rows.Count > 0)
            {
                gvFinal.UseAccessibleHeader = true;
                gvFinal.HeaderRow.TableSection = System.Web.UI.WebControls.TableRowSection.TableHeader;
            }
        }
    }
}