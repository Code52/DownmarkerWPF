using System.Threading.Tasks;

namespace MarkPad.Tests
{
    public static class NSubstituteHelper
    {
        /// <summary>
        /// Prevents compiler warnings like: 
        /// Warning 1       
        /// Because this call is not awaited, execution of the current method continues before the call is completed. 
        /// Consider applying the 'await' operator to the result of the call.
        /// </summary>
        /// <param name="task"></param>
        public static void IgnoreAwaitForNSubstituteAssertion(this Task task)
        {

        }
    }
}
