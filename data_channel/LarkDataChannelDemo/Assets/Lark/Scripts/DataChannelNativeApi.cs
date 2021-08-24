using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace lark { 
    public class DataChannelNativeApi : MonoBehaviour
    {
        // 连接数据通道成功
        public delegate void OnConnected();
        // 收到文本消息
        public delegate void OnTextMessage(string msg);
        // 收到字节消息
        public delegate void OnBinaryMessaeg(byte[] binary);
        // 数据通道关闭
        public delegate void OnClose(ErrorCode code);

        public const int XR_SUCCESS = 0;
        public const int XR_ERROR_INTERFACE_FAILED = -1;
        public const int XR_ERROR_SERVER_UNSUPPORT = -2;
        public const int XR_ERROR_PARAM = -3;
        public const int XR_ERROR_OPREATION = -4;

        public enum DataType
        {
            DATA_STRING = 0,
            DATA_BIN = 1,
        }
        // 数据通道关闭时错误码
        public enum ErrorCode
        {
            ERROR_SERVER_CLOSE = 0,             //服务端主动断开链接
            ERROR_DC_UNSUPPORTED = 1,           //服务端授权不支持DataChannel
            ERROR_SERVER_CONNECTION_FAILED = 2,	//无法链接服务器或者与服务器握手失败(检查taskid传递是否正确)
        }
        // 接口调用结果
        public enum ApiRestult
        {
            XR_SUCCESS = 0,
            XR_ERROR_INTERFACE_FAILED = -1,
            XR_ERROR_SERVER_UNSUPPORT = -2,
            XR_ERROR_PARAM = -3,
            XR_ERROR_OPREATION = -4,
        }
        #region instance methods.
        // inner task
        private delegate void GUITask();
        private Queue<GUITask> TaskQueue = new Queue<GUITask>();
        private readonly object _queueLock = new object();

        DataChannelNativeApi()
        {
            cs_on_connected = c_on_connected;
            cs_on_data = c_on_data;
            cs_on_disconnected = c_on_disconnected;
        }

        private void Start()
        {

        }
        private void Update()
        {
            lock (_queueLock)
            {
                if (TaskQueue.Count > 0)
                    TaskQueue.Dequeue()();
            }
        }

        private void ScheduleTask(GUITask newTask)
        {
            lock (_queueLock)
            {
                TaskQueue.Enqueue(newTask);
            }
        }
        #endregion

        #region apis
        // 通道连接成功代理
        public OnConnected onConnected;
        // 文本消息代理
        public OnTextMessage onText;
        // 字节消息代理
        public OnBinaryMessaeg onBinary;
        // 通道关闭代理
        public OnClose onClose;
        /// <summary>
        /// 开始连接数据通道。 
        /// </summary>
        /// <param name="taskId">启动应用时分配的 TaskID，在云雀管理后台启用附加参数选项，可在命令行参数中获取 TaskID</param>
        /// <returns>启动结果</returns>
        public ApiRestult StartConnect(string taskId)
        {
            Debug.Log("start connect taskId " + taskId);
            ApiRestult res = (ApiRestult)lr_client_start(taskId, cs_on_connected, cs_on_data, cs_on_disconnected);
            return res;
        }
        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="msg">文本消息</param>
        public ApiRestult Send(string msg)
        {
            if (msg == "")
            {
                return ApiRestult.XR_ERROR_PARAM;
            }
            byte[] binary = Encoding.UTF8.GetBytes(msg);
            // Debug.Log("Send Text " + msg + " binary " + BitConverter.ToString(binary) + " size " + binary.Length);
            ApiRestult result = (ApiRestult)lr_client_send((int)DataType.DATA_STRING, ref binary[0], binary.Length);
            return result;
        }
        /// <summary>
        /// 发送字节消息
        /// </summary>
        /// <param name="binary">字节消息</param>
        public ApiRestult Send(byte[] binary)
        {
            if (binary.Length == 0)
            {
                return ApiRestult.XR_ERROR_PARAM;
            }
            ApiRestult result = (ApiRestult)lr_client_send((int)DataType.DATA_BIN, ref binary[0], binary.Length);
            return result;
        }
        /// <summary>
        /// 主动停止数据通道
        /// </summary>
        public void Stop()
        {
            Debug.Log("disconect from datachannel");
            lr_client_stop();
        }
        #endregion

        #region native callbacks
        // callbacks
        private delegate void on_connected();
        private delegate void on_data(int type, IntPtr data, int size);
        private delegate void on_disconnected(int code);

        private on_connected cs_on_connected;
        private on_data cs_on_data;
        private on_disconnected cs_on_disconnected;

        private void c_on_connected()
        {
            Debug.Log("on connected");
            ScheduleTask(new GUITask(delegate {
                onConnected?.Invoke();
            }));
        }
        private void c_on_data(int type, IntPtr data, int size)
        {
            byte[] array = new byte[size];

            Marshal.Copy(data, array, 0, size);
            
            if ((DataType)type == DataType.DATA_STRING) {
                // End with 0
                string strMsg = Encoding.UTF8.GetString(array, 0, size > 0 ? size - 1 : size);
                ScheduleTask(new GUITask(delegate {
                    onText?.Invoke(strMsg);
                }));
            } else {
                ScheduleTask(new GUITask(delegate {
                    onBinary?.Invoke(array);
                }));
            }
        }
        private void c_on_disconnected(int code)
        {
            Debug.Log("on disconnected " + code);
            ScheduleTask(new GUITask(delegate {
                onClose?.Invoke((ErrorCode)code);
            }));
        }
        #endregion

        #region native LarkXRDataChannel.h
        // 异步连接LarkXR服务端,必须传入回调函数，返回XR_ERROR_SUCCESS代表接口创建成功
        [DllImport("LarkXRDataChannel64")]
        private static extern int lr_client_start(string taskId, on_connected cb_connected, on_data cb_data, on_disconnected cb_disconnected);
        [DllImport("LarkXRDataChannel64")]
        private static extern int lr_client_send(int dataType, ref byte data, int size);
        [DllImport("LarkXRDataChannel64")]
        private static extern void lr_client_stop();
        #endregion
    }
}