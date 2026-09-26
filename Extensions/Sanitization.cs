namespace FormsApi.Extensions
{
    public static class Sanitization
    {
        public static string? Sanitize(this string? input)
        {
            if (input is null)
                return null;
            
            var trimmed = input.Trim();
            var chars = trimmed.Where(c => !char.IsControl(c) || c is '\n' or '\r' or '\t')
                .ToArray();
            
            return new string(chars);
        }
    }
}
