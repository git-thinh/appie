
//alert('plugin.js');

document.addEventListener('DOMContentLoaded', function (event) {
    var a;
    a = document.querySelectorAll('img'); for (var i = 0; i < a.length; i++) { a[i].remove(); };
    a = document.querySelectorAll('*'); for (var i = 0; i < a.length; i++) { a[i].style.height = 'auto'; };
    console.log('DOM_CONTENT_LOADED');
});