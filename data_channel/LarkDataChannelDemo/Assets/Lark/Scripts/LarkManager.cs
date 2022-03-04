using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace lark { 
    public class LarkManager : MonoBehaviour
    {
        const string OBJ_NAME = "LarkManager";

        // 异步获取。
        // 添加 DataChannelNativeApi onTaskStatus 代理
        public string TaskId { get; private set; } = "";

        private static LarkManager larkManager = null;
        public static LarkManager Instance
        {
            get
            {
                if (larkManager == null)
                {
                    larkManager = FindObjectOfType<LarkManager>();
                }
                if (larkManager == null)
                {
                    var go = new GameObject(OBJ_NAME);
                    larkManager = go.AddComponent<LarkManager>();
                }
                return larkManager;
            }
        }

        public DataChannelNativeApi DataChannel
        {
            get;
            private set;
        }

        private void Awake()
        {
            DataChannel = GetComponent<DataChannelNativeApi>();

            if (DataChannel == null)
            {
                DataChannel = gameObject.AddComponent<DataChannelNativeApi>();
            }

            try
            {
#if UNITY_EDITOR
                // 编辑器内使用测试 TaskID
                TaskId = "123456";
#else
                DataChannel.RegisterTaskstatusCallback();
                DataChannel.onTaskStatus += OnTaskStatus;
#endif
            }
            catch (Exception e) {
                Debug.LogError("检测到taskID格式不正确，请在云雀后台设置使用附加参数 " + e.Message);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnDestroy()
        {
            DataChannel.Stop();
        }

        /// <summary>
        /// 开始连接数据通道
        /// </summary>
        /// <returns>连接结果</returns>
        public DataChannelNativeApi.ApiRestult StartConnect() {
            return DataChannel.StartConnect(TaskId);
        }

        /// <summary>
        /// 开启智能语流程
        /// 调用此函数一定要StartConnect成功，并且回调函数也返回成功后再调用
        /// </summary>
        /// <returns></returns>
        public DataChannelNativeApi.ApiRestult StartAiVoice()
        {
            return (DataChannelNativeApi.ApiRestult)DataChannel.RegisterAivoiceCallback();
        }

        /// <summary>
        /// 主动关闭数据通道
        /// </summary>
        public void Stop()
        {
            DataChannel.Stop();
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="txt">文本消息</param>
        public DataChannelNativeApi.ApiRestult Send(string txt)
        {
            return DataChannel.Send(txt);
        }
        /// <summary>
        /// 发送字节消息
        /// </summary>
        /// <param name="binary">字节消息</param>
        public DataChannelNativeApi.ApiRestult Send(byte[] binary) {
            return DataChannel.Send(binary);
        }

        #region callback
        public void OnTaskStatus(bool status, string taskId)
        {
            Debug.Log("on task status change. " + status + " " + taskId);
            TaskId = taskId;
        }
        #endregion
    }
}