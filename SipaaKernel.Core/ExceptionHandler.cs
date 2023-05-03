using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipaaKernel.Core
{
    public class ExceptionHandler
    {
        public static Action<Exception> SKBugCheckRaised = null;

        public static void SKBugCheck(Exception ex)
        {
            if (SKBugCheckRaised != null)
                SKBugCheckRaised.Invoke(ex);
        }
    }
}
