function AddToCart(id)
{
    $.ajax({
        type: 'POST',
        url: "@Url.Content(~/Posts/Like)",
        data: {id},
        success: function (result) {
            alert('hi'); 
        }
    });
}