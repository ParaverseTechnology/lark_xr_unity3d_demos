using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JsonCmd
{
    public enum CmdType
    {
        // 通知网页载入摄像机个数
        CMD_CAMERA_LOADED = 1000,
        // 网页控制切换使用某个摄像机
        CMD_SWITCH_CAMERA = 1001,

        // 通知网页载入的物体
        CMD_OBJECT_LOADED = 2001,
        // 物体被选择s
        CMD_OBJECT_PICKED = 2002,
        // 切换某个物体的显示状态
        CMD_TOGGLE_OBJECT = 2003,

        CMD_WINDOW_RESIZE = 3001,
    }

    public static JsonCmd ParseJsonCmd(string res)
    {
        return JsonUtility.FromJson<JsonCmd>(res);
    }

    public CmdType type;
    public int data;
    public int clientWidth = 0;
    public int clientHeight = 0;

    public JsonCmd(CmdType type, int data = 0)
    {
        this.type = type;
        this.data = data;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}
