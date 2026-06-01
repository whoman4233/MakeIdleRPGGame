/// <summary>
/// 세이브 데이터의 영속화 백엔드 추상화.
/// 로컬 파일 / 클라우드 / 암호화 저장소 등을 동일 인터페이스로 교체 가능하게 한다.
/// SaveManager는 구체 구현이 아닌 이 인터페이스에만 의존한다.
/// </summary>
public interface ISaveStorage
{
    void Save(string key, string data);
    string Load(string key);
    bool Exists(string key);
    void Delete(string key);
}
