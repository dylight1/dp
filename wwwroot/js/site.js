
function handleMissingProductImage(img) {
    img.onerror = function() {
        this.src = '/images/no-product-image.png';
        this.onerror = null; // Prevent infinite loop if default image is also missing
    };
    return true;
}

// Apply missing image handler to all product images
document.addEventListener('DOMContentLoaded', function() {
    // Find all images with product image class or in product containers
    const productImages = document.querySelectorAll('img[src^="/uploads/images/"]');
    productImages.forEach(img => {
        handleMissingProductImage(img);
    });
});

// Add scroll effect to navbar
window.addEventListener('scroll', function () {
    const navbar = document.querySelector('.navbar');
    if (window.scrollY > 50) {
        navbar.classList.add('scrolled');
    } else {
        navbar.classList.remove('scrolled');
    }
});

// Toast notification helper function
function showToast(type, title, message) {
    // Create toast element
    const toast = $('<div class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="4000">');
    
    // Create header based on type
    const headerClass = type === 'success' ? 'bg-success' : type === 'error' ? 'bg-danger' : 'bg-primary';
    const header = $('<div class="toast-header ' + headerClass + ' text-white">');
    header.append('<strong class="me-auto">' + title + '</strong>');
    header.append('<button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>');
    
    // Create body
    const body = $('<div class="toast-body">').html(message);
    
    // Assemble toast
    toast.append(header).append(body);
    
    // Add to container
    $('#toastContainer').append(toast);
    
    // Initialize and show
    const bsToast = new bootstrap.Toast(toast.get(0));
    bsToast.show();
    
    // Remove from DOM when hidden
    toast.on('hidden.bs.toast', function() {
        $(this).remove();
    });
    
    return toast;
}

// Handle add to cart AJAX requests
function handleAddToCart(productId, quantity = 1) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { productId: productId, quantity: quantity },
        success: function(response) {
            if (response.success) {
                // Update the cart count
                updateCartCount();
                
                // Show success toast
                showToast('success', 'Success', 'Product added to cart successfully! <a href="/Cart" class="text-decoration-none fw-bold">View Cart</a>');
            } else if (response.requiresAuth) {
                // User needs to log in
                showToast('error', 'Authentication Required', 'Please <a href="/Identity/Account/Login?ReturnUrl=' + encodeURIComponent(window.location.pathname) + '" class="text-decoration-none fw-bold">log in</a> or <a href="/Identity/Account/Register?ReturnUrl=' + encodeURIComponent(window.location.pathname) + '" class="text-decoration-none fw-bold">register</a> to add items to your cart.');
                
                // Option to redirect after short delay
                setTimeout(function() {
                    window.location.href = '/Identity/Account/Login?ReturnUrl=' + encodeURIComponent(window.location.pathname);
                }, 3000);
            }
        },
        error: function() {
            // Show error toast
            showToast('error', 'Error', 'Failed to add product to cart. Please try again.');
        }
    });
}
