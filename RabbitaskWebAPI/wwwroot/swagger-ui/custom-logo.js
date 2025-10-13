// custom-logo.js

window.onload = function () {
    // Select the topbar and customize it
    const topbar = document.querySelector('.swagger-ui .topbar');
    if (topbar) {
        // Clear existing content
        topbar.innerHTML = '';

        // Create a new logo element
        const logo = document.createElement('img');
        logo.src = '/images/logoRabbitask.png'; // Path to your custom logo
        logo.style.height = '40px';    // Adjust logo dimensions as needed
        logo.style.verticalAlign = 'middle';

        // Custom link to the homepage
        const logoLink = document.createElement('a');
        logoLink.href = 'https://example.com';  // Add your website URL
        logoLink.appendChild(logo);

        // Append the logo to the topbar
        topbar.appendChild(logoLink);
    }
}