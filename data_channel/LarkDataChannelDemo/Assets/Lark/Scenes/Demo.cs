#define ENABLE_NAUDIO

using lark;
// 是否使用 Naudio 加载MP3
#if ENABLE_NAUDIO
using NAudio.Wave;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
    public Text taskIdText;
    public Text statusText;
    public InputField sendInput;
    public Text receiveText;

    public List<Camera> cameras;
    public List<GameObject> objects;

    public float sensitivityMouse = 2f;
    public float sensitivetyKeyBoard = 0.1f;
    public float sensitivetyMouseWheel = 10f;

    // 是否支持智能语音服务
    public bool supportAiVoice = true;

    private GetTaskInfo getTaskInfo = new GetTaskInfo();
    private GetExtraParams getExtraParams = new GetExtraParams();

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(taskIdText != null);
        Debug.Assert(statusText != null);
        Debug.Assert(sendInput != null);
        Debug.Assert(receiveText != null);

        // 异步获取。
        // 注意添加 DataChannelNativeApi onTaskStatus 代理
        taskIdText.text += lark.LarkManager.Instance.TaskId;

        lark.LarkManager larkManager = lark.LarkManager.Instance;
        larkManager.DataChannel.onTaskStatus += OnTaskStatus;
        larkManager.DataChannel.onConnected += OnConnected;
        larkManager.DataChannel.onText += OnTextMessage;
        larkManager.DataChannel.onBinary += OnBinaryMessaeg;
        larkManager.DataChannel.onClose += OnClose;
        // 智能语音相关回调 
        larkManager.DataChannel.onAiVoiceURL += OnAiVoiceURL;
        larkManager.DataChannel.onAiVoiceStream += OnAiVoiceStream;

        // start connect
        lark.DataChannelNativeApi.ApiRestult restult = lark.LarkManager.Instance.StartConnect();

        if (restult == lark.DataChannelNativeApi.ApiRestult.XR_SUCCESS)
        {
            if (supportAiVoice)
            {
                Debug.Log("try start ai voice");

                restult = lark.LarkManager.Instance.StartAiVoice();
                if (restult != lark.DataChannelNativeApi.ApiRestult.XR_SUCCESS) {
                    statusText.text = "Start AiVoice Failed. Result " + restult.ToString();
                } else
                {
                    statusText.text = "Start AiVoice Success.";
                    Debug.Log("start ai voice success");
                }
            } else
            {
                statusText.text = "Start DataChannel Connect Success.";
            }
        } else
        {
            statusText.text = "Start Failed. Result " + restult.ToString();
        }

        // default carmera 0
        SwitchCamera(0);
        // default hide object 0
        ToggleGameObject(0);

        // SetWindowSize(1280, 720);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        // 滚轮实现镜头缩进和拉远
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            // Camera.main.fieldOfView = Camera.main.fieldOfView - Input.GetAxis("Mouse ScrollWheel") * sensitivetyMouseWheel;
        }
        // 按着鼠标左键实现视角转动
        if (Input.GetMouseButton(0))
        {
            Camera.main.transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityMouse, 0);
        }
        // 键盘按钮←/a和→/d实现视角水平移动，键盘按钮↑/w和↓/s实现视角水平旋转
        if (Input.GetAxis("Horizontal") != 0)
        {
            Camera.main.transform.Translate(Input.GetAxis("Horizontal") * sensitivetyKeyBoard, 0, 0);
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            Camera.main.transform.Translate(0, 0, Input.GetAxis("Vertical") * sensitivetyKeyBoard);
        }
    }

    public void Send()
    {
        string text = sendInput.text;
        if (text != null) {
            lark.LarkManager.Instance.Send(text);
        }
    }

    public void SendX1000()
    {
        string text = sendInput.text;
        if (text != null)
        {
            for (int i = 0; i < 1000; i++) { 
                lark.LarkManager.Instance.Send(text + " no." + (i + 1));
            }
        }
    }


    #region call api
    public void GetTaskInfo()
    {
        StartCoroutine("GetTaskInfoCoroutine");
    }

    public void GetExraParams()
    {
        StartCoroutine("GetExraParamsCoroutine");
    }

    private IEnumerator GetTaskInfoCoroutine()
    {
        yield return getTaskInfo.Send(lark.LarkManager.Instance.TaskId);
        if (getTaskInfo.IsResultSuccess) {
            receiveText.text = getTaskInfo.taskInfo.ToString();
        }
        else
        {
            receiveText.text = getTaskInfo.Error;
        }
    }

    private IEnumerator GetExraParamsCoroutine()
    {
        yield return getExtraParams.Send(lark.LarkManager.Instance.TaskId);
        if (getExtraParams.IsResultSuccess) {
            receiveText.text = getExtraParams.extraParams;
        }
        else
        {
            receiveText.text = getExtraParams.Error;
        }
    }

    #endregion

    #region cmd
    private void SwitchCamera(int index)
    {
        if (index < 0 || index > cameras.Count - 1)
        {
            Debug.Log("camera index illegal. index " + index + " carmera count " + cameras.Count);
            return;
        }
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].gameObject.SetActive(i == index);
            cameras[i].enabled = i == index;
        }
    }
    private void ToggleGameObject(int index)
    {
        if (index < 0 || index > objects.Count - 1)
        {
            Debug.LogWarning("objects index illegal. index " + " object count " + objects.Count);
            return;
        }
        for (int i = 0; i < objects.Count; i++)
        {
            if (i == index)
            {
                objects[i].SetActive(!objects[i].activeSelf);
            }
        }
    }

    private void SendCameraLoaded()
    {
        if (cameras.Count == 0)
        {
            Debug.LogWarning("please set camera");
            return;
        }

        JsonCmd cmd = new JsonCmd(JsonCmd.CmdType.CMD_CAMERA_LOADED, cameras.Count);
        Debug.Log("cameras " + cmd.ToJson());
        SendText(cmd.ToJson());
    }
    private void SendObjectsLoaded()
    {
        if (objects.Count == 0)
        {
            Debug.LogWarning("please set object");
            return;
        }
        JsonCmd cmd = new JsonCmd(JsonCmd.CmdType.CMD_OBJECT_LOADED, objects.Count);
        Debug.Log("objects " + cmd.ToJson());
        SendText(cmd.ToJson());
    }
    private void SetWindowSize(int width, int height)
    {
        Screen.SetResolution(width, height, false);
    }
    // parse cmd
    private void ParseJsonCmd(string jsonStr)
    {
        try
        {
            JsonCmd cmd = JsonCmd.ParseJsonCmd(jsonStr);
            switch (cmd.type)
            {
                // webclient request switch camera.
                case JsonCmd.CmdType.CMD_SWITCH_CAMERA:
                    receiveText.text = "收到 JSON CMD： 切换摄像机 ( " + cmd.data + " )";
                    SwitchCamera(cmd.data);
                    break;
                // webclient request toggle object.
                case JsonCmd.CmdType.CMD_TOGGLE_OBJECT:
                    receiveText.text = "收到 JSON CMD 切换物体 ( " + cmd.data + " ) 显示状态";
                    ToggleGameObject(cmd.data);
                    break;
                case JsonCmd.CmdType.CMD_WINDOW_RESIZE:
                    receiveText.text = "收到网页窗口大小 width " + cmd.clientWidth + " height " + cmd.clientHeight;
                    if (cmd.clientWidth != 0 && cmd.clientHeight != 0) { 
                        SetWindowSize(cmd.clientWidth, cmd.clientHeight);
                    }
                    break;
                default:
                    // other cmd.
                    receiveText.text = jsonStr;
                    break;
            }
        }
        catch (Exception)
        {
            // parse failed. receive pure text.
            receiveText.text = jsonStr;
        }

    }
    #endregion

    #region wrap channel 
    public void SendText(string txt)
    {
        lark.LarkManager.Instance.Send(txt);
    }

    public void SendBinary(byte[] data)
    {
        lark.LarkManager.Instance.Send(data);
    }
    #endregion

    #region callbacks
    public void OnTaskStatus(bool status, string taskId) {
        Debug.Log("on task status change " + status + " "  +taskId);
        taskIdText.text = "收到 TaskID " + taskId + " 是否开启状态 " + status;
        if (!status) {
            Debug.Log("task 关闭，恢复初始状态");
            // default carmera 0
            SwitchCamera(0);
            // default hide object 0
            ToggleGameObject(0);
        }
    }
    public void OnConnected()
    {
        statusText.text = "连接成功";

        // send infos to client.
        SendCameraLoaded();
        SendObjectsLoaded();
    }
    public void OnTextMessage(string msg)
    {
        Debug.Log("OnTextMessage " + msg);
        ParseJsonCmd(msg);
    }
    public void OnBinaryMessaeg(byte[] binary)
    {
        Debug.Log("OnBinaryMessaeg " + BitConverter.ToString(binary));
        receiveText.text = "Binary msg: " + BitConverter.ToString(binary);
    }
    public void OnClose(lark.DataChannelNativeApi.ErrorCode code)
    {
        statusText.text = "通道已关闭 code " + code;
    }
    #endregion
    #region aivoice callback
    public void OnAiVoiceURL(lark.DataChannelNativeApi.AiVoiceURL aiVoiceURL) {
        Debug.Log("OnAiVoiceURL " + aiVoiceURL.nlg + " url " + aiVoiceURL.online_url);
        receiveText.text = "收到智能语音 URL 文本: " + aiVoiceURL.nlg + " ;url " + aiVoiceURL.online_url;

        _ = StartCoroutine(nameof(GetAudioClip), aiVoiceURL.online_url);
    }

    public void OnAiVoiceStream(lark.DataChannelNativeApi.AiVoiceStream aiVoiceStream) {
        Debug.Log("OnAiVoiceURL " + aiVoiceStream.nlg);
        receiveText.text = "收到智能语音 Stream 文本: " + aiVoiceStream.nlg;
    }

    IEnumerator GetAudioClip(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
#if ENABLE_NAUDIO
            byte[] results = www.downloadHandler.data;

            // 使用 NAudio 将 mp3 转换为 wave 播放
            using (var stream = new MemoryStream(results)) { 
                var reader = new Mp3FileReader(stream);
                var wo = new WaveOutEvent();
                wo.Init(reader);
                wo.Play();
                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    yield return new WaitForSeconds(1);
                }
            }
#endif
        }
    }
    #endregion
}
