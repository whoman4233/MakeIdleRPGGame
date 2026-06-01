using System.IO;
using UnityEngine;

/// <summary>
/// 로컬 파일 기반 세이브 저장소.
/// key를 파일명으로 사용해 Application.persistentDataPath 아래에 저장한다.
/// (Android: /data/data/.../files, Windows Editor: AppData/LocalLow/...)
/// </summary>
public class LocalFileStorage : ISaveStorage
{
    private string PathFor(string key) => Path.Combine(Application.persistentDataPath, key);

    public void Save(string key, string data) => File.WriteAllText(PathFor(key), data);

    public string Load(string key) => File.ReadAllText(PathFor(key));

    public bool Exists(string key) => File.Exists(PathFor(key));

    public void Delete(string key)
    {
        var path = PathFor(key);
        if (File.Exists(path)) File.Delete(path);
    }
}
