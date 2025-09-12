using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loom : YjjSingleton<Loom>
{
    private Queue<Action> queues = new Queue<Action>();

    private void Update()
    {
        while (queues.Count > 0)
        {
            queues.Dequeue()?.Invoke();
        }
    }
    public void Enqueue(Action action)
    {
        queues.Enqueue(action);
    }
}
