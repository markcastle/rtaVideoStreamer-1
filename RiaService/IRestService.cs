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

         [OperationContract]
         [WebGet]
         void EnableETagEffect(bool IsEnable);

         [OperationContract]
         [WebGet(ResponseFormat = WebMessageFormat.Json)]
         bool GetIsEnableETagEffect();

        
    }
}
