var settingsObj = new settingsObject();

$(document).ready(function () {
    settingsObj.registerEvent_ExportColumnChecked();

    $('#tableExportColumns td').each(function () {
        $(this).css('width', $(this).width() + 'px');
    });

    $("#tableExportColumns tbody").sortable({
        items: ".ab-row-moveable",
        start: function (event, ui) {
            var _item = ui.item;
            $(_item).addClass('ab-sorting');
        },
        stop: function (event, ui) {
            var _item = ui.item;
            $(_item).removeClass('ab-sorting');
        }
    });
});

function settingsObject() {
    var _self = this;

    this.registerEvent_ExportColumnChecked = function () {
        $('#chbAll_ExportColumn').off().on('click', function () {
            if ($(this).is(':checked')) {
                $('.chbExportColumnItem').attr('checked', 'true');
                $('.chbExportColumnItem').parents('tr').addClass('ab-selected-row');
            }
            else {
                $('.chbExportColumnItem').attr('checked', null);
                $('.chbExportColumnItem').parents('tr').removeClass('ab-selected-row');
            }
        });

        $('.chbExportColumnItem').off().on('click', function () {
            if ($('.chbExportColumnItem:checked').length == $('.chbExportColumnItem').length) {
                $('#chbAll_ExportColumn').attr('checked', 'true');
            }
            else {
                $('#chbAll_ExportColumn').attr('checked', null);
            }

            if ($(this).is(':checked')) {
                $(this).parents('tr').addClass('ab-selected-row');
            }
            else {
                $(this).parents('tr').removeClass('ab-selected-row');
            }
        });

        $('#btnSaveExportColumn').off().on('click', function () {
            var valueList = [];
            $('.chbExportColumnItem:checked').each(function (index, obj) {
                valueList[index] = $(obj).val();
            });
            var msg = '';
            if (valueList.length) {
                ajaxGet('/Settings/UpdateValue', { id: 1, value: valueList.toString() }, function (result) {
                    if (result.IsSuccess) {
                        msg = 'Update setting successful';
                    }
                    else {
                        msg = 'Error! Please try again later';
                    }
                    abNotify('', msg, result.IsSuccess);
                });
            }
        });
    };
}