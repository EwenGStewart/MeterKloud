

function PlotProfileTabClick() {
     if (Global.Tab_Profile_Setup) return;
    PlotProfile();
    Global.Tab_Profile_Setup = true;
}


function PlotProfile() {

    
    var plotData = [{ "opacity": 0.5, "name": "Summer", "uid": "1ee6fa", "mode": "lines", "y": [0.204, 0.187, 0.199, 0.207, 0.214, 0.228, 0.228, 0.219, 0.227, 0.214, 0.216, 0.238, 0.279, 0.367, 0.447, 0.693, 0.674, 0.803, 0.943, 0.95, 0.938, 0.938, 0.919, 0.672, 0.65, 0.66, 0.682, 0.533, 0.326, 0.33, 0.395, 0.479, 0.443, 0.529, 0.56, 0.606, 0.575, 0.506, 0.563, 0.57, 0.593, 0.686, 0.618, 0.46, 0.37, 0.313, 0.26, 0.237], "x": ["Midnight", "00:30am", "1:00am", "1:30am", "2:00am", "2:30am", "3:00am", "3:30am", "4:00am", "4:30am", "5:00am", "5:30am", "6:00am", "6:30am", "7:00am", "7:30am", "8:00am", "8:30am", "9:00am", "9:30am", "10:00am", "10:30am", "11:00am", "11:30am", "Midday", "12:30pm", "1:00pm", "1:30pm", "2:00pm", "2:30pm", "3:00pm", "3:30pm", "4:00pm", "4:30pm", "5:00pm", "5:30pm", "6:00pm", "6:30pm", "7:00pm", "7:30pm", "8:00pm", "8:30pm", "9:00pm", "9:30pm", "10:00pm", "10:30pm", "11:00pm", "11:30pm"], "line": { "color": "rgb(255, 0, 0)", "width": 2, "dash": "solid", "shape": "spline" }, "type": "scatter", "fill": "none" }, { "opacity": 0.5, "name": "Winter", "uid": "ac5c41", "mode": "lines", "y": [0.222, 0.196, 0.176, 0.182, 0.202, 0.173, 0.191, 0.169, 0.17, 0.168, 0.202, 0.231, 0.326, 0.237, 0.254, 0.32, 0.483, 0.514, 0.479, 0.736, 0.803, 0.798, 0.822, 0.76, 0.758, 0.726, 0.59, 0.379, 0.311, 0.333, 0.374, 0.345, 0.407, 0.434, 0.539, 0.534, 0.673, 0.712, 0.694, 0.68, 0.757, 0.811, 0.791, 0.698, 0.53, 0.435, 0.35, 0.28], "x": ["Midnight", "00:30am", "1:00am", "1:30am", "2:00am", "2:30am", "3:00am", "3:30am", "4:00am", "4:30am", "5:00am", "5:30am", "6:00am", "6:30am", "7:00am", "7:30am", "8:00am", "8:30am", "9:00am", "9:30am", "10:00am", "10:30am", "11:00am", "11:30am", "Midday", "12:30pm", "1:00pm", "1:30pm", "2:00pm", "2:30pm", "3:00pm", "3:30pm", "4:00pm", "4:30pm", "5:00pm", "5:30pm", "6:00pm", "6:30pm", "7:00pm", "7:30pm", "8:00pm", "8:30pm", "9:00pm", "9:30pm", "10:00pm", "10:30pm", "11:00pm", "11:30pm"], "line": { "color": "rgb(31, 119, 180)", "width": 2, "dash": "solid", "shape": "spline" }, "type": "scatter", "fill": "none" }, { "opacity": 0.5, "name": "Spring", "uid": "8c02e9", "mode": "lines", "y": [0.215, 0.194, 0.179, 0.166, 0.159, 0.169, 0.177, 0.178, 0.192, 0.209, 0.208, 0.224, 0.227, 0.239, 0.331, 0.391, 0.565, 0.823, 0.86, 0.955, 1.125, 1.108, 1.081, 1.003, 0.847, 0.873, 0.774, 0.557, 0.489, 0.513, 0.583, 0.582, 0.546, 0.561, 0.568, 0.654, 0.701, 0.564, 0.59, 0.58, 0.626, 0.627, 0.586, 0.523, 0.424, 0.364, 0.282, 0.231], "x": ["Midnight", "00:30am", "1:00am", "1:30am", "2:00am", "2:30am", "3:00am", "3:30am", "4:00am", "4:30am", "5:00am", "5:30am", "6:00am", "6:30am", "7:00am", "7:30am", "8:00am", "8:30am", "9:00am", "9:30am", "10:00am", "10:30am", "11:00am", "11:30am", "Midday", "12:30pm", "1:00pm", "1:30pm", "2:00pm", "2:30pm", "3:00pm", "3:30pm", "4:00pm", "4:30pm", "5:00pm", "5:30pm", "6:00pm", "6:30pm", "7:00pm", "7:30pm", "8:00pm", "8:30pm", "9:00pm", "9:30pm", "10:00pm", "10:30pm", "11:00pm", "11:30pm"], "line": { "color": "rgb(44, 160, 44)", "width": 2, "dash": "solid", "shape": "spline" }, "type": "scatter", "fill": "none" }, { "opacity": 0.5, "name": "Autumn", "uid": "63b19d", "mode": "lines", "y": [0.313, 0.236, 0.222, 0.179, 0.211, 0.217, 0.197, 0.208, 0.18, 0.192, 0.223, 0.252, 0.301, 0.288, 0.314, 0.469, 0.686, 0.698, 0.938, 1.015, 1.047, 1.13, 1.09, 0.925, 0.748, 0.807, 0.716, 0.586, 0.455, 0.481, 0.513, 0.452, 0.554, 0.603, 0.805, 0.863, 1.003, 0.973, 0.971, 0.945, 0.907, 0.913, 0.923, 0.854, 0.703, 0.651, 0.421, 0.385], "x": ["Midnight", "00:30am", "1:00am", "1:30am", "2:00am", "2:30am", "3:00am", "3:30am", "4:00am", "4:30am", "5:00am", "5:30am", "6:00am", "6:30am", "7:00am", "7:30am", "8:00am", "8:30am", "9:00am", "9:30am", "10:00am", "10:30am", "11:00am", "11:30am", "Midday", "12:30pm", "1:00pm", "1:30pm", "2:00pm", "2:30pm", "3:00pm", "3:30pm", "4:00pm", "4:30pm", "5:00pm", "5:30pm", "6:00pm", "6:30pm", "7:00pm", "7:30pm", "8:00pm", "8:30pm", "9:00pm", "9:30pm", "10:00pm", "10:30pm", "11:00pm", "11:30pm"], "line": { "color": "rgb(255, 127, 14)", "width": 2, "dash": "solid", "shape": "spline" }, "type": "scatter", "fill": "none" }, { "opacity": 0.5, "name": "Weekday", "uid": "fd167b", "mode": "lines", "y": [0.223, 0.21, 0.184, 0.182, 0.191, 0.203, 0.21, 0.204, 0.21, 0.202, 0.251, 0.246, 0.268, 0.342, 0.404, 0.553, 0.644, 0.679, 0.723, 0.833, 0.891, 0.893, 0.869, 0.784, 0.669, 0.658, 0.594, 0.413, 0.328, 0.325, 0.349, 0.379, 0.4, 0.464, 0.542, 0.618, 0.675, 0.705, 0.736, 0.756, 0.748, 0.754, 0.719, 0.628, 0.542, 0.428, 0.326, 0.266], "x": ["Midnight", "00:30am", "1:00am", "1:30am", "2:00am", "2:30am", "3:00am", "3:30am", "4:00am", "4:30am", "5:00am", "5:30am", "6:00am", "6:30am", "7:00am", "7:30am", "8:00am", "8:30am", "9:00am", "9:30am", "10:00am", "10:30am", "11:00am", "11:30am", "Midday", "12:30pm", "1:00pm", "1:30pm", "2:00pm", "2:30pm", "3:00pm", "3:30pm", "4:00pm", "4:30pm", "5:00pm", "5:30pm", "6:00pm", "6:30pm", "7:00pm", "7:30pm", "8:00pm", "8:30pm", "9:00pm", "9:30pm", "10:00pm", "10:30pm", "11:00pm", "11:30pm"], "line": { "color": "rgb(0, 0, 0)", "width": 4, "dash": "dot", "shape": "spline" }, "type": "scatter", "fill": "none" }, { "opacity": 0.5, "name": "Weekend", "uid": "668816", "mode": "lines", "y": [0.242, 0.204, 0.193, 0.178, 0.188, 0.194, 0.192, 0.187, 0.188, 0.197, 0.214, 0.23, 0.271, 0.251, 0.317, 0.461, 0.606, 0.728, 0.819, 0.944, 1.002, 1.005, 0.988, 0.853, 0.769, 0.783, 0.699, 0.523, 0.427, 0.446, 0.488, 0.483, 0.499, 0.519, 0.612, 0.651, 0.72, 0.676, 0.684, 0.685, 0.711, 0.749, 0.713, 0.635, 0.51, 0.447, 0.332, 0.288], "x": ["Midnight", "00:30am", "1:00am", "1:30am", "2:00am", "2:30am", "3:00am", "3:30am", "4:00am", "4:30am", "5:00am", "5:30am", "6:00am", "6:30am", "7:00am", "7:30am", "8:00am", "8:30am", "9:00am", "9:30am", "10:00am", "10:30am", "11:00am", "11:30am", "Midday", "12:30pm", "1:00pm", "1:30pm", "2:00pm", "2:30pm", "3:00pm", "3:30pm", "4:00pm", "4:30pm", "5:00pm", "5:30pm", "6:00pm", "6:30pm", "7:00pm", "7:30pm", "8:00pm", "8:30pm", "9:00pm", "9:30pm", "10:00pm", "10:30pm", "11:00pm", "11:30pm"], "line": { "color": "rgb(0, 0, 255)", "width": 4, "dash": "dashdot", "shape": "linear" }, "type": "scatter", "fill": "none" }];
    var layout = { "hidesources": false, "autosize": true, "yaxis": { "showexponent": "all", "showticklabels": true, "titlefont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 14 }, "linecolor": "rgba(152, 0, 0, 0.5)", "mirror": false, "nticks": 0, "rangemode": "normal", "autorange": true, "linewidth": 1.5, "tickmode": "auto", "title": "kWh Avg", "ticks": "", "showgrid": true, "zeroline": true, "type": "linear", "zerolinewidth": 1, "ticklen": 6, "tickcolor": "rgba(0, 0, 0, 0)", "showline": true, "tickfont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 12 }, "tickangle": "auto", "gridwidth": 1, "zerolinecolor": "#444", "range": [0.10505555555555557, 1.1839444444444442], "gridcolor": "rgb(238, 238, 238)", "exponentformat": "B" }, "paper_bgcolor": "#fff", "plot_bgcolor": "#fff", "dragmode": "zoom", "showlegend": true, "separators": ".,", "height": 816, "width": 1620, "bargap": 0, "titlefont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 17 }, "hovermode": "x", "xaxis": { "showticklabels": true, "titlefont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 14 }, "linecolor": "rgba(152, 0, 0, 0.5)", "mirror": false, "nticks": 0, "rangemode": "normal", "autorange": true, "linewidth": 1.5, "tickmode": "auto", "ticks": "", "showgrid": true, "zeroline": true, "type": "category", "zerolinewidth": 1, "ticklen": 6, "tickcolor": "rgba(0, 0, 0, 0)", "showline": true, "tickfont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 12 }, "tickangle": "auto", "gridwidth": 1, "zerolinecolor": "#444", "range": [0, 47], "gridcolor": "rgb(238, 238, 238)", "title": "Time - EST - No Daylight savings" }, "title": "Average Daily Consumption profile", "barmode": "overlay", "font": { "color": "#444", "family": "\"Droid Sans\", sans-serif", "size": 12 }, "margin": { "r": 20, "b": 80, "l": 40, "t": 50 }, "legend": { "bordercolor": "#444", "traceorder": "normal", "xanchor": "center", "bgcolor": "#fff", "borderwidth": 0, "y": 1, "x": 0.1, "font": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 12 } } };
    

    widgetBody = $("#ProfileContainer");


    var profiles = new Profiles(30);
    profiles.ProcessMeterData(Global.MeterData);

    //var xdata =   ['Midnight',   '1:00am',   '2:00am',   '3:00am',   '4:00am',   '5:00am' 
    //                , '6:00am',  '7:00am',   '8:00am',   '9:00am',   '10:00am',  '11:00am'  
    //                , 'Midday',   '1:00pm',   '2:00pm',  '3:00pm',   '4:00pm',   '5:00pm' 
    //                , '6:00pm',   '7:00pm',   '8:00pm',   '9:00pm',   '10:00pm',  '11:00pm' 
    //];


    var xdata = ['Midnight', '00:30am', '1:00am', '1:30am', '2:00am', '2:30am', '3:00am', '3:30am', '4:00am', '4:30am', '5:00am', '5:30am'
                    , '6:00am', '6:30am', '7:00am', '7:30am', '8:00am', '8:30am', '9:00am', '9:30am', '10:00am', '10:30am', '11:00am', '11:30am'
                    , 'Midday', '12:30pm', '1:00pm', '1:30pm', '2:00pm', '2:30pm', '3:00pm', '3:30pm', '4:00pm', '4:30pm', '5:00pm', '5:30pm'
                    , '6:00pm', '6:30pm', '7:00pm', '7:30pm', '8:00pm', '8:30pm', '9:00pm', '9:30pm', '10:00pm', '10:30pm', '11:00pm', '11:30pm'
    ];





    plotData[0].x = xdata;
    plotData[0].y = profiles.Summer.Net();

    plotData[1].x = xdata;
    plotData[1].y = profiles.Winter.Net();

    plotData[2].x = xdata;
    plotData[2].y = profiles.Spring.Net();

    plotData[3].x = xdata;
    plotData[3].y = profiles.Autumn.Net();

    plotData[4].x = xdata;
    plotData[4].y = profiles.WeekDay.Net();


    plotData[5].x = xdata;
    plotData[5].y = profiles.WeekEnd.Net();



 //   plotData[0].x = Global.MeterData.GetDayList();

 //   plotData[0].y = Global.MeterData.GetDayValue(CONSUMPTION);


    var h = $(window).height() - widgetBody.offset().top - 20;
    if (h < 500) h = 500;
    widgetBody.height(h);

    widgetBody.html('');
    layout.height = widgetBody.height();
    layout.width = widgetBody.width();
    Plotly.newPlot(widgetBody[0], plotData, layout);

    Plotly.Plots.resize(widgetBody[0]);

}
