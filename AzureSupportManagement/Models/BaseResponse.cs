using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSupportManagement.Models
{
    public class BaseResponse
    {
        public bool Success { get; set; }
        public object Result { get; set; }
        public string Message { get; set; }
    }
}
