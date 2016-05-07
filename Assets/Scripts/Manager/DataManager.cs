﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DataManager : FZ.Singletone<DataManager>
{
    public enum DataType
    {
        UNIT,
        ATTACH_ACTOR,
        MAP
    }

    private DataManager() { }

    private Dictionary<DataType, DataObject> _datas = new Dictionary<DataType, DataObject>();

    public void LoadDatas()
    {
        // TODO : 나은 위치에서 로드하게 수정
        // 리플랙션을 사용해서 데이터들을 로드하게 하는 건 어떨지?
        LoadData(new UnitDataFactory());
        LoadData(new UnitActorDataFactory());
        LoadData(new MapDataFactory(1)); // TODO : 맵 번호를 런타임에 변경할 수 있게 수정 필요
    }

    public void LoadData(DataFactory factory)
    {
        _datas.Add(factory.GetDataType(), factory.LoadDatas());
    }

    public DataObject GetData(DataType type)
    {
        if (_datas.ContainsKey(type))
        {
            return _datas[type];
        }

        return null;
    }
}

#region DataFactory

public abstract class DataFactory
{
    protected class JsonParser<T>
    {
        public T LoadDatas(DataFactory factory)
        {
            T datas;
            try
            {
                // 직렬화된 클래스들은 반드시 변수명까지 json 형식과 일치해야 한다.
                datas = JsonUtility.FromJson<T>(factory.GetJsonString());
            }
            catch (Exception e)
            {
                throw new UnityException(typeof(T) + " : " + e.ToString());
            }

            return datas;
        }
    }

    protected abstract string GetName();

    public abstract DataManager.DataType GetDataType();

    public abstract DataObject LoadDatas();

    protected string GetJsonString()
    {
        var textAsset = Resources.Load(string.Format("Data/{0}", GetName())) as TextAsset;
        if (textAsset != null)
        {
            return textAsset.text;
        }
        else
        {
            throw new UnityException("DataFactory : DataPath is not available.");
        }
    }
}

public class UnitDataFactory : DataFactory
{
    protected override string GetName()
    {
        return "unit";
    }

    public override DataManager.DataType GetDataType()
    {
        return DataManager.DataType.UNIT;
    }

    public override DataObject LoadDatas()
    {
        return new JsonParser<UnitDataList>().LoadDatas(this);
    }
}

public class UnitActorDataFactory : DataFactory
{
    protected override string GetName()
    {
        return "unitActor";
    }

    public override DataManager.DataType GetDataType()
    {
        return DataManager.DataType.ATTACH_ACTOR;
    }

    public override DataObject LoadDatas()
    {
        return new JsonParser<UnitActorDataList>().LoadDatas(this);
    }
}

public class MapDataFactory : DataFactory
{
    private int _mapNo;

    public MapDataFactory(int mapNo)
    {
        _mapNo = mapNo;
    }

    protected override string GetName()
    {
        return string.Format("map{0}", _mapNo);
    }

    public override DataManager.DataType GetDataType()
    {
        return DataManager.DataType.MAP;
    }

    public override DataObject LoadDatas()
    {
        return new JsonParser<MapData>().LoadDatas(this);
    }
}

#endregion
