
namespace UVT.Pipe
{
    public interface IPipeComposite : IPipeImportable
    {
        IPipeImportable Input
        {
            get; set;
        }
        IPipeImportable[] GetAllInputs();
    }
}