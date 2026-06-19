const apiBase = (window.location.protocol === 'http:' || window.location.protocol === 'https:')
  ? window.location.origin
  : 'http://127.0.0.1:5011';
const statusEl = document.getElementById('status');
const deptGrid = document.getElementById('departmentGrid');
const empGrid = document.getElementById('employeeGrid');
const deptForm = document.getElementById('departmentForm');
const employeeForm = document.getElementById('employeeForm');
const deptSelect = document.getElementById('empDepartmentId');
const departmentFilter = document.getElementById('departmentFilter');
const refreshDepartments = document.getElementById('refreshDepartments');
const refreshEmployees = document.getElementById('refreshEmployees');
const departmentCountEl = document.getElementById('departmentCount');
const employeeCountEl = document.getElementById('employeeCount');

let departments = [];
let employees = [];

function updateStatus(text, type = 'info') {
  statusEl.textContent = text;
  statusEl.style.backgroundColor = type === 'error' ? '#fee2e2' : '#e0f2fe';
  statusEl.style.color = type === 'error' ? '#b91c1c' : '#0369a1';
}

async function request(path, options = {}) {
  try {
    const response = await fetch(`${apiBase}${path}`, options);
    if (!response.ok) {
      const error = await response.text();
      throw new Error(`${response.status} ${response.statusText}: ${error}`);
    }
    return response.status === 204 ? null : response.json();
  } catch (err) {
    updateStatus(err.message, 'error');
    throw err;
  }
}

function renderCard(title, content, footer) {
  const card = document.createElement('div');
  card.className = 'card';
  const body = document.createElement('div');
  body.className = 'card-body';
  body.innerHTML = `<h3>${title}</h3>${content}`;
  const cardFooter = document.createElement('div');
  cardFooter.className = 'card-footer';
  if (footer) cardFooter.appendChild(footer);
  card.appendChild(body);
  card.appendChild(cardFooter);
  return card;
}

function createButton(text, onClick) {
  const btn = document.createElement('button');
  btn.type = 'button';
  btn.textContent = text;
  btn.addEventListener('click', onClick);
  return btn;
}

async function loadDepartments() {
  try {
    departments = await request('/api/Department');
    deptGrid.innerHTML = departments.length ? '' : '<p>No departments found.</p>';
    deptSelect.innerHTML = '<option value="">Select department</option>';
    departmentFilter.innerHTML = '<option value="">All departments</option>';

    const sortedDepartments = [...departments].sort((a, b) => a.departmentName.localeCompare(b.departmentName));
    sortedDepartments.forEach((dept, index) => {
      const footer = document.createElement('div');
      footer.append(createButton('Edit', () => editDepartment(dept)), createButton('Delete', () => deleteDepartment(dept.departmentId)));
      const content = `
        <strong>No.:</strong> ${index + 1}<br />
        <strong>Name:</strong> ${dept.departmentName}<br />
        <strong>Description:</strong> ${dept.description || '<em>none</em>'}`;
      deptGrid.appendChild(renderCard(`${index + 1}. ${dept.departmentName}`, content, footer));

      const option = document.createElement('option');
      option.value = dept.departmentId;
      option.textContent = dept.departmentName;
      deptSelect.appendChild(option);

      const filterOption = document.createElement('option');
      filterOption.value = dept.departmentId;
      filterOption.textContent = dept.departmentName;
      departmentFilter.appendChild(filterOption);
    });
    departmentCountEl.textContent = departments.length;
    updateStatus('Departments loaded.');
  } catch (error) {
    console.error(error);
  }
}

function groupEmployeesByDepartment(employeeList) {
  return employeeList.reduce((grouped, employee) => {
    const groupName = employee.departmentName || 'Unassigned';
    grouped[groupName] = grouped[groupName] || [];
    grouped[groupName].push(employee);
    return grouped;
  }, {});
}

function renderEmployees() {
  const selectedDepartmentId = Number(departmentFilter.value || 0);
  const filtered = selectedDepartmentId
    ? employees.filter((employee) => employee.departmentId === selectedDepartmentId)
    : employees;
  const sortedEmployees = [...filtered].sort((a, b) =>
    a.departmentName.localeCompare(b.departmentName)
    || a.firstName.localeCompare(b.firstName)
    || a.lastName.localeCompare(b.lastName)
  );

  empGrid.innerHTML = sortedEmployees.length ? '' : '<p>No employees found for this filter.</p>';

  let serialNumber = 1;
  const grouped = groupEmployeesByDepartment(sortedEmployees);
  Object.keys(grouped).forEach((departmentName) => {
    const header = document.createElement('h3');
    header.textContent = departmentName;
    header.className = 'group-heading';
    empGrid.appendChild(header);

    grouped[departmentName].forEach((emp) => {
      const displayNumber = serialNumber;
      serialNumber += 1;
      const footer = document.createElement('div');
      footer.append(createButton('Edit', () => editEmployee(emp)), createButton('Delete', () => deleteEmployee(emp.employeeId)));
      const content = `
        <strong>No.:</strong> ${displayNumber}<br />
        <strong>Name:</strong> ${emp.fullName}<br />
        <strong>Email:</strong> ${emp.email}<br />
        <strong>Phone:</strong> ${emp.phone || '<em>none</em>'}<br />
        <strong>Salary:</strong> ${emp.salary.toFixed(2)}<br />
        <strong>Department:</strong> ${emp.departmentName}`;
      empGrid.appendChild(renderCard(`${displayNumber}. ${emp.fullName}`, content, footer));
    });
  });

  employeeCountEl.textContent = sortedEmployees.length;
  updateStatus('Employees loaded.');
}

async function loadEmployees() {
  try {
    employees = await request('/api/Employee');
    renderEmployees();
  } catch (error) {
    console.error(error);
  }
}

function editDepartment(dept) {
  document.getElementById('deptName').value = dept.departmentName;
  document.getElementById('deptDesc').value = dept.description || '';
  deptForm.dataset.editId = dept.departmentId;
  deptForm.querySelector('h3').textContent = 'Edit Department';
}

function editEmployee(emp) {
  document.getElementById('empFirstName').value = emp.firstName || '';
  document.getElementById('empLastName').value = emp.lastName || '';
  document.getElementById('empEmail').value = emp.email;
  document.getElementById('empSalary').value = emp.salary;
  document.getElementById('empDepartmentId').value = emp.departmentId;
  document.getElementById('empPhone').value = emp.phone || '';
  employeeForm.dataset.editId = emp.employeeId;
  employeeForm.querySelector('h3').textContent = 'Edit Employee';
}

async function deleteDepartment(id) {
  if (!confirm('Delete this department?')) return;
  await request(`/api/Department/${id}`, { method: 'DELETE' });
  await loadDepartments();
  await loadEmployees();
}

async function deleteEmployee(id) {
  if (!confirm('Delete this employee?')) return;
  await request(`/api/Employee/${id}`, { method: 'DELETE' });
  await loadEmployees();
}

deptForm.addEventListener('submit', async (event) => {
  event.preventDefault();
  const id = deptForm.dataset.editId;
  const payload = {
    departmentName: document.getElementById('deptName').value.trim(),
    description: document.getElementById('deptDesc').value.trim(),
  };

  if (id) {
    await request(`/api/Department/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
    deptForm.removeAttribute('data-edit-id');
    deptForm.querySelector('h3').textContent = 'Create Department';
  } else {
    await request('/api/Department', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
  }

  deptForm.reset();
  await loadDepartments();
  await loadEmployees();
});

employeeForm.addEventListener('submit', async (event) => {
  event.preventDefault();
  const id = employeeForm.dataset.editId;
  const payload = {
    firstName: document.getElementById('empFirstName').value.trim(),
    lastName: document.getElementById('empLastName').value.trim(),
    email: document.getElementById('empEmail').value.trim(),
    phone: document.getElementById('empPhone').value.trim() || null,
    salary: Number(document.getElementById('empSalary').value),
    departmentId: Number(document.getElementById('empDepartmentId').value),
  };

  if (id) {
    await request(`/api/Employee/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
    employeeForm.removeAttribute('data-edit-id');
    employeeForm.querySelector('h3').textContent = 'Create Employee';
  } else {
    await request('/api/Employee', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
  }

  employeeForm.reset();
  await loadEmployees();
});

refreshDepartments.addEventListener('click', loadDepartments);
departmentFilter.addEventListener('change', renderEmployees);
refreshEmployees.addEventListener('click', loadEmployees);

(async function init() {
  try {
    await loadDepartments();
    await loadEmployees();
    updateStatus('Ready');
  } catch (error) {
    updateStatus('Unable to connect to API.', 'error');
  }
})();
