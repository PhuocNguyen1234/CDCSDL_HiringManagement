<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CandidateFiltering.aspx.cs" Inherits="Web_Recruitment_Management.CandidateFiltering" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .main-card { background: #ffffff; border: none; border-radius: 12px; box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1); overflow: hidden; }
        .card-header-custom { background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); color: white; padding: 20px; font-weight: 700; font-size: 1.25rem; }
        .form-label { font-weight: 600; color: #475569; margin-bottom: 0.5rem; }
        .btn-analyze { background: #2563eb; border: none; border-radius: 8px; padding: 12px; font-weight: 700; text-transform: uppercase; transition: all 0.3s; }
        .btn-analyze:hover { background: #1d4ed8; transform: translateY(-2px); box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
        .result-box { background-color: #f8fafc; border: 2px dashed #cbd5e1; border-radius: 12px; padding: 25px; text-align: center; height: 100%; display: flex; flex-direction: column; justify-content: center; align-items: center; }
        .prob-circle { width: 120px; height: 120px; border-radius: 50%; background: white; border: 8px solid #e2e8f0; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; font-weight: 800; color: #ef4444; margin-bottom: 10px; }
        .status-badge { font-size: 1.5rem; margin: 15px 0; display: block; }
        /* Thiết lập khung vòng tròn */
        .circular-progress {
            position: relative;
            width: 140px;
            height: 140px;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
            margin: 0 auto 15px auto;
            /* Mặc định màu xám khi chưa phân tích */
            background: conic-gradient(#e9ecef 0%, #e9ecef 100%); 
            transition: background 0.5s ease;
        }

        /* Tạo lõi trắng ở giữa để biến vòng tròn đặc thành vòng nhẫn */
        .circular-progress::before {
            content: "";
            position: absolute;
            width: 114px;
            height: 114px;
            border-radius: 50%;
            background-color: #fff;
        }

        /* Con số phần trăm hiển thị ở giữa */
        .progress-value {
            position: relative;
            font-size: 32px;
            font-weight: 800;
            color: #475569;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <div class="row justify-content-center">
            <div class="col-lg-10">
                <div class="main-card card">
                    <div class="card-header-custom">
                        <i class="fas fa-search me-2"></i> Phân Tích Đánh Giá Ứng Viên
                    </div>
                    <div class="card-body p-4 p-md-5">
                        <div class="row g-4">
                            <div class="col-md-6 border-end pe-md-4">
                                <h5 class="mb-4 text-primary"><i class="fas fa-id-card me-2"></i>Thông tin hồ sơ</h5>
                                
                                <div class="mb-3">
                                    <label class="form-label">Trình độ học vấn (Degree)</label>
                                    <asp:DropDownList ID="ddlDegree" runat="server" CssClass="form-select"></asp:DropDownList>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label">Lĩnh vực chuyên môn (Stream)</label>
                                    <asp:DropDownList ID="ddlStream" runat="server" CssClass="form-select"></asp:DropDownList>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label">Điểm GPA (Hệ 4.0)</label>
                                    <div class="input-group">
                                        <span class="input-group-text"><i class="fas fa-graduation-cap"></i></span>
                                        <asp:TextBox ID="txtGPA" runat="server" CssClass="form-control" placeholder="Ví dụ: 3.5"></asp:TextBox>
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="col-6 mb-3">
                                        <label class="form-label">Kinh nghiệm (năm)</label>
                                        <asp:TextBox ID="txtExp" runat="server" CssClass="form-control" placeholder="Ví dụ: 2"></asp:TextBox>
                                    </div>
                                    <div class="col-6 mb-3">
                                        <label class="form-label">Số lượng dự án</label>
                                        <asp:TextBox ID="txtProjects" runat="server" CssClass="form-control" placeholder="Ví dụ: 5"></asp:TextBox>
                                    </div>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="form-label">Số lượng kỹ năng (Skill Count)</label>
                                    <div class="input-group">
                                        <span class="input-group-text"><i class="fas fa-tools"></i></span>
                                        <asp:TextBox ID="txtSkillCount" runat="server" CssClass="form-control" placeholder="Ví dụ: 8"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="mb-4">
                                    <label class="form-label text-secondary">Chi tiết kỹ năng</label>
                                    <div class="input-group">
                                        <span class="input-group-text bg-light"><i class="fas fa-code text-secondary"></i></span>
                                        <asp:TextBox ID="txtSkillsList" runat="server" CssClass="form-control" placeholder="VD: C#, SQL, React..."></asp:TextBox>
                                    </div>
                                </div>

                                <div class="d-flex gap-2 mt-3">
                                    <asp:Button ID="btnAnalyze" runat="server" Text="Phân tích" 
                                        CssClass="btn btn-primary btn-analyze w-50" OnClick="btnAnalyze_Click" />
                                    <button type="button" class="btn btn-success btn-analyze w-50" data-bs-toggle="modal" data-bs-target="#saveCandidateModal">
                                        <i class="fas fa-save me-1"></i> Lưu Ứng Viên
                                    </button>
                                </div>
                            </div>

                            <div class="col-md-6 ps-md-4">
                                <div class="result-box">
                                    <h5 class="text-secondary mb-4"><i class="fas fa-chart-pie me-2"></i>Đánh giá từ hệ thống</h5>
                                    
                                    <div id="divCircleProgress" runat="server" class="circular-progress">
                                        <span id="lblPercentValue" runat="server" class="progress-value">--%</span>
                                    </div>
                                    <asp:Label ID="lblProbability" runat="server" Text="Tỉ lệ phù hợp: --%" CssClass="fw-bold text-danger"></asp:Label>

                                    <div class="mt-3">
                                        <asp:Label ID="lblStatus" runat="server" Text="Trạng thái: Chờ phân tích" CssClass="status-badge fw-bold"></asp:Label>
                                    </div>
                                    
                                    <div class="mt-4 p-3 border rounded text-start w-100" style="background-color: #ffffff; box-shadow: 0 1px 3px rgba(0,0,0,0.05);">
                                        <h6 class="text-primary mb-3 text-center"><i class="fas fa-balance-scale me-2"></i>So sánh Thuật toán</h6>
                                        <div class="d-flex justify-content-between align-items-center mb-2">
                                            <span>Decision Tree:</span>
                                            <asp:Label ID="lblDT_Compare" runat="server" Text="--%" CssClass="fw-bold"></asp:Label>
                                        </div>
                                        <div class="d-flex justify-content-between align-items-center mb-3 border-bottom pb-3">
                                            <span>Logistic Regression:</span>
                                            <asp:Label ID="lblLR_Compare" runat="server" Text="--%" CssClass="fw-bold text-primary"></asp:Label>
                                        </div>
                                        <asp:Literal ID="litFeatureImpact" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="saveCandidateModal" tabindex="-1" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header" style="background: #10b981; color: white;">
            <h5 class="modal-title"><i class="fas fa-user-plus me-2"></i>Bổ sung thông tin Ứng viên</h5>
            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body text-start">
             <div class="mb-3">
                 <label class="form-label">Họ và Tên</label>
                 <asp:TextBox ID="txtName" runat="server" CssClass="form-control"></asp:TextBox>
             </div>
             <div class="row">
                 <div class="col-6 mb-3">
                     <label class="form-label">Giới tính</label>
                     <asp:DropDownList ID="ddlGender" runat="server" CssClass="form-select">
                         <asp:ListItem Value="Male">Nam</asp:ListItem>
                         <asp:ListItem Value="Female">Nữ</asp:ListItem>
                     </asp:DropDownList>
                 </div>
                 <div class="col-6 mb-3">
                     <label class="form-label">Tuổi</label>
                     <asp:TextBox ID="txtAge" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                 </div>
             </div>
             <div class="mb-3">
                 <label class="form-label">Trường đại học</label>
                 <asp:TextBox ID="txtCollege" runat="server" CssClass="form-control"></asp:TextBox>
             </div>
             <div class="mb-3">
                 <label class="form-label">Email</label>
                 <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
             </div>
             <asp:HiddenField ID="hdfCluster" runat="server" />
             <asp:HiddenField ID="hdfStatus" runat="server" />
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
            <asp:HiddenField ID="hdfReAppId" runat="server" />
            <asp:HiddenField ID="hdfReCandidateId" runat="server" />
            <asp:Button ID="btnCancel" runat="server" Text="HỦY ĐÁNH GIÁ" CssClass="btn btn-secondary me-2" OnClick="btnCancel_Click" Visible="false" />
            <asp:Button ID="btnSaveCandidate" runat="server" Text="Lưu Database" CssClass="btn btn-success fw-bold" OnClick="btnSaveCandidate_Click" />
          </div>
        </div>
      </div>
    </div>
</asp:Content>