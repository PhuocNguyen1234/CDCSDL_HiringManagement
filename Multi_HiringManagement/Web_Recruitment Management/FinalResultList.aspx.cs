using System;
using Web_Recruitment_Management.App_Code;
using System.Data;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
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

        protected void gvFinal_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Chỉ tác động lên các dòng chứa dữ liệu (bỏ qua Header và Footer)
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Lấy ID ứng viên từ dòng hiện tại
                string appId = DataBinder.Eval(e.Row.DataItem, "ApplicationID").ToString();

                // Gán sự kiện click gọi hàm JS showDetail
                e.Row.Attributes["onclick"] = $"showDetail({appId});";

                // Thêm class CSS để có hiệu ứng bàn tay và đổi màu khi hover
                e.Row.CssClass = "table-hover-row";
            }
        }

        // --- 2. HÀM API TRẢ VỀ CHI TIẾT ỨNG VIÊN CHO MODAL ---
        [WebMethod]
        public static string GetCandidateDetail(int applicationId)
        {
            XuLyDuLieu db = new XuLyDuLieu();
            DataTable dt = db.GetFullDetail(applicationId);

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                // Format bảng HTML trả về (Đã tối ưu giao diện Bootstrap)
                string html = $@"
                    <table class='table table-sm table-bordered mb-0'>
                        <tr><th width='35%' class='bg-light text-end pe-3'>Mã Hồ sơ:</th><td class='ps-3'>{row["CandidateID"]}</td></tr>
                        <tr><th class='bg-light text-end pe-3'>Họ tên:</th><td class='fw-bold text-primary ps-3'>{row["FullName"]}</td></tr>
                        <tr><th class='bg-light text-end pe-3'>Email:</th><td class='ps-3'>{row["Email"]}</td></tr>
                        <tr><th class='bg-light text-end pe-3'>Lĩnh vực:</th><td class='ps-3'><span class='badge bg-info text-dark'>{row["JobTitle"]}</span></td></tr>
                        <tr><th class='bg-light text-end pe-3'>Trường Đại học:</th><td class='ps-3'>{row["College_Name"]}</td></tr>
                        <tr><th class='bg-light text-end pe-3'>Kỹ năng chính:</th><td class='ps-3'>{row["Skills"]}</td></tr>
                        <tr><th class='bg-light text-end pe-3'>GPA:</th><td class='ps-3 fw-bold'>{row["GPA"]}</td></tr>
                        <tr><th class='bg-light text-end pe-3'>Kinh nghiệm:</th><td class='ps-3'>{row["Years_Of_Experience"]} năm</td></tr>
                        <tr><th class='bg-light text-end pe-3'>Dự án thực tế:</th><td class='ps-3'>{row["Projects_Count"]} dự án</td></tr>
                    </table>";
                return html;
            }
            return "<div class='alert alert-warning text-center'>Không tìm thấy thông tin chi tiết của ứng viên này.</div>";
        }
    }
}