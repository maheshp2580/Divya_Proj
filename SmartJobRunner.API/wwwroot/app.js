const API_URL = '/api/jobs';

// DOM Elements
const jobsContainer = document.getElementById('jobsContainer');
const createModal = document.getElementById('createModal');
const executionsModal = document.getElementById('executionsModal');
const executionsContainer = document.getElementById('executionsContainer');
const createJobForm = document.getElementById('createJobForm');

function toggleJobTypeFields() {
    const type = document.getElementById('jobType').value;
    if (type === '0') {
        document.getElementById('httpFields').classList.remove('hidden-field');
        document.getElementById('dbFields').classList.add('hidden-field');
    } else {
        document.getElementById('httpFields').classList.add('hidden-field');
        document.getElementById('dbFields').classList.remove('hidden-field');
    }
}

// State
let pollingInterval = null;
let currentViewingJobId = null;

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    fetchJobs();
});

// APIs
async function fetchJobs() {
    try {
        const response = await fetch(API_URL);
        const jobs = await response.json();
        renderJobs(jobs);
    } catch (error) {
        console.error('Error fetching jobs:', error);
        jobsContainer.innerHTML = '<p class="exec-error">Failed to load jobs. Ensure API is running.</p>';
    }
}

async function handleCreateJob(e) {
    e.preventDefault();
    
    const cronVal = document.getElementById('jobCron').value.trim();
    
    const payload = {
        name: document.getElementById('jobName').value,
        description: document.getElementById('jobDesc').value,
        retryCount: parseInt(document.getElementById('jobRetries').value),
        baseDelaySeconds: parseInt(document.getElementById('jobDelay').value),
        simulateFailureForDemo: document.getElementById('jobSimFail').checked,
        jobType: parseInt(document.getElementById('jobType').value),
        scheduleType: cronVal ? 1 : 0, 
        cronExpression: cronVal || null,
        httpMethod: document.getElementById('httpMethod').value,
        url: document.getElementById('jobUrl').value,
        payload: document.getElementById('jobPayload').value,
        storedProcedureName: document.getElementById('jobSpName').value
    };

    try {
        const res = await fetch(API_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            const result = await res.json();
            
            // If cron expression is provided, schedule it now
            if (cronVal) {
                await fetch(`${API_URL}/${result.jobId}/schedule`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(cronVal)
                });
            }
            
            createJobForm.reset();
            toggleJobTypeFields();
            closeCreateModal();
            fetchJobs();
        } else {
            alert('Failed to create job');
        }
    } catch (err) {
        console.error(err);
        alert('Exception creating job');
    }
}

async function executeJob(jobId) {
    try {
        const res = await fetch(`${API_URL}/${jobId}/execute`, { method: 'POST' });
        if (res.ok) {
            openExecutionsModal(jobId);
        }
    } catch (err) {
        console.error('Execution fault', err);
    }
}

async function deleteJob(jobId) {
    if (!confirm('Are you sure you want to delete this job and its execution history?')) return;
    try {
        const res = await fetch(`${API_URL}/${jobId}`, { method: 'DELETE' });
        if (res.ok) {
            fetchJobs();
        } else {
            alert('Failed to delete job.');
        }
    } catch (err) {
        console.error('Delete fault', err);
    }
}

async function toggleJobStatus(jobId) {
    try {
        const res = await fetch(`${API_URL}/${jobId}/toggle-status`, { method: 'PUT' });
        if (res.ok) {
            fetchJobs();
        } else {
            alert('Failed to toggle status.');
        }
    } catch (err) {
        console.error('Toggle fault', err);
    }
}

async function loadExecutions(jobId) {
    try {
        const res = await fetch(`${API_URL}/${jobId}/executions`);
        const executions = await res.json();
        renderExecutions(executions);
        
        // Polling logic: if any row is pending/running, keep polling periodically
        const isRunning = executions.some(e => e.status === 'Pending' || e.status === 'Running');
        if (isRunning && currentViewingJobId === jobId) {
            clearTimeout(pollingInterval);
            pollingInterval = setTimeout(() => loadExecutions(jobId), 1500);
        } else {
            clearTimeout(pollingInterval);
            pollingInterval = null;
        }
    } catch (err) {
        executionsContainer.innerHTML = '<p>Error loading executions.</p>';
    }
}

// Rendering
function renderJobs(jobs) {
    if (jobs.length === 0) {
        jobsContainer.innerHTML = `
            <div style="grid-column: 1/-1; text-align:center; padding: 3rem; color: var(--text-muted);">
                <i class="fa-solid fa-box-open" style="font-size: 3rem; margin-bottom: 1rem; opacity: 0.5;"></i>
                <p>No jobs configured yet. Create one to get started!</p>
            </div>`;
        return;
    }

    jobsContainer.innerHTML = jobs.map(job => {
        let typeBadge = '';
        if (job.jobType === 'Http') typeBadge = '<i class="fa-solid fa-globe"></i> HTTP';
        else if (job.jobType === 'Database') typeBadge = '<i class="fa-solid fa-database"></i> Database';
        
        let scheduleBadge = job.cronExpression 
            ? `<span style="font-size: 0.75rem; color: #a78bfa;"><i class="fa-regular fa-clock"></i> ${job.cronExpression}</span>`
            : '';

        const statusText = job.isActive ? 'ACTIVE' : 'INACTIVE';
        const toggleIcon = job.isActive ? 'fa-toggle-on' : 'fa-toggle-off';
        const executeDisabled = !job.isActive ? 'disabled style="opacity: 0.5; cursor: not-allowed;"' : '';

        return `
        <div class="job-card" ${!job.isActive ? 'style="opacity: 0.8;"' : ''}>
            <div class="job-card-header">
                <div>
                    <div class="job-title">${job.name}</div>
                    <div style="font-size: 0.8rem; color: var(--text-muted); margin-top: 0.25rem;">
                        ${typeBadge} &nbsp;&nbsp; ${scheduleBadge}
                    </div>
                </div>
                <div class="job-status status-${statusText.toLowerCase()}">${statusText}</div>
            </div>
            <div class="job-desc">${job.description}</div>
            
            <div class="job-actions">
                <button class="btn-action execute-btn" ${executeDisabled} onclick="${job.isActive ? `executeJob('${job.id}')` : 'return false'}" title="Execute Now">
                    <i class="fa-solid fa-play"></i>
                </button>
                <button class="btn-action" onclick="openExecutionsModal('${job.id}')" title="View Logs">
                    <i class="fa-solid fa-history"></i>
                </button>
                <button class="btn-action" onclick="toggleJobStatus('${job.id}')" title="Toggle Active Status">
                    <i class="fa-solid ${toggleIcon}"></i>
                </button>
                <button class="btn-action" onclick="deleteJob('${job.id}')" title="Delete Job" style="color: #ef4444; border-color: rgba(239, 68, 68, 0.3);">
                    <i class="fa-solid fa-trash"></i>
                </button>
            </div>
        </div>
        `;
    }).join('');
}

function renderExecutions(executions) {
    if (executions.length === 0) {
        executionsContainer.innerHTML = '<p style="color:var(--text-muted); text-align:center; padding: 2rem;">No executions recorded yet.</p>';
        return;
    }

    executionsContainer.innerHTML = executions.map(ex => {
        
        // Formatting dates
        const started = new Date(ex.startedAt).toLocaleString();
        const completed = ex.completedAt ? new Date(ex.completedAt).toLocaleString() : '-';

        // Spinner if running
        const isRunning = ex.status === 'Running' || ex.status === 'Pending';
        const spinnerStr = isRunning ? '&nbsp; <i class="fa-solid fa-circle-notch fa-spin"></i>' : '';

        // Error rendering
        const errorHtml = ex.errorMessage ? `<div class="exec-error">${ex.errorMessage}</div>` : '';
        
        // AI rendering
        const aiHtml = ex.aiAnalysis ? `
            <div class="exec-ai-badge">
                <i class="fa-solid fa-robot"></i> AI Root Cause Analysis
            </div>
            <div class="exec-ai-analysis">${ex.aiAnalysis}</div>
        ` : '';

        return `
        <div class="execution-item">
            <div class="exec-header">
                <div>ID: <span style="font-family:monospace; opacity:0.8;">${ex.id.substring(0,8)}</span></div>
                <div class="exec-status ${ex.status}">${ex.status}${spinnerStr}</div>
            </div>
            <div class="exec-date">Started: ${started}</div>
            <div class="exec-date">Completed: ${completed}</div>
            ${errorHtml}
            ${aiHtml}
        </div>
        `;
    }).join('');
}

// Modals
function openCreateModal() {
    createModal.classList.remove('hidden');
    document.getElementById('jobName').focus();
}

function closeCreateModal() {
    createModal.classList.add('hidden');
}

function openExecutionsModal(jobId) {
    currentViewingJobId = jobId;
    executionsContainer.innerHTML = '<div class="loading-spinner"><i class="fa-solid fa-circle-notch fa-spin"></i></div>';
    executionsModal.classList.remove('hidden');
    loadExecutions(jobId);
}

function closeExecutionsModal() {
    executionsModal.classList.add('hidden');
    currentViewingJobId = null;
    clearTimeout(pollingInterval);
    pollingInterval = null;
}
