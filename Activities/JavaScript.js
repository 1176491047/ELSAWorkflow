var message = "halm时段(每小时) 同一不良超过20片推送 \n";
var datas = JSON.parse(input.response);
datas.data.forEach((dataObject) => {
  const count = dataObject.count;
  const datetime = dataObject.datetime;
  const lineName = dataObject.line_name;
    const typeFullName = dataObject.type_full_name;
    const tracingresult = dataObject.tracingresult;

// 匹配日期部分（yyyy-MM-dd）
const dateRegex = /^(\d{4}-\d{2}-\d{2})/;

// 匹配时间部分（HH:mm:ss）
const timeRegex = /(\d{2}:\d{2}:\d{2})$/;

const datePart = datetime.match(dateRegex)[1];
const timePart = datetime.match(timeRegex)[1];


  // Formatting the values for each data object
    const formattedData = `**${lineName}** ${datePart} ${timePart} **${typeFullName}**; 数量：${count} \n`;
    formattedData += "详情: \n";
    tracingresult.forEach((da) => {
        const detail = `工艺段[${da.ProcessUnitName}]设备[${da.EquipmentName}]数量[${da.Count}]占比[${da.Precent}]% \n`;
        formattedData += detail;
    });
    message += formattedData +"\n";
});
return message;

function isUnicode(value) {
    // 检查值是否是字符串类型
    if (typeof value !== 'string') {
        return false;
    }

    // 检查字符串是否只包含数字
    if (!/^\d+$/.test(value)) {
        return false;
    }

    // 将字符串转换为Unicode字符
    var unicodeChar = String.fromCharCode(parseInt(value, 10));

    // 检查转换后的字符是否是中文字符
    if (/[\u4e00-\u9fa5]/.test(unicodeChar)) {
        return unicodeChar;
    } else {
        return false;
    }
}


datas.data.forEach((dataObject) => {
    const count = dataObject.count;
    const datetime = dataObject.datetime;
    const lineName = dataObject.line_name;
    const typeFullName = dataObject.type_full_name;
    const tracingresult = dataObject.tracingresult;

    // 匹配日期部分（yyyy-MM-dd）
    const dateRegex = /^(\d{4}-\d{2}-\d{2})/;

    // 匹配时间部分（HH:mm:ss）
    const timeRegex = /(\d{2}:\d{2}:\d{2})$/;

    const datePart = datetime.match(dateRegex)[1];
    const timePart = datetime.match(timeRegex)[1];


    // Formatting the values for each data object
    const formattedData = `**${lineName}** ${datePart} ${timePart} **${typeFullName}**; 数量：${count} \n`;
    formattedData += "详情: \n";
    tracingresult.forEach((da) => {
        var processName = isUnicode(da.ProcessUnitName);
        if (!processName) {
            processName = da.ProcessUnitName;
        }
        const detail = `工艺段[${da.ProcessUnitName}]设备[${da.EquipmentName}]数量[${da.Count}]占比[${da.Precent}]% \n`;
        formattedData += detail;
    });
    message += formattedData + "\n";
});


var i = 0;
var message = "halm时段(每小时) 同一不良超过20片推送 \n";
var datas = JSON.parse(input.response);
setVariable("datss", datas);
datas.data.forEach((dataObject) => {
    i++;
    setVariable(`key${i}`, dataObject);
    const allcount = dataObject.count;
    const datetime = dataObject.datetime;
    const lineName = dataObject.line_name;
    const typeFullName = dataObject.type_full_name;
    const tracingresult = dataObject.tracingresult;

    //// 匹配日期部分（yyyy-MM-dd）
    //const dateRegex = /^(\d{4}-\d{2}-\d{2})/;

    //// 匹配时间部分（HH:mm:ss）
    //const timeRegex = /(\d{2}:\d{2}:\d{2})$/;

    //const datePart = datetime.match(dateRegex)[1];
    //const timePart = datetime.match(timeRegex)[1];


    // Formatting the values for each data object
    var formattedData = `**${lineName}**  **${typeFullName}**; 数量：${allcount} \n`;
    formattedData += "详情: \n";
    tracingresult.forEach((da) => {
        const detail = `工艺段【${da.ProcessUnitName}】设备【${da.EquipmentName}】数量【${da.Count}】占比【${da.Precent}%】 \n`;
        formattedData += detail;
    });
    message += formattedData + "\n";
});



return message;



var message = "halm时段(每小时) 同一不良超过20片推送 \n";
var datas = JSON.parse(input.response);
datas.data.forEach((dataObject) => {
    const allcount = dataObject.count;
    const datetime = dataObject.datetime;
    const lineName = dataObject.line_name;
    const typeFullName = dataObject.type_full_name;
    const tracingresult = dataObject.tracingresult;

    // 匹配日期部分（yyyy-MM-dd）
    const dateRegex = /^(\d{4}-\d{2}-\d{2})/;

    // 匹配时间部分（HH:mm:ss）
    const timeRegex = /(\d{2}:\d{2}:\d{2})$/;

    const datePart = datetime.match(dateRegex)[1];
    const timePart = datetime.match(timeRegex)[1];


    // Formatting the values for each data object
    const formattedData = `**${lineName}** ${datePart} ${timePart} **${typeFullName}**; 数量：${allcount} \n`;
    formattedData += "详情: \n";
    tracingresult.forEach((da) => {
        const detail = `工艺段【${da.ProcessUnitName}】设备【${da.EquipmentName}】数量【${da.Count}】占比【${da.Precent}%】 \n`;
        formattedData += detail;
    });
    message += formattedData + "\n";
});
return message;


var message = "halm时段(每小时) 同一不良超过20片推送 \n";
var datas = JSON.parse(input.response);
datas.data.forEach((dataObject) => {
    const allcount = dataObject.count;
    const datetime = dataObject.datetime;
    const lineName = dataObject.line_name;
    const typeFullName = dataObject.type_full_name;
    const tracingresults = dataObject.tracingresult;

    //// 匹配日期部分（yyyy-MM-dd）
    //const dateRegex = /^(\d{4}-\d{2}-\d{2})/;

    //// 匹配时间部分（HH:mm:ss）
    //const timeRegex = /(\d{2}:\d{2}:\d{2})$/;

    //const datePart = datetime.match(dateRegex)[1];
    //const timePart = datetime.match(timeRegex)[1];


    //// Formatting the values for each data object
    //const formattedData = `**${lineName}** ${datePart} ${timePart} **${typeFullName}**; 数量：${allcount} \n`;
    formattedData += "详情: \n";
    tracingresults.forEach((da) => {
        const detail = `工艺段【${da.ProcessUnitName}】设备【${da.EquipmentName}】数量【${da.Count}】占比【${da.Precent}%】 \n`;
        formattedData += detail;
    });
    message += formattedData + "\n";
});
return message;

var i = 0;
var message = "halm时段(每小时) 同一不良超过20片推送 <br/>";
var datas = JSON.parse(input.response);
setVariable("datss", datas);
datas.data.forEach((dataObject) => {
    i++;
    setVariable(`key${i}`, dataObject);
    var allcount = dataObject.count;
    var datetime = dataObject.datetime;
    var lineName = dataObject.line_name;
    var typeFullName = dataObject.type_full_name;
    var tracingresult = dataObject.tracingresult;

    //// 匹配日期部分（yyyy-MM-dd）
    //const dateRegex = /^(\d{4}-\d{2}-\d{2})/;

    //// 匹配时间部分（HH:mm:ss）
    //const timeRegex = /(\d{2}:\d{2}:\d{2})$/;

    //const datePart = datetime.match(dateRegex)[1];
    //const timePart = datetime.match(timeRegex)[1];


    // Formatting the values for each data object
    var formattedData = `****${lineName}${datetime} ${typeFullName} 数量：${allcount}**** <br/>`;
    formattedData += "详情:  <br/>";
    tracingresult.forEach((da) => {
        var detail = `工艺段： ${da.ProcessUnitName}设备：${da.EquipmentName} 数量：${da.Count} 占比：${da.Precent}%   <br/>`;
        formattedData += detail;
    });
    message += formattedData + " <br/>";
});



return message;