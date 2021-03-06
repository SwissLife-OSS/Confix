using MongoDB.Driver;

namespace Confix.Authoring.Store.Mongo
{
    public interface IConfixAuthorDbContext
    {
        IMongoCollection<Application> Applications { get; }
        IMongoCollection<Component> Components { get; }
        IMongoCollection<Variable> Variables { get; }
        IMongoCollection<VariableValue> VariableValues { get; }
    }
}
