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
                double gpa = double.TryParse(txtGPA.Text, out var g) ? g : 0;
                int exp = int.TryParse(txtExp.Text, out var ex) ? ex : 0;
                int projects = int.TryParse(txtProjects.Text, out var p) ? p : 0;

                if (degree == "0" || stream == "0")
                {
                    lblStatus.Text = "<span style='color:red'>Vui lòng chọn đầy đủ Trình độ và Lĩnh vực!</span>";
                    return;
                }

                string dmx = $@"
                SELECT 
                    Predict([Placement Status]) AS [Result], 
                    PredictProbability([Placement Status], 'Placed') AS [Prob]
                FROM [Candidate 1]
                NATURAL PREDICTION JOIN
                (SELECT 
                    '{degree.Replace("'", "''")}' AS [Degree], 
                    '{stream.Replace("'", "''")}' AS [Stream],
                    {gpa} AS [Gpa], 
                    {exp} AS [Years Of Experience], 
                    {projects} AS [Projects Count]
                ) AS t";

                DataTable dt = _xuly.getPredicted(dmx);

                DisplayResult(dt);
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"<span style='color:orange'>Lỗi thực thi: </span>{ex.Message}";
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

                litProb.Text = probability.ToString("0.00") + "%";

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
            Response.Write($"<script>alert('{message.Replace("'", "\\'")}');</script>");
        }
    }
}