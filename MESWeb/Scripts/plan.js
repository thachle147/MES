$(document).ready(function () {
    var planObj = new planObject();
    var planGridEditable = new gridEditable();

    planGridEditable.registerEventChecked();

    $('#btnSendTarget').on('click', function () {
        var planIDList = [];
        $('.chbPlanItem:checked').each(function (index, obj) {
            planIDList[index] = $(obj).val();
        });
        if (planIDList.length) {
            ajaxGet('/Home/SendTargetToDevice', { ids: planIDList.toString() }, function (result) {
                $('#chbCheckAllPlan').attr('checked', null);
                $('.chbPlanItem').attr('checked', null);
                var msg = "Sent: " + result.Extend + "/" + result.Value;
                $.gritter.add({
                    title: 'Plan notification',
                    text: msg,
                    sticky: false
                });
                $('#btnSendTarget').hide();
            });
        }
    });

    $('.btnExportExcel').on('click', function () {
        planObj.exportExcel($(this).data('value'));
    });

    $('.btnSavePlanForm').on('click', function () {
        planObj.submitForm();
    });

    $('#btnSaveAll').on('click', function () {
        if ($('.ab-row-editing').length) {
            $('#listPlanForm').empty();
            planGridEditable.copyRowForm();
            planGridEditable.submitRows();
        }
        else {
            $.gritter.add({
                title: 'Plan notification',
                text: 'No row changed. Click [Edit] to start modify data',
                sticky: false
            });
        }
    });

    $('.ab-click-row-edit').on('click', function () {
        var _id = $(this).data('edit-for');
        if ($(this).hasClass('ab-row-editing')) { //Cancel editing
            planGridEditable.hideEditor(_id);
        }
        else { // Start editing
            planGridEditable.showEditor(_id);
        }
    });

    $('.ab-click-row-save').on('click', function () {
        $('#listPlanForm').empty();
        var _id = $(this).data('edit-for');
        planGridEditable.copyRowFormItem(_id, 0);
        planGridEditable.submitRows(function () {
            $('.ab-row-editable-' + _id + ' input[type="text"]').each(function () {
                $(this).prev().text($(this).val());
            });
            planGridEditable.hideEditor(_id);
        });
    });
});

function waitingImportExcel(isShow) {
    if (isShow) {
        $('#iframeImportExcel').hide();
        $('#divProcessing').show();
    }
    else {
        $('#divProcessing').hide();
        $('#iframeImportExcel').show();
        setTimeout(function () {
            location.reload();
        }, 500);
    }
};

function planObject() {
    var _self = this;

    this.exportExcel = function (_day) {
        if (_day == 'online device')
            _day = -1;
        window.location = '/Home/ExportExcel?_day=' + _day;
    };

    this.submitForm = function () {
        if (_self.isValidForm()) { //Check form is valid
            ajaxPost('/Home/SavePlan', $('#planForm').serialize(), function (result) {
                window.scrollTo(0, 0);
                if (result.IsSuccess) {
                    $('#divSaveSuccess').show();
                    setTimeout(function () {
                        location.href = '/Home/Index';
                    }, 1000);
                }
                else {
                    $('#divValidationMsg').hide();
                    $('#divSaveErrorMsg').show();
                    $('#divPlanFormMsg').show();
                }
            });
        }
    };

    this.isValidForm = function () {
        var isValid = true;
        $('input.ab-required:visible, select.ab-required').each(function () {
            if ($(this).val().length) { //Valid
                $(this).css('border', '');
                isValid = isValid && true;
            }
            else {
                $(this).css('border', '1px solid #DA542E');
                isValid = isValid && false;
            }
        });
        if (isValid)
            $('#divPlanFormMsg').hide();
        else
            $('#divPlanFormMsg').show();
        return isValid;
    }
}

function gridEditable() {
    var _self = this;

    this.registerEventChecked = function () {
        $('#chbCheckAllPlan').on('click', function () {
            if ($(this).is(':checked'))
                $('.chbPlanItem').attr('checked', 'true');
            else
                $('.chbPlanItem').attr('checked', null);
            _self.checkSendTarget();
        });

        $('.chbPlanItem').on('click', function () {
            if ($('.chbPlanItem:checked').length == $('.chbPlanItem').length)
                $('#chbCheckAllPlan').attr('checked', 'true');
            else
                $('#chbCheckAllPlan').attr('checked', null);
            _self.checkSendTarget();
        });
    };

    this.checkSendTarget = function () {
        if ($('.chbPlanItem:checked').length) {
            $('#btnSendTarget').show();
        }
        else {
            $('#btnSendTarget').hide();
        }
    };

    this.showEditor = function (id) {
        $('#btnEditRow_' + id).addClass('ab-row-editing');
        $('#btnEditRow_' + id).text('Cancel');
        $('#btnSaveRow_' + id).show();
        $('.ab-cell-edit-' + id + ' input').show();
        $('.ab-cell-edit-' + id + ' span').hide();
        $($('.ab-row-editable-' + id + ' input[type="text"]')[0]).focus();
        $('#btnSaveAll').show();
    }

    this.hideEditor = function (id) {
        $('#btnEditRow_' + id).removeClass('ab-row-editing');
        $('#btnEditRow_' + id).html('<i style="color:#C94E1B;">Edit</i>');
        $('#btnSaveRow_' + id).hide();
        $('.ab-cell-edit-' + id + ' input').hide();
        $('.ab-cell-edit-' + id + ' span').show();
        if (!$('.ab-row-editing').length)
            $('#btnSaveAll').hide();
    };

    this.copyRowForm = function () {
        $('.ab-row-editing').each(function (index) {
            var _id = $(this).data('edit-for');
            _self.copyRowFormItem(_id, index);
        });
    };

    this.copyRowFormItem = function (_id, index) {
        var _form = '<div><input type="text" name="[' + index + '].ID" value="' + _id + '" />';
        $('.ab-row-editable-' + _id + ' input[type="text"]').each(function (_index, _element) {
            _form += '<input type="text" name="[' + index + '].' + $(_element).attr('name') + '" value="' + $(_element).val() + '" />';
            _form += '</div>';
        });
        $('#listPlanForm').append(_form);
    };

    this.submitRows = function (callback) {
        ajaxPost('/Home/SavePlanList', $('#listPlanForm').serialize(), function (result) {
            if (result.IsSuccess) {
                $.gritter.add({
                    title: 'Plan notification',
                    text: 'Save all data completed',
                    sticky: false,
                    class_name: 'ab-gritter-info'
                });
                if (callback != null)
                    callback();
                else
                    setTimeout(function () {
                        location.reload();
                    }, 1000);
            }
            else {
                $.gritter.add({
                    title: 'Plan notification',
                    text: 'Error! Save changes failed. Try again later.',
                    sticky: false
                });
            }
        });
    };
}