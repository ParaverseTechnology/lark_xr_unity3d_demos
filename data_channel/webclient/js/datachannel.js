// Lark Server 访问地址
var serverAddr = "192.168.31.120:8585";

var config = {
	userLocalIP: true,
	server: "http://" + serverAddr + "/", // server
	webclient: "http://" + serverAddr + "/webclient", // client
    // webclient: "http://192.168.0.122:8080/", // debug client
    // testAppId: "745612252752642048",
	testAppId: "879819862925377536",
	// test appurl
	// testAppUrl: "http://127.0.0.1:8080/cloudlark/webclient/#/?appServer=192.168.0.223&appPort=10002&taskId=123456&debugTask=true&logLevel=info&",
}


$(document).ready(function() {
    // 进入应用应用列表中获取的第一个应用。
    $("#enter").on("click", function(e) {
        if (!config.server) {
            alert("请设置 config.server");
            return;
		}
		if (config.testAppUrl) {
			$("#iframe").attr("src", config.testAppUrl);
		} else if (config.testAppId) {
            enterApp(config.testAppId);
        } else {
            $.get(config.server + "getAppliList", function(res) {
                if (res && res.code === 1000) {
                    console.log("load list success", res.result.records);
                    if (res.result.records && res.result.records.length > 0) {
                        enterApp(res.result.records[0].appliId);
                    } else {
                        console.log("empty list. please use create server ip.");
                    }
                } else {
                    console.warn("load list failed", res);
                }
            });
        }
    })
    // 关闭应用
    $("#close").on("click", function(){
		$("#iframe").attr("src", "");
    });
    function enterApp(appliId) {
		console.log("enter appli:", config.server + "getEnterAppliInfo?appliId=" + appliId);
		$.get(config.server + "getEnterAppliInfo?appliId=" + appliId, function(res){
			console.log("enter appli res:", res, joinParam(res.result));
			if (res && res.code == 1000) {
				$("#iframe").attr("src", config.webclient + "?" + joinParam(res.result));
			}
		})
    }
    
    function joinParam(params){
	    var res = '';
	    for (const i in params) {
	        if (i) {
	            res += i + '=' + params[i] + '&';
	        }
	    }
	    return res;
    };

    // iframe websocket test
	(function() {
		var poster = new lark.iframePoster($("#iframe").get(0), {
            onMessage: onMessage,
            listenKeyboard: true,
        })
		// 监听消息
		function onMessage(e) {
			switch(e.data.type) {
				// open
				case 20200:
					console.log("通道开启", e.data.data);
					$("#receive").text("通道开启");
					// 同步客户端窗口大小
					syncClientWindowSize();
					startSendClientWindowSize();
					break;
				case 20201:
					console.log("通道关闭", e.data.data);
					$("#receive").text("通道关闭");
					break;
				// 接收到字节消息
				case 20202:
					console.log("接收到字节消息", e.data.data);
					$("#receive").text(e.data.data);
					break;
				// 接收到文本消息
				case 20203:
					console.log("接收到文本消息", e.data.data);
					handleJsonCmd(e.data.data);
					break;
				default:
					// console.log("receive message." + e.data.prex, e.data.type, e.data.message, e.data.data);
					break;
			}
		};

		// 发送字符消息。
		function sendText(jsonStr) {
			poster.sendTextToRenderSererAppDataChannel(jsonStr);
		}

		// 发送字节消息
		function sendBinary(binary) {	
			poster.sendBinaryToRenderServerAppDataChannel(binary);
		}

		$("#test-ws").on("click", function() {
			sendText($("#input").val());
		});

		$("#send-test-binary").on("click", function() {
            sendBinary(new Uint8Array([0x50, 0x58, 0x59, 0xf0]));
		});

		// test json cmdtype
		var JsonCmdType  = {
			CMD_CAMERA_LOADED: 1000,
			CMD_SWITCH_CAMERA: 1001,
	
			CMD_OBJECT_LOADED: 2001,
			CMD_OBJECT_PICKED: 2002,
			CMD_TOGGLE_OBJECT: 2003,

			CMD_WINDOW_RESIZE: 3001,
		};

		function handleJsonCmd(jsonStr) {
			$("#receive").text(jsonStr);
			try {
				var cmd = JSON.parse(jsonStr);
				switch(cmd.type) {
					case JsonCmdType.CMD_CAMERA_LOADED:
						// init camera list.
						// clear old
						$('#cameralist-iframe').empty();
						for (var i = 0; i < cmd.data; i++) {
							$("#cameralist-iframe").append(
								'<button class="btn btn-default camera"' + 
									'data-index="' + i + '">Switch To Camera ' + i + 
								'</button>'
							);
						}
						// switch object.
						$('#cameralist-iframe .camera').on('click', function(e) {
							var index = $(this).attr('data-index');
							console.log('switch to ' + index + ' camera');
							sendText(JSON.stringify({
								type: JsonCmdType.CMD_SWITCH_CAMERA,
								data: parseInt(index),
							}));
						});
						break;
					case JsonCmdType.CMD_OBJECT_LOADED:
						// init object list.
						// clear old
						$("#objectList-iframe").empty();
						for (var i = 0; i < cmd.data; i++) {
							$("#objectList-iframe").append(
								'<button class="btn btn-default object"' + 
									'data-index="' + i + '">Toggle Object ' + i + 
								'</button>'
							);
						}
						// toggle object.
						$('#objectList-iframe .object').on('click', function(e) {
							var index = $(this).attr('data-index');
							console.log('toggle ' + index + ' object');
							sendText(JSON.stringify({
								type: JsonCmdType.CMD_TOGGLE_OBJECT,
								data: parseInt(index),
							}));
						});
						break;
					default:
						console.log('got json cmd ' + jsonStr);
						break;
				}
			} catch(e) {
				console.warn('cmd parse failed.');
			}
		}

		var timeout = null;

		function startSendClientWindowSize() {
			window.addEventListener("resize", function() {
				if (timeout != null) {
					window.clearTimeout(timeout);
				}
				timeout = window.setTimeout(function() {
					syncClientWindowSize();
				}, 50);
			});
		}

		function syncClientWindowSize() {
			var size = {};
			if (document.compatMode === 'BackCompat') {
				size = {
					width: document.body.clientWidth,
					height: document.body.clientHeight
				};
			} else {
				size = {
					width: document.documentElement.clientWidth,
					height: document.documentElement.clientHeight
				};
			}
			sendText(JSON.stringify({
				type: JsonCmdType.CMD_WINDOW_RESIZE,
				data: 0,
				clientWidth: size.width,
				clientHeight: size.height,
			}));
		}
	})();
});