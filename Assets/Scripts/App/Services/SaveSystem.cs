public sealed class SaveSystem
{
    private readonly ISaveService _saveService;

    public SaveSystem()
    {
        _saveService = new SaveService();
    }

    public bool HasSave()
    {
        return _saveService.HasSave();
    }

    public SaveData Load()
    {
        return _saveService.Load();
    }

    public void Save(SaveData saveData)
    {
        _saveService.Save(saveData);
    }

    public void Delete()
    {
        _saveService.Delete();
    }
}

