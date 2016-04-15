﻿using UnityEngine;
using System;
using System.Collections.Generic;

namespace MCN
{
    [System.Serializable]
    public class PlaceInfo
    {
        public Vector2 pos;
        public int unitNo;
    }

    #region pairs
    public class Pair<T, T2>
    {
        public T key;
        public T2 value;
    }

    [System.Serializable]
    public class StringIntPair : Pair<string, int>{ }
    #endregion
}
