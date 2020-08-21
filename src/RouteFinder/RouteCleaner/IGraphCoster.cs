using RouteCleaner.Model;

namespace RouteCleaner
{
    public interface IGraphCoster
    {
        double Cost(Way way, double distance);
    }
}
