using System;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.Services;
using Web_Recruitment_Management.App_Code;

namespace Web_Recruitment_Management
{
    public partial class CandidateList : System.Web.UI.Page
    {
        App_Code.XuLyDuLieu _xuly = new App_Code.XuLyDuLieu();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadClusters();
                LoadData("All", "All");
            }
        }

        private void LoadClusters()
        {
            string sql = "SELECT DISTINCT AI_Cluster_Group FROM Fact_Applications WHERE AI_Cluster_Group IS NOT NULL";
            DataTable dt = _xuly.getTable(sql);
            foreach (DataRow row in dt.Rows)
            {
                ddlFilterCluster.Items.Add(row["AI_Cluster_Group"].ToString());
            }
        }
        protected void gvCandidates_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCandidates.PageIndex = e.NewPageIndex;
            LoadData(ddlFilterCluster.SelectedValue, ddlFilterStatus.SelectedValue);
        }
        private void LoadData(string clusterFilter, string statusFilter) // Thêm tham số statusFilter
        {
            string sql = @"
                SELECT 
                    f.ApplicationID AS app_id,
                    c.FullName AS name, 
                    c.Degree AS degree, 
                    c.Stream AS stream, 
                    f.GPA AS gpa, 
                    f.Years_Of_Experience AS exp,
                    f.Projects_Count AS projects,
                    f.Prediction_Probability AS prob,
                    f.ApplyDate AS apply_date,
                    f.Placement_Status AS placement_status, 
                    f.AI_Cluster_Group AS cluster_name
                FROM Fact_Applications f
                INNER JOIN Dim_Candidates c ON f.CandidateID = c.CandidateID
                WHERE 1=1";

            // 1. Điều kiện lọc theo cụm AI
            if (!string.IsNullOrEmpty(clusterFilter) && clusterFilter != "All")
            {
                sql += $" AND f.AI_Cluster_Group = N'{clusterFilter}'";
            }

            // 2. THÊM MỚI: Điều kiện lọc theo trạng thái kết quả
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                if (statusFilter == "Placed")
                {
                    // Lấy những người có chữ Placed nhưng KHÔNG có chữ Not
                    sql += " AND f.Placement_Status LIKE '%Placed%' AND f.Placement_Status NOT LIKE '%Not%'";
                }
                else if (statusFilter == "Not Placed")
                {
                    // Lấy những người có chữ Not
                    sql += " AND f.Placement_Status LIKE '%Not%'";
                }
            }

            sql += " ORDER BY f.ApplicationID DESC";

            gvCandidates.DataSource = _xuly.getTable(sql);
            gvCandidates.DataBind();

            if (gvCandidates.Rows.Count > 0)
            {
                gvCandidates.UseAccessibleHeader = true;
                gvCandidates.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

        protected void gvCandidates_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteRow")
            {
                // Kiểm tra xem ID có lấy được không
                int appId = Convert.ToInt32(e.CommandArgument);

                // Thực hiện xóa
                bool result = _xuly.XoaUngVien(appId);

                if (result)
                {
                    // Load lại dữ liệu để cập nhật bảng ngay lập tức
                    LoadData(ddlFilterCluster.SelectedValue, ddlFilterStatus.SelectedValue);
                }
                else
                {
                    // Thông báo nếu có lỗi
                    Response.Write("<script>alert('Không thể xóa do ràng buộc dữ liệu!');</script>");
                }
            }
            else if (e.CommandName == "EditRow")
            {
                int appId = Convert.ToInt32(e.CommandArgument);
                DataTable dt = _xuly.LấyThongTinUngVienDeSua(appId);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    // Đổ dữ liệu vào các ô TextBox
                    hdfEditAppId.Value = row["ApplicationID"].ToString();
                    hdfEditCandidateId.Value = row["CandidateID"].ToString();
                    txtEditName.Text = row["FullName"].ToString();
                    txtEditEmail.Text = row["Email"].ToString();
                    txtEditCollege.Text = row["College_Name"].ToString();
                    txtEditSkills.Text = row["Skills"].ToString();
                    txtEditGPA.Text = row["GPA"].ToString();
                    txtEditExp.Text = row["Years_Of_Experience"].ToString();
                    txtEditProjects.Text = row["Projects_Count"].ToString();

                    string script = @"
                        window.addEventListener('DOMContentLoaded', function() {
                            var myModal = new bootstrap.Modal(document.getElementById('editModal'));
                            myModal.show();
                        });";
                    ClientScript.RegisterStartupScript(this.GetType(), "showEditModal", script, true);
                }
            }
            else if (e.CommandName == "ReEvaluate")
            {
                // Lấy ID và chuyển hướng sang trang Phân tích, kèm theo reAppId trên URL
                string appId = e.CommandArgument.ToString();
                Response.Redirect($"CandidateFiltering.aspx?reAppId={appId}");
            }
        }

        protected void btnUpdateCandidate_Click(object sender, EventArgs e)
        {
            try
            {
                int appId = Convert.ToInt32(hdfEditAppId.Value);
                int candidateId = Convert.ToInt32(hdfEditCandidateId.Value);

                string name = txtEditName.Text.Trim();
                string email = txtEditEmail.Text.Trim();
                string college = txtEditCollege.Text.Trim();
                string skillsList = txtEditSkills.Text.Trim();

                // Chuyển đổi dữ liệu số (xử lý dấu phẩy thập phân an toàn)
                double gpa = 0;
                double.TryParse(txtEditGPA.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out gpa);
                int exp = int.TryParse(txtEditExp.Text, out int ex) ? ex : 0;
                int projects = int.TryParse(txtEditProjects.Text, out int p) ? p : 0;

                // Gọi hàm Update
                bool success = _xuly.CapNhatUngVien(candidateId, appId, name, email, college, gpa, exp, projects, skillsList);

                if (success)
                {
                    // Load lại bảng và gọi JS để đóng Modal
                    LoadData(ddlFilterCluster.SelectedValue, ddlFilterStatus.SelectedValue);
                    ClientScript.RegisterStartupScript(this.GetType(), "successEdit", "alert('Cập nhật thông tin thành công!');", true);
                }
                else
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "errorEdit", "alert('Lỗi cập nhật CSDL!');", true);
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "errorEx", $"alert('Lỗi hệ thống: {ex.Message}');", true);
            }
        }

        protected void ddlFilterCluster_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData(ddlFilterCluster.SelectedValue, ddlFilterStatus.SelectedValue);
        }
        protected void ddlFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData(ddlFilterCluster.SelectedValue, ddlFilterStatus.SelectedValue);
        }

        [WebMethod]
        public static string GetCandidateDetail(int applicationId)
        {
            // Khởi tạo đối tượng xử lý (thay bằng tên lớp của bạn)
            XuLyDuLieu db = new XuLyDuLieu();
            DataTable dt = db.GetFullDetail(applicationId);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                // Tạo chuỗi HTML để trả về hiển thị trong Modal
                string html = $@"
                        <table class='table table-sm'>
                            <tr><th width='40%'>ID Ứng viên:</th><td>{row["CandidateID"]}</td></tr>
                            <tr><th>Họ tên:</th><td>{row["FullName"]}</td></tr>
                            <tr><th>Email:</th><td>{row["Email"]}</td></tr>
                            <tr><th>Chuyên ngành:</th><td><span class='badge bg-info'>{row["JobTitle"]}</span></td></tr>
                            <tr><th>Trường học:</th><td>{row["College_Name"]}</td></tr>
                            <tr><th>Kỹ năng chính:</th><td>{row["Skills"]}</td></tr>
                        </table>";
                return html;
            }
            return "Không tìm thấy thông tin.";
        }
    }
}
