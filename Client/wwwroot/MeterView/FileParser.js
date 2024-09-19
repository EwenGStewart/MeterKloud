

function FileOnLoad(evt) {
    var files = evt.target.files; // FileList object

    // files is a FileList of File objects. List some properties.
    if (files.length > 0) {
        FileParse(files[0]);
    }


}



function FileLoadUrl(url) {

    $("#FileInfoName").text(url);
    $("#FileInfoType").text("web");
    $("#FileBytes").text("n/a");

    Papa.parse(url, {
        download: true,
        // rest of config ...
        delimiter: "",	// auto-detect
        newline: "\n",	// auto-detect
        header: false,
        dynamicTyping: false,
        preview: 0,
        encoding: "",
        worker: false,
        comments: false,
        step: undefined,
        complete: FileParseComplete,
        error: undefined,

        skipEmptyLines: false,
        chunk: undefined,
        fastMode: undefined,
        beforeFirstChunk: undefined,
        withCredentials: undefined

    });


}


function FileParse(f) {


    $("#FileInfoName").text(f.name);
    $("#FileInfoType").text(f.type);
    $("#FileBytes").text(f.size);

    Papa.parse(f,
    {
        delimiter: ",",	// auto-detect
        newline: "\n",	// auto-detect
        header: false,
        dynamicTyping: false,
        preview: 0,
        encoding: "",
        worker: false,
        comments: false,
        step: undefined,
        complete: FileParseComplete,
        error: undefined,
        download: false,
        skipEmptyLines: true,
        chunk: undefined,
        fastMode: undefined,
        beforeFirstChunk: undefined,
        withCredentials: undefined
    }
    );
}






function FileParseComplete(results, file) {

    var result = { Valid: true, ErrorMessage: null };
    if (results.errors.length > 0) {
        result = { Valid: false, ErrorMessage: results.errors[0] };
    }

    if (result.Valid) {

        var csv = results.data;
        RemoveNewLines(csv);

        $("#FileLines").text(csv.length + " lines");



        var type = FileGetType(csv);

        $("#FileFormat").text(type);



        switch (type) {
            case "NEM12":
                result = FileProcessNem12(csv);
                break;
            case "powershop":
                result = FileProcessPowerShop(csv);
                break;

            case "AGL":
                result = FileProcessAGL(csv);
                break;

            case "sapn":
                result = FileProcessSAPN(csv);
                break;
            case "ERM":
                result = FileProcessERM(csv);
                break;

            case "AUSNET":
                result = FileProcessAUSNET(csv);
                break;

            case "invalid":
                result = { Valid: false, ErrorMessage: "Sorry I could not recognise the file format" };
                break;
            default:
                result = { Valid: false, ErrorMessage: "Invalid file type " + type };
                break;

        }

    }
    if (result && result.Valid) {
        PostProcess(result);
        SaveToLocalSotrage(result);
        Global.MeterData = result;
        SetupTabsForData();
        InitGlobalVars(true);
    }

}


function PostProcess(result) { }


function FileToCsv(f) {
    return null;
}

function FileGetType(csv) {

    if (csv === null) return "Invalid";
    if (csv.length === null) return "Invalid";
    if (csv.length === 0) return "Invalid";

    if (csv[0] === null) return "Invalid";
    if (csv[0].length === null) return "Invalid";
    if (csv[0].length === 0) return "Invalid";

    var line = csv[0].join().toUpperCase();

    if (line.startsWith("\n")) line = line.substring(1);
    if (line.startsWith("\r")) line = line.substring(1);




    if (line.startsWith("NMI,METER SERIAL NUMBER,CON/GEN,DATE,ESTIMATED?")) return "powershop";
    if (line.startsWith("ACCOUNTNUMBER,NMI,DEVICENUMBER,DEVICETYPE,REGISTERCODE,RATETYPEDESCRIPTION,STARTDATE,ENDDATE,PROFILEREADVALUE,REGISTERREADVALUE,QUALITYFLAG")) return "AGL";
    if (line.startsWith("NMI,DATE,TIME,KWH")) return "sapn";
    if (line.startsWith("NMI,METERSERIAL,PERIOD,LOCAL TIME,E")) return "ERM";
    if (line.startsWith("METER TYPE,READ DATE AND TIME,READ VALUE (KWH)")) return "AUSNET";

    if (csv[0][0] === '100' || csv[0][0] === '200' || csv[0][0] === '900') return "NEM12";

    return "invalid";





}



function RemoveNewLines(csv) {

    if (csv === null) return;

    if (csv.length === null) return;
    if (csv.length === 0) return;


    for (var i = 0, len = csv.length; i < len; i++) {
         
        if (csv[i][0].startsWith("\n")) csv[i][0] = csv[i][0].substr(1);
        if (csv[i][0].startsWith("\r")) csv[i][0] = csv[i][0].substr(1);
    }

    return; 


}



