$(document).ready(function () {
    var url = window.location;
    $('.navbar .nav').find('.active').removeClass('active');
    $('.navbar .nav li a').each(function () {
        if (this.href == url) {
            $(this).parent().addClass('active');
        }
    });
    $('a.list-group-item').each(function () {
        if (this.href == url) {
            $(this).addClass('active');
        }
    });
});