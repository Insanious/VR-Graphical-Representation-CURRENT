using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

//[Serializable]
public class JSONParser
{
    public static Linker.Container Read(string path)
    {
        /*
        TextAsset textFile = new TextAsset();
        textFile = Resources.Load<TextAsset>(file);
        return JsonConvert.DeserializeObject<Linker.Container>(textFile.text);
        */
        //path = "C:/Users/admin/Downloads/VR-anton/VR-Graphical-Representation-CURRENT/Assets/Resources/hib_hotspot_proto.json";
        return JsonConvert.DeserializeObject<Linker.Container>(File.ReadAllText(path));
    }
}