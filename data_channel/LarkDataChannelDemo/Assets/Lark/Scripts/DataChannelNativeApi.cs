using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace lark
{
    public class DataChannelNativeApi : MonoBehaviour
    {
        // TaskStatus 通过监听 task 状态获取客户端连接情况。
        // 在应用使用预启动功能时，应用保持长开启状态，当有客户端连接的时候
        // Task 状态发生变化。
        // status true:客户端连接 false:客户端断开
        public delegate void OnTaskStatus(bool status, string taskId);
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


        // 智能语音相关结构
        public struct AiVoiceURL
        {
            // 语音ID
            public uint voice_id;
            // 如果url为true,该字段为url地址,否则该字段为NULL 
            public string online_url;
            // 当前语音对讲的文本
            public string nlg;
        }

        public struct AiVoiceStream
        {
            // 语音ID
            public uint voice_id;
            // 当前语音对讲的文本
            public string nlg;

            // 如果URL为false 那么下面字段描述每一个pcm包
            public uint slice_id;
            // eg.16000
            public int samples_per_sec;
            // eg.1
            public int channels;
            // 数据包指针,如果 url 为true 该字段为空
            public byte[] audio;
            // 是否为最后一包
            public bool last_packet;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct NativeAiVoicePacket
        {
            // true :online audio url(mp3) .false: audio pack (pcm)
            public bool url;
            // 语音ID
            public uint voice_id;
            // 如果url为true,该字段为url地址,否则该字段为NULL 
            public IntPtr online_url;
            // url长度 包含\0
            public int url_size;
            // 当前语音对讲的文本
            public IntPtr nlg;
            // 对讲文本长度 包含\0
            public int nlg_size;

            // 如果URL为false 那么下面字段描述每一个pcm包
            public uint slice_id;
            // eg.16000
            public int samples_per_sec;
            // eg.1
            public int channels;
            // 数据包指针,如果 url 为true 该字段为空
            public IntPtr audio;
            // 每一包的字节数
            public int size_byte;
            // 是否为最后一包
            public bool last_packet;
        };

        // 收到智能语音 url 形式播放结果, url 为 mp3 连接
        public delegate void OnAiVoiceURL(AiVoiceURL aiVoiceURL);
        // 收到智能语音流式播放结果, pcm 数据
        public delegate void OnAiVoiceStream(AiVoiceStream aiVoiceStream);

        #region instance methods.
        // inner task
        private delegate void GUITask();
        private Queue<GUITask> TaskQueue = new Queue<GUITask>();
        private readonly object _queueLock = new object();

        DataChannelNativeApi()
        {
            cs_on_taskstatus = c_on_taskstatus;
            cs_on_connected = c_on_connected;
            cs_on_data = c_on_data;
            cs_on_disconnected = c_on_disconnected;

            cs_on_aivoice_callback = c_on_aivoice_callback;
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
        // Task 状态变化
        public OnTaskStatus onTaskStatus;
        // 通道连接成功代理
        public OnConnected onConnected;
        // 文本消息代理
        public OnTextMessage onText;
        // 字节消息代理
        public OnBinaryMessaeg onBinary;
        // 通道关闭代理
        public OnClose onClose;

        //************************************
        // Method:    lr_client_register_getTaskId_callback
        // FullName:  lr_client_register_getTaskId_callback
        // Access:    public 
        // Returns:   LARKXR_API void DC_CALL
        // Qualifier: 应用通过此回调接口获取客户单连接状态以及taskid
        // Parameter: on_taskid get_task
        // Parameter: void * user_data
        //************************************
        public void RegisterTaskstatusCallback()
        {
            lr_client_register_taskstatus_callback(cs_on_taskstatus, IntPtr.Zero);
        }
        /// <summary>
        /// 开始连接数据通道。 
        /// </summary>
        /// <param name="taskId">启动应用时分配的 TaskID，在云雀管理后台启用附加参数选项，可在命令行参数中获取 TaskID</param>
        /// <returns>启动结果</returns>
        public ApiRestult StartConnect(string taskId)
        {
            Debug.Log("start connect taskId " + taskId);
            ApiRestult res = (ApiRestult)lr_client_start(taskId, cs_on_connected, cs_on_data, cs_on_disconnected, IntPtr.Zero);
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

        /// <summary>
        /// 智能语音代理
        /// </summary>
        public OnAiVoiceURL onAiVoiceURL;
        public OnAiVoiceStream onAiVoiceStream;

        //************************************
        // Method:    lr_client_register_aivoice_callback
        // FullName:  lr_client_register_aivoice_callback
        // Access:    public 
        // Returns:   XR_SUCCESS:注册成功	 XR_ERROR_INTERFACE_FAILED:没有调用 r_client_start XR_ERROR_SERVER_UNSUPPORT:数据通道不支持/智能语音不支持
        // Qualifier: 调用此函数一定要再lr_client_start成功，并且回调函数也返回成功后再调用
        // Parameter: on_aivoice_callback cb
        // Parameter: void * user_data
        public int RegisterAivoiceCallback()
        {
            return lr_client_register_aivoice_callback(cs_on_aivoice_callback, IntPtr.Zero);
        }
        #endregion

        #region native callbacks
        // callbacks
        private delegate void on_taskstatus(bool status/*true:客户端连接 false:客户端断开*/, string taskId, IntPtr user_data);
        private delegate void on_connected(IntPtr user_data);
        private delegate void on_data(int type, IntPtr data, int size, IntPtr user_data);
        private delegate void on_disconnected(int code, IntPtr user_data);

        private on_taskstatus cs_on_taskstatus;
        private on_connected cs_on_connected;
        private on_data cs_on_data;
        private on_disconnected cs_on_disconnected;

        private void c_on_taskstatus(bool status/*true:客户端连接 false:客户端断开*/, string taskId, IntPtr user_data)
        {
            ScheduleTask(new GUITask(delegate {
                onTaskStatus?.Invoke(status, taskId);
            }));
        }

        private void c_on_connected(IntPtr user_data)
        {
            Debug.Log("on connected");
            ScheduleTask(new GUITask(delegate {
                onConnected?.Invoke();
            }));
        }
        private void c_on_data(int type, IntPtr data, int size, IntPtr user_data)
        {
            byte[] array = new byte[size];

            Marshal.Copy(data, array, 0, size);

            if ((DataType)type == DataType.DATA_STRING)
            {
                // Debug.Log("c_on_data len" + size + " array " + string.Join(" ", array));
                // End with 0
                // string strMsg = Encoding.UTF8.GetString(array, 0, size > 0 ? size - 1 : size);
                // update 20211018
                string strMsg = Encoding.UTF8.GetString(array, 0, size);
                ScheduleTask(new GUITask(delegate {
                    onText?.Invoke(strMsg);
                }));
            }
            else
            {
                ScheduleTask(new GUITask(delegate {
                    onBinary?.Invoke(array);
                }));
            }
        }
        private void c_on_disconnected(int code, IntPtr user_data)
        {
            Debug.Log("on disconnected " + code);
            ScheduleTask(new GUITask(delegate {
                onClose?.Invoke((ErrorCode)code);
            }));
        }
        #endregion

        #region native LarkXRDataChannel.h
        [DllImport("LarkXRDataChannel64")]
        private static extern void lr_client_register_taskstatus_callback(on_taskstatus taskstatus, IntPtr user_data);
        // 异步连接LarkXR服务端,必须传入回调函数，返回XR_ERROR_SUCCESS代表接口创建成功
        [DllImport("LarkXRDataChannel64")]
        private static extern int lr_client_start(string taskId, on_connected cb_connected, on_data cb_data, on_disconnected cb_disconnected, IntPtr user_data);
        [DllImport("LarkXRDataChannel64")]
        private static extern int lr_client_send(int dataType, ref byte data, int size);
        [DllImport("LarkXRDataChannel64")]
        private static extern void lr_client_stop();
        #endregion

        #region aivoice native callbacks
        // callbacks
        private delegate void on_aivoice_callback(NativeAiVoicePacket packet, IntPtr user_data);

        private on_aivoice_callback cs_on_aivoice_callback;

        private void c_on_aivoice_callback(NativeAiVoicePacket packet, IntPtr user_data)
        {
            Debug.Log("c_on_aivoice_callback " + packet.voice_id + " " + Marshal.SizeOf(packet) + " " + packet.url);
            if (packet.url)
            {

                Debug.Log("c_on_aivoice_callback " + packet.url_size + " " + packet.nlg_size + " " + packet.online_url + " " + packet.nlg + " " + packet.voice_id);

                byte[] urlData = new byte[packet.url_size];
                Marshal.Copy(packet.online_url, urlData, 0, packet.url_size);

                byte[] nalData = new byte[packet.nlg_size];
                Marshal.Copy(packet.nlg, nalData, 0, packet.nlg_size);

                AiVoiceURL aiVoiceURL = new AiVoiceURL();
                aiVoiceURL.voice_id = packet.voice_id;

                aiVoiceURL.online_url = Encoding.UTF8.GetString(urlData, 0, packet.url_size > 0 ? packet.url_size - 1 : 0);

                aiVoiceURL.nlg = Encoding.UTF8.GetString(nalData, 0, packet.nlg_size > 0 ? packet.nlg_size - 1 : 0);

                Debug.Log("c_on_aivoice_callback url0*** " + aiVoiceURL.online_url);

                Debug.Assert(onAiVoiceURL != null);

                ScheduleTask(new GUITask(delegate {
                    onAiVoiceURL?.Invoke(aiVoiceURL);
                }));
            }
            else
            {
                Debug.Log("c_on_aivoice_callback pcm " + packet.size_byte + " " + packet.voice_id);

                byte[] nalData = new byte[packet.nlg_size];
                Marshal.Copy(packet.nlg, nalData, 0, packet.nlg_size);

                byte[] stream = new byte[packet.size_byte];
                Marshal.Copy(packet.audio, stream, 0, packet.size_byte);

                AiVoiceStream aiVoiceStream = new AiVoiceStream();
                aiVoiceStream.voice_id = packet.voice_id;
                aiVoiceStream.audio = stream;
                aiVoiceStream.channels = packet.channels;
                aiVoiceStream.last_packet = packet.last_packet;
                aiVoiceStream.samples_per_sec = packet.samples_per_sec;
                aiVoiceStream.slice_id = packet.slice_id;
                aiVoiceStream.nlg = Encoding.UTF8.GetString(nalData, 0, packet.nlg_size);

                ScheduleTask(new GUITask(delegate {
                    onAiVoiceStream?.Invoke(aiVoiceStream);
                }));
            }
        }
        #endregion

        #region native LarkXRAiVoice.h
        [DllImport("LarkXRDataChannel64")]
        private static extern int lr_client_register_aivoice_callback(on_aivoice_callback cb, IntPtr user_data);
        #endregion
    }
}
