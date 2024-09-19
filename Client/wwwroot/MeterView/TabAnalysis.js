
function PlotAnalysisClick() {
    if (Global.Tab_Analysis_Setup) return;
    PlotAnalysis();
    Global.Tab_Analysis_Setup = true;
}


function PlotAnalysis() {


    var profiles = new Profiles(60);
    profiles.ProcessMeterData(Global.MeterData);

    var profilesOverall = profiles.Overall;
    var consumptionValues = profilesOverall.Consumption();



    $("#xModelSite").text(Global.MeterData.Site);
    $("#xAverageConsumption").text(profilesOverall.AverageConsumption());
    var overallTotal = profilesOverall.TotalConsumption();
    $("#xAverageWeekly").text(overallTotal);

    var top1 = profilesOverall.TopNConsumption(1)[0];

    $("#xAvgProfileTopkW").text(top1.Consumption())
    $("#xAvgProfileTopHour").text(top1.Hours())


    var top4 = profilesOverall.TopNConsumption(4);
    var hrs = "";
    top4.sort(function (a, b) { return a.Hour() - b.Hour(); }).forEach(function (x, i, y) {
        if (i > 0) {
            if (i === y.length - 1) { hrs += " and "; }
            else { hrs += ", "; }
        }
        hrs += x.Hours();
    })
    var top4vals = [top4[0].Consumption(), top4[1].Consumption(), top4[2].Consumption(), top4[3].Consumption()];
    var top4Avg = ss.mean(top4vals).toFixed(3);
    var top4sum = ss.sum(top4vals).toFixed(3);
    var top4pc = (top4sum / overallTotal * 100).toFixed(1);

    $("#xTopNHours").text(hrs);
    $("#xAvgProfileTopNkW").text(top4sum);
    $("#xAvgProfileTopNpc").text(top4pc);

    var base = profilesOverall.Base();

    $("#xWeekDayAvgProfileBase").text(base);
    $("#xWeekDayAvgProfileBaseTotal").text(profilesOverall.BaseTotal());
    $("#xWeekDayAvgProfileBasePC").text(profilesOverall.BasePc());

    var night = profilesOverall.GetBlock([17, 18, 19, 20, 21, 22, 23]);
    night.Range = "5pm to midnight";
    night.Name = "Night";
    night.Label = "Night";


    var Day = profilesOverall.GetBlock([9, 10, 11, 12, 13, 14, 15, 16]);
    Day.Range = " 9am to 5pm";
    Day.Name = "Day";
    Day.Label = "Day";

    var Morning = profilesOverall.GetBlock([5, 6, 7, 8]);
    Morning.Range = "5am to 9pm";
    Morning.Name = "Morning";
    Morning.Label = "Morning";



    var ONight = profilesOverall.GetBlock([0, 1, 2, 3, 4]);
    ONight.Range = "Midnight to 5am";
    ONight.Name = "ONight";
    ONight.Label = "Overnight";

    var blocks = [night, Day, Morning, ONight];

    blocks.forEach(function (x) {
        $("#xBlock_" + x.Name + "_TimeRange").text(x.Range);
        $("#xBlock_" + x.Name + "_kW").text(x.AvgEnergykW);
        $("#xBlock_" + x.Name + "_kWh").text(x.TotEnergyKWh);
        $("#xBlock_" + x.Name + "_PC").text(x.PercentageTotal);
    });


    var baseblock = {
        Range: "Midnight to Mignight"
        , Name: "Base"
        , Label: "Base"
        , AvgEnergykW: base
        , TotEnergyKWh: profilesOverall.BaseTotal()
        , PercentageTotal: profilesOverall.BasePc()
    };










    var plotData = [{
        "opacity": 0.7, "autobinx": true, "autobiny": true, "ysrc": "ewenstewart:29:f4a1fa", "xsrc": "ewenstewart:29:5d2353"
        , "name": "Consumption", "marker": {
            "symbol": "hexagon-open", "maxdisplayed": 33, "size": 6,
            "color": "rgb(255, 0, 0)"
        }, "mode": "lines+markers",
        "line": { "dash": "solid", "color": "rgb(255, 0, 0)", "shape": "spline", "width": 1 }, "fill": "none",
        "type": "scatter", "uid": "04493a", "error_x": { "copy_ystyle": true },
        "error_y": { "color": "rgb(0, 67, 88)", "width": 1, "thickness": 1 }
    }, {
        "opacity": 0.7, "error_x": { "color": "black", "width": "2", "thickness": "1", "copy_ystyle": true },
        "error_y": { "color": "rgb(31, 138, 112)", "width": 1, "thickness": 1 }, "fill": "none", "ysrc": "ewenstewart:29:b205fe",
        "xsrc": "ewenstewart:29:5d2353", "name": "Generation", "marker": {
            "color": "rgb(31, 138, 112)",
            "line": { "color": "white", "width": 6 }, "symbol": "hexagon-open", "size": 6
        }, "mode": "lines", "line": { "dash": "dot", "color": "rgb(31, 138, 112)", "shape": "spline", "width": 1 }, "autobiny": true, "type": "scatter", "autobinx": true, "uid": "795eef"
    }, {
        "opacity": 0.49, "autobinx": true, "uid": "5eea14", "name": "Base usage",
        "ysrc": "ewenstewart:29:a53d86", "xsrc": "ewenstewart:29:5d2353", "mode": "lines", "fillcolor": "rgba(31, 119, 180, 0.38)", "line": { "dash": "solid", "color": "rgb(31, 119, 180)", "shape": "linear", "width": 2 }, "fill": "tozeroy", "type": "bar", "autobiny": true, "marker": { "color": "rgb(44, 160, 44)" }, "error_x": { "copy_ystyle": true }, "error_y": { "color": "rgb(0, 67, 88)", "width": 1, "thickness": 1 }
    }, {
        "opacity": 0.49, "autobinx": true, "uid": "2f311e", "name": "Night usage",
        "ysrc": "ewenstewart:29:ce54c2", "xsrc": "ewenstewart:29:5d2353", "mode": "lines", "fillcolor": "rgba(31, 119, 180, 0.38)", "line": { "dash": "solid", "color": "rgb(31, 119, 180)", "shape": "linear", "width": 2 }, "fill": "tozeroy", "type": "bar", "autobiny": true, "marker": { "color": "rgb(214, 39, 40)" }, "error_x": { "copy_ystyle": true }, "error_y": { "color": "rgb(31, 138, 112)", "width": 1, "thickness": 1 }
    }, {
        "opacity": 0.49, "autobinx": true, "uid": "86e7ee", "name": "Overnight usage",
        "ysrc": "ewenstewart:29:b113d3", "xsrc": "ewenstewart:29:5d2353", "mode": "lines", "fillcolor": "rgba(31, 119, 180, 0.38)", "line": { "dash": "solid", "color": "rgb(31, 119, 180)", "shape": "linear", "width": 2 }, "fill": "tozeroy", "type": "bar", "autobiny": true, "marker": { "color": "rgb(148, 103, 189)" }, "error_x": { "copy_ystyle": true }, "error_y": { "color": "rgb(190, 219, 57)", "width": 1, "thickness": 1 }
    }, {
        "opacity": 0.49, "autobinx": true, "uid": "7d5111", "name": "Morning usage",
        "ysrc": "ewenstewart:29:8927ae", "xsrc": "ewenstewart:29:5d2353", "mode": "lines", "fillcolor": "rgba(31, 119, 180, 0.38)", "line": { "dash": "solid", "color": "rgb(31, 119, 180)", "shape": "linear", "width": 2 }, "fill": "tozeroy", "type": "bar", "autobiny": true, "marker": { "color": "rgb(23, 190, 207)" }, "error_x": { "copy_ystyle": true }, "error_y": { "color": "rgb(255, 225, 26)", "width": 1, "thickness": 1 }
    }, {
        "opacity": 0.49, "autobinx": true, "uid": "dbb65c", "name": "Day usage",
        "ysrc": "ewenstewart:29:f73f0b", "xsrc": "ewenstewart:29:5d2353", "mode": "lines", "fillcolor": "rgba(31, 119, 180, 0.38)", "line": { "dash": "solid", "color": "rgb(31, 119, 180)", "shape": "linear", "width": 2 }, "fill": "tozeroy", "type": "bar", "autobiny": true, "marker": { "color": "rgb(255, 127, 14)" }, "error_x": { "copy_ystyle": true }, "error_y": { "color": "rgb(253, 116, 0)", "width": 1, "thickness": 1 }
    }];
    var layout = {
        "autosize": true, "font": { "color": "#444", "family": "\"Droid Sans\", sans-serif", "size": 12 }, "yaxis": {
            "showexponent": "all", "showticklabels": true,
            "titlefont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 14 }, "linecolor": "rgba(152, 0, 0, 0.5)", "mirror": false, "nticks": 0, "linewidth": 1.5, "autorange": true, "tickmode": "auto", "title": "kW", "ticks": "", "rangemode": "normal", "zeroline": true, "type": "linear", "zerolinewidth": 1, "ticklen": 6, "tickcolor": "rgba(0, 0, 0, 0)", "showline": false, "showgrid": true, "tickfont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 12 }, "tickangle": "auto", "gridwidth": 1, "zerolinecolor": "#444", "range": [-1.6310104936581804, 1.6691993795054294], "gridcolor": "rgb(238, 238, 238)", "exponentformat": "B"
        }, "paper_bgcolor": "#fff", "barnorm": "", "plot_bgcolor": "#fff", "dragmode": "zoom", "showlegend": true,
        "separators": ".,", "height": 743, "width": 1242, "legend": {
            "bordercolor": "#444", "yanchor": "auto",
            "traceorder": "normal", "xanchor": "auto", "bgcolor": "#fff", "borderwidth": 0, "y": 1.1, "x": 0.04,
            "font": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 12 }
        }, "bargap": 0.04, "titlefont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 17 },
        "bargroupgap": 0.03, "xaxis": {
            "showticklabels": true, "titlefont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 14 },
            "linecolor": "rgba(152, 0, 0, 0.5)", "mirror": false, "nticks": 0, "linewidth": 1.5, "autorange": true, "tickmode": "auto", "ticks": "", "rangemode": "normal", "zeroline": true, "type": "category", "zerolinewidth": 1, "ticklen": 6, "tickcolor": "rgba(0, 0, 0, 0)", "showline": false, "showgrid": true, "tickfont": { "color": "#444", "family": "\"Open Sans\", verdana, arial, sans-serif", "size": 12 }, "tickangle": "auto", "gridwidth": 1, "zerolinecolor": "#444", "range": [-1.36850326611758, 24.368503266117578], "gridcolor": "rgb(238, 238, 238)"
        }, "title": "Average Consumption", "hovermode": "closest", "barmode": "stack", "margin": { "r": 20, "b": 80, "l": 40, "t": 50 }, "hidesources": false
    };

    //plot the brs





    var xVals = ['Midnight', '1:00am', '2:00am', '3:00am', '4:00am', '5:00am'
            , '6:00am', '7:00am', '8:00am', '9:00am', '10:00am', '11:00am'
            , 'Midday', '1:00pm', '2:00pm', '3:00pm', '4:00pm', '5:00pm'
            , '6:00pm', '7:00pm', '8:00pm', '9:00pm', '10:00pm', '11:00pm'
    ];


    var yConsumption = profilesOverall.Consumption();

    var yGen = profilesOverall.Generation();;

    var yBase = [];
    for (h = 0 ; h < 24 ; h++) { yBase.push(base); }


    var yNight = night.Values;
    var yOverNight = ONight.Values;
    var yMorning = Morning.Values;
    var yDay = Day.Values;




    plotData[0].x = xVals;
    plotData[0].y = yConsumption;
    plotData[1].x = xVals;
    plotData[1].y = yGen;
    plotData[2].x = xVals;
    plotData[2].y = yBase;

    plotData[3].x = xVals;
    plotData[3].y = yNight;



    plotData[4].x = xVals;
    plotData[4].y = yOverNight;


    plotData[5].x = xVals;
    plotData[5].y = yMorning;


    plotData[6].x = xVals;
    plotData[6].y = yDay;






    widgetBody = $("#graphTouWeekDay");
    widgetBody.html('');
    layout.height = widgetBody.height();
    layout.width = widgetBody.width();
    Plotly.newPlot(widgetBody[0], plotData, layout);
    blocks.push(baseblock);

    // copy the colors 
    // var blocks = [night, Day, Morning, ONight];
    baseblock.Colour = plotData[2].marker.color;
    night.Colour = plotData[3].marker.color;
    Day.Colour = plotData[6].marker.color;
    Morning.Colour = plotData[5].marker.color;
    ONight.Colour = plotData[4].marker.color;


    plotPie(blocks)
    doBillingSim();

}

function plotPie(blocks) {

    var widgetBody = $("#PieChart");

    var sortedBlocks = blocks.slice().sort(function (a, b) { return b.PercentageTotal - a.PercentageTotal });

 


 
 
    var plotData = [{ name:"" , textposition: "auto", "showlegend": true, "uid": "24fe82", "labels": ["Base", "Day", "Night", "Morning", "Overnight"], "domain": { "y": [0], "x": [0, 0.97] }, "values": ["9.408", "6.911", "5.692", "1.731", "0.043"], "hoverinfo": "all", "marker": { "colors": ["rgb(44, 160, 44)", "rgb(255, 127, 14)", "rgb(214, 39, 40)", "rgb(23, 190, 207)", "rgb(148, 103, 189)"], "line": { "width": 0 } }, "textinfo": "label+percent", "hole": 0, "type": "pie", "opacity": 1, "textfont": { "color": "rgb(217, 217, 217)" }, "outsidetextfont": { "color": "rgb(0, 0, 0)" } }];
    var layout = { "autosize": false, "title": "Energy breakdown by zone", "height": 400, "width": 400, "margin": { "autoexpand": true, "r": 40, "b": 40, "t": 40, "l": 40 }, "showlegend": false };
 



    sortedBlocks.forEach(function (x ,i ) {
        plotData[0].values[i] = x.TotEnergyKWh; 
        plotData[0].labels[i] = x.Label; 
        plotData[0].marker.colors[i] = x.Colour;
    });



    // layout.height = widgetBody.height();
    //  layout.width = widgetBody.width();
    Plotly.newPlot(widgetBody[0], plotData, layout);


}



function doBillingSim() {

    // simple hard coded billingsimulaotr 


    var data = Global.MeterData;
    var days = data.Days.length;
    var proRata = 365 / days;   // convert to year values 

    // define the products 
    var prods = 
        [
        {       Option: 1 ,
            Name : 'Flat',
            Components: [


                    { Name: "Daily",
                        Calc: function (total, md) { return total + 1 ; },
                        Rate: 73.7000,
                        RateFactor: 0.01,
                        Unit:'days'
                    }

                    ,

                    { Name: "PEAK",
                    Calc: function (total, md) { return total + ss.sum(md.E); },
                    Rate: 17.6880,
                    RateFactor: 0.01,
                    Unit:'kWh'
                    }
            ]
        }
,
        {       Option: 2 ,
        Name : 'TOD',
        Components: [


                { Name: "Daily",
                    Calc: function (total, md) { return total + 1 ; },
                    Rate: 81.07,
                    RateFactor: 0.01,
                    Unit:'days'
                }

                ,

                { Name: "PEAK",
                Calc: function (total, md) {

                    // mon-fri 
                    if (md.Date.getDay() === 0 || md.Date.getDay() === 6) return total;



                    // get the Hour 
                    var result = total;
                    for (var p = 0 ; p < md.Periods() ; ++p) {
                        // get the hour 
                        h = Math.floor(p * md.Interval / 60);
                        // do simple DLS 
                        if (md.Date.getMonth() < 4 || md.Date.getMonth() >= 10) {
                            h = h + 1;
                            if (h > 23) h = 0;
                        }
                        if (h>=15 && h< 23) // 3pm-11pm mon-fri 
                        {
                              result += md.E[p];
                        }
                        
                    }
                    return result; 
                                    },
                    Rate: 25.06,
                    RateFactor: 0.01,
                    Unit:'kWh'
                }

                ,
                     {
                         Name: "SHOULDER",
                         Calc: function (total, md) {

                             // sat / sun 
                             //if (md.Date.getDay() === 0 || md.Date.getDay() === 6) return total



                             // get the Hour 
                             var result = total;
                             for (var p = 0 ; p < md.Periods() ; ++p) {
                                 // get the hour 
                                 h = Math.floor(p * md.Interval / 60);
                                 // do simple DLS 
                                 if (md.Date.getMonth() < 4 || md.Date.getMonth() >= 10) {
                                     h = h + 1;
                                     if (h > 23) h = 0;
                                 }


                                 if (md.Date.getDay() === 0 || md.Date.getDay() === 6) {
                                     //weekend 
                                     if (h >= 7 && h <= 22) {
                                         result += md.E[p];
                                     }
                                 }
                                 else {
                                     //weekday  
                                     if (h >= 7 && h < 15) {
                                         result += md.E[p];
                                     }
                                 }





                                 //if (h >= 7 && h < 15) {    //7am to 3pm mon-fri 
                                 //      result += md.E[p];
                                 //}






                             }
                             return result;
                         },
                         Rate: 15.11,
                         RateFactor: 0.01,
                         Unit: 'kWh'
                     }

                         ,
                     {
                         Name: "OFFPEAK",
                         Calc: function (total, md) {

                             // sat / sun 
                             //if (md.Date.getDay() === 0 || md.Date.getDay() === 6) return total + ss.sum(md.E);  // all day sat / sun 



                             // get the Hour 
                             var result = total;
                             for (var p = 0 ; p < md.Periods() ; ++p) {
                                 // get the hour 
                                 h = Math.floor(p * md.Interval / 60);
                                 // do simple DLS 
                                 if (md.Date.getMonth() < 4 || md.Date.getMonth() >= 10) {
                                     h = h + 1;
                                     if (h > 23) h = 0;
                                 }
                                 if (h < 7 || h > 22) {
                                       result += md.E[p];
                                 }

                             }
                             return result;
                         },
                         Rate: 10.17,
                         RateFactor: 0.01,
                         Unit: 'kWh'
                     }




    ]
    }
    
    ,
        {
            Option: 3,
            Name: 'TOD FLEX',
            Components: [


                    {
                        Name: "Daily",
                        Calc: function (total, md) { return total + 1; },
                        Rate: 81.07,
                        RateFactor: 0.01,
                        Unit: 'days'
                    }

                    ,

                    {
                        Name: "PEAK",
                        Calc: function (total, md) {

                            // mon-fri 
                            if (md.Date.getDay() === 0 || md.Date.getDay() === 6) return total;



                            // get the Hour 
                            var result = total;
                            for (var p = 0 ; p < md.Periods() ; ++p) {
                                // get the hour 
                                h = Math.floor(p * md.Interval / 60);
                                // do simple DLS 
                                if (md.Date.getMonth() < 4 || md.Date.getMonth() >= 10) {
                                    h = h + 1;
                                    if (h > 23) h = 0;
                                }
                                if (h >= 15 && h < 22) {
                                    result += md.E[p];
                                }

                            }
                            return result;
                        },
                        Rate: 27.27,
                        RateFactor: 0.01,
                        Unit: 'kWh'
                    }

                    ,
                         {
                             Name: "SHOULDER",
                             Calc: function (total, md) {
 


                                 // get the Hour 
                                 var result = total;
                                 for (var p = 0 ; p < md.Periods() ; ++p) {
                                     // get the hour 
                                     h = Math.floor(p * md.Interval / 60);
                                     // do simple DLS 
                                     if (md.Date.getMonth() < 4 || md.Date.getMonth() >= 10) {
                                         h = h + 1;
                                         if (h > 23) h = 0;
                                     }

                                     if (md.Date.getDay() === 0 || md.Date.getDay() === 6) {
                                         //weekend 
                                         if (h >= 7 && h < 22) {
                                             result += md.E[p];
                                         }
                                     }
                                     else 
                                     {
                                         //weekday  
                                         if (h >= 7 && h < 15) {
                                             result += md.E[p];
                                         }
                                     }


                                 }
                                 return result;
                             },
                             Rate: 15.48,
                             RateFactor: 0.01,
                             Unit: 'kWh'
                         }

                             ,
                         {
                             Name: "OFFPEAK",
                             Calc: function (total, md) {

                                 


                                 // get the Hour 
                                 var result = total;
                                 for (var p = 0 ; p < md.Periods() ; ++p) {
                                     // get the hour 
                                     h = Math.floor(p * md.Interval / 60);
                                     // do simple DLS 
                                     if (md.Date.getMonth() < 4 || md.Date.getMonth() >= 10) {
                                         h = h + 1;
                                         if (h > 23) h = 0;
                                     }
                                     if (h < 7 || h >= 22) {
                                         result += md.E[p];
                                     }

                                 }
                                 return result;
                             },
                             Rate: 11.64,
                             RateFactor: 0.01,
                             Unit: 'kWh'
                         }




            ]
        }



        ];



    // do the calc 
    prods.forEach(function (p) {
        p.TotalDollars = 0;
        p.Components.forEach( function(c) {
            // calc the component
            c.Total = data.Days.reduce(c.Calc, 0).roundNumber(3);
            c.ProRataTotal = (c.Total * proRata).roundNumber(3);;
            c.Dollars = (c.ProRataTotal * c.Rate * c.RateFactor).roundNumber(2);
            p.TotalDollars += c.Dollars;
        });

    });

    // results
    $($('#CalcDaily td')[1]).text(prods[0].Components[0].ProRataTotal + ' ' + prods[0].Components[0].Unit);
    $($('#CalcDaily td')[2]).text('$' + prods[0].Components[0].Dollars.formatMoney(2));
    $($('#CalcPeak td')[1]).text(prods[0].Components[1].ProRataTotal + ' ' + prods[0].Components[1].Unit);
    $($('#CalcPeak td')[2]).text('$' + prods[0].Components[1].Dollars.formatMoney(2));
    $($('#CalcShoulder td')[1]).text("");
    $($('#CalcShoulder td')[2]).text("");
    $($('#CalcOffpeak td')[1]).text("");
    $($('#CalcOffpeak td')[2]).text("");
     $($('#CalcTotal td')[0]).text('$' + prods[0].TotalDollars.formatMoney(2));




    
    $($('#CalcDaily td')[3]).text(prods[1].Components[0].ProRataTotal + ' ' + prods[1].Components[0].Unit);
    $($('#CalcDaily td')[4]).text('$' + prods[1].Components[0].Dollars.formatMoney(2));
    $($('#CalcPeak td')[3]).text(prods[1].Components[1].ProRataTotal + ' ' + prods[1].Components[1].Unit);
    $($('#CalcPeak td')[4]).text('$' + prods[1].Components[1].Dollars.formatMoney(2));
    $($('#CalcShoulder td')[3]).text(prods[1].Components[2].ProRataTotal + ' ' + prods[1].Components[2].Unit);
    $($('#CalcShoulder td')[4]).text('$' + prods[1].Components[2].Dollars.formatMoney(2));
    $($('#CalcOffpeak td')[3]).text(prods[1].Components[3].ProRataTotal + ' ' + prods[1].Components[3].Unit);
    $($('#CalcOffpeak td')[4]).text('$' + prods[1].Components[3].Dollars.formatMoney(2));
    $($('#CalcTotal td')[1]).text('$' + prods[1].TotalDollars.formatMoney(2));


    $($('#CalcDaily td')[5]).text(prods[2].Components[0].ProRataTotal + ' ' + prods[1].Components[0].Unit);
    $($('#CalcDaily td')[6]).text('$' + prods[2].Components[0].Dollars.formatMoney(2));
    $($('#CalcPeak td')[5]).text(prods[2].Components[1].ProRataTotal + ' ' + prods[1].Components[1].Unit);
    $($('#CalcPeak td')[6]).text('$' + prods[2].Components[1].Dollars.formatMoney(2));
    $($('#CalcShoulder td')[5]).text(prods[2].Components[2].ProRataTotal + ' ' + prods[1].Components[2].Unit);
    $($('#CalcShoulder td')[6]).text('$' + prods[2].Components[2].Dollars.formatMoney(2));
    $($('#CalcOffpeak td')[5]).text(prods[2].Components[3].ProRataTotal + ' ' + prods[1].Components[3].Unit);
    $($('#CalcOffpeak td')[6]).text('$' + prods[2].Components[3].Dollars.formatMoney(2));
    $($('#CalcTotal td')[2]).text('$' + prods[2].TotalDollars.formatMoney(2));




}





