/**
 * 获取就诊人信息
 * @param {any} jzrid 就诊人id
 * @param {any} fuc 回调函数
 */
function SelectPatient(jzrid, fuc,completeFuc) {
    t_ajax("patient/SelectPatient", { "jzrid": jzrid }, function (data) {
        if (fuc != undefined && fuc != null) {
            fuc(data)
        }
    }, completeFuc)
}
/**
 * 获取住院号的请求
 * @param {any} hzid
 * @param {any} fuc
 * @param {any} completeFuc
 */
function SelectCardInHospital( hzid, fuc, completeFuc) {
    t_ajax('BeInHospital/SelectInHospitalInfo', { yyid: Setting.HospitalId, hzid: hzid }, function (data) {
        if (fuc != undefined && fuc != null) {
            fuc(data)
        }
    }, completeFuc)
}
