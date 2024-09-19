

function ShowExportTabClick() {
  //  if (Global.Tab_Export_Setup) return;
   //  setTimeout(ShowExportFile, 100); 
   //  Global.Tab_Export_Setup = true;
}


function GetExportCsv() {

    



    let times = Global.MeterData.GetTimeListEST();
    let e = Global.MeterData.GetTimeValue(CONSUMPTION); 
    let b = Global.MeterData.GetTimeValue(GENERATION); 
    let n = Global.MeterData.GetTimeValue(NET); 
    let q = Global.MeterData.GetTimeValue(EXPORTKVARH); 
    let k = Global.MeterData.GetTimeValue(IMPORTKVARH); 
    let kw = Global.MeterData.GetTimeValue(KW); 
    let kva = Global.MeterData.GetTimeValue(KVA); 
    let pf = Global.MeterData.GetTimeValue(POWERFACTOR);


    var csvContent = "";  //  "data:text/csv;charset=utf-8,";


    
    csvContent += "Read datetime,Export kWh,Import kWh,Net kWh,Export kVARh,Import kVARh,kW Demand,kVA Demand,Power Factor%\n";

    for (let p = 0; p < times.length; ++p) {
        
      if ( e[p] === null) e[p] = 0;
       if ( b[p] === null) b[p] = 0;
        if ( n[p] === null) n[p] = 0; 
        


       if ( q[p] === null) q[p] = 0; 
       if ( k[p] === null) k[p] = 0; 
       if ( kw[p] === null) kw[p] = 0; 
       if ( kva[p] === null) kva[p] = 0; 
       if ( pf[p] === null) pf[p] = 0; 
       

       
        csvContent += moment(times[p]).format("YYYY-MM-DD HH:mm:ss") + ',' + e[p].toString() + ',' + b[p].toString() + ',' + n[p].toString() + ',' + q[p].toString() + ',' + k[p].toString() + ',' + kw[p].toString() + ',' + kva[p].toString() + ',' + pf[p].toString() + '\n';
    }
 
    return csvContent;
    

}


window.downloadCSV = function (args) {
    var data, filename, link;

    var csv = GetExportCsv();

    if (csv === null) return;

    filename = 'export.csv';

    //if (!csv.match(/^data:text\/csv/i)) {
    //    csv = 'data:text/csv;charset=utf-8,' + csv;
    //}
    //data = encodeURI(csv);

    var a = document.createElement("a");
    document.body.appendChild(a);
    a.style = "display: none";
    var 
        blob = new Blob([csv], { type: "text/csv;charset=utf-8" }),
        url = window.URL.createObjectURL(blob);

    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);



    //link = document.createElement('a');
    //link.setAttribute('href', data);
    //link.setAttribute('download', filename);
    //link.click();

       



};





