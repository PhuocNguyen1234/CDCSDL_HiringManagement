<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CandidateFiltering.aspx.cs" Inherits="Web_Recruitment_Management.CandidateFiltering" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Hệ Thống Phân Tích Tuyển Dụng AI</title>
    
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- Font Awesome (Icons) -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
    <!-- Google Fonts -->
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;600;700&display=swap" rel="stylesheet" />

    <style>
        body {
            font-family: 'Inter', sans-serif;
            background-color: #f4f7f9;
            color: #334155;
        }
        .navbar {
            background-color: #1e293b;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .main-card {
            background: #ffffff;
            border: none;
            border-radius: 12px;
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }
        .card-header-custom {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            padding: 20px;
            font-weight: 700;
            font-size: 1.25rem;
        }
        .form-label {
            font-weight: 600;
            color: #475569;
            margin-bottom: 0.5rem;
        }
        .form-control, .form-select {
            border-radius: 8px;
            padding: 10px 15px;
            border: 1px solid #e2e8f0;
            transition: all 0.3s ease;
        }
        .form-control:focus {
            border-color: #3b82f6;
            box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
        }
        .btn-analyze {
            background: #2563eb;
            border: none;
            border-radius: 8px;
            padding: 12px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            transition: all 0.3s;
        }
        .btn-analyze:hover {
            background: #1d4ed8;
            transform: translateY(-2px);
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        }
        .result-box {
            background-color: #f8fafc;
            border: 2px dashed #cbd5e1;
            border-radius: 12px;
            padding: 25px;
            height: 100%;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            text-align: center;
        }
        .status-badge {
            font-size: 1.5rem;
            margin: 15px 0;
            display: block;
        }
        .prob-circle {
            width: 120px;
            height: 120px;
            border-radius: 50%;
            background: white;
            border: 8px solid #e2e8f0;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.5rem;
            font-weight: 800;
            color: #ef4444;
            margin-bottom: 10px;
            box-shadow: inset 0 2px 4px rgba(0,0,0,0.05);
        }
        .footer-text {
            color: #94a3b8;
            font-size: 0.85rem;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Navbar giả lập hệ thống quản lý -->
        <nav class="navbar navbar-dark mb-5">
            <div class="container">
                <a class="navbar-brand" href="#">
                    <i class="fas fa-robot me-2"></i> Recruitment AI Analytics
                </a>
            </div>
        </nav>

        <div class="container">
            <div class="row justify-content-center">
                <div class="col-lg-10">
                    <div class="main-card card">
                        <div class="card-header-custom">
                            <i class="fas fa-search me-2"></i> Phân Tích Đánh Giá Ứng Viên
                        </div>
                        <div class="card-body p-4 p-md-5">
                            <div class="row g-4">
                                <!-- Cột nhập liệu -->
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

                                    <asp:Button ID="btnAnalyze" runat="server" Text="Bắt đầu phân tích AI" 
                                        CssClass="btn btn-primary btn-analyze w-100 mt-3" OnClick="btnAnalyze_Click" />
                                </div>

                                <!-- Cột kết quả -->
                                <div class="col-md-6 ps-md-4">
                                    <div class="result-box">
                                        <h5 class="text-secondary mb-4"><i class="fas fa-chart-pie me-2"></i>Đánh giá từ hệ thống</h5>
                                        
                                        <div class="prob-circle">
                                            <asp:Literal ID="litProb" runat="server" Text="--%"></asp:Literal>
                                        </div>
                                        <asp:Label ID="lblProbability" runat="server" Text="Tỉ lệ phù hợp: --%" CssClass="fw-bold text-danger"></asp:Label>

                                        <div class="mt-3">
                                            <asp:Label ID="lblStatus" runat="server" Text="Trạng thái: Chờ phân tích" CssClass="status-badge fw-bold"></asp:Label>
                                        </div>

                                        <p class="footer-text mt-auto">
                                            <i class="fas fa-info-circle me-1"></i> Kết quả dựa trên mô hình Decision Tree của hệ thống quản lý tuyển dụng 2026.
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="text-center mt-4 text-muted small">
                        &copy; 2026 Recruitment Management System | Powered by Gemini AI
                    </div>
                </div>
            </div>
        </div>
    </form>

    <!-- Bootstrap JS -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>