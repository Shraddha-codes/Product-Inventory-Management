$(document).ready(function () {
    fetchCategories();
    fetchProducts();

    $("#signupForm").on("submit", function (event) {
        event.preventDefault();
        const username = $("#username").val();
        const password = $("#password").val();
        $.ajax({
            url: "/api/user/signup",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({ username, password }),
            success: function () {
                alert("Signup successful! Please log in.");
                window.location.href = "login.html";
            },
            error: function (xhr) {
                alert("Signup failed. Please try again.");
                console.error("Error: " + xhr.responseText);
            }
        });
    });

    $("#loginForm").on("submit", function (event) {
        event.preventDefault();
        const username = $("#loginUsername").val();
        const password = $("#loginPassword").val();
        $.ajax({
            url: "/api/user/login",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({ username, password }),
            success: function (response) {
                localStorage.setItem("token", response.token);
                localStorage.setItem("role", response.role); 
                alert("Login successful!");
                window.location.href = "products.html"; 
            },
            error: function (xhr) {
                alert("Login failed. Please check your credentials.");
                console.error("Error: " + xhr.responseText);
            }
        });
    });

    $("#filterButton").on("click", function () {
        const selectedCategory = $("#categoryFilter").val();
        fetchProducts(selectedCategory);
    });

    checkAdminRole();

    $("#showCreateProductForm").on("click", function () {
        $("#createProductSection").toggle();
        $("#adjustPriceSection").hide(); 
    });

    $("#showAdjustPriceForm").on("click", function () {
        $("#adjustPriceSection").toggle();
        $("#createProductSection").hide(); 
    });

$("#createProductForm").on("submit", function (event) {
    event.preventDefault();
    
    const productData = {
        name: $("#productName").val().trim(),
        description: $("#productDescription").val().trim(),
        price: parseFloat($("#productPrice").val()),
        SKU: $("#productSKU").val().trim(),
        categories: $("#productCategories").val().split(',').map(cat => cat.trim()).filter(cat => cat !== ''),
        inventory: parseInt($("#productInventory").val())
    };

    // Validate required fields
    if (!productData.name || 
        !productData.description || 
        isNaN(productData.price) || 
        !productData.SKU || 
        !productData.categories.length || 
        isNaN(productData.inventory)) {
        alert("Please fill in all required fields.");
        return;
    }

    createProduct(productData);
});


    $("#adjustPriceForm").on("submit", function (event) {
        event.preventDefault();
        const id = parseInt($("#priceProductId").val());
        const newPrice = parseFloat($("#newPrice").val());
        adjustPrice(id, newPrice);
    });

    $("#openCreateProductModal").on("click", function () {
        $("#createProductModal").css("display", "block");
    });

    $("#openAdjustPriceModal").on("click", function () {
        $("#adjustPriceModal").css("display", "block");
    });

    $("#closeCreateProductModal").on("click", function () {
        $("#createProductModal").css("display", "none");
    });

    $("#closeAdjustPriceModal").on("click", function () {
        $("#adjustPriceModal").css("display", "none");
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#createProductModal")) {
            $("#createProductModal").css("display", "none");
        }
        if ($(event.target).is("#adjustPriceModal")) {
            $("#adjustPriceModal").css("display", "none");
        }
    });
});

function checkAdminRole() {
    const role = localStorage.getItem("role");
    if (role === "Admin") {
        $("#adminFunctions").show();
    }
}

function createProduct(productData) {
    $.ajax({
        url: "/api/v1/products/createProduct",
        method: "POST",
        contentType: "application/json",
        headers: {
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        data: JSON.stringify(productData),
        success: function () {
            alert("Product created successfully!");
            fetchProducts(); 
        },
        error: function (xhr) {
            alert("Failed to create product: " + xhr.responseText);
            console.error("Error: " + xhr.responseText);
        }
    });
}



function adjustPrice(id, newPrice) {
    $.ajax({
        url: `/api/v1/products/product/${id}/price`,
        method: "PATCH",
        contentType: "application/json",
        headers: {
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        data: JSON.stringify({ price: newPrice }), // Wrap newPrice in an object
        success: function () {
            alert("Price adjusted successfully!");
            fetchProducts(); 
        },
        error: function (xhr) {
            alert("Failed to adjust price.");
            console.error("Error: " + xhr.responseText);
        }
    });
}

function fetchCategories() {
    $.ajax({
        url: "/api/v1/products/categories", 
        method: "GET",
        headers: {
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        success: function (categories) {
            categories.forEach(category => {
                $("#categoryFilter").append(`<option value="${category}">${category}</option>`);
                $("#productCategories").append(`<option value="${category}">${category}</option>`); 
            });
        },
        error: function (xhr) {
            console.error("Error fetching categories:", xhr.responseText);
        }
    });
}

function fetchProducts(category = '') {
    $.ajax({
        url: `/api/v1/products/products?category=${encodeURIComponent(category)}`,
        method: "GET",
        headers: {
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        success: function (products) {
            $("#productList").empty();
            products.forEach(product => {
                const productItem = `
                    <div class="product-item">
                        <div class="product-name">${product.name}</div>
                        <div class="product-details">
                            <p>Description: ${product.description}</p>
                            <p>Price: $${product.price}</p>
                            <p>SKU: ${product.sku}</p>
                            <p>Quantity: ${product.quantity}</p>
                        </div>
                    </div>`;
                $("#productList").append(productItem);
            });

            if ($("#productList").is(':empty')) {
                $("#productList").append("<p>No products found for the selected category.</p>");
            }
        },
        error: function (xhr) {
            alert("Failed to load products.");
            console.error("Error: " + xhr.responseText);
        }
    });
}
