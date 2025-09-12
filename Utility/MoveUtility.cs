using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YJJTool
{
    public static class MoveUtility
    {

        /// <summary>
        /// 已知起点终点和时间， 获取加速度
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="speed"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        //s = vt+0.5at^2
        public static float GetAddSpeed(Vector3 begin, Vector3 end, float speed, float time)
        {
            var distance = Vector3.Distance(begin, end);
            var result = (distance - speed * time) * 2 / MathF.Pow(time, 2);
            return result;
        }

        /// <summary>
        /// 已知当前速度和加速度及时间，求运动距离  s = vt + 1/2 * 加速度 * t²
        /// </summary>
        /// <param name="time"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        public static float GetDistanceWithAddSpeed(float speed, float time, float add)
        {
            var result = speed * time + 0.5f * add * Mathf.Pow(time, 2);
            return result;
        }

        public static Coroutine MoveWithAddSpeed(this MonoBehaviour mono, Vector3 targetPos, float time, Quaternion targetRotation, SpeedData speedData,Action action = null)
        {
            var current = mono.transform.position;
            var dir = (targetPos - new Vector3(current.x, targetPos.y, current.z)).normalized;
            var add = GetAddSpeed(current, targetPos, speedData.speed, time);
            var maxDistance = Vector3.Distance(targetPos, current);
            var cor = mono.StartCoroutine(Move(mono.transform));
            return cor;

            IEnumerator Move(Transform transform)
            {
                var all = time;
                var startRotation = transform.rotation;
                while (time > 0)
                {
                    var delta = Time.deltaTime;
                    time -= delta;
                    var distance = GetDistanceWithAddSpeed(speedData.speed, delta, add);
                    distance = distance > maxDistance ? maxDistance : distance;
                    transform.position += dir * distance;
                    maxDistance -= distance;
                    speedData.speed += add * delta;
                    var t = 1 - time / all;
                    transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                    yield return null;
                }
                action?.Invoke();
            }
        }
    }
    [System.Serializable]
    public class SpeedData
    {
        public float speed;
    }
}
