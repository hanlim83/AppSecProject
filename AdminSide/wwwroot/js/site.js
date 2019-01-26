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
    $('#CWresults').DataTable({
        colReorder: true,
        responsive: true
    });

    var incerment = 1;
    var table = $("#" + incerment);
    while (table[0] != null) {
        $('#' + incerment).DataTable();
        ++incerment;
        var table = $("#" + incerment);
    }
});
function deleteSubnetInput(ID) {
    $('#subnetDeletionInput').val(ID);
    $('#deleteSubnet').modal()
}
function modifyServerInput(ID) {
    $('#modifyServerInput').val(ID);
}