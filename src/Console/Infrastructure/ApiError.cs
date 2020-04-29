using System.Collections.Generic;

namespace Omnia.CLI.Infrastructure
{
    public class ApiError
    {
        public List<ValidationError> Errors { get; }
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
