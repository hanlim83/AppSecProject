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
    $('#CWresults').DataTable();

    var increment = 1;
    var table = $("#" + increment);
    while (table[0] != null) {
        $('#' + increment).DataTable();
        ++increment;
        var table = $("#" + increment);
    }
});
$('#NewsFeedTable').DataTable(
    {
        "search": false,
        "responsive" : true
    }
);
function deleteSubnetInput(ID) {
    $('#subnetDeletionInput').val(ID);
    $('#deleteSubnet').modal()
}
function modifyServerInput(ID) {
    $('#modifyServerInput').val(ID);
}
