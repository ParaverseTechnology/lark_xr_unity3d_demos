//--------------------------------------------------------------------------------------
// File: 	LarkXRDataChannel.h
// Author: 	Kevin
// Data:	2020/12/11
// Copyright (c) PXY Technologies Co., Ltd.. All rights reserved.
// https://www.pingxingyun.com/
//--------------------------------------------------------------------------------------
#ifndef LarkXRDataChannel_h__
#define LarkXRDataChannel_h__
#ifdef __cplusplus
extern "C" {
#endif
#include <stddef.h>
#include <stdint.h>
#include <stdbool.h>
#ifdef LARKXR_EXPORTS
#define LARKXR_API __declspec(dllexport)
#else
#define LARKXR_API __declspec(dllimport)
#endif
#define DC_CALL __stdcall

#define LARKXR_DC_VERSION_MAJOR			3
#define LARKXR_DC_VERSION_MINOR			2
#define LARKXR_DC_VERSION_REVISE		3
#define LARKXR_DC_VERSION_BUILD			1

#define LARKXR_DC_VERSION "3.2.3.1"

#define XR_SUCCESS					   0
#define XR_ERROR_INTERFACE_FAILED	  (XR_SUCCESS-1)
#define XR_ERROR_SERVER_UNSUPPORT	  (XR_SUCCESS-2)
#define XR_ERROR_PARAM				  (XR_SUCCESS-3)
#define XR_ERROR_OPREATION			  (XR_SUCCESS-4)

	enum data_type
	{
		DATA_STRING		= 0,
		DATA_BIN		= 1,
	};

	typedef void(*on_dc_connected)(void* user_data);

	typedef void(*on_dc_data)(enum data_type type, const char* data,int size,void* user_data);

	enum ErrorCode
	{
		ERROR_SERVER_CLOSE				= 0,	//服务端主动断开链接
		ERROR_DC_UNSUPPORTED			= 1,	//服务端授权不支持DataChannel
		ERROR_SERVER_CONNECTION_FAILED	= 2,	//无法链接服务器或者与服务器握手失败(检查taskid传递是否正确)
	};
	typedef void(*on_dc_error)(enum ErrorCode code,void* user_data);
	//-----------------------------------------
	typedef void(*on_taskstatus)(bool status/*true:客户端连接 false:客户端断开*/,const char* taskId, void* user_data);


	//************************************
	// Method:    lr_client_register_getTaskId_callback
	// FullName:  lr_client_register_getTaskId_callback
	// Access:    public 
	// Returns:   LARKXR_API void DC_CALL
	// Qualifier: 应用通过此回调接口获取客户单连接状态以及taskid
	// Parameter: on_taskid get_task
	// Parameter: void * user_data
	//************************************
	LARKXR_API void DC_CALL lr_client_register_taskstatus_callback(on_taskstatus taskstatus,void* user_data);

	//************************************
	// Method:    lr_client_start
	// FullName:  lr_client_start
	// Access:    public 
	// Returns:   LARKXR_API int DC_CALL
	// Qualifier: 异步连接LarkXR服务端,必须传入回调函数，返回XR_ERROR_SUCCESS代表接口创建成功
	// Parameter: const char * taskid(如果无法从命令行获取taskid，请直接传NULL)
	// Parameter: on_connected cb_connected
	// Parameter: on_data cb_data
	// Parameter: on_disconnected cb_disconnected
	// Parameter: void* user_data

	//************************************
	LARKXR_API int  DC_CALL lr_client_start(const char* taskid, on_dc_connected cb_connected,on_dc_data cb_data,on_dc_error cb_disconnected,void* user_data);
	//************************************
	// Method:    lr_client_send
	// FullName:  lr_client_send
	// Access:    public 
	// Returns:   LARKXR_API int			[if send success  ret = input size]
	// Qualifier: send data to server		[utf8 string or binary data]
	// Parameter: const char * data			[send data]
	// Parameter: int size					[send data size,in vr mode ,the max size is 1388 byte] 
	//************************************
	LARKXR_API int DC_CALL lr_client_send(enum data_type type, const char* data, size_t size);

	//************************************
	// Method:    lr_client_disconnect
	// FullName:  lr_client_disconnect
	// Access:    public 
	// Returns:   LARKXR_API void
	// Qualifier: disconnect server connection 
	//************************************
	LARKXR_API void DC_CALL lr_client_stop();

#ifdef __cplusplus
}
#endif
#endif // LarkXRDataChannel_h__
