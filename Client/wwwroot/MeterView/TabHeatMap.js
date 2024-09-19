

function PlotHeapMapTabClick() {
    if (Global.Tab_HeatMap_Setup) return;
   // PlotHeatMap();
    setTimeout(PlotHeatMap, 100);
    Global.Tab_HeatMap_Setup = true;
}


function PlotHeatMap() {


    var plotData = [{
        "autocolorscale": false,
        colorscale: [
[
0,
"rgb(0,0,0)"
],
[
0.2,
"rgb(230,0,0)"
],
[
0.6,
"rgb(255,210,0)"
],
[
1,
"rgb(255,255,255)"
]
        ],

        autobiny: true,
        autobinx: true,
        "z": [],
        "y": [],
        "x": [],
        "contours": {
            "size": 200,
            "coloring": "fill",
            "showlines": true
        },
        "type": "heatmap",
        mode: "lines",
        "zauto": true,
        "name": "y",
        "cauto": true
    }];



    var layout ={
            autosize:true,
            yaxis:{
            range:[
            -0.5,
            731.5
            ],
            type:"category",
            autorange:true,
            title:"",
            tickwidth:1.4000000000000001,
            domain:[
            0.03,
            1
            ],
            tickmode:"linear",
            dtick:16
            },
        paper_bgcolor:"rgb(255, 255, 255)",
        title:"Heatmap of Net kWh",
        scene:{
            aspectratio:{
                    y:2,
                    x:1,
                    z:1
            },
            camera:{
                    eye:{
                        y:1.25,
                        x:1.25,
                        z:1.25
                    },
                up:{
                        y:0,
                        x:0,
                        z:1
                },
                center:{
                        y:0,
                        x:0,
                        z:0
                }
            },
            dragmode:"turntable"
        },
        height:744,
        width:1242,
        zaxis:{
            title:"kWh"
        },
        xaxis:{
                range:[
                -0.5,
                47.5
                ],
                type:"category",
                autorange:true,
                title:"",
                tickwidth:1.4000000000000001,
                domain:[
                0.02,
                1
                ]
        },
        font:{
                size:10
        },
        margin:{
                pad:0,
                r:20,
                b:50,
                l:50,
                t:30
        }
    }
    //var layout = {
    //    "autosize": true,
    //    "xaxis": { title: "", type: "category" },
    //    "yaxis": { title: "", type: "category" },
    //    zaxis: {
    //        title: "kWh"
    //    },
    //    "scene": {
    //        "aspectratio": {
    //            "y": 2,
    //            "x": 1,
    //            "z": 1
    //        },
    //        camera: {
    //            eye: {
    //                y: 1.25,
    //                x: 1.25,
    //                z: 1.25
    //            },
    //            up: {
    //                y: 0,
    //                x: 0,
    //                z: 1
    //            },
    //            center: {
    //                y: 0,
    //                x: 0,
    //                z: 0
    //            }
    //        },
    //        "dragmode": "turntable"
    //    },

    //    "font": { "size": 10 },
    //    "margin": { "l": 50, "r": 20, "t": 20, "b": 50, "pad": 0 },
    //    "paper_bgcolor": "rgb(255, 255, 255)",
    //    title: "Heatmap of Net kWh"
    //};







    var first;
    var prevDate = "";
    var zRow = [];
    plotData[0].x = ['Midnight', '00:30am', '1:00am', '1:30am', '2:00am', '2:30am', '3:00am', '3:30am', '4:00am', '4:30am', '5:00am', '5:30am'
                    , '6:00am', '6:30am', '7:00am', '7:30am', '8:00am', '8:30am', '9:00am', '9:30am', '10:00am', '10:30am', '11:00am', '11:30am'
                    , 'Midday', '12:30pm', '1:00pm', '1:30pm', '2:00pm', '2:30pm', '3:00pm', '3:30pm', '4:00pm', '4:30pm', '5:00pm', '5:30pm'
                    , '6:00pm', '6:30pm', '7:00pm', '7:30pm', '8:00pm', '8:30pm', '9:00pm', '9:30pm' , '10:00pm', '10:30pm', '11:00pm', '11:30pm'
    ];

    Global.MeterData.Days.forEach(function (d) {

        var s = moment(d.Date).format("Do-MMM-YY");
        plotData[0].y.unshift( s ); 
        plotData[0].z.unshift(d.Get30minValue(NET));
    });
     
     

    widgetBody = $("#HeatMapContainer");




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
