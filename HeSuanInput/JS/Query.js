/**
     * 获取 localStorage中的openid
     * @param callback
     */
window.GetOpenId_L = function (callback) {
    if (localStorage.getItem("openid") == this.undefined ||
        localStorage.getItem("openid") == null ||
        localStorage.getItem("openid") == "" ||
        localStorage.getItem("openid") == "null" ) {
        let json = {
            "sign": "GetOpeinid",
            "code": GetQueryString("code")
        };

        $.showLoading("正在加载...");
        $.ajax({
            url: "../../CTL/Query.ashx",
            type: "POST",
            data: json,
            dataType: "json",
            success: function (json) {
                $.hideLoading();
                if (json.code == "0")
                    weui.alert(json.msg);
                else {
                    localStorage.setItem("openid", json.msg)
                }
                callback();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $.hideLoading();
                alert(XMLHttpRequest.status);
                alert(XMLHttpRequest.readyState);
                alert(textStatus);
            }
        }); 
    }
    else {
        callback();
    }
}
/**
 * 获取路径中的参数
 * @param name
 */
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) {
        return unescape(r[2]);
    }
    return null;
}



/**
 * 字符串中间***替换 敏感信息防泄漏
 * @param {any} str
 * @param {any} frontLen 前面留的长度
 * @param {any} endLen 后面留的长度
 */
function sub_qm(str, frontLen, endLen) {
    var len = str.length - frontLen - endLen;
    var xing = '';
    for (var i = 0; i < len; i++) {
        xing += '*';
    }
    return str.substring(0, frontLen) + xing + str.substring(str.length - endLen);
}
/**
 * 去除敏感信息
 * @param {any} str 要处理的字符串
 * @param {any} type 1 证件号，2姓名
 */
function str_qm(str,type) {
    if (type == 1) {
        return sub_qm(str, 10, 2);
    } else if (type == 2) {
        return sub_qm(str, 0, str.length - 1);
    } else {
        return str;
    }
}

/**
 * 左侧补0方法
 * @param {any} str 需要修改的字符串
 * @param {any} lenght  最后的字符串长度
 */
function padLeft(str, lenght) {
    if (str.length >= lenght)
        return str;
    else
        return padLeft("0" + str, lenght);
}