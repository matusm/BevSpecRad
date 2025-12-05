namespace BevSpecRad.Abstractions
{
    public interface IShutter
    {
        string Name { get; }
        void Open();
        void Close();
    }
}
