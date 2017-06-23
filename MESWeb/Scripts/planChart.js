var planChartObj = new planChartObject();
var planColumnChartObj;

$(document).ready(function () {
    planChartObj.registerEventCheckbox();

    $('#btnViewPlanChart').on('click', function () {
        planChartObj.loadChart();
    });

    //$('#divPopupDisplayChart').on('hidden', function () {
    //    clearInterval(planChartObj.intervalReloadChart);
    //});

    planChartObj.renderChartPager($('#hidCurrentPage').val(), $('#hidTotal').val(), 64);

    
});


function planChartObject() {
    var _self = this;
    this.planChartSelected = [];
    this.timeoutReloadChart = null;
    //this.intervalReloadChart = null;
    this.chartData = [];

    this.registerEventCheckbox = function () {
        $('#chbAll').on('click', function () {
            if ($(this).is(':checked')) {
                $('.chbViewPlanChart').attr('checked', 'true');
                $('.chbViewPlanChart:checked').each(function (index, obj) {
                    _self.planChartSelected.push($(obj).val());
                });
            }
            else {
                $('.chbViewPlanChart').attr('checked', null);
                _self.planChartSelected = [];
            }
        });

        $('.chbViewPlanChart').on('click', function () {
            if ($('.chbViewPlanChart:checked').length == $('.chbViewPlanChart').length)
                $('#chbAll').attr('checked', 'true');
            else
                $('#chbAll').attr('checked', null);

            var _value = $(this).val();

            if ($(this).is(':checked'))
                _self.planChartSelected.push(_value);
            else {
                _self.planChartSelected = $.grep(_self.planChartSelected, function (value) {
                    return value != _value;
                });
            }
        });
    };

    this.loadChart = function () {

        $.ajax({
            url: '/Statistics/SetupChartObject',
            type: 'POST',
            data: $.param({ list: _self.planChartSelected }, true),
            beforeSend: function () {
                //waitingDialog.show();
            },
            success: function (chartName) {
                //waitingDialog.hide();
                $('body').append('<a style="display:none;" id="' + chartName + '" href="/Statistics/ShowPlanChart?chartId=' + chartName + '&pageNum=1" target="_blank">');
                $('#' + chartName)[0].click();
            }
        });

    }

    this.renderChartPager = function (current, items, size) {
        if (current < 1)
            current = 1;

        var url = '/Statistics/ShowPlanChart';
        var link = url + location.search.split('&pageNum=')[0] + '&pageNum=';

        var pages = parseInt(items / size);
        if (items % size > 0)
            pages++;
        var range = 4;
        var start = current - range;
        var end = parseInt(current) + parseInt(range);
        if (start < 1)
            start = 1;
        if (end > pages)
            end = pages;

        var html = '<ul>';

        //Prev button
        if (current > 1)
            html += '<li><a href="' + link + (current - 1) + '">Prev</a></li>';
        else
            html += '<li><a href="#">Prev</a></li>';

        //Item number button
        for (var i = start; i <= end; i++) {
            if (i == current) {
                html += '<li class="active"><a href="#">' + i + '</a></li>';
            }
            else {
                html += '<li><a href="' + link + i + '">' + i + '</a></li>';
            }
        }

        //Next button
        if (current < end)
            html += '<li><a class="planchart-next-reload" href="' + link + (parseInt(current) + 1) + '">Next</a></li></ul>';
        else
            html += '<li><a class="planchart-next-reload" href="#">Next</a></li>';

        html += '</ul>';

        $('#divChartPager').html(html);
    };
}