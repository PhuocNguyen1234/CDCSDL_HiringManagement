<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Web_Recruitment_Management.Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
    
    <style>
        .chart-container {
            background: #fff;
            padding: 25px;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            margin-top: 15px;
            margin-bottom: 25px;
        }
        /* Style cho thanh điều khiển */
        .dashboard-controls {
            background: #f8f9fa;
            padding: 15px 20px;
            border-radius: 8px;
            border-left: 5px solid #007bff;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
            margin-top: 20px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        
        <div class="row">
            <div class="col-md-12">
                <div class="dashboard-controls d-flex align-items-center justify-content-between">
                    <h5 class="m-0 font-weight-bold text-primary">TỔNG QUAN PHÂN TÍCH ỨNG VIÊN</h5>
                    <div class="d-flex align-items-center">
                        <label for="viewSelector" class="mr-2 mb-0 font-weight-bold text-secondary">Chế độ xem:</label>
                        <select id="viewSelector" class="form-control" style="width: 300px; cursor: pointer;" onchange="toggleDashboardView()">
                            <option value="chartView">📊 Biểu đồ: Số lượng ứng viên Đậu (Pass)</option>
                            <option value="tableView">📋 Bảng: Phân tích Rào cản </option>
                        </select>
                    </div>
                </div>
            </div>
        </div>

        <div class="row" id="chartContainer" style="display: block;">
            <div class="col-md-12">
                <div class="chart-container">
                    <h4 class="text-center" style="font-family: Arial; color: #333;">
                        Dự báo Ứng viên "Pass" theo Trường Đại học
                    </h4>
                    <hr />
                    <canvas id="collegeChart" style="max-height: 500px;"></canvas>
                </div>
            </div>
        </div>

        <div class="row" id="tableContainer" style="display: none;">
            <div class="col-md-12">
                <div class="chart-container">
                    <h4 class="text-center" style="font-family: Arial; color: #dc3545;">
                        Phân Tích Rào Cản Ứng Viên Theo Trường 
                    </h4>
                    <p class="text-center text-muted">Hệ thống tự động trích xuất nguyên nhân trượt phổ biến nhất dựa trên thuật toán Cây quyết định.</p>
                    <hr />
                    <div class="table-responsive">
                        <table class="table table-bordered table-hover mt-3">
                            <thead class="thead-light" style="background-color: #f8f9fa;">
                                <tr>
                                    <th style="width: 40%">Trường Đại Học</th>
                                    <th style="width: 40%">Rào Cản Chính (Điểm yếu)</th>
                                    <th style="width: 20%; text-align: center;">Số Ứng Viên Bị Loại</th>
                                </tr>
                            </thead>
                            <tbody id="weaknessTableBody">
                                </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>

    </div>

    <script type="text/javascript">
        function toggleDashboardView() {
            const selectedView = document.getElementById('viewSelector').value;
            const chartDiv = document.getElementById('chartContainer');
            const tableDiv = document.getElementById('tableContainer');

            if (selectedView === 'chartView') {
                chartDiv.style.display = 'block';
                tableDiv.style.display = 'none';
            } else if (selectedView === 'tableView') {
                chartDiv.style.display = 'none';
                tableDiv.style.display = 'block';
            }
        }

        document.addEventListener("DOMContentLoaded", function () {
            fetch('Dashboard.aspx/GetChartData', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({})
            })
                .then(response => response.json())
                .then(data => {
                    const rawData = data.d;
                    if (!rawData || rawData.length === 0) return;

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
                            scales: { x: { beginAtZero: true, ticks: { precision: 0 } } },
                            plugins: { legend: { display: false } }
                        }
                    });
                })
                .catch(err => console.error("Lỗi khi fetch Chart data:", err));

            fetch('Dashboard.aspx/GetWeaknessData', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({})
            })
                .then(response => response.json())
                .then(data => {
                    const rawData = data.d;
                    const tbody = document.getElementById('weaknessTableBody');

                    if (!rawData || rawData.length === 0) {
                        tbody.innerHTML = "<tr><td colspan='3' class='text-center'>Không có dữ liệu phân tích.</td></tr>";
                        return;
                    }

                    let htmlContent = '';
                    rawData.forEach(item => {
                        // Thiết lập màu sắc trực tiếp thay vì dùng class badge để tránh lỗi CSS của Template
                        let ruleColor = '#d9534f'; // Màu đỏ mặc định
                        if (item.WeaknessRule.includes('Project')) ruleColor = '#17a2b8'; // Màu xanh lá cho Project
                        else if (item.WeaknessRule.includes('GPA')) ruleColor = '#fd7e14'; // Màu cam cho GPA

                        htmlContent += `
                        <tr>
                            <td class="font-weight-bold align-middle">${item.CollegeName}</td>
                            <td class="align-middle">
                                <span style="color: ${ruleColor}; font-weight: 600; font-size: 0.95em;">
                                    <i class="fas fa-exclamation-triangle mr-1"></i> ${item.WeaknessRule}
                                </span>
                            </td>
                            <td class="text-center font-weight-bold align-middle" style="font-size: 1.1em;">
                                <span class="text-danger">${item.CandidateCount}</span> 
                                <span class="text-muted" style="font-size: 0.85em; font-weight: normal;"> / ${item.TotalCount || 0}</span>
                            </td>
                        </tr>
                    `;
                    });

                    tbody.innerHTML = htmlContent;
                })
                .catch(err => console.error("Lỗi khi fetch Weakness data:", err));
        });
    </script>
</asp:Content>