//import { type } from "os";

//文件Setting.js必须在此文件之前

/*
 * 获取URL地址中参数信息
 * parakey:参数键
 * return:参数值,找不到返回""
 */
function t_request(parakey) {
    var url = location.href;
    var paraString = url.substring(url.indexOf("?") + 1, url.length).split("&");
    var paraObj = {};
    for (i = 0; j = paraString[i]; i++) {
        paraObj[j.substring(0, j.indexOf("=")).toLowerCase()] = j.substring(j.indexOf("=") + 1, j.length);
    }
    var returnValue = paraObj[parakey.toLowerCase()];
    if (typeof (returnValue) == "undefined") {
        return "";
    } else {
        return returnValue;
    }
}

/*
 * 服务调用
 * method:服务方法名
 * paras:调用服务传入参数
 * callback:
 * completeCallbackl 成功或失败后必须执行的函数
 */
function t_ajax(method, paras, callback, completeCallbackl) {
    $.ajax({
        type: "POST",
        url: method,
        data: paras,
        dataType: "json",
        success: function (res) {
            if (callback)
                callback(res);
        },
        error: function (x, y, z) {
            console.log('网络错误：' + y);
            callback(x, y, z);
            //$.toptip('网络错误', 'error');
        },
        complete: function (XMLHttpRequest, status) {
            if (completeCallbackl != undefined && completeCallbackl != null) {
                completeCallbackl(XMLHttpRequest, status);
            }
        }

    });
}



/*
 * 图片上传服务调用
 * method:服务方法名
 * paras:调用服务传入参数
 * callback:
 */
function t_ajax_img(method, paras, callback) {
    $.ajax({
        type: "POST",
        url: Setting.ServerPath + method,
        data: paras,
        processData: false,
        contentType: false,
        success: function (res) {
            if (callback)
                callback(res);;
        },
        error: function (x, y, z) {
            console.log('网络错误：' + y);
        }
    });
}

/*
 * 图片上传服务调用
 * method:服务方法名
 * paras:调用服务传入参数
 * callback:
 */
function t_ajax_imgNoAsync(method, paras, callback) {
    $.ajax({
        type: "POST",
        url: Setting.ServerPath + method,
        data: paras,
        processData: false,
        contentType: false,
        async: false,
        success: function (res) {
            if (callback)
                callback(res);;
        },
        error: function (x, y, z) {
            console.log('网络错误：' + y);
        }
    });
}

function t_ajaxNoAsync(method, paras, callback, completeCallbackl) {
    $.ajax({
        type: "POST",
        url: Setting.ServerPath + method,
        data: paras,
        dataType: "json",
        async: false,
        success: function (res) {
            if (callback)
                callback(res);;
        },
        error: function (x, y, z) {
            console.log('网络错误：' + y);
            callback(x, y, z);
            //$.toptip('网络错误', 'error');
        },
        complete: function () {
            if (completeCallbackl != undefined && completeCallbackl != null) {
                completeCallbackl();
            }
        }
    });
}


/**检查登陆状态 */
function CheckLoginStatus() {
    let hzid = sessionStorage.getItem("hzid");
    let hzmc = sessionStorage.getItem("hzmc");
    return !IsNullOrEmpty(hzid) && !IsNullOrEmpty(hzmc);
}
function IsNullOrEmpty(value) {

    return value == undefined || value == null || (value + "").trim().length == 0;
}

function GetLocalOpenid() {
    return sessionStorage.getItem("openid");
}
function SetLocalOpenid(openid) {
    return sessionStorage.setItem("openid", openid);
}

function GetLocalInfo() {
    return localStorage.getItem("info");
}
function SetLocalInfo(info) {
    return localStorage.setItem("info", info);
}

/**跳转到用户绑定页面 */
function ToUserBind(userBindUrl) {
    if (IsNullOrEmpty(userBindUrl)) {
        return;
    }
    window.location.href = userBindUrl;
}
/**根据登录状态进行下一步 */
function CheckLoginAndNext(userBindUrl, fuc) {
    if (CheckLoginStatus()) {
        //已登录
        if (fuc != undefined || fuc != null) {
            fuc();
        }
        return;
    }
    //进行登录或者注册
    var openid = GetLocalOpenid();

    if (IsNullOrEmpty(openid)) {
        openid = GetOpenId(false);
        if (IsNullOrEmpty(openid)) {
            alert("您的信息获取失败，请重新进入平台");
            try {
                WeixinJSBridge.call('closeWindow');
            } catch (e) {

            }
            return;
        } else {
            SetLocalOpenid(openid);
        }
    }
    var loginJson = { "HZPTID": openid, "YYID": Setting.HospitalId, "KFPTLX": Setting.AppType };
    //alert(JSON.stringify(loginJson));
    t_ajax("user/UserSelect", loginJson, function (infodata) {
        //  alert(JSON.stringify(infodata))
        //判断用户信息
        if (infodata.IsSuccess) {
            let pat = infodata.Data;
            if (JSON.stringify(pat) == "[]") {
                ToUserBind(userBindUrl);
            } else {
                let info = pat;
                sessionStorage.setItem("hzid", info.HZID);
                sessionStorage.setItem("hzmc", info.HZMC);
                if (fuc != undefined || fuc != null) {
                    fuc();
                } else {
                    window.location.href = document.referrer;
                }

            }
        } else {
            ToUserBind(userBindUrl);
        }
    })
}

function GetHzidByOpenid(openid) {
    if (IsNullOrEmpty(openid)) {
        return;
    }
    var loginJson = { "HZPTID": openid, "YYID": Setting.HospitalId, "KFPTLX": Setting.AppType };
    //alert(JSON.stringify(loginJson));
    t_ajax("user/UserSelect", loginJson, function (infodata) {
        //判断用户信息
        if (infodata.IsSuccess) {
            let pat = infodata.Data;
            if (JSON.stringify(pat) != "[]") {
                let info = pat;
                sessionStorage.setItem("hzid", info.HZID);
                sessionStorage.setItem("hzmc", info.HZMC);
            }
        }
    })



}

/**
 * 是异步吗
 * @param {any} isAsync
 */
function GetOpenId(isAsync, completeFuc) {
    var yhcode = localStorage.getItem("yhcode");//首页queryString传参:用户编码
    var code = null;
    // alert("参数：HospitalId：" + Setting.HospitalId + "AppType：" + Setting.AppType + "；yhcode：" + yhcode)
    let fuc = null;
    if (isAsync) {
        fuc = t_ajax;
    } else {
        fuc = t_ajaxNoAsync
    }
    fuc("user/GetOpenID", { "YYID": Setting.HospitalId, "KFPTLX": Setting.AppType, "YHBM": yhcode }, function (data) {
        //  alert("IsSuccess:" + data.IsSuccess + "Data:" + data.Data)
        if (data.IsSuccess) {
            code = data.Data;
            if (isAsync) {
                SetLocalOpenid(code);
            }
        } else {
            //alert(JSON.stringify(data.Data));
        }
    }, function () {
        if (completeFuc != undefined && completeFuc != null) {
            completeFuc();
        }
    })
    return code;
}
/**
 * 修改微信端网页的title
 * @param {any} titleStr
 */
function AlterHtmlTitle(titleStr) {
    var $body = $('body');
    document.title = titleStr;

    var $iframe = $('<iframe src="/favicon.ico"></iframe>');
    $iframe.on('load', function () {
        setTimeout(function () {
            $iframe.off('load').remove();
        }, 0);
    }).appendTo($body);
}

Date.prototype.format = function (format) {
    /*
     * eg:format="YYYY-MM-dd hh:mm:ss";

     */
    var o = {
        "M+": this.getMonth() + 1, // month
        "d+": this.getDate(), // day
        "h+": this.getHours(), // hour
        "m+": this.getMinutes(), // minute
        "s+": this.getSeconds(), // second
        "q+": Math.floor((this.getMonth() + 3) / 3), // quarter
        "S": this.getMilliseconds()
        // millisecond
    }
    if (/(y+)/.test(format)) {
        format = format.replace(RegExp.$1, (this.getFullYear() + "")
            .substr(4 - RegExp.$1.length));
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(format)) {
            format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k]
                : ("00" + o[k]).substr(("" + o[k]).length));
        }
    }
    return format;
}
/**
    * 获取n天前后的日期
    * @param day
    */
function GetDayBeforeOrAfter(day) {
    var time = new Date();
    time.setDate(time.getDate() + day);//获取Day天后的日期
    var y = time.getFullYear();
    var m = time.getMonth() + 1;//获取当前月份的日期
    var d = time.getDate();
    return y + "-" + m + "-" + d;
}


/**
 * 往前数monthNum月份，不能往后数monthNum
 * @param date 计算的开始时间。格式：2020-01-01
 * @param monthNum 往前的月数
 */
function GetMonthBeforeOrAfter(date, monthNum) {
    let dateArr = date.split('-')
    let year = dateArr[0] //获取当前日期的年份
    let month = dateArr[1] //获取当前日期的月份
    let day = dateArr[2] //获取当前日期的日
    let days = new Date(year, month, 0)
    days = days.getDate() //获取当前日期中月的天数
    let year2 = year
    let month2 = parseInt(month) - monthNum
    if (month2 <= 0) {
        year2 =
            parseInt(year2) -
            parseInt(month2 / 12 == 0 ? 1 : Math.abs(parseInt(month2 / 12)) + 1)
        month2 = 12 - (Math.abs(month2) % 12)
    }
    let day2 = day
    let days2 = new Date(year2, month2, 0)
    days2 = days2.getDate()
    if (day2 > days2) {
        day2 = days2
    }
    if (month2 < 10) {
        month2 = '0' + month2
    }
    let t2 = year2 + '-' + month2 + '-' + day2
    return t2
}

function GetQueryFalseInfo(data) {
    let str = "查询失败";
    if ((typeof (data) == "string") && !IsNullOrEmpty(data)) {
        str = data;
    }
    return str;
}
//img_url : 功能未开放图片地址
function wsx_dialog(img_url) {
    msg = '<img style="height:310px;width:300px" src="' + img_url + '"/>'
    dialog2 = '<div class="js_dialog" id="wkfDialog" style="display: none;">\
            <div class="weui-mask"></div>\
            <div class="weui-dialog" style="margin: auto;width:300px;">\
                <div class="weui-dialog__bd" style="margin: 0;padding:0;">'+ msg + '</div>\
                <div class="weui-dialog__ft">\
                    <a href="javascript:;" id="a_click" class="weui-dialog__btn weui-dialog__btn_primary">我知道了</a>\
                </div>\
            </div>\
                </div >';
    if (!$('#wkfDialog').length) {
        $('body').append(dialog2);
    } else {
        $('#wkfDialog .weui_dialog_bd').html(msg);
    }
    $('#wkfDialog').fadeIn(200);
    $('#a_click').click(function () {
        $('#wkfDialog').fadeOut(200);
    });
    $('#wkfDialog .weui-mask').click(function () {
        $('#wkfDialog').fadeOut(200);
    });
    setTimeout(function () {
        $('#wkfDialog').fadeOut(200);
    }, 2000);
}

//时间工具
var datetimeTool = {
    weeks: new Array('周日', '周一', '周二', '周三', '周四', '周五', '周六'),
    changeDatetime: function (datetime) {
        return datetime.replace(/-/g, "/").replace(/\./g, "/").replace(/[A-Za-z]+/g, " ");
    },
    getDatetime: function (datetime) {
        return new Date(this.changeDatetime(datetime));
    },
    getTimeStamp: function (datetime) {
        return this.getDatetime(datetime).getTime();
    },
    getWeek: function (datetime) {
        var daytime = this.getDatetime(datetime);
        return weeks[daytime.getDay()];
    },
    getTimeSpan: function (pretime, curtime) {
        if (IsNullOrEmpty(pretime))
            return 0;
        var pre = this.getDatetime(pretime);
        var cur = new Date();
        if (!IsNullOrEmpty(curtime))
            cur = this.getDatetime(curtime);
        //返回时间间隔，分钟
        return (cur - pre) / 1000 / 60;
    },
    getCustomDateTime: function (datetime, istalk) {
        var daytime = this.getDatetime(datetime);
        var dtYear = daytime.getFullYear();
        var dtMonth = daytime.getMonth() + 1;
        var dtDay = daytime.getDate();
        var dtHour = daytime.getHours();
        var dtMin = daytime.getMinutes();
        var dtWeek = this.weeks[daytime.getDay()];
        daytime = this.getDatetime(dtYear + "-" + GetTwoLength(dtMonth) + "-" + GetTwoLength(dtDay) + " 00:00:00");
        var now = new Date();//this.getDatetime("2020-03-28 15:20:21"); //
        var nYear = now.getFullYear();
        var nMonth = now.getMonth() + 1;
        var nDay = now.getDate();
        var splitMin = (now - daytime) / 1000 / 60;
        var splitDay = splitMin / 60 / 24;
        var back = "";
        var talkStr = GetTwoLength(dtHour) + ":" + GetTwoLength(dtMin);
        if (nYear > dtYear)
            back = dtYear + "年" + GetTwoLength(dtMonth) + "月" + GetTwoLength(dtDay) + "日";
        else {
            if (splitDay >= 7)
                back = GetTwoLength(dtMonth) + "月" + GetTwoLength(dtDay) + "日";
            else {
                if (splitDay >= 2) {
                    if (daytime.getDay() < now.getDay())
                        back = dtWeek;
                    else
                        back = GetTwoLength(dtMonth) + "月" + GetTwoLength(dtDay) + "日";
                }
                else if (splitDay >= 1) {
                    back = "昨天";
                }
                else {
                    if (nDay != dtDay)
                        back = "昨天";
                    else
                        back = talkStr;
                }
            }
        }
        if (istalk && back != talkStr)
            back += " " + talkStr;

        function GetTwoLength(num) {
            var str = num + '';
            if (str.length == 1)
                return "0" + str;
            return str;
        }

        return back;

    }
}
/**
 * 验证手机号格式规范
 * @param {any} mobile
 */
function CheckPhone(mobile) {
    return /^1[0-9]{10}$/.test(mobile);
    //let tel = /^0\d{2,3}-?\d{7,8}$/;
    //let phone = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1}))+\d{8})$/;
    //if (mobile.length == 11) {//手机号码
    //    if (phone.test(mobile)) {
    //        return true;
    //    }
    //} else if (mobile.length == 13 && mobile.indexOf("-") != -1) {//电话号码
    //    if (tel.test(mobile)) {
    //        return true;
    //    }
    //}
    //return false;
}

/**
 * 验证身份证格式规范
 * @param idcode string
 */
function CheckIDCard(idcode) {
    num = idcode.toUpperCase();
    //身份证号码为15位或者18位，15位时全为数字，18位前17位为数字，最后一位是校验位，可能为数字或字符X。   
    if (!(/(^\d{15}$)|(^\d{17}([0-9]|X)$)/.test(num))) {
        return false;
    }
    //校验位按照ISO 7064:1983.MOD 11-2的规定生成，X可以认为是数字10。 
    //下面分别分析出生日期和校验位 
    var len, re;
    len = num.length;
    if (len == 15) {
        re = new RegExp(/^(\d{6})(\d{2})(\d{2})(\d{2})(\d{3})$/);
        var arrSplit = num.match(re);

        //检查生日日期是否正确 
        var dtmBirth = new Date('19' + arrSplit[2] + '/' + arrSplit[3] + '/' + arrSplit[4]);
        var bCorrectDay;
        bCorrectDay = (dtmBirth.getYear() == Number(arrSplit[2])) && ((dtmBirth.getMonth() + 1) == Number(arrSplit[3])) &&
            (
                dtmBirth.getDate() == Number(arrSplit[4]));
        if (!bCorrectDay) {
            return false;
        } else {
            //将15位身份证转成18位 
            //校验位按照ISO 7064:1983.MOD 11-2的规定生成，X可以认为是数字10。 
            var arrInt = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2);
            var arrCh = new Array('1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2');
            var nTemp = 0,
                i;
            num = num.substr(0, 6) + '19' + num.substr(6, num.length - 6);
            for (i = 0; i < 17; i++) {
                nTemp += num.substr(i, 1) * arrInt[i];
            }
            num += arrCh[nTemp % 11];
            return true;
        }
    }
    if (len == 18) {
        re = new RegExp(/^(\d{6})(\d{4})(\d{2})(\d{2})(\d{3})([0-9]|X)$/);
        var arrSplit = num.match(re);

        //检查生日日期是否正确 
        var dtmBirth = new Date(arrSplit[2] + "/" + arrSplit[3] + "/" + arrSplit[4]);
        var bCorrectDay;
        bCorrectDay = (dtmBirth.getFullYear() == Number(arrSplit[2])) && ((dtmBirth.getMonth() + 1) == Number(arrSplit[3])) &&
            (dtmBirth.getDate() == Number(arrSplit[4]));
        if (!bCorrectDay) {
            return false;
        } else {
            //检验18位身份证的校验码是否正确。 
            //校验位按照ISO 7064:1983.MOD 11-2的规定生成，X可以认为是数字10。 
            var valnum;
            var arrInt = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2);
            var arrCh = new Array('1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2');
            var nTemp = 0,
                i;
            for (i = 0; i < 17; i++) {
                nTemp += num.substr(i, 1) * arrInt[i];
            }
            valnum = arrCh[nTemp % 11];
            if (valnum != num.substr(17, 1)) {
                return false;
            }
            return true;
        }
    }
    return false;
}

function GetAge(identityCard) {
    var len = (identityCard + "").length;
    if (len == 0) {
        return 0;
    } else {
        if ((len != 15) && (len != 18))//身份证号码只能为15位或18位其它不合法
        {
            return 0;
        }
    }
    var strBirthday = "";
    if (len == 18)//处理18位的身份证号码从号码中得到生日和性别代码
    {
        strBirthday = identityCard.substr(6, 4) + "/" + identityCard.substr(10, 2) + "/" + identityCard.substr(12, 2);
    }
    if (len == 15) {
        strBirthday = "19" + identityCard.substr(6, 2) + "/" + identityCard.substr(8, 2) + "/" + identityCard.substr(10, 2);
    }
    //时间字符串里，必须是“/”
    var birthDate = new Date(strBirthday);
    var nowDateTime = new Date();
    var age = nowDateTime.getFullYear() - birthDate.getFullYear();
    //再考虑月、天的因素;.getMonth()获取的是从0开始的，这里进行比较，不需要加1
    if (nowDateTime.getMonth() < birthDate.getMonth() || (nowDateTime.getMonth() == birthDate.getMonth() && nowDateTime.getDate() < birthDate.getDate())) {
        age--;
    }
    return age;
}

function GetSex(idcard) {
    if (parseInt(idcard.substr(16, 1)) % 2 == 1) {
        return  "男";
    } else {
        return  "女";
    }
}


function GUID() {
    this.date = new Date();   /* 判断是否初始化过，如果初始化过以下代码，则以下代码将不再执行，实际中只执行一次 */
    if (typeof this.newGUID != 'function') {   /* 生成GUID码 */
        GUID.prototype.newGUID = function () {
            this.date = new Date(); var guidStr = '';
            sexadecimalDate = this.hexadecimal(this.getGUIDDate(), 16);
            sexadecimalTime = this.hexadecimal(this.getGUIDTime(), 16);
            for (var i = 0; i < 9; i++) {
                guidStr += Math.floor(Math.random() * 16).toString(16);
            }
            guidStr += sexadecimalDate;
            guidStr += sexadecimalTime;
            while (guidStr.length < 32) {
                guidStr += Math.floor(Math.random() * 16).toString(16);
            }
            return this.formatGUID(guidStr);
        }
        /* * 功能：获取当前日期的GUID格式，即8位数的日期：19700101 * 返回值：返回GUID日期格式的字条串 */
        GUID.prototype.getGUIDDate = function () {
            return this.date.getFullYear() + this.addZero(this.date.getMonth() + 1) + this.addZero(this.date.getDay());
        }
        /* * 功能：获取当前时间的GUID格式，即8位数的时间，包括毫秒，毫秒为2位数：12300933 * 返回值：返回GUID日期格式的字条串 */
        GUID.prototype.getGUIDTime = function () {
            return this.addZero(this.date.getHours()) + this.addZero(this.date.getMinutes()) + this.addZero(this.date.getSeconds()) + this.addZero(parseInt(this.date.getMilliseconds() / 10));
        }
        /* * 功能: 为一位数的正整数前面添加0，如果是可以转成非NaN数字的字符串也可以实现 * 参数: 参数表示准备再前面添加0的数字或可以转换成数字的字符串 * 返回值: 如果符合条件，返回添加0后的字条串类型，否则返回自身的字符串 */
        GUID.prototype.addZero = function (num) {
            if (Number(num).toString() != 'NaN' && num >= 0 && num < 10) {
                return '0' + Math.floor(num);
            } else {
                return num.toString();
            }
        }
            /*  * 功能：将y进制的数值，转换为x进制的数值 * 参数：第1个参数表示欲转换的数值；第2个参数表示欲转换的进制；第3个参数可选，表示当前的进制数，如不写则为10 * 返回值：返回转换后的字符串 */GUID.prototype.hexadecimal = function (num, x, y) {
            if (y != undefined) { return parseInt(num.toString(), y).toString(x); }
            else { return parseInt(num.toString()).toString(x); }
        }
        /* * 功能：格式化32位的字符串为GUID模式的字符串 * 参数：第1个参数表示32位的字符串 * 返回值：标准GUID格式的字符串 */
        GUID.prototype.formatGUID = function (guidStr) {
            var str1 = guidStr.slice(0, 8) + '-', str2 = guidStr.slice(8, 12) + '-', str3 = guidStr.slice(12, 16) + '-', str4 = guidStr.slice(16, 20) + '-', str5 = guidStr.slice(20);
            return str1 + str2 + str3 + str4 + str5;
        }
    }
};
GUID();

/**
 * 跳转到添加就诊人的页面
 * @param {any} url 添加就诊人页面的url
 */
function ShowToAddPat(url) {
    weui.confirm("您的信息为空，请先绑定信息。", function () {
        if (IsNullOrEmpty(url)) {
            url = "../AddPatientInfo.html";
        }
        window.location.href = url;
    }, function () {
        window.history.go(-1);
    })
}

