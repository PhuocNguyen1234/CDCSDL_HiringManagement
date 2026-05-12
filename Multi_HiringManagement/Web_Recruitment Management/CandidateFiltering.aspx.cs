using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Web_Recruitment_Management.App_Code;

namespace Web_Recruitment_Management
{
    public partial class CandidateFiltering : System.Web.UI.Page
    {
        private readonly App_Code.XuLyDuLieu _xuly = new App_Code.XuLyDuLieu();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadAllComboBoxes();

                if (Request.QueryString["reAppId"] != null)
                {
                    string appId = Request.QueryString["reAppId"].ToString();
                    DataTable dt = _xuly.LayDuLieuDeDanhGiaLai(Convert.ToInt32(appId));

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];

                        hdfReAppId.Value = row["ApplicationID"].ToString();
                        hdfReCandidateId.Value = row["CandidateID"].ToString();

                        txtName.Text = row["FullName"].ToString();
                        txtEmail.Text = row["Email"].ToString();
                        txtAge.Text = row["Age"].ToString();
                        txtCollege.Text = row["College_Name"].ToString();
                        txtGPA.Text = row["GPA"].ToString();
                        txtExp.Text = row["Years_Of_Experience"].ToString();
                        txtProjects.Text = row["Projects_Count"].ToString();
                        txtSkillCount.Text = row["SkillCount"].ToString();
                        txtSkillsList.Text = row["SkillsList"].ToString();

                        try { ddlGender.SelectedValue = row["Gender"].ToString(); } catch { }
                        try { ddlDegree.SelectedValue = row["Degree"].ToString(); } catch { }
                        try { ddlStream.SelectedValue = row["Stream"].ToString(); } catch { }

                        btnSaveCandidate.Text = "CẬP NHẬT ĐÁNH GIÁ";
                        btnSaveCandidate.CssClass = "btn btn-warning fw-bold text-dark";
                        btnCancel.Visible = true; 
                    }
                }
            }
        }

        private void LoadAllComboBoxes()
        {
            try
            {
                BindDropDown(ddlDegree, "degree", "-- Chọn trình độ --");
                BindDropDown(ddlStream, "stream", "-- Chọn lĩnh vực --");
            }
            catch (Exception ex)
            {
                ShowAlert($"Lỗi nạp dữ liệu: {ex.Message}");
            }
        }

        private void BindDropDown(DropDownList ddl, string columnName, string defaultText)
        {
            string sql = $"SELECT DISTINCT {columnName} FROM [Candidate] WHERE {columnName} IS NOT NULL";
            DataTable dt = _xuly.getTable(sql);

            ddl.DataSource = dt;
            ddl.DataTextField = columnName;
            ddl.DataValueField = columnName;
            ddl.DataBind();

            ddl.Items.Insert(0, new ListItem(defaultText, "0"));
        }

        protected void btnAnalyze_Click(object sender, EventArgs e)
        {
            try
            {
                string degree = ddlDegree.SelectedValue;
                string stream = ddlStream.SelectedValue;

                double gpa = 0;
                string gpaInput = txtGPA.Text.Replace(",", ".");
                double.TryParse(gpaInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out gpa);

                int exp = int.TryParse(txtExp.Text, out var ex) ? ex : 0;
                int projects = int.TryParse(txtProjects.Text, out var p) ? p : 0;
                int skills = int.TryParse(txtSkillCount.Text, out var s) ? s : 0;

                if (degree == "0" || stream == "0")
                {
                    lblStatus.Text = "Vui lòng chọn đầy đủ Trình độ và Lĩnh vực!";
                    lblStatus.CssClass = "status-badge fw-bold text-danger";
                    lblPercentValue.InnerText = "ERR";
                    return;
                }

                string gpaStr = gpa.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture);
                string expStr = exp.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture);
                string projectsStr = projects.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture);

                Web_Recruitment_Management.App_Code.XuLyDuLieu db = new Web_Recruitment_Management.App_Code.XuLyDuLieu();

                // =========================================================
                // DECISION TREE
                // =========================================================
                string dmxDT = $@"
                SELECT 
                    Predict([Placement Status]) AS [Result], 
                    PredictProbability([Placement Status], 'Placed') AS [Prob]
                FROM [Candidate 1]
                NATURAL PREDICTION JOIN
                (SELECT 
                    '{degree.Replace("'", "''")}' AS [Degree], 
                    '{stream.Replace("'", "''")}' AS [Stream],
                    {gpaStr} AS [Gpa], 
                    {expStr} AS [Years Of Experience], 
                    {projectsStr} AS [Projects Count]
                ) AS t";

                DataTable dtDT = db.getPredicted(dmxDT);
                int percentDT = 0;

                if (dtDT != null && dtDT.Rows.Count > 0)
                {
                    double probValue = 0;
                    if (dtDT.Rows[0]["Prob"] != DBNull.Value)
                    {
                        probValue = Convert.ToDouble(dtDT.Rows[0]["Prob"]);
                    }

                    percentDT = (int)(Math.Round(probValue * 100));
                    string colorHex = "#dc3545"; // Mặc định Đỏ
                    if (percentDT >= 75) colorHex = "#198754";      // Xanh lá
                    else if (percentDT >= 50) colorHex = "#ffc107"; // Vàng

                    lblPercentValue.InnerText = $"{percentDT}%";
                    lblPercentValue.Style["color"] = colorHex;

                    divCircleProgress.Style["background"] = $"conic-gradient({colorHex} {percentDT}%, #e9ecef {percentDT}%)";

                    if (lblDT_Compare != null) lblDT_Compare.Text = $"{percentDT}%";

                    if (percentDT >= 50)
                    {
                        lblProbability.Text = $"Dự đoán: TRÚNG TUYỂN";
                        lblProbability.CssClass = "fw-bold text-success";
                    }
                    else
                    {
                        lblProbability.Text = $"Dự đoán KHÔNG ĐẠT";
                        lblProbability.CssClass = "fw-bold text-danger";
                    }
                }

                // =========================================================
                // LOGISTIC REGRESSION (MÔ HÌNH SO SÁNH)
                // =========================================================
                string dmxLR = $@"
                SELECT 
                    Predict([Placement Status]) AS [Result], 
                    PredictProbability([Placement Status], 'Placed') AS [Prob]
                FROM [Candidate_LR] 
                NATURAL PREDICTION JOIN
                (SELECT 
                    '{degree.Replace("'", "''")}' AS [Degree], 
                    '{stream.Replace("'", "''")}' AS [Stream],
                    {gpaStr} AS [Gpa], 
                    {expStr} AS [Years Of Experience], 
                    {projectsStr} AS [Projects Count]
                ) AS t";

                try
                {
                    DataTable dtLR = db.getPredicted(dmxLR);
                    if (dtLR != null && dtLR.Rows.Count > 0)
                    {
                        double probLR = 0;
                        if (dtLR.Rows[0]["Prob"] != DBNull.Value)
                        {
                            probLR = Convert.ToDouble(dtLR.Rows[0]["Prob"]);
                        }

                        int percentLR = (int)(Math.Round(probLR * 100));
                        if (lblLR_Compare != null) lblLR_Compare.Text = $"{percentLR}%";

                        // ---------------------------------------------------------
                        // TÍNH TOÁN ĐỘNG: MỨC ĐỘ ẢNH HƯỞNG TỪNG TIÊU CHÍ
                        // ---------------------------------------------------------
                        double weightGPA = 0.45;
                        double weightExp = 0.35;
                        double weightProj = 0.20;

                        double impactGPA = gpa * weightGPA;
                        double impactExp = exp * weightExp;
                        double impactProj = projects * weightProj;

                        double totalImpact = impactGPA + impactExp + impactProj;

                        if (totalImpact > 0 && litFeatureImpact != null)
                        {
                            int percentGPA = (int)Math.Round((impactGPA / totalImpact) * 100);
                            int percentExp = (int)Math.Round((impactExp / totalImpact) * 100);
                            int percentProj = 100 - percentGPA - percentExp;

                            litFeatureImpact.Text = $@"
                            <div class='mb-2 mt-2'>
                                <div class='d-flex justify-content-between text-secondary mb-1' style='font-size: 0.8rem;'>
                                    <span class='fw-semibold'>GPA</span>
                                    <span class='fw-bold text-success'>{percentGPA}%</span>
                                </div>
                                <div class='progress' style='height: 8px;'>
                                    <div class='progress-bar bg-success' role='progressbar' style='width: {percentGPA}%'></div>
                                </div>
                            </div>

                            <div class='mb-2'>
                                <div class='d-flex justify-content-between text-secondary mb-1' style='font-size: 0.8rem;'>
                                    <span class='fw-semibold'>Kinh nghiệm</span>
                                    <span class='fw-bold text-info'>{percentExp}%</span>
                                </div>
                                <div class='progress' style='height: 8px;'>
                                    <div class='progress-bar bg-info' role='progressbar' style='width: {percentExp}%'></div>
                                </div>
                            </div>

                            <div class='mb-1'>
                                <div class='d-flex justify-content-between text-secondary mb-1' style='font-size: 0.8rem;'>
                                    <span class='fw-semibold'>Dự án</span>
                                    <span class='fw-bold text-warning'>{percentProj}%</span>
                                </div>
                                <div class='progress' style='height: 8px;'>
                                    <div class='progress-bar bg-warning' role='progressbar' style='width: {percentProj}%'></div>
                                </div>
                            </div>";
                        }
                        else if (litFeatureImpact != null)
                        {
                            litFeatureImpact.Text = "<div class='progress-bar bg-secondary' style='width: 100%'>Chưa đủ dữ liệu tính toán</div>";
                        }
                    }
                }
                catch (Exception)
                {
                    if (lblLR_Compare != null) lblLR_Compare.Text = "Đang cập nhật Model";
                    if (litFeatureImpact != null) litFeatureImpact.Text = "<div class='progress-bar bg-secondary' style='width: 100%'>Đang cập nhật Model</div>";
                }

                // =========================================================
                // CLUSTERING 
                // =========================================================
                string rawCluster = db.DuDoanNhomUngVien(gpa, exp, stream, projects, skills);

                string clusterName = "";
                string cssClass = "";

                switch (rawCluster.Trim())
                {
                    // --- NHÓM SĂN ĐÓN ---
                    case "Cluster 8":
                    case "Cluster 9":
                        clusterName = "Senior - Dày dặn kinh nghiệm";
                        cssClass = "status-badge fw-bold text-success border border-success";
                        break;
                    case "Cluster 10":
                        clusterName = "Expert - Kỹ năng vượt trội";
                        cssClass = "status-badge fw-bold text-success border border-success";
                        break;

                    // --- NHÓM TÀI NĂNG TRẺ ---
                    case "Cluster 4":
                        clusterName = "Fresher - IT Xuất Sắc";
                        cssClass = "status-badge fw-bold text-primary";
                        break;
                    case "Cluster 5":
                        clusterName = "Fresher - Kỹ thuật Xuất Sắc";
                        cssClass = "status-badge fw-bold text-primary";
                        break;

                    // --- NHÓM THỰC CHIẾN / TIỀM NĂNG ---
                    case "Cluster 7":
                        clusterName = "Mid-level Thực chiến";
                        cssClass = "status-badge fw-bold text-info";
                        break;
                    case "Cluster 3":
                        clusterName = "Junior - Tiềm Năng";
                        cssClass = "status-badge fw-bold text-info";
                        break;
                    case "Cluster 1":
                        clusterName = "Ứng viên Phổ thông";
                        cssClass = "status-badge fw-bold text-secondary";
                        break;

                    // --- NHÓM CẦN ĐÀO TẠO / CÂN NHẮC ---
                    case "Cluster 6":
                        clusterName = "Fresher - Tiêu Chuẩn";
                        cssClass = "status-badge fw-bold text-warning";
                        break;
                    case "Cluster 2":
                        clusterName = "Fresher - Cần đào tạo thêm";
                        cssClass = "status-badge fw-bold text-warning border border-warning";
                        break;

                    default:
                        clusterName = rawCluster != "Chưa xác định" ? rawCluster : "Chưa xác định";
                        cssClass = "status-badge fw-bold text-dark";
                        break;
                }

                lblStatus.Text = "Phân loại: " + clusterName;
                lblStatus.CssClass = cssClass;

                hdfCluster.Value = clusterName;
                hdfStatus.Value = percentDT >= 50 ? "Placed" : "Not Placed";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Lỗi thực thi Hệ thống: {ex.Message}";
                lblStatus.CssClass = "status-badge fw-bold text-danger";
                lblPercentValue.InnerText = "ERR";
            }
        }

        private void DisplayResult(DataTable dt)
        {
            if (dt?.Rows.Count > 0)
            {
                string prediction = dt.Rows[0]["Result"].ToString();
                double probability = 0;

                if (dt.Rows[0]["Prob"] != DBNull.Value)
                {
                    probability = Convert.ToDouble(dt.Rows[0]["Prob"]) * 100;
                }

                lblPercentValue.InnerText = probability.ToString("0.00") + "%";

                lblProbability.Text = $"Độ phù hợp: {probability:0.00}%";

                bool isPlaced = prediction.Equals("Placed", StringComparison.OrdinalIgnoreCase);
                lblStatus.Text = isPlaced ? "Trạng thái: <span style='color:green'>TRÚNG TUYỂN</span>"
                                          : "Trạng thái: <span style='color:red'>KHÔNG ĐẠT</span>";
            }
            else
            {
                lblStatus.Text = "Không thể đưa ra dự báo. Vui lòng kiểm tra lại dữ liệu.";
            }
        }

        private void ShowAlert(string message)
        {
            string cleanMessage = message.Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "");
            ClientScript.RegisterStartupScript(this.GetType(), "alertMessage", $"alert('{cleanMessage}');", true);
        }

        protected void btnSaveCandidate_Click(object sender, EventArgs e)
        {
            try
            {
                string cluster = hdfCluster.Value;
                string status = hdfStatus.Value;

                if (string.IsNullOrEmpty(cluster))
                {
                    ShowAlert("Vui lòng ấn Phân tích để AI đánh giá trước khi lưu!");
                    return;
                }

                string degree = ddlDegree.SelectedValue;
                string stream = ddlStream.SelectedValue;

                double gpa = 0;
                double.TryParse(txtGPA.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out gpa);

                int exp = int.TryParse(txtExp.Text, out int ex) ? ex : 0;
                int projects = int.TryParse(txtProjects.Text, out int p) ? p : 0;
                int skills = int.TryParse(txtSkillCount.Text, out int s) ? s : 0;
                double age = double.TryParse(txtAge.Text, out double ag) ? ag : 0;

                string name = txtName.Text.Trim();
                string gender = ddlGender.SelectedValue;
                string college = txtCollege.Text.Trim();
                string email = txtEmail.Text.Trim();

                string skillsList = txtSkillsList.Text.Trim();

                bool success = false;

                if (!string.IsNullOrEmpty(hdfReAppId.Value))
                {
                    int appId = Convert.ToInt32(hdfReAppId.Value);
                    int candidateId = Convert.ToInt32(hdfReCandidateId.Value);

                    success = _xuly.CapNhatSauDanhGiaLai(appId, candidateId, name, gender, age, degree, stream, college, gpa, exp, projects, skills, email, status, cluster, skillsList);
                }
                else
                {
                    success = _xuly.LuuUngVienMoi(name, gender, age, degree, stream, college, gpa, exp, projects, skills, email, status, cluster, skillsList);
                }

                if (success)
                {
                    string msg = string.IsNullOrEmpty(hdfReAppId.Value) ? "Đã lưu thành công ứng viên mới" : "Đã cập nhật đánh giá AI thành công";
                    string script = $@"
                alert('{msg} vào cụm: {cluster}');
                window.location.href = 'CandidateList.aspx';
            ";
                    ClientScript.RegisterStartupScript(this.GetType(), "successSave", script, true);
                }
                else
                {
                    ShowAlert("Lưu thất bại! Hãy kiểm tra lại dữ liệu hoặc kết nối CSDL.");
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"Hệ thống báo lỗi: {ex.Message}");
            }
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("CandidateList.aspx");
        }
    }
}