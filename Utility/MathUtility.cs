using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YJJTool
{
    public static class MathUtility
    {
        public static int FindFirstIndexGE(List<Vector2> arr, float targetX)
        {
            int low = 0, high = arr.Count - 1, ans = arr.Count;
            while (low <= high)
            {
                int mid = (low + high) >> 1;
                if (arr[mid].x >= targetX)
                {
                    ans = mid;
                    high = mid - 1;
                }
                else
                {
                    low = mid + 1;
                }
            }
            return ans;
        }

        public static int FindLastIndexLE(List<Vector2> arr, float targetX)
        {
            int low = 0, high = arr.Count - 1, ans = -1;
            while (low <= high)
            {
                int mid = (low + high) >> 1;
                if (arr[mid].x <= targetX)
                {
                    ans = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            return ans;
        }
    }
}

