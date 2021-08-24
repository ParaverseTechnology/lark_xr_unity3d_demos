using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ExtraParam : MonoBehaviour
{
    // 网页进入应用
    // 如：http://192.168.0.55:8181/appli/start?appliId=879408743551336448&extraParam.userId=12345&extraParam.userName=%E5%BC%A0%E4%B8%89

    // 请在云雀后台上传应用时设置《接口调用是否附加参数》为<<是>>
    // 附加参数 taskId 将以传递给命令行最后一位
    // 如果 taskId 未设置或不正确将无法获取附加参数
    public Text taskIDText;

    // 请求后台的 uri
    // 在Lark平台上启动后可通过如下接口获取附加参数
    // http://localhost:8089/taskInfo/getExtraParams?taskId=[taskId]
    public InputField requestUri;

    // 显示结果
    public Text resultText;

    private UnityWebRequest www;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(taskIDText != null);
        Debug.Assert(requestUri != null);
        Debug.Assert(resultText != null);

        // 请在云雀后台上传应用时设置《接口调用是否附加参数》为<<是>>
        // 附加参数 taskId 将以传递给命令行最后一位
        // 如果 taskId 未设置或不正确将无法获取附加参数
        string taskId = "";
        try
        {
#if UNITY_EDITOR
            // 编辑器内使用测试 TaskID
            taskId = "123456";
#else
            // Task ID 应为一串数字
            long taskNumber = long.Parse(Environment.GetCommandLineArgs().Last());
            taskId = Environment.GetCommandLineArgs().Last();
#endif
            taskIDText.text += taskId;
        }
        catch (System.Exception e)
        {
            Debug.LogError("检测到taskID格式不正确，请在云雀后台设置使用附加参数 " + e.Message);
            resultText.text += "检测到taskID格式不正确，请在云雀后台设置使用附加参数 " + e.Message;
        }
        requestUri.text = "http://localhost:8089/taskInfo/getExtraParams?taskId=" + taskId;
        StartGetExtraParam();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGetExtraParam()
    {
        StartCoroutine("GetExtraParam");
    }

    IEnumerator GetExtraParam() {
        yield return Get(requestUri.text);
        if (www != null)
        {
            if (www.isNetworkError || www.isHttpError)
            {
                resultText.text = "请求失败: " + www.error;
            }
            else
            {
                resultText.text = "请求成功: " + www.downloadHandler.text;
            }
        }
    }

    IEnumerator Get(string uri)
    {
        try
        {
            www = UnityWebRequest.Get(uri);
        }
        catch (Exception e)
        {
            www = null;
            resultText.text = "解析Uri失败: " + e.Message;
        }
        if (www != null)
        {
            //www.timeout = 1;
            yield return www.SendWebRequest();
        }
    }
}
