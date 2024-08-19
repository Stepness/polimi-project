function loadUsers() {
    let jwtToken = localStorage.getItem('jwtToken');
    $.ajax({
        url: 'http://localhost:5245/users',
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${jwtToken}`
        },
        success: function(users) {
            populateUserTable(users);
            
        },
        error: function() {
            alert('Failed to load users');
        }
    });
}

function populateUserTable(users) {
    const tbody = $('#users-table tbody');
    tbody.empty();
    users.forEach(user => {
        const tr = $('<tr></tr>');
        tr.append(`<td>${user.username}</td>`);
        tr.append(`<td>${user.role}</td>`);
        tr.append(`<td><button class="promote-btn" data-username="${user.username}">Promote</button></td>`);
        tbody.append(tr);
    });

    $('.promote-btn').on('click', function() {
        const username = $(this).data('username');
        promoteUser(username);
    });
}

function promoteUser(username) {
    let jwtToken = localStorage.getItem('jwtToken');
    $.ajax({
        url: `http://localhost:5245/users/${username}/promote-role`,
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${jwtToken}`
        },
        success: function() {
            alert(`${username} has been promoted!`);
            loadUsers(); 
        },
        error: function() {
            alert('Failed to promote user');
        }
    });
}

$('#logout').on('click', function() {
    localStorage.removeItem('jwtToken');
    window.location.href = './login.html';
});
