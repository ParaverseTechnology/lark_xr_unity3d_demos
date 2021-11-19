using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lark {
    public class GetTaskInfo : ApiBase<GetTaskInfo.TaskInfo>
    {
        private const string METHOD = "/taskInfo/getTask";

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

        public TaskInfo taskInfo { get; private set; } = null;


        public IEnumerator Send(string taskId)
        {
            HttpQueryParam param = new HttpQueryParam();
            param.Add("taskId", taskId);
            yield return GetText(METHOD, param.ToString());
        }

        protected override void OnApiResponseSuccess(ApiResponse<TaskInfo> response)
        {
            base.OnApiResponseSuccess(response);
            //Debug.Log("============ applist serrch result:" + response.code + "; list:" + response.result.Count);
            if (IsCodeSuccess(response.code))
            {
                IsResultSuccess = true;
                Message = response.message;
                taskInfo = response.result;
            }
            else
            {
                IsResultSuccess = false;
                Message = response.message;
                taskInfo = null;
            }
        }

        protected override void OnFailed(string error)
        {
            base.OnFailed(error);
            IsResultSuccess = false;
            Message = "";
            taskInfo = null;
        }


        [System.Serializable]
        public class TaskInfo
        {
            public string taskId;
            public string serverId;
            public string serverIp;
            public string publicIp;
            public string clientIp;
            public string country;
            public string province;
            public string city;
            public string appKey;
            public string appliId;
            public int appliType;
            public int startProcType;
            public int status;
            public string startParam;
            public string startAt;
            public int limitMaxFps;
            public int offScreen;
            public int useGamepad;
            public int playerMode;
            public int playerListToggle;
            public int adminViewer;
            public string createDate;
            public string updateDate;
            public string shareUrl;
            public int dcs;
            public int reserveFlag;

            public override string ToString()
            {
                return JsonUtility.ToJson(this);
            }
        }
    }
}
