using System;
namespace ApiBA.Models
{
    public class OperationResult
    {
        private static readonly OperationResult _success = new OperationResult
        {
            Succeeded = true
        };

        public object Data { get; set; }

        public string Message { get; set; }

        public bool Succeeded { get; protected set; }

        public static OperationResult Success => _success;

        public Exception Exception { get; protected set; }

        public static OperationResult Failed(Exception ex, string message = null)
        {
            OperationResult operationResult = new OperationResult
            {
                Succeeded = false
            };
            if (ex != null)
            {
                operationResult.Exception = ex;
            }

            operationResult.Message = message;
            return operationResult;
        }

        public void ThrowIfNotSuccess()
        {
            if (Exception != null)
            {
                throw Exception;
            }
        }

        public override string ToString()
        {
            object obj = Message;
            if (obj == null)
            {
                obj = Exception?.InnerException?.Message;
                if (obj == null)
                {
                    obj = Exception?.Message;
                    if (obj == null)
                    {
                        if (!Succeeded)
                        {
                            return base.ToString();
                        }

                        obj = "Succeeded";
                    }
                }
            }

            return (string)obj;
        }

        public OperationResult ToJsonSafetyResult()
        {
            if (Exception != null)
            {
                string text = Exception.InnerException?.Message ?? Exception.Message;
                return Failed(null, text + Exception.StackTrace);
            }

            return this;
        }

        public OperationResult OperationSucceeded(string message = null)
        {
            Succeeded = true;
            if (message != null)
            {
                Message = message;
            }

            return this;
        }
    }
}

