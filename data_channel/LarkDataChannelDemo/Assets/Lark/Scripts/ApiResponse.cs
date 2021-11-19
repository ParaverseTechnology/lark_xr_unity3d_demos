using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lark
{
    [System.Serializable]
    public class ApiResponse<T>
    {
        public const int RESPONSE_SUCCESS_LARK = 1000;
        public static ApiResponse<T> ParseApiResponse(string res)
        {
            return JsonUtility.FromJson<ApiResponse<T>>(res);
        }

        public int code;
        public string message;
        public T result;
    }
}