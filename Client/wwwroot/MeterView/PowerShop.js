

function FileProcessPowerShop(csv) {


    var result = new MeterSiteData();

    var _nmi = "";


    csv.forEach(processPowerShopRow);

    if (result.Days.length > 0 && result.Days[0].E.length > 0) { result.Valid = true; }
    return result;




    function processPowerShopRow(row, idx, rows) {

        if (row === null || row.length === 0) return;

        if (idx === 0) return;  // ignore first line 
        var channel = "";


        //to do :200 header processa 
        if (row.length >= 53) {

            var nmi = row.GetColUpper(0);
            if (_nmi > "" && nmi !== _nmi) {
                _ignoreOtherSites = true; // skipany other site
                return;
            }
            _nmi = nmi;


            //             string serial = cols.GetColUpper(1);
            //string RegisterId = null;

            switch (row.GetColUpper(2)) {
                case "CONSUMPTION":

                    channel = "E";
                    break;
                case "GENERATION":
                    channel = "B";
                    break;
                default:
                    return;
                   
            }


            var interval = (row.length >= 101 ? 15 : 30);




            var readDate = row.GetColDate(3, "D/MM/YYYY");
            var values = [];
            //  string qualityMethod = (cols.GetColUpper(4) == "YES" ? "S" : "A");

            if (row.length >= 101) {

                for (let i = 1; i <= 96; ++i) {
                    values[i - 1] = row.GetColFloat(4 + i);
                }
            }
            else if (row.length >= 53) {

                for (let i = 1; i <= 48; ++i) {
                    values[i - 1] = row.GetColFloat(4 + i);
                }

            }

            result.AddDay(_nmi, channel, readDate, interval, values);

        }




    }




}