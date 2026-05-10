using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using Microsoft.AnalysisServices.AdomdClient;

namespace Web_Recruitment_Management.App_Code
{
    public class XuLyDuLieu
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

        public int LayIdJobTuStream(string streamName)
        {
            try
            {
                Open();
                string sql = "SELECT TOP 1 JobID FROM Dim_Jobs WHERE JobTitle = ?";
                OleDbCommand cmd = new OleDbCommand(sql, CON);
                cmd.Parameters.AddWithValue("JobTitle", streamName);

                object obj = cmd.ExecuteScalar();
                Close();

                if (obj != null && obj != DBNull.Value)
                {
                    return Convert.ToInt32(obj);
                }
                return 0;
            }
            catch
            {
                Close();
                return 0;
            }
        }
        public bool LuuUngVienMoi(string name, string gender, double age, string degree, string stream, 
            string college, double gpa, int exp, int projects, int skills, string email, string status, 
            string clusterName, string skillsList)
        {
            double predictProb = 0;
            string placementStatus = status;
            if (status != null && status.Contains("|"))
            {
                string[] parts = status.Split('|');
                placementStatus = parts[0];
                double.TryParse(parts[1], out predictProb);
            }

            OleDbTransaction transaction = null;
            try
            {
                Open();
                transaction = CON.BeginTransaction();

                // 1. CHÈN VÀO DIM_CANDIDATES
                string sqlDim = "INSERT INTO Dim_Candidates (FullName, Gender, Age, Degree, Stream, College_Name, Email) VALUES (?, ?, ?, ?, ?, ?, ?)";
                OleDbCommand cmdDim = new OleDbCommand(sqlDim, CON, transaction);
                cmdDim.Parameters.AddWithValue("FullName", name ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("Gender", gender ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("Age", age);
                cmdDim.Parameters.AddWithValue("Degree", degree ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("Stream", stream ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("College", college ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("Email", email ?? (object)DBNull.Value);
                cmdDim.ExecuteNonQuery();

                cmdDim.CommandText = "SELECT @@IDENTITY";
                int newCandidateID = Convert.ToInt32(cmdDim.ExecuteScalar());

                // 2. CHÈN VÀO FACT_APPLICATIONS 
                string sqlFact = @"INSERT INTO Fact_Applications (CandidateID, GPA, Projects_Count, Years_Of_Experience, Placement_Status, Prediction_Probability, AI_Cluster_Group, Final_Result, ApplyDate)
                                   VALUES (?, ?, ?, ?, ?, ?, ?, NULL, GETDATE())";
                OleDbCommand cmdFact = new OleDbCommand(sqlFact, CON, transaction);
                cmdFact.Parameters.AddWithValue("CandidateID", newCandidateID);
                cmdFact.Parameters.AddWithValue("GPA", gpa);
                cmdFact.Parameters.AddWithValue("Projects", projects);
                cmdFact.Parameters.AddWithValue("Exp", exp);
                cmdFact.Parameters.AddWithValue("Status", placementStatus);
                cmdFact.Parameters.AddWithValue("Prob", predictProb);
                cmdFact.Parameters.AddWithValue("AIClusterGroup", clusterName);
                cmdFact.ExecuteNonQuery();

                // 3. XỬ LÝ CHUỖI KỸ NĂNG (NẾU CÓ NHẬP)
                if (!string.IsNullOrWhiteSpace(skillsList))
                {
                    string[] skillArray = skillsList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string s in skillArray)
                    {
                        string skillName = s.Trim(); 
                        if (string.IsNullOrEmpty(skillName)) continue;

                        int skillId = 0;

                        string checkSql = "SELECT TOP 1 SkillID FROM Dim_Skills WHERE SkillName = ?";
                        OleDbCommand cmdCheck = new OleDbCommand(checkSql, CON, transaction);
                        cmdCheck.Parameters.AddWithValue("SkillName", skillName);
                        object objSkillId = cmdCheck.ExecuteScalar();

                        if (objSkillId != null && objSkillId != DBNull.Value)
                        {
                            skillId = Convert.ToInt32(objSkillId);
                        }
                        else
                        {
                            string insertSkill = "INSERT INTO Dim_Skills (SkillName, SkillGroup) VALUES (?, 'Other')";
                            OleDbCommand cmdInsertSkill = new OleDbCommand(insertSkill, CON, transaction);
                            cmdInsertSkill.Parameters.AddWithValue("SkillName", skillName);
                            cmdInsertSkill.ExecuteNonQuery();

                            cmdInsertSkill.CommandText = "SELECT @@IDENTITY";
                            skillId = Convert.ToInt32(cmdInsertSkill.ExecuteScalar());
                        }

                        string insertFactSkill = "INSERT INTO Fact_Candidate_Skills (CandidateID, SkillID) VALUES (?, ?)";
                        OleDbCommand cmdFactSkill = new OleDbCommand(insertFactSkill, CON, transaction);
                        cmdFactSkill.Parameters.AddWithValue("CandidateID", newCandidateID);
                        cmdFactSkill.Parameters.AddWithValue("SkillID", skillId);
                        cmdFactSkill.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                Close();
                return true;
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                Close();
                throw new Exception(ex.Message);
            }
        }

        public bool XoaUngVien(int applicationID)
        {
            OleDbTransaction transaction = null;
            try
            {
                Open();
                transaction = CON.BeginTransaction();

                // 1. Lấy CandidateID từ ApplicationID trước khi bảng Fact bị xóa
                string sqlGetId = "SELECT CandidateID FROM Fact_Applications WHERE ApplicationID = " + applicationID;
                OleDbCommand cmdGet = new OleDbCommand(sqlGetId, CON, transaction);
                object objCandidateId = cmdGet.ExecuteScalar();

                if (objCandidateId != null && objCandidateId != DBNull.Value)
                {
                    int candidateId = Convert.ToInt32(objCandidateId);

                    // 2. Xóa ở bảng Kỹ năng ứng viên (Nếu bạn có dùng bảng này)
                    string sqlSkills = "DELETE FROM Fact_Candidate_Skills WHERE CandidateID = " + candidateId;
                    OleDbCommand cmdSkills = new OleDbCommand(sqlSkills, CON, transaction);
                    cmdSkills.ExecuteNonQuery();

                    // 3. Xóa ở bảng Fact_Applications
                    string sqlFact = "DELETE FROM Fact_Applications WHERE ApplicationID = " + applicationID;
                    OleDbCommand cmdFact = new OleDbCommand(sqlFact, CON, transaction);
                    cmdFact.ExecuteNonQuery();

                    // 4. Cuối cùng mới xóa ở bảng Dim_Candidates
                    string sqlDim = "DELETE FROM Dim_Candidates WHERE CandidateID = " + candidateId;
                    OleDbCommand cmdDim = new OleDbCommand(sqlDim, CON, transaction);
                    cmdDim.ExecuteNonQuery();
                }

                // Xác nhận hoàn tất chuỗi lệnh xóa
                transaction.Commit();
                Close();
                return true;
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                Close();
                return false;
            }
        }
        public DataTable GetFullDetail(int applicationId)
        {
            string sql = @"
                SELECT 
                    C.CandidateID, 
                    C.FullName,  -- Đã sửa thành FullName
                    C.Email, 
                    C.College_Name, 
                    ISNULL(J.JobTitle, N'Chưa ứng tuyển') AS JobTitle,
                    
                    -- Dùng truy vấn con để gom tên kỹ năng lại
                    ISNULL((
                        SELECT STRING_AGG(S.SkillName, ', ') 
                        FROM Fact_Candidate_Skills FS 
                        INNER JOIN Dim_Skills S ON FS.SkillID = S.SkillID
                        WHERE FS.CandidateID = C.CandidateID
                    ), N'Không có dữ liệu') AS Skills

                FROM Fact_Applications A
                INNER JOIN Dim_Candidates C ON A.CandidateID = C.CandidateID
                LEFT JOIN Dim_Jobs J ON A.JobID = J.JobID
                WHERE A.ApplicationID = " + applicationId;

            return getTable(sql);
        }
        public DataTable LấyThongTinUngVienDeSua(int appId)
        {
            string sql = @"
                SELECT A.ApplicationID, A.CandidateID, C.FullName, C.Email, C.College_Name, 
                       A.GPA, A.Years_Of_Experience, A.Projects_Count,
                       (SELECT STRING_AGG(S.SkillName, ', ') 
                        FROM Fact_Candidate_Skills FS 
                        INNER JOIN Dim_Skills S ON FS.SkillID = S.SkillID
                        WHERE FS.CandidateID = C.CandidateID) AS Skills
                FROM Fact_Applications A
                INNER JOIN Dim_Candidates C ON A.CandidateID = C.CandidateID
                WHERE A.ApplicationID = " + appId;
            return getTable(sql);
        }



        // 2. Hàm thực thi lệnh UPDATE dữ liệu
        public bool CapNhatUngVien(int candidateId, int appId, string name, string email, string college, double gpa, int exp, int projects, string skillsList)
        {
            OleDbTransaction transaction = null;
            try
            {
                Open();
                transaction = CON.BeginTransaction();

                // Cập nhật bảng Thông tin cá nhân (Dim)
                string sqlDim = "UPDATE Dim_Candidates SET FullName = ?, Email = ?, College_Name = ? WHERE CandidateID = ?";
                OleDbCommand cmdDim = new OleDbCommand(sqlDim, CON, transaction);
                cmdDim.Parameters.AddWithValue("FullName", name ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("Email", email ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("College", college ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("CandidateID", candidateId);
                cmdDim.ExecuteNonQuery();

                // Cập nhật bảng Ứng tuyển (Fact)
                string sqlFact = "UPDATE Fact_Applications SET GPA = ?, Years_Of_Experience = ?, Projects_Count = ? WHERE ApplicationID = ?";
                OleDbCommand cmdFact = new OleDbCommand(sqlFact, CON, transaction);
                cmdFact.Parameters.AddWithValue("GPA", gpa);
                cmdFact.Parameters.AddWithValue("Exp", exp);
                cmdFact.Parameters.AddWithValue("Projects", projects);
                cmdFact.Parameters.AddWithValue("ApplicationID", appId);
                cmdFact.ExecuteNonQuery();

                // CẬP NHẬT KỸ NĂNG:
                // A. Xóa toàn bộ kỹ năng cũ của ứng viên này trong bảng nối
                string sqlDeleteSkills = "DELETE FROM Fact_Candidate_Skills WHERE CandidateID = ?";
                OleDbCommand cmdDelSkills = new OleDbCommand(sqlDeleteSkills, CON, transaction);
                cmdDelSkills.Parameters.AddWithValue("CandidateID", candidateId);
                cmdDelSkills.ExecuteNonQuery();

                // B. Thêm lại danh sách kỹ năng mới (Nếu có nhập)
                if (!string.IsNullOrWhiteSpace(skillsList))
                {
                    string[] skillArray = skillsList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in skillArray)
                    {
                        string skillName = s.Trim();
                        if (string.IsNullOrEmpty(skillName)) continue;

                        int skillId = 0;
                        // Kiểm tra kỹ năng đã tồn tại trong thư viện chưa
                        string checkSql = "SELECT TOP 1 SkillID FROM Dim_Skills WHERE SkillName = ?";
                        OleDbCommand cmdCheck = new OleDbCommand(checkSql, CON, transaction);
                        cmdCheck.Parameters.AddWithValue("SkillName", skillName);
                        object objSkillId = cmdCheck.ExecuteScalar();

                        if (objSkillId != null && objSkillId != DBNull.Value)
                        {
                            skillId = Convert.ToInt32(objSkillId);
                        }
                        else
                        {
                            // Nếu chưa có thì Insert kỹ năng mới
                            string insertSkill = "INSERT INTO Dim_Skills (SkillName, SkillGroup) VALUES (?, 'Other')";
                            OleDbCommand cmdInsertSkill = new OleDbCommand(insertSkill, CON, transaction);
                            cmdInsertSkill.Parameters.AddWithValue("SkillName", skillName);
                            cmdInsertSkill.ExecuteNonQuery();

                            cmdInsertSkill.CommandText = "SELECT @@IDENTITY";
                            skillId = Convert.ToInt32(cmdInsertSkill.ExecuteScalar());
                        }

                        // Nối kỹ năng với ứng viên
                        string insertFactSkill = "INSERT INTO Fact_Candidate_Skills (CandidateID, SkillID) VALUES (?, ?)";
                        OleDbCommand cmdFactSkill = new OleDbCommand(insertFactSkill, CON, transaction);
                        cmdFactSkill.Parameters.AddWithValue("CandidateID", candidateId);
                        cmdFactSkill.Parameters.AddWithValue("SkillID", skillId);
                        cmdFactSkill.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                Close();
                return true;
            }
            catch
            {
                if (transaction != null) transaction.Rollback();
                Close();
                return false;
            }
        }

        public DataTable LayDuLieuDeDanhGiaLai(int appId)
        {
            string sql = @"
                SELECT A.ApplicationID, A.CandidateID, C.FullName, C.Email, C.College_Name, 
                       C.Gender, C.Age, C.Degree, C.Stream,
                       A.GPA, A.Years_Of_Experience, A.Projects_Count,
                       (SELECT COUNT(*) FROM Fact_Candidate_Skills FS WHERE FS.CandidateID = C.CandidateID) AS SkillCount,
                       (SELECT STRING_AGG(S.SkillName, ', ') FROM Fact_Candidate_Skills FS INNER JOIN Dim_Skills S ON FS.SkillID = S.SkillID WHERE FS.CandidateID = C.CandidateID) AS SkillsList
                FROM Fact_Applications A
                INNER JOIN Dim_Candidates C ON A.CandidateID = C.CandidateID
                WHERE A.ApplicationID = " + appId;
            return getTable(sql);
        }

        // 2. Hàm Cập nhật CSDL sau khi AI đã đánh giá lại xong
        public bool CapNhatSauDanhGiaLai(int appId, int candidateId, string name, string gender, double age, string degree, string stream, string college, double gpa, int exp, int projects, int skills, string email, string status, string clusterName, string skillsList)
        {
            double predictProb = 0;
            string placementStatus = status;
            if (status != null && status.Contains("|"))
            {
                string[] parts = status.Split('|');
                placementStatus = parts[0];
                double.TryParse(parts[1], out predictProb);
            }

            OleDbTransaction transaction = null;
            try
            {
                Open();
                transaction = CON.BeginTransaction();

                // Cập nhật bảng Dim
                string sqlDim = "UPDATE Dim_Candidates SET FullName=?, Gender=?, Age=?, Degree=?, Stream=?, College_Name=?, Email=? WHERE CandidateID=?";
                OleDbCommand cmdDim = new OleDbCommand(sqlDim, CON, transaction);
                cmdDim.Parameters.AddWithValue("FullName", name ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("Gender", gender ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("Age", age);
                cmdDim.Parameters.AddWithValue("Degree", degree ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("Stream", stream ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("College", college ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("Email", email ?? (object)DBNull.Value);
                cmdDim.Parameters.AddWithValue("CandidateID", candidateId);
                cmdDim.ExecuteNonQuery();

                // Cập nhật bảng Fact (Bao gồm cả Cụm AI và Tỉ lệ mới)
                string sqlFact = "UPDATE Fact_Applications SET GPA=?, Years_Of_Experience=?, Projects_Count=?, Placement_Status=?, Prediction_Probability=?, AI_Cluster_Group=? WHERE ApplicationID=?";
                OleDbCommand cmdFact = new OleDbCommand(sqlFact, CON, transaction);
                cmdFact.Parameters.AddWithValue("GPA", gpa);
                cmdFact.Parameters.AddWithValue("Exp", exp);
                cmdFact.Parameters.AddWithValue("Projects", projects);
                cmdFact.Parameters.AddWithValue("Status", placementStatus);
                cmdFact.Parameters.AddWithValue("Prob", predictProb);
                cmdFact.Parameters.AddWithValue("AIClusterGroup", clusterName);
                cmdFact.Parameters.AddWithValue("ApplicationID", appId);
                cmdFact.ExecuteNonQuery();

                // Xóa kỹ năng cũ và thêm kỹ năng mới (Tương tự code update kỹ năng bạn đã có)
                OleDbCommand cmdDelSkills = new OleDbCommand("DELETE FROM Fact_Candidate_Skills WHERE CandidateID=?", CON, transaction);
                cmdDelSkills.Parameters.AddWithValue("CandidateID", candidateId);
                cmdDelSkills.ExecuteNonQuery();

                if (!string.IsNullOrWhiteSpace(skillsList))
                {
                    string[] skillArray = skillsList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in skillArray)
                    {
                        string skillName = s.Trim();
                        if (string.IsNullOrEmpty(skillName)) continue;
                        int skillId = 0;
                        object objSkillId = new OleDbCommand($"SELECT TOP 1 SkillID FROM Dim_Skills WHERE SkillName = '{skillName}'", CON, transaction).ExecuteScalar();
                        if (objSkillId != null) skillId = Convert.ToInt32(objSkillId);
                        else
                        {
                            OleDbCommand cmdInsert = new OleDbCommand("INSERT INTO Dim_Skills (SkillName, SkillGroup) VALUES (?, 'Other')", CON, transaction);
                            cmdInsert.Parameters.AddWithValue("SkillName", skillName);
                            cmdInsert.ExecuteNonQuery();
                            skillId = Convert.ToInt32(new OleDbCommand("SELECT @@IDENTITY", CON, transaction).ExecuteScalar());
                        }
                        OleDbCommand cmdFactSkill = new OleDbCommand("INSERT INTO Fact_Candidate_Skills (CandidateID, SkillID) VALUES (?, ?)", CON, transaction);
                        cmdFactSkill.Parameters.AddWithValue("CandidateID", candidateId);
                        cmdFactSkill.Parameters.AddWithValue("SkillID", skillId);
                        cmdFactSkill.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
                Close();
                return true;
            }
            catch { if (transaction != null) transaction.Rollback(); Close(); return false; }
        }
    }
}