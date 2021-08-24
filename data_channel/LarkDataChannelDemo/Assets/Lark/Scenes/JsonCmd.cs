using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JsonCmd
{
    public enum CmdType
    {
        // ֪ͨ��ҳ�������������
        CMD_CAMERA_LOADED = 1000,
        // ��ҳ�����л�ʹ��ĳ�������
        CMD_SWITCH_CAMERA = 1001,

        // ֪ͨ��ҳ���������
        CMD_OBJECT_LOADED = 2001,
        // ���屻ѡ��s
        CMD_OBJECT_PICKED = 2002,
        // �л�ĳ���������ʾ״̬
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
