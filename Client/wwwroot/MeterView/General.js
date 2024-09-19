


// setup called on document ready 
function Setup() {

    // CHeck if the browser is compatible 
    if (!BrowserCompat()) return;   // TODO:  show error 
    // General stuff 
    GeneralStart();
    // setup the tabs 
    TabSetup();

    // if we have a data loaded then start with tab 1 
    if (HasExisitngFileLoaded()) {
        SetupTabsForData();
        InitGlobalVars(true);
    }
    else {
// no data - show file screen 
        SetupNoData();
        InitGlobalVars(false);
    }

    // Hook up the file handler 
    SetUpFileHandler();



    }

// General stuff 
function GeneralStart() {

    // implement startswith 
    if (typeof String.prototype.startsWith !== 'function') {
        String.prototype.startsWith = function (str) {
            return this.substring(0, str.length) === str;
        };
    }


    // Setup prototypes for arrays 
    Array.prototype.GetColUpper = function (idx) {


        if (this === null || this.length <= idx) return "";
        var s = this[idx];
        if (IsNullOrWhiteSpace(s)) return "";
        return s.trim().toUpperCase();

    };

    Array.prototype.GetColInt = function (idx) {
        if (this === null || this.length <= idx) return 0;
        var s = parseInt(this[idx]);
        if (isNaN(s)) s = 0;
        return s;
    };




    Array.prototype.GetColFloat = function (idx) {
        if (this === null || this.length <= idx) return 0.0;
        var s = parseFloat(this[idx]);
        if (isNaN(s)) { s = 0.0; }
        return s;
    };


    Array.prototype.GetColDate_yyyyMMdd = function (idx) {
        if (this === null || this.length <= idx) return new Date();
        var s = this[idx];
        if (IsNullOrWhiteSpace(s)) return new Date();
        if (!/^(\d){8}$/.test(s)) { return new Date(); }
        var y = s.substr(0, 4),
            m = s.substr(4, 2),
            d = s.substr(6, 2);


        return new Date(y + '-' + m + '-' + d);

    };



    Array.prototype.GetColDate = function (idx, format) {
        if (this === null || this.length <= idx) return new Date();
        var s = this[idx];
        if (IsNullOrWhiteSpace(s)) return new Date();

        var m = moment(s, format);
        if (!m.isValid()) {
            return new Date();
        }
        return m.toDate();

    };




    Number.prototype.formatMoney = function (decPlaces, thouSeparator, decSeparator) {
        var n = this;

        decPlaces = (isNaN(decPlaces = Math.abs(decPlaces)) ? 2 : decPlaces);
        decSeparator = decSeparator === undefined ? "." : decSeparator;
        thouSeparator = thouSeparator === undefined ? "," : thouSeparator;
        var sign = n < 0 ? "-" : "",
            i = parseInt(n = Math.abs(+n || 0).toFixed(decPlaces)) + "",
            j = (j = i.length) > 3 ? j % 3 : 0;
        return sign + (j ? i.substr(0, j) + thouSeparator : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thouSeparator) + (decPlaces ? decSeparator + Math.abs(n - i).toFixed(decPlaces).slice(2) : "");

    };



    Number.prototype.roundNumber = function (dec) {
        return( Math.round(this * Math.pow(10, dec)) / Math.pow(10, dec));
    };

    

}

function IsNullOrWhiteSpace(input) {
    return (typeof input === 'undefined' || input === null)
      || input.replace(/\s/g, '').length < 1;
}

function round_number(num, dec) {
    return Math.round(num * Math.pow(10, dec)) / Math.pow(10, dec);
}


function SetUpFileHandler() {
    $("#InputFile").change(FileOnLoad);


    $("#buttonDownload1").click(function (e) {
        FileLoadUrl("Sample1.csv");
    });

    $("#buttonDownload2").click(function (e) {
        FileLoadUrl("Sample2.csv");
    });

}





function SetupNoData() {
    SetupTabsForNoData();


}



function BrowserCompat() {
    if (Modernizr.localstorage) { return true; }
    Alert("This application uses local storage to process meter data, thus avoiding the need to send the data to a remote server. Unfortunately your browser does not support it");
    return false;
}

function HasExisitngFileLoaded() {

    try{
    var o = LoadFromLocalSotrage();
    if (!o) return false;
    Global.MeterData = o;
    return true;
    }
    catch (e) {
        return false;
    }
}




