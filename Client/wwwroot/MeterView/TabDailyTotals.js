

function PlotDailyTotalsTabClick() {
    if (Global.Tab_Daily_Setup) return;
    PlotDailyTotals();
    Global.Tab_Daily_Setup = true;
}


function PlotDailyTotals() {




    var plotData = [
                  {

                      type: "scatter",
                      "name": "Net Consumption",

                      "fill": "tozeroy",
                      "marker": { "color": "rgb(0, 67, 88)", "symbol": "hexagon-open", "size": 6, "line": { "color": "black", "width": 0.8 } },

                      "line": { "color": "rgb(0, 67, 88)", "width": 1, "dash": "solid", "shape": "linear" },

                      "opacity": 1,
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
        "title": "Daily Energy Profile",
        "showlegend": false,

        "barmode": "overlay",
        "bargap": 0.2,

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




    widgetBody = $("#DailyTotalsContainer");




    plotData[0].x = Global.MeterData.GetDayList();
  
    plotData[0].y = Global.MeterData.GetDayValue(CONSUMPTION);


    var h = $(window).height() - widgetBody.offset().top - 20;
    if (h < 500) h = 500;
    widgetBody.height(h);

    widgetBody.html('');
    layout.height = widgetBody.height();
    layout.width = widgetBody.width();
    Plotly.newPlot(widgetBody[0], plotData, layout);

    Plotly.Plots.resize(widgetBody[0]);

}
