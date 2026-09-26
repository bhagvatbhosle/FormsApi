namespace FormsApi.Exceptions
{
    public class FormConcurrencyException : Exception
    {
        public FormConcurrencyException(Guid id)
            : base($"Form '{id}' was modified by another request. Reload and retry.") { }
    }
}
