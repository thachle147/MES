var orgObj = new organizationObject();

$(document).ready(function () {
    $('.btnEditCompany').on('click', function () {
        orgObj.popupCompanyForm($(this).data('id'));
    });

    $('.btnEditFactory').on('click', function () {
        orgObj.popupFactoryForm($(this).data('id'));
    });

    $('#btnSaveCompany').on('click', function () {
        orgObj.saveCompany();
    });

    $('#btnSaveFactory').on('click', function () {
        orgObj.saveFactory();
    });

    orgObj.registerWorkerEvent();

});

function organizationObject() {
    var _self = this;

    this.popupCompanyForm = function (_id) {
        var _value = $('#' + _id).text();
        _self.bindDataCompanyForm(_id, _value);
        $('#modalCompanyForm').modal();
    };

    this.bindDataCompanyForm = function (formId, companyName) {
        $('#txtCompanyName').val(companyName);
        $('#hidCompanyName').val(companyName);
        $('#hidCompanyFormId').val(formId);
    };

    this.saveCompany = function () {
        ajaxGet('/Organization/SaveCompany', { current: $('#hidCompanyName').val(), name: $('#txtCompanyName').val() }, function (result) {
            if (result.IsSuccess) {
                abNotify('', 'Save change company successful', true);
                //update UI
                var _id = $('#hidCompanyFormId').val();
                $('#' + _id).text($('#txtCompanyName').val());

                _self.bindDataCompanyForm('', '');
                $('#modalCompanyForm').modal('hide');
            }
            else {
                abNotify('', 'Error! Please try again', false);
            }
        });
    };

    
    //--- Factory
    //------------------------------------------------------------------------
    this.popupFactoryForm = function (_id) {
        var _value = $('#' + _id).text();
        _self.bindDataFactoryForm(_id, _value);
        $('#modalFactoryForm').modal();
    };

    this.bindDataFactoryForm = function (formId, factoryName) {
        $('#txtFactoryName').val(factoryName);
        $('#hidFactoryName').val(factoryName);
        $('#hidFactoryFormId').val(formId);
    };

    this.saveFactory = function () {
        ajaxGet('/Organization/SaveFactory', { current: $('#hidFactoryName').val(), name: $('#txtFactoryName').val() }, function (result) {
            if (result.IsSuccess) {
                abNotify('', 'Save change Factory successful', true);
                //update UI
                var _id = $('#hidFactoryFormId').val();
                $('#' + _id).text($('#txtFactoryName').val());

                _self.bindDataFactoryForm('', ''); //clear form
                $('#modalFactoryForm').modal('hide');
            }
            else {
                abNotify('', 'Error! Please try again later', false);
            }
        });
    };


    //--- Worker
    //------------------------------------------------------------------------
    this.registerWorkerEvent = function () {
        $('#btnAddNewWorker').off().on('click', function () {
            orgObj.popupWorkerForm('');
        });

        $('#btnSaveWorkerForm').off().on('click', function () {
            orgObj.saveWorker();
        });

        $('.btnEditWorker').off().on('click', function () {
            orgObj.popupWorkerForm($(this).data('id'));
        });

        $('.btnDeleteWorker').off().on('click', function () {
            orgObj.deleteWorker($(this).data('id'));
        });


    };

    this.popupWorkerForm = function (_workerNo) {
        ajaxGet('/Organization/WorkerDetail', { workerNo: _workerNo }, function (result) {
            $('#divWorkFormContainer').html(result);
            $('#modalWorkerForm').modal(); //show popup
            _self.registerWorkerEvent();
            if ($('#Cus_worker').val().length)
                $('#Cus_worker').attr('disabled', true);
        });
    };

    this.saveWorker = function () {
        if ($('#Cus_worker').val().length) {
            $('#Cus_worker').attr('disabled', null);
            $('#Cus_worker').css('border', '');
            ajaxPost('/Organization/SaveWorker', $('#workerForm').serialize(), function (result) {
                if (result.IsSuccess) {
                    abNotify('', 'Save work successful', true);
                    $('#modalWorkerForm').modal('hide');//hide popup
                    setTimeout(function () {
                        location.href = '/Organization/Worker';
                    }, 1000);
                }
                else {
                    abNotify('', 'Error! Please try again later', false);
                }
            });
        }
        else {
            //abNotify('Worker Form', 'Worker No. is requried', false);
            $('#Cus_worker').css('border', '1px solid #b94a48');
        }
    };

    this.deleteWorker = function (_workerNo) {
        if (confirm("Do you want to delete this worker ?")) {
            ajaxGet('/Organization/DeleteWorker', { workerNo: _workerNo }, function (result) {
                location.href = '/Organization/Worker';
            });
        }
    };
}