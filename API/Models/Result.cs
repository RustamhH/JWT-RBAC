namespace API.Models
{
    public class Result
    {

        private Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            {
                throw new ArgumentException("Invalid error", nameof(error));
            }
            IsSuccess = isSuccess;
            Error = error;
        }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error? Error { get; }
        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);
    }

    public class Result<T>
    {
        private Result(T value, bool isSuccess, Error error)
        {
            if ((isSuccess && error != Error.None) ||
                (!isSuccess && error == Error.None))
            {
                throw new ArgumentException("Invalid error state", nameof(error));
            }

            Value = value;
            IsSuccess = isSuccess;
            Error = error;
        }

        public T Value { get; }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        public static Result<T> Success(T value) =>
            new(value, true, Error.None);

        public static Result<T> Failure(Error error) =>
            new(default!, false, error);




    }
}
