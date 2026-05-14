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
                        <select id="viewSelector" class="form-control" style="width: 320px; cursor: pointer;" onchange="toggleDashboardView()">
                            <option value="chartView">📊 Biểu đồ: Số lượng ứng viên Đậu (Pass)</option>
                            <option value="tableView">📋 Bảng: Phân tích Rào cản</option>
                            <option value="dnaView">🧬 Thẻ: DNA Kỹ năng theo Trường</option>
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

        <div class="row" id="dnaContainer" style="display: none;">
            <div class="col-md-12">
                <div class="chart-container" style="background-color: #fcfcfc;">
                    <h4 class="text-center" style="font-family: Arial; color: #17a2b8;">
                        <i class="fas fa-dna me-2"></i> DNA Kỹ năng: Top 3 Thế mạnh cốt lõi
                    </h4>
                    <p class="text-center text-muted">Thống kê nhóm kỹ năng của các ứng viên đã Trúng tuyển theo từng trường Đại học.</p>
                    <hr />
                    <div class="row mt-3 mb-2 px-3" id="dnaFilterBar" style="display: none;">
                        <div class="col-md-5">
                            <label class="font-weight-bold text-secondary mb-1"><i class="fas fa-filter"></i> Lọc theo Trường:</label>
                            <select id="filterCollege" class="form-control border-info" onchange="applyDnaFilter()">
                                <option value="All">-- Tất cả Trường Đại học --</option>
                            </select>
                        </div>
                        <div class="col-md-5">
                            <label class="font-weight-bold text-secondary mb-1"><i class="fas fa-search"></i> Lọc theo Kỹ năng cốt lõi:</label>
                            <select id="filterSkill" class="form-control border-info" onchange="applyDnaFilter()">
                                <option value="All">-- Tất cả Kỹ năng --</option>
                            </select>
                        </div>
                        <div class="col-md-2 d-flex align-items-end">
                            <button class="btn btn-outline-secondary w-100" onclick="resetDnaFilter()">
                                <i class="fas fa-sync-alt"></i> Làm mới
                            </button>
                        </div>
                    </div>
                    <div class="row mt-4" id="dnaCardsContainer">
                        <div class="text-center w-100 p-4">
                            <div class="spinner-border text-info" role="status"></div>
                            <p class="mt-2 text-muted">Đang phân tích DNA Kỹ năng từ AI...</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>

    <script type="text/javascript">
        // HÀM ẨN HIỆN CÁC KHUNG GIAO DIỆN
        function toggleDashboardView() {
            const selectedView = document.getElementById('viewSelector').value;
            const chartDiv = document.getElementById('chartContainer');
            const tableDiv = document.getElementById('tableContainer');
            const dnaDiv = document.getElementById('dnaContainer'); // Lấy div mới

            // Ẩn tất cả trước
            chartDiv.style.display = 'none';
            tableDiv.style.display = 'none';
            dnaDiv.style.display = 'none';

            // Hiển thị khung tương ứng với dropdown
            if (selectedView === 'chartView') {
                chartDiv.style.display = 'block';
            } else if (selectedView === 'tableView') {
                tableDiv.style.display = 'block';
            } else if (selectedView === 'dnaView') {
                dnaDiv.style.display = 'block';
            }
        }

        document.addEventListener("DOMContentLoaded", function () {
            // 1. FETCH DỮ LIỆU BIỂU ĐỒ (Giữ nguyên)
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

            // 2. FETCH DỮ LIỆU BẢNG ĐIỂM YẾU (Giữ nguyên)
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
                        let ruleColor = '#d9534f';
                        if (item.WeaknessRule.includes('Project')) ruleColor = '#17a2b8';
                        else if (item.WeaknessRule.includes('GPA')) ruleColor = '#fd7e14';

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

            // 3. FETCH DỮ LIỆU DNA KỸ NĂNG (MỚI THÊM)
            let globalDnaData = []; // Biến toàn cục lưu dữ liệu để lọc

            fetch('Dashboard.aspx/GetDNAChartData', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({})
            })
                .then(response => response.json())
                .then(data => {
                    globalDnaData = data.d;
                    const container = document.getElementById('dnaCardsContainer');

                    if (!globalDnaData || globalDnaData.length === 0) {
                        container.innerHTML = "<div class='col-12 text-center text-muted'>Không có dữ liệu DNA phân tích.</div>";
                        return;
                    }

                    // Hiện thanh bộ lọc và khởi tạo dữ liệu
                    document.getElementById('dnaFilterBar').style.display = 'flex';
                    populateDnaFilters(globalDnaData);
                    renderDnaCards(globalDnaData, 'All', 'All');
                })
                .catch(err => {
                    console.error("Lỗi tải DNA:", err);
                    document.getElementById('dnaCardsContainer').innerHTML = "<div class='text-danger text-center w-100'>Lỗi kết nối máy chủ!</div>";
                });

            // Hàm trích xuất dữ liệu tạo Option cho Dropdown
            function populateDnaFilters(data) {
                const colleges = new Set();
                const skills = new Set();
                data.forEach(item => {
                    colleges.add(item.CollegeName);
                    skills.add(item.SkillName);
                });

                const filterCollege = document.getElementById('filterCollege');
                const filterSkill = document.getElementById('filterSkill');

                [...colleges].sort().forEach(c => filterCollege.innerHTML += `<option value="${c}">${c}</option>`);
                [...skills].sort().forEach(s => filterSkill.innerHTML += `<option value="${s}">${s}</option>`);
            }

            // Gắn hàm lọc vào đối tượng window để gọi từ HTML HTML onclick
            window.applyDnaFilter = function () {
                const selectedCollege = document.getElementById('filterCollege').value;
                const selectedSkill = document.getElementById('filterSkill').value;
                renderDnaCards(globalDnaData, selectedCollege, selectedSkill);
            };

            window.resetDnaFilter = function () {
                document.getElementById('filterCollege').value = 'All';
                document.getElementById('filterSkill').value = 'All';
                renderDnaCards(globalDnaData, 'All', 'All');
            };

            // Hàm vẽ Card (Đã thêm logic Lọc)
            function renderDnaCards(data, collegeFilter, skillFilter) {
                // Nhóm dữ liệu
                const groupedData = {};
                data.forEach(item => {
                    if (!groupedData[item.CollegeName]) groupedData[item.CollegeName] = [];
                    groupedData[item.CollegeName].push(item);
                });

                let htmlContent = '';
                const medals = [
                    '<i class="fas fa-medal text-warning"></i>',
                    '<i class="fas fa-medal text-secondary"></i>',
                    '<i class="fas fa-medal" style="color: #cd7f32;"></i>'
                ];

                for (const [collegeName, skills] of Object.entries(groupedData)) {
                    // LỌC THEO TRƯỜNG: Bỏ qua nếu không khớp
                    if (collegeFilter !== 'All' && collegeName !== collegeFilter) continue;

                    // LỌC THEO KỸ NĂNG: Bỏ qua thẻ Card này nếu trường không có kỹ năng đang tìm
                    if (skillFilter !== 'All') {
                        const hasSkill = skills.some(s => s.SkillName === skillFilter);
                        if (!hasSkill) continue;
                    }

                    let skillsHtml = '';
                    skills.forEach((skill, index) => {
                        let medalIcon = index < 3 ? medals[index] : '';
                        let countValue = skill.CandidateCount !== undefined ? skill.CandidateCount : '0';

                        // Hiệu ứng Highlight kỹ năng nếu đang lọc theo kỹ năng đó
                        let highlightClass = (skillFilter !== 'All' && skill.SkillName === skillFilter) ? 'bg-warning font-weight-bold' : 'bg-transparent';

                        skillsHtml += `
                            <li class="list-group-item d-flex justify-content-between align-items-center px-2 py-2 border-0 border-bottom ${highlightClass}">
                                <span class="text-dark">${medalIcon} ${skill.SkillName}</span>
                                <span class="badge badge-pill shadow-sm" style="background-color: #17a2b8; color: #ffffff; font-size: 0.85rem; padding: 0.4em 0.8em;">
                                    ${countValue} CV
                                </span>
                            </li>`;
                    });

                    htmlContent += `
                        <div class="col-md-4 mb-4">
                            <div class="card h-100 border-info shadow-sm" style="border-radius: 8px;">
                                <div class="card-header bg-info text-white py-2 font-weight-bold" style="font-size: 0.95rem;">
                                    <i class="fas fa-university me-1"></i> ${collegeName}
                                </div>
                                <div class="card-body p-2 bg-white">
                                    <ul class="list-group list-group-flush" style="font-size: 0.9rem;">
                                        ${skillsHtml}
                                    </ul>
                                </div>
                            </div>
                        </div>`;
                }

                const container = document.getElementById('dnaCardsContainer');
                if (htmlContent === '') {
                    container.innerHTML = "<div class='col-12 text-center text-danger font-weight-bold p-5'><h5>Không tìm thấy Trường nào phù hợp với bộ lọc!</h5></div>";
                } else {
                    container.innerHTML = htmlContent;
                }
            }
        });
    </script>
</asp:Content>