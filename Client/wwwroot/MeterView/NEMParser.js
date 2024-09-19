function FileProcessNem12(csv) {

    var result = new MeterSiteData();

    var _nmi = "";
    var _mult = 1;
    var _channel = "";
    var _interval = 30;
    var _ignoreOtherSites = false;

    csv.forEach(processNem12Row);

    if (result.Days.length > 0 && result.Days[0].E.length > 0) { result.Valid = true; }
    return result;



    function processNem12Row(row, idx, rows) {
        if (row === null || row.length === 0) return;
        if (row[0].startsWith("\n")) row[0] = row[0].substr(1);
        if (row[0].startsWith("\r")) row[0] = row[0].substr(1);


        switch (row[0]) {
            case "200":
                processNem12Row200(row, idx, rows);
                break;
            case "300":
                processNem12Row300(row, idx, rows);
                break;
            default:
                break;

        }
    }


    function processNem12Row200(row, idx, rows) {


        if (row.length >= 9) {

            var nmi = row.GetColUpper(1);
            _ignoreOtherSites = false;
            if (_nmi > "" && nmi !== _nmi) {
                _ignoreOtherSites = true; // skip any other site
                return;
            }
            _nmi = nmi;

            //var RegisterId = row.GetColUpper(3);
            var suffix = row.GetColUpper(4);
            if ((suffix.length !== 2) && (row.GetColUpper(3).length === 2)) {
                suffix = row.GetColUpper(3);
            }
            //var serial = row.GetColUpper(6);
            var uom = row.GetColUpper(7);
            _interval = row.GetColInt(8);
            _channel = suffix.substr(0, 1);
            switch (uom) {
                case "MWH":
                case "MVARH":
                    _mult = 1000.0;
                    break;
                case "WH":
                case "VARH":
                    _mult = 0.001;
                    break;
                default:
                    _mult = 1.0;
                    break;

            }





            //Nem200Record rec = new Nem200Record() { IntervalLenght = Interval, MeterSerial = serial, NMI = nmi, nmiSuffix = suffix, registerId = RegisterId, UOM = uom };
            //mdm.AddNem200(rec);



        }







    }


    function processNem12Row300(row, idx, rows) {

        if (_ignoreOtherSites) return;

        //to do :300 data process
        if (_interval <= 0) return;

        var expectedCols = Math.floor(24 * 60 / _interval);


        if (row.length >= expectedCols + 2) {
            var readDate = row.GetColDate_yyyyMMdd(1);
            var values = [];

            //  var allZero = true ; 
            for (var i = 1; i <= expectedCols; i++) {
                values[i - 1] = row.GetColFloat(1 + i) * _mult;  //   note I+1 is not a mistake. first col starts at array postion 2 (3rd column)
                //  if (allZero) if (values[i - i] != 0) allZero = false;
            }
            //  if (!allZero) {
            result.AddDay(_nmi, _channel, readDate, _interval, values);

            //  }


        }
    }







}










