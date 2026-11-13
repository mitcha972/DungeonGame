using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
public static class MapResolution {
static Type gv=typeof(Editor).Assembly.GetType("UnityEditor.GameView");
public static string Set(int width,int height) {
var win=EditorWindow.GetWindow(gv);
var prop=gv.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
if(!SessionState.GetBool("MapResolutionSaved",false)) {SessionState.SetInt("MapResolutionIndex",(int)prop.GetValue(win));SessionState.SetBool("MapResolutionSaved",true);}
var sizesType=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
var sizes=singleton.GetProperty("instance").GetValue(null);
var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{0});
var sizeType=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
var enumType=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
var size=Activator.CreateInstance(sizeType,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,new object[]{Enum.ToObject(enumType,1),width,height,"Minimap QA"},null);
group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
prop.SetValue(win,count-1);win.Repaint();
return width+"x"+height;
}
public static string Restore() {
var win=EditorWindow.GetWindow(gv);
gv.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(win,SessionState.GetInt("MapResolutionIndex",0));
SessionState.SetBool("MapResolutionSaved",false);return "Game view selection restored";
}}
