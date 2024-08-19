function getTokenBody(){
    var jwtToken = localStorage.getItem('jwtToken');
  
    var tokenParts = jwtToken.split('.');
    var base64Url = tokenParts[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var decodedToken = JSON.parse(atob(base64));
  
    return decodedToken;
  }