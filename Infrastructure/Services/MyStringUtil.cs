namespace WebApiSample.Infrastructure.Services
{
    public interface IMyStringService
    {
        bool IsEmpty(string str);
    }

    public class MyStringService : IMyStringService
    {
        public bool IsEmpty(string str)
        {
            System.Console.WriteLine("[StringUtil.IsEmpty]");
            return string.IsNullOrWhiteSpace(str);
        }
    }
}
