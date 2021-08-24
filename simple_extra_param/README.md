# 网页进入应用连接

网页进入应用
如：http://192.168.0.55:8181/appli/start?appliId=879408743551336448&extraParam.userId=12345&extraParam.userName=%E5%BC%A0%E4%B8%89

# 配置

请在云雀后台上传应用时设置《接口调用是否附加参数》为<<是>>
附加参数 taskId 将以传递给命令行最后一位
如果 taskId 未设置或不正确将无法获取附加参数

# 应用中请求的接口地址

在Lark平台上启动后可通过如下接口获取附加参数
http://localhost:8089/taskInfo/getExtraParams?taskId=[taskId]
