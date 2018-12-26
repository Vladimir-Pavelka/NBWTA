namespace NBWTA.Result
{
    public class Result<TValue> where TValue : class
    {
        public TValue Value { get; }
        public bool HasValue { get; }

        public Result(TValue value)
        {
            Value = value;
            HasValue = value != null;
        }
    }
}