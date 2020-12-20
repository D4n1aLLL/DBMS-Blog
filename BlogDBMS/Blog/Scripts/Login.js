function getPasswordHash(passwordElement, nonceElement, hashElement)
{
    var password = $('#' + passwordElement).attr('value');
    var nonce = $('#' + nonceElement).attr('value');
    $('#' + hashElement).attr('value', $.sha256(password + nonce));
    $('#' + passwordElement).attr('value', '');
}