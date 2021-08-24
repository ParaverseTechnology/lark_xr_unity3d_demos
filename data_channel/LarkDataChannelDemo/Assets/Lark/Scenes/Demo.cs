using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
    public Text taskIdText;
    public Text statusText;
    public InputField sendInput;
    public Text receiveText;

    public List<Camera> cameras;
    public List<GameObject> objects;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(taskIdText != null);
        Debug.Assert(statusText != null);
        Debug.Assert(sendInput != null);
        Debug.Assert(receiveText != null);

        // 请在云雀后台上传应用时设置《接口调用是否附加参数》为<<是>>
        // 附加参数 taskId 将以传递给命令行最后一位
        // 如果 taskId 未设置或不正确将无法连接数据通道
        taskIdText.text += lark.LarkManager.Instance.TaskId;

        lark.LarkManager larkManager = lark.LarkManager.Instance;
        larkManager.DataChannel.onConnected += OnConnected;
        larkManager.DataChannel.onText += OnTextMessage;
        larkManager.DataChannel.onBinary += OnBinaryMessaeg;
        larkManager.DataChannel.onClose += OnClose;
        // start connect
        lark.DataChannelNativeApi.ApiRestult restult = lark.LarkManager.Instance.StartConnect();

        if (restult != lark.DataChannelNativeApi.ApiRestult.XR_SUCCESS)
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
}
