using generated.com.tylertech.custom.xsdbindings.errorstream;
using System;

namespace gov.nmcourts.webservices.exception
{
    public class OdysseyWebServiceException: Exception
    {
        private ERRORSTREAM errorResponseObject;

        public OdysseyWebServiceException(String msg, Exception cause) : base(msg, cause)
        {

        }

        public OdysseyWebServiceException(ERRORSTREAM errorResponseObject, String msg, Exception cause): base(msg, cause)
        {
            this.errorResponseObject = errorResponseObject;
        }

        public ERRORSTREAM GetErrorstream()
        {
            return errorResponseObject;
        }

    }
}
