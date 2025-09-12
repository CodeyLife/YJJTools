using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntEvent : UnityEvent<int> { }
[System.Serializable]
public class stringEvent : UnityEvent<string> { }

[System.Serializable]
public class ByteEvent : UnityEvent<byte[]> { }

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

[System.Serializable]
public class TransfromEvent : UnityEvent<Transform> { }
