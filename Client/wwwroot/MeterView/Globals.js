


function Globals() {
    this.MeterData = null;
    this.Tab_Daily_Setup = false;
    this.Tab_Detail_Setup = false;
    this.Tab_HeatMap_Setup = false;
    this.Tab_3D_Setup = false;
    this.Tab_Profile_Setup = false;
    this.Tab_Analysis_Setup = false; 
}


var Global = new Globals();








function InitGlobalVars(dataLoaded) {
    if (!dataLoaded || Global.MeterData === null || Global.MeterData.Valid === false ) {
        Global = new Globals();
        Global.MeterData = null;

    }
    else {
        // just reset the tabs 
        Global.Tab_Daily_Setup = false;
        Global.Tab_Detail_Setup = false;

        Global.Tab_HeatMap_Setup = false;
        Global.Tab_3D_Setup = false;
        Global.Tab_Profile_Setup = false;  
        Global.Tab_Analysis_Setup = false;
    }











}