<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Web_Recruitment_Management.Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        .chart-container {
            background: #fff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            margin-top: 20px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-md-12">
                <div class="chart-container">
                    <h3 class="text-center" style="font-family: Arial; color: #333;">
                        Dự báo Ứng viên "Pass" theo Trường Đại học (Decision Tree)
                    </h3>
                    <hr />
                    <canvas id="collegeChart" style="max-height: 500px;"></canvas>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {
            fetch('Dashboard.aspx/GetChartData', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({}) 
            })
            .then(response => response.json())
            .then(data => {
                const rawData = data.d;

                if (!rawData || rawData.length === 0) {
                    console.warn("Không tìm thấy dữ liệu từ Cube.");
                    return;
                }

                const labels = rawData.map(item => item.CollegeName);
                const values = rawData.map(item => item.PassCount);

                const ctx = document.getElementById('collegeChart').getContext('2d');
                new Chart(ctx, {
                    type: 'bar', 
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Số ứng viên dự báo Pass',
                            data: values,
                            backgroundColor: 'rgba(54, 162, 235, 0.6)', 
                            borderColor: 'rgba(54, 162, 235, 1)',
                            borderWidth: 1
                        }]
                    },
                    options: {
                        indexAxis: 'y', 
                        responsive: true,
                        scales: {
                            x: {
                                beginAtZero: true,
                                ticks: { precision: 0 }
                            }
                        },
                        plugins: {
                            legend: { display: false }
                        }
                    }
                });
            })
            .catch(err => console.error("Lỗi khi fetch dữ liệu dashboard:", err));
        });
    </script>
</asp:Content>
