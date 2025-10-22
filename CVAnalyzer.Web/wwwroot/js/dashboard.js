// Load dashboard statistics
async function loadStatistics() {
    try {
        const response = await fetch('/Dashboard/GetStatistics');
        const data = await response.json();

        document.getElementById('totalStudents').textContent = data.totalStudents;
        document.getElementById('cvsProcessed').textContent = data.cvsProcessed;
        document.getElementById('skillsIdentified').textContent = data.skillsIdentified;
        document.getElementById('clustersCreated').textContent = data.clustersCreated;
    } catch (error) {
        console.error('Error loading statistics:', error);
    }
}

// Load recent activity
async function loadRecentActivity() {
    try {
        const response = await fetch('/Dashboard/GetRecentActivity');
        const activities = await response.json();

        const tbody = document.querySelector('#recentActivityTable tbody');
        tbody.innerHTML = '';

        if (activities.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">No recent activity</td></tr>';
            return;
        }

        activities.forEach(activity => {
            const statusClass = activity.status === 'Completed' ? 'success' :
                activity.status === 'Processing' ? 'info' :
                    activity.status === 'Failed' ? 'danger' : 'warning';

            const row = `
                <tr>
                    <td><i class="fas fa-file-upload text-primary"></i> ${activity.action}</td>
                    <td>${activity.student}</td>
                    <td><small class="text-muted">${activity.date}</small></td>
                    <td><span class="badge bg-${statusClass}">${activity.status}</span></td>
                </tr>
            `;
            tbody.innerHTML += row;
        });
    } catch (error) {
        console.error('Error loading recent activity:', error);
    }
}

// Initialize skills chart
function initializeSkillsChart() {
    const ctx = document.getElementById('skillsChart');
    if (!ctx) return;

    // Sample data - replace with actual API call
    const skillsData = {
        labels: ['C#', 'Python', 'JavaScript', 'Java', 'SQL', 'React', 'Angular', 'Node.js'],
        datasets: [{
            label: 'Number of Students',
            data: [45, 38, 42, 35, 50, 28, 25, 30],
            backgroundColor: [
                'rgba(54, 162, 235, 0.8)',
                'rgba(255, 206, 86, 0.8)',
                'rgba(75, 192, 192, 0.8)',
                'rgba(153, 102, 255, 0.8)',
                'rgba(255, 159, 64, 0.8)',
                'rgba(255, 99, 132, 0.8)',
                'rgba(201, 203, 207, 0.8)',
                'rgba(83, 211, 87, 0.8)'
            ],
            borderColor: [
                'rgba(54, 162, 235, 1)',
                'rgba(255, 206, 86, 1)',
                'rgba(75, 192, 192, 1)',
                'rgba(153, 102, 255, 1)',
                'rgba(255, 159, 64, 1)',
                'rgba(255, 99, 132, 1)',
                'rgba(201, 203, 207, 1)',
                'rgba(83, 211, 87, 1)'
            ],
            borderWidth: 1
        }]
    };

    new Chart(ctx, {
        type: 'bar',
        data: skillsData,
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    display: false
                },
                title: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            }
        }
    });
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    loadStatistics();
    loadRecentActivity();
    initializeSkillsChart();

    // Refresh statistics every 30 seconds
    setInterval(loadStatistics, 30000);

    // Refresh activity every minute
    setInterval(loadRecentActivity, 60000);
});

