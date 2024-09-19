
const CONSUMPTION = "Consumption";
const GENERATION = "Generation";
const NET = "Net";
const KVA = "KVA";
const KW = "KW";
const EXPORTKVARH = "EXPORTKVARH";
const IMPORTKVARH = "IMPORTKVARH";

const POWERFACTOR = "POWERFACTOR";




function MeterDay(date, interval) {
    if (! (date instanceof Date)) { throw new Error("Invalid read date"); }

    this.Date =  new Date(  date.getFullYear() , date.getMonth() , date.getDate() );
    this.Interval = interval;
    this.E = [];
    this.B = [];
    this.K = [];
    this.Q = [];

    this.Periods = function () {
        return (Math.floor(60 * 24 / interval));

    };


    this.GetDayValue = function (valueType) {

        switch (valueType) {
            case CONSUMPTION:
                if (!this.E || this.E.length === 0) return 0.0;
                var v = round_number(ss.sum(this.E), 3);
                return v;


            case GENERATION:
                if (!this.B || this.B.length === 0) return 0.0;
                return round_number(ss.sum(this.B), 3);

            case EXPORTKVARH:
                if (!this.Q || this.Q.length === 0) return 0.0;
                return round_number(ss.sum(this.Q), 3);

            case IMPORTKVARH:
                if (!this.K || this.K.length === 0) return 0.0;
                return round_number(ss.sum(this.K), 3);



            case NET:
                return this.GetDayValue(CONSUMPTION) - this.GetDayValue(GENERATION);

            case KW:
                if (!this.E || this.E.length === 0) return 0.0;
                return round_number(ss.max(this.GetTimeValue(KW)), 3);


            case KVA:
                if (!this.E || this.E.length === 0) return 0.0;
                return round_number(ss.max(this.GetTimeValue(KVA)), 3);




            default:
                throw new Error("Unknown type " + valueType);

        }

    };

    this.EmptyArray = function () {
        var result = [];
        for (let p = 0; p < this.Periods(); ++p) {
            result.push(0.0);
        }
        return result;
    };


    this.GetTimeValue = function (valueType) {

        switch (valueType) {
            case CONSUMPTION:
                if (!this.E || this.E.length === 0) return this.EmptyArray();

                return this.E;

            case GENERATION:
                if (!this.B || this.B.length === 0) return this.EmptyArray();
                return this.B;


            case NET:
                {
                    let result = [];

                    for (let p = 0; p < this.Periods(); ++p) {

                        let eVal = (this.E[p] ? this.E[p] : 0);
                        let bVal = (this.B[p] ? this.B[p] : 0);

                        result.push(eVal - bVal);
                    }
                    return result;
                }

            case EXPORTKVARH:
                if (!this.Q || this.Q.length === 0) return this.EmptyArray();
                return this.Q;

            case IMPORTKVARH:
                if (!this.K || this.K.length === 0) return this.EmptyArray();
                return this.K;




            case KW:
                {
                    if (!this.E || this.E.length === 0) return this.EmptyArray();

                    var result = [];
                    for (var p = 0; p < this.Periods(); ++p) {
                        result.push(this.E[p] * 60 / this.Interval);
                    }
                    return result;


                }




            case KVA:
                {
                    if (!this.E || this.E.length === 0) return this.EmptyArray();

                    let result = [];
                    for (let p = 0; p < this.Periods(); ++p) {
                        let eVal = this.E[p];
                        let qVal = (this.Q[p] ? this.Q[p] : 0);
                        let kVal = (this.K[p] ? this.K[p] : 0);
                        let kvaVal = round_number( Math.sqrt(Math.pow(eVal, 2) + Math.pow(qVal - kVal, 2)) * 60 / this.Interval , 3 ) ;
                        result.push(kvaVal);
                    }
                    return result;


                }


            case POWERFACTOR:
                {
                 

                    let result = [];
                    let kva = this.GetTimeValue(KVA);
                    let kw = this.GetTimeValue(KW);
                    for (let p = 0; p < this.Periods(); ++p) {
                        let pf = round_number(kw[p] / kva[p] * 100, 1) 
                        result.push(pf);
                    }
                    return result;


                }


            default:
                throw new Error("Unknown type " + valueType);

        }

    };

    

    this.Get30minValue = function (valueType) {
        var result = [];
        for (let p = 0; p < 48; ++p) { result[p] = 0.0; }

        for (let idx = 0; idx < this.Periods(); ++idx) {
            var p = idx;
            if (this.Interval === 15) { p = Math.floor(idx / 2); }

            switch (valueType) {
                case CONSUMPTION:
                    result[p] += this.E[idx];
                    break;

                case GENERATION:
                    result[p] += (+this.B[idx]);
                    break;

                case NET:
                    result[p] += round_number(this.E.GetColFloat(idx) - this.B.GetColFloat(idx), 3);
                    break;
                default:
                    throw new Error("Unknown type " + valueType);

            }
        }
        return result;
    };











}


function ProfilePeriod(period,interval ) {
    this.Period = period ; 
    this.Interval = interval ; 
    this.Hour = function () {
        return (moment(new Date(2000, 1, 1)).add(interval * period, "m").hour());
    };

    this.Hours = function () {
        return (moment(new Date(2000, 1, 1)).add(interval * period, "m").format("ha"));
    };





    this._Consumption = [] ;
    this._Generation = [] ; 
  
    this._calcConcumption = null ;
    this._calcGeneration= null ;

    this.Consumption = function () {

        if (typeof this._calcConcumption  === 'number'   ) return this._calcConcumption;
        if (!this._Consumption || !this._Consumption.length || this._Consumption.length === 0)  this._calcConcumption = 0;
        else this._calcConcumption  =  round_number(ss.mean(this._Consumption), 3);
        return   this._calcConcumption; 

    };

    this.Generation = function () {
        if (typeof this._calcGeneration === "number" ) return this._calcGeneration;
        if (!this._Generation || !this._Generation.length || this._Generation.length === 0) this._calcGeneration = 0;
        else this._calcGeneration = round_number(ss.mean(this._Generation), 3);
        return   this._calcGeneration; 

    };

    this.Net = function () { return round_number(this.Consumption() - this.Generation(), 3); };
  



}

function MeterDataProfile(dow, months, interval) {
    
    this.Months = months;
    this.Dow = dow;
    this.Interval = interval;

    this.ProfilePeriods = [];

    this.Periods = function () {
        return (Math.floor(60 * 24 / this.Interval));

    };




    for (var p = 0; p < this.Periods() ; ++p) {
        this.ProfilePeriods[p] = new ProfilePeriod(p,interval);

    }


    
    this.AddMeterDay = function (md) {
        var d = md.Date;
        if ($.isArray(this.Months) && this.Months.length > 0) {
            var m = d.getMonth();
            //if (!this.Months.includes(m)) return;
            if ($.inArray(m, this.Months) < 0) {
                return; // Sunny Code
            }
        }

        if ($.isArray(this.Dow) && this.Dow.length > 0) {
            var dow = d.getDay();
            //if (!this.Dow.includes(dow)) return;
            if ($.inArray(dow, this.Dow) < 0) {
                return; // Sunny Code 
            }


        }


        var e = ConvertPeriodArray(md.E, this.Interval);
        var b = ConvertPeriodArray(md.B, this.Interval);

        for (p = 0; p < this.Periods(); ++p) {
            if (e.length > p) this.ProfilePeriods[p]._Consumption.push(e[p]);
            if (b.length > p) this.ProfilePeriods[p]._Generation.push(b[p]);
        }
    };



    this.Net = function () {
        var result = [];
        for (var p = 0; p < this.Periods(); ++p) {
            result[p] = this.ProfilePeriods[p].Net();
        }
        return result;
    };

    this.Consumption = function () {
        var result = [];
        for (var p = 0; p < this.Periods(); ++p) {
            result[p] = this.ProfilePeriods[p].Consumption();
        }
        return result;
    };

    this.Generation = function () {
        var result = [];
        for (var p = 0; p < this.Periods(); ++p) {
            result[p] = this.ProfilePeriods[p].Generation();
        }
        return result;

    };




    this.AverageConsumption = function () { return ss.mean(this.Consumption()).toFixed(3); };    //@Model.WeekDayAvgProfile.AverageConsumption kW
    this.TotalConsumption = function () { return ss.sum(this.Consumption()).toFixed(3); };    //@Model.WeekDayAvgProfile.AverageConsumption kW
    this.HasGeneration = function () { return (ss.sum(this.Generation()) > 0); };
    this.TopNConsumption = function (n) {
        if (!n) n = 1;



        var sorted = this.ProfilePeriods.slice().sort(function (a, b) { return b.Consumption() - a.Consumption(); });
        return sorted.slice(0, n);
    };
     
    this._base = -1;
    this.Base = function () {

        if (this._base > 0) return this._base;

        var FourHours = 4 * 60 / interval;
        var result = 0;
        if (this.HasGeneration()) {

            result = this.ProfilePeriods.filter(function (a) { return (a.Generation() <= 0 && (a.Hour() <= 6 || a.Hour() > 18)); }).slice().sort(
                function (a, b) { return a.Consumption() - b.Consumption(); }
            ).slice(0, FourHours);
        }
        else {

            result = this.ProfilePeriods.slice().sort(
                function (a, b) { return a.Consumption() - b.Consumption(); }
            ).slice(0, FourHours);


        }

        var sum = 0;
        result.forEach(function (x) { sum += x.Consumption(); });
        var avg = sum / FourHours;

        this._base = avg.toFixed(3);
        return this._base;

    };

    this.BasePerPeriod = function () {
        return (this.Base() * (interval / 60.0)).toFixed(3);
    };

      
    this.BaseTotal = function () {
        return (this.Base() * 24).toFixed(3);
    };

    this.BasePc = function () {
        return (this.BaseTotal() / this.TotalConsumption() * 100).toFixed(1);
    };
    
    //@Model.WeekDayAvgProfile.TotalConsumption kWh
    //@Model.WeekDayAvgProfile.Peak.Top kW
    //@Model.WeekDayAvgProfile.Peak.TopHour
    //@Model.WeekDayAvgProfile.Peak.N
    //@Model.WeekDayAvgProfile.Peak.TopNHours
    //@Model.WeekDayAvgProfile.Peak.TopNkWh kWh
    //@Model.WeekDayAvgProfile.Peak.TopNPercentage
    //@Model.WeekDayAvgProfile.Base kW
    //@Model.WeekDayAvgProfile.BaseTotal kWh
    //@Model.WeekDayAvgProfile.BasePercentage
    //@Model.WeekDayAvgProfile.AverageNightly.TimeRange
    //@Model.WeekDayAvgProfile.AverageNightly.AvgEnergy kW<
    //@Model.WeekDayAvgProfile.AverageNightly.TotEnergy kWh
    //@Model.WeekDayAvgProfile.AverageNightly.PercentageTotal



    this.GetBlock = function (periods) {

        var result = {
            AvgEnergykW: 0.000
            , TotEnergyKWh: 0.000
            , PercentageTotal: 0.0
            , Values: []
        };

        var parent = this;
        var v = [];
        var bpp = this.BasePerPeriod();
        var con = this.Consumption();


        for (p = 0; p < this.Periods(); ++p) {
            result.Values[p] = 0;
        }



        periods.forEach(function (p) {
            var val = (con[p] - bpp);
            v.push(val);

            //if (con[p] > bpp) v.push(con[p] - bpp)
            //else v.push(0.000);
        });



        result.AvgEnergykW = (ss.mean(v) * 60 / interval).toFixed(3);

        periods.forEach(function (p) {
            result.Values[p] = result.AvgEnergykW;
            //if (con[p] > bpp) v.push(con[p] - bpp)
            //else v.push(0.000);
        });






        result.TotEnergyKWh = ss.sum(v).toFixed(3);
        result.PercentageTotal = ((result.TotEnergyKWh / this.TotalConsumption()) * 100).toFixed(1);


        return result;

    };


    
}


function ConvertPeriodArray( source , TargetInterval ){
    if (!$.isArray(source) ) return []; 
    if (source.length === 0) return [];
    
    var result = []; 
    var sourceInterval = Math.floor(24 * 60 / source.length);
    if (sourceInterval === TargetInterval) return source; 
    var tp = 0; 
    var expectedPeriod = Math.floor(24 * 60 / TargetInterval);
    for(let p = 0; p< expectedPeriod; ++p) { result[p] = 0.0; }

    if ( sourceInterval <= TargetInterval ) { 
        // add one or more source intervals to get target 
        for( let p = 0; p<source.length; ++p )   { 
            tp = Math.floor(p * sourceInterval / TargetInterval);
            result[tp] += source[p];
        }

    }
    else { 
        for( let p = 0; p<expectedPeriod; ++p )   { 
            tp = Math.floor(p * TargetInterval / sourceInterval);

            result[p] = source[tp] *  TargetInterval / sourceInterval; 

        }


    }
    return result; 

}




function Profiles(interval) {
    this.Overall = new MeterDataProfile(null, null, interval);
    this.WeekDay = new MeterDataProfile([1, 2, 3, 4, 5], null, interval);
    this.WeekEnd = new MeterDataProfile([0, 6], null, interval);
    this.Summer = new MeterDataProfile(null, [12, 1, 2], interval);
    this.Winter = new MeterDataProfile([0, 6], [6, 7, 8], interval);
    this.Spring = new MeterDataProfile([0, 6], [9, 10, 11], interval);
    this.Autumn = new MeterDataProfile([0, 6], [3, 4, 5], interval);

    this.ProcessMeterData = function (SiteData) {

        var parent = this;

        SiteData.Days.forEach(function (md) {
            parent.Overall.AddMeterDay(md);
            parent.WeekDay.AddMeterDay(md);
            parent.WeekEnd.AddMeterDay(md);
            parent.Summer.AddMeterDay(md);
            parent.Winter.AddMeterDay(md);
            parent.Spring.AddMeterDay(md);
            parent.Autumn.AddMeterDay(md);
        });


    };
}







function MeterSiteData() {
    this.Valid = true;
    this.Site = null;
    this.Days = [];
    this._CurrentDay = null;
    this._lastIdx = -1;
    this.MinDate = null;
    this.MaxDate = null;

    this.Site = null;
    this.Error = null;




    this.AddDay = function (nmi, channel, readDate, interval, values) {

        if (!(readDate instanceof Date)) { throw new Error("Invalid read date"); }

        readDate = new Date(readDate.getFullYear(), readDate.getMonth(), readDate.getDate());


        if (channel !== "E" && channel !== "B" && channel !== "K" && channel !== "Q") {
            return;
        }

        if (this.Site > "" && this.Site !== nmi) return;
        this.Site = nmi;

        if (interval <= 0) return;

        var expectedLength = Math.floor(60 * 24 / interval);
        if (values.length !== expectedLength) throw new Error('Value array length ' + (values.length) + ' does not match expected length ' + expectedLength + ' for interval ' + interval);

        var md = null;
        if (this._CurrentDay !== null && this._CurrentDay.Date.getTime() === readDate.getTime()) {
            // yeah
            md = this._CurrentDay;

        }
        else if (this.Days === null || this.Days.length === 0) {
            // first row
            md = new MeterDay(readDate, interval);
            this._CurrentDay = md;
            this.MinDate = readDate;
            this.MaxDate = readDate;
            this.Days.push(md);
            this._lastIdx = this.Days.length - 1;
        }
        else if (this.MaxDate.getTime() < readDate.getTime()) {
            // new end element
            md = new MeterDay(readDate, interval);
            this.MaxDate = readDate;
            this.Days.push(md);
            this._CurrentDay = md;
            this._lastIdx = this.Days.length - 1;
        }
        else if (this.MinDate.getTime() > readDate.getTime()) {
            // new pre element
            md = new MeterDay(readDate, interval);
            this.MinDate = readDate;
            this.Days.unshift(md);
            this._lastIdx = 0;

        }
        else if (this._lastIdx + 1 < this.Days.length && this.Days[this._lastIdx + 1].Date.getTime() === readDate.getTime()) {
            md = this.Days[this._lastIdx + 1];
            this._lastIdx = this._lastIdx + 1;
            this._CurrentDay = md;
        }

        else {
            for (var i = 0; i < this.Days.length; i++) {
                if (this.Days[i].Date.getTime() === readDate.getTime()) {
                    md = this.Days[i];
                    this._lastIdx = i;
                    break;
                }
                else if (this.Days[i].Date.getTime() > readDate.getTime()) {
                    md = new MeterDay(readDate, interval);
                    this.Days.splice(i, 0, md);
                    this._lastIdx = i;
                    break;
                }
            }
            if (md === null) {
                md = new MeterDay(readDate, interval);
                this.Days.push(md);
                this._lastIdx = this.Days.length - 1;
            }
        }


        // ok
        // validate the interval
        if (md.Interval !== interval) throw new Error("Intervals on the same day must match in this version -  Site:" + nmi + " Date:" + readDate.toString());

        if (md[channel] === null) { md[channel] = values; }
        else if (md[channel].length === 0) { md[channel] = values; }
        else {
            for (let i = 0; i < expectedLength; i++) {
                md[channel][i] += values[i];
            }
        }



    };



    this.GetDayList = function () {

        var result = [];
        this.Days.forEach(function (d) { result.push(new Date(d.Date)); });
        return result;

    };


    this.GetDayValue = function (valuetype) {
        var result = [];
        this.Days.forEach(function (d) { result.push(d.GetDayValue(valuetype)); });
        return result;
          };


    this.GetTimeListUTC = function () {

        var result = [];
        this.Days.forEach(function (d) {
            var bd = d.Date;
            for (p = 0; p < d.Periods(); ++p) {
                result.push(new Date(   bd.getTime() + p * d.Interval * 60000 ));
            }
        });
        return result;

    };


    this.GetTimeListEST = function () {

        var result = [];
        this.Days.forEach(function (d) {
            var bd = d.Date;
            for (p = 0; p < d.Periods(); ++p) {
                result.push(new Date(bd.getTime() + p * d.Interval * 60000));
            }
            

        }
        );
        return result;

    };

    this.GetTimeValue = function (valuetype) {

        var result = [];
        this.Days.forEach(function (d) {
            d.GetTimeValue(valuetype).forEach(function (t) { result.push(t); });
        });
        return result;

    };

    this.GetProfile = function () {







    };


}


function LoadFromLocalSotrage() {

    if (!Modernizr.localstorage) { return null; }

    if (!localStorage.MeterData) { return null; }
    try {
        var result = null;

        var s = localStorage.MeterData;
        var o = JSON.parse(s);
        if (!o && !o.Valid) { return null; }
        result = new MeterSiteData();

        // copy the keys 


        result.Valid = o.Valid;
        result.Site = o.Site;
        result.MinDate = new Date(o.MinDate);
        result.MaxDate = new Date(o.MaxDate);
        o.Days.forEach(function (x) {
            d = new MeterDay(new Date(x.Date), x.Interval);
            d.B = x.B;
            d.E = x.E;
            d.K = x.K;
            d.Q = x.Q;
            result.Days.push(d);
        });


        return result;
    }
    catch (e) {
        return null;
    }
}



function SaveToLocalSotrage(result) {
   
    if (!Modernizr.localstorage) { return; }
    localStorage.MeterData = JSON.stringify(result);

}