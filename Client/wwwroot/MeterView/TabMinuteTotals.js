

function PlotMinuteTotalsTabClick() {
    if (Global.Tab_Detail_Setup) return;
     setTimeout(PlotMinuteTotals, 100); 
    Global.Tab_Detail_Setup = true;
}


function PlotMinuteTotals() {



    var plotData = [


    {
        type: "scatter",
            "name": "Net Consumption",

                "fill": "tozeroy",
                    "marker": { "color": "rgb(0, 67, 88)", "symbol": "hexagon-open", "size": 6, "line": { "color": "black", "width": 0.8 } },

        "line": { "color": "rgb(0, 67, 88)", "width": 1, "dash": "solid", "shape": "linear" },

        "opacity": 0.5,
            "fillcolor": "rgba(31, 119, 180, 0.71)",
                x: [],
                    y: []
    }
        
    ];



    var layout = {
        "autosize": true,
        "dragmode": "zoom",
        "yaxis": {
            "title": "kWh", "titlefont": { "size": 14, "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif" },

            "gridcolor": "rgb(238, 238, 238)",
            "showline": false,
            "type": "linear",
            "autorange": true,
            "ticks": "",
            "showgrid": true,
            "linecolor": "rgba(152, 0, 0, 0.5)",
            "mirror": false,
            "zeroline": true,
            "linewidth": 1.5,
            "tickfont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 12 },
            "tickcolor": "rgba(0, 0, 0, 0)",
            "zerolinewidth": 1,
            "ticklen": 6,
            "gridwidth": 1,
            "nticks": 0,
            "rangemode": "normal",
            "tickmode": "auto",
            "showticklabels": true,
            "tickangle": "auto",
            "zerolinecolor": "#444"
        },
        "title": "Energy Profile",
        "showlegend": false,




        "xaxis": {
            "type": "date",
            "autorange": true,
            "ticks": "",
            "showgrid": true,
            "linecolor": "rgba(152, 0, 0, 0.5)",
            "mirror": false,
            "zeroline": true,
            "showline": false,
            "linewidth": 1.5,
            "tickfont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 12 },
            "tickcolor": "rgba(0, 0, 0, 0)",
            "zerolinewidth": 1,
            "ticklen": 6,
            "gridwidth": 1,
            "gridcolor": "rgb(238, 238, 238)",
            "titlefont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 14 },
            "nticks": 0,
            "rangemode": "normal",
            "tickmode": "auto",
            "showticklabels": true,
            "tickangle": "auto",
            "zerolinecolor": "#444"
        },
        "margin": { "r": 30, "b": 40, "l": 40, "t": 50 },
        "legend": { "y": 1, "yanchor": "top", "xanchor": "center", "borderwidth": 0, "x": 0.02, "bgcolor": "white", "bordercolor": "black" },
        "paper_bgcolor": "#fff",
        "plot_bgcolor": "#fff",
        "font": { "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 12, "color": "#444" },
        "titlefont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 17 },
        "separators": ".,",
        "hidesources": false,
        "hovermode": "x"
    };


    var selectorOptions = {
        buttons: [{
            step: 'month',
            stepmode: 'backward',
            count: 1,
            label: '1m'
        }, {
            step: 'month',
            stepmode: 'backward',
            count: 6,
            label: '6m'
        }, {
            step: 'year',
            stepmode: 'todate',
            count: 1,
            label: 'YTD'
        }, {
            step: 'year',
            stepmode: 'backward',
            count: 1,
            label: '1y'
        }, {
            step: 'all',
        }],
    };



    widgetBody = $("#MinuteTotalsContainer");


    plotData[0].x = Global.MeterData.GetTimeListEST();

    plotData[0].y = Global.MeterData.GetTimeValue(NET);

   // plotData[1].x = plotData[0].x 
   //plotData[1].y = Global.MeterData.GetTimeValue(KVA);




    var h = $(window).height() - widgetBody.offset().top - 20;
    if (h < 500) h = 500;
    widgetBody.height(h);

    layout.height = widgetBody.height();
    layout.width = widgetBody.width();
    layout.xaxis.rangeselector = selectorOptions;
    layout.xaxis.rangeslider = {};

    layout.xaxis.type = "date";
    var fromIndex = plotData[0].x.length - 1 - 31 * 48;
    if (fromIndex < 0) fromIndex = 0; 
    var toIndex = plotData[0].x.length - 1;

    layout.xaxis.range = new Array(plotData[0].x[fromIndex].valueOf(), plotData[0].x[toIndex].valueOf());



    layout.xaxis.autorange = false;

    widgetBody.html('');
    Plotly.newPlot(widgetBody[0], plotData, layout);
    Plotly.Plots.resize(widgetBody[0]);

     

}
