/**
 * wrap common iframe poster functions
 * use: 
 *       var poster = new lark.iframePoster(element, config);
 *  element: iframeElement htmlelement
 *  config: {
 *    listenKeyboard: bool
 *    onMessage: callback
 *  }
 */
var lark = (function() {
    // event types.
    var EventTypes = {
        // 进入应用接口调用
        LK_API_ENTERAPPLI_SUCCESS                        : 10,
        LK_API_ENTERAPPLI_FAILED                         : 11,
        // 同步应用中
        LK_TASK_SYNC_APP                                 : 20,
        //
        // 连接应用服务器事件。直连渲染服务器时抛出
        // 消息来源：websocket 连接事件
        //
        // 连接渲染服务器成功
        LK_RENDER_SERVER_CONNECTED                       : 100,
        // 连接渲染服务器失败
        LK_RENDER_SERVER_FAILED                          : 101,
        // 与渲染服务器连接关闭
        LK_RENDER_SERVER_CLOSE                           : 102,
        // 与渲染服务器连接出错
        LK_RENDER_SERVER_ERROR                           : 103,

        //
        // 连接 websocket 代理服务器事件
        // 消息来源：websocekt proxy 连接事件
        //
        // 连接代理服务器成功
        LK_PROXY_SERVER_CONNECTED                        : 200,
        // 连接代理服务器失败
        LK_PROXY_SERVER_FAILED                           : 201,
        // 与代理服务器连接关闭
        LK_PROXY_SERVER_CLOSE                            : 202,
        // 与代理服务器连接出错
        LK_PROXY_SERVER_ERROR                            : 203,

        //
        // 版本检测 
        // 消息来源：服务端协议返回 ToClientMessage->VersionCheckResponse
        //
        // 版本检测成功
        LK_VERSION_CHECK_SUCCESS                         : 300,
        // 版本检测失败
        LK_VERSION_CHECK_FAILED                          : 301,

        //
        // task 检测 
        // 消息来源： 服务端协议返回 ToClientMessage->TaskResponse
        //
        // 请求 Task 成功
        LK_TASK_SUCCESS                                  : 400,
        // 未发现 Task
        LK_TASK_NOTFOUND                                 : 401,
        // 服务器端错误
        LK_TASK_SERVER_ERROR                             : 402,
        // 应用参数错误
        LK_TASK_APP_WRONGPARAM                           : 403,
        // Task 获取成功，但是没有可分配的显卡
        LK_TASK_NO_GPU_RESOURCE                          : 404,

        //
        // 启动流媒体
        // 消息来源： 服务端协议返回 ToClientMessage->StartStreamResponse
        //
        // 启动流媒体成功
        LK_START_STREAM_SUCCESS                           : 500,
        // 启动流媒体出错
        LK_START_STREAM_PROCESS_START_FAILED              : 501,
        // 启动流媒体超时
        LK_START_STREAM_PROCESS_START_TIMEOUT             : 502,
        // 启动流媒体未串流
        LK_START_STREAM_NOT_STREAMING                     : 503,
        // 启动流媒体编码错误
        LK_START_STREAM_ENCODER_ERROR                     : 504,

        //
        // RTC 事件
        // 消息来源：webrtc 连接事件和 ToClientMessage->WebrtcError
        //
        // RTC 连接成功
        LK_RTC_EVENT_PEERCONNECTION_CONNECTED             : 600,
        // RTC 连接关闭
        LK_RTC_EVENT_PEERCONNECTION_CLOSED                : 601,
        // RTC 连接出错
        LK_RTC_EVENT_PEERCONNECTION_ERROR                 : 602,
        // RTC 状态
        // 3.1.1.8 新增
        LK_RTC_EVENT_PEERCONNECTION_STATE                 : 610,

        //
        // 加载视频流
        // 消息来源：浏览器或原生组件
        //
        LK_VIDEO_LOADED                                  : 700,

        //
        // 服务端主动退出
        // 消息来源：后台协议 NotifyClientLogout
        //
        LK_NOTIFY_CLIENT_LOGOUT_PLAYER_LOGOUT            : 800,
        // 一人操作多人看房主退出
        LK_NOTIFY_CLIENT_LOGOUT_TASKOWNER_LOGOUT         : 801,

        //
        // 服务端推送云端应用事件
        // 消息来源：后台协议 AppProcessNotification
        //
        LK_APP_PROCESS_NOTIFI_APP_QUIT                   : 900,
        // 云端应用大小变换
        // 3.1.1.8 新增
        LK_APP_RESIZE                                    : 910,
        // 云端应用鼠标模式变化
        // 3.1.1.8 新增
        LK_APP_MOUSE_MODE                                : 911,
        // 3.1.1.10 新增
        LK_APP_PLAER_LIST                                : 912,
        // APP 请求输入文字
        LK_APP_REQUEST_TEXT                              : 913,
        // 用户主动点击关闭按钮
        LK_USER_REQUEST_QUIT                             : 920,
        // UI resize
        LK_UI_RESIZE                                     : 930,

        //
        // XR 相关事件
        //
        // 启动VR流媒体启动成功
        LK_STARTVRSTREAM_SUCCESS                            : 1000,
        // 启动VR流媒体过程失败
        LK_STARTVRSTREAM_START_PROCESS_FAILED               : 1001,
        // 启动VR流媒体驱动超时
        LK_STARTVRSTREAM_START_DRIVER_RUNTIME_TIMEOUT       : 1002,
        // 启动VR流媒体 udp 端口出错
        LK_STARTVRSTREAM_START_DRIVER_RUNTIME_UDPPORT_ERROR : 1003,
        // 启动VR流媒体 udp 编码出错
        LK_STARTVRSTREAM_START_DRIVER_RUNTIME_ENCODER_ERROR : 1004,


      //
        // iframe 外部发送给 web 客户端消息
        //
        // 操作
        LK_IFRAME_POSTER_OPERATE_MOUSE_MOVE               : 10000,
        LK_IFRAME_POSTER_OPERATE_MOUSE_DOWN               : 10001,
        LK_IFRAME_POSTER_OPERATE_MOUSE_UP                 : 10002,
        LK_IFRAME_POSTER_OPERATE_MOUSE_WHEEL              : 10003,
        LK_IFRAME_POSTER_OPERATE_KEY_DOWN                 : 10010,
        LK_IFRAME_POSTER_OPERATE_KEY_UP                   : 10011,
        // 功能
        LK_IFRAME_POSTER_FUNC_MOUSE_MODE                  : 10100,
        LK_IFRAME_POSTER_FUNC_SCALE_MODE                  : 10101,
        // 控制 ui
        // 是否显示桌面端控制栏
        LK_IFRAME_POSTER_UI_CONTROLLER_BAR                : 10200,
        // 是否显示玩家列表
        LK_IFRAME_POSTER_UI_PLAYER_LIST                   : 10201,
        // 是否显示分享模式下分享 url
        LK_IFRAME_POSTER_UI_PLAYER_SHARE_URL              : 10202,
        // 是否显示手机端控制球
        LK_IFRAME_POSTER_UI_MOBILE_CONTROL_BALL           : 10203,
        // 是否显示手机端摇杆
        LK_IFRMAE_POSTER_UI_MOBILE_JOYSTICK               : 10204,
        // 是否显示手机端虚拟键盘
        LK_IFRAME_POSTER_UI_MOBILE_VIRTUAL_KEYBOARD       : 10205,
        // 是否显示手机端虚拟鼠标
        LK_IFRAME_POSTER_UI_MOBILE_VIRTUAL_MOUSE          : 10206,
        // 是否显示手机端菜单
        LK_IFRAME_POSTER_UI_MOBILE_MENU                   : 10207,
        // 是否手机端强制横屏
        LK_IFRAME_POSTER_UI_MOBILE_FORCE_LANDSCAPE        : 10208,
        //iframe 外部发送给 web 客户端消息结束

        //
        // iframe 外部接收 web 客户端消息 from datachannel 
        //
        LK_DATA_CHANNEL_ESTABLISHED                        : 20000,
        LK_DATA_CHANNEL_RETYING                            : 20001,
        LK_DATA_CHANNEL_CLOSE                              : 20002,
        LK_DATA_CHANNEL_ERROR                              : 20003,
        LK_DATA_CHANNEL_BINARY_MESSAGE                     : 20004,
        LK_DATA_CHANNEL_TEXT_MESSAGE                       : 20005,
        //
        // iframe外部发送给 web 客户端 to datachannel 
        //
        LK_RE_CONNECT_DATA_CHANNEL                         : 20100,
        LK_CLOSE_DATA_CHANNEL                              : 20101,
        LK_DATA_CHANNEL_SEND_TEXT                          : 20102,
        LK_DATA_CHANNEL_SEND_BINARY                        : 20103,
        //
        // iframe 外部接收 web 客户端消息  from datachannel-renderserver
        //
        LK_DATA_CHANNEL_RENDERSERVER_OPEN                  : 20200,
        LK_DATA_CHANNEL_RENDERSERVER_CLOSE                 : 20201,
        LK_DATA_CHANNEL_RENDERSERVER_BINARY_MESSAGE        : 20202,
        LK_DATA_CHANNEL_RENDERSERVER_TEXT_MESSAGE          : 20203,
        //
        // iframe 外部发送给 web 客户端 to datachannel-renderserver
        //
        LK_DATA_CHANNEL_RENDERSERVER_SEND_TEXT            : 20300,
        LK_DATA_CHANNEL_RENDERSERVER_SEND_BINARY          : 20301,
    };

    /**
     * 
     * @param {*} iframeElement htmlelement
     * @param {
     *  listenKeyboard: boolean, if listen keyboard function. default true
     *  onMessage: onMessage(e), handle message
     * } config 
     */
    var iframePoster = function(iframeElement, config) {
        var element = iframeElement;
        if (!element) {
            console.error('please set iframe element.');
            return null;
        }

        if (!config) {
            config = {
                listenKeyboard: true,
            }
        } else {
            if (!config.hasOwnProperty('listenKeyboard')) {
                config.listenKeyboard = true;
            }
        }

        window.addEventListener("message", function(e) {
            if (config.onMessage && typeof config.onMessage == 'function') {
                config.onMessage(e);
            }
        }, false);

        function sendToIframe(type, data, message) {
            if (element.contentWindow) {
                var win = element.contentWindow;
                win.postMessage({
                    prex: "pxymessage", // 约定的消息头部
                    type: type,         // 消息类型
                    data: data,         // 具体数据
                    message: message,   // 附加信息
                },'*');
            } else {
                console.warn('content window not find.');
            }
        }

        if (config.listenKeyboard) {
            document.addEventListener("keydown", function(e) {
                sendToIframe(EventTypes.LK_IFRAME_POSTER_OPERATE_KEY_DOWN, {
                    key: e.code,
                    isRepeat: e.repeat,
                }, "");
            });
            // window key eventup
            document.addEventListener("keyup", function(e) {
                sendToIframe(EventTypes.LK_IFRAME_POSTER_OPERATE_KEY_UP, {
                    key: e.code,
                }, "");
            });
        }

        /**
         * 发送键盘按键按下
         * @param {string} key key code 为浏览器 keydown 事件 code 字段，如 KeyW/KeyA/KeyS/KeyD
         * @param {boolean} isRepeat 是否长按按键，浏览器 keydown 事件 repeat 字段
         */
        function sendKeyDown(key, isRepeat) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_OPERATE_KEY_DOWN, {
                key,
                isRepeat,
            }, "");
        }

        /**
         * 发送键盘按键抬起
         * @param {string} key key code 为浏览器 keyup 事件 code 字段
         */
        function sendKeyUp(key) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_OPERATE_KEY_UP, {
                key,
                isRepeat: false,
            }, "");
        }

        /**
         * 发送鼠标滚轮向上滚动
         * @param {number} x 鼠标位置 x
         * @param {number} y 鼠标位置 y
         */
        function sendWheelUp(x, y) {
            // windows 消息 deltaY 上是 +120 下是 -120
            sendToIframe(EventTypes.LK_IFRAME_POSTER_OPERATE_MOUSE_WHEEL, {
                x: x,
                y: y,
                wheel: 120,
            }, "");
        }

        /**
         * 发送鼠标滚轮向下滚动
         * @param {*} x 鼠标位置 x
         * @param {*} y 鼠标位置 y
         */
        function sendWheelDown(x, y) {
            // windows 消息 deltaY 上是 +120 下是 -120
            sendToIframe(EventTypes.LK_IFRAME_POSTER_OPERATE_MOUSE_WHEEL, {
                x: x,
                y: y,
                wheel: -120,
            }, "");
        }

        /**
         * 发送鼠标移动
         * @param {number} x 鼠标绝对位置（相对云端应用）
         * @param {number} y 鼠标绝对位置（相对云端应用）
         * @param {number} rx 鼠标移动相对位置
         * @param {number} ry 鼠标移动相对位置
         */
        function sendMouseMove(x, y, rx, ry) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_OPERATE_MOUSE_MOVE, {
                x: x,
                y: y,
                rx: rx,
                ry: ry,
            }, "");
        }

        /**
         * 发送鼠标按下
         * @param {string} button 鼠标按键的值，为： left/right/mid
         * @param {number} x 鼠标的绝对位置（相对云端应用）
         * @param {number} y 鼠标的绝对位置（相对云端应用）
         */
        function sendMouseDown(button, x, y) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_OPERATE_MOUSE_DOWN, {
                button: button,
                x: x,
                y: y,
            }, "");
        }

        /**
         * 发送鼠标抬起
         * @param {string} button button 鼠标按键的值，为： left/right/mid
         * @param {number} x 鼠标的绝对位置（相对云端应用）
         * @param {number} y 鼠标的绝对位置（相对云端应用）
         */
        function sendMouseUp(button, x, y) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_OPERATE_MOUSE_UP, {
                button: button,
                x: x,
                y: y,
            }, "");
        }
        
        /**
         * 设置视频显示的缩放模式
         * @param {string} mode 缩放模式值为： fit/cover/contain/fill_stretch
         */
        function setScaleMode(mode) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_FUNC_SCALE_MODE, mode, "");
        }

        /**
         * 设置鼠标模式，是否时锁定模式
         * @param {string} mode "true"/"false" 鼠标模式 true 为锁定模式，false 为自动判断模式
         */
        function setMouseMode(mode) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_FUNC_MOUSE_MODE, mode, "");
        }

        /**
         * 设置控 pc 上制球是否显示
         * @param {string} show "true"/"false" 是否显示控制球
         */
        function setShowControlBall(show) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_UI_CONTROLLER_BAR, show, "");
        }

        /**
         * 互动模式时是否显示玩家列表
         * @param {string} show "true"/"false" 是否显示玩家列表
         */
        function setShowPlayerList(show) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_UI_PLAYER_LIST, show, "");
        }

        /**
         * 是否显示互动模式中分享连接
         * @param {string} show "true"/"false" 是否显示分享连接
         */
        function setShowPlayerListShareUrl(show) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_UI_PLAYER_SHARE_URL, show, "");
        }

        /**
         * 手机端是否显示控制球
         * @param {string} show "true"/"false" 手机端是否显示控制球
         */
        function setShowMobileControlBall(show) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_UI_MOBILE_CONTROL_BALL, show, "");
        }

        /**
         * 手机端是否显示摇杆
         * @param {string} show "true"/"false" 手机端是否显示摇杆
         */
        function setShowMobileJoystick(show) {
            sendToIframe(EventTypes.LK_IFRMAE_POSTER_UI_MOBILE_JOYSTICK, show, "");
        }

        /**
         * 手机端是否显示虚拟键盘
         * @param {string} show "true"/"false" 是否显示虚拟键盘
         */
        function setShowMobileKeyboard(show) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_UI_MOBILE_VIRTUAL_KEYBOARD, show, "");
        }

        /**
         * 手机端是否显示虚拟鼠标
         * @param {string} show "true"/"false" 是否显示虚拟鼠标
         */
        function setShowMobileVritualMouse(show) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_UI_MOBILE_VIRTUAL_MOUSE, show, "");
        }

        /**
         * 手机端是否显示菜单栏
         * @param {string} show "true"/"false" 是否显示菜单栏
         */
        function setShowMobileMenu(show) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_UI_MOBILE_MENU, show, "");
        }

        /**
         * 移动端是否强制横屏
         * @param {string} force "true"/"false" 是否强制横屏
         */
        function setMobileForcelandscape(force) {
            sendToIframe(EventTypes.LK_IFRAME_POSTER_UI_MOBILE_FORCE_LANDSCAPE, force, "");
        }

        /**
         * @deprecated
         * 向数据通道服务发送字符串消息。
         * 需要打开数据通道服务后才能正常使用
         * @param {string} str 发送的消息
         */
        function sendTextToDataChannel(str) {
            sendToIframe(EventTypes.LK_DATA_CHANNEL_SEND_TEXT, str, "");
        }

        /**
         * @deprecated
         * 向数据通道服务发送字符串消息。
         * 需要打开数据通道服务后才能正常使用
         * @param {Uint8Array} binary 字节消息
         */
        function sendBinaryToDataChannel(binary) {
            sendToIframe(EventTypes.LK_DATA_CHANNEL_SEND_BINARY, binary, "");
        }

        /**
         * 向数据通道服务发送字符串消息。
         * 需要打开数据通道服务后才能正常使用
         * {string} str 发送的消息
         */
        function sendTextToRenderSererAppDataChannel(str) {
            sendToIframe(EventTypes.LK_DATA_CHANNEL_RENDERSERVER_SEND_TEXT, str, "");
        }

        /**
         * 向数据通道服务发送字符串消息。
         * 需要打开数据通道服务后才能正常使用
         * @param {Uint8Array} binary 字节消息
         */
        function sendBinaryToRenderServerAppDataChannel(binary) {
            sendToIframe(EventTypes.LK_DATA_CHANNEL_RENDERSERVER_SEND_BINARY, binary, "");
        }

        var poster = {};
        poster.sendToIframe = sendToIframe;
        poster.sendKeyDown = sendKeyDown;
        poster.sendKeyUp = sendKeyUp;
        poster.sendWheelUp = sendWheelUp;
        poster.sendWheelDown = sendWheelDown;
        poster.sendMouseMove = sendMouseMove;
        poster.sendMouseDown = sendMouseDown;
        poster.sendMouseUp = sendMouseUp;
        poster.setScaleMode = setScaleMode;
        poster.setMouseMode = setMouseMode;
        poster.setShowControlBall = setShowControlBall;
        poster.setShowPlayerList = setShowPlayerList;
        poster.setShowPlayerListShareUrl = setShowPlayerListShareUrl;
        poster.setShowMobileControlBall = setShowMobileControlBall;
        poster.setShowMobileJoystick = setShowMobileJoystick;
        poster.setShowMobileKeyboard = setShowMobileKeyboard;
        poster.setShowMobileVritualMouse = setShowMobileVritualMouse;
        poster.setShowMobileMenu = setShowMobileMenu;
        poster.setMobileForcelandscape = setMobileForcelandscape;
        poster.sendTextToDataChannel = sendTextToDataChannel;
        poster.sendBinaryToDataChannel = sendBinaryToDataChannel;
        poster.sendTextToRenderSererAppDataChannel = sendTextToRenderSererAppDataChannel;
        poster.sendBinaryToRenderServerAppDataChannel = sendBinaryToRenderServerAppDataChannel;
        return poster;
    };

    var exp = {};
    exp.EventTypes = EventTypes;
    exp.iframePoster = iframePoster;
    return exp;
})();
