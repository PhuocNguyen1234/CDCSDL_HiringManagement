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
            XuLyDuLieu dataHelper = new XuLyDuLieu();
            return dataHelper.GetPassPredictionByCollege();
        }

        [WebMethod]
        public static List<CollegeWeaknessResult> GetWeaknessData()
        {
            XuLyDuLieu xl = new XuLyDuLieu();
            return xl.GetCollegeWeaknessAnalysis();
        }

        [WebMethod]
        public static List<CollegeSkillDNA> GetDNAChartData()
        {
            XuLyDuLieu dataHelper = new XuLyDuLieu();
            return dataHelper.GetCollegeSkillDNA();
        }
    }
}