using System;
using System.Data;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using Microsoft.AnalysisServices.AdomdClient;

namespace Web_Recruitment_Management.App_Code
{
    public class CollegeSkillDNA
    {
        public string CollegeName { get; set; }
        public string SkillName { get; set; }
        public int CandidateCount { get; set; }
    }
    public class CollegeWeaknessResult
    {
        public string CollegeName { get; set; }
        public string WeaknessRule { get; set; }
        public int CandidateCount { get; set; }
        public int TotalCount { get; set; }
    }
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

            string mdxQuery = @"
                SELECT 
                    { [Measures].[Fact Applications Count] } ON COLUMNS,
                    NON EMPTY 
                    {
                        Order(
                            [Dim_Candidates].[College Name].Children,
                            [Measures].[Fact Applications Count],
                            DESC
                        ) * [Fact_Applications].[Placement Status].[Placed]
                    } ON ROWS
                FROM [Recruitment_Cube]";

            try
            {
                DataTable dtKetQua = this.getPredicted(mdxQuery);

                if (dtKetQua != null && dtKetQua.Rows.Count > 0)
                {
                    foreach (DataRow row in dtKetQua.Rows)
                    {

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

        public List<CollegeWeaknessResult> GetCollegeWeaknessAnalysis()
        {
            var resultList = new List<CollegeWeaknessResult>();
            string mdxQuery = @"
                WITH MEMBER [Measures].[Total College Apps] AS
                    Sum(
                        [Dim_DT_AI].[Placement Status].CurrentMember.Siblings, 
                        [Measures].[Fact Applications Count]
                    )
                SELECT 
                    { [Measures].[Fact Applications Count], [Measures].[Total College Apps] } ON COLUMNS,
                    NON EMPTY 
                    {
                        TopCount(
                            [Dim_Candidates].[College Name].Children * [Dim_DT_AI].[Placement Status].Children,
                            15,
                            [Measures].[Fact Applications Count]
                        )
                    } ON ROWS
                FROM [Recruitment_Cube]";

            try
            {
                DataTable dt = this.getPredicted(mdxQuery);
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string rawRule = row[1] != DBNull.Value ? row[1].ToString() : "";

                        string friendlyRule = rawRule;
                        if (rawRule.Contains("Projects Count <"))
                        {
                            friendlyRule = "Thiếu dự án thực tế";
                        }
                        else if (rawRule.Contains("GPA <"))
                        {
                            friendlyRule = "Điểm trung bình (GPA) thấp";
                        }
                        else if (rawRule.Contains("Years Of Experience <"))
                        {
                            friendlyRule = "Kinh nghiệm làm việc chưa đủ";
                        }

                        if (string.IsNullOrWhiteSpace(friendlyRule))
                        {
                            friendlyRule = "Không xác định rõ";
                        }

                        if (string.IsNullOrWhiteSpace(friendlyRule)) { friendlyRule = "Không xác định rõ"; }

                        resultList.Add(new CollegeWeaknessResult
                        {
                            CollegeName = row[0] != DBNull.Value ? row[0].ToString() : "Chưa cập nhật",
                            WeaknessRule = friendlyRule,
                            CandidateCount = row[2] == DBNull.Value ? 0 : Convert.ToInt32(row[2]),

                            TotalCount = row[3] == DBNull.Value ? 0 : Convert.ToInt32(row[3])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi phân tích điểm yếu: " + ex.Message);
            }
            return resultList;
        }

        public List<CollegeSkillDNA> GetCollegeSkillDNA()
        {
            var resultList = new List<CollegeSkillDNA>();

            string mdxQuery = @"
                SELECT 
                    { [Measures].[Fact Candidate Skills Count] } ON COLUMNS,
                    NON EMPTY 
                    Generate(
                        [Dim_Candidates].[College Name].Children,
                        CrossJoin(
                            { [Dim_Candidates].[College Name].CurrentMember },
                            TopCount(
                                [Dim_Skills].[Skill Name].Children, 
                                3, 
                                [Measures].[Fact Candidate Skills Count]
                            )
                        )
                    ) ON ROWS
                FROM [Recruitment_Cube]
                WHERE ( [Fact_Applications].[Placement Status].[Placed] )";

            try
            {
                DataTable dt = this.getPredicted(mdxQuery);
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        resultList.Add(new CollegeSkillDNA
                        {
                            CollegeName = row[0] != DBNull.Value ? row[0].ToString() : "Chưa cập nhật",
                            SkillName = row[1] != DBNull.Value ? row[1].ToString() : "Không có dữ liệu",
                            CandidateCount = row[2] == DBNull.Value ? 0 : Convert.ToInt32(row[2])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy DNA Kỹ năng: " + ex.Message);
            }

            return resultList;
        }
    }
}