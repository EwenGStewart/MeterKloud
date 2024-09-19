


function SetupTabsForData() {
    //Enable all the tabls
    var list = $("#myTabs li");
    list.each(function () { TabEnable(this); });
    TabActivate($(list[1]));
}



function SetupTabsForNoData() {
    //disable all the tabs
    var list = $("#myTabs li");
    TabActivate($(list[0]));
    list.each(function (i) { if (i > 0) TabDisable($(this)); });

}

function TabEnable(e) {
    if (!e.jquery) { e = $(e); }
    if (e.hasClass(DISABLED)) { e.removeClass(DISABLED); }


}

function TabDisable(e) {
    if (e.hasClass("active")) { e.removeClass("active"); }
    if (!e.hasClass(DISABLED)) {
        e.addClass(DISABLED);
    }

}

function TabActivate(e) {
    if (!e.jquery) { e = $(e); }
   
    if (e.hasClass(DISABLED)) { TabEnable(e); }

    e.parent().find("LI.active").removeClass("active");



    e.addClass("active");
    e.children('A').tab('show');
    var trgt = e.children('A').attr("data-target");
    $('#myTabPanels .tab-pane.active').removeClass("active");
    $(trgt).addClass('active');
    ProcessTab(trgt);
}






function TabSetup() {
    $('#myTabs a').click(function (e) {

        if ($(e).attr("href") > "") return;

        e.preventDefault();
        if ($(this).parent().hasClass(DISABLED)) {
            return false;
        }
        $(this).tab('show');
        return true;
    });



    $('#myTabs a').on('shown.bs.tab', function (e) {
        var target = $(e.target).attr("data-target"); // activated tab
     
        ProcessTab(target);
    });





}



function ProcessTab(s)
{
    switch (s) {

        case "#Analysis":
            PlotAnalysisClick();
            break;

        case "#DailyTotals":
            PlotDailyTotalsTabClick();
            break;

        case "#Detail":
            PlotMinuteTotalsTabClick();
            break;

        case "#HeatMap":
            PlotHeapMapTabClick() ;
            break;

        case "#3DProfile":
            PlotThreeDTabClick();
            break; 
        case "#DayProfile":
            PlotProfileTabClick();
            break;



        case "#MaxDemand":
            PlotDemandMinuteTotalsTabClick();
            break;

        case "#Export":
            ShowExportTabClick();
            break;


        default:
            break;


    }
}