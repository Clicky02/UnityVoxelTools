namespace UVT.Pipe
{
    [System.Flags]
    public enum EditFlags
    {
        NONE = 0,
        INPUT = 1,
        TOOLS = 2,
        STORAGE_MODE = 4,
    }

    public interface IPipeEditable
    {
#if UNITY_EDITOR
        bool DrawInspector(EditFlags flags);
        bool DrawMultiInstanceEditor(EditFlags flags, UnityEngine.Object[] objects);
#endif
    }
}