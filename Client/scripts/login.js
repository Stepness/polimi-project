async function register() {
    var username = $('#username').val().trim();
    var password = $('#password').val();
  
    var requestBody = {
      username: username,
      password: password
    };
  
    await $.ajax({
      url: 'http://localhost:5245/users/register',
      type: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(requestBody),
      success: function(response) {
        localStorage.setItem('jwtToken', response);
        location.href = './index.html';
      }
    });
  }

  async function login() {
    var username = $('#username').val().trim();
    var password = $('#password').val();
  
    var requestBody = {
      username: username,
      password: password
    };
  
    await $.ajax({
      url: 'http://localhost:5245/users/login',
      type: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(requestBody),
      success: function(response) {
        localStorage.setItem('jwtToken', response);
        location.href = './index.html';
      }
    });
  }