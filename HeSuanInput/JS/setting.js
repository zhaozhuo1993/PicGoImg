var Setting = {
    HospitalId: "042693131082410141",//医院唯一标识:027154130226510151
    ServerPath: "https://www.bjzhenyuankeji.com/api/",//服务地址:https://www.bjzhenyuankeji.com/  http://182.92.74.187:5018/api/
    SocketPath: "https://www.bjdingjikeji.com:3001/",//即时通讯服务器地址:https://www.bjdingjikeji.com:3001/
    AppType: "01",//平台类型
    MsgType: {//消息类型
        //（01图文、02文字、03图片、04电话、05视频）
        ImgText: "01",
        Text: "02",
        Img: "03",
        Telephone: "04",
        Video: "05",
        File: "06"
    },
    MsgerType: {//消息发送者类型
        //（01医生 02用户）
        Doctor: "01",
        User: "02"
    }
}

