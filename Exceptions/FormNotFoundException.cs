namespace FormsApi.Exceptions
{
    public class FormNotFoundException : Exception
    {
        public FormNotFoundException(Guid id) : base($"Form '{id}' was not found.") { }
    }
}
