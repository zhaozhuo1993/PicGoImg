//支付成功跳转成功页面
function onBridgeReady(appId, timeStamp, nonceStr, packages, signType, paySign, success_url) {
    WeixinJSBridge.invoke(
        'getBrandWCPayRequest', {
        "appId": appId,
        "timeStamp": timeStamp,
        "nonceStr": nonceStr,
        "package": packages,
        "signType": signType,
        "paySign": paySign
    },
        function (res) {
            //get_brand_wcpay_request:ok  支付成功
            //get_brand_wcpay_request:cancel 支付过程中用户取消
            //get_brand_wcpay_request:fail 支付失败
            if (res.err_msg == "get_brand_wcpay_request:ok") {
                if (success_url == undefined || success_url == "")
                    weui.alert("支付成功");
                else
                    window.location.href = success_url;
            }
            // 使用以上方式判断前端返回,微信团队郑重提示：res.err_msg将在用户支付成功后返回 ok，但并不保证它绝对可靠。 
        }
    );
}

/**
	 * toast 微信支付
	 * @param {string} openid 用户微信唯一识别码
	 * @param {number} amount 金额
	 * @param {string} paytype 交易信息
	 * @param {string} attach json字符串，传卡号、患者ID、就诊人等信息
	 * @param {string} msgtitle 提示信息标题
	 * @param {string} msgcontent 提示信息
	 * @param {string} success_url 支付成功之后跳转页面
	 *
	 */
function Pay(openid, amount,paytype, attach, msgtitle, msgcontent, success_url) {
    if (isNaN(amount)) {
        weui.alert("请输入正确的充值金额");
        return;
    }
    if (typeof WeixinJSBridge == "undefined") {
        if (document.addEventListener) {
            document.addEventListener('WeixinJSBridgeReady', onBridgeReady, false);
        } else if (document.attachEvent) {
            document.attachEvent('WeixinJSBridgeReady', onBridgeReady);
            document.attachEvent('onWeixinJSBridgeReady', onBridgeReady);
        }
    }

    weui.confirm(msgcontent, function () {
        //确定支付
        //var loading = weui.loading("支付中...");
       
        t_ajax('Pay/Pay', { openid: openid, total_fee: amount, payDesc: paytype, attach: attach, yyid: Setting.HospitalId, kfptlx: Setting.AppType }, function (res) {
            //loading.hide();
            if (res.IsSuccess) {
                if (res.Data != "" && res.Data != "{}") {
                    var obj = JSON.parse(res.Data);
                    onBridgeReady(obj.appId, obj.timeStamp, obj.nonceStr, obj.package, obj.signType, obj.paySign, success_url);
                }
                else
                    weui.toast('微信支付失败', 2000);
            }
            else
                weui.alert(res.Data);
        });
    }, function () {
            //取消支付
        weui.toast('您已取消支付', 1000);
    }, {
        title: msgtitle
    });
}