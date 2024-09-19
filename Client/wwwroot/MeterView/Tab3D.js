

function PlotThreeDTabClick() {
    if (Global.Tab_3D_Setup) return;
    //PlotThreeD();
    
    setTimeout(PlotThreeD, 100);
    Global.Tab_3D_Setup = true;
}


function PlotThreeD() {


    var plotData = [{
        "autocolorscale": false,
        "colorscale": [[0, "rgb(150,0,90)"], [0.125, "rgb(0,0,200)"], [0.25, "rgb(0,25,255)"], [0.375, "rgb(0,152,255)"], [0.5, "rgb(44,255,150)"], [0.625, "rgb(151,255,0)"], [0.75, "rgb(255,234,0)"], [0.875, "rgb(255,111,0)"], [1, "rgb(255,0,0)"]],
        "z": [],
        "y": [],
        "x": [],
        "contours": {
            "size": 200,
            "coloring": "fill",
            "showlines": true
        },
        "type": "surface",
        mode: "lines",
        "zauto": true,
        "name": "y",
        "cauto": true
    }];
    var layout = {
        "autosize": true,
        "xaxis": { title: "", type: "category" },
        "yaxis": { title: "", type: "category" },
        zaxis: {
            title: "kWh"
        },
        "scene": {
            "aspectratio": {
                "y": 2,
                "x": 1,
                "z": 1
            },
            camera: {
                eye: {
                    y: 1.25,
                    x: 1.25,
                    z: 1.25
                },
                up: {
                    y: 0,
                    x: 0,
                    z: 1
                },
                center: {
                    y: 0,
                    x: 0,
                    z: 0
                }
            },
            "dragmode": "turntable"
        },

        "font": { "size": 10 },
        "margin": { "l": 20, "r": 20, "t": 20, "b": 20, "pad": 0 },
        "paper_bgcolor": "rgb(255, 255, 255)",
        title: "Net kWh 3d Area Plot"
    };




 
    plotData[0].x = (['Midnight', '00:30am', '1:00am', '1:30am', '2:00am', '2:30am', '3:00am', '3:30am', '4:00am', '4:30am', '5:00am', '5:30am'
                    , '6:00am', '6:30am', '7:00am', '7:30am', '8:00am', '8:30am', '9:00am', '9:30am', '10:00am', '10:30am', '11:00am', '11:30am'
                    , 'Midday', '12:30pm', '1:00pm', '1:30pm', '2:00pm', '2:30pm', '3:00pm', '3:30pm', '4:00pm', '4:30pm', '5:00pm', '5:30pm'
                    , '6:00pm', '6:30pm', '7:00pm', '7:30pm', '8:00pm', '8:30pm', '9:00pm', '9:30pm', '10:00pm', '10:30pm', '11:00pm', '11:30pm'
    ]).reverse();




    Global.MeterData.Days.forEach(function (d) {

        var s = moment(d.Date).format("Do-MMM-YY");
        plotData[0].y.push(s);
        plotData[0].z.push(d.Get30minValue(NET).reverse());
    });



    widgetBody = $("#ThreeDContainer");




    // plotData[0].x = Global.MeterData.GetDayList();

    // plotData[0].y = Global.MeterData.GetDayValue(CONSUMPTION);




    widgetBody.html('');

    var h = $(window).height() - widgetBody.offset().top - 20;
    if (h < 500) h = 500;
    widgetBody.height(h);


    layout.height = widgetBody.height();
    layout.width = widgetBody.width();
    Plotly.newPlot(widgetBody[0], plotData, layout);

    Plotly.Plots.resize(widgetBody[0]);

}
