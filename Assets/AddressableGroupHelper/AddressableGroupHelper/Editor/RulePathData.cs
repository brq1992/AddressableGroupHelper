internal class RulePathData
{
    private string _guid;
    private string _path;

    public string Path { get { return _path; } }
    public string GUID { get { return _guid; } }

    public RulePathData(string guid, string path)
    {
        this._guid = guid;
        this._path = path;
    }

}