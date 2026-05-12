using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services; 
using Web_Recruitment_Management.App_Code; 

namespace Web_Recruitment_Management
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        [WebMethod]
        public static List<CollegePassResult> GetChartData()
        {
            // Vì hàm này là static, ta phải khởi tạo đối tượng XuLyDuLieu mới dùng được
            XuLyDuLieu dataHelper = new XuLyDuLieu();
            return dataHelper.GetPassPredictionByCollege();
        }

        [WebMethod]
        public static List<CollegeWeaknessResult> GetWeaknessData()
        {
            // Khởi tạo class chứa hàm truy vấn MDX
            XuLyDuLieu xl = new XuLyDuLieu();

            // Gọi hàm lấy dữ liệu điểm yếu mà chúng ta vừa viết lúc nãy
            return xl.GetCollegeWeaknessAnalysis();
        }
    }
}