using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
public static class PolishMap {
static Sprite Sprite(string name,bool ring) {
string path="Assets/GamePlay/Character/"+name+".png";
var tex=new Texture2D(256,256,TextureFormat.RGBA32,false);
var pixels=new Color[256*256];
for(int y=0;y<256;y++)for(int x=0;x<256;x++) {
float r=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(128,128));
float alpha=Mathf.Clamp01(127-r);if(ring)alpha*=Mathf.Clamp01(r-122);
pixels[y*256+x]=new Color(1,1,1,alpha);
}
tex.SetPixels(pixels);tex.Apply();System.IO.File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);
AssetDatabase.ImportAsset(path);
var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.SaveAndReimport();
return AssetDatabase.LoadAssetAtPath<Sprite>(path);
}
public static string Run(){
var circle=Sprite("MinimapCircle",false);var ring=Sprite("MinimapRing",true);
string path="Assets/Prefabs/Player/PlayerHolder.prefab";var root=PrefabUtility.LoadPrefabContents(path);
try {
var m=root.GetComponentInChildren<MapController>(true);
m.mapPanel.Find("CircularViewport").GetComponent<Image>().sprite=circle;
m.mapPanel.Find("NorthBubble").GetComponent<Image>().sprite=circle;
var border=new GameObject("MapBorder",typeof(RectTransform),typeof(Image)).GetComponent<Image>();
border.transform.SetParent(m.mapPanel,false);border.rectTransform.anchorMin=Vector2.zero;border.rectTransform.anchorMax=Vector2.one;border.rectTransform.offsetMin=Vector2.zero;border.rectTransform.offsetMax=Vector2.zero;
border.sprite=ring;border.color=new Color(.6f,.46f,.24f);border.raycastTarget=false;
m.mapPanel.Find("NorthBubble").SetAsLastSibling();
m.mapPanel.Find("MapDisplayBackground").GetComponent<Image>().sprite=circle;
PrefabUtility.SaveAsPrefabAsset(root,path);
}finally{PrefabUtility.UnloadPrefabContents(root);}
return "Added smooth geometric mask sprites and visible gold ring";
}}
