//--------------------------------------------------------------------------------------
// File: 	LarkXRAiVoice.h
// Author: 	Kevin
// Data:	2022/02/24
// Copyright (c) PXY Technologies Co., Ltd.. All rights reserved.
// https://www.pingxingyun.com/
//--------------------------------------------------------------------------------------
#ifndef LarkXRAiVoice_h__
#define LarkXRAiVoice_h__

#ifdef __cplusplus
extern "C" {
#endif
	#include"LarkXRDataChannel.h"

	struct AiVoicePacket
	{
		bool	url;				//true :online audio url(mp3) .false: audio pack (pcm)
		unsigned int voice_id;		//语音ID
		const char* online_url;		//如果url为true,该字段为url地址,否则该字段为NULL 
		int	    url_size;			//url长度 包含\0
		const char* nlg;			//当前语音对讲的文本
		int	    nlg_size;			//对讲文本长度 包含\0

		//如果URL为false 那么下面字段描述每一个pcm包
		unsigned int slice_id;		//一个语音分片ID
		int		samples_per_sec;	//eg.16000
		int		channels;		    //eg.1
		const char* audio;			//数据包指针,如果 url 为true 该字段为空
		int		size_byte;			//每一包的字节数
		bool	last_packet;		//是否为最后一包
	};
	typedef void(*on_aivoice_callback)(struct AiVoicePacket* packet,void* user_data);

	
	//************************************
	// Method:    lr_client_register_aivoice_callback
	// FullName:  lr_client_register_aivoice_callback
	// Access:    public 
	// Returns:   XR_SUCCESS:注册成功	 XR_ERROR_INTERFACE_FAILED:没有调用 r_client_start XR_ERROR_SERVER_UNSUPPORT:数据通道不支持/智能语音不支持
	// Qualifier: 调用此函数一定要再lr_client_start成功，并且回调函数也返回成功后再调用
	// Parameter: on_aivoice_callback cb
	// Parameter: void * user_data
	//************************************
	LARKXR_API int  DC_CALL lr_client_register_aivoice_callback(on_aivoice_callback cb,void* user_data);





#ifdef __cplusplus
}
#endif
#endif // LarkXRAiVoice_h__