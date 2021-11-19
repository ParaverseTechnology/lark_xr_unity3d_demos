using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lark
{
    public class GetExtraParams : ApiBase<string>
    {
        private const string METHOD = "/taskInfo/getExtraParams";


        public override bool IsError
        {
            get
            {
                return base.IsError || !IsResultSuccess;
            }
        }

        public override string Error
        {
            get
            {
                if (!base.Error.Equals(""))
                {
                    return base.Error;
                }
                else if (!IsResultSuccess)
                {
                    return Message;
                }
                else
                {
                    return "";
                }
            }
        }

        public bool IsResultSuccess { get; private set; } = false;

        public string Message { get; private set; } = "";

        public string extraParams { get; private set; } = null;

        public IEnumerator Send(string taskId)
        {
            HttpQueryParam param = new HttpQueryParam();
            param.Add("taskId", taskId);
            yield return GetText(METHOD, param.ToString());
        }

        protected override void OnApiResponseSuccess(ApiResponse<string> response)
        {
            base.OnApiResponseSuccess(response);
            //Debug.Log("============ applist serrch result:" + response.code + "; list:" + response.result.Count);
            if (IsCodeSuccess(response.code))
            {
                IsResultSuccess = true;
                Message = response.message;
                extraParams = response.result;
            }
            else
            {
                IsResultSuccess = false;
                Message = response.message;
                extraParams = "";
            }
        }

        protected override void OnFailed(string error)
        {
            base.OnFailed(error);
            IsResultSuccess = false;
            Message = "";
            extraParams = "";
        }
    }
}

