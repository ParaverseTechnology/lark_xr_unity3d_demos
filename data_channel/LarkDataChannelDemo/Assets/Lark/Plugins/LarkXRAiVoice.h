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
		unsigned int voice_id;		//����ID
		const char* online_url;		//���urlΪtrue,���ֶ�Ϊurl��ַ,������ֶ�ΪNULL 
		int	    url_size;			//url���� ����\0
		const char* nlg;			//��ǰ�����Խ����ı�
		int	    nlg_size;			//�Խ��ı����� ����\0

		//���URLΪfalse ��ô�����ֶ�����ÿһ��pcm��
		unsigned int slice_id;		//һ��������ƬID
		int		samples_per_sec;	//eg.16000
		int		channels;		    //eg.1
		const char* audio;			//���ݰ�ָ��,��� url Ϊtrue ���ֶ�Ϊ��
		int		size_byte;			//ÿһ�����ֽ���
		bool	last_packet;		//�Ƿ�Ϊ���һ��
	};
	typedef void(*on_aivoice_callback)(struct AiVoicePacket* packet,void* user_data);

	
	//************************************
	// Method:    lr_client_register_aivoice_callback
	// FullName:  lr_client_register_aivoice_callback
	// Access:    public 
	// Returns:   XR_SUCCESS:ע��ɹ�	 XR_ERROR_INTERFACE_FAILED:û�е��� r_client_start XR_ERROR_SERVER_UNSUPPORT:����ͨ����֧��/����������֧��
	// Qualifier: ���ô˺���һ��Ҫ��lr_client_start�ɹ������һص�����Ҳ���سɹ����ٵ���
	// Parameter: on_aivoice_callback cb
	// Parameter: void * user_data
	//************************************
	LARKXR_API int  DC_CALL lr_client_register_aivoice_callback(on_aivoice_callback cb,void* user_data);





#ifdef __cplusplus
}
#endif
#endif // LarkXRAiVoice_h__