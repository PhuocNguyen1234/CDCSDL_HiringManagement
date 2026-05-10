<%@ Page Title="Kết Quả Tuyển Dụng" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FinalResultList.aspx.cs" Inherits="Web_Recruitment_Management.FinalResultList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    </asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        <h3 class="text-uppercase fw-bold text-dark mb-4"><i class="fas fa-trophy me-2"></i>Danh Sách Kết Quả Tuyển Dụng</h3>
        
        <div class="card shadow-sm border-0">
            <div class="card-body">
                <asp:GridView ID="gvFinal" runat="server" AutoGenerateColumns="False" 
                    CssClass="table table-hover table-bordered align-middle text-center" 
                    EmptyDataText="Chưa có ứng viên nào được chốt kết quả.">
                    
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
</asp:Content>