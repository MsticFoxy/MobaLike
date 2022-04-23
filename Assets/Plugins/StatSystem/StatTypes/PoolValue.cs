using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PoolValueFloat : ICloneable
{
    public Action<float> OnCurrentChanged;
    public Action<float, float> OnRangeChanged;
    public Action<float> OnCurrentIsMin;
    public Action<float> OnCurrentIsMax;
    
    [SerializeField]
    private float _min;
    public float min
    {
        get
        {
            return _min;
        }
        set
        {
            _min = value;
            if (OnRangeChanged != null)
            {
                OnRangeChanged.Invoke(_min, _max);
            }
        }
    }

    [SerializeField]
    private float _max = 1;
    public float max
    {
        get
        {
            return _max;
        }
        set
        {
            _max = value;
            if (OnRangeChanged != null)
            {
                OnRangeChanged.Invoke(_min, _max);
            }
        }
    }

    [SerializeField]
    private float _current;
    public float current
    {
        get
        {
            return _current;
        }
        set
        {
            _current = value;
            if (OnCurrentChanged != null)
            {
                OnCurrentChanged.Invoke(_current);
            }
            if(_current <= _min)
            {
                _current = _min;
                if(OnCurrentIsMin != null)
                {
                    OnCurrentIsMin.Invoke(_current);
                }
            }
            if (_current >= _max)
            {
                _current = _max;
                if (OnCurrentIsMax != null)
                {
                    OnCurrentIsMax.Invoke(_current);
                }
            }
        }
    }

    public float percent
    {
        get
        {
            return (current - _min) / (max - _min);
        }
    }

    public object Clone()
    {
        PoolValueFloat ret = new PoolValueFloat();
        ret.max = max;
        ret.min = min;
        ret.current = current;
        ret.OnCurrentChanged = OnCurrentChanged;
        ret.OnCurrentIsMin = OnCurrentIsMin;
        ret.OnCurrentIsMax = OnCurrentIsMax;
        ret.OnRangeChanged = OnRangeChanged;
        return ret;
    }
}


[Serializable]
public class PoolValueInt : ICloneable
{
    public Action<int> OnCurrentChanged;
    public Action<int, int> OnRangeChanged;
    public Action<int> OnCurrentIsMin;
    public Action<int> OnCurrentIsMax;

    [SerializeField]
    private int _min;
    public int min
    {
        get
        {
            return _min;
        }
        set
        {
            _min = value;
            if (OnRangeChanged != null)
            {
                OnRangeChanged.Invoke(_min, _max);
            }
        }
    }

    [SerializeField]
    private int _max = 1;
    public int max
    {
        get
        {
            return _max;
        }
        set
        {
            _max = value;
            if (OnRangeChanged != null)
            {
                OnRangeChanged.Invoke(_min, _max);
            }
        }
    }

    [SerializeField]
    private int _current;
    public int current
    {
        get
        {
            return _current;
        }
        set
        {
            _current = value;
            if (OnCurrentChanged != null)
            {
                OnCurrentChanged.Invoke(_current);
            }
            if (_current <= _min)
            {
                _current = _min;
                if (OnCurrentIsMin != null)
                {
                    OnCurrentIsMin.Invoke(_current);
                }
            }
            if (_current >= _max)
            {
                _current = _max;
                if (OnCurrentIsMax != null)
                {
                    OnCurrentIsMax.Invoke(_current);
                }
            }
        }
    }

    public float percent
    {
        get
        {
            return ((float)current - (float)_min) / ((float)max - (float)_min);
        }
    }

    public object Clone()
    {
        PoolValueInt ret = new PoolValueInt();
        ret.max = max;
        ret.min = min;
        ret.current = current;
        ret.OnCurrentChanged = OnCurrentChanged;
        ret.OnCurrentIsMin = OnCurrentIsMin;
        ret.OnCurrentIsMax = OnCurrentIsMax;
        ret.OnRangeChanged = OnRangeChanged;
        return ret;
    }
}
