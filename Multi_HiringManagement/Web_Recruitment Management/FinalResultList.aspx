<%@ Page Title="Kết Quả Tuyển Dụng" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FinalResultList.aspx.cs" Inherits="Web_Recruitment_Management.FinalResultList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Hiệu ứng khi di chuột qua từng dòng ứng viên */
        .table-hover-row {
            cursor: pointer;
        }
        .table-hover-row:hover {
            background-color: #f1f8ff !important;
            transition: background-color 0.2s ease;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        <h3 class="text-uppercase fw-bold text-dark mb-4"><i class="fas fa-trophy me-2"></i>Danh Sách Kết Quả Tuyển Dụng</h3>
        
        <div class="card shadow-sm border-0">
            <div class="card-body">
                <asp:GridView ID="gvFinal" runat="server" AutoGenerateColumns="False" 
                    CssClass="table table-hover table-bordered align-middle text-center" 
                    EmptyDataText="Chưa có ứng viên nào được chốt kết quả."
                    DataKeyNames="ApplicationID"
                    OnRowDataBound="gvFinal_RowDataBound">
                    
                    <HeaderStyle CssClass="table-dark" />
                    
                    <Columns>
                        <asp:BoundField DataField="ApplicationID" HeaderText="Mã HS" />
                        <asp:BoundField DataField="FullName" HeaderText="Tên Ứng Viên" />
                        <asp:BoundField DataField="Stream" HeaderText="Lĩnh vực" />
                        <asp:BoundField DataField="AI_Cluster_Group" HeaderText="Đánh giá AI (Dự kiến)" />
                        
                        <asp:TemplateField HeaderText="QUYẾT ĐỊNH CUỐI CÙNG">
                            <ItemTemplate>
                                <span class='badge fs-6 <%# Eval("Final_Result").ToString() == "Pass" ? "bg-success" : (Eval("Final_Result").ToString() == "Fail" ? "bg-danger" : "bg-warning text-dark") %>'>
                                    <%# Eval("Final_Result") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <div class="modal fade" id="detailModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-dark text-white">
                    <h5 class="modal-title"><i class="fas fa-id-card me-2"></i>Chi tiết ứng viên</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body p-4" id="modalContent">
                    <div class="text-center">
                        <div class="spinner-border text-primary" role="status"></div>
                        <p class="mt-2 text-muted">Đang tải dữ liệu...</p>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script type="text/javascript">
        function showDetail(appId) {
            // Set trạng thái Loading trước khi gọi dữ liệu
            $('#modalContent').html('<div class="text-center"><div class="spinner-border text-primary" role="status"></div><p class="mt-2 text-muted">Đang tải dữ liệu...</p></div>');
            
            // Hiển thị Modal
            var myModal = new bootstrap.Modal(document.getElementById('detailModal'));
            myModal.show();

            // Gọi AJAX lấy dữ liệu từ Backend
            $.ajax({
                type: "POST",
                url: "FinalResultList.aspx/GetCandidateDetail", // Chỉ đúng vào trang hiện tại
                data: JSON.stringify({ applicationId: appId }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    $('#modalContent').html(response.d); // Đổ bảng HTML vào thân Modal
                },
                error: function () {
                    $('#modalContent').html('<div class="alert alert-danger"><i class="fas fa-exclamation-triangle me-2"></i>Không thể lấy dữ liệu từ máy chủ!</div>');
                }
            });
        }
    </script>
</asp:Content>