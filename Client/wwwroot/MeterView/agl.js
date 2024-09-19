

function FileProcessAGL(csv) {


    var result = new MeterSiteData();

    var _nmi = "";


    var _values = [];
    var _values_B = [];


    csv.forEach(processAGLRow);

    if (result.Days.length > 0 && result.Days[0].E.length > 0) { result.Valid = true; }
    return result;





    function processAGLRow(row, idx, rows) {

        if (row === null || row.length === 0) return;

        if (idx === 0) return;  // ignore first line 
        var channel = "";

        _nmi = row.GetColUpper(1);
        channel = row.GetColUpper(4).slice(-2).slice(0, 1);

        var readdate = row.GetColDate(6, "D/MM/YYYY h:mm:ss A");

        var period = (readdate.getHours() * 60 + readdate.getMinutes()) / 30
        if (period < 0) period = 0;
        if (period >= 48) period = 47;

        var readdateOnly = new Date( readdate.setHours(0,0,0));


        if (channel === "E") {
            _values[period] = row.GetColFloat(8);
            if (period === 47) {
                
                result.AddDay(_nmi, channel, readdateOnly, 30, _values);
                _values = [];
            }
        }
        else if (channel === "B") {
            _values_B[period] = row.GetColFloat(8);
            if (period === 47) {
                result.AddDay(_nmi, channel, readdateOnly, 30, _values_B);
                _values_B = [];
            }


        }




    }



}


function FileProcessSAPN(csv) {


    var result = new MeterSiteData();

    var _nmi = "";


    var _values = [];
    var _values_B = [];


    csv.forEach(processSAPNRow);

    if (result.Days.length > 0 && result.Days[0].E.length > 0) { result.Valid = true; }
    return result;





    function processSAPNRow(row, idx, rows) {

        if (row === null || row.length === 0) return;

        if (idx === 0) return;  // ignore first line 
        var channel = "";
        var readdate;
        
        _nmi = row.GetColUpper(0);

        var dtString = row.GetColUpper(1) + ' ' + row.GetColUpper(2);
        var m = moment(dtString, "D/MM/YYYY h:mm:ss A");
        if (m.isValid()) {
            m.subtract(30, "m");
            readdate = m.toDate();
        }
        else {
            readdate = new Date();
        }

        
        var period = (readdate.getHours() * 60 + readdate.getMinutes()) / 30
        if (period < 0) period = 0;
        if (period >= 48) period = 47;

        var readdateOnly = new Date(readdate.setHours(0, 0, 0));
        var readValue = row.GetColFloat(3);
        _values[period] = readValue;
        if (period === 47) {
           result.AddDay(_nmi, "E", readdateOnly, 30, _values);
             _values = [];
        }
        



    }



}

function FileProcessERM(csv) {


    var result = new MeterSiteData();

    var _nmi = "";


    var _values = [];
    var _values_B = [];


    csv.forEach(processERMRow);

    if (result.Days.length > 0 && result.Days[0].E.length > 0) { result.Valid = true; }
    return result;





    function processERMRow(row, idx, rows) {

        if (row === null || row.length === 0) return;

        if (idx === 0) return;  // ignore first line 
        var channel = "";
        var readdate;

        _nmi = row.GetColUpper(0);

        var dtString = row.GetColUpper(3) ;
        var m = moment(dtString, "D/MM/YYYY hh:mm");
        if (m.isValid()) {
            //m.subtract(30, "m");
            readdate = m.toDate();
        }
        else {
            readdate = new Date();
        }


        var period = (readdate.getHours() * 60 + readdate.getMinutes()) / 30
        if (period < 0) period = 0;
        if (period >= 48) period = 47;

        var readdateOnly = new Date(readdate.setHours(0, 0, 0));
        var readValue = row.GetColFloat(4);
        _values[period] = readValue;
        if (period === 47) {
            result.AddDay(_nmi, "E", readdateOnly, 30, _values);
            _values = [];
        }




    }



}

function FileProcessAUSNET(csv) {


    var result = new MeterSiteData();

    var _nmi = "";


    var _values = [];
    var _values_B = [];


    csv.forEach(processAUSNETRow);

    if (result.Days.length > 0 && result.Days[0].E.length > 0) { result.Valid = true; }
    return result;





    function processAUSNETRow(row, idx, rows) {

        if (row === null || row.length === 0) return;

        if (idx === 0) return;  // ignore first line 
        var channel = "";
        var readdate;

        _nmi = row.GetColUpper(5);

        var dtString = row.GetColUpper(1);
        var m = moment(dtString, "D/MM/YYYY h:mm");
        
        if (m.isValid()) {
            //m.subtract(30, "m");
            readdate = m.toDate();
        }
        else {
            readdate = new Date();
        }


        var period = (readdate.getHours() * 60 + readdate.getMinutes()) / 30
        if (period < 0) period = 0;
        if (period >= 48) period = 47;

        var readdateOnly = new Date(readdate.setHours(0, 0, 0));
        var readValue = row.GetColFloat(2);
        _values[period] = readValue;
        if (period === 47) {
            result.AddDay(_nmi, "E", readdateOnly, 30, _values);
            _values = [];
        }




    }



}

