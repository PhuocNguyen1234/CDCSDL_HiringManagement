using System;
using System.Data;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using Microsoft.AnalysisServices.AdomdClient;

namespace Web_Recruitment_Management.App_Code
{
    public class CollegePassResult
    {
        public string CollegeName { get; set; }
        public int PassCount { get; set; }
    }
    public partial class XuLyDuLieu
    {
        public string DuDoanNhomUngVien(double gpa, int yearsOfExperience, string stream, int projectsCount, int skillCount)
        {
            string ketQuaCachGoi = "Chưa xác định";

            string gpaStr = gpa.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture);
            string expStr = yearsOfExperience.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture);
            string projStr = projectsCount.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture);
            string skillStr = skillCount.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture);

            string dmxQuery = $@"
                SELECT Cluster() AS [Nhom_Ung_Vien]
                FROM [ViewMiningCandidateData]
                NATURAL PREDICTION JOIN
                (
                    SELECT 
                        {gpaStr} AS [Gpa], 
                        {expStr} AS [Years Of Experience], 
                        '{stream.Replace("'", "''")}' AS [Stream],
                        {projStr} AS [Projects Count],
                        {skillStr} AS [Skill Count]
                ) AS t";

            try
            {
                DataTable dtKetQua = this.getPredicted(dmxQuery);
                if (dtKetQua != null && dtKetQua.Rows.Count > 0)
                {
                    ketQuaCachGoi = dtKetQua.Rows[0]["Nhom_Ung_Vien"].ToString();
                }
            }
            catch (Exception ex)
            {
                return "Lỗi phân cụm: " + ex.Message;
            }

            return ketQuaCachGoi;
        }

        public List<CollegePassResult> GetPassPredictionByCollege()
        {
            var resultList = new List<CollegePassResult>();

            // Câu lệnh MDX: Lấy danh sách các trường và sắp xếp giảm dần theo số ứng viên Pass
            string mdxQuery = @"
                SELECT 
                    { [Measures].[Fact Applications Count] } ON COLUMNS,
                    NON EMPTY 
                    {
                        Order(
                            [Dim_Candidates].[College Name].Children,
                            [Measures].[Fact Applications Count],
                            DESC
                        ) * [Dim_DT_AI].[Placement Status].[Pass]
                    } ON ROWS
                FROM [Recruitment_Cube]";

            try
            {
                DataTable dtKetQua = this.getPredicted(mdxQuery);

                if (dtKetQua != null && dtKetQua.Rows.Count > 0)
                {
                    foreach (DataRow row in dtKetQua.Rows)
                    {
                        // Trong DataTable trả về từ MDX:
                        // row[0] là Tên trường (College Name)
                        // row[1] là Trạng thái (Pass)
                        // row[2] là Số lượng (Measure)

                        resultList.Add(new CollegePassResult
                        {
                            CollegeName = row[0].ToString(),
                            PassCount = row[2] == DBNull.Value ? 0 : Convert.ToInt32(row[2])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy dữ liệu Dashboard: " + ex.Message);
            }

            return resultList;
        }
    }
}