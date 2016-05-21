﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

// 픽셀을 다루기 때문에 아틀라스 텍스쳐는 reabable이어야 한다.
public class MapEditor : EditorWindow
{
    private string _mapName;
    private Vector2 _mapSize;

    private Texture2D _tileAtlasTexture;

    private Vector2 _mapScrollPos;
    private Vector2 _tileScrollPos;

    // result datas
    private string _resultMapName;
    private AtlasData[] _tileTextureData;

    // 현재 맵의 설치된 오브젝트들, key = position
    private Dictionary<int, PlaceInfo> _mapPlaceInfo = new Dictionary<int, PlaceInfo>();

    private MapData _resultData;
    // result datas end

    private AtlasDataList _tileRawData;

    private readonly float MENU_WIDTH = 300;

    [MenuItem("Window/FZTools/MapEditor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(MapEditor));
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();

        DisplaySettingBox();

        DisplayMapData();

        GUILayout.EndHorizontal();
    }

    private void DisplaySettingBox()
    {
        GUILayout.BeginVertical("Box", GUILayout.Width(MENU_WIDTH));

        GUILayout.Label("Map Data", EditorStyles.boldLabel);

        _mapName = EditorGUILayout.TextField("Map Name: ", _mapName);
        _mapSize = EditorGUILayout.Vector2Field("Map Size", _mapSize);

        if (GUILayout.Button("New"))
        {
            if (_tileRawData == null || _tileRawData.infos.Length <= 0)
            {
                LoadTileMap();
            }

            _resultData = new MapData();

            int mapMaxSize = (int)_mapSize.x * (int)_mapSize.y;

            _tileTextureData = new AtlasData[mapMaxSize];

            for (int y = 0; y < _mapSize.y; ++y)
            {
                for (int x = 0; x < _mapSize.x; ++x)
                {
                    int position = x + (int)_mapSize.x * y;

                    // 신규 맵을 작성할 시 첫 번째 타일맵으로 전부 다 채운다.
                    _tileTextureData[position] = _tileRawData.infos[0];

                    // 결과로 뽑혀나갈 타일 텍스쳐의 이름을 결과 데이터에 적재
                    _resultData.tileTextureNames.Add(_tileTextureData[position].imageName);
                }
            }

            _resultData.x = (int)_mapSize.x;
            _resultData.y = (int)_mapSize.y;
        }
        if (GUILayout.Button("Save"))
        {

        }
        if (GUILayout.Button("Load"))
        {

        }
        if (GUILayout.Button("Load Tilemap"))
        {
            DataManager.Instance.LoadData(new AtlasDataFactory("TileAtlas", DataManager.DataType.ATLAS_TILE));
            _tileRawData = DataManager.Instance.GetData<AtlasDataList>(DataManager.DataType.ATLAS_TILE);
            _tileAtlasTexture = _tileRawData.GetMaterial().mainTexture as Texture2D;
        }

        DisplayPlaceObjectTab();
        DisplayObjectInfoTab();
        DisplaySelectTileListTab();

        GUILayout.EndVertical();
    }

    private void LoadTileMap()
    {
        DataManager.Instance.LoadData(new AtlasDataFactory("TileAtlas", DataManager.DataType.ATLAS_TILE));
        _tileAtlasTexture = Resources.Load("Atlases/TileAtlas", typeof(Texture2D)) as Texture2D;
        _tileRawData = DataManager.Instance.GetData<AtlasDataList>(DataManager.DataType.ATLAS_TILE);
    }

    private PlaceInfo _placeInfo = new PlaceInfo();
    private PlaceInfo _selPlaceInfo = null;
    private GUIStyle _toggleButtonStyleToggled;

    private void DisplayPlaceObjectTab()
    {
        if (_selTileGridInt > -1)
        {
            _selPlaceInfo = null;
        }

        GUILayout.Label("Place Object", EditorStyles.boldLabel);

        if (_placeInfo.no <= 0) _placeInfo.no = 1;
        _placeInfo.no = EditorGUILayout.IntField("No", _placeInfo.no);
        _placeInfo.team = (eCombatTeam)EditorGUILayout.EnumPopup("Team", _placeInfo.team);
        _placeInfo.type = (eObjType)EditorGUILayout.EnumPopup("Type", _placeInfo.type);

        if (_toggleButtonStyleToggled == null)
        {
            _toggleButtonStyleToggled = new GUIStyle("Button");
            _toggleButtonStyleToggled.normal.background = _toggleButtonStyleToggled.active.background;
        }

        if (GUILayout.Button("Select", _selPlaceInfo != null ? _toggleButtonStyleToggled : "Button"))
        {
            _selPlaceInfo = _placeInfo;
            _selTileGridInt = -1;
        }

        if (GUILayout.Button("Unselect All"))
        {
            _selPlaceInfo = null;
            _selTileGridInt = -1;
        }
    }

    private void DisplayObjectInfoTab()
    {
        GUILayout.Label("Tile Object Info", EditorStyles.boldLabel);

        GUILayout.Label(string.Format("No : {0}", _selMapGridInt < 0 || !_mapPlaceInfo.ContainsKey(_selMapGridInt) ? string.Empty : _mapPlaceInfo[_selMapGridInt].no.ToString()));
        GUILayout.Label(string.Format("Team : {0}", _selMapGridInt < 0 || !_mapPlaceInfo.ContainsKey(_selMapGridInt) ? string.Empty : _mapPlaceInfo[_selMapGridInt].team.ToString()));
        GUILayout.Label(string.Format("Type : {0}", _selMapGridInt < 0 || !_mapPlaceInfo.ContainsKey(_selMapGridInt) ? string.Empty : _mapPlaceInfo[_selMapGridInt].type.ToString()));
    }

    public int _selTileGridInt = -1;
    private void DisplaySelectTileListTab()
    {
        if (_tileRawData != null)
        {
            int lineWidth = 0;
            int lineCount = 0;
            
            List<GUIContent> contents = new List<GUIContent>();

            for (int i = 0; i < _tileRawData.infos.Length; ++i)
            {
                var tileData = _tileRawData.infos[i];
                Texture2D tileImg = GetTileImg(tileData);
 
                lineWidth += tileImg.width;
               
                if (lineWidth <= MENU_WIDTH)
                {                    
                    lineCount += 1;
                }

                contents.Add(new GUIContent(tileImg));
            }

            if(lineCount <= 0)
            {
                lineCount = 1;
            }

            GUILayout.Label("Select Tile", EditorStyles.boldLabel);

            GUILayout.BeginVertical("Box", GUILayout.Width(MENU_WIDTH));
            _tileScrollPos = EditorGUILayout.BeginScrollView(_tileScrollPos);

            _selTileGridInt = GUILayout.SelectionGrid(_selTileGridInt, contents.ToArray(), lineCount, GUILayout.ExpandWidth(false));

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }

    private Dictionary<string, Texture2D> _tileImgPool = new Dictionary<string, Texture2D>();

    private Texture2D GetTileImg(AtlasData imageData)
    {
        if(_tileImgPool.ContainsKey(imageData.imageName))
        {
            return _tileImgPool[imageData.imageName];
        }

        var leftTop = new Vector2(imageData.offsetX, imageData.offsetY + imageData.scaleY);
        var leftBottom = new Vector2(imageData.offsetX, imageData.offsetY);
        var rightBottom = new Vector2(imageData.offsetX + imageData.scaleX, imageData.offsetY);

        int x = (int)(_tileAtlasTexture.width * leftBottom.x);
        int y = (int)(_tileAtlasTexture.height * leftBottom.y);
        int width = (int)(_tileAtlasTexture.width * (Vector2.Distance(leftBottom, rightBottom)));
        int height = (int)(_tileAtlasTexture.height * (Vector2.Distance(leftBottom, leftTop)));

        var tileImg = new Texture2D(width, height);
        tileImg.SetPixels(_tileAtlasTexture.GetPixels(x, y, width, height));
        tileImg.Apply(false);

        _tileImgPool.Add(imageData.imageName, tileImg);

        return tileImg;
    }
    

    public int _selMapGridInt = -1;
    private void DisplayMapData()
    {
        // 뭔가를 배치하는 모드라면 mapGrid 인덱스를 초기화
        if (_selTileGridInt > -1 || _selPlaceInfo != null)
        {
            _selMapGridInt = -1;
        }

        var margin = 3;
        
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.imagePosition = ImagePosition.ImageAbove;
        style.fontSize = 8;
        style.normal.textColor = Color.red;
        style.onNormal = GUI.skin.box.normal;
        style.margin = new RectOffset(margin * 2, margin * 2, margin, margin);

        List<GUIContent> contents = new List<GUIContent>();
        
        if (_resultData != null)
        {
            for (int y = 0; y < _resultData.y; ++y)
            {
                for (int x = 0; x < _resultData.x; ++x)
                {
                    int position = x + (int)_resultData.x * y;

                    var texture = GetTileImg(_tileTextureData[position]);

                    // 결과로 뽑혀나갈 타일 텍스쳐의 이름을 결과 데이터에 적재
                    _resultData.tileTextureNames[position] = _tileTextureData[position].imageName;

                    GUIContent content = new GUIContent(" ", texture);

                    if(_mapPlaceInfo.ContainsKey(position))
                    {
                        content.text = "placed";
                    }

                    contents.Add(content);
                }
            }
        }

        if (contents.Count > 0)
        {
            GUILayout.BeginVertical("Box", GUILayout.Height(EditorWindow.focusedWindow.position.height));
            _mapScrollPos = EditorGUILayout.BeginScrollView(_mapScrollPos);

            _selMapGridInt = GUILayout.SelectionGrid(_selMapGridInt, contents.ToArray(), (int)_resultData.x, style, GUILayout.ExpandWidth(false));
            if (_selTileGridInt > -1 && _selMapGridInt >= 0)
            {
                _tileTextureData[_selMapGridInt] = _tileRawData.infos[_selTileGridInt];
            }
            if(_selPlaceInfo != null && _selMapGridInt >= 0)
            {
                _mapPlaceInfo[_selMapGridInt] = _selPlaceInfo;
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
