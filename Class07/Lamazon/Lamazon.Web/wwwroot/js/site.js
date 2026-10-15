// JavaScript for the shop (every page that uses _Layout)

// The anti-forgery token that _Layout renders once (@Html.AntiForgeryToken()).
// ASP.NET Core accepts it in the "RequestVerificationToken" header of an AJAX request.
function getAntiForgeryToken() {
    return $('input[name="__RequestVerificationToken"]').first().val();
}

// The red counter on the cart icon in the navbar
function updateShoppingCartItemsCount(count) {
    var counter = $('#shoppingCartItemsCount');
    counter.text(count);
    counter.toggleClass('hidden', count <= 0);
}

// Add to cart / Remove from cart: POST the product id, the server updates the cart cookie
// and answers with the new number of items: { "itemsCount": 3 }
function changeShoppingCart(clickedButton) {
    $.ajax({
        type: 'POST',
        url: clickedButton.data('url'),
        data: { productId: clickedButton.data('productId') },
        headers: { 'RequestVerificationToken': getAntiForgeryToken() },
        success: function (result) {
            updateShoppingCartItemsCount(result.itemsCount);

            // In the shopping cart itself, removing a product reloads the page (new list, new total)
            if (clickedButton.data('reload')) {
                window.location.reload();
                return;
            }

            // Show the other button of the pair (Add ⇄ Remove)
            clickedButton.addClass('hidden');
            clickedButton.siblings('.btn-add-to-cart, .btn-remove-from-cart').removeClass('hidden');
        },
        error: function () {
            notyf.error('Something went wrong. Please try again.');
        }
    });
}

// "Event delegation": one handler on the document works for every button, also ones added later
$(document).on('click', '.btn-add-to-cart, .btn-remove-from-cart', function () {
    changeShoppingCart($(this));
});
