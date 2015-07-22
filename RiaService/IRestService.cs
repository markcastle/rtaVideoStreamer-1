using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace RiaService
{
     [ServiceContract]
    public interface IRestService
    {
         [OperationContract]
        [WebGet(UriTemplate="/jpg/{ch}/image.jpg")]
           Stream jpg(string ch);
    }
}
