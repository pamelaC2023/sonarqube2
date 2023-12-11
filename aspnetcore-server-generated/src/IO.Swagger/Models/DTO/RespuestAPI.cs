using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace IO.Swagger.Models.DTO
{
    public class RespuestAPI
    {

        public RespuestAPI()
        {
            ErrorMessages = new List<string>();
        }

        public HttpStatusCode StatusCode { get; set; }
        public bool IsSucces { get; set; }
        public List<string> ErrorMessages { get; set; }

        public object Result { get; set; }
    }
}
