<%@ Page Title="Danh sách Ứng viên theo Cụm" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="CandidateList.aspx.cs" Inherits="Web_Recruitment_Management.CandidateList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .table-scroll-wrapper {
            max-height: 550px; 
            overflow-y: auto;  
            border: 1px solid #dee2e6;
            border-radius: 8px;
            box-shadow: inset 0 0 5px rgba(0,0,0,0.05);
            position: relative; 
        }
        
        .table-scroll-wrapper thead th {
            position: sticky;
            top: -1px; 
            background-color: #212529; 
            color: white;
            z-index: 10;
            border-bottom: 2px solid #495057;
            box-shadow: 0 -1px 0 #212529; 
        }

        .pagination-ys {
            position: sticky;
            bottom: -1px; 
            background-color: #f8f9fa; 
            z-index: 11; 
            box-shadow: 0 -4px 10px rgba(0,0,0,0.05); 
        }

        .pagination-ys > td {
            padding: 12px 0 !important;
            border-top: 2px solid #dee2e6;
        }

        .pagination-ys table {
            margin: 0 auto; 
            border-collapse: separate;
            border-spacing: 6px 0; 
        }

        .pagination-ys table > tbody > tr > td {
            display: inline-block;
            padding: 0;
        }

        .pagination-ys table > tbody > tr > td > a,
        .pagination-ys table > tbody > tr > td > span {
            display: block; 
            padding: 8px 16px; 
            line-height: 1.5; 
            text-decoration: none; 
            color: #0d6efd; 
            background-color: #fff;
            border: 1px solid #dee2e6; 
            border-radius: 8px; 
            font-weight: 500;
            transition: all 0.2s ease-in-out;
        }

        .pagination-ys table > tbody > tr > td > a:hover {
            color: #0a58ca; 
            background-color: #e9ecef; 
            transform: translateY(-2px); 
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        }

        .pagination-ys table > tbody > tr > td > span {
            color: #fff; 
            background-color: #0d6efd; 
            border-color: #0d6efd; 
            box-shadow: 0 4px 6px rgba(13, 110, 253, 0.3); 
            cursor: default;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid px-4">
        <div class="bg-white p-4 rounded shadow-sm">
            <h3 class="text-primary fw-bold mb-4"><i class="fas fa-users me-2"></i>Quản lý Ứng viên & Phân loại AI</h3>
            
            <div class="row mb-4 align-items-end">
                <div class="col-md-4 col-sm-6 mb-3 mb-md-0">
                    <label class="form-label fw-bold text-secondary mb-2">Lọc theo Cụm phân loại:</label>
                    <asp:DropDownList ID="ddlFilterCluster" runat="server" CssClass="form-select border-info" AutoPostBack="true" OnSelectedIndexChanged="ddlFilterCluster_SelectedIndexChanged">
                        <asp:ListItem Value="All">-- Tất cả cụm AI --</asp:ListItem>
                    </asp:DropDownList>
                </div>
                
                <div class="col-md-4 col-sm-6 mb-3 mb-md-0">
                    <label class="form-label fw-bold text-secondary mb-2">Lọc theo Kết quả:</label>
                    <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-select border-success" AutoPostBack="true" OnSelectedIndexChanged="ddlFilterStatus_SelectedIndexChanged">
                        <asp:ListItem Value="All">-- Tất cả kết quả --</asp:ListItem>
                        <asp:ListItem Value="Placed">Trúng tuyển</asp:ListItem>
                        <asp:ListItem Value="Not Placed">Không đạt</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <div class="table-scroll-wrapper">
                <asp:GridView ID="gvCandidates" runat="server" CssClass="table table-hover table-bordered align-middle mb-0 " 
                    AutoGenerateColumns="False" AllowPaging="True" PageSize="50" 
                    OnPageIndexChanging="gvCandidates_PageIndexChanging"
                    style="table-layout: fixed; word-wrap: break-word;"
                    OnRowCommand="gvCandidates_RowCommand"> 
                    <HeaderStyle CssClass="table-dark text-center" />
                    <RowStyle CssClass="text-center" />
                    <PagerStyle CssClass="pagination-ys pb-3" />

                    <Columns>
                        <asp:BoundField DataField="app_id" HeaderText="ID" ItemStyle-CssClass="fw-bold text-secondary" HeaderStyle-Width="6%" />
                        
                        <asp:BoundField DataField="name" HeaderText="Họ Tên" ItemStyle-CssClass="fw-bold text-start" HeaderStyle-Width="18%" />
                        
                        <asp:TemplateField HeaderText="Năng lực (Exp/Proj)" HeaderStyle-Width="12%">
                            <ItemTemplate>
                                <span class="d-block" style="font-size: 0.9em;">
                                    <i class="fas fa-briefcase text-secondary me-1"></i>
                                    <%# Eval("exp") != DBNull.Value ? Eval("exp") : "0" %> năm
                                </span>
                                <span class="d-block" style="font-size: 0.9em;">
                                    <i class="fas fa-project-diagram text-secondary me-1"></i>
                                    <%# Eval("projects") != DBNull.Value ? Eval("projects") : "0" %> D.án
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="GPA" HeaderStyle-Width="6%">
                            <ItemTemplate>
                                <span class="text-primary fw-bold"><%# Eval("gpa") != DBNull.Value ? Eval("gpa") : "N/A" %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        
                        <asp:TemplateField HeaderText="Tỉ lệ AI" HeaderStyle-Width="8%">
                            <ItemTemplate>
                                <span class="badge bg-secondary">
                                    <%# Eval("prob") != DBNull.Value && !string.IsNullOrEmpty(Eval("prob").ToString()) ? Eval("prob").ToString() + "%" : "Chưa có" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Phân nhóm" HeaderStyle-Width="20%">
                            <ItemTemplate>
                                <span class='<%# Eval("cluster_name") != DBNull.Value && !string.IsNullOrEmpty(Eval("cluster_name").ToString()) ? "badge border border-info text-info" : "badge border border-warning text-warning" %>'>
                                    <i class="fas fa-layer-group me-1"></i>
                                    <%# Eval("cluster_name") != DBNull.Value && !string.IsNullOrEmpty(Eval("cluster_name").ToString()) ? Eval("cluster_name") : "Chưa phân tích" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Kết quả dự đoán" HeaderStyle-Width="12%">
                            <ItemTemplate>
                                <span class='<%# Eval("placement_status") != DBNull.Value && Eval("placement_status").ToString().Contains("Placed") && !Eval("placement_status").ToString().Contains("Not") ? "badge bg-success" : "badge bg-danger" %>'>
                                    <%# Eval("placement_status") != DBNull.Value && Eval("placement_status").ToString().Contains("Placed") && !Eval("placement_status").ToString().Contains("Not") ? "TRÚNG TUYỂN" : "KHÔNG ĐẠT" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        
                        <asp:BoundField DataField="apply_date" HeaderText="Ngày ứng tuyển" DataFormatString="{0:dd/MM/yyyy}" HeaderStyle-Width="10%" />

                        <asp:TemplateField HeaderText="Hành động" HeaderStyle-Width="10%">
                            <ItemTemplate>
                                <div class="d-flex justify-content-center flex-wrap gap-2">
                                    <button type="button" class="btn btn-sm btn-outline-primary" 
                                            onclick='showDetail(<%# Eval("app_id") %>)' title="Xem chi tiết">
                                        <i class="fas fa-eye"></i>
                                    </button>
                                    <asp:LinkButton ID="btnDelete" runat="server" 
                                        CssClass="btn btn-sm btn-outline-danger" 
                                        ToolTip="Xóa ứng viên"
                                        CommandName="DeleteRow" 
                                        CommandArgument='<%# Eval("app_id") %>'
                                        OnClientClick="return confirm('Bạn có chắc chắn muốn xóa ứng viên này và mọi dữ liệu liên quan không?');">
                                        <i class="fas fa-trash-alt"></i>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnEdit" runat="server" 
                                        CssClass="btn btn-sm btn-outline-warning" 
                                        ToolTip="Chỉnh sửa ứng viên"
                                        CommandName="EditRow" 
                                        CommandArgument='<%# Eval("app_id") %>'>
                                        <i class="fas fa-edit"></i>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnReEvaluate" runat="server" 
                                        CssClass="btn btn-sm btn-outline-info" 
                                        ToolTip="Cho AI đánh giá lại"
                                        CommandName="ReEvaluate" 
                                        CommandArgument='<%# Eval("app_id") %>'>
                                        <i class="fas fa-robot"></i>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnFinalEval" runat="server" 
                                        CssClass="btn btn-sm btn-outline-success" 
                                        ToolTip="Đánh giá cuối cùng"
                                        CommandName="OpenFinalEval" 
                                        CommandArgument='<%# Eval("app_id") %>'>
                                        <i class="fas fa-check-circle"></i>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <div class="modal fade" id="detailModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title"><i class="fas fa-info-circle me-2"></i>Chi tiết ứng viên</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body" id="modalContent">
                    <div class="text-center">
                        <div class="spinner-border text-primary" role="status"></div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="editModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header bg-warning text-dark">
                    <h5 class="modal-title"><i class="fas fa-edit me-2"></i>Cập nhật thông tin</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body text-start">
                    <asp:HiddenField ID="hdfEditAppId" runat="server" />
                    <asp:HiddenField ID="hdfEditCandidateId" runat="server" />

                    <div class="mb-3">
                        <label class="form-label fw-bold">Họ tên</label>
                        <asp:TextBox ID="txtEditName" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Email</label>
                        <asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Trường Đại học</label>
                        <asp:TextBox ID="txtEditCollege" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Kỹ năng chính</label>
                        <asp:TextBox ID="txtEditSkills" runat="server" CssClass="form-control" placeholder="VD: C#, Python, SQL..."></asp:TextBox>
                        <small class="text-muted fst-italic">Các kỹ năng cách nhau bởi dấu phẩy (,)</small>
                    </div>
                    
                    <div class="row">
                        <div class="col-4 mb-3">
                            <label class="form-label fw-bold">Điểm GPA</label>
                            <asp:TextBox ID="txtEditGPA" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="col-4 mb-3">
                            <label class="form-label fw-bold">Kinh nghiệm</label>
                            <asp:TextBox ID="txtEditExp" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                        </div>
                        <div class="col-4 mb-3">
                            <label class="form-label fw-bold">Dự án</label>
                            <asp:TextBox ID="txtEditProjects" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                    <asp:Button ID="btnUpdateCandidate" runat="server" Text="Lưu thay đổi" CssClass="btn btn-warning fw-bold" OnClick="btnUpdateCandidate_Click" />
                </div>
            </div>
        </div>
    </div>

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script>
        function showDetail(appId) {
            var myModal = new bootstrap.Modal(document.getElementById('detailModal'));
            myModal.show();

            $.ajax({
                type: "POST",
                url: "CandidateList.aspx/GetCandidateDetail",
                data: JSON.stringify({ applicationId: appId }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    $('#modalContent').html(response.d);
                },
                error: function () {
                    $('#modalContent').html('<p class="text-danger">Không thể lấy dữ liệu!</p>');
                }
            });
        }
    </script>

    <div class="modal fade" id="finalEvalModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-sm modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header bg-dark text-white">
                    <h5 class="modal-title"><i class="fas fa-gavel me-2"></i>Quyết Định Cuối Cùng</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body text-center">
                    <p class="text-secondary mb-3">Vui lòng chọn kết quả cho hồ sơ này:</p>
                    
                    <asp:HiddenField ID="hdfFinalAppId" runat="server" />
                    
                    <asp:DropDownList ID="ddlFinalResult" runat="server" CssClass="form-select form-select-lg mb-3 border-dark fw-bold">
                        <asp:ListItem Value="Pass" Text="🟢 PASS (Đạt)"></asp:ListItem>
                        <asp:ListItem Value="Re-interview" Text="🟡 RE-INTERVIEW (PV Lại)"></asp:ListItem>
                        <asp:ListItem Value="Fail" Text="🔴 FAIL (Loại)"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="modal-footer justify-content-center bg-light">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                    <asp:Button ID="btnSaveFinal" runat="server" Text="Xác nhận lưu" CssClass="btn btn-dark fw-bold" OnClick="btnSaveFinal_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>