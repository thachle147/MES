$(document).ready(function () {
    highlightMenu();
});

function highlightMenu() {
    var _href = location.pathname;
    var planMenu = { id: 'planMenu', links: ["/", "/Home/Index", "/Home/PlanForm"], child:[] };
    var statisticsMenu = { id: 'statisticsMenu', links: ["/Statistics", "/Statistics/PlanChart"], child: [] };
    var deviceMenu = { id: 'deviceMenu', links: ["/Device", "/Device/Index"], child: [] };
    var companyMenu = { id: 'companyMenu', links: ["/Organization/Company"], child: [] };
    var factoryMenu = { id: 'factoryMenu', links: ["/Organization/Factory"], child: [] };
    var workerMenu = { id: 'workerMenu', links: ["/Organization/Worker"], child: [] };
    var orgMenu = { id: 'organizationMenu', links: ["/Organization/Company", "/Organization/Factory", "/Organization/Worker"], child: [companyMenu, factoryMenu, workerMenu] };

    var menuList = [planMenu, statisticsMenu, deviceMenu, orgMenu];
    $('#sidebar .submenu ul').hide();
    for (var i = 0; i < menuList.length; i++) {
        if (menuList[i].links.indexOf(_href) >= 0) {
            $('#sidebar .active').removeClass('active');
            $('#' + menuList[i].id).addClass('active');
            if (menuList[i].child.length) {
                $('#' + menuList[i].id + ' ul').show();
                for (var c = 0; c < menuList[i].child.length; c++) {
                    if (menuList[i].child[c].links.indexOf(_href) >= 0) {
                        $('#sidebar li.submenu .ab-active-submenu').removeClass('ab-active-submenu');
                        $('#' + menuList[i].child[c].id + ' a').addClass('ab-active-submenu');
                        break;
                    }
                }
            }
            break;
        }
    }
}

function abNotify(title, message, isInfo) {
    $.gritter.add({
        title: title.length ? title : 'Notification',
        text: message,
        sticky: false,
        class_name: isInfo ? 'ab-gritter-info' : ''
    });
}

function ajaxPost(url, data, fnSuccess) {
    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        beforeSend: function () {
            waitingDialog.show();
        },
        success: function (result) {
            waitingDialog.hide();
            fnSuccess(result);
        }
    });
}

function ajaxGet(url, data, fnSuccess) {
    $.ajax({
        url: url,
        type: 'GET',
        data: data,
        beforeSend: function () {
            waitingDialog.show();
        },
        success: function (result) {
            waitingDialog.hide();
            fnSuccess(result);
        }
    });
}

